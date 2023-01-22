#define SEVENZIP	//SevenZipSharpを使うときはこれを定義する。

using System;
using System.Collections.Generic;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.IO;						//Directory, File
using System.Threading;					//ThreadPool, WaitCallback
using System.Windows.Forms;
using System.Collections;				//ICollection asでつかう
using System.Linq;

namespace Marmi
{
	public partial class Form1 : Form, IRemoteObject
	{
		#region static var
		//コンフィグ保存用。ただ１つだけ存在
		public static AppGlobalConfig g_Config;
		//Form1参照用ハンドル
		public static Form1 _instance;
		#endregion


		//static定義
		public static readonly int DEFAULT_THUMBNAIL_SIZE = 400;	//サムネイル標準サイズ
		public static PackageInfo g_pi = null;				//現在見ているパッケージ情報

		#region const
		//コンフィグファイル名。XmlSerializeで利用
		//private const string CONFIGNAME = "Marmi.xml";
		//アプリ名。タイトルとして利用
		private const string APPNAME = "Marmi";
		//サムネイルキャッシュの拡張子
		private const string CACHEEXT = ".tmp";
		//サイドバーの標準サイズ
		private const int SIDEBAR_DEFAULT_WIDTH = 200;
		//非同期IOタイムアウト値
		const int ASYNC_TIMEOUT = 5000;
		#endregion

		//画面表示関連
		public int g_viewPages = 1;						//今見ているページ数：１か２
		//private Bitmap g_originalSizeBitmap = null;		//表示されている原寸Bitmap

		//Susieプラグイン
		private Susie susie = new Susie();
		//unrar.dllプラグイン ver1.76
		private Unrar unrar = new Unrar();

		#region --- コントロール ---
		//メイン画面
		public PicturePanel PicPanel = new PicturePanel();
		//ルーペ
		private Loupe loupe = null;
		//TrackBar
		private ToolStripTrackBar g_trackbar;
		//サムネイルパネル本体
		private ThumbnailPanel g_ThumbPanel = null;
		//フェードするPictureBox
		private ClearPanel g_ClearPanel = null;
		//サイドバー
		private SideBar g_Sidebar = null;
		//TrackBar用のサムネイル表示バー
		private NaviBar3 g_trackNaviPanel = null;
		//ホバー中のメニュー/ツールアイテム。非Focusクリック対応
		private object g_hoverStripItem = null;	
		//private PicturePanel overlay = new PicturePanel();
		#endregion

		#region --- リソースオブジェクト ---
		private readonly Icon iconLoope = Properties.Resources.loopeIcon;
		private readonly Icon iconLeftFinger = Properties.Resources.finger_left_shadow_ico;
		private readonly Icon iconRightFinger = Properties.Resources.finger_right_shadow_ico;
		private readonly Icon iconHandOpen = Properties.Resources.iconHandOpen;
		private Cursor cursorLeft;
		private Cursor cursorRight;
		private Cursor cursorLoupe;
		public static Cursor cursorHandOpen;
		#endregion

		#region --- データクラス ---
		private List<string> DeleteDirList = new List<string>();	//削除候補ディレクトリ
		//ver1.35 スクリーンキャッシュ
		Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();
		#endregion

		#region --- 非同期IO用オブジェクト ---
		//非同期IO用スレッド
		private Thread AsyncIOThread = null;
		//非同期取得用スタック
		//ver1.81 Sidebarからも登録するためpublicに変更
		private PrioritySafeQueue<KeyValuePair<int, Delegate>> stack = new PrioritySafeQueue<KeyValuePair<int, Delegate>>();
		//非同期全展開用SevenZipWrapper
		SevenZipWrapper m_AsyncSevenZip = null;
		#endregion

		//フラグ類
		//サムネイルを作るか。1000個以上あったときのフラグ
		//private bool g_makeThumbnail = true;
		//マウスクリックされた位置を保存。ドラッグ操作用
		private Point g_LastClickPoint = Point.Empty;
		volatile static ThreadStatus tsThumbnail = ThreadStatus.STOP;	//スレッドの状況を見る
		//ver1.51 事前のScreenCacheを作るかどうかのフラグ
		private bool needMakeScreenCache = false;

		//BeginInvoke用Delegate
		private delegate void StatusbarRenew(string s);


		#region --- スライドショー用オブジェクト ---
		//ver1.35 スライドショータイマー
		System.Windows.Forms.Timer SlideShowTimer = new System.Windows.Forms.Timer();
		//スライドショー中かどうか
		public bool isSlideShow { get { return SlideShowTimer.Enabled; } }
		#endregion

		//ver1.80 本棚機能用パネル
		FlowLayoutPanel BookShelf = null;


		// コンストラクタ *************************************************************/
		public Form1()
		{
			this.Name = "Marmi";
			_instance = this;

			////設定ファイルの読み込み
			////g_Config = (AppGlobalConfig)LoadFromXmlFile();
			//g_Config = (AppGlobalConfig)AppGlobalConfig.LoadFromXmlFile();
			//if (g_Config == null)
			//	g_Config = new AppGlobalConfig();

			//コントロールを追加。ツールストリップは最後に追加
			MyInitializeComponent();
			InitializeComponent();
			toolStrip1.Items.Add(g_trackbar);
			//
			// ver1.62 ツールバーの位置
			//
			toolStrip1.Dock = g_Config.isToolbarTop ? DockStyle.Top : DockStyle.Bottom;

			//初期設定
			this.KeyPreview = true;
			this.BackColor = g_Config.BackColor;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.SetStyle(ControlStyles.Opaque, true);
			Application.Idle += new EventHandler(Application_Idle);

			//zオーダーを初期化
			//SetFullScreen(false);
			//ver1.77 フルスクリーン状態の保存に対応
			//if (g_Config.saveFullScreenMode)
			//	SetFullScreen(g_Config.isFullScreen);
			//else
			//	SetFullScreen(false);

			//ツールバーの文字を設定 ver1.67
			SetToolbarString();

			//非同期IOの開始
			AsyncIOThreadStart();
        }

		private void MyInitializeComponent()
		{
			//
			//パッケージ情報 PackageInfo
			//
			g_pi = new PackageInfo();
			//
			//PicturePanel
			//
			this.Controls.Add(PicPanel);
			PicPanel.Enabled = true;
			PicPanel.Visible = true;
			//PicPanel.Left = 0;	//ver1.62コメントアウト
			PicPanel.Width = ClientRectangle.Width;
			PicPanel.BackColor = g_Config.BackColor;
			PicPanel.MouseClick += (s, e) => { OnMouseClick(e); };
			PicPanel.MouseDoubleClick += (s, e) => { OnMouseDoubleClick(e); };
			PicPanel.MouseMove += (s, e) => { OnMouseMove(e); };
			PicPanel.MouseUp += (s, e) => { OnMouseUp(e); };
			PicPanel.MouseWheel += new MouseEventHandler(PicPanel_MouseWheel);
			PicPanel.Dock = DockStyle.Fill;	//ver1.62 追加
			//
			//NaviBar
			//
			g_Sidebar = new SideBar();
			this.Controls.Add(g_Sidebar);
			g_Sidebar.Visible = false;
			//g_Sidebar.Width = SIDEBAR_DEFAULT_WIDTH
			g_Sidebar.Width = g_Config.sidebarWidth;
			g_Sidebar.Dock = DockStyle.Left;
			g_Sidebar.SidebarSizeChanged += new EventHandler(g_Sidebar_SidebarSizeChanged);
			//
			//TrackBar
			//
			g_trackbar = new ToolStripTrackBar();
			g_trackbar.Name = "MarmiTrackBar";
			g_trackbar.AutoSize = false;
			g_trackbar.Size = new System.Drawing.Size(300, 20);
			g_trackbar.ValueChanged += new EventHandler(g_trackbar_ValueChanged);
			g_trackbar.MouseUp += new MouseEventHandler(g_trackbar_MouseUp);
			g_trackbar.MouseLeave += new EventHandler(g_trackbar_MouseLeave);
			g_trackbar.MouseDown += new MouseEventHandler(g_trackbar_MouseDown);
			g_trackbar.MouseWheel += new EventHandler<MouseEventArgs>(g_trackbar_MouseWheel);
			//g_trackbar.MouseEnter += new EventHandler(g_trackbar_MouseEnter);
			//
			//サムネイルパネル
			//
			g_ThumbPanel = new ThumbnailPanel();
			this.Controls.Add(g_ThumbPanel);
			g_ThumbPanel.MouseMove += new MouseEventHandler(g_ThumbPanel_MouseMove);
			g_ThumbPanel.Init();
			g_ThumbPanel.Visible = false;
			g_ThumbPanel.Dock = DockStyle.Fill;	//ver1.64追加
			//
			//ClearPanel
			//
			g_ClearPanel = new ClearPanel(PicPanel);

			//その他変数初期化
			g_hoverStripItem = null;	//ホバー中のメニュー/ツールアイテム。非Focusクリック対応
			//ver1.81 変更
			//SetKeyConfig();
			SetKeyConfig2();

			//ver1.35 スライドショータイマー
			SlideShowTimer.Tick += new EventHandler(SlideShowTimer_Tick);
		}




		// フォームイベント *************************************************************/

		private void Form1_Load(object sender, EventArgs e)
		{
			//アイコン、カーソルの設定
			cursorLeft = new Cursor(iconLeftFinger.Handle);
			cursorRight = new Cursor(iconRightFinger.Handle);
			cursorLoupe = new Cursor(iconLoope.Handle);
			cursorHandOpen = new Cursor(iconHandOpen.Handle);

			////設定のロード/適用
			//生成はMyInitで先に実施しておくことにする。ver0.982
			//g_Config = (AppGlobalConfig)LoadFromXmlFile();
			//if (g_Config == null)
			//    g_Config = new AppGlobalConfig();

			applySettingToApplication();

			//初期化
			InitControls();
			UpdateToolbar();
			ResizeTrackBar();

			//起動パラメータの確認
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				//表示対象ファイルを取得
				//1つめに自分のexeファイル名が入っているので除く
				string[] a = new string[args.Length - 1];
				for (int i = 1; i < args.Length; i++)
					a[i - 1] = args[i];

				//ファイルを渡して開始
				//CheckAndStart(a);
				Start(a);
			}

		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			//非表示
			this.Hide();

			//ver1.77 MRUリストの更新
			UpdateMRUList();

			//全画面モードの解放
			if (g_Config.isFullScreen)
			{
				SetFullScreen(false);
				//ver1.77 元に戻すけどモード保存はさせる
				g_Config.isFullScreen = true;
			}


			//非同期IOスレッドの終了
			AsyncIOThread.Abort();
			AsyncIOThread.Join();

			//サムネイルモードの解放
			if (g_Config.isThumbnailView)
			{
				SetThumbnailView(false);
			}


			//7z解凍をしていたら中断
			if (m_AsyncSevenZip != null)
			{
				m_AsyncSevenZip.CancelAsyncExtractAll();
			}

			//スレッドが動作していたら停止させる.
			//サムネイルの保存
			//ファイルハンドルの解放
			InitControls();

			//ver1.62ツールバー位置を保存
			g_Config.isToolbarTop = (toolStrip1.Dock == DockStyle.Top);

			////////////////////////////////////////ver1.10

			//ver1.10
			//設定の保存
			if (g_Config.isSaveConfig)
			{
				//設定ファイルを保存する
				applySettingToConfig();
				AppGlobalConfig.SaveToXmlFile(g_Config);
			}
			else
			{
				//設定ファイルがあれば削除する
				//string configFile = AppGlobalConfig.getConfigFileName();
				string configFile = AppGlobalConfig.configFilename;
				if (File.Exists(configFile))
					File.Delete(configFile);
			}

			//古いキャッシュファイルを捨てる
			if (g_Config.isAutoCleanOldCache)
				ClearOldCacheDBFile();


			//Application.Idleの解放
			Application.Idle -= new EventHandler(Application_Idle);

			//ver1.57 susie解放
			susie.Dispose();
		}

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			base.OnDragEnter(drgevent);

			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
				drgevent.Effect = DragDropEffects.All;
			else
				drgevent.Effect = DragDropEffects.None;
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			base.OnDragDrop(drgevent);

			Uty.WriteLine("OnDragDrop() Start");

			//ドロップされた物がファイルかどうかチェック
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				//Formをアクティブ
				this.Activate();
				string[] files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
				//Start(files);
				AsyncStart(files);
				Uty.WriteLine("OnDragDrop() End");
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			Uty.WriteLine("OnResize()");

			//初期化が出来ていないときも帰れ
			//フォームが生成される前にもResize()は呼ばれる可能性がある。
			if (g_Config == null)
				return;

			//最小化時には何もしないで帰れ
			if (this.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			//ver0.972 ナビバーがあればナビバーをリサイズ
			AjustSidebarArrangement();

			//サムネイルか？
			//Formが表示する前にも呼ばれるのでThumbPanel != nullは必須
			if (g_ThumbPanel != null && g_Config.isThumbnailView)
			{
				//ver1.64 DockStyleにしたのでコメントアウト
				//Rectangle rect = GetClientRectangle();
				//g_ThumbPanel.Location = rect.Location;
				//g_ThumbPanel.Size = rect.Size;

				//ver0.91 ここでreturnしないと駄目では？
				//ThumbPanel.Refresh();
				//return;
			}

			////リサイズ時に描写しない設定か
			//if (g_Config.isStopPaintingAtResize)
			//    return;

			//ステータスバーに倍率表示
			UpdateStatusbar();

			//ver1.60 タイトルバーDClickによる最大化の時、ResizeEnd()が飛ばない
			if (this.WindowState == FormWindowState.Maximized)
				OnResizeEnd(null);
		}

		protected override void OnResizeEnd(EventArgs e)
		{
			base.OnResizeEnd(e);
			Uty.WriteLine("OnResizeEnd()");

			//サムネイル表示モードか
			if (g_ThumbPanel != null && g_Config.isThumbnailView)
			{
				//サムネイルパネルが表示されている場合はそちらを実施
				//表示する
				//ThumbPanel.thumbnailImageSet = g_pi.Items;
				//Rectangle rect = GetClientRectangle();
				//ThumbPanel.Location = rect.Location;
				//ThumbPanel.Size = rect.Size;
				//ThumbPanel.drawThumbnailToOffScreen();

				//スレッド版にしたため不要
				//ThumbPanel.MakeThumbnailScreen(true);
			}
			else
			{
				//ResizeEndでスクロールバーを表示
				//UpdateFormScrollbar();

				//画面を描写。ただしハイクオリティで
				//PaintBG2(LastDrawMode.HighQuality);
				//this.Refresh();		//this.Invalidate();
				UpdateStatusbar();
				PicPanel.ResizeEnd();
			}

			//トラックバー表示を直す
			ResizeTrackBar();
		}

		private void Application_Idle(object sender, EventArgs e)
		{
			//return;

			//Debug.WriteLine(DateTime.Now, "Application_Idle()");
			UpdateToolbar();
			//setStatusbarPages();

			//低品質描写だったら高品質で書き直す
			if (PicPanel.LastDrawMode
				== System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor)
			{
				PicPanel.fastDraw = false;
				PicPanel.Refresh();
			}

			//ver1.24 サイドバー
			//if (g_Sidebar.Visible)
			//{
			//    g_Sidebar.Invalidate();
			//}

			// スクリーンキャッシュ生成・パージ
			// ver1.38 Idleで毎回やらずにSetViewPageの最後でやる
			//getScreenCache();
			//ClearScreenCache();

			//サムネイルモードのApplication_Idle()へ
			if (g_Config.isThumbnailView)
			{
				g_ThumbPanel.Application_Idle();
			}

			//ScreenCacheを作る必要があれば作成。
			if (needMakeScreenCache)
			{
				needMakeScreenCache = false;
				ThreadPool.QueueUserWorkItem(dummy =>
				{
					getScreenCache();
					PurgeScreenCache();
					g_pi.FileCacheCleanUp2(g_Config.CacheSize);
				});
			}
		}

		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			/// 矢印キーへ対応させる
			switch (e.KeyCode)
			{
				//矢印キーが押されたことを表示する
				case Keys.Up:
				case Keys.Left:
				case Keys.Right:
				case Keys.Down:
				case Keys.Escape:
					Debug.WriteLine(e.KeyCode, "Form1_PreviewKeyDown()");
					e.IsInputKey = true;
					break;
			}

		}


		// 非同期スタート ***************************************************************/
		/// <summary>
		/// 非同期タイプのStart()
		/// 実際はThreadPoolからStartを呼び出しているだけ。
		/// D&DでDragしたプロセスが持ちきりになってしまうのを解決。
		/// OnDragDrop()からの呼び出しのみこれを使う。
		/// </summary>
		/// <param name="files"></param>
		private void AsyncStart(string[] files)
		{
			ThreadPool.QueueUserWorkItem(dummy =>
				{
					this.Invoke((MethodInvoker)(() => { Start(files); }));
				});
		}

		// スタート *********************************************************************/
		private void Start(string[] filenames)
		{
			//ver1.73 MRUリストの更新
			//今まで見ていたものを登録する
			UpdateMRUList();

			//ファイルがすでに開いているかどうかチェック
			if (filenames.Length == 1 && filenames[0] == g_pi.PackageName)
			{
				string text = "おなじフォルダ/ファイルを開こうとしています。開きますか？";
				string title = "同一フォルダ/ファイルオープンの確認";
				if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
					return;
			}

			//ver1.41 非同期IOを停止
			stack.Clear();
			stack.Push(new KeyValuePair<int, Delegate>(-1, null));
			g_pi.Initialize();
			setStatusbarInfo("準備中・・・" + filenames[0]);

			//ver1.35スクリーンキャッシュをクリア
			ScreenCache.Clear();

			//コントロールの初期化
			InitControls();		//この時点ではg_pi.PackageNameはできていない＝MRUがつくられない。

			//ver1.78単一ファイルの場合、そのディレクトリを対象とする
			string onePicFile = string.Empty;
			if (filenames.Length == 1 && Uty.isPictureFilename(filenames[0]))
			{
				onePicFile = filenames[0];
				filenames[0] = Path.GetDirectoryName(filenames[0]);
			}

			//ファイル一覧を生成
			SevenZipWrapper.ClearPassword();	//書庫のパスワードをクリア
			bool needRecurse = setPackageInfo(filenames);

			//ver1.37 再帰構造だけでなくSolid書庫も展開
			//ver1.79 常に一時書庫に展開オプションに対応
			//if (needRecurse )
			if (needRecurse || g_pi.isSolid || g_Config.AlwaysExtractArchive)
			{
				using (AsyncExtractForm ae = new AsyncExtractForm())
				{
					setStatusbarInfo("書庫を展開中です" + filenames[0]);

					//ver1.73 一時フォルダ指定対応でmakeTempDirName(bool isMakeDir)を変更
					g_pi.tempDirname = makeTempDirName(true);
					//ver1.79 一時フォルダが作れないときの対応
					if(string.IsNullOrEmpty(g_pi.tempDirname))
					{
						MessageBox.Show("一時展開フォルダが作成できませんでした。設定を確認してください");
						g_pi.Initialize();
						return;
					}

					//ダイアログを表示
					ae.ArchivePath = filenames[0];
					ae.ExtractDir = g_pi.tempDirname;
					ae.ShowDialog(this);

					//ダイアログの表示が終了
					//ディレクトリをすべてg_piに読み込む
					this.Cursor = Cursors.WaitCursor;
					g_pi.packType = PackageType.Pictures;
					g_pi.Items.Clear();
					GetDirPictureList(g_pi.tempDirname, true);
					this.Cursor = Cursors.Arrow;
				}
			}
			SortPackage();
			//UIを初期化
			UpdateToolbar();

			//ver1.73 MRUリストの更新
			//ここではだめ.最終ページを保存できない。
			//UpdateMRUList();

			//pdfチェック
			if (g_pi.packType == PackageType.Pdf)
			{
				if (!susie.isSupportedExtentions("pdf"))
				{
					string str = "pdfファイルはサポートしていません";
					g_ClearPanel.ShowAndClose(str, 1000);
					setStatusbarInfo(str);
					g_pi.Clear();
					return;
				}
			}


			//ver1.30 画面に読み込み状況を表示
			//MaskPanel mp = new MaskPanel(PicPanel.Bounds);
			//mp.addText("読み込み中" + filenames[0]);
			//this.Controls.Add(mp);
			//mp.BringToFront();
			//mp.Refresh();

			if (g_pi.Items.Count <= 0)
			{
				//画面をクリア、準備中の文字を消す
				string str = "表示できるファイルがありませんでした";
				g_ClearPanel.ShowAndClose(str, 1000);
				setStatusbarInfo(str);
				return;
			}
			//mp.addText("アイテム数チェック");
			//mp.Refresh();

			//ページを初期化
			g_pi.NowViewPage = 0;
			//CheckAndStart();

			//サムネイルDBがあれば読み込む
			//loadThumbnailDBFile();
			if (g_Config.isContinueZipView)
			{
				//読み込み値を無視し、０にセット
				//g_pi.NowViewPage = 0;
				foreach (var mru in g_Config.mru)
				{
					if (mru == null)
						continue;
					else if (mru.Name == g_pi.PackageName
						//ver1.79 コメントアウト
						//&& g_pi.packType == PackageType.Archive)
						)
					{
						//最終ページを設定する。
						g_pi.NowViewPage = mru.LastViewPage;
						//Bookmarkを設定する
						g_pi.setBookmarks(mru.Bookmarks);
						break;
					}
				}
			}

			//１ファイルドロップによるディレクトリ参照の場合
			//最初に見るページをドロップしたファイルにする。
			if(!string.IsNullOrEmpty(onePicFile))
			{
				int i = g_pi.Items.FindIndex(c => c.filename == onePicFile);
				if (i < 0) i = 0;
				g_pi.NowViewPage = i;
			}

			//トラックバーを初期化
			InitTrackbar();

			//SideBarへ登録
			g_Sidebar.Init(g_pi);

			//タイトルバーの設定
			//this.Text = APPNAME + @" - " + g_pi.PackageName;
			//ver1.79 フルパス表示をやめる
			this.Text = APPNAME + @" - " + Path.GetFileName(g_pi.PackageName);

			//サムネイルの作成
			AsyncLoadImageInfo();

			//画像を表示
			PicPanel.Message = string.Empty;
			SetViewPage(g_pi.NowViewPage);
		}

		/// <summary>
		/// パッケージ情報をpiに取る
		/// </summary>
		/// <param name="files">対象ファイル</param>
		/// <returns>書庫内書庫がある場合はtrue</returns>
		private bool setPackageInfo(string[] files)
		{
			//初期化
			g_pi.Initialize();

			if (files.Length == 1)
			{
				//ドロップされたのは1つ
				g_pi.PackageName = files[0];	//ディレクトリかZipファイル名を想定

				//ドロップされたファイルの詳細を探る
				if (Directory.Exists(g_pi.PackageName))
				{
					//ディレクトリの場合
					g_pi.packType = PackageType.Directory;
					GetDirPictureList(files[0], g_Config.isRecurseSearchDir);
				}
				else if (unrar.dllLoaded && files[0].ToLower().EndsWith(".rar"))
				{
					//
					//unrar.dllを使う。
					//
					g_pi.packType = PackageType.Archive;
					g_pi.isSolid = true;

					//ファイルリストを構築
					unrar.Open(files[0], Unrar.OpenMode.List);
					int num = 0;
					while(unrar.ReadHeader())
					{
						if (!unrar.CurrentFile.IsDirectory)
						{
							g_pi.Items.Add(new ImageInfo(
								num++,
								unrar.CurrentFile.FileName,
								unrar.CurrentFile.FileTime,
								unrar.CurrentFile.UnpackedSize
								));
						}
						unrar.Skip();
					}
					unrar.Close();

					//展開が必要なのでtrueを返す
					return true;
				}
				else if (Uty.isAvailableArchiveFile(g_pi.PackageName))
				{
					// 書庫ファイル
					g_pi.packType = PackageType.Archive;
					bool needRecurse = GetArchivedFileInfo(files[0]);
					//if (needRecurse)
					//{
					//    Uty.RecurseExtractAll(files[0], @"d:\temp");
					//}
					//MRUListを更新
					//UpdateMRUList();
					if (needRecurse)
						return true;
				}
				else if (files[0].ToLower().EndsWith(".pdf"))
				{
					//pdfファイル
					g_pi.packType = PackageType.Pdf;
					if (susie.isSupportPdf())
					{
						var list = susie.GetArchiveInfo(files[0]);
						foreach (var e in list)
						{
							g_pi.Items.Add(new ImageInfo((int)e.position, e.filename, e.timestamp, e.filesize));
						}
						return false;
					}
					else
					{
						//pdfは未サポート
						//g_pi.PackageName = string.Empty;
						return false;
					}
				}
				else
				{
					//単一画像ファイル
					g_pi.PackageName = string.Empty;	//zipでもディレクトリでもない
					g_pi.packType = PackageType.Pictures;

					//１つだけファイルを登録
					if (Uty.isPictureFilename(files[0]))
					{
						FileInfo fi = new FileInfo(files[0]);
						//g_pi.Items.Add(new ImageInfo(files[0], fi.CreationTime, fi.Length));
						g_pi.Items.Add(new ImageInfo(0, files[0], fi.CreationTime, fi.Length));
					}
				}
			}
			else //if (files.Length == 1)
			{
				//複数ファイル
				g_pi.PackageName = string.Empty;	//zipでもディレクトリでもない
				//g_pi.isZip = false;
				g_pi.packType = PackageType.Pictures;

				//ファイルを追加する
				int index = 0;
				foreach (string filename in files)
				{
					if (Uty.isPictureFilename(filename))
					{
						FileInfo fi = new FileInfo(filename);
						g_pi.Items.Add(new ImageInfo(index++, filename, fi.CreationTime, fi.Length));
					}
				}
			}//if (files.Length == 1)
			return false;
		}

		// メニュー操作 *****************************************************************/

		private void NavigateToBack()
		{
			//前に戻る
			long drawOrderTick = DateTime.Now.Ticks;
			int prev = GetPrevPage(g_pi.NowViewPage);
			if (prev >= 0)
				SetViewPage(prev, drawOrderTick);
			else
				g_ClearPanel.ShowAndClose("先頭のページです", 1000);

		}

		private void NavigateToForword()
		{
			//ver1.35 ループ機能を実装
			long drawOrderTick = DateTime.Now.Ticks;
			int now = g_pi.NowViewPage;
			int next = GetNextPage(g_pi.NowViewPage);
			Uty.WriteLine("NavigateToForword() {0} -> {1}", now, next);
//#if DEBUG
//            StackFrame callerFrame = new StackFrame(1);
//            Uty.WriteLine("StackFrame: {0}", callerFrame.GetMethod().Name);
//#endif
			if (next >= 0)
			{
				SetViewPage(next, drawOrderTick);
			}
			else if(g_Config.lastPage_toTop)
			{
				//先頭ページへループ
				SetViewPage(0, drawOrderTick);
				g_ClearPanel.ShowAndClose("先頭ページに戻りました", 1000);
			}
			else if(g_Config.lastPage_toNextArchive)
			{
				//ver1.70 最終ページで次の書庫を開く
				if (g_pi.packType != PackageType.Directory)
				{
					string filename = g_pi.PackageName;
					string dirname = Path.GetDirectoryName(filename);
					string[] files = Directory.GetFiles(dirname);
					Array.Sort(files, Uty.Compare_unsafeFast);
					bool match = false;
					foreach (var s in files)
					{
						if (s == filename)
						{
							match = true;
							continue; 
						}
						if (match)
						{
							if (Uty.isAvailableFile(s))
							{
								g_ClearPanel.ShowAndClose("次へ移動します："+Path.GetFileName(s), 1000);
								Start(new string[] { s });
								return;
							}
						}
					}
					g_ClearPanel.ShowAndClose("最後のページです。次の書庫が見つかりませんでした", 1000);
				}
				else
				{
					//先頭ページへループ
					SetViewPage(0, drawOrderTick);
					g_ClearPanel.ShowAndClose("先頭ページに戻りました", 1000);
				}
			}
			else //if(g_Config.lastPage_stay)
			{
				g_ClearPanel.ShowAndClose("最後のページです", 1000);
			}

		}

		public void SetThumbnailView(bool isShow)
		{
			if (isShow)
			{
				//表示準備する
				g_Config.isThumbnailView = true;
				//Rectangle rect = GetClientRectangle();
				//g_ThumbPanel.Location = rect.Location;
				//g_ThumbPanel.Size = rect.Size;					//これでOnSize()が呼ばれるはずなんだけど。

				//SideBarがある場合は消す
				if (g_Sidebar.Visible)
					g_Sidebar.Visible = false;

				//トラックバーはDisable Ver0.975
				g_trackbar.Enabled = false;

				//PicPanelを非表示に
				PicPanel.Visible = false;
				PicPanel.Dock = DockStyle.None;

				//表示する
				if (!this.Controls.Contains(g_ThumbPanel))
					this.Controls.Add(g_ThumbPanel);
				g_ThumbPanel.Dock = DockStyle.Fill;	//ver1.64
				g_ThumbPanel.Visible = true;
				toolButtonThumbnail.Checked = true;
				g_ThumbPanel.ReDraw();
			}
			else
			{
				//表示をやめる
				g_Config.isThumbnailView = false;
				//this.Controls.Remove(g_ThumbPanel);
				g_ThumbPanel.Visible = false;
				g_ThumbPanel.Dock = DockStyle.None;	//ver1.64
				toolButtonThumbnail.Checked = false;

				//PicPanelを表示
				PicPanel.Dock = DockStyle.Fill;
				PicPanel.Visible = true;

				//ver0.91 きちんと再描写する
				//リサイズされている可能性があるので再描写
				//Form1_ResizeEnd(null, null);	//ここでg_bgのRe-Allocate()が行われる。
				//OnResizeEnd(null);
				//PaintBG2(LastDrawMode.HighQuality);
				//this.Refresh();		//this.Invalidate();
				UpdateStatusbar();

				//NaviBarを戻す
				if (g_Config.visibleNavibar)
					g_Sidebar.Visible = true;

				//トラックバーを戻す Ver0.975
				g_trackbar.Enabled = true;
			}
		}

		private void OpenDialog()
		{
			using (OpenFileDialog of = new OpenFileDialog())
			{
				of.DefaultExt = "zip";
				of.FileName = "";
				of.Filter = "対応ファイル形式(書庫ファイル;画像ファイル)|*.zip;*.lzh;*.tar;*.rar;*.7z;*.jpg;*.bmp;*.gif;*.ico;*.png;*.jpeg|"
					+ "書庫ファイル|*.zip;*.lzh;*.tar;*.rar;*.7z|"
					+ "画像ファイル|*.jpg;*.bmp;*.gif;*.ico;*.png;*.jpeg|"
					+ "すべてのファイル|*.*";
				of.FilterIndex = 1;
				of.CheckFileExists = true;
				of.Multiselect = true;
				of.RestoreDirectory = true;

				if (of.ShowDialog() == DialogResult.OK)
				{
					//ver1.09 OpenFileAndStart()とりやめに伴い展開
					//OpenFileAndStart(of.FileName);
					Start(of.FileNames);
				}
			}
		}

		private void ToggleFullScreen()
		{
			SetFullScreen(!g_Config.isFullScreen);
		}

		private void SetFullScreen(bool isFullScreen)
		{
			if (isFullScreen)
			{
				//全画面にする
				g_Config.isFullScreen = true;

				menuStrip1.Visible = false;
				toolStrip1.Visible = false;
				statusbar.Visible = false;
				//g_Config.visibleMenubar = false;


				//Zオーダーを変更する
				this.Controls.Remove(statusbar);
				this.Controls.Remove(toolStrip1);
				this.Controls.Remove(menuStrip1);
				this.Controls.Remove(g_Sidebar);
				this.Controls.Remove(PicPanel);
				this.Controls.Remove(g_ThumbPanel);

				this.Controls.Add(menuStrip1);
				this.Controls.Add(toolStrip1);
				this.Controls.Add(statusbar);
				this.Controls.Add(PicPanel);
				this.Controls.Add(g_ThumbPanel);
				this.Controls.Add(g_Sidebar);

				if (this.WindowState != FormWindowState.Normal)
					this.WindowState = FormWindowState.Normal;
				this.FormBorderStyle = FormBorderStyle.None;

				//toolButtonFullScreen.Checked = true;			//これを先にやっておかないと
				this.WindowState = FormWindowState.Maximized;	//ここで発生するResizeイベントに間に合わない
			}
			else
			{
				//全画面を解除する
				g_Config.isFullScreen = false;
				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.WindowState = FormWindowState.Normal;

				//ツールバーを復元する /ver0.17 2009年3月8日
				//this.Controls.Add(toolStrip1);

				//Zオーダーを元に戻す
				this.Controls.Remove(statusbar);
				this.Controls.Remove(toolStrip1);
				this.Controls.Remove(menuStrip1);
				this.Controls.Remove(g_Sidebar);
				this.Controls.Remove(PicPanel);
				this.Controls.Remove(g_ThumbPanel);

				this.Controls.Add(PicPanel);
				this.Controls.Add(g_ThumbPanel);
				this.Controls.Add(g_Sidebar);
				this.Controls.Add(toolStrip1);
				this.Controls.Add(statusbar);
				this.Controls.Add(menuStrip1);

				toolButtonFullScreen.Checked = false;
				toolStrip1.Visible = g_Config.visibleToolBar;
				menuStrip1.Visible = g_Config.visibleMenubar;
				statusbar.Visible = g_Config.visibleStatusBar;
			}

			//メニュー、ツールバーの更新
			Menu_ViewFullScreen.Checked = g_Config.isFullScreen;
			Menu_ContextFullView.Checked = g_Config.isFullScreen;
			toolButtonFullScreen.Checked = g_Config.isFullScreen;

			AjustSidebarArrangement();
			UpdateStatusbar();

			//画面表示を修復
			if (PicPanel.Visible)
				PicPanel.ResizeEnd();
			else if (g_ThumbPanel.Visible)
				g_ThumbPanel.ReDraw();

		}

		/// <summary>
		/// サイドバーとPicPanelの位置関係を調整する
		/// </summary>
		public void AjustSidebarArrangement()
		{
			Rectangle rect = GetClientRectangle();
			if (g_Sidebar.Visible)
			{
				//g_Sidebar.SetSizeAndDock(GetClientRectangle());
				g_Sidebar.Top = rect.Top;
				g_Sidebar.Left = rect.Left;
				g_Sidebar.Height = rect.Height;

				PicPanel.Top = rect.Top;
				PicPanel.Left = g_Sidebar.Right + 1;
				PicPanel.Width = rect.Width - g_Sidebar.Width;
				PicPanel.Height = rect.Height;

				if (g_ThumbPanel.Visible)
				{
					g_ThumbPanel.Top = rect.Top;
					g_ThumbPanel.Left = g_Sidebar.Right + 1;
					g_ThumbPanel.Width = rect.Width - g_Sidebar.Width;
					g_ThumbPanel.Height = rect.Height;
				}
			}
			else
			{
				//PicPanel.fastDraw = false;
				PicPanel.Bounds = rect;
				if (g_ThumbPanel.Visible)
					g_ThumbPanel.Bounds = rect;
			}
		}

		private void SetDualViewMode(bool isDual)
		{
			Debug.WriteLine(isDual, "SetDualViewMode()");
			g_Config.dualView = isDual;
			toolButtonDualMode.Checked = isDual;
			Menu_View2Page.Checked = isDual;

			//ver1.36 スクリーンキャッシュをクリア
			//ClearScreenCache();
			ScreenCache.Clear();

			SetViewPage(g_pi.NowViewPage);	//ver0.988 2010年6月20日
		}

		private void ToggleBookmark()
		{
			if (g_pi.Items.Count > 0
				&& g_pi.NowViewPage >= 0)
			{
				g_pi.Items[g_pi.NowViewPage].isBookMark
					= !g_pi.Items[g_pi.NowViewPage].isBookMark;

				if (g_viewPages == 2)
					g_pi.Items[g_pi.NowViewPage + 1].isBookMark
						= !g_pi.Items[g_pi.NowViewPage + 1].isBookMark;

			}
		}

		/// <summary>
		/// 一時フォルダの名前を返す。
		/// 作れない場合はnullが返る。
		/// </summary>
		/// <param name="isMakeDir">名前だけでなく作成する場合はtrue。</param>
		/// <returns>一時フォルダのフルパス。作れない場合はnull</returns>
		private string makeTempDirName(bool isMakeDir)
		{
			//存在しないランダムなフォルダ名を作る
			string tempDir;
			
			//tempフォルダのルートとなるフォルダを決める。
			string rootPath = g_Config.tmpFolder;
			if (string.IsNullOrEmpty(rootPath))
				rootPath = Application.StartupPath;	//アプリのパス
				//Path.GetTempPath(),		//windows標準のTempDir

			//ユニークなフォルダを探す
			do
			{
				tempDir = Path.Combine(
					rootPath,
					"TEMP7Z" + Path.GetRandomFileName().Substring(0, 8));
			}
			while (Directory.Exists(tempDir));

			//ディレクトリ作成フラグが立っていたら作成
			if (isMakeDir)
			{
				//ディレクトリが作れないときはnullを返す
				try
				{
					Directory.CreateDirectory(tempDir);
					DeleteDirList.Add(tempDir);
				}
				catch
				{
					return null;
				}
			}

			return tempDir;
		}


		private void AsyncLoadImageInfo()
		{
			//ver1.54 2013年5月7日
			for (int cnt = 0; cnt < g_pi.Items.Count; cnt++)
			{
				//ラムダ式の外で文字列作成
				string s = string.Format("画像情報読み込み中...{0}/{1}", cnt + 1, g_pi.Items.Count);

				//スタックに入れる
				stack.PushLow(new KeyValuePair<int, Delegate>(cnt, (MethodInvoker)(() =>
				{
					setStatusbarInfo(s);
					//読み込んだものをPurge対象にする
					g_pi.FileCacheCleanUp2(g_Config.CacheSize);
				})));
				//Uty.WriteLine("{0}のサムネイル作成登録が終了", cnt);
			}
			//読み込み完了メッセージもPush
			stack.PushLow(new KeyValuePair<int, Delegate>(g_pi.Items.Count - 1, (MethodInvoker)(() =>
			{
				setStatusbarInfo("事前画像情報読み込み完了");
			})));
		}

		private void InitTrackbar()
		{
			g_trackbar.Minimum = 0;
			if (g_pi.Items.Count > 0)
			{
				g_trackbar.Maximum = g_pi.Items.Count - 1;
				g_trackbar.Enabled = true;
				g_trackbar.Value = g_pi.NowViewPage;
			}
			else
			{
				g_trackbar.Maximum = 0;
				g_trackbar.Value = 0;
				g_trackbar.Enabled = false;
			}
		}

		/// <summary>
		/// D&Dやアプリ起動時に呼ばれる初期化ルーチン
		/// スレッドを止め、すべての状態を初期化する。
		/// 読み込み対象のファイルについては一切何もしない。
		/// Form1_Load()
		/// Form1_FormClosed()
		/// OpenFileAndStart()
		/// Form1_DragDrop()
		/// </summary>
		/// 
		private void InitControls()
		{
			//サムネイルモードの解放
			if (g_Config.isThumbnailView)
				SetThumbnailView(false);

			//2011/08/19 サムネイル初期化
			g_ThumbPanel.Init();

			//2011年11月11日 ver1.24 サイドバー
			//g_Sidebar.Init();
			g_Sidebar.Init(null);	//ver1.37
			if (g_Sidebar.Visible)
				g_Sidebar.Invalidate();

			//ver1.25 trackNavi
			if (g_trackNaviPanel != null)
			{
				g_trackNaviPanel.Dispose();
				g_trackNaviPanel = null;
			}

			//2011/08/19 trackbarを初期化
			//InitTrackBar();
			//nullReferのため何もしないことにする
			//g_trackbar.Initialize();


			//MRUを更新する ver1.73 コメントアウト
			//UpdateMRUList();

			//サムネイルをバックグラウンドで保存する
			//saveDBFile();
			g_pi.Initialize();

			//パッケージ情報を初期化
			//古いのはスレッド中で捨てるので新しいのを作る
			//g_pi = new PackageInfo();

			//7z解凍をしていたら中断
			if (m_AsyncSevenZip != null)
			{
				m_AsyncSevenZip.CancelAsyncExtractAll();
			}
			//tempフォルダがあれば削除
			//if (!string.IsNullOrEmpty(g_pi.tempDirname))
			//{
			//    DeleteTempDir(g_pi.tempDirname);
			//    setStatusbarInfo("一時フォルダを削除しました - " + g_pi.tempDirname);
			//}
			foreach (string dir in DeleteDirList)
			{
				Uty.DeleteTempDir(dir);
			}
			DeleteDirList.Clear();

			//2011/08/19 Bitmapキャッシュ
			//g_FileCache.Clear();

			//2012/09/04 非同期IOを中止
			stack.Clear();
			stack.Push(new KeyValuePair<int, Delegate>(-1, null));

			//そのほか本体内の情報をクリア
			g_viewPages = 1;
			//g_lastDrawMode = LastDrawMode.HighQuality;	//Idleで余計なことをさせない
			g_LastClickPoint = Point.Empty;
			//if (g_originalSizeBitmap != null)
			//{
			//    PicPanel.bmp = null;
			//    g_originalSizeBitmap.Dispose();	//解放済みだったら例外が発生するかも
			//    g_originalSizeBitmap = null;
			//}

			//画像表示をやめる
			PicPanel.Message = string.Empty;
			PicPanel.bmp = null;

			//GC
			//Uty.ForceGC();
		}

		//private void saveDBFile()
		//{
		//    if (g_Config.isSaveThumbnailCache && g_pi.Items.Count > 0)
		//    {
		//        saveThumbnailDBFile(g_pi);
		//    }
		//}
		//private void saveDBFileOnThread()
		//{
		//    if (g_Config.isSaveThumbnailCache && g_pi.Items.Count > 0)
		//    {
		//        PackageInfo savedata = g_pi;
		//        Thread t = new Thread(() =>
		//        {
		//            saveThumbnailDBFile(savedata);
		//            savedata.Dispose();
		//        });
		//        t.Name = "SaveThumbnail Thread";
		//        t.IsBackground = false;
		//        setStatusbarInfo("サムネイル保存中");
		//        t.Start();
		//    }
		//}


		private void SortPackage()
		{
			//ファイルリストを並び替える
			if (g_pi.Items.Count > 0)
			{
				NaturalOrderComparer2 noc = new NaturalOrderComparer2();
				g_pi.Items.Sort(noc);
			}
			return;
		}

		/// <summary>
		/// 書庫情報を取得
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>書庫内書庫がある場合はtrye</returns>
		private bool GetArchivedFileInfo(string filename)
		{
			using (SevenZipWrapper szw = new SevenZipWrapper())
			{
				bool retval = false;

				if (szw.Open(filename) == false)
				{
					MessageBox.Show("エラーのため書庫は開けませんでした。");
					g_pi.Initialize();
					Uty.ForceGC();		//書庫を開放・GCする必要がある
					return false;
				}

				//Zipファイル情報を設定
				g_pi.PackageName = filename;
				FileInfo fi = new FileInfo(g_pi.PackageName);
				g_pi.size = fi.Length;
				g_pi.isSolid = szw.isSolid;

				//ver1.31 7zファイルなのにソリッドじゃないことがある！？
				if (Path.GetExtension(filename) == ".7z")
					g_pi.isSolid = true;

				//g_pi.isZip = true;
				g_pi.packType = PackageType.Archive;

				//ファイルをリストに追加
				//TODO: IEnumerable を実装してforeachにしたい
				g_pi.Items.Clear();
				//for (int i = 0; i < szw.itemCount; i++)
				//{
				//    ArchiveItem ai = szw.Item(i);
				//    if (!ai.isDirectory && Uty.isPictureFilename(ai.filename))
				//        g_pi.Items.Add(new ImageInfo(ai.filename, ai.datetime, (long)ai.filesize));
				//    else if (Uty.isAvailableArchiveFile(ai.filename))
				//    {
				//        //ver1.26 書庫内書庫を発見
				//        //string extractDir = makeTempDirName(true);
				//        //szw.ExtractFile(ai.filename, extractDir);
				//    }
				//}//for
				foreach (var item in szw.Items)
				{
					if (item.IsDirectory)
						continue;
					if (Uty.isPictureFilename(item.FileName))
						g_pi.Items.Add(new ImageInfo(item.Index, item.FileName, item.CreationTime, (long)item.Size));
					else if (Uty.isAvailableArchiveFile(item.FileName))
					{
						retval = true;
					}
				}
				return retval;
			}//using
		}

		/// <summary>
		/// 現在閲覧しているg_pi.PackageNameをMRUに追加する
		/// 以前も見たことがある場合、閲覧日付だけを更新
		/// </summary>
		private void UpdateMRUList()
		{
			//なにも無ければ追加しない
			if (string.IsNullOrEmpty(g_pi.PackageName))
				return;

			//ディレクトリでも追加しない
			if (g_pi.packType == PackageType.Directory)
				return;

		
			//MRUに追加する必要があるか確認
			bool needMruAdd = true;
			for (int i = 0; i < g_Config.mru.Length; i++)
			{
				if (g_Config.mru[i] == null)
					continue;
				if (g_Config.mru[i].Name == g_pi.PackageName)
				{
					//登録済みのMRUを更新
					//日付だけ更新
					g_Config.mru[i].Date = DateTime.Now;
					//最後に見たページも更新 v1.37
					g_Config.mru[i].LastViewPage = g_pi.NowViewPage;
					needMruAdd = false;

					//ver1.77 Bookmarkも設定
					g_Config.mru[i].Bookmarks = g_pi.getBookmarks();
				}
			}
			if (needMruAdd)
			{
				//MRUを新しく登録
				//古い順に並べる→先頭に追加
				Array.Sort(g_Config.mru);
				g_Config.mru[0] = new MRUList(
									g_pi.PackageName,
									DateTime.Now,
									g_pi.NowViewPage,
									g_pi.getBookmarks());
			}
			Array.Sort(g_Config.mru);	//並べ直す


		}


		//*****************************************************************
		// 画面遷移

		/// <summary>
		/// 指定したインデックスの画像を表示する。
		/// publicになっている理由はサイドバーやサムネイル画面からの
		/// 呼び出しに対応するため。
		/// 前のページに戻らないようにdrawOrderTickは現在時を内部で指定している。
		/// </summary>
		/// <param name="index">移動したいページインデックス番号</param>
		public void SetViewPage(int index)
		{
			SetViewPage(index, DateTime.Now.Ticks);
		}

	
		/// <summary>
		/// 指定したインデックスの画像を表示する。
		/// publicになっている理由はサイドバーやサムネイル画面からの
		/// 呼び出しに対応するため。
		/// 前のページに戻らないようにdrawOrderTickを導入
		/// </summary>
		/// <param name="index">インデックス番号</param>
		/// <param name="drawOrderTick">描写順序を示すオーダー時間</param>
		public void SetViewPage(int index, long drawOrderTick)
		{
			//ver1.09 オプションダイアログを閉じると必ずここに来ることに対するチェック
			if (g_pi.Items == null || g_pi.Items.Count == 0)
				return;

			//ver1.36 Index範囲チェック
			Debug.Assert(CheckIndex(index));

			// ページ進行方向 進む方向なら正、戻る方向なら負
			// アニメーションで利用する
			int pageDirection = index - g_pi.NowViewPage;

			//ページ番号を更新
			g_pi.NowViewPage = index;
			g_trackbar.Value = index;


			//ver1.35 スクリーンキャッシュチェック
			Bitmap screenImage = null;
			if (ScreenCache.TryGetValue(index, out screenImage))
			{
				//スクリーンキャッシュあったのですぐに描写
				SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
				Debug.WriteLine(index, "Use ScreenCache");
			}
			else
			{
				//ver1.50
				//Keyだけある{key,null}キャッシュだったら消す。稀に発生するため
				if (ScreenCache.ContainsKey(index))
					ScreenCache.Remove(index);

				//ver1.50 読み込み中と表示
				setStatusbarInfo("Now Loading ... " + (index + 1).ToString());
				Application.DoEvents();

				//画像作成をスレッドプールに登録
				ThreadPool.QueueUserWorkItem(dummy =>
				{
					//screenImage = MakeOriginalSizeImage(g_pi.NowViewPage);
					screenImage = MakeOriginalSizeImage(index);
					this.Invoke((MethodInvoker)(() =>
					{
						SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
					}));
				});

				//カーソルをWaitに
				this.Cursor = Cursors.WaitCursor;
			}
		}


		private void SetViewPage2(int index, int pageDirection, Bitmap screenImage, long orderTime)
		{
			//ver1.50 古かったら表示しない
			//if (index != g_pi.NowViewPage)
			//{
			//    Uty.WriteLine("Skip SetViewPage2({0}) now = {1}", index, g_pi.NowViewPage);
			//    return;
			//}

			//ver1.55 drawOrderTickのチェック.
			// スレッドプールに入るため稀に順序が前後する。
			// 最新の描写でなければスキップ
			if (PicPanel.drawOrderTime > orderTime)
			{
				Uty.WriteLine("Skip SetViewPage2({0}) too old order={1} < now={2}",
					index,
					orderTime,
					PicPanel.drawOrderTime);
				return;
			}

			//描写開始
			PicPanel.State = PicturePanel.DrawStatus.drawing;
			PicPanel.drawOrderTime = orderTime;

			if (screenImage == null)
			{
				Uty.WriteLine("bmpがnull(index={0})", index);
				PicPanel.State = PicturePanel.DrawStatus.idle;
				PicPanel.Message = "表示エラー 再度表示してみてください" + index.ToString();
				PicPanel.Refresh();
				return;
			}

			//ver1.50 表示
			PicPanel.State = PicturePanel.DrawStatus.drawing;
			PicPanel.Message = string.Empty;
			if (g_Config.pictureSwitchMode != AnimateMode.none	//アニメーションモードである
				&& !g_Config.keepMagnification					//倍率固定モードではアニメーションしない
				&& pageDirection != 0)
			{
				//スライドインアニメーション
				PicPanel.AnimateSlideIn(screenImage, pageDirection);
			}

			#region ページを描写
			PicPanel.bmp = screenImage;
			PicPanel.ResetView();
			PicPanel.fastDraw = false;

			//ver1.78コメントアウト
			////常に画面切り替わり時はフィットモードで起動
			//float r = PicPanel.FittingRatio;
			//if (r > 1.0f && Form1.g_Config.noEnlargeOver100p)
			//	r = 1.0f;
			//PicPanel.ZoomRatio = r;
			//PicPanel.AjustViewAndShow();

			//ver1.78 倍率をオプション指定できるように変更
			if (!g_Config.keepMagnification		//倍率維持モードではない
				|| isFitToScreen())				//画面にフィットしている
			{
				//画面切り替わり時はフィットモードで起動
				float r = PicPanel.FittingRatio;
				if (r > 1.0f && Form1.g_Config.noEnlargeOver100p)
					r = 1.0f;
				PicPanel.ZoomRatio = r;
			}

			PicPanel.AjustViewAndShow();
			#endregion

			//1ページ表示か2ページ表示か
			//viewPages = CanDualView(index) ? 2 : 1;
			g_viewPages = (int)screenImage.Tag;
			PicPanel.State = PicturePanel.DrawStatus.idle;

			#region 終了処理
			//カーソルを元に戻す
			this.Cursor = Cursors.Default;

			//UI更新
			UpdateStatusbar();
			UpdateToolbar();

			//サイドバーでアイテムを中心に
			if (g_Sidebar.Visible)
				g_Sidebar.SetItemToCenter(g_pi.NowViewPage);

			//ver1.37
			//スクリーンキャッシュを取得
			//さらに不要なメモリを解放し、GCする
			//ThreadPool.QueueUserWorkItem(dummy =>
			//{
			//    // ver1.38 Idleで毎回やらずにSetViewPageの最後でやる
			//    getScreenCache();
			//    PurgeScreenCache();
			//    //FileCacheもクリア
			//    g_pi.FileCacheCleanUp2(g_Config.CacheSize);
			//    //GC
			//    //Uty.ForceGC();
			//});
			//ver1.51 Idle()で作るためのフラグ
			needMakeScreenCache = true;
			#endregion

			//PicPanel.Message = string.Empty;
			PicPanel.State = PicturePanel.DrawStatus.idle;
			Uty.ForceGC();
		}

		//ver1.35 前のページ番号。すでに先頭ページなら-1
		private int GetPrevPage(int index)
		{
			if (index > 0)
			{
				int declimentPages = -1;
				//2ページ減らすことが出来るか
				if (CanDualView(g_pi.NowViewPage - 2))
					declimentPages = -2;

				int ret = index + declimentPages;
				return ret >= 0 ? ret : 0;
			}
			else
				//すでに先頭ページなので-1を返す
				return -1;
		}


		//ver1.36次のページ番号。すでに最終ページなら-1
		private int GetNextPage(int index)
		{
			int pages = CanDualView(index) ? 2 : 1;

			if (index + pages <= g_pi.Items.Count - 1)
				return (index + pages);
			else
				//最終ページ
				return -1;
		}


		public void AsyncGetBitmap(int index, Delegate action)
		{
			//キャッシュを持っていれば非同期しない
			//Bitmap bmp = g_pi.GetBitmapFromCache(index);
			//if (bmp != null)
			if(g_pi.hasCacheImage(index))
			{
			    if (action != null)
			        ((MethodInvoker)action)();
			    return;
			}

			////ver1.57 pdf対応
			//if (g_pi.packType == PackageType.Pdf)
			//{
			//    int filesize = (int)g_pi.Items[index].length;
			//    byte[] buf = susie.GetFile(g_pi.PackageName, index, filesize);
			//    ImageConverter ic = new ImageConverter();
			//    Bitmap _bmp = ic.ConvertFrom(buf) as Bitmap;
			//    g_pi.Items[index].cacheImage.Add(_bmp);
			//    if (action != null)
			//        ((MethodInvoker)action)();
			//    return;
			//}

			//ver1.54 HighQueueとして登録されているかどうか確認する。
			var array = stack.ToArrayHigh();
			foreach (var elem in array)
			{
				if (elem.Key == index)
				{
					Uty.WriteLine("AsyncGetBitmap() Skip {0}", index);
					return;
				}
			}

			//非同期するためにPush
			//Uty.WriteLine("AsyncGetBitmap() Push {0}", index);
			stack.Push(new KeyValuePair<int, Delegate>(index, action));
		}

		public Bitmap SyncGetBitmap(int index)
		{
			Bitmap bmp = g_pi.GetBitmapFromCache(index);
			if (bmp != null)
				return bmp;
			else
			{
				bool asyncFinished = false;
				Stopwatch sw = Stopwatch.StartNew();
				AsyncGetBitmap(index, (MethodInvoker)(() =>
				{
					asyncFinished = true;
				}));

				while (!asyncFinished && sw.ElapsedMilliseconds < ASYNC_TIMEOUT)
					Application.DoEvents();
				sw.Stop();

				if (sw.ElapsedMilliseconds < ASYNC_TIMEOUT)
					return g_pi.GetBitmapFromCache(index);
				else
				{
					Uty.WriteLine("SyncGetBitmap({0}), timeOut", index);
					return null;
				}
			}
		}

		/// <summary>
		/// Bitmapサイズを取得する
		/// Bitmap化しないだけ速いはず。
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private Size SyncGetBitmapSize(int index)
		{
			if (g_pi.Items[index].hasInfo)
				return g_pi.Items[index].bmpsize;
			else
			{
				//非同期のGetBitmap()を読み終わるまで待つ
				bool asyncFinished = false;
				Stopwatch sw = Stopwatch.StartNew();
				AsyncGetBitmap(index, (MethodInvoker)(() =>
				{
					asyncFinished = true;
				}));
				while (!asyncFinished && sw.ElapsedMilliseconds < ASYNC_TIMEOUT)
					Application.DoEvents();
				sw.Stop();

				if (g_pi.Items[index].hasInfo)
					return g_pi.Items[index].bmpsize;
				else
					return Size.Empty;
			}
		}

		private Bitmap MakeOriginalSizeImage(int index)
		{
			Uty.WriteLine("MakeOriginalSizeImage({0})", index);

			//とりあえず1枚読め！
			//Bitmap bmp1 = g_pi.GetBitmap(index);
			Bitmap bmp1 = SyncGetBitmap(index);
			if (bmp1 == null)
			{
				if (g_pi.isSolid && g_Config.isExtractIfSolidArchive)
					PicPanel.Message  = "画像ファイルを展開中です";
				else
					PicPanel.Message = "読込みに時間がかかってます.リロードしてください";
				return null;
			}
			
			//ver1.81 サムネイル登録
			//g_pi.AsyncThumnailMaker(index, bmp1);
			g_pi.AsyncThumnailMaker(index, bmp1.Clone() as Bitmap);

			if (g_Config.dualView && CanDualView(index))
			{
				//2枚表示
				//viewPages = 2;
				//Bitmap bmp2 = g_pi.GetBitmap(index + 1);
				Bitmap bmp2 = SyncGetBitmap(index + 1);
				if (bmp2 == null)
				{
					//2枚目の読み込みがエラーなので1枚表示にする
					//viewPages = 1;
					//g_originalSizeBitmap = bmp1;
					bmp1.Tag = 1;
					return bmp1;
				}

				//ver1.81 サムネイル登録
				g_pi.AsyncThumnailMaker(index + 1, bmp2.Clone() as Bitmap);


				//合成ページを作る
				int width1 = bmp1.Width;
				int width2 = bmp2.Width;
				int height1 = bmp1.Height;
				int height2 = bmp2.Height;
				Bitmap returnBmp = new Bitmap(
					width1 + width2,
					(height1 > height2) ? height1 : height2);

				using (Graphics g = Graphics.FromImage(returnBmp))
				{
					g.Clear(g_Config.BackColor);

					if (g_pi.LeftBook)
					{
						//左から右へ
						//2枚目(左）を描写
						g.DrawImage(bmp2, 0, 0, width2, height2);
						//1枚目（右）を描写
						g.DrawImage(bmp1, width2, 0, width1, height1);
					}
					else
					{
						//右から左へ
						//2枚目(左）を描写
						g.DrawImage(bmp1, 0, 0, width1, height1);
						//1枚目（右）を描写
						g.DrawImage(bmp2, width1, 0, width2, height2);
					}
				}
				bmp1.Dispose();
				bmp2.Dispose();
				returnBmp.Tag = 2;
				return returnBmp;
			}
			else
			{
				//1枚表示
				bmp1.Tag = 1;
				return bmp1;
			}
		}


		// ユーティリティ系 *************************************************************/
		private void UpdateToolbar()
		{

			//画面モードの状態反映
			toolButtonDualMode.Checked = g_Config.dualView;
			toolButtonFullScreen.Checked = g_Config.isFullScreen;
			toolButtonThumbnail.Checked = g_Config.isThumbnailView;

			//Sidebar
			toolStripButton_Sidebar.Checked = g_Sidebar.Visible;

			if (g_pi.Items == null || g_pi.Items.Count < 1)
			{
				//ファイルを閲覧していない場合のツールバー
				g_trackbar.Enabled = false;
				toolButtonLeft.Enabled = false;
				toolButtonRight.Enabled = false;
				toolButtonThumbnail.Enabled = false;
				toolStripButton_Zoom100.Checked = false;
				toolStripButton_ZoomFit.Checked = false;
				toolStripButton_ZoomOut.Enabled = false;
				toolStripButton_ZoomIn.Enabled = false;
				toolStripButton_Zoom100.Enabled = false;
				toolStripButton_ZoomFit.Enabled = false;
				toolStripButton_Favorite.Enabled = false;
				toolStripButton_Rotate.Enabled = false;
				return;
			}
			else
			{
				//サムネイルボタン
				toolButtonThumbnail.Enabled = true;
				//if(g_makeThumbnail)
				//    toolButtonThumbnail.Enabled = true;
				//else
				//    toolButtonThumbnail.Enabled = false;


				if (g_Config.isThumbnailView)
				{
					//サムネイル表示中
					toolButtonLeft.Enabled = false;
					toolButtonRight.Enabled = false;
					toolStripButton_Zoom100.Enabled = false;
					toolStripButton_ZoomFit.Enabled = false;
					toolStripButton_Favorite.Enabled = false;
					toolStripButton_Sidebar.Enabled = false;
					//toolStripButton_Zoom100.Checked = false;
					//toolStripButton_ZoomFit.Checked = false;
				}
				else
				{
					//通常表示中
					toolStripButton_ZoomIn.Enabled = true;
					toolStripButton_ZoomOut.Enabled = true;
					toolStripButton_Zoom100.Enabled = true;
					toolStripButton_ZoomFit.Enabled = true;
					toolStripButton_Favorite.Enabled = true;
					toolStripButton_Sidebar.Enabled = true;
					toolStripButton_Rotate.Enabled = true;

					//左右ボタンの有効無効
					if (g_Config.isReplaceArrowButton)
					{
						//入れ替え
						toolButtonLeft.Enabled = !IsLastPageViewing();		//最終ページチェック
						toolButtonRight.Enabled = (bool)(g_pi.NowViewPage != 0);	//先頭ページチェック
					}
					else
					{
						toolButtonLeft.Enabled = (bool)(g_pi.NowViewPage != 0);	//先頭ページチェック
						toolButtonRight.Enabled = !IsLastPageViewing();		//最終ページチェック
					}

					//100%ズーム
					toolStripButton_Zoom100.Checked = isScreen100p();

					//画面フィットズーム
					toolStripButton_ZoomFit.Checked = isFitToScreen();

					//Favorite
					if (g_pi.Items[g_pi.NowViewPage].isBookMark)
						toolStripButton_Favorite.Checked = true;
					else if (g_viewPages == 2 
						&& g_pi.NowViewPage < g_pi.Items.Count - 1		//ver1.69 最終ページより前チェック
						&& g_pi.Items[g_pi.NowViewPage + 1].isBookMark)	//
						toolStripButton_Favorite.Checked = true;
					else
						toolStripButton_Favorite.Checked = false;

					//Sidebar
					toolStripButton_Sidebar.Checked = g_Sidebar.Visible;
				}

				//TrackBar
				//ここで直すとUIが遅くなる。
				//g_trackbar.Value = g_pi.NowViewPage;
			}
		}

		//画面にフィットしているかどうか
		private bool isFitToScreen()
		{
			return (Math.Abs(PicPanel.ZoomRatio - PicPanel.FittingRatio) < 0.001f);
		}
		//100%表示かどうか
		private bool isScreen100p()
		{
			return (Math.Abs(PicPanel.ZoomRatio - 1.0f) < 0.001f);
		}

		/// <summary>
		/// MRUリストを更新する。実際にメニューの中身を更新
		/// この関数を呼び出しているのはMenu_File_DropDownOpeningのみ
		/// </summary>
		private void UpdateMruMenuListUI()
		{
			MenuItem_FileRecent.DropDownItems.Clear();

			Array.Sort(g_Config.mru);

			int menuCount = 0;

			//for (int i = 0; i < mySetting.mru.Length; i++)	//古い順
			for (int i = g_Config.mru.Length - 1; i >= 0; i--)		//新しい順にする
			{
				if (g_Config.mru[i] == null)
					continue;

				MenuItem_FileRecent.DropDownItems.Add(
					g_Config.mru[i].Name,					//アイテムのテキスト
					null,									//アイテムのイメージ
					new System.EventHandler(OnClickMRUMenu)	//イベント
					);

				//ver1.73 MRU表示数の制限
				if (++menuCount >= g_Config.numberOfMru)
					break;
			}
		}

		/// <summary>
		/// ディレクトリの画像をリストに追加する。再帰的に呼び出すために関数化
		/// </summary>
		/// <param name="dirName">追加対象のディレクトリ名</param>
		/// <param name="isRecurse">再帰的に走査する場合はtrue</param>
		private void GetDirPictureList(string dirName, bool isRecurse)
		{

			string[] files = Directory.GetFiles(dirName);

			//画像ファイルだけを追加する
			//g_filelist.AddRange(f);
			int index = 0;
			foreach (string name in files)
			{
				if (Uty.isPictureFilename(name))
				{
					FileInfo fi = new FileInfo(name);
					g_pi.Items.Add(new ImageInfo(index++, name, fi.CreationTime, fi.Length));
				}
			}

			//再帰的に取得するかどうか。
			//if (g_Config.isRecurseSearchDir)
			if (isRecurse)
			{
				string[] dirs = Directory.GetDirectories(dirName);
				foreach (string name in dirs)
					GetDirPictureList(name, isRecurse);
			}
		}

		/// <summary>
		/// 最終ページを見ているかどうか確認。２ページ表示に対応
		/// 先頭ページはそのまま０かどうかチェックするだけなので作成しない。
		/// </summary>
		/// <returns>最終ページであればtrue</returns>
		private bool IsLastPageViewing()
		{
			if (string.IsNullOrEmpty(g_pi.PackageName))
				return false;
			if (g_pi.Items.Count <= 1)
				return false;
			return (bool)(g_pi.NowViewPage + g_viewPages >= g_pi.Items.Count);

		}

		private bool CheckIndex(int index)
		{
			return (index >= 0 && index < g_pi.Items.Count);
		}

		/// <summary>
		/// クライアントサイズを求める。ツールバーやステータスバーなどを換算
		/// ツールバーの大きさなどを考慮したクライアント領域を返す
		/// 補正対象：
		///   メニューバー, スクロールバー, サイドバー, ツールバー, ステータスバー
		/// </summary>
		/// <returns>クライアント位置、サイズを表すRectangle</returns>
		public Rectangle GetClientRectangle()
		{
			Rectangle rect = this.ClientRectangle; // this.Bounds;

			//ツールバーの高さ
			int toolbarHeight = (toolStrip1.Visible && !g_Config.isFullScreen) ? toolStrip1.Height : 0;

			//メニューバーの高さ
			int menubarHeight = (menuStrip1.Visible) ? menuStrip1.Height : 0;

			//ステータスバーの高さ
			int statusbarHeight = (statusbar.Visible && !g_Config.isFullScreen) ? statusbar.Height : 0;

			//ツールバーが上の時だけYから控除
			if (g_Config.isToolbarTop)
				rect.Y += toolbarHeight;

			//各パラメータの補正
			rect.Y += menubarHeight;
			rect.Height -= (toolbarHeight + menubarHeight + statusbarHeight);

			//マイナスにならないように補正 ver0.985a
			//例外発生をなくす
			if (rect.Width < 0) rect.Width = 0;
			if (rect.Height < 0) rect.Height = 0;

			return rect;
		}

		/// <summary>
		/// ver1.67 ツールバーの文字を表示/非表示する。
		/// </summary>
		private void SetToolbarString()
		{
			if (g_Config.eraseToolbarItemString)
			{
				toolButtonClose.DisplayStyle = ToolStripItemDisplayStyle.Image;
				toolButtonFullScreen.DisplayStyle = ToolStripItemDisplayStyle.Image;
			}
			else
			{
				toolButtonClose.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
				toolButtonFullScreen.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			}
		}

		// サイドバー関連 ***************************************************************/
		/// <summary>
		/// サイドバーのサイズが変更された（され終わった）ときに呼び出される。
		/// これが呼び出されるときはサイドバー固定の時のみ。
		/// 2010年6月6日 ver0.985で実装
		/// </summary>
		/// <param name="sender">利用せず</param>
		/// <param name="e">利用せず</param>
		void g_Sidebar_SidebarSizeChanged(object sender, EventArgs e)
		{
			OnResizeEnd(null);
		}


		// 画像処理 *********************************************************************/


		private void AnimateThumbnailBlur(int index)
		{
			//ver1.25 サムネイルを使って高速表示
			Bitmap tempbmp = null;

			if (g_pi.Items[index].thumbnail != null)
			{
				if (g_Config.dualView && CanDualView(index))
				{
					//実画像相当サイズの画像をサムネイルから作る
					tempbmp = new Bitmap(
						g_pi.Items[index].width + g_pi.Items[index + 1].width,
						g_pi.Items[index].height > g_pi.Items[index + 1].height
							? g_pi.Items[index].height
							: g_pi.Items[index + 1].height);


					using (Graphics g = Graphics.FromImage(tempbmp))
					{
						g.DrawImage(
							g_pi.Items[index + 1].thumbnail,
							0,
							0,
							g_pi.Items[index + 1].width,
							g_pi.Items[index + 1].height);
						g.DrawImage(
							g_pi.Items[index].thumbnail,
							g_pi.Items[index + 1].width,
							0,
							g_pi.Items[index].width,
							g_pi.Items[index].height);
					}
				}
				else
				{
					//ver1.29 間違っている
					//tempbmp = g_pi.Items[g_pi.NowViewPage].ThumbImage as Bitmap;

					tempbmp = new Bitmap(g_pi.Items[index].width, g_pi.Items[index].height);
					using (Graphics g = Graphics.FromImage(tempbmp))
					{
						g.DrawImage(g_pi.Items[index].thumbnail,
							0,
							0,
							g_pi.Items[index].width,
							g_pi.Items[index].height);
					}
				}

				PicPanel.bmp = tempbmp;
				//ズーム比率設定
				if (g_Config.isFitScreenAndImage)
					PicPanel.ZoomRatio = PicPanel.FittingRatio;
				else
				{
					PicPanel.ZoomRatio = 1.0f;
				}

				if (PicPanel.ZoomRatio > 1.0f && g_Config.noEnlargeOver100p)
					PicPanel.ZoomRatio = 1.0f;


				PicPanel.AjustViewAndShow();
			}
			//終わるのを待つ！
			//while (!th.Join(10))
			//{}

			//ここでDisPoseしちゃだめ
			//if (tempbmp != null)
			//    tempbmp.Dispose();
		}


		/// <summary>
		/// 指定されたインデックスから２枚表示できるかチェック
		/// チェックはImageInfoに取り込まれた値を利用、縦横比で確認する。
		/// </summary>
		/// <param name="index">インデックス値</param>
		/// <returns>2画面表示できるときはtrue</returns>
		private bool CanDualView(int index)
		{
			//最後のページになっていないか確認
			if (index >= g_pi.Items.Count - 1 || index < 0)
				return false;

			//コンフィグ条件を確認
			if (!g_Config.dualView)
				return false;

			//ver1.79判定なしの2ページ表示
			if (g_Config.dualView_Force)
				return true;

			//1枚目チェック
			if (!g_pi.Items[index].hasInfo)
				//SyncGetBitmap(index);
				SyncGetBitmapSize(index);
			//if (g_pi.Items[index].width > g_pi.Items[index].height)
			if (g_pi.Items[index].isFat)
				return false; //横長だった

			//２枚目チェック
			if (!g_pi.Items[index + 1].hasInfo)
				//SyncGetBitmap(index + 1);
				SyncGetBitmapSize(index + 1);
			//if (g_pi.Items[index + 1].width > g_pi.Items[index + 1].height)
			if (g_pi.Items[index + 1].isFat)
				return false;　//横長だった

			//全て縦長だった時の処理
			//ver1.70 縦長ならOKとする
			//if(!g_Config.dualview_exactCheck)
			//	return true;
			//ver1.79 簡易チェック：縦画像2枚でOK
			if (g_Config.dualView_Normal)
				return true; //縦画像2枚

			//ver1.20 ほぼ同じサイズかどうかをチェック
			//縦の長さがほとんど変わらなければtrue
			const int ACCEPTABLE_RANGE = 200;
			if (Math.Abs(g_pi.Items[index].height - g_pi.Items[index + 1].height) < ACCEPTABLE_RANGE)
			    return true;
			else
			    return false;
		}

		// ユーティリティ系：画像キャッシュ *********************************************/



		#region スクリーンキャッシュ
		/// <summary>
		/// 前後ページの画面キャッシュを作成する
		/// 現在見ているページを中心とする
		/// </summary>
		private void getScreenCache()
		{
			//ver1.37 スレッドで使うことを前提にロック
			lock ((ScreenCache as ICollection).SyncRoot)
			{
				//前のページ
				int ix = GetPrevPage(g_pi.NowViewPage);
				if (ix >= 0 && !ScreenCache.ContainsKey(ix))
				{
					Debug.WriteLine(ix, "getScreenCache() Add Prev");
					ScreenCache.Add(ix, MakeOriginalSizeImage(ix));
				}

				//前のページ
				ix = GetNextPage(g_pi.NowViewPage);
				if (ix >= 0 && !ScreenCache.ContainsKey(ix))
				{
					Debug.WriteLine(ix, "getScreenCache() Add Next");
					ScreenCache.Add(ix, MakeOriginalSizeImage(ix));
				}
			}
		}
		/// <summary>
		/// 不要なスクリーンキャッシュを削除する
		/// </summary>
		private void PurgeScreenCache()
		{
			//削除候補をリストアップ
			int now = g_pi.NowViewPage;
			const int DISTANCE = 2;
			List<int> deleteCandidate = new List<int>();

			foreach (var ix in ScreenCache.Keys)
			{
				if (ix > now + DISTANCE || ix < now - DISTANCE)
				{
					deleteCandidate.Add(ix);
				}
			}

			//削除候補を削除する
			if (deleteCandidate.Count > 0)
			{
				foreach (int ix in deleteCandidate)
				{
					//先に消してはだめ！
					//ディクショナリから削除した後BitmapをDispose()
					//Bitmap tempBmp = ScreenCache[key];
					//ScreenCache.Remove(key);
					//tempBmp.Dispose();
					Bitmap tempBmp = null;
					if (ScreenCache.TryGetValue(ix, out tempBmp))
					{
						ScreenCache.Remove(ix);
						//if (tempBmp != null) 
						//    tempBmp.Dispose();
						Uty.WriteLine("PurgeScreenCache({0})", ix);
					}
					else
						Uty.WriteLine("PurgeScreenCache({0})失敗", ix);
				}
			}
			//ver1.37GC
			//Uty.ForceGC();
		}


		#endregion

		//古いキャッシュDBファイルを消去する。Form1_FormClosed()から呼ばれる
		private void ClearOldCacheDBFile()
		{
			string[] files = Directory.GetFiles(Application.StartupPath, "*" + CACHEEXT);
			foreach (string sz in files)
			{
				bool isDel = true;
				string file1 = Path.GetFileNameWithoutExtension(sz);

				//MRUリストをチェック
				//MRUリストにないキャッシュファイルは削除する。
				int mruCount = g_Config.mru.Length;
				for (int i = 0; i < mruCount; i++)
				{
					//NullException対応。Nullの可能性有 ver0.982
					if (g_Config.mru[i] == null)
						continue;

					string file2 = Path.GetFileName(g_Config.mru[i].Name);
					if (file1.CompareTo(file2) == 0)
					{
						isDel = false;
						break;
					}
				}
				if (isDel)
				{
					try
					{
						File.Delete(sz);
					}
					catch
					{
						//ver1.19 エラーがあっても何もしない
					}
					Debug.WriteLine(sz, "Cacheを削除しました");
				}
			}
		}


		/// <summary> 
		/// ver1.35 現在のページをゴミ箱に入れる
		/// </summary>
		private void RecycleBinNowPage()
		{
			//アイテムがなにもなければなにもしない
			if (g_pi.Items.Count == 0)
				return;
			//2ページモードの時もなにもしない
			if (g_viewPages == 2)
				return;
			//アーカイブに対してもなにもしない
			if (g_pi.packType == PackageType.Archive)
				return;

			//今のページ番号を保存
			int now = g_pi.NowViewPage;
			string nowfile = g_pi.Items[now].filename;

			int next = GetNextPage(now);
			if (next != -1)
			{
				//後ろにページがあるので後ろのページを表示
				//Screenキャッシュを有効に使うため先にページ変更
				SetViewPage(next);

				//削除されたためページ番号を戻す
				g_pi.NowViewPage = now;
			}
			else if (now > 0)
			{
				//前のページに移動
				next = now - 1;
				SetViewPage(next);
			}
			else
			{
				//最後のページを消した
				Debug.WriteLine("最後のページを消した");
				PicPanel.bmp = null;
				PicPanel.ResetView();
				PicPanel.Refresh();
			}


			//ゴミ箱へ送る
			Uty.RecycleBin(nowfile);
			Debug.WriteLine(now.ToString() + "," + nowfile, "Delete");

			//piから削除
			g_pi.Items.RemoveAt(now);


			//ScreenCacheから削除
			ScreenCache.Clear();

			//Trackbarを変更
			InitTrackbar();

			////ページを切り替える
			//if (!CheckIndex(now))
			//    now--;
			//if (now < 0)
			//{
			//    //最後のページを消した
			//    Debug.WriteLine("最後のページを消した");
			//    PicPanel.bmp = null;
			//    PicPanel.ResetView();
			//    PicPanel.Refresh();
			//    //g_originalSizeBitmap.Dispose();
			//    g_pi.Items.Clear();
			//}
			//else
			//{
			//    SetViewPage(now);
			//}
		}


		//*****************************************************************
		// スライドショー関連
		private void Menu_SlideShow_Click(object sender, EventArgs e)
		{
			if (SlideShowTimer.Enabled)
			{
				StopSlideShow();
			}
			else
			{
				if (g_pi.Items.Count == 0)
					return;

				g_ClearPanel.ShowAndClose(
					"スライドショーを開始します。\r\nマウスクリックまたはキー入力で終了します。",
					1500);
				SlideShowTimer.Interval = g_Config.slideShowTime;
				//SlideShowTimer.Tick += new EventHandler(SlideShowTimer_Tick);
				SlideShowTimer.Start();
			}
		}

		void SlideShowTimer_Tick(object sender, EventArgs e)
		{
			if (GetNextPage(g_pi.NowViewPage) == -1)
				StopSlideShow();
			else
				NavigateToForword();
		}

		void StopSlideShow()
		{
			if (SlideShowTimer.Enabled)
			{
				//スライドショーを終了させる
				SlideShowTimer.Stop();
				g_ClearPanel.ShowAndClose("スライドショーを終了しました", 1500);
			}
		}



		/// <summary>
		/// IPCで呼ばれるインターフェースメソッド
		/// コマンドライン引数が入ってくるので
		/// それを起動。
		/// ただし、このメソッドが呼ばれるときはフォームのスレッドではないので
		/// Invokeが必要
		/// </summary>
		/// <param name="args">コマンドライン引数</param>
		void IRemoteObject.IPCMessage(string[] args)
		{
			//this.Activate();
			this.Invoke(((Action)(()=>
			{
				//自分を前面にする
				this.Activate();

				//表示対象ファイルを取得
				if (args.Length > 1)
				{
					//1つめに自分のexeファイル名が入っているので除く
					AsyncStart(args.Skip(1).ToArray());
				}
			})));
		}

		/// <summary>
		/// スタックにプッシュする
		/// SidebarやTrackbarから画像を取得したいときに使う。
		/// </summary>
		/// <param name="index"></param>
		/// <param name="f"></param>
		public void PushLow(int index, Delegate f)
		{
			stack.PushLow( new KeyValuePair<int,Delegate>(index, f));
		}

	} // Class Form1
}
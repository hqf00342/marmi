#define SEVENZIP	//SevenZipSharpを使うときはこれを定義する。

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form, IRemoteObject
    {
        //Form1参照用ハンドル
        public static Form1 _instance;

        //画面表示関連：今見ているページ数：１か２
        public int g_viewPages = 1;

        #region --- コントロール ---

        //メイン画面
        public PicturePanel PicPanel = new PicturePanel();

        //ルーペ
        private Loupe loupe = null;

        //TrackBar
        private ToolStripTrackBar _trackbar;

        //サムネイルパネル本体
        private ThumbnailPanel _thumbPanel = null;

        //フェードするPictureBox
        private ClearPanel _clearPanel = null;

        //サイドバー
        private SideBar _sidebar = null;

        //TrackBar用のサムネイル表示バー
        private NaviBar3 _trackNaviPanel = null;

        //ホバー中のメニュー/ツールアイテム。非Focusクリック対応
        private object _hoverStripItem = null;

        #endregion --- コントロール ---

        #region --- データクラス ---

        private readonly List<string> DeleteDirList = new List<string>();    //削除候補ディレクトリ

        //ver1.35 スクリーンキャッシュ
        //private Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();

        #endregion --- データクラス ---


        //マウスクリックされた位置を保存。ドラッグ操作用
        private Point g_LastClickPoint = Point.Empty;

        //ver1.51 事前のScreenCacheを作るかどうかのフラグ
        private bool needMakeScreenCache = false;

        //ver1.35 スライドショータイマー
        private readonly System.Windows.Forms.Timer SlideShowTimer = new System.Windows.Forms.Timer();

        //スライドショー中かどうか
        public bool IsSlideShow => SlideShowTimer.Enabled;

        public Form1()
        {
            this.Name = App.APPNAME;
            _instance = this;

            //DpiAware 2021年3月1日
            this.AutoScaleMode = AutoScaleMode.Dpi;
            //this.DpiChanged += Form1_DpiChanged;

            //設定ファイルの読み込みはProgram.csで実施

            //コントロールを追加。ツールストリップは最後に追加
            MyInitializeComponent();
            InitializeComponent();
            toolStrip1.Items.Add(_trackbar);
            //
            // ver1.62 ツールバーの位置
            //
            toolStrip1.Dock = App.Config.IsToolbarTop ? DockStyle.Top : DockStyle.Bottom;

            //初期設定
            this.KeyPreview = true;
            this.BackColor = App.Config.BackColor;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SetStyle(ControlStyles.Opaque, true);
            Application.Idle += Application_Idle;

            //ツールバーの文字を設定 ver1.67
            SetToolbarString();

            //非同期IOの開始
            AsyncIO.StartThread();
        }

        /// <summary>コンストラクタから一度だけ呼ばれる</summary>
        private void MyInitializeComponent()
        {
            //
            // PicPanel
            //
            this.Controls.Add(PicPanel);
            PicPanel.Enabled = true;
            PicPanel.Visible = true;
            PicPanel.Width = ClientRectangle.Width;
            PicPanel.BackColor = App.Config.BackColor;
            PicPanel.MouseClick += (s, e) => OnMouseClick(e);
            PicPanel.MouseDoubleClick += (s, e) => OnMouseDoubleClick(e);
            PicPanel.MouseMove += (s, e) => OnMouseMove(e);
            PicPanel.MouseUp += (s, e) => OnMouseUp(e);
            PicPanel.MouseWheel += PicPanel_MouseWheel;
            PicPanel.Dock = DockStyle.Fill;
            //
            //NaviBar
            //
            _sidebar = new SideBar();
            this.Controls.Add(_sidebar);
            _sidebar.Visible = false;
            _sidebar.Width = App.Config.SidebarWidth;
            _sidebar.Dock = DockStyle.Left;
            _sidebar.SidebarSizeChanged += Sidebar_SidebarSizeChanged;
            //
            //TrackBar
            //
            _trackbar = new ToolStripTrackBar();
            _trackbar.Name = "MarmiTrackBar";
            _trackbar.AutoSize = false;
            _trackbar.Size = new System.Drawing.Size(300, 20);
            _trackbar.ValueChanged += Trackbar_ValueChanged;
            _trackbar.MouseUp += Trackbar_MouseUp;
            _trackbar.MouseDown += Trackbar_MouseDown;
            _trackbar.MouseWheel += Trackbar_MouseWheel;
            //g_trackbar.MouseEnter += new EventHandler(g_trackbar_MouseEnter);
            //
            //サムネイルパネル
            //
            _thumbPanel = new ThumbnailPanel();
            this.Controls.Add(_thumbPanel);
            _thumbPanel.MouseMove += ThumbPanel_MouseMove;
            _thumbPanel.Init();
            _thumbPanel.Visible = false;
            _thumbPanel.Dock = DockStyle.Fill;
            //
            //ClearPanel
            //
            _clearPanel = new ClearPanel(PicPanel);

            //ホバー中のメニュー/ツールアイテム。非Focusクリック対応
            _hoverStripItem = null;

            //ver1.81 変更
            SetKeyConfig2();

            //ver1.35 スライドショータイマー
            SlideShowTimer.Tick += SlideShowTimer_Tick;
        }

        #region Event

        //protected override void OnDpiChanged(DpiChangedEventArgs e)
        //{
        //    base.OnDpiChanged(e);
        //}

        private async void Form1_Load(object sender, EventArgs e)
        {
            //設定をFormに適用する
            ApplyConfigToWindow();

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
                await Start(a);
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
            if (App.Config.isFullScreen)
            {
                SetFullScreen(false);
                //ver1.77 元に戻すけどモード保存はさせる
                App.Config.isFullScreen = true;
            }

            //非同期IOスレッドの終了
            AsyncIO.StopThread();

            //サムネイルモードの解放
            if (App.Config.isThumbnailView)
            {
                SetThumbnailView(false);
            }

            //7z解凍をしていたら中断
            //m_AsyncSevenZip?.CancelExtractAll();

            //スレッドが動作していたら停止させる.
            //サムネイルの保存
            //ファイルハンドルの解放
            InitControls();

            //ver1.62ツールバー位置を保存
            App.Config.IsToolbarTop = (toolStrip1.Dock == DockStyle.Top);

            ////////////////////////////////////////ver1.10

            //ver1.10
            //設定の保存
            if (App.Config.IsSaveConfig)
            {
                //設定ファイルを保存する
                App.Config.windowLocation = this.Location;
                App.Config.windowSize = this.Size;
                AppGlobalConfig.SaveToXmlFile(App.Config);
            }
            else
            {
                //設定ファイルがあれば削除する
                //string configFile = AppGlobalConfig.getConfigFileName();
                string configFile = AppGlobalConfig.ConfigFilename;
                if (File.Exists(configFile))
                    File.Delete(configFile);
            }

            //Application.Idleの解放
            Application.Idle -= Application_Idle;

            //ver1.57 susie解放
            App.susie.Dispose();
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

            Debug.WriteLine("OnDragDrop() Start");

            //ドロップされた物がファイルかどうかチェック
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Formをアクティブ
                this.Activate();
                string[] files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
                //Start(files);
                AsyncStart(files);
            }
            Debug.WriteLine("OnDragDrop() End");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            //Debug.WriteLine("OnResize()");

            //初期化前なら何もしない。
            //フォーム生成前にResize()は呼ばれる可能性がある。
            if (App.Config == null)
                return;

            //最小化時には何もしない
            if (this.WindowState == FormWindowState.Minimized)
                return;

            //ver0.972 Sidebarをリサイズ
            AjustSidebarArrangement();

            //サムネイルか？
            //Formが表示する前にも呼ばれるのでThumbPanel != nullは必須
            if (_thumbPanel != null && App.Config.isThumbnailView)
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
            //if (App.Config.isStopPaintingAtResize)
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
            //Debug.WriteLine("OnResizeEnd()");

            if (_thumbPanel != null && App.Config.isThumbnailView)
            {
                //サムネイル表示モード中
            }
            else
            {
                //画面を描写。ただしハイクオリティで
                UpdateStatusbar();
                if (PicPanel.Visible)
                    PicPanel.ResizeEnd();
            }

            //トラックバー表示を直す
            ResizeTrackBar();
        }

        private async void Application_Idle(object sender, EventArgs e)
        {
            UpdateToolbar();

            //低品質描写だったら高品質で書き直す
            if (PicPanel.LastDrawMode
                == System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor)
            {
                PicPanel.FastDraw = false;
                PicPanel.Refresh();
            }

            //ScreenCacheを作る必要があれば作成。
            if (needMakeScreenCache)
            {
                needMakeScreenCache = false;
                await ScreenCache.MakeCacheForPreAndNextPagesAsync();
                ScreenCache.Purge();
                App.g_pi.FileCacheCleanUp2(App.Config.CacheSize);
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

        #endregion Event

        #region START

        /// <summary>
        /// 非同期タイプのStart()
        /// 実際はThreadPoolからStartを呼び出しているだけ。
        /// D&DでDragしたプロセスが持ちきりになってしまうのを解決。
        /// OnDragDrop()からの呼び出しのみこれを使う。
        /// </summary>
        /// <param name="files"></param>
        private void AsyncStart(string[] files)
        {
            //ThreadPool.QueueUserWorkItem(_ => this.Invoke((MethodInvoker)(() => Start(files))));
            Task.Run(() => this.Invoke((MethodInvoker)(async () => await Start(files))));
        }

        private async Task Start(string[] filenames)
        {
            //ver1.73 MRUリストの更新
            //今まで見ていたものを登録する
            UpdateMRUList();

            //ファイルがすでに開いているかどうかチェック
            if (filenames.Length == 1 && filenames[0] == App.g_pi.PackageName)
            {
                const string text = "おなじフォルダ/ファイルを開こうとしています。開きますか？";
                const string title = "同一フォルダ/ファイルオープンの確認";
                if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //ver1.41 非同期IOを停止
            AsyncIO.ClearJob();
            AsyncIO.AddJob(-1, null);
            App.g_pi.Initialize();
            SetStatusbarInfo("準備中・・・" + filenames[0]);

            //ver1.35スクリーンキャッシュをクリア
            ScreenCache.Clear();

            //コントロールの初期化
            InitControls();
            //この時点ではg_pi.PackageNameはできていない＝MRUがつくられない。

            //ver1.78単一ファイルの場合、そのディレクトリを対象とする
            string onePicFile = string.Empty;
            if (filenames.Length == 1 && Uty.IsPictureFilename(filenames[0]))
            {
                onePicFile = filenames[0];
                filenames[0] = Path.GetDirectoryName(filenames[0]);
            }

            //書庫のパスワードをクリア
            SevenZipWrapper.ClearPassword();

            //ファイル一覧を生成
            bool needRecurse = SetPackageInfo(filenames);

            //ver1.37 再帰構造だけでなくSolid書庫も展開
            //ver1.79 常に一時書庫に展開オプションに対応
            //if (needRecurse )
            if (needRecurse || App.g_pi.isSolid || App.Config.AlwaysExtractArchive)
            {
                using (AsyncExtractForm ae = new AsyncExtractForm())
                {
                    SetStatusbarInfo("書庫を展開中です" + filenames[0]);

                    //ver1.73 一時フォルダ作成
                    try
                    {
                        var tempDir = SuggestTempDirName();
                        Directory.CreateDirectory(tempDir);
                        DeleteDirList.Add(tempDir);
                        App.g_pi.tempDirname = tempDir;
                    }
                    catch
                    {
                        //ver1.79 一時フォルダが作れないときの対応
                        MessageBox.Show("一時展開フォルダが作成できませんでした。設定を確認してください");
                        App.g_pi.Initialize();
                        return;
                    }

                    //ダイアログを表示
                    ae.ArchivePath = filenames[0];
                    ae.ExtractDir = App.g_pi.tempDirname;
                    ae.ShowDialog(this);

                    //ダイアログの表示が終了
                    //ディレクトリをすべてg_piに読み込む
                    this.Cursor = Cursors.WaitCursor;
                    App.g_pi.PackType = PackageType.Pictures;
                    App.g_pi.Items.Clear();
                    GetDirPictureList(App.g_pi.tempDirname, true);
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
            if (App.g_pi.PackType == PackageType.Pdf)
            {
                if (!App.susie.isSupportedExtentions("pdf"))
                {
                    const string str = "pdfファイルはサポートしていません";
                    _clearPanel.ShowAndClose(str, 1000);
                    SetStatusbarInfo(str);
                    App.g_pi.Initialize();
                    return;
                }
            }

            if (App.g_pi.Items.Count == 0)
            {
                //画面をクリア、準備中の文字を消す
                const string str = "表示できるファイルがありませんでした";
                _clearPanel.ShowAndClose(str, 1000);
                SetStatusbarInfo(str);
                return;
            }

            //ページを初期化
            App.g_pi.NowViewPage = 0;
            //CheckAndStart();

            //サムネイルDBがあれば読み込む
            //loadThumbnailDBFile();
            if (App.Config.IsContinueZipView)
            {
                //読み込み値を無視し、０にセット
                //g_pi.NowViewPage = 0;
                foreach (var mru in App.Config.Mru)
                {
                    if (mru == null)
                    {
                        continue;
                    }
                    else if (mru.Name == App.g_pi.PackageName
                        //ver1.79 コメントアウト
                        //&& g_pi.packType == PackageType.Archive)
                        )
                    {
                        //最終ページを設定する。
                        App.g_pi.NowViewPage = mru.LastViewPage;
                        //Bookmarkを設定する
                        App.g_pi.LoadBookmarkString(mru.Bookmarks);
                        break;
                    }
                }
            }

            //１ファイルドロップによるディレクトリ参照の場合
            //最初に見るページをドロップしたファイルにする。
            if (!string.IsNullOrEmpty(onePicFile))
            {
                int i = App.g_pi.Items.FindIndex(c => c.Filename == onePicFile);
                if (i < 0) i = 0;
                App.g_pi.NowViewPage = i;
            }

            //トラックバーを初期化
            InitTrackbar();

            //SideBarへ登録
            _sidebar.Init(App.g_pi);

            //タイトルバーの設定
            this.Text = $"{App.APPNAME} - {Path.GetFileName(App.g_pi.PackageName)}";

            //サムネイルの作成
            AsyncLoadImageInfo();

            //画像を表示
            PicPanel.Message = string.Empty;
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }

        /// <summary>
        /// パッケージ情報をpiに取る
        /// </summary>
        /// <param name="files">対象ファイル</param>
        /// <returns>書庫内書庫がある場合はtrue</returns>
        private static bool SetPackageInfo(string[] files)
        {
            //初期化
            App.g_pi.Initialize();

            if (files.Length == 1)
            {
                //ドロップされたのは1つ
                App.g_pi.PackageName = files[0];    //ディレクトリかZipファイル名を想定

                //ドロップされたファイルの詳細を探る
                if (Directory.Exists(App.g_pi.PackageName))
                {
                    //ディレクトリの場合
                    App.g_pi.PackType = PackageType.Directory;
                    GetDirPictureList(files[0], App.Config.IsRecurseSearchDir);
                }
                else if (App.unrar.dllLoaded && files[0].EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                {
                    //
                    //unrar.dllを使う。
                    //
                    App.g_pi.PackType = PackageType.Archive;
                    App.g_pi.isSolid = true;

                    //ファイルリストを構築
                    ListRar(files[0]);

                    //展開が必要なのでtrueを返す
                    return true;
                }
                else if (Uty.IsSupportArchiveFile(App.g_pi.PackageName))
                {
                    // 書庫ファイル
                    App.g_pi.PackType = PackageType.Archive;
                    bool needRecurse = GetArchivedFileInfo(files[0]);
                    if (needRecurse)
                        return true;
                }
                else if (files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    //pdfファイル
                    return ListPdf(files[0]);
                }
                else
                {
                    //単一画像ファイル
                    App.g_pi.PackageName = string.Empty;
                    App.g_pi.PackType = PackageType.Pictures;
                    if (Uty.IsPictureFilename(files[0]))
                    {
                        App.g_pi.Items.Add(new ImageInfo(0, files[0]));
                    }
                }
            }
            else //if (files.Length == 1)
            {
                //複数ファイルパターン
                App.g_pi.PackageName = string.Empty;
                App.g_pi.PackType = PackageType.Pictures;

                //ファイルを追加する
                int index = 0;
                foreach (string filename in files)
                {
                    if (Uty.IsPictureFilename(filename))
                    {
                        App.g_pi.Items.Add(new ImageInfo(index++, filename));
                    }
                }
            }//if (files.Length == 1)
            return false;

            /// <summary>unrarを使ってリスト化</summary>
            void ListRar(string file)
            {
                App.unrar.Open(file, Unrar.OpenMode.List);
                int num = 0;
                while (App.unrar.ReadHeader())
                {
                    if (!App.unrar.CurrentFile.IsDirectory)
                    {
                        App.g_pi.Items.Add(new ImageInfo(
                            num++,
                            App.unrar.CurrentFile.FileName,
                            App.unrar.CurrentFile.FileTime,
                            App.unrar.CurrentFile.UnpackedSize
                            ));
                    }
                    App.unrar.Skip();
                }
                App.unrar.Close();
            }

            bool ListPdf(string file)
            {
                App.g_pi.PackType = PackageType.Pdf;
                if (App.susie.isSupportPdf())
                {
                    foreach (var e in App.susie.GetArchiveInfo(file))
                    {
                        App.g_pi.Items.Add(new ImageInfo((int)e.position, e.filename, e.timestamp, e.filesize));
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
        }

        #endregion START

        // メニュー操作 *****************************************************************/

        public void SetThumbnailView(bool isShow)
        {
            if (isShow)
            {
                //表示準備する
                App.Config.isThumbnailView = true;

                //SideBarがある場合は消す
                if (_sidebar.Visible)
                    _sidebar.Visible = false;

                //トラックバーはDisable Ver0.975
                _trackbar.Enabled = false;

                //PicPanelを非表示に
                PicPanel.Visible = false;
                PicPanel.Dock = DockStyle.None;

                //表示する
                if (!this.Controls.Contains(_thumbPanel))
                    this.Controls.Add(_thumbPanel);
                if (!App.Config.isFullScreen)
                    _thumbPanel.BringToFront();        //ver1.83 最前面になるようにする。ツールバー対策
                _thumbPanel.Dock = DockStyle.Fill; //ver1.64
                _thumbPanel.Visible = true;
                toolButtonThumbnail.Checked = true;
                _thumbPanel.ReDraw();
            }
            else
            {
                //表示をやめる
                App.Config.isThumbnailView = false;
                //this.Controls.Remove(g_ThumbPanel);
                _thumbPanel.Visible = false;
                _thumbPanel.Dock = DockStyle.None; //ver1.64
                toolButtonThumbnail.Checked = false;

                //PicPanelを表示
                PicPanel.Dock = DockStyle.Fill;
                PicPanel.Visible = true;

                UpdateStatusbar();

                //NaviBarを戻す
                if (App.Config.VisibleNavibar)
                    _sidebar.Visible = true;

                //トラックバーを戻す Ver0.975
                _trackbar.Enabled = true;
            }
        }

        private async Task OpenDialog()
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
                    await Start(of.FileNames);
                }
            }
        }

        /// <summary>
        /// サイドバーとPicPanelの位置関係を調整する
        /// </summary>
        public void AjustSidebarArrangement()
        {
            Rectangle rect = GetClientRectangle();
            if (_sidebar.Visible)
            {
                //g_Sidebar.SetSizeAndDock(GetClientRectangle());
                _sidebar.Top = rect.Top;
                _sidebar.Left = rect.Left;
                _sidebar.Height = rect.Height;

                PicPanel.Top = rect.Top;
                PicPanel.Left = _sidebar.Right + 1;
                PicPanel.Width = rect.Width - _sidebar.Width;
                PicPanel.Height = rect.Height;

                if (_thumbPanel.Visible)
                {
                    _thumbPanel.Top = rect.Top;
                    _thumbPanel.Left = _sidebar.Right + 1;
                    _thumbPanel.Width = rect.Width - _sidebar.Width;
                    _thumbPanel.Height = rect.Height;
                }
            }
            else
            {
                //PicPanel.fastDraw = false;
                PicPanel.Bounds = rect;
                if (_thumbPanel.Visible)
                    _thumbPanel.Bounds = rect;
            }
        }

        private void ToggleBookmark()
        {
            if (App.g_pi.Items.Count > 0
                && App.g_pi.NowViewPage >= 0)
            {
                App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark
                    = !App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark;

                if (g_viewPages == 2)
                {
                    App.g_pi.Items[App.g_pi.NowViewPage + 1].IsBookMark
                        = !App.g_pi.Items[App.g_pi.NowViewPage + 1].IsBookMark;
                }
            }
        }

        /// <summary>
        /// 一時フォルダの名前を返す。
        /// 作れない場合はnullが返る。
        /// </summary>
        /// <returns>一時フォルダのフルパス。作れない場合はnull</returns>
        private static string SuggestTempDirName()
        {
            //存在しないランダムなフォルダ名を作る
            string tempDir;

            //tempフォルダのルートとなるフォルダを決める。
            string rootPath = App.Config.TmpFolder;
            if (string.IsNullOrEmpty(rootPath))
                rootPath = Application.StartupPath; //アプリのパス
                                                    //Path.GetTempPath(),		//windows標準のTempDir

            //ユニークなフォルダを探す
            do
            {
                tempDir = Path.Combine(
                    rootPath,
                    "TEMP7Z" + Path.GetRandomFileName().Substring(0, 8));
            }
            while (Directory.Exists(tempDir));
            return tempDir;
        }

        private void AsyncLoadImageInfo()
        {
            //ver1.54 2013年5月7日
            //全画像読込をスタックに積む
            for (int cnt = 0; cnt < App.g_pi.Items.Count; cnt++)
            {
                //ラムダ式の外で文字列作成
                var msg = $"画像情報読み込み中...{cnt + 1}/{App.g_pi.Items.Count}";


                //サムネイルを作成するだけなのでawaitせず高速に回す。
                AsyncIO.AddJobLow(cnt, () =>
                {
                    SetStatusbarInfo(msg);
                    //読み込んだものをPurge対象にする
                    App.g_pi.FileCacheCleanUp2(App.Config.CacheSize);
                });
            }
            //読み込み完了メッセージ
            AsyncIO.AddJobLow(App.g_pi.Items.Count - 1, () => SetStatusbarInfo("事前画像情報読み込み完了"));
        }

        private void InitTrackbar()
        {
            _trackbar.Minimum = 0;
            if (App.g_pi.Items.Count > 0)
            {
                _trackbar.Maximum = App.g_pi.Items.Count - 1;
                _trackbar.Enabled = true;
                _trackbar.Value = App.g_pi.NowViewPage;
            }
            else
            {
                _trackbar.Maximum = 0;
                _trackbar.Value = 0;
                _trackbar.Enabled = false;
            }
        }

        /// <summary>
        /// D&Dやアプリ起動時に呼ばれる初期化ルーチン
        /// スレッドを止め、すべての状態を初期化する。
        /// 読み込み対象のファイルについては一切何もしない。
        /// Form1_Load(), Form1_FormClosed(), OpenFileAndStart(), Form1_DragDrop()
        /// </summary>
        private void InitControls()
        {
            //サムネイルモードの解放
            if (App.Config.isThumbnailView)
                SetThumbnailView(false);

            //2011/08/19 サムネイル初期化
            _thumbPanel.Init();

            //2011年11月11日 ver1.24 サイドバー
            //g_Sidebar.Init();
            _sidebar.Init(null);   //ver1.37
            if (_sidebar.Visible)
                _sidebar.Invalidate();

            //ver1.25 trackNavi
            if (_trackNaviPanel != null)
            {
                _trackNaviPanel.Dispose();
                _trackNaviPanel = null;
            }

            //2011/08/19 trackbarを初期化
            //InitTrackBar();
            //nullReferのため何もしないことにする
            //g_trackbar.Initialize();

            //MRUを更新する ver1.73 コメントアウト
            //UpdateMRUList();

            //サムネイルをバックグラウンドで保存する
            //saveDBFile();
            App.g_pi.Initialize();

            //パッケージ情報を初期化
            //古いのはスレッド中で捨てるので新しいのを作る
            //g_pi = new PackageInfo();

            //7z解凍をしていたら中断
            //m_AsyncSevenZip?.CancelExtractAll();
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
            //App.stack.Clear();
            //App.stack.Push(new KeyValuePair<int, Delegate>(-1, null));
            AsyncIO.ClearJob();
            AsyncIO.AddJob(-1, null);

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
            PicPanel.Bmp = null;

            //GC: 2021年2月26日 前の書庫のガベージを消すためここでやっておく。
            Uty.ForceGC();
        }

        #region パッケージ操作

        private void SortPackage()
        {
            //ファイルリストを並び替える
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
                App.g_pi.Items.Sort(comparer);
            }
            return;
        }

        /// <summary>
        /// 書庫情報を取得
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>書庫内書庫がある場合はtrye</returns>
        private static bool GetArchivedFileInfo(string filename)
        {
            var szw = new SevenZipWrapper();
            bool retval = false;

            if (!szw.Open(filename))
            {
                MessageBox.Show("エラーのため書庫は開けませんでした。");
                App.g_pi.Initialize();
                //2021年2月26日 GCをやめる
                //Uty.ForceGC();      //書庫を開放・GCする必要がある
                return false;
            }

            //Zipファイル情報を設定
            App.g_pi.PackageName = filename;
            var fi = new FileInfo(App.g_pi.PackageName);
            App.g_pi.PackageSize = fi.Length;
            App.g_pi.isSolid = szw.IsSolid;

            //ver1.31 7zファイルなのにソリッドじゃないことがある！？
            if (Path.GetExtension(filename) == ".7z")
                App.g_pi.isSolid = true;

            //g_pi.isZip = true;
            App.g_pi.PackType = PackageType.Archive;

            //ファイルをリストに追加
            App.g_pi.Items.Clear();
            foreach (var item in szw.Items)
            {
                if (item.IsDirectory)
                    continue;
                if (Uty.IsPictureFilename(item.FileName))
                {
                    App.g_pi.Items.Add(new ImageInfo(item.Index, item.FileName, item.CreationTime, (long)item.Size));
                }
                else if (Uty.IsSupportArchiveFile(item.FileName))
                {
                    retval = true;
                }
            }
            return retval;
        }

        /// <summary>
        /// ディレクトリの画像をリストに追加する。再帰的に呼び出すために関数化
        /// </summary>
        /// <param name="dirName">追加対象のディレクトリ名</param>
        /// <param name="isRecurse">再帰的に走査する場合はtrue</param>
        private static void GetDirPictureList(string dirName, bool isRecurse)
        {
            //画像ファイルだけを追加する
            int index = 0;
            foreach (string name in Directory.EnumerateFiles(dirName))
            {
                if (Uty.IsPictureFilename(name))
                {
                    App.g_pi.Items.Add(new ImageInfo(index++, name));
                }
            }

            //再帰的に取得するかどうか。
            if (isRecurse)
            {
                foreach (var name in Directory.GetDirectories(dirName))
                    GetDirPictureList(name, isRecurse);
            }
        }

        #endregion パッケージ操作

        #region MRU操作

        /// <summary>
        /// 現在閲覧しているg_pi.PackageNameをMRUに追加する
        /// 以前も見たことがある場合、閲覧日付だけを更新
        /// </summary>
        private static void UpdateMRUList()
        {
            //なにも無ければ追加しない
            if (string.IsNullOrEmpty(App.g_pi.PackageName))
                return;

            //MRUに追加する必要があるか確認
            bool needMruAdd = true;
            for (int i = 0; i < App.Config.Mru.Count; i++)
            {
                if (App.Config.Mru[i] == null)
                    continue;
                if (App.Config.Mru[i].Name == App.g_pi.PackageName)
                {
                    //登録済みのMRUを更新
                    //日付だけ更新
                    App.Config.Mru[i].Date = DateTime.Now;
                    //最後に見たページも更新 v1.37
                    App.Config.Mru[i].LastViewPage = App.g_pi.NowViewPage;
                    needMruAdd = false;

                    //ver1.77 Bookmarkも設定
                    App.Config.Mru[i].Bookmarks = App.g_pi.CreateBookmarkString();
                }
            }
            if (needMruAdd)
            {
                App.Config.Mru.Add(new MRU(App.g_pi.PackageName, DateTime.Now, App.g_pi.NowViewPage, App.g_pi.CreateBookmarkString()));
            }
        }

        /// <summary>
        /// MRUリストを更新する。実際にメニューの中身を更新
        /// この関数を呼び出しているのはMenu_File_DropDownOpeningのみ
        /// </summary>
        private void UpdateMruMenuListUI()
        {
            MenuItem_FileRecent.DropDownItems.Clear();

            //Array.Sort(App.Config.mru);
            App.Config.Mru = App.Config.Mru.OrderBy(a => a.Date).ToList();

            int menuCount = 0;

            //新しい順にする
            for (int i = App.Config.Mru.Count - 1; i >= 0; i--)
            {
                if (App.Config.Mru[i] == null)
                    continue;

                MenuItem_FileRecent.DropDownItems.Add(App.Config.Mru[i].Name, null, new EventHandler(OnClickMRUMenu));

                //ver1.73 MRU表示数の制限
                if (++menuCount >= App.Config.NumberOfMru)
                    break;
            }
        }

        #endregion MRU操作

        /// <summary>
        /// 指定したインデックスの画像を表示する。
        /// publicになっている理由はサイドバーやサムネイル画面からの
        /// 呼び出しに対応するため。
        /// 前のページに戻らないようにdrawOrderTickを導入
        /// </summary>
        /// <param name="index">インデックス番号</param>
        /// <param name="drawOrderTick">描写順序を示すオーダー時間 = DateTime.Now.Ticks</param>
        public async Task SetViewPageAsync(int index, long drawOrderTick = 0)
        {
            if (drawOrderTick == 0)
                drawOrderTick = DateTime.Now.Ticks;

            //ver1.09 オプションダイアログを閉じると必ずここに来ることに対するチェック
            if (App.g_pi.Items == null || App.g_pi.Items.Count == 0)
                return;

            //ver1.36 Index範囲チェック
            Debug.Assert(index >= 0 && index < App.g_pi.Items.Count);

            // ページ進行方向 進む方向なら正、戻る方向なら負
            // アニメーションで利用する
            int pageDirection = index - App.g_pi.NowViewPage;

            //ページ番号を更新
            App.g_pi.NowViewPage = index;
            _trackbar.Value = index;

            //ver1.35 スクリーンキャッシュチェック
            if (ScreenCache.Dic.TryGetValue(index, out Bitmap screenImage))
            {
                //スクリーンキャッシュあったのですぐに描写
                SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
                Debug.WriteLine(index, "Use ScreenCache");
            }
            else
            {
                //ver1.50
                //Keyだけある{key,null}キャッシュだったら消す。稀に発生するため
                if (ScreenCache.Dic.ContainsKey(index))
                    ScreenCache.Remove(index);

                //ver1.50 読み込み中と表示
                SetStatusbarInfo("Now Loading ... " + (index + 1).ToString());

                //画像作成をスレッドプールに登録
                var img = await Bmp.MakeOriginalSizeImageAsync(index);
                if (img == null)
                {
                    PicPanel.Message = "読込みに時間がかかってます.リロードしてください";
                }
                else
                {
                    SetViewPage2(index, pageDirection, img, drawOrderTick);
                }

                //カーソルをWaitに
                //this.Cursor = Cursors.WaitCursor;
            }
        }

        private void SetViewPage2(int index, int pageDirection, Bitmap screenImage, long orderTime)
        {
            //ver1.55 drawOrderTickのチェック.
            // スレッドプールに入るため稀に順序が前後する。
            // 最新の描写でなければスキップ
            if (PicPanel.DrawOrderTime > orderTime)
            {
                Debug.WriteLine($"Skip SetViewPage2({index}) too old order={orderTime} < now={PicPanel.DrawOrderTime}");
                return;
            }

            //描写開始
            PicPanel.State = DrawStatus.drawing;
            PicPanel.DrawOrderTime = orderTime;

            if (screenImage == null)
            {
                Debug.WriteLine($"bmpがnull(index={index})");
                PicPanel.State = DrawStatus.idle;
                PicPanel.Message = "表示エラー 再度表示してみてください" + index.ToString();
                PicPanel.Refresh();
                return;
            }

            //ver1.50 表示
            PicPanel.State = DrawStatus.drawing;
            PicPanel.Message = string.Empty;
            if (App.Config.PictureSwitchMode != AnimateMode.none  //アニメーションモードである
                && !App.Config.KeepMagnification                  //倍率固定モードではアニメーションしない
                && pageDirection != 0)
            {
                //スライドインアニメーション
                PicPanel.AnimateSlideIn(screenImage, pageDirection);
            }

            PicPanel.Bmp = screenImage;
            PicPanel.ResetView();
            PicPanel.FastDraw = false;

            //ver1.78 倍率をオプション指定できるように変更
            if (!App.Config.KeepMagnification     //倍率維持モードではない
                || IsFitToScreen)             //画面にフィットしている
            {
                //画面切り替わり時はフィットモードで起動
                float r = PicPanel.FittingRatio;
                if (r > 1.0f && App.Config.NoEnlargeOver100p)
                    r = 1.0f;
                PicPanel.ZoomRatio = r;
            }

            //ページを描写
            PicPanel.AjustViewAndShow();

            //1ページ表示か2ページ表示か
            //viewPages = CanDualView(index) ? 2 : 1;
            g_viewPages = (int)screenImage.Tag;
            PicPanel.State = DrawStatus.idle;

            //カーソルを元に戻す
            this.Cursor = Cursors.Default;

            //UI更新
            UpdateStatusbar();
            UpdateToolbar();

            //サイドバーでアイテムを中心に
            if (_sidebar.Visible)
                _sidebar.SetItemToCenter(App.g_pi.NowViewPage);

            needMakeScreenCache = true;

            //PicPanel.Message = string.Empty;
            PicPanel.State = DrawStatus.idle;
            //2021年2月26日 GCをやめる
            //ToDo:ここだけはあったほうがいいかもしれないがLOHの扱いも同時にすべき
            //Uty.ForceGC();
        }

        // ユーティリティ系 *************************************************************/
        private void UpdateToolbar()
        {
            //画面モードの状態反映
            toolButtonDualMode.Checked = App.Config.DualView;
            toolButtonFullScreen.Checked = App.Config.isFullScreen;
            toolButtonThumbnail.Checked = App.Config.isThumbnailView;

            //Sidebar
            toolStripButton_Sidebar.Checked = _sidebar.Visible;

            if (App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //ファイルを閲覧していない場合のツールバー
                _trackbar.Enabled = false;
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

                if (App.Config.isThumbnailView)
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
                    if (App.Config.IsReplaceArrowButton)
                    {
                        //入れ替え
                        toolButtonLeft.Enabled = !IsLastPageViewing();      //最終ページチェック
                        toolButtonRight.Enabled = (bool)(App.g_pi.NowViewPage != 0);    //先頭ページチェック
                    }
                    else
                    {
                        toolButtonLeft.Enabled = (bool)(App.g_pi.NowViewPage != 0); //先頭ページチェック
                        toolButtonRight.Enabled = !IsLastPageViewing();     //最終ページチェック
                    }

                    //100%ズーム
                    toolStripButton_Zoom100.Checked = IsScreen100p;

                    //画面フィットズーム
                    toolStripButton_ZoomFit.Checked = IsFitToScreen;

                    //Favorite
                    if (App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark)
                    {
                        toolStripButton_Favorite.Checked = true;
                    }
                    else if (g_viewPages == 2
                        && App.g_pi.NowViewPage < App.g_pi.Items.Count - 1      //ver1.69 最終ページより前チェック
                        && App.g_pi.Items[App.g_pi.NowViewPage + 1].IsBookMark) //
                    {
                        toolStripButton_Favorite.Checked = true;
                    }
                    else
                    {
                        toolStripButton_Favorite.Checked = false;
                    }

                    //Sidebar
                    toolStripButton_Sidebar.Checked = _sidebar.Visible;
                }

                //TrackBar
                //ここで直すとUIが遅くなる。
                //g_trackbar.Value = g_pi.NowViewPage;
            }
        }

        #region Navigation

        private async Task NavigateToBackAsync()
        {
            //前に戻る
            long drawOrderTick = DateTime.Now.Ticks;
            int prev = await GetPrevPageIndex(App.g_pi.NowViewPage);
            if (prev >= 0)
            {
                await SetViewPageAsync(prev, drawOrderTick);
            }
            else
                _clearPanel.ShowAndClose("先頭のページです", 1000);
        }

        private async Task NavigateToForwordAsync()
        {
            //ver1.35 ループ機能を実装
            long drawOrderTick = DateTime.Now.Ticks;
            int now = App.g_pi.NowViewPage;
            int next = await GetNextPageIndexAsync(App.g_pi.NowViewPage);
            Debug.WriteLine($"NavigateToForword() {now} -> {next}");
            if (next >= 0)
            {
                await SetViewPageAsync(next, drawOrderTick);
            }
            else if (App.Config.LastPage_toTop)
            {
                //先頭ページへループ
                await SetViewPageAsync(0, drawOrderTick);
                _clearPanel.ShowAndClose("先頭ページに戻りました", 1000);
            }
            else
            {
                _clearPanel.ShowAndClose("最後のページです", 1000);
            }
        }

        /// <summary>
        /// 最終ページを見ているかどうか確認。２ページ表示に対応
        /// 先頭ページはそのまま０かどうかチェックするだけなので作成しない。
        /// </summary>
        /// <returns>最終ページであればtrue</returns>
        private bool IsLastPageViewing()
        {
            if (string.IsNullOrEmpty(App.g_pi.PackageName))
                return false;
            if (App.g_pi.Items.Count <= 1)
                return false;
            return App.g_pi.NowViewPage + g_viewPages >= App.g_pi.Items.Count;
        }

        //ver1.35 前のページ番号。すでに先頭ページなら-1
        internal static async Task<int> GetPrevPageIndex(int index)
        {
            if (index > 0)
            {
                int declimentPages = -1;
                //2ページ減らすことが出来るか
                if (await CanDualViewAsync(App.g_pi.NowViewPage - 2))
                    declimentPages = -2;

                int ret = index + declimentPages;
                return ret >= 0 ? ret : 0;
            }
            else
            {
                //すでに先頭ページなので-1を返す
                return -1;
            }
        }

        //ver1.36次のページ番号。すでに最終ページなら-1
        internal static async Task<int> GetNextPageIndexAsync(int index)
        {
            int pages = await CanDualViewAsync(index) ? 2 : 1;

            if (index + pages <= App.g_pi.Items.Count - 1)
            {
                return index + pages;
            }
            else
            {
                //最終ページ
                return -1;
            }
        }

        #endregion Navigation

        /// <summary>
        /// ver1.67 ツールバーの文字を表示/非表示する。
        /// </summary>
        private void SetToolbarString()
        {
            if (App.Config.EraseToolbarItemString)
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

        #region Screen操作

        private async Task SetDualViewModeAsync(bool isDual)
        {
            Debug.WriteLine(isDual, "SetDualViewMode()");
            App.Config.DualView = isDual;
            toolButtonDualMode.Checked = isDual;
            Menu_View2Page.Checked = isDual;

            //ver1.36 スクリーンキャッシュをクリア
            //ClearScreenCache();
            ScreenCache.Clear();

            await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010年6月20日
        }

        private void ToggleFullScreen()
        {
            SetFullScreen(!App.Config.isFullScreen);
        }

        private void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                //全画面にする
                App.Config.isFullScreen = true;

                menuStrip1.Visible = false;
                toolStrip1.Visible = false;
                statusbar.Visible = false;
                //App.Config.visibleMenubar = false;

                //Zオーダーを変更する
                this.Controls.Remove(statusbar);
                this.Controls.Remove(toolStrip1);
                this.Controls.Remove(menuStrip1);
                this.Controls.Remove(_sidebar);
                this.Controls.Remove(PicPanel);
                this.Controls.Remove(_thumbPanel);

                this.Controls.Add(menuStrip1);
                this.Controls.Add(toolStrip1);
                this.Controls.Add(statusbar);
                this.Controls.Add(PicPanel);
                this.Controls.Add(_thumbPanel);
                this.Controls.Add(_sidebar);

                if (this.WindowState != FormWindowState.Normal)
                    this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;

                //toolButtonFullScreen.Checked = true;			//これを先にやっておかないと
                this.WindowState = FormWindowState.Maximized;   //ここで発生するResizeイベントに間に合わない
            }
            else
            {
                //全画面を解除する
                App.Config.isFullScreen = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;

                //ツールバーを復元する /ver0.17 2009年3月8日
                //this.Controls.Add(toolStrip1);

                //Zオーダーを元に戻す
                this.Controls.Remove(statusbar);
                this.Controls.Remove(toolStrip1);
                this.Controls.Remove(menuStrip1);
                this.Controls.Remove(_sidebar);
                this.Controls.Remove(PicPanel);
                this.Controls.Remove(_thumbPanel);

                this.Controls.Add(PicPanel);
                this.Controls.Add(_thumbPanel);
                this.Controls.Add(_sidebar);
                this.Controls.Add(toolStrip1);
                this.Controls.Add(statusbar);
                this.Controls.Add(menuStrip1);

                toolButtonFullScreen.Checked = false;
                toolStrip1.Visible = App.Config.VisibleToolBar;
                menuStrip1.Visible = App.Config.VisibleMenubar;
                statusbar.Visible = App.Config.VisibleStatusBar;
            }

            //メニュー、ツールバーの更新
            Menu_ViewFullScreen.Checked = App.Config.isFullScreen;
            Menu_ContextFullView.Checked = App.Config.isFullScreen;
            toolButtonFullScreen.Checked = App.Config.isFullScreen;

            AjustSidebarArrangement();
            UpdateStatusbar();

            //画面表示を修復
            if (PicPanel.Visible)
                PicPanel.ResizeEnd();
            else if (_thumbPanel.Visible)
                _thumbPanel.ReDraw();
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
            var rect = this.ClientRectangle; // this.Bounds;

            //ツールバーの高さ
            int toolbarHeight = (toolStrip1.Visible && !App.Config.isFullScreen) ? toolStrip1.Height : 0;

            //メニューバーの高さ
            int menubarHeight = (menuStrip1.Visible) ? menuStrip1.Height : 0;

            //ステータスバーの高さ
            int statusbarHeight = (statusbar.Visible && !App.Config.isFullScreen) ? statusbar.Height : 0;

            //ツールバーが上の時だけYから控除
            if (App.Config.IsToolbarTop)
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
        /// 指定されたインデックスから２枚表示できるかチェック
        /// チェックはImageInfoに取り込まれた値を利用、縦横比で確認する。
        /// </summary>
        /// <param name="index">インデックス値</param>
        /// <returns>2画面表示できるときはtrue</returns>
        public static async Task<bool> CanDualViewAsync(int index)
        {
            //最後のページになっていないか確認
            if (index >= App.g_pi.Items.Count - 1 || index < 0)
                return false;

            //コンフィグ条件を確認
            if (!App.Config.DualView)
                return false;

            //ver1.79判定なしの2ページ表示
            if (App.Config.DualView_Force)
                return true;

            //1枚目チェック
            //if (!App.g_pi.Items[index].HasInfo)
            //    Bmp.SyncGetBitmapSize(index);
            await Bmp.LoadBitmapAsync(index, true);
            if (App.g_pi.Items[index].IsFat)
                return false; //横長だった

            //２枚目チェック
            //if (!App.g_pi.Items[index + 1].HasInfo)
            //    Bmp.SyncGetBitmapSize(index + 1);
            await Bmp.LoadBitmapAsync(index + 1, true);
            if (App.g_pi.Items[index + 1].IsFat)
                return false; //横長だった

            //全て縦長だった時の処理
            if (App.Config.DualView_Normal)
                return true; //縦画像2枚

            //2画像の高さがほとんど変わらなければtrue
            const int ACCEPTABLE_RANGE = 200;
            return Math.Abs(App.g_pi.Items[index].Height - App.g_pi.Items[index + 1].Height) < ACCEPTABLE_RANGE;
        }

        /// <summary>表示中の画像が画面いっぱいにフィットしているかどうか</summary>
        private bool IsFitToScreen => Math.Abs(PicPanel.ZoomRatio - PicPanel.FittingRatio) < 0.001f;

        /// <summary>現在の表示が原寸かどうか</summary>
        private bool IsScreen100p => Math.Abs(PicPanel.ZoomRatio - 1.0f) < 0.001f;

        #endregion Screen操作

        /// <summary>
        /// 現在のページをゴミ箱に入れる。削除後にページ遷移を行う。(ver1.35)
        /// </summary>
        private async Task RecycleBinNowPageAsync()
        {
            //アイテムがなにもなければなにもしない
            if (App.g_pi.Items.Count == 0) return;

            //2ページモードの時もなにもしない
            if (g_viewPages == 2) return;

            //アーカイブに対してもなにもしない
            if (App.g_pi.PackType == PackageType.Archive) return;

            //今のページ番号を保存
            int now = App.g_pi.NowViewPage;
            string nowfile = App.g_pi.Items[now].Filename;

            //ページ遷移
            int next = await GetNextPageIndexAsync(now);
            if (next != -1)
            {
                //次ページがあるので次ページを表示
                //Screenキャッシュを有効に使うため先にページ変更
                await SetViewPageAsync(next);

                //削除されたためページ番号を戻す
                App.g_pi.NowViewPage = now;
            }
            else if (now > 0)
            {
                //前のページに移動
                next = now - 1;
                await SetViewPageAsync(next);
            }
            else
            {
                //next=0 : 最後のページを消した
                Debug.WriteLine("最後のページを消した");
                PicPanel.Bmp = null;
                PicPanel.ResetView();
                PicPanel.Refresh();
            }

            //ゴミ箱へ送る
            Uty.RecycleBin(nowfile);
            Debug.WriteLine(now.ToString() + "," + nowfile, "Delete");

            //piから削除
            App.g_pi.Items.RemoveAt(now);

            //ScreenCacheから削除
            ScreenCache.Clear();

            //Trackbarを変更
            InitTrackbar();
        }

        #region スライドショー

        private void Menu_SlideShow_Click(object sender, EventArgs e)
        {
            if (SlideShowTimer.Enabled)
            {
                StopSlideShow();
            }
            else
            {
                if (App.g_pi.Items.Count == 0)
                    return;

                _clearPanel.ShowAndClose(
                    "スライドショーを開始します。\r\nマウスクリックまたはキー入力で終了します。",
                    1500);
                SlideShowTimer.Interval = App.Config.SlideShowTime;
                //SlideShowTimer.Tick += new EventHandler(SlideShowTimer_Tick);
                SlideShowTimer.Start();
            }
        }

        private async void SlideShowTimer_Tick(object sender, EventArgs e)
        {
            if (await GetNextPageIndexAsync(App.g_pi.NowViewPage) == -1)
                StopSlideShow();
            else
                await NavigateToForwordAsync();
        }

        private void StopSlideShow()
        {
            if (SlideShowTimer.Enabled)
            {
                //スライドショーを終了させる
                SlideShowTimer.Stop();
                _clearPanel.ShowAndClose("スライドショーを終了しました", 1500);
            }
        }

        #endregion スライドショー

        /// <summary>
        /// IPCで呼ばれるインターフェースメソッド
        /// コマンドライン引数が入ってくるのでそれを起動。
        /// ただし、このメソッドが呼ばれるときはフォームのスレッドではないので
        /// Invokeが必要
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        void IRemoteObject.IPCMessage(string[] args)
        {
            this.Invoke((Action)(() =>
            {
                //自分を前面にする
                this.Activate();

                //表示対象ファイルを取得
                //1つめに自分のexeファイル名が入っているので除く
                if (args.Length > 1)
                {
                    AsyncStart(args.Skip(1).ToArray());
                }
            }));
        }

        #region UIエレメントからのイベント

        private void Menu_Unsharp_Click(object sender, EventArgs e)
        {
            App.Config.UseUnsharpMask = !App.Config.UseUnsharpMask;
            MenuItem_Unsharp.Checked = App.Config.UseUnsharpMask;

            //再描写
            PicPanel.Invalidate();
        }

        private void Menu_Help_GC_Clicked(object sender, EventArgs e) => Uty.ForceGC();

        /// <summary>
        /// サイドバーのサイズが変更された（され終わった）ときに呼び出される。
        /// これが呼び出されるときはサイドバー固定の時のみ。
        /// 2010年6月6日 ver0.985で実装
        /// </summary>
        /// <param name="sender">利用せず</param>
        /// <param name="e">利用せず</param>
        private void Sidebar_SidebarSizeChanged(object sender, EventArgs e)
        {
            OnResizeEnd(null);
        }

        #endregion UIエレメントからのイベント
    } // Class Form1
}
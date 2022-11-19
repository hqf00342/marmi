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
        public int g_viewPages { get; set; } = 1;

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
        private readonly Timer SlideShowTimer = new Timer();

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
            this.BackColor = (App.Config.General.BackColor.A==0) ? Color.SlateBlue : App.Config.General.BackColor;
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
            PicPanel.BackColor = App.Config.General.BackColor;
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
            //_sidebar.SidebarSizeChanged += Sidebar_SidebarSizeChanged;
            _sidebar.SidebarSizeChanged += (s, e) => OnResizeEnd(null);
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
            await InitMarmiAsync();
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
                await StartAsync(a);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            //非表示
            this.Hide();

            //ver1.77 MRUリストの更新
            App.Config.AddMRU(App.g_pi);

            //非同期IOスレッドの終了
            AsyncIO.StopThread();

            //Tempディレクトリの削除
            DeleteAllTempDirs();

            //設定の保存
            if (App.Config.General.IsSaveConfig)
            {
                XmlFile.SaveToXmlFile(App.Config, App.ConfigFilename);
            }
            else
            {
                //コンフィグファイルを削除
                if (File.Exists(App.ConfigFilename))
                    File.Delete(App.ConfigFilename);
            }

            //Application.Idleの解放
            Application.Idle -= Application_Idle;

            //ver1.57 susie解放
            App.susie?.Dispose();
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                drgevent.Effect = DragDropEffects.All;
            else
                drgevent.Effect = DragDropEffects.None;
        }

        protected override async void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            Debug.WriteLine("OnDragDrop() Start");

            //ドロップされた物がファイルかどうかチェック
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Formをアクティブ
                this.Activate();
                string[] files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];

                //2022年9月17日 非同期IOを中止
                await AsyncIO.ClearJobAndWaitAsync();

#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                //await StartAsync(files);
                StartAsync(files);
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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
            if (_thumbPanel != null && ViewState.ThumbnailView)
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
            Debug.WriteLine("OnResizeEnd()");

            //ウィンドウサイズ、位置を保存
            if (this.WindowState == FormWindowState.Normal)
            {
                App.Config.windowSize = this.Size; //new Size(this.Width, this.Height);
                App.Config.windowLocation = this.Location; // new Point(this.Left, this.Top);
            }

            if (_thumbPanel != null && ViewState.ThumbnailView)
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
                if (App.Config.UseScreenCache)
                {
                    await ScreenCache.MakeCacheForPreAndNextPagesAsync();
                    ScreenCache.Purge();
                }
                App.g_pi.FileCacheCleanUp2(App.Config.Advance.CacheSize);
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

        // メニュー操作 *****************************************************************/

        public void SetThumbnailView(bool isShow)
        {
            if (isShow)
            {
                //表示準備する
                ViewState.ThumbnailView = true;

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
                if (!ViewState.FullScreen)
                    _thumbPanel.BringToFront();        //ver1.83 最前面になるようにする。ツールバー対策
                _thumbPanel.Dock = DockStyle.Fill; //ver1.64
                _thumbPanel.Visible = true;
                toolButtonThumbnail.Checked = true;
                _thumbPanel.ReDraw();
            }
            else
            {
                //表示をやめる
                ViewState.ThumbnailView = false;
                //this.Controls.Remove(g_ThumbPanel);
                _thumbPanel.Visible = false;
                _thumbPanel.Dock = DockStyle.None; //ver1.64
                toolButtonThumbnail.Checked = false;

                //PicPanelを表示
                PicPanel.Dock = DockStyle.Fill;
                PicPanel.Visible = true;

                UpdateStatusbar();

                //NaviBarを戻す
                if (ViewState.VisibleSidebar)
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
                    await StartAsync(of.FileNames);
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
        /// 全画像読込をジョブスタックに積む
        /// </summary>
        private void PreloadAllImages()
        {
            //ver1.54 2013年5月7日
            for (int cnt = 0; cnt < App.g_pi.Items.Count; cnt++)
            {
                //ラムダ式の外で文字列作成
                var msg = $"画像情報読み込み中...{cnt + 1}/{App.g_pi.Items.Count}";

                //サムネイルを作成するだけなのでawaitせず高速に回す。
                AsyncIO.AddJobLow(cnt, () =>
                {
                    SetStatusbarInfo(msg);
                    //読み込んだものをPurge対象にする
                    App.g_pi.FileCacheCleanUp2(App.Config.Advance.CacheSize);
                });
            }
            //読み込み完了メッセージ
            AsyncIO.AddJobLow(App.g_pi.Items.Count - 1, () => SetStatusbarInfo("事前画像情報読み込み完了"));
        }

        // ユーティリティ系 *************************************************************/

        #region Screen操作

        private async Task SetDualViewModeAsync(bool isDual)
        {
            Debug.WriteLine(isDual, "SetDualViewMode()");
            ViewState.DualView = isDual;
            toolButtonDualMode.Checked = isDual;
            Menu_View2Page.Checked = isDual;

            //ver1.36 スクリーンキャッシュをクリア
            //ClearScreenCache();
            ScreenCache.Clear();

            await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010年6月20日
        }

        private void ToggleFullScreen()
        {
            SetFullScreen(!ViewState.FullScreen);
        }

        private void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                //全画面にする
                ViewState.FullScreen = true;

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
                ViewState.FullScreen = false;
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
                toolStrip1.Visible = ViewState.VisibleToolBar;
                menuStrip1.Visible = ViewState.VisibleMenubar;
                statusbar.Visible = ViewState.VisibleStatusBar;
            }

            //メニュー、ツールバーの更新
            Menu_ViewFullScreen.Checked = ViewState.FullScreen;
            Menu_ContextFullView.Checked = ViewState.FullScreen;
            toolButtonFullScreen.Checked = ViewState.FullScreen;

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
            int toolbarHeight = (toolStrip1.Visible && !ViewState.FullScreen) ? toolStrip1.Height : 0;

            //メニューバーの高さ
            int menubarHeight = (menuStrip1.Visible) ? menuStrip1.Height : 0;

            //ステータスバーの高さ
            int statusbarHeight = (statusbar.Visible && !ViewState.FullScreen) ? statusbar.Height : 0;

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
                PicPanel.ResetZoomAndAlpha();
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
                    //AsyncStart(args.Skip(1).ToArray());
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                    StartAsync(args.Skip(1).ToArray());
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                }
            }));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //フォーカスをもらった時のクリックがツールバーアイテムを押していたなら
            //そのツールバーを実行する。
            if (_hoverStripItem is ToolStripItem cnt)
            {
                cnt.PerformClick();
            }
        }

        /// <summary>
        /// ウィンドウを最小化する。
        /// Toggleにしたが実質最小化のみ。
        /// </summary>
        private void ToggleFormSizeMinNormal()
        {
            this.WindowState = (this.WindowState == FormWindowState.Minimized)
                ? FormWindowState.Normal
                : FormWindowState.Minimized;
        }
    }
}
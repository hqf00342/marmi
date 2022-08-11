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
            this.BackColor = App.Config.General.BackColor;
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
            InitMarmi();
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
            InitMarmi();

            //ver1.62ツールバー位置を保存
            App.Config.IsToolbarTop = (toolStrip1.Dock == DockStyle.Top);

            ////////////////////////////////////////ver1.10

            //ver1.10
            //設定の保存
            if (App.Config.General.IsSaveConfig)
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
                //Start(files);
                //AsyncStart(files);
                await Start(files);
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
                    App.g_pi.FileCacheCleanUp2(App.Config.Advance.CacheSize);
                });
            }
            //読み込み完了メッセージ
            AsyncIO.AddJobLow(App.g_pi.Items.Count - 1, () => SetStatusbarInfo("事前画像情報読み込み完了"));
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
                if (++menuCount >= App.Config.General.NumberOfMru)
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

            //回転情報を適用
            var rotate = App.g_pi.Items[App.g_pi.NowViewPage].Rotate;
            if (rotate != 0)
            {
                PicPanel.Rotate(rotate);
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
            if (App.Config.View.PictureSwitchMode != AnimateMode.none  //アニメーションモードである
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
                if (r > 1.0f && App.Config.View.NoEnlargeOver100p)
                    r = 1.0f;
                PicPanel.ZoomRatio = r;
            }

            //ページを描写
            PicPanel.AjustViewAndShow();

            //1ページ表示か2ページ表示か
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
            else if (App.Config.View.LastPage_toTop)
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
            //最後のページならfalse
            if (index >= App.g_pi.Items.Count - 1 || index < 0)
                return false;

            //コンフィグ条件を確認
            if (!App.Config.DualView)
                return false;

            //ver1.79：2ページ強制表示
            if (App.Config.View.DualView_Force)
                return true;

            //1枚目読み込み
            if (App.g_pi.Items[index].ImgSize == Size.Empty)
            {
                await Bmp.LoadBitmapAsync(index, true);
            }

            //1枚目が横長ならfalse
            if (App.g_pi.Items[index].IsFat)
                return false;

            //2枚目読み込み
            if (App.g_pi.Items[index + 1].ImgSize == Size.Empty)
            {
                await Bmp.LoadBitmapAsync(index + 1, true);
            }

            //2枚目が横長ならfalse
            if (App.g_pi.Items[index + 1].IsFat)
                return false; //横長だった

            //全て縦長だった時の処理
            if (App.Config.View.DualView_Normal)
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
                    //AsyncStart(args.Skip(1).ToArray());
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                    Start(args.Skip(1).ToArray());
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                }
            }));
        }

        #region UIエレメントからのイベント

        private void Menu_Unsharp_Click(object sender, EventArgs e)
        {
            App.Config.Advance.UseUnsharpMask = !App.Config.Advance.UseUnsharpMask;
            MenuItem_Unsharp.Checked = App.Config.Advance.UseUnsharpMask;

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
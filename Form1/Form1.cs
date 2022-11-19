#define SEVENZIP	//SevenZipSharp���g���Ƃ��͂�����`����B

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
        //Form1�Q�Ɨp�n���h��
        public static Form1 _instance;

        //��ʕ\���֘A�F�����Ă���y�[�W���F�P���Q
        public int g_viewPages { get; set; } = 1;

        #region --- �R���g���[�� ---

        //���C�����
        public PicturePanel PicPanel = new PicturePanel();

        //���[�y
        private Loupe loupe = null;

        //TrackBar
        private ToolStripTrackBar _trackbar;

        //�T���l�C���p�l���{��
        private ThumbnailPanel _thumbPanel = null;

        //�t�F�[�h����PictureBox
        private ClearPanel _clearPanel = null;

        //�T�C�h�o�[
        private SideBar _sidebar = null;

        //TrackBar�p�̃T���l�C���\���o�[
        private NaviBar3 _trackNaviPanel = null;

        //�z�o�[���̃��j���[/�c�[���A�C�e���B��Focus�N���b�N�Ή�
        private object _hoverStripItem = null;

        #endregion --- �R���g���[�� ---

        #region --- �f�[�^�N���X ---

        private readonly List<string> DeleteDirList = new List<string>();    //�폜���f�B���N�g��

        //ver1.35 �X�N���[���L���b�V��
        //private Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();

        #endregion --- �f�[�^�N���X ---

        //�}�E�X�N���b�N���ꂽ�ʒu��ۑ��B�h���b�O����p
        private Point g_LastClickPoint = Point.Empty;

        //ver1.51 ���O��ScreenCache����邩�ǂ����̃t���O
        private bool needMakeScreenCache = false;

        //ver1.35 �X���C�h�V���[�^�C�}�[
        private readonly Timer SlideShowTimer = new Timer();

        //�X���C�h�V���[�����ǂ���
        public bool IsSlideShow => SlideShowTimer.Enabled;

        public Form1()
        {
            this.Name = App.APPNAME;
            _instance = this;

            //DpiAware 2021�N3��1��
            this.AutoScaleMode = AutoScaleMode.Dpi;
            //this.DpiChanged += Form1_DpiChanged;

            //�ݒ�t�@�C���̓ǂݍ��݂�Program.cs�Ŏ��{

            //�R���g���[����ǉ��B�c�[���X�g���b�v�͍Ō�ɒǉ�
            MyInitializeComponent();
            InitializeComponent();
            toolStrip1.Items.Add(_trackbar);
            //
            // ver1.62 �c�[���o�[�̈ʒu
            //
            toolStrip1.Dock = App.Config.IsToolbarTop ? DockStyle.Top : DockStyle.Bottom;

            //�����ݒ�
            this.KeyPreview = true;
            this.BackColor = (App.Config.General.BackColor.A==0) ? Color.SlateBlue : App.Config.General.BackColor;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SetStyle(ControlStyles.Opaque, true);
            Application.Idle += Application_Idle;

            //�c�[���o�[�̕�����ݒ� ver1.67
            SetToolbarString();

            //�񓯊�IO�̊J�n
            AsyncIO.StartThread();
        }

        /// <summary>�R���X�g���N�^�����x�����Ă΂��</summary>
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
            //�T���l�C���p�l��
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

            //�z�o�[���̃��j���[/�c�[���A�C�e���B��Focus�N���b�N�Ή�
            _hoverStripItem = null;

            //ver1.81 �ύX
            SetKeyConfig2();

            //ver1.35 �X���C�h�V���[�^�C�}�[
            SlideShowTimer.Tick += SlideShowTimer_Tick;
        }

        #region Event

        //protected override void OnDpiChanged(DpiChangedEventArgs e)
        //{
        //    base.OnDpiChanged(e);
        //}

        private async void Form1_Load(object sender, EventArgs e)
        {
            //�ݒ��Form�ɓK�p����
            ApplyConfigToWindow();

            //������
            await InitMarmiAsync();
            UpdateToolbar();
            ResizeTrackBar();

            //�N���p�����[�^�̊m�F
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                //�\���Ώۃt�@�C�����擾
                //1�߂Ɏ�����exe�t�@�C�����������Ă���̂ŏ���
                string[] a = new string[args.Length - 1];
                for (int i = 1; i < args.Length; i++)
                    a[i - 1] = args[i];

                //�t�@�C����n���ĊJ�n
                //CheckAndStart(a);
                await StartAsync(a);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            //��\��
            this.Hide();

            //ver1.77 MRU���X�g�̍X�V
            App.Config.AddMRU(App.g_pi);

            //�񓯊�IO�X���b�h�̏I��
            AsyncIO.StopThread();

            //Temp�f�B���N�g���̍폜
            DeleteAllTempDirs();

            //�ݒ�̕ۑ�
            if (App.Config.General.IsSaveConfig)
            {
                XmlFile.SaveToXmlFile(App.Config, App.ConfigFilename);
            }
            else
            {
                //�R���t�B�O�t�@�C�����폜
                if (File.Exists(App.ConfigFilename))
                    File.Delete(App.ConfigFilename);
            }

            //Application.Idle�̉��
            Application.Idle -= Application_Idle;

            //ver1.57 susie���
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

            //�h���b�v���ꂽ�����t�@�C�����ǂ����`�F�b�N
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Form���A�N�e�B�u
                this.Activate();
                string[] files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];

                //2022�N9��17�� �񓯊�IO�𒆎~
                await AsyncIO.ClearJobAndWaitAsync();

#pragma warning disable CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
                //await StartAsync(files);
                StartAsync(files);
#pragma warning restore CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
            }
            Debug.WriteLine("OnDragDrop() End");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            //Debug.WriteLine("OnResize()");

            //�������O�Ȃ牽�����Ȃ��B
            //�t�H�[�������O��Resize()�͌Ă΂��\��������B
            if (App.Config == null)
                return;

            //�ŏ������ɂ͉������Ȃ�
            if (this.WindowState == FormWindowState.Minimized)
                return;

            //ver0.972 Sidebar�����T�C�Y
            AjustSidebarArrangement();

            //�T���l�C�����H
            //Form���\������O�ɂ��Ă΂��̂�ThumbPanel != null�͕K�{
            if (_thumbPanel != null && ViewState.ThumbnailView)
            {
                //ver1.64 DockStyle�ɂ����̂ŃR�����g�A�E�g
                //Rectangle rect = GetClientRectangle();
                //g_ThumbPanel.Location = rect.Location;
                //g_ThumbPanel.Size = rect.Size;

                //ver0.91 ������return���Ȃ��Ƒʖڂł́H
                //ThumbPanel.Refresh();
                //return;
            }

            ////���T�C�Y���ɕ`�ʂ��Ȃ��ݒ肩
            //if (App.Config.isStopPaintingAtResize)
            //    return;

            //�X�e�[�^�X�o�[�ɔ{���\��
            UpdateStatusbar();

            //ver1.60 �^�C�g���o�[DClick�ɂ��ő剻�̎��AResizeEnd()����΂Ȃ�
            if (this.WindowState == FormWindowState.Maximized)
                OnResizeEnd(null);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Debug.WriteLine("OnResizeEnd()");

            //�E�B���h�E�T�C�Y�A�ʒu��ۑ�
            if (this.WindowState == FormWindowState.Normal)
            {
                App.Config.windowSize = this.Size; //new Size(this.Width, this.Height);
                App.Config.windowLocation = this.Location; // new Point(this.Left, this.Top);
            }

            if (_thumbPanel != null && ViewState.ThumbnailView)
            {
                //�T���l�C���\�����[�h��
            }
            else
            {
                //��ʂ�`�ʁB�������n�C�N�I���e�B��
                UpdateStatusbar();
                if (PicPanel.Visible)
                    PicPanel.ResizeEnd();
            }

            //�g���b�N�o�[�\���𒼂�
            ResizeTrackBar();
        }

        private async void Application_Idle(object sender, EventArgs e)
        {
            UpdateToolbar();

            //��i���`�ʂ������獂�i���ŏ�������
            if (PicPanel.LastDrawMode
                == System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor)
            {
                PicPanel.FastDraw = false;
                PicPanel.Refresh();
            }

            //ScreenCache�����K�v������΍쐬�B
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

            /// ���L�[�֑Ή�������
            switch (e.KeyCode)
            {
                //���L�[�������ꂽ���Ƃ�\������
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

        // ���j���[���� *****************************************************************/

        public void SetThumbnailView(bool isShow)
        {
            if (isShow)
            {
                //�\����������
                ViewState.ThumbnailView = true;

                //SideBar������ꍇ�͏���
                if (_sidebar.Visible)
                    _sidebar.Visible = false;

                //�g���b�N�o�[��Disable Ver0.975
                _trackbar.Enabled = false;

                //PicPanel���\����
                PicPanel.Visible = false;
                PicPanel.Dock = DockStyle.None;

                //�\������
                if (!this.Controls.Contains(_thumbPanel))
                    this.Controls.Add(_thumbPanel);
                if (!ViewState.FullScreen)
                    _thumbPanel.BringToFront();        //ver1.83 �őO�ʂɂȂ�悤�ɂ���B�c�[���o�[�΍�
                _thumbPanel.Dock = DockStyle.Fill; //ver1.64
                _thumbPanel.Visible = true;
                toolButtonThumbnail.Checked = true;
                _thumbPanel.ReDraw();
            }
            else
            {
                //�\������߂�
                ViewState.ThumbnailView = false;
                //this.Controls.Remove(g_ThumbPanel);
                _thumbPanel.Visible = false;
                _thumbPanel.Dock = DockStyle.None; //ver1.64
                toolButtonThumbnail.Checked = false;

                //PicPanel��\��
                PicPanel.Dock = DockStyle.Fill;
                PicPanel.Visible = true;

                UpdateStatusbar();

                //NaviBar��߂�
                if (ViewState.VisibleSidebar)
                    _sidebar.Visible = true;

                //�g���b�N�o�[��߂� Ver0.975
                _trackbar.Enabled = true;
            }
        }

        private async Task OpenDialog()
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.DefaultExt = "zip";
                of.FileName = "";
                of.Filter = "�Ή��t�@�C���`��(���Ƀt�@�C��;�摜�t�@�C��)|*.zip;*.lzh;*.tar;*.rar;*.7z;*.jpg;*.bmp;*.gif;*.ico;*.png;*.jpeg|"
                    + "���Ƀt�@�C��|*.zip;*.lzh;*.tar;*.rar;*.7z|"
                    + "�摜�t�@�C��|*.jpg;*.bmp;*.gif;*.ico;*.png;*.jpeg|"
                    + "���ׂẴt�@�C��|*.*";
                of.FilterIndex = 1;
                of.CheckFileExists = true;
                of.Multiselect = true;
                of.RestoreDirectory = true;

                if (of.ShowDialog() == DialogResult.OK)
                {
                    //ver1.09 OpenFileAndStart()�Ƃ��߂ɔ����W�J
                    //OpenFileAndStart(of.FileName);
                    await StartAsync(of.FileNames);
                }
            }
        }

        /// <summary>
        /// �T�C�h�o�[��PicPanel�̈ʒu�֌W�𒲐�����
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
        /// �S�摜�Ǎ����W���u�X�^�b�N�ɐς�
        /// </summary>
        private void PreloadAllImages()
        {
            //ver1.54 2013�N5��7��
            for (int cnt = 0; cnt < App.g_pi.Items.Count; cnt++)
            {
                //�����_���̊O�ŕ�����쐬
                var msg = $"�摜���ǂݍ��ݒ�...{cnt + 1}/{App.g_pi.Items.Count}";

                //�T���l�C�����쐬���邾���Ȃ̂�await���������ɉ񂷁B
                AsyncIO.AddJobLow(cnt, () =>
                {
                    SetStatusbarInfo(msg);
                    //�ǂݍ��񂾂��̂�Purge�Ώۂɂ���
                    App.g_pi.FileCacheCleanUp2(App.Config.Advance.CacheSize);
                });
            }
            //�ǂݍ��݊������b�Z�[�W
            AsyncIO.AddJobLow(App.g_pi.Items.Count - 1, () => SetStatusbarInfo("���O�摜���ǂݍ��݊���"));
        }

        // ���[�e�B���e�B�n *************************************************************/

        #region Screen����

        private async Task SetDualViewModeAsync(bool isDual)
        {
            Debug.WriteLine(isDual, "SetDualViewMode()");
            ViewState.DualView = isDual;
            toolButtonDualMode.Checked = isDual;
            Menu_View2Page.Checked = isDual;

            //ver1.36 �X�N���[���L���b�V�����N���A
            //ClearScreenCache();
            ScreenCache.Clear();

            await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010�N6��20��
        }

        private void ToggleFullScreen()
        {
            SetFullScreen(!ViewState.FullScreen);
        }

        private void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                //�S��ʂɂ���
                ViewState.FullScreen = true;

                menuStrip1.Visible = false;
                toolStrip1.Visible = false;
                statusbar.Visible = false;
                //App.Config.visibleMenubar = false;

                //Z�I�[�_�[��ύX����
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

                //toolButtonFullScreen.Checked = true;			//������ɂ���Ă����Ȃ���
                this.WindowState = FormWindowState.Maximized;   //�����Ŕ�������Resize�C�x���g�ɊԂɍ���Ȃ�
            }
            else
            {
                //�S��ʂ���������
                ViewState.FullScreen = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;

                //�c�[���o�[�𕜌����� /ver0.17 2009�N3��8��
                //this.Controls.Add(toolStrip1);

                //Z�I�[�_�[�����ɖ߂�
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

            //���j���[�A�c�[���o�[�̍X�V
            Menu_ViewFullScreen.Checked = ViewState.FullScreen;
            Menu_ContextFullView.Checked = ViewState.FullScreen;
            toolButtonFullScreen.Checked = ViewState.FullScreen;

            AjustSidebarArrangement();
            UpdateStatusbar();

            //��ʕ\�����C��
            if (PicPanel.Visible)
                PicPanel.ResizeEnd();
            else if (_thumbPanel.Visible)
                _thumbPanel.ReDraw();
        }

        /// <summary>
        /// �N���C�A���g�T�C�Y�����߂�B�c�[���o�[��X�e�[�^�X�o�[�Ȃǂ����Z
        /// �c�[���o�[�̑傫���Ȃǂ��l�������N���C�A���g�̈��Ԃ�
        /// �␳�ΏہF
        ///   ���j���[�o�[, �X�N���[���o�[, �T�C�h�o�[, �c�[���o�[, �X�e�[�^�X�o�[
        /// </summary>
        /// <returns>�N���C�A���g�ʒu�A�T�C�Y��\��Rectangle</returns>
        public Rectangle GetClientRectangle()
        {
            var rect = this.ClientRectangle; // this.Bounds;

            //�c�[���o�[�̍���
            int toolbarHeight = (toolStrip1.Visible && !ViewState.FullScreen) ? toolStrip1.Height : 0;

            //���j���[�o�[�̍���
            int menubarHeight = (menuStrip1.Visible) ? menuStrip1.Height : 0;

            //�X�e�[�^�X�o�[�̍���
            int statusbarHeight = (statusbar.Visible && !ViewState.FullScreen) ? statusbar.Height : 0;

            //�c�[���o�[����̎�����Y����T��
            if (App.Config.IsToolbarTop)
                rect.Y += toolbarHeight;

            //�e�p�����[�^�̕␳
            rect.Y += menubarHeight;
            rect.Height -= (toolbarHeight + menubarHeight + statusbarHeight);

            //�}�C�i�X�ɂȂ�Ȃ��悤�ɕ␳ ver0.985a
            //��O�������Ȃ���
            if (rect.Width < 0) rect.Width = 0;
            if (rect.Height < 0) rect.Height = 0;

            return rect;
        }

        #endregion Screen����

        /// <summary>
        /// ���݂̃y�[�W���S�~���ɓ����B�폜��Ƀy�[�W�J�ڂ��s���B(ver1.35)
        /// </summary>
        private async Task RecycleBinNowPageAsync()
        {
            //�A�C�e�����Ȃɂ��Ȃ���΂Ȃɂ����Ȃ�
            if (App.g_pi.Items.Count == 0) return;

            //2�y�[�W���[�h�̎����Ȃɂ����Ȃ�
            if (g_viewPages == 2) return;

            //�A�[�J�C�u�ɑ΂��Ă��Ȃɂ����Ȃ�
            if (App.g_pi.PackType == PackageType.Archive) return;

            //���̃y�[�W�ԍ���ۑ�
            int now = App.g_pi.NowViewPage;
            string nowfile = App.g_pi.Items[now].Filename;

            //�y�[�W�J��
            int next = await GetNextPageIndexAsync(now);
            if (next != -1)
            {
                //���y�[�W������̂Ŏ��y�[�W��\��
                //Screen�L���b�V����L���Ɏg�����ߐ�Ƀy�[�W�ύX
                await SetViewPageAsync(next);

                //�폜���ꂽ���߃y�[�W�ԍ���߂�
                App.g_pi.NowViewPage = now;
            }
            else if (now > 0)
            {
                //�O�̃y�[�W�Ɉړ�
                next = now - 1;
                await SetViewPageAsync(next);
            }
            else
            {
                //next=0 : �Ō�̃y�[�W��������
                Debug.WriteLine("�Ō�̃y�[�W��������");
                PicPanel.Bmp = null;
                PicPanel.ResetZoomAndAlpha();
                PicPanel.Refresh();
            }

            //�S�~���֑���
            Uty.RecycleBin(nowfile);
            Debug.WriteLine(now.ToString() + "," + nowfile, "Delete");

            //pi����폜
            App.g_pi.Items.RemoveAt(now);

            //ScreenCache����폜
            ScreenCache.Clear();

            //Trackbar��ύX
            InitTrackbar();
        }

        #region �X���C�h�V���[

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
                    "�X���C�h�V���[���J�n���܂��B\r\n�}�E�X�N���b�N�܂��̓L�[���͂ŏI�����܂��B",
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
                //�X���C�h�V���[���I��������
                SlideShowTimer.Stop();
                _clearPanel.ShowAndClose("�X���C�h�V���[���I�����܂���", 1500);
            }
        }

        #endregion �X���C�h�V���[

        /// <summary>
        /// IPC�ŌĂ΂��C���^�[�t�F�[�X���\�b�h
        /// �R�}���h���C�������������Ă���̂ł�����N���B
        /// �������A���̃��\�b�h���Ă΂��Ƃ��̓t�H�[���̃X���b�h�ł͂Ȃ��̂�
        /// Invoke���K�v
        /// </summary>
        /// <param name="args">�R�}���h���C������</param>
        void IRemoteObject.IPCMessage(string[] args)
        {
            this.Invoke((Action)(() =>
            {
                //������O�ʂɂ���
                this.Activate();

                //�\���Ώۃt�@�C�����擾
                //1�߂Ɏ�����exe�t�@�C�����������Ă���̂ŏ���
                if (args.Length > 1)
                {
                    //AsyncStart(args.Skip(1).ToArray());
#pragma warning disable CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
                    StartAsync(args.Skip(1).ToArray());
#pragma warning restore CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
                }
            }));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //�t�H�[�J�X������������̃N���b�N���c�[���o�[�A�C�e���������Ă����Ȃ�
            //���̃c�[���o�[�����s����B
            if (_hoverStripItem is ToolStripItem cnt)
            {
                cnt.PerformClick();
            }
        }

        /// <summary>
        /// �E�B���h�E���ŏ�������B
        /// Toggle�ɂ����������ŏ����̂݁B
        /// </summary>
        private void ToggleFormSizeMinNormal()
        {
            this.WindowState = (this.WindowState == FormWindowState.Minimized)
                ? FormWindowState.Normal
                : FormWindowState.Minimized;
        }
    }
}
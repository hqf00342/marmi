#define SEVENZIP	//SevenZipSharp���g���Ƃ��͂�����`����B

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form, IRemoteObject
    {
        //�R���t�B�O�B�����P�������݁BApp.Config�Ɉړ�
        //�ǂݍ��݂� Program.cs�ōs���Ă���B
        //public static AppGlobalConfig g_onfig;

        //���݌��Ă���p�b�P�[�W���. App�N���X�ֈړ�
        //public static PackageInfo App.g_pi = null;

        //Form1�Q�Ɨp�n���h��
        public static Form1 _instance;

        //��ʕ\���֘A�F�����Ă���y�[�W���F�P���Q
        public int g_viewPages = 1;

        #region --- �R���g���[�� ---

        //���C�����
        public PicturePanel PicPanel = new PicturePanel();

        //���[�y
        private Loupe loupe = null;

        //TrackBar
        private ToolStripTrackBar g_trackbar;

        //�T���l�C���p�l���{��
        private ThumbnailPanel g_ThumbPanel = null;

        //�t�F�[�h����PictureBox
        private ClearPanel g_ClearPanel = null;

        //�T�C�h�o�[
        private SideBar g_Sidebar = null;

        //TrackBar�p�̃T���l�C���\���o�[
        private NaviBar3 g_trackNaviPanel = null;

        //�z�o�[���̃��j���[/�c�[���A�C�e���B��Focus�N���b�N�Ή�
        private object g_hoverStripItem = null;

        #endregion --- �R���g���[�� ---

        #region --- �f�[�^�N���X ---

        private readonly List<string> DeleteDirList = new List<string>();    //�폜���f�B���N�g��

        //ver1.35 �X�N���[���L���b�V��
        //private Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();

        #endregion --- �f�[�^�N���X ---

        //�񓯊��S�W�J�pSevenZipWrapper
        private SevenZipWrapper m_AsyncSevenZip = null;

        //�t���O��
        //�T���l�C������邩�B1000�ȏ゠�����Ƃ��̃t���O
        //private bool g_makeThumbnail = true;
        //�}�E�X�N���b�N���ꂽ�ʒu��ۑ��B�h���b�O����p
        private Point g_LastClickPoint = Point.Empty;

        //private static volatile ThreadStatus tsThumbnail = ThreadStatus.STOP;   //�X���b�h�̏󋵂�����

        //ver1.51 ���O��ScreenCache����邩�ǂ����̃t���O
        private bool needMakeScreenCache = false;

        //BeginInvoke�pDelegate
        private delegate void StatusbarRenew(string s);

        //ver1.35 �X���C�h�V���[�^�C�}�[
        private readonly System.Windows.Forms.Timer SlideShowTimer = new System.Windows.Forms.Timer();

        //�X���C�h�V���[�����ǂ���
        public bool IsSlideShow => SlideShowTimer.Enabled;

        // �R���X�g���N�^ *************************************************************/
        public Form1()
        {
            this.Name = App.APPNAME;
            _instance = this;

            //DpiAware 2021�N3��1��
            this.AutoScaleMode = AutoScaleMode.Dpi;
            //this.DpiChanged += Form1_DpiChanged;

            ////�ݒ�t�@�C���̓ǂݍ��݂�Program.cs�Ŏ��{
            ////App.Config = (AppGlobalConfig)LoadFromXmlFile();

            //�R���g���[����ǉ��B�c�[���X�g���b�v�͍Ō�ɒǉ�
            MyInitializeComponent();
            InitializeComponent();
            toolStrip1.Items.Add(g_trackbar);
            //
            // ver1.62 �c�[���o�[�̈ʒu
            //
            toolStrip1.Dock = App.Config.isToolbarTop ? DockStyle.Top : DockStyle.Bottom;

            //�����ݒ�
            this.KeyPreview = true;
            this.BackColor = App.Config.BackColor;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SetStyle(ControlStyles.Opaque, true);
            Application.Idle += Application_Idle;

            //z�I�[�_�[��������
            //SetFullScreen(false);
            //ver1.77 �t���X�N���[����Ԃ̕ۑ��ɑΉ�
            //if (App.Config.saveFullScreenMode)
            //	SetFullScreen(App.Config.isFullScreen);
            //else
            //	SetFullScreen(false);

            //�c�[���o�[�̕�����ݒ� ver1.67
            SetToolbarString();

            //�񓯊�IO�̊J�n
            AsyncIO.StartThread();
        }

        private void MyInitializeComponent()
        {
            //
            //�p�b�P�[�W��� PackageInfo
            //
            App.g_pi = new PackageInfo();
            //
            //PicturePanel
            //
            this.Controls.Add(PicPanel);
            PicPanel.Enabled = true;
            PicPanel.Visible = true;
            //PicPanel.Left = 0;	//ver1.62�R�����g�A�E�g
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
            g_Sidebar = new SideBar();
            this.Controls.Add(g_Sidebar);
            g_Sidebar.Visible = false;
            //g_Sidebar.Width = SIDEBAR_DEFAULT_WIDTH
            g_Sidebar.Width = App.Config.sidebarWidth;
            g_Sidebar.Dock = DockStyle.Left;
            g_Sidebar.SidebarSizeChanged += Sidebar_SidebarSizeChanged;
            //
            //TrackBar
            //
            g_trackbar = new ToolStripTrackBar();
            g_trackbar.Name = "MarmiTrackBar";
            g_trackbar.AutoSize = false;
            g_trackbar.Size = new System.Drawing.Size(300, 20);
            g_trackbar.ValueChanged += g_trackbar_ValueChanged;
            g_trackbar.MouseUp += g_trackbar_MouseUp;
            g_trackbar.MouseDown += g_trackbar_MouseDown;
            g_trackbar.MouseWheel += g_trackbar_MouseWheel;
            //g_trackbar.MouseEnter += new EventHandler(g_trackbar_MouseEnter);
            //
            //�T���l�C���p�l��
            //
            g_ThumbPanel = new ThumbnailPanel();
            this.Controls.Add(g_ThumbPanel);
            g_ThumbPanel.MouseMove += ThumbPanel_MouseMove;
            g_ThumbPanel.Init();
            g_ThumbPanel.Visible = false;
            g_ThumbPanel.Dock = DockStyle.Fill;
            //
            //ClearPanel
            //
            g_ClearPanel = new ClearPanel(PicPanel);

            //�z�o�[���̃��j���[/�c�[���A�C�e���B��Focus�N���b�N�Ή�
            g_hoverStripItem = null;

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

        private void Form1_Load(object sender, EventArgs e)
        {
            //�ݒ��Form�ɓK�p����
            ApplyConfigToWindow();

            //������
            InitControls();
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
                Start(a);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            //��\��
            this.Hide();

            //ver1.77 MRU���X�g�̍X�V
            UpdateMRUList();

            //�S��ʃ��[�h�̉��
            if (App.Config.isFullScreen)
            {
                SetFullScreen(false);
                //ver1.77 ���ɖ߂����ǃ��[�h�ۑ��͂�����
                App.Config.isFullScreen = true;
            }

            //�񓯊�IO�X���b�h�̏I��
            AsyncIO.StopThread();

            //�T���l�C�����[�h�̉��
            if (App.Config.isThumbnailView)
            {
                SetThumbnailView(false);
            }

            //7z�𓀂����Ă����璆�f
            m_AsyncSevenZip?.CancelExtractAll();

            //�X���b�h�����삵�Ă������~������.
            //�T���l�C���̕ۑ�
            //�t�@�C���n���h���̉��
            InitControls();

            //ver1.62�c�[���o�[�ʒu��ۑ�
            App.Config.isToolbarTop = (toolStrip1.Dock == DockStyle.Top);

            ////////////////////////////////////////ver1.10

            //ver1.10
            //�ݒ�̕ۑ�
            if (App.Config.isSaveConfig)
            {
                //�ݒ�t�@�C����ۑ�����
                App.Config.windowLocation = this.Location;
                App.Config.windowSize = this.Size;
                AppGlobalConfig.SaveToXmlFile(App.Config);
            }
            else
            {
                //�ݒ�t�@�C��������΍폜����
                //string configFile = AppGlobalConfig.getConfigFileName();
                string configFile = AppGlobalConfig.configFilename;
                if (File.Exists(configFile))
                    File.Delete(configFile);
            }

            //�Â��L���b�V���t�@�C�����̂Ă�
            if (App.Config.isAutoCleanOldCache)
                ClearOldCacheDBFile();

            //Application.Idle�̉��
            Application.Idle -= Application_Idle;

            //ver1.57 susie���
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

            //�h���b�v���ꂽ�����t�@�C�����ǂ����`�F�b�N
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Form���A�N�e�B�u
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
            if (g_ThumbPanel != null && App.Config.isThumbnailView)
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
            //Debug.WriteLine("OnResizeEnd()");

            if (g_ThumbPanel != null && App.Config.isThumbnailView)
            {
                //�T���l�C���\�����[�h��
            }
            else
            {
                //�ʏ���
                //ResizeEnd�ŃX�N���[���o�[��\��
                //UpdateFormScrollbar();

                //��ʂ�`�ʁB�������n�C�N�I���e�B��
                //PaintBG2(LastDrawMode.HighQuality);
                //this.Refresh();		//this.Invalidate();
                UpdateStatusbar();
                if (PicPanel.Visible)
                    PicPanel.ResizeEnd();
            }

            //�g���b�N�o�[�\���𒼂�
            ResizeTrackBar();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            //return;

            UpdateToolbar();

            //��i���`�ʂ������獂�i���ŏ�������
            if (PicPanel.LastDrawMode
                == System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor)
            {
                PicPanel.fastDraw = false;
                PicPanel.Refresh();
            }

            //ver1.24 �T�C�h�o�[
            //if (g_Sidebar.Visible)
            //{
            //    g_Sidebar.Invalidate();
            //}

            // �X�N���[���L���b�V�������E�p�[�W
            // ver1.38 Idle�Ŗ����炸��SetViewPage�̍Ō�ł��
            //getScreenCache();
            //ClearScreenCache();

            //�T���l�C�����[�h��Application_Idle()��
            //if (App.Config.isThumbnailView)
            //{
            //    g_ThumbPanel.Application_Idle();
            //}

            //ScreenCache�����K�v������΍쐬�B
            if (needMakeScreenCache)
            {
                needMakeScreenCache = false;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    MakeScreenCache();
                    PurgeScreenCache();
                    App.g_pi.FileCacheCleanUp2(App.Config.CacheSize);
                });
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

        #endregion

        // �񓯊��X�^�[�g ***************************************************************/
        /// <summary>
        /// �񓯊��^�C�v��Start()
        /// ���ۂ�ThreadPool����Start���Ăяo���Ă��邾���B
        /// D&D��Drag�����v���Z�X����������ɂȂ��Ă��܂��̂������B
        /// OnDragDrop()����̌Ăяo���݂̂�����g���B
        /// </summary>
        /// <param name="files"></param>
        private void AsyncStart(string[] files)
        {
            ThreadPool.QueueUserWorkItem(_ => this.Invoke((MethodInvoker)(() => Start(files))));
        }

        // �X�^�[�g *********************************************************************/
        private void Start(string[] filenames)
        {
            //ver1.73 MRU���X�g�̍X�V
            //���܂Ō��Ă������̂�o�^����
            UpdateMRUList();

            //�t�@�C�������łɊJ���Ă��邩�ǂ����`�F�b�N
            if (filenames.Length == 1 && filenames[0] == App.g_pi.PackageName)
            {
                const string text = "���Ȃ��t�H���_/�t�@�C�����J�����Ƃ��Ă��܂��B�J���܂����H";
                const string title = "����t�H���_/�t�@�C���I�[�v���̊m�F";
                if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //ver1.41 �񓯊�IO���~
            //App.stack.Clear();
            //App.stack.Push(new KeyValuePair<int, Delegate>(-1, null));
            AsyncIO.ClearJob();
            AsyncIO.AddJob(-1, null);
            App.g_pi.Initialize();
            SetStatusbarInfo("�������E�E�E" + filenames[0]);

            //ver1.35�X�N���[���L���b�V�����N���A
            App.ScreenCache.Clear();

            //�R���g���[���̏�����
            InitControls();     //���̎��_�ł�g_pi.PackageName�͂ł��Ă��Ȃ���MRU�������Ȃ��B

            //ver1.78�P��t�@�C���̏ꍇ�A���̃f�B���N�g����ΏۂƂ���
            string onePicFile = string.Empty;
            if (filenames.Length == 1 && Uty.IsPictureFilename(filenames[0]))
            {
                onePicFile = filenames[0];
                filenames[0] = Path.GetDirectoryName(filenames[0]);
            }

            //�t�@�C���ꗗ�𐶐�
            SevenZipWrapper.ClearPassword();    //���ɂ̃p�X���[�h���N���A
            bool needRecurse = SetPackageInfo(filenames);

            //ver1.37 �ċA�\�������łȂ�Solid���ɂ��W�J
            //ver1.79 ��Ɉꎞ���ɂɓW�J�I�v�V�����ɑΉ�
            //if (needRecurse )
            if (needRecurse || App.g_pi.isSolid || App.Config.AlwaysExtractArchive)
            {
                using (AsyncExtractForm ae = new AsyncExtractForm())
                {
                    SetStatusbarInfo("���ɂ�W�J���ł�" + filenames[0]);

                    //ver1.73 �ꎞ�t�H���_�쐬
                    try
                    {
                        var tempDir = SuggestTempDirName();
                        Directory.CreateDirectory(tempDir);
                        DeleteDirList.Add(tempDir);
                        App.g_pi.tempDirname = tempDir;
                    }
                    catch
                    {
                        //ver1.79 �ꎞ�t�H���_�����Ȃ��Ƃ��̑Ή�
                        MessageBox.Show("�ꎞ�W�J�t�H���_���쐬�ł��܂���ł����B�ݒ���m�F���Ă�������");
                        App.g_pi.Initialize();
                        return;
                    }

                    //�_�C�A���O��\��
                    ae.ArchivePath = filenames[0];
                    ae.ExtractDir = App.g_pi.tempDirname;
                    ae.ShowDialog(this);

                    //�_�C�A���O�̕\�����I��
                    //�f�B���N�g�������ׂ�g_pi�ɓǂݍ���
                    this.Cursor = Cursors.WaitCursor;
                    App.g_pi.PackType = PackageType.Pictures;
                    App.g_pi.Items.Clear();
                    GetDirPictureList(App.g_pi.tempDirname, true);
                    this.Cursor = Cursors.Arrow;
                }
            }
            SortPackage();
            //UI��������
            UpdateToolbar();

            //ver1.73 MRU���X�g�̍X�V
            //�����ł͂���.�ŏI�y�[�W��ۑ��ł��Ȃ��B
            //UpdateMRUList();

            //pdf�`�F�b�N
            if (App.g_pi.PackType == PackageType.Pdf)
            {
                if (!App.susie.isSupportedExtentions("pdf"))
                {
                    const string str = "pdf�t�@�C���̓T�|�[�g���Ă��܂���";
                    g_ClearPanel.ShowAndClose(str, 1000);
                    SetStatusbarInfo(str);
                    App.g_pi.Initialize();
                    return;
                }
            }

            if (App.g_pi.Items.Count == 0)
            {
                //��ʂ��N���A�A�������̕���������
                const string str = "�\���ł���t�@�C��������܂���ł���";
                g_ClearPanel.ShowAndClose(str, 1000);
                SetStatusbarInfo(str);
                return;
            }

            //�y�[�W��������
            App.g_pi.NowViewPage = 0;
            //CheckAndStart();

            //�T���l�C��DB������Γǂݍ���
            //loadThumbnailDBFile();
            if (App.Config.isContinueZipView)
            {
                //�ǂݍ��ݒl�𖳎����A�O�ɃZ�b�g
                //g_pi.NowViewPage = 0;
                foreach (var mru in App.Config.mru)
                {
                    if (mru == null)
                    {
                        continue;
                    }
                    else if (mru.Name == App.g_pi.PackageName
                        //ver1.79 �R�����g�A�E�g
                        //&& g_pi.packType == PackageType.Archive)
                        )
                    {
                        //�ŏI�y�[�W��ݒ肷��B
                        App.g_pi.NowViewPage = mru.LastViewPage;
                        //Bookmark��ݒ肷��
                        App.g_pi.LoadBookmarkString(mru.Bookmarks);
                        break;
                    }
                }
            }

            //�P�t�@�C���h���b�v�ɂ��f�B���N�g���Q�Ƃ̏ꍇ
            //�ŏ��Ɍ���y�[�W���h���b�v�����t�@�C���ɂ���B
            if (!string.IsNullOrEmpty(onePicFile))
            {
                int i = App.g_pi.Items.FindIndex(c => c.Filename == onePicFile);
                if (i < 0) i = 0;
                App.g_pi.NowViewPage = i;
            }

            //�g���b�N�o�[��������
            InitTrackbar();

            //SideBar�֓o�^
            g_Sidebar.Init(App.g_pi);

            //�^�C�g���o�[�̐ݒ�
            this.Text = $"{App.APPNAME} - {Path.GetFileName(App.g_pi.PackageName)}";

            //�T���l�C���̍쐬
            AsyncLoadImageInfo();

            //�摜��\��
            PicPanel.Message = string.Empty;
            SetViewPage(App.g_pi.NowViewPage);
        }

        /// <summary>
        /// �p�b�P�[�W����pi�Ɏ��
        /// </summary>
        /// <param name="files">�Ώۃt�@�C��</param>
        /// <returns>���ɓ����ɂ�����ꍇ��true</returns>
        private static bool SetPackageInfo(string[] files)
        {
            //������
            App.g_pi.Initialize();

            if (files.Length == 1)
            {
                //�h���b�v���ꂽ�̂�1��
                App.g_pi.PackageName = files[0];    //�f�B���N�g����Zip�t�@�C������z��

                //�h���b�v���ꂽ�t�@�C���̏ڍׂ�T��
                if (Directory.Exists(App.g_pi.PackageName))
                {
                    //�f�B���N�g���̏ꍇ
                    App.g_pi.PackType = PackageType.Directory;
                    GetDirPictureList(files[0], App.Config.isRecurseSearchDir);
                }
                else if (App.unrar.dllLoaded && files[0].EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                {
                    //
                    //unrar.dll���g���B
                    //
                    App.g_pi.PackType = PackageType.Archive;
                    App.g_pi.isSolid = true;

                    //�t�@�C�����X�g���\�z
                    App.unrar.Open(files[0], Unrar.OpenMode.List);
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

                    //�W�J���K�v�Ȃ̂�true��Ԃ�
                    return true;
                }
                else if (Uty.IsSupportArchiveFile(App.g_pi.PackageName))
                {
                    // ���Ƀt�@�C��
                    App.g_pi.PackType = PackageType.Archive;
                    bool needRecurse = GetArchivedFileInfo(files[0]);
                    //if (needRecurse)
                    //{
                    //    Uty.RecurseExtractAll(files[0], @"d:\temp");
                    //}
                    //MRUList���X�V
                    //UpdateMRUList();
                    if (needRecurse)
                        return true;
                }
                else if (files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    //pdf�t�@�C��
                    App.g_pi.PackType = PackageType.Pdf;
                    if (App.susie.isSupportPdf())
                    {
                        var list = App.susie.GetArchiveInfo(files[0]);
                        foreach (var e in list)
                        {
                            App.g_pi.Items.Add(new ImageInfo((int)e.position, e.filename, e.timestamp, e.filesize));
                        }
                        return false;
                    }
                    else
                    {
                        //pdf�͖��T�|�[�g
                        //g_pi.PackageName = string.Empty;
                        return false;
                    }
                }
                else
                {
                    //�P��摜�t�@�C��
                    App.g_pi.PackageName = string.Empty;    //zip�ł��f�B���N�g���ł��Ȃ�
                    App.g_pi.PackType = PackageType.Pictures;

                    //�P�����t�@�C����o�^
                    if (Uty.IsPictureFilename(files[0]))
                    {
                        FileInfo fi = new FileInfo(files[0]);
                        //g_pi.Items.Add(new ImageInfo(files[0], fi.CreationTime, fi.Length));
                        App.g_pi.Items.Add(new ImageInfo(0, files[0], fi.CreationTime, fi.Length));
                    }
                }
            }
            else //if (files.Length == 1)
            {
                //�����t�@�C��
                App.g_pi.PackageName = string.Empty;    //zip�ł��f�B���N�g���ł��Ȃ�
                                                        //g_pi.isZip = false;
                App.g_pi.PackType = PackageType.Pictures;

                //�t�@�C����ǉ�����
                int index = 0;
                foreach (string filename in files)
                {
                    if (Uty.IsPictureFilename(filename))
                    {
                        FileInfo fi = new FileInfo(filename);
                        App.g_pi.Items.Add(new ImageInfo(index++, filename, fi.CreationTime, fi.Length));
                    }
                }
            }//if (files.Length == 1)
            return false;
        }

        // ���j���[���� *****************************************************************/

        private void NavigateToBack()
        {
            //�O�ɖ߂�
            long drawOrderTick = DateTime.Now.Ticks;
            int prev = GetPrevPageIndex(App.g_pi.NowViewPage);
            if (prev >= 0)
                SetViewPage(prev, drawOrderTick);
            else
                g_ClearPanel.ShowAndClose("�擪�̃y�[�W�ł�", 1000);
        }

        private void NavigateToForword()
        {
            //ver1.35 ���[�v�@�\������
            long drawOrderTick = DateTime.Now.Ticks;
            int now = App.g_pi.NowViewPage;
            int next = GetNextPageIndex(App.g_pi.NowViewPage);
            Debug.WriteLine($"NavigateToForword() {now} -> {next}");
            if (next >= 0)
            {
                SetViewPage(next, drawOrderTick);
            }
            else if (App.Config.lastPage_toTop)
            {
                //�擪�y�[�W�փ��[�v
                SetViewPage(0, drawOrderTick);
                g_ClearPanel.ShowAndClose("�擪�y�[�W�ɖ߂�܂���", 1000);
            }
            else if (App.Config.lastPage_toNextArchive)
            {
                //ver1.70 �ŏI�y�[�W�Ŏ��̏��ɂ��J��
                if (App.g_pi.PackType != PackageType.Directory)
                {
                    //���̏��ɂ�T��
                    string filename = App.g_pi.PackageName;
                    string dirname = Path.GetDirectoryName(filename);
                    string[] files = Directory.GetFiles(dirname);

                    //�t�@�C�����Ń\�[�g����
                    //Array.Sort(files, Uty.Compare_unsafeFast);
                    Array.Sort(files, NaturalStringComparer.CompareS);

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
                            if (Uty.IsAvailableFile(s))
                            {
                                g_ClearPanel.ShowAndClose("���ֈړ����܂��F" + Path.GetFileName(s), 1000);
                                Start(new string[] { s });
                                return;
                            }
                        }
                    }
                    g_ClearPanel.ShowAndClose("�Ō�̃y�[�W�ł��B���̏��ɂ�������܂���ł���", 1000);
                }
                else
                {
                    //�擪�y�[�W�փ��[�v
                    SetViewPage(0, drawOrderTick);
                    g_ClearPanel.ShowAndClose("�擪�y�[�W�ɖ߂�܂���", 1000);
                }
            }
            else //if(App.Config.lastPage_stay)
            {
                g_ClearPanel.ShowAndClose("�Ō�̃y�[�W�ł�", 1000);
            }
        }

        public void SetThumbnailView(bool isShow)
        {
            if (isShow)
            {
                //�\����������
                App.Config.isThumbnailView = true;
                //Rectangle rect = GetClientRectangle();
                //g_ThumbPanel.Location = rect.Location;
                //g_ThumbPanel.Size = rect.Size;					//�����OnSize()���Ă΂��͂��Ȃ񂾂��ǁB

                //SideBar������ꍇ�͏���
                if (g_Sidebar.Visible)
                    g_Sidebar.Visible = false;

                //�g���b�N�o�[��Disable Ver0.975
                g_trackbar.Enabled = false;

                //PicPanel���\����
                PicPanel.Visible = false;
                PicPanel.Dock = DockStyle.None;

                //�\������
                if (!this.Controls.Contains(g_ThumbPanel))
                    this.Controls.Add(g_ThumbPanel);
                if (!App.Config.isFullScreen)
                    g_ThumbPanel.BringToFront();        //ver1.83 �őO�ʂɂȂ�悤�ɂ���B�c�[���o�[�΍�
                g_ThumbPanel.Dock = DockStyle.Fill; //ver1.64
                g_ThumbPanel.Visible = true;
                toolButtonThumbnail.Checked = true;
                g_ThumbPanel.ReDraw();
            }
            else
            {
                //�\������߂�
                App.Config.isThumbnailView = false;
                //this.Controls.Remove(g_ThumbPanel);
                g_ThumbPanel.Visible = false;
                g_ThumbPanel.Dock = DockStyle.None; //ver1.64
                toolButtonThumbnail.Checked = false;

                //PicPanel��\��
                PicPanel.Dock = DockStyle.Fill;
                PicPanel.Visible = true;

                //ver0.91 ������ƍĕ`�ʂ���
                //���T�C�Y����Ă���\��������̂ōĕ`��
                //Form1_ResizeEnd(null, null);	//������g_bg��Re-Allocate()���s����B
                //OnResizeEnd(null);
                //PaintBG2(LastDrawMode.HighQuality);
                //this.Refresh();		//this.Invalidate();
                UpdateStatusbar();

                //NaviBar��߂�
                if (App.Config.visibleNavibar)
                    g_Sidebar.Visible = true;

                //�g���b�N�o�[��߂� Ver0.975
                g_trackbar.Enabled = true;
            }
        }

        private void OpenDialog()
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
                    Start(of.FileNames);
                }
            }
        }

        private void ToggleFullScreen()
        {
            SetFullScreen(!App.Config.isFullScreen);
        }

        private void SetFullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                //�S��ʂɂ���
                App.Config.isFullScreen = true;

                menuStrip1.Visible = false;
                toolStrip1.Visible = false;
                statusbar.Visible = false;
                //App.Config.visibleMenubar = false;

                //Z�I�[�_�[��ύX����
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

                //toolButtonFullScreen.Checked = true;			//������ɂ���Ă����Ȃ���
                this.WindowState = FormWindowState.Maximized;   //�����Ŕ�������Resize�C�x���g�ɊԂɍ���Ȃ�
            }
            else
            {
                //�S��ʂ���������
                App.Config.isFullScreen = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;

                //�c�[���o�[�𕜌����� /ver0.17 2009�N3��8��
                //this.Controls.Add(toolStrip1);

                //Z�I�[�_�[�����ɖ߂�
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
                toolStrip1.Visible = App.Config.visibleToolBar;
                menuStrip1.Visible = App.Config.visibleMenubar;
                statusbar.Visible = App.Config.visibleStatusBar;
            }

            //���j���[�A�c�[���o�[�̍X�V
            Menu_ViewFullScreen.Checked = App.Config.isFullScreen;
            Menu_ContextFullView.Checked = App.Config.isFullScreen;
            toolButtonFullScreen.Checked = App.Config.isFullScreen;

            AjustSidebarArrangement();
            UpdateStatusbar();

            //��ʕ\�����C��
            if (PicPanel.Visible)
                PicPanel.ResizeEnd();
            else if (g_ThumbPanel.Visible)
                g_ThumbPanel.ReDraw();
        }

        /// <summary>
        /// �T�C�h�o�[��PicPanel�̈ʒu�֌W�𒲐�����
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
            App.Config.dualView = isDual;
            toolButtonDualMode.Checked = isDual;
            Menu_View2Page.Checked = isDual;

            //ver1.36 �X�N���[���L���b�V�����N���A
            //ClearScreenCache();
            App.ScreenCache.Clear();

            SetViewPage(App.g_pi.NowViewPage);  //ver0.988 2010�N6��20��
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
        /// �ꎞ�t�H���_�̖��O��Ԃ��B
        /// ���Ȃ��ꍇ��null���Ԃ�B
        /// </summary>
        /// <returns>�ꎞ�t�H���_�̃t���p�X�B���Ȃ��ꍇ��null</returns>
        private static string SuggestTempDirName()
        {
            //���݂��Ȃ������_���ȃt�H���_�������
            string tempDir;

            //temp�t�H���_�̃��[�g�ƂȂ�t�H���_�����߂�B
            string rootPath = App.Config.tmpFolder;
            if (string.IsNullOrEmpty(rootPath))
                rootPath = Application.StartupPath; //�A�v���̃p�X
                                                    //Path.GetTempPath(),		//windows�W����TempDir

            //���j�[�N�ȃt�H���_��T��
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
            //ver1.54 2013�N5��7��
            for (int cnt = 0; cnt < App.g_pi.Items.Count; cnt++)
            {
                //�����_���̊O�ŕ�����쐬
                string s = string.Format("�摜���ǂݍ��ݒ�...{0}/{1}", cnt + 1, App.g_pi.Items.Count);

                //�X�^�b�N�ɓ����
                AsyncIO.AddJobLow(-1, () =>
                {
                    SetStatusbarInfo(s);
                    //�ǂݍ��񂾂��̂�Purge�Ώۂɂ���
                    App.g_pi.FileCacheCleanUp2(App.Config.CacheSize);
                });
            }
            //�ǂݍ��݊������b�Z�[�W
            AsyncIO.AddJobLow(App.g_pi.Items.Count - 1, () => SetStatusbarInfo("���O�摜���ǂݍ��݊���"));
        }

        private void InitTrackbar()
        {
            g_trackbar.Minimum = 0;
            if (App.g_pi.Items.Count > 0)
            {
                g_trackbar.Maximum = App.g_pi.Items.Count - 1;
                g_trackbar.Enabled = true;
                g_trackbar.Value = App.g_pi.NowViewPage;
            }
            else
            {
                g_trackbar.Maximum = 0;
                g_trackbar.Value = 0;
                g_trackbar.Enabled = false;
            }
        }

        /// <summary>
        /// D&D��A�v���N�����ɌĂ΂�鏉�������[�`��
        /// �X���b�h���~�߁A���ׂĂ̏�Ԃ�����������B
        /// �ǂݍ��ݑΏۂ̃t�@�C���ɂ��Ă͈�؉������Ȃ��B
        /// Form1_Load(), Form1_FormClosed(), OpenFileAndStart(), Form1_DragDrop()
        /// </summary>
        private void InitControls()
        {
            //�T���l�C�����[�h�̉��
            if (App.Config.isThumbnailView)
                SetThumbnailView(false);

            //2011/08/19 �T���l�C��������
            g_ThumbPanel.Init();

            //2011�N11��11�� ver1.24 �T�C�h�o�[
            //g_Sidebar.Init();
            g_Sidebar.Init(null);   //ver1.37
            if (g_Sidebar.Visible)
                g_Sidebar.Invalidate();

            //ver1.25 trackNavi
            if (g_trackNaviPanel != null)
            {
                g_trackNaviPanel.Dispose();
                g_trackNaviPanel = null;
            }

            //2011/08/19 trackbar��������
            //InitTrackBar();
            //nullRefer�̂��߉������Ȃ����Ƃɂ���
            //g_trackbar.Initialize();

            //MRU���X�V���� ver1.73 �R�����g�A�E�g
            //UpdateMRUList();

            //�T���l�C�����o�b�N�O���E���h�ŕۑ�����
            //saveDBFile();
            App.g_pi.Initialize();

            //�p�b�P�[�W����������
            //�Â��̂̓X���b�h���Ŏ̂Ă�̂ŐV�����̂����
            //g_pi = new PackageInfo();

            //7z�𓀂����Ă����璆�f
            m_AsyncSevenZip?.CancelExtractAll();
            //temp�t�H���_������΍폜
            //if (!string.IsNullOrEmpty(g_pi.tempDirname))
            //{
            //    DeleteTempDir(g_pi.tempDirname);
            //    setStatusbarInfo("�ꎞ�t�H���_���폜���܂��� - " + g_pi.tempDirname);
            //}
            foreach (string dir in DeleteDirList)
            {
                Uty.DeleteTempDir(dir);
            }
            DeleteDirList.Clear();

            //2011/08/19 Bitmap�L���b�V��
            //g_FileCache.Clear();

            //2012/09/04 �񓯊�IO�𒆎~
            //App.stack.Clear();
            //App.stack.Push(new KeyValuePair<int, Delegate>(-1, null));
            AsyncIO.ClearJob();
            AsyncIO.AddJob(-1, null);

            //���̂ق��{�̓��̏����N���A
            g_viewPages = 1;
            //g_lastDrawMode = LastDrawMode.HighQuality;	//Idle�ŗ]�v�Ȃ��Ƃ������Ȃ�
            g_LastClickPoint = Point.Empty;
            //if (g_originalSizeBitmap != null)
            //{
            //    PicPanel.bmp = null;
            //    g_originalSizeBitmap.Dispose();	//����ς݂��������O���������邩��
            //    g_originalSizeBitmap = null;
            //}

            //�摜�\������߂�
            PicPanel.Message = string.Empty;
            PicPanel.bmp = null;

            //GC: 2021�N2��26�� �O�̏��ɂ̃K�x�[�W���������߂����ł���Ă����B
            Uty.ForceGC();
        }

        private void SortPackage()
        {
            //�t�@�C�����X�g����ёւ���
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
                App.g_pi.Items.Sort(comparer);
            }
            return;
        }

        /// <summary>
        /// ���ɏ����擾
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>���ɓ����ɂ�����ꍇ��trye</returns>
        private static bool GetArchivedFileInfo(string filename)
        {
            var szw = new SevenZipWrapper();
            bool retval = false;

            if (!szw.Open(filename))
            {
                MessageBox.Show("�G���[�̂��ߏ��ɂ͊J���܂���ł����B");
                App.g_pi.Initialize();
                //2021�N2��26�� GC����߂�
                //Uty.ForceGC();      //���ɂ��J���EGC����K�v������
                return false;
            }

            //Zip�t�@�C������ݒ�
            App.g_pi.PackageName = filename;
            var fi = new FileInfo(App.g_pi.PackageName);
            App.g_pi.PackageSize = fi.Length;
            App.g_pi.isSolid = szw.IsSolid;

            //ver1.31 7z�t�@�C���Ȃ̂Ƀ\���b�h����Ȃ����Ƃ�����I�H
            if (Path.GetExtension(filename) == ".7z")
                App.g_pi.isSolid = true;

            //g_pi.isZip = true;
            App.g_pi.PackType = PackageType.Archive;

            //�t�@�C�������X�g�ɒǉ�
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
        /// ���݉{�����Ă���g_pi.PackageName��MRU�ɒǉ�����
        /// �ȑO���������Ƃ�����ꍇ�A�{�����t�������X�V
        /// </summary>
        private void UpdateMRUList()
        {
            //�Ȃɂ�������Βǉ����Ȃ�
            if (string.IsNullOrEmpty(App.g_pi.PackageName))
                return;

            //�f�B���N�g���ł��ǉ����Ȃ�
            if (App.g_pi.PackType == PackageType.Directory)
                return;

            //MRU�ɒǉ�����K�v�����邩�m�F
            bool needMruAdd = true;
            for (int i = 0; i < App.Config.mru.Count; i++)
            {
                if (App.Config.mru[i] == null)
                    continue;
                if (App.Config.mru[i].Name == App.g_pi.PackageName)
                {
                    //�o�^�ς݂�MRU���X�V
                    //���t�����X�V
                    App.Config.mru[i].Date = DateTime.Now;
                    //�Ō�Ɍ����y�[�W���X�V v1.37
                    App.Config.mru[i].LastViewPage = App.g_pi.NowViewPage;
                    needMruAdd = false;

                    //ver1.77 Bookmark���ݒ�
                    App.Config.mru[i].Bookmarks = App.g_pi.CreateBookmarkString();
                }
            }
            if (needMruAdd)
            {
                //MRU��V�����o�^
                //�Â����ɕ��ׂ遨�擪�ɒǉ�
                //Array.Sort(App.Config.mru);
                //App.Config.mru[0] = new MRU(App.g_pi.PackageName, DateTime.Now, App.g_pi.NowViewPage, App.g_pi.GetCsvFromBookmark());
                App.Config.mru.Add(new MRU(App.g_pi.PackageName, DateTime.Now, App.g_pi.NowViewPage, App.g_pi.CreateBookmarkString()));
            }
            //Array.Sort(App.Config.mru);   //���ג���
        }

        //*****************************************************************
        // ��ʑJ��

        /// <summary>
        /// �w�肵���C���f�b�N�X�̉摜��\������B
        /// public�ɂȂ��Ă��闝�R�̓T�C�h�o�[��T���l�C����ʂ����
        /// �Ăяo���ɑΉ����邽�߁B
        /// �O�̃y�[�W�ɖ߂�Ȃ��悤��drawOrderTick�͌��ݎ�������Ŏw�肵�Ă���B
        /// </summary>
        /// <param name="index">�ړ��������y�[�W�C���f�b�N�X�ԍ�</param>
        public void SetViewPage(int index)
        {
            SetViewPage(index, DateTime.Now.Ticks);
        }

        /// <summary>
        /// �w�肵���C���f�b�N�X�̉摜��\������B
        /// public�ɂȂ��Ă��闝�R�̓T�C�h�o�[��T���l�C����ʂ����
        /// �Ăяo���ɑΉ����邽�߁B
        /// �O�̃y�[�W�ɖ߂�Ȃ��悤��drawOrderTick�𓱓�
        /// </summary>
        /// <param name="index">�C���f�b�N�X�ԍ�</param>
        /// <param name="drawOrderTick">�`�ʏ����������I�[�_�[���� = DateTime.Now.Ticks</param>
        public void SetViewPage(int index, long drawOrderTick)
        {
            //ver1.09 �I�v�V�����_�C�A���O�����ƕK�������ɗ��邱�Ƃɑ΂���`�F�b�N
            if (App.g_pi.Items == null || App.g_pi.Items.Count == 0)
                return;

            //ver1.36 Index�͈̓`�F�b�N
            Debug.Assert(index >= 0 && index < App.g_pi.Items.Count);

            // �y�[�W�i�s���� �i�ޕ����Ȃ琳�A�߂�����Ȃ畉
            // �A�j���[�V�����ŗ��p����
            int pageDirection = index - App.g_pi.NowViewPage;

            //�y�[�W�ԍ����X�V
            App.g_pi.NowViewPage = index;
            g_trackbar.Value = index;

            //ver1.35 �X�N���[���L���b�V���`�F�b�N
            if (App.ScreenCache.TryGetValue(index, out Bitmap screenImage))
            {
                //�X�N���[���L���b�V���������̂ł����ɕ`��
                SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
                Debug.WriteLine(index, "Use ScreenCache");
            }
            else
            {
                //ver1.50
                //Key��������{key,null}�L���b�V��������������B�H�ɔ������邽��
                if (App.ScreenCache.ContainsKey(index))
                    App.ScreenCache.Remove(index);

                //ver1.50 �ǂݍ��ݒ��ƕ\��
                SetStatusbarInfo("Now Loading ... " + (index + 1).ToString());

                //�摜�쐬���X���b�h�v�[���ɓo�^
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var img = Bmp.MakeOriginalSizeImage(index);
                    this.Invoke((MethodInvoker)(() =>
                    {
                        if (img == null)
                        {
                            PicPanel.Message = "�Ǎ��݂Ɏ��Ԃ��������Ă܂�.�����[�h���Ă�������";
                        }
                        else
                        {
                            SetViewPage2(index, pageDirection, img, drawOrderTick);
                        }
                    }));
                });

                //�J�[�\����Wait��
                this.Cursor = Cursors.WaitCursor;
            }
        }

        private void SetViewPage2(int index, int pageDirection, Bitmap screenImage, long orderTime)
        {
            //ver1.55 drawOrderTick�̃`�F�b�N.
            // �X���b�h�v�[���ɓ��邽�ߋH�ɏ������O�シ��B
            // �ŐV�̕`�ʂłȂ���΃X�L�b�v
            if (PicPanel.drawOrderTime > orderTime)
            {
                Debug.WriteLine($"Skip SetViewPage2({index}) too old order={orderTime} < now={PicPanel.drawOrderTime}");
                return;
            }

            //�`�ʊJ�n
            PicPanel.State = PicturePanel.DrawStatus.drawing;
            PicPanel.drawOrderTime = orderTime;

            if (screenImage == null)
            {
                Debug.WriteLine($"bmp��null(index={index})");
                PicPanel.State = PicturePanel.DrawStatus.idle;
                PicPanel.Message = "�\���G���[ �ēx�\�����Ă݂Ă�������" + index.ToString();
                PicPanel.Refresh();
                return;
            }

            //ver1.50 �\��
            PicPanel.State = PicturePanel.DrawStatus.drawing;
            PicPanel.Message = string.Empty;
            if (App.Config.pictureSwitchMode != AnimateMode.none  //�A�j���[�V�������[�h�ł���
                && !App.Config.keepMagnification                  //�{���Œ胂�[�h�ł̓A�j���[�V�������Ȃ�
                && pageDirection != 0)
            {
                //�X���C�h�C���A�j���[�V����
                PicPanel.AnimateSlideIn(screenImage, pageDirection);
            }

            PicPanel.bmp = screenImage;
            PicPanel.ResetView();
            PicPanel.fastDraw = false;

            //ver1.78 �{�����I�v�V�����w��ł���悤�ɕύX
            if (!App.Config.keepMagnification     //�{���ێ����[�h�ł͂Ȃ�
                || IsFitToScreen())             //��ʂɃt�B�b�g���Ă���
            {
                //��ʐ؂�ւ�莞�̓t�B�b�g���[�h�ŋN��
                float r = PicPanel.FittingRatio;
                if (r > 1.0f && App.Config.noEnlargeOver100p)
                    r = 1.0f;
                PicPanel.ZoomRatio = r;
            }

            //�y�[�W��`��
            PicPanel.AjustViewAndShow();

            //1�y�[�W�\����2�y�[�W�\����
            //viewPages = CanDualView(index) ? 2 : 1;
            g_viewPages = (int)screenImage.Tag;
            PicPanel.State = PicturePanel.DrawStatus.idle;

            //�J�[�\�������ɖ߂�
            this.Cursor = Cursors.Default;

            //UI�X�V
            UpdateStatusbar();
            UpdateToolbar();

            //�T�C�h�o�[�ŃA�C�e���𒆐S��
            if (g_Sidebar.Visible)
                g_Sidebar.SetItemToCenter(App.g_pi.NowViewPage);

            needMakeScreenCache = true;

            //PicPanel.Message = string.Empty;
            PicPanel.State = PicturePanel.DrawStatus.idle;
            //2021�N2��26�� GC����߂�
            //ToDo:���������͂������ق���������������Ȃ���LOH�̈����������ɂ��ׂ�
            //Uty.ForceGC();
        }

        //ver1.35 �O�̃y�[�W�ԍ��B���łɐ擪�y�[�W�Ȃ�-1
        private static int GetPrevPageIndex(int index)
        {
            if (index > 0)
            {
                int declimentPages = -1;
                //2�y�[�W���炷���Ƃ��o���邩
                if (CanDualView(App.g_pi.NowViewPage - 2))
                    declimentPages = -2;

                int ret = index + declimentPages;
                return ret >= 0 ? ret : 0;
            }
            else
            {
                //���łɐ擪�y�[�W�Ȃ̂�-1��Ԃ�
                return -1;
            }
        }

        //ver1.36���̃y�[�W�ԍ��B���łɍŏI�y�[�W�Ȃ�-1
        private static int GetNextPageIndex(int index)
        {
            int pages = CanDualView(index) ? 2 : 1;

            if (index + pages <= App.g_pi.Items.Count - 1)
            {
                return index + pages;
            }
            else
            {
                //�ŏI�y�[�W
                return -1;
            }
        }

        // ���[�e�B���e�B�n *************************************************************/
        private void UpdateToolbar()
        {
            //��ʃ��[�h�̏�Ԕ��f
            toolButtonDualMode.Checked = App.Config.dualView;
            toolButtonFullScreen.Checked = App.Config.isFullScreen;
            toolButtonThumbnail.Checked = App.Config.isThumbnailView;

            //Sidebar
            toolStripButton_Sidebar.Checked = g_Sidebar.Visible;

            if (App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //�t�@�C�����{�����Ă��Ȃ��ꍇ�̃c�[���o�[
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
                //�T���l�C���{�^��
                toolButtonThumbnail.Enabled = true;
                //if(g_makeThumbnail)
                //    toolButtonThumbnail.Enabled = true;
                //else
                //    toolButtonThumbnail.Enabled = false;

                if (App.Config.isThumbnailView)
                {
                    //�T���l�C���\����
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
                    //�ʏ�\����
                    toolStripButton_ZoomIn.Enabled = true;
                    toolStripButton_ZoomOut.Enabled = true;
                    toolStripButton_Zoom100.Enabled = true;
                    toolStripButton_ZoomFit.Enabled = true;
                    toolStripButton_Favorite.Enabled = true;
                    toolStripButton_Sidebar.Enabled = true;
                    toolStripButton_Rotate.Enabled = true;

                    //���E�{�^���̗L������
                    if (App.Config.isReplaceArrowButton)
                    {
                        //����ւ�
                        toolButtonLeft.Enabled = !IsLastPageViewing();      //�ŏI�y�[�W�`�F�b�N
                        toolButtonRight.Enabled = (bool)(App.g_pi.NowViewPage != 0);    //�擪�y�[�W�`�F�b�N
                    }
                    else
                    {
                        toolButtonLeft.Enabled = (bool)(App.g_pi.NowViewPage != 0); //�擪�y�[�W�`�F�b�N
                        toolButtonRight.Enabled = !IsLastPageViewing();     //�ŏI�y�[�W�`�F�b�N
                    }

                    //100%�Y�[��
                    toolStripButton_Zoom100.Checked = IsScreen100p();

                    //��ʃt�B�b�g�Y�[��
                    toolStripButton_ZoomFit.Checked = IsFitToScreen();

                    //Favorite
                    if (App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark)
                    {
                        toolStripButton_Favorite.Checked = true;
                    }
                    else if (g_viewPages == 2
                        && App.g_pi.NowViewPage < App.g_pi.Items.Count - 1      //ver1.69 �ŏI�y�[�W���O�`�F�b�N
                        && App.g_pi.Items[App.g_pi.NowViewPage + 1].IsBookMark) //
                    {
                        toolStripButton_Favorite.Checked = true;
                    }
                    else
                    {
                        toolStripButton_Favorite.Checked = false;
                    }

                    //Sidebar
                    toolStripButton_Sidebar.Checked = g_Sidebar.Visible;
                }

                //TrackBar
                //�����Œ�����UI���x���Ȃ�B
                //g_trackbar.Value = g_pi.NowViewPage;
            }
        }

        /// <summary>�\�����̉摜����ʂ����ς��Ƀt�B�b�g���Ă��邩�ǂ���</summary>
        private bool IsFitToScreen()
        {
            return Math.Abs(PicPanel.ZoomRatio - PicPanel.FittingRatio) < 0.001f;
        }

        /// <summary>���݂̕\�����������ǂ���</summary>
        private bool IsScreen100p()
        {
            return Math.Abs(PicPanel.ZoomRatio - 1.0f) < 0.001f;
        }

        /// <summary>
        /// MRU���X�g���X�V����B���ۂɃ��j���[�̒��g���X�V
        /// ���̊֐����Ăяo���Ă���̂�Menu_File_DropDownOpening�̂�
        /// </summary>
        private void UpdateMruMenuListUI()
        {
            MenuItem_FileRecent.DropDownItems.Clear();

            //Array.Sort(App.Config.mru);
            App.Config.mru = App.Config.mru.OrderBy(a => a.Date).ToList();

            int menuCount = 0;

            //�V�������ɂ���
            for (int i = App.Config.mru.Count - 1; i >= 0; i--)
            {
                if (App.Config.mru[i] == null)
                    continue;

                MenuItem_FileRecent.DropDownItems.Add(App.Config.mru[i].Name, null, new EventHandler(OnClickMRUMenu));

                //ver1.73 MRU�\�����̐���
                if (++menuCount >= App.Config.numberOfMru)
                    break;
            }
        }

        /// <summary>
        /// �f�B���N�g���̉摜�����X�g�ɒǉ�����B�ċA�I�ɌĂяo�����߂Ɋ֐���
        /// </summary>
        /// <param name="dirName">�ǉ��Ώۂ̃f�B���N�g����</param>
        /// <param name="isRecurse">�ċA�I�ɑ�������ꍇ��true</param>
        private static void GetDirPictureList(string dirName, bool isRecurse)
        {
            //�摜�t�@�C��������ǉ�����
            int index = 0;
            foreach (string name in Directory.EnumerateFiles(dirName))
            {
                if (Uty.IsPictureFilename(name))
                {
                    var fi = new FileInfo(name);
                    App.g_pi.Items.Add(new ImageInfo(index++, name, fi.CreationTime, fi.Length));
                }
            }

            //�ċA�I�Ɏ擾���邩�ǂ����B
            if (isRecurse)
            {
                foreach (var name in Directory.GetDirectories(dirName))
                    GetDirPictureList(name, isRecurse);
            }
        }

        /// <summary>
        /// �ŏI�y�[�W�����Ă��邩�ǂ����m�F�B�Q�y�[�W�\���ɑΉ�
        /// �擪�y�[�W�͂��̂܂܂O���ǂ����`�F�b�N���邾���Ȃ̂ō쐬���Ȃ��B
        /// </summary>
        /// <returns>�ŏI�y�[�W�ł����true</returns>
        private bool IsLastPageViewing()
        {
            if (string.IsNullOrEmpty(App.g_pi.PackageName))
                return false;
            if (App.g_pi.Items.Count <= 1)
                return false;
            return (bool)(App.g_pi.NowViewPage + g_viewPages >= App.g_pi.Items.Count);
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
            int toolbarHeight = (toolStrip1.Visible && !App.Config.isFullScreen) ? toolStrip1.Height : 0;

            //���j���[�o�[�̍���
            int menubarHeight = (menuStrip1.Visible) ? menuStrip1.Height : 0;

            //�X�e�[�^�X�o�[�̍���
            int statusbarHeight = (statusbar.Visible && !App.Config.isFullScreen) ? statusbar.Height : 0;

            //�c�[���o�[����̎�����Y����T��
            if (App.Config.isToolbarTop)
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

        /// <summary>
        /// ver1.67 �c�[���o�[�̕�����\��/��\������B
        /// </summary>
        private void SetToolbarString()
        {
            if (App.Config.eraseToolbarItemString)
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

        // �T�C�h�o�[�֘A ***************************************************************/
        /// <summary>
        /// �T�C�h�o�[�̃T�C�Y���ύX���ꂽ�i����I������j�Ƃ��ɌĂяo�����B
        /// ���ꂪ�Ăяo�����Ƃ��̓T�C�h�o�[�Œ�̎��̂݁B
        /// 2010�N6��6�� ver0.985�Ŏ���
        /// </summary>
        /// <param name="sender">���p����</param>
        /// <param name="e">���p����</param>
        private void Sidebar_SidebarSizeChanged(object sender, EventArgs e)
        {
            OnResizeEnd(null);
        }

        // �摜���� *********************************************************************/

        /// <summary>
        /// �w�肳�ꂽ�C���f�b�N�X����Q���\���ł��邩�`�F�b�N
        /// �`�F�b�N��ImageInfo�Ɏ�荞�܂ꂽ�l�𗘗p�A�c����Ŋm�F����B
        /// </summary>
        /// <param name="index">�C���f�b�N�X�l</param>
        /// <returns>2��ʕ\���ł���Ƃ���true</returns>
        public static bool CanDualView(int index)
        {
            //�Ō�̃y�[�W�ɂȂ��Ă��Ȃ����m�F
            if (index >= App.g_pi.Items.Count - 1 || index < 0)
                return false;

            //�R���t�B�O�������m�F
            if (!App.Config.dualView)
                return false;

            //ver1.79����Ȃ���2�y�[�W�\��
            if (App.Config.dualView_Force)
                return true;

            //1���ڃ`�F�b�N
            if (!App.g_pi.Items[index].HasInfo)
                Bmp.SyncGetBitmapSize(index);
            if (App.g_pi.Items[index].IsFat)
                return false; //����������

            //�Q���ڃ`�F�b�N
            if (!App.g_pi.Items[index + 1].HasInfo)
                Bmp.SyncGetBitmapSize(index + 1);
            if (App.g_pi.Items[index + 1].IsFat)
                return false; //����������

            //�S�ďc�����������̏���
            if (App.Config.dualView_Normal)
                return true; //�c�摜2��

            //2�摜�̍������قƂ�Ǖς��Ȃ����true
            const int ACCEPTABLE_RANGE = 200;
            return Math.Abs(App.g_pi.Items[index].Height - App.g_pi.Items[index + 1].Height) < ACCEPTABLE_RANGE;
        }

        #region �X�N���[���L���b�V��

        /// <summary>
        /// �O��y�[�W�̉�ʃL���b�V�����쐬����
        /// ���݌��Ă���y�[�W�𒆐S�Ƃ���
        /// </summary>
        private static void MakeScreenCache()
        {
            //ver1.37 �X���b�h�Ŏg�����Ƃ�O��Ƀ��b�N
            lock ((App.ScreenCache as ICollection).SyncRoot)
            {
                //�O�̃y�[�W
                int ix = GetPrevPageIndex(App.g_pi.NowViewPage);
                if (ix >= 0 && !App.ScreenCache.ContainsKey(ix))
                {
                    Debug.WriteLine(ix, "getScreenCache() Add Prev");
                    var bmp = Bmp.MakeOriginalSizeImage(ix);
                    if (bmp != null)
                        App.ScreenCache.Add(ix, bmp);
                }

                //�O�̃y�[�W
                ix = GetNextPageIndex(App.g_pi.NowViewPage);
                if (ix >= 0 && !App.ScreenCache.ContainsKey(ix))
                {
                    Debug.WriteLine(ix, "getScreenCache() Add Next");
                    var bmp = Bmp.MakeOriginalSizeImage(ix);
                    if (bmp != null)
                        App.ScreenCache.Add(ix, bmp);
                }
            }
        }

        /// <summary>
        /// �s�v�ȃX�N���[���L���b�V�����폜����
        /// </summary>
        private static void PurgeScreenCache()
        {
            //�폜�������X�g�A�b�v
            int now = App.g_pi.NowViewPage;
            const int DISTANCE = 2;
            List<int> deleteCandidate = new List<int>();

            foreach (var ix in App.ScreenCache.Keys)
            {
                if (ix > now + DISTANCE || ix < now - DISTANCE)
                {
                    deleteCandidate.Add(ix);
                }
            }

            //�폜�����폜����
            if (deleteCandidate.Count > 0)
            {
                foreach (int ix in deleteCandidate)
                {
                    //��ɏ����Ă͂��߁I
                    //�f�B�N�V���i������폜������Bitmap��Dispose()
                    //Bitmap tempBmp = ScreenCache[key];
                    //ScreenCache.Remove(key);
                    //tempBmp.Dispose();
                    if (App.ScreenCache.TryGetValue(ix, out Bitmap tempBmp))
                    {
                        App.ScreenCache.Remove(ix);
                        Debug.WriteLine($"PurgeScreenCache({ix})");
                    }
                    else
                    {
                        Debug.WriteLine($"PurgeScreenCache({ix})���s");
                    }
                }
            }
            //ver1.37GC
            //Uty.ForceGC();
        }

        #endregion �X�N���[���L���b�V��

        //�Â��L���b�V��DB�t�@�C������������BForm1_FormClosed()����Ă΂��
        private static void ClearOldCacheDBFile()
        {
            string[] files = Directory.GetFiles(Application.StartupPath, "*" + App.CACHEEXT);
            foreach (string sz in files)
            {
                bool isDel = true;
                string file1 = Path.GetFileNameWithoutExtension(sz);

                //MRU���X�g���`�F�b�N
                //MRU���X�g�ɂȂ��L���b�V���t�@�C���͍폜����B
                int mruCount = App.Config.mru.Count;
                for (int i = 0; i < mruCount; i++)
                {
                    //NullException�Ή��BNull�̉\���L ver0.982
                    if (App.Config.mru[i] == null)
                        continue;

                    string file2 = Path.GetFileName(App.Config.mru[i].Name);
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
                        //ver1.19 �G���[�������Ă��������Ȃ�
                    }
                    Debug.WriteLine(sz, "Cache���폜���܂���");
                }
            }
        }

        /// <summary>
        /// ���݂̃y�[�W���S�~���ɓ����B�폜��Ƀy�[�W�J�ڂ��s���B(ver1.35)
        /// </summary>
        private void RecycleBinNowPage()
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
            int next = GetNextPageIndex(now);
            if (next != -1)
            {
                //���y�[�W������̂Ŏ��y�[�W��\��
                //Screen�L���b�V����L���Ɏg�����ߐ�Ƀy�[�W�ύX
                SetViewPage(next);

                //�폜���ꂽ���߃y�[�W�ԍ���߂�
                App.g_pi.NowViewPage = now;
            }
            else if (now > 0)
            {
                //�O�̃y�[�W�Ɉړ�
                next = now - 1;
                SetViewPage(next);
            }
            else
            {
                //next=0 : �Ō�̃y�[�W��������
                Debug.WriteLine("�Ō�̃y�[�W��������");
                PicPanel.bmp = null;
                PicPanel.ResetView();
                PicPanel.Refresh();
            }

            //�S�~���֑���
            Uty.RecycleBin(nowfile);
            Debug.WriteLine(now.ToString() + "," + nowfile, "Delete");

            //pi����폜
            App.g_pi.Items.RemoveAt(now);

            //ScreenCache����폜
            App.ScreenCache.Clear();

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

                g_ClearPanel.ShowAndClose(
                    "�X���C�h�V���[���J�n���܂��B\r\n�}�E�X�N���b�N�܂��̓L�[���͂ŏI�����܂��B",
                    1500);
                SlideShowTimer.Interval = App.Config.slideShowTime;
                //SlideShowTimer.Tick += new EventHandler(SlideShowTimer_Tick);
                SlideShowTimer.Start();
            }
        }

        private void SlideShowTimer_Tick(object sender, EventArgs e)
        {
            if (GetNextPageIndex(App.g_pi.NowViewPage) == -1)
                StopSlideShow();
            else
                NavigateToForword();
        }

        private void StopSlideShow()
        {
            if (SlideShowTimer.Enabled)
            {
                //�X���C�h�V���[���I��������
                SlideShowTimer.Stop();
                g_ClearPanel.ShowAndClose("�X���C�h�V���[���I�����܂���", 1500);
            }
        }

        #endregion

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
                    AsyncStart(args.Skip(1).ToArray());
                }
            }));
        }

        private void Menu_Unsharp_Click(object sender, EventArgs e)
        {
            App.Config.useUnsharpMask = !App.Config.useUnsharpMask;
            MenuItem_Unsharp.Checked = App.Config.useUnsharpMask;

            //�ĕ`��
            PicPanel.Invalidate();
        }

        private void Menu_Help_GC_Clicked(object sender, EventArgs e) => Uty.ForceGC();
    } // Class Form1
}
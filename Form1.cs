#define SEVENZIP	//SevenZipSharp���g���Ƃ��͂�����`����B

using System;
using System.Collections.Generic;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.IO;						//Directory, File
using System.Threading;					//ThreadPool, WaitCallback
using System.Windows.Forms;
using System.Collections;				//ICollection as�ł���
using System.Linq;

namespace Marmi
{
	public partial class Form1 : Form, IRemoteObject
	{
		#region static var
		//�R���t�B�O�ۑ��p�B�����P��������
		public static AppGlobalConfig g_Config;
		//Form1�Q�Ɨp�n���h��
		public static Form1 _instance;
		#endregion


		//static��`
		public static readonly int DEFAULT_THUMBNAIL_SIZE = 400;	//�T���l�C���W���T�C�Y
		public static PackageInfo g_pi = null;				//���݌��Ă���p�b�P�[�W���

		#region const
		//�R���t�B�O�t�@�C�����BXmlSerialize�ŗ��p
		//private const string CONFIGNAME = "Marmi.xml";
		//�A�v�����B�^�C�g���Ƃ��ė��p
		private const string APPNAME = "Marmi";
		//�T���l�C���L���b�V���̊g���q
		private const string CACHEEXT = ".tmp";
		//�T�C�h�o�[�̕W���T�C�Y
		private const int SIDEBAR_DEFAULT_WIDTH = 200;
		//�񓯊�IO�^�C���A�E�g�l
		const int ASYNC_TIMEOUT = 5000;
		#endregion

		//��ʕ\���֘A
		public int g_viewPages = 1;						//�����Ă���y�[�W���F�P���Q
		//private Bitmap g_originalSizeBitmap = null;		//�\������Ă��錴��Bitmap

		//Susie�v���O�C��
		private Susie susie = new Susie();
		//unrar.dll�v���O�C�� ver1.76
		private Unrar unrar = new Unrar();

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
		//private PicturePanel overlay = new PicturePanel();
		#endregion

		#region --- ���\�[�X�I�u�W�F�N�g ---
		private readonly Icon iconLoope = Properties.Resources.loopeIcon;
		private readonly Icon iconLeftFinger = Properties.Resources.finger_left_shadow_ico;
		private readonly Icon iconRightFinger = Properties.Resources.finger_right_shadow_ico;
		private readonly Icon iconHandOpen = Properties.Resources.iconHandOpen;
		private Cursor cursorLeft;
		private Cursor cursorRight;
		private Cursor cursorLoupe;
		public static Cursor cursorHandOpen;
		#endregion

		#region --- �f�[�^�N���X ---
		private List<string> DeleteDirList = new List<string>();	//�폜���f�B���N�g��
		//ver1.35 �X�N���[���L���b�V��
		Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();
		#endregion

		#region --- �񓯊�IO�p�I�u�W�F�N�g ---
		//�񓯊�IO�p�X���b�h
		private Thread AsyncIOThread = null;
		//�񓯊��擾�p�X�^�b�N
		//ver1.81 Sidebar������o�^���邽��public�ɕύX
		private PrioritySafeQueue<KeyValuePair<int, Delegate>> stack = new PrioritySafeQueue<KeyValuePair<int, Delegate>>();
		//�񓯊��S�W�J�pSevenZipWrapper
		SevenZipWrapper m_AsyncSevenZip = null;
		#endregion

		//�t���O��
		//�T���l�C������邩�B1000�ȏ゠�����Ƃ��̃t���O
		//private bool g_makeThumbnail = true;
		//�}�E�X�N���b�N���ꂽ�ʒu��ۑ��B�h���b�O����p
		private Point g_LastClickPoint = Point.Empty;
		volatile static ThreadStatus tsThumbnail = ThreadStatus.STOP;	//�X���b�h�̏󋵂�����
		//ver1.51 ���O��ScreenCache����邩�ǂ����̃t���O
		private bool needMakeScreenCache = false;

		//BeginInvoke�pDelegate
		private delegate void StatusbarRenew(string s);


		#region --- �X���C�h�V���[�p�I�u�W�F�N�g ---
		//ver1.35 �X���C�h�V���[�^�C�}�[
		System.Windows.Forms.Timer SlideShowTimer = new System.Windows.Forms.Timer();
		//�X���C�h�V���[�����ǂ���
		public bool isSlideShow { get { return SlideShowTimer.Enabled; } }
		#endregion

		//ver1.80 �{�I�@�\�p�p�l��
		FlowLayoutPanel BookShelf = null;


		// �R���X�g���N�^ *************************************************************/
		public Form1()
		{
			this.Name = "Marmi";
			_instance = this;

			////�ݒ�t�@�C���̓ǂݍ���
			////g_Config = (AppGlobalConfig)LoadFromXmlFile();
			//g_Config = (AppGlobalConfig)AppGlobalConfig.LoadFromXmlFile();
			//if (g_Config == null)
			//	g_Config = new AppGlobalConfig();

			//�R���g���[����ǉ��B�c�[���X�g���b�v�͍Ō�ɒǉ�
			MyInitializeComponent();
			InitializeComponent();
			toolStrip1.Items.Add(g_trackbar);
			//
			// ver1.62 �c�[���o�[�̈ʒu
			//
			toolStrip1.Dock = g_Config.isToolbarTop ? DockStyle.Top : DockStyle.Bottom;

			//�����ݒ�
			this.KeyPreview = true;
			this.BackColor = g_Config.BackColor;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.SetStyle(ControlStyles.Opaque, true);
			Application.Idle += new EventHandler(Application_Idle);

			//z�I�[�_�[��������
			//SetFullScreen(false);
			//ver1.77 �t���X�N���[����Ԃ̕ۑ��ɑΉ�
			//if (g_Config.saveFullScreenMode)
			//	SetFullScreen(g_Config.isFullScreen);
			//else
			//	SetFullScreen(false);

			//�c�[���o�[�̕�����ݒ� ver1.67
			SetToolbarString();

			//�񓯊�IO�̊J�n
			AsyncIOThreadStart();
        }

		private void MyInitializeComponent()
		{
			//
			//�p�b�P�[�W��� PackageInfo
			//
			g_pi = new PackageInfo();
			//
			//PicturePanel
			//
			this.Controls.Add(PicPanel);
			PicPanel.Enabled = true;
			PicPanel.Visible = true;
			//PicPanel.Left = 0;	//ver1.62�R�����g�A�E�g
			PicPanel.Width = ClientRectangle.Width;
			PicPanel.BackColor = g_Config.BackColor;
			PicPanel.MouseClick += (s, e) => { OnMouseClick(e); };
			PicPanel.MouseDoubleClick += (s, e) => { OnMouseDoubleClick(e); };
			PicPanel.MouseMove += (s, e) => { OnMouseMove(e); };
			PicPanel.MouseUp += (s, e) => { OnMouseUp(e); };
			PicPanel.MouseWheel += new MouseEventHandler(PicPanel_MouseWheel);
			PicPanel.Dock = DockStyle.Fill;	//ver1.62 �ǉ�
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
			//�T���l�C���p�l��
			//
			g_ThumbPanel = new ThumbnailPanel();
			this.Controls.Add(g_ThumbPanel);
			g_ThumbPanel.MouseMove += new MouseEventHandler(g_ThumbPanel_MouseMove);
			g_ThumbPanel.Init();
			g_ThumbPanel.Visible = false;
			g_ThumbPanel.Dock = DockStyle.Fill;	//ver1.64�ǉ�
			//
			//ClearPanel
			//
			g_ClearPanel = new ClearPanel(PicPanel);

			//���̑��ϐ�������
			g_hoverStripItem = null;	//�z�o�[���̃��j���[/�c�[���A�C�e���B��Focus�N���b�N�Ή�
			//ver1.81 �ύX
			//SetKeyConfig();
			SetKeyConfig2();

			//ver1.35 �X���C�h�V���[�^�C�}�[
			SlideShowTimer.Tick += new EventHandler(SlideShowTimer_Tick);
		}




		// �t�H�[���C�x���g *************************************************************/

		private void Form1_Load(object sender, EventArgs e)
		{
			//�A�C�R���A�J�[�\���̐ݒ�
			cursorLeft = new Cursor(iconLeftFinger.Handle);
			cursorRight = new Cursor(iconRightFinger.Handle);
			cursorLoupe = new Cursor(iconLoope.Handle);
			cursorHandOpen = new Cursor(iconHandOpen.Handle);

			////�ݒ�̃��[�h/�K�p
			//������MyInit�Ő�Ɏ��{���Ă������Ƃɂ���Bver0.982
			//g_Config = (AppGlobalConfig)LoadFromXmlFile();
			//if (g_Config == null)
			//    g_Config = new AppGlobalConfig();

			applySettingToApplication();

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
			if (g_Config.isFullScreen)
			{
				SetFullScreen(false);
				//ver1.77 ���ɖ߂����ǃ��[�h�ۑ��͂�����
				g_Config.isFullScreen = true;
			}


			//�񓯊�IO�X���b�h�̏I��
			AsyncIOThread.Abort();
			AsyncIOThread.Join();

			//�T���l�C�����[�h�̉��
			if (g_Config.isThumbnailView)
			{
				SetThumbnailView(false);
			}


			//7z�𓀂����Ă����璆�f
			if (m_AsyncSevenZip != null)
			{
				m_AsyncSevenZip.CancelAsyncExtractAll();
			}

			//�X���b�h�����삵�Ă������~������.
			//�T���l�C���̕ۑ�
			//�t�@�C���n���h���̉��
			InitControls();

			//ver1.62�c�[���o�[�ʒu��ۑ�
			g_Config.isToolbarTop = (toolStrip1.Dock == DockStyle.Top);

			////////////////////////////////////////ver1.10

			//ver1.10
			//�ݒ�̕ۑ�
			if (g_Config.isSaveConfig)
			{
				//�ݒ�t�@�C����ۑ�����
				applySettingToConfig();
				AppGlobalConfig.SaveToXmlFile(g_Config);
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
			if (g_Config.isAutoCleanOldCache)
				ClearOldCacheDBFile();


			//Application.Idle�̉��
			Application.Idle -= new EventHandler(Application_Idle);

			//ver1.57 susie���
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

			//�h���b�v���ꂽ�����t�@�C�����ǂ����`�F�b�N
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				//Form���A�N�e�B�u
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

			//���������o���Ă��Ȃ��Ƃ����A��
			//�t�H�[�������������O�ɂ�Resize()�͌Ă΂��\��������B
			if (g_Config == null)
				return;

			//�ŏ������ɂ͉������Ȃ��ŋA��
			if (this.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			//ver0.972 �i�r�o�[������΃i�r�o�[�����T�C�Y
			AjustSidebarArrangement();

			//�T���l�C�����H
			//Form���\������O�ɂ��Ă΂��̂�ThumbPanel != null�͕K�{
			if (g_ThumbPanel != null && g_Config.isThumbnailView)
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
			//if (g_Config.isStopPaintingAtResize)
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
			Uty.WriteLine("OnResizeEnd()");

			//�T���l�C���\�����[�h��
			if (g_ThumbPanel != null && g_Config.isThumbnailView)
			{
				//�T���l�C���p�l�����\������Ă���ꍇ�͂���������{
				//�\������
				//ThumbPanel.thumbnailImageSet = g_pi.Items;
				//Rectangle rect = GetClientRectangle();
				//ThumbPanel.Location = rect.Location;
				//ThumbPanel.Size = rect.Size;
				//ThumbPanel.drawThumbnailToOffScreen();

				//�X���b�h�łɂ������ߕs�v
				//ThumbPanel.MakeThumbnailScreen(true);
			}
			else
			{
				//ResizeEnd�ŃX�N���[���o�[��\��
				//UpdateFormScrollbar();

				//��ʂ�`�ʁB�������n�C�N�I���e�B��
				//PaintBG2(LastDrawMode.HighQuality);
				//this.Refresh();		//this.Invalidate();
				UpdateStatusbar();
				PicPanel.ResizeEnd();
			}

			//�g���b�N�o�[�\���𒼂�
			ResizeTrackBar();
		}

		private void Application_Idle(object sender, EventArgs e)
		{
			//return;

			//Debug.WriteLine(DateTime.Now, "Application_Idle()");
			UpdateToolbar();
			//setStatusbarPages();

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
			if (g_Config.isThumbnailView)
			{
				g_ThumbPanel.Application_Idle();
			}

			//ScreenCache�����K�v������΍쐬�B
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
			ThreadPool.QueueUserWorkItem(dummy =>
				{
					this.Invoke((MethodInvoker)(() => { Start(files); }));
				});
		}

		// �X�^�[�g *********************************************************************/
		private void Start(string[] filenames)
		{
			//ver1.73 MRU���X�g�̍X�V
			//���܂Ō��Ă������̂�o�^����
			UpdateMRUList();

			//�t�@�C�������łɊJ���Ă��邩�ǂ����`�F�b�N
			if (filenames.Length == 1 && filenames[0] == g_pi.PackageName)
			{
				string text = "���Ȃ��t�H���_/�t�@�C�����J�����Ƃ��Ă��܂��B�J���܂����H";
				string title = "����t�H���_/�t�@�C���I�[�v���̊m�F";
				if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
					return;
			}

			//ver1.41 �񓯊�IO���~
			stack.Clear();
			stack.Push(new KeyValuePair<int, Delegate>(-1, null));
			g_pi.Initialize();
			setStatusbarInfo("�������E�E�E" + filenames[0]);

			//ver1.35�X�N���[���L���b�V�����N���A
			ScreenCache.Clear();

			//�R���g���[���̏�����
			InitControls();		//���̎��_�ł�g_pi.PackageName�͂ł��Ă��Ȃ���MRU�������Ȃ��B

			//ver1.78�P��t�@�C���̏ꍇ�A���̃f�B���N�g����ΏۂƂ���
			string onePicFile = string.Empty;
			if (filenames.Length == 1 && Uty.isPictureFilename(filenames[0]))
			{
				onePicFile = filenames[0];
				filenames[0] = Path.GetDirectoryName(filenames[0]);
			}

			//�t�@�C���ꗗ�𐶐�
			SevenZipWrapper.ClearPassword();	//���ɂ̃p�X���[�h���N���A
			bool needRecurse = setPackageInfo(filenames);

			//ver1.37 �ċA�\�������łȂ�Solid���ɂ��W�J
			//ver1.79 ��Ɉꎞ���ɂɓW�J�I�v�V�����ɑΉ�
			//if (needRecurse )
			if (needRecurse || g_pi.isSolid || g_Config.AlwaysExtractArchive)
			{
				using (AsyncExtractForm ae = new AsyncExtractForm())
				{
					setStatusbarInfo("���ɂ�W�J���ł�" + filenames[0]);

					//ver1.73 �ꎞ�t�H���_�w��Ή���makeTempDirName(bool isMakeDir)��ύX
					g_pi.tempDirname = makeTempDirName(true);
					//ver1.79 �ꎞ�t�H���_�����Ȃ��Ƃ��̑Ή�
					if(string.IsNullOrEmpty(g_pi.tempDirname))
					{
						MessageBox.Show("�ꎞ�W�J�t�H���_���쐬�ł��܂���ł����B�ݒ���m�F���Ă�������");
						g_pi.Initialize();
						return;
					}

					//�_�C�A���O��\��
					ae.ArchivePath = filenames[0];
					ae.ExtractDir = g_pi.tempDirname;
					ae.ShowDialog(this);

					//�_�C�A���O�̕\�����I��
					//�f�B���N�g�������ׂ�g_pi�ɓǂݍ���
					this.Cursor = Cursors.WaitCursor;
					g_pi.packType = PackageType.Pictures;
					g_pi.Items.Clear();
					GetDirPictureList(g_pi.tempDirname, true);
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
			if (g_pi.packType == PackageType.Pdf)
			{
				if (!susie.isSupportedExtentions("pdf"))
				{
					string str = "pdf�t�@�C���̓T�|�[�g���Ă��܂���";
					g_ClearPanel.ShowAndClose(str, 1000);
					setStatusbarInfo(str);
					g_pi.Clear();
					return;
				}
			}


			//ver1.30 ��ʂɓǂݍ��ݏ󋵂�\��
			//MaskPanel mp = new MaskPanel(PicPanel.Bounds);
			//mp.addText("�ǂݍ��ݒ�" + filenames[0]);
			//this.Controls.Add(mp);
			//mp.BringToFront();
			//mp.Refresh();

			if (g_pi.Items.Count <= 0)
			{
				//��ʂ��N���A�A�������̕���������
				string str = "�\���ł���t�@�C��������܂���ł���";
				g_ClearPanel.ShowAndClose(str, 1000);
				setStatusbarInfo(str);
				return;
			}
			//mp.addText("�A�C�e�����`�F�b�N");
			//mp.Refresh();

			//�y�[�W��������
			g_pi.NowViewPage = 0;
			//CheckAndStart();

			//�T���l�C��DB������Γǂݍ���
			//loadThumbnailDBFile();
			if (g_Config.isContinueZipView)
			{
				//�ǂݍ��ݒl�𖳎����A�O�ɃZ�b�g
				//g_pi.NowViewPage = 0;
				foreach (var mru in g_Config.mru)
				{
					if (mru == null)
						continue;
					else if (mru.Name == g_pi.PackageName
						//ver1.79 �R�����g�A�E�g
						//&& g_pi.packType == PackageType.Archive)
						)
					{
						//�ŏI�y�[�W��ݒ肷��B
						g_pi.NowViewPage = mru.LastViewPage;
						//Bookmark��ݒ肷��
						g_pi.setBookmarks(mru.Bookmarks);
						break;
					}
				}
			}

			//�P�t�@�C���h���b�v�ɂ��f�B���N�g���Q�Ƃ̏ꍇ
			//�ŏ��Ɍ���y�[�W���h���b�v�����t�@�C���ɂ���B
			if(!string.IsNullOrEmpty(onePicFile))
			{
				int i = g_pi.Items.FindIndex(c => c.filename == onePicFile);
				if (i < 0) i = 0;
				g_pi.NowViewPage = i;
			}

			//�g���b�N�o�[��������
			InitTrackbar();

			//SideBar�֓o�^
			g_Sidebar.Init(g_pi);

			//�^�C�g���o�[�̐ݒ�
			//this.Text = APPNAME + @" - " + g_pi.PackageName;
			//ver1.79 �t���p�X�\������߂�
			this.Text = APPNAME + @" - " + Path.GetFileName(g_pi.PackageName);

			//�T���l�C���̍쐬
			AsyncLoadImageInfo();

			//�摜��\��
			PicPanel.Message = string.Empty;
			SetViewPage(g_pi.NowViewPage);
		}

		/// <summary>
		/// �p�b�P�[�W����pi�Ɏ��
		/// </summary>
		/// <param name="files">�Ώۃt�@�C��</param>
		/// <returns>���ɓ����ɂ�����ꍇ��true</returns>
		private bool setPackageInfo(string[] files)
		{
			//������
			g_pi.Initialize();

			if (files.Length == 1)
			{
				//�h���b�v���ꂽ�̂�1��
				g_pi.PackageName = files[0];	//�f�B���N�g����Zip�t�@�C������z��

				//�h���b�v���ꂽ�t�@�C���̏ڍׂ�T��
				if (Directory.Exists(g_pi.PackageName))
				{
					//�f�B���N�g���̏ꍇ
					g_pi.packType = PackageType.Directory;
					GetDirPictureList(files[0], g_Config.isRecurseSearchDir);
				}
				else if (unrar.dllLoaded && files[0].ToLower().EndsWith(".rar"))
				{
					//
					//unrar.dll���g���B
					//
					g_pi.packType = PackageType.Archive;
					g_pi.isSolid = true;

					//�t�@�C�����X�g���\�z
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

					//�W�J���K�v�Ȃ̂�true��Ԃ�
					return true;
				}
				else if (Uty.isAvailableArchiveFile(g_pi.PackageName))
				{
					// ���Ƀt�@�C��
					g_pi.packType = PackageType.Archive;
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
				else if (files[0].ToLower().EndsWith(".pdf"))
				{
					//pdf�t�@�C��
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
						//pdf�͖��T�|�[�g
						//g_pi.PackageName = string.Empty;
						return false;
					}
				}
				else
				{
					//�P��摜�t�@�C��
					g_pi.PackageName = string.Empty;	//zip�ł��f�B���N�g���ł��Ȃ�
					g_pi.packType = PackageType.Pictures;

					//�P�����t�@�C����o�^
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
				//�����t�@�C��
				g_pi.PackageName = string.Empty;	//zip�ł��f�B���N�g���ł��Ȃ�
				//g_pi.isZip = false;
				g_pi.packType = PackageType.Pictures;

				//�t�@�C����ǉ�����
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

		// ���j���[���� *****************************************************************/

		private void NavigateToBack()
		{
			//�O�ɖ߂�
			long drawOrderTick = DateTime.Now.Ticks;
			int prev = GetPrevPage(g_pi.NowViewPage);
			if (prev >= 0)
				SetViewPage(prev, drawOrderTick);
			else
				g_ClearPanel.ShowAndClose("�擪�̃y�[�W�ł�", 1000);

		}

		private void NavigateToForword()
		{
			//ver1.35 ���[�v�@�\������
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
				//�擪�y�[�W�փ��[�v
				SetViewPage(0, drawOrderTick);
				g_ClearPanel.ShowAndClose("�擪�y�[�W�ɖ߂�܂���", 1000);
			}
			else if(g_Config.lastPage_toNextArchive)
			{
				//ver1.70 �ŏI�y�[�W�Ŏ��̏��ɂ��J��
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
								g_ClearPanel.ShowAndClose("���ֈړ����܂��F"+Path.GetFileName(s), 1000);
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
			else //if(g_Config.lastPage_stay)
			{
				g_ClearPanel.ShowAndClose("�Ō�̃y�[�W�ł�", 1000);
			}

		}

		public void SetThumbnailView(bool isShow)
		{
			if (isShow)
			{
				//�\����������
				g_Config.isThumbnailView = true;
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
				g_ThumbPanel.Dock = DockStyle.Fill;	//ver1.64
				g_ThumbPanel.Visible = true;
				toolButtonThumbnail.Checked = true;
				g_ThumbPanel.ReDraw();
			}
			else
			{
				//�\������߂�
				g_Config.isThumbnailView = false;
				//this.Controls.Remove(g_ThumbPanel);
				g_ThumbPanel.Visible = false;
				g_ThumbPanel.Dock = DockStyle.None;	//ver1.64
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
				if (g_Config.visibleNavibar)
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
			SetFullScreen(!g_Config.isFullScreen);
		}

		private void SetFullScreen(bool isFullScreen)
		{
			if (isFullScreen)
			{
				//�S��ʂɂ���
				g_Config.isFullScreen = true;

				menuStrip1.Visible = false;
				toolStrip1.Visible = false;
				statusbar.Visible = false;
				//g_Config.visibleMenubar = false;


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
				this.WindowState = FormWindowState.Maximized;	//�����Ŕ�������Resize�C�x���g�ɊԂɍ���Ȃ�
			}
			else
			{
				//�S��ʂ���������
				g_Config.isFullScreen = false;
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
				toolStrip1.Visible = g_Config.visibleToolBar;
				menuStrip1.Visible = g_Config.visibleMenubar;
				statusbar.Visible = g_Config.visibleStatusBar;
			}

			//���j���[�A�c�[���o�[�̍X�V
			Menu_ViewFullScreen.Checked = g_Config.isFullScreen;
			Menu_ContextFullView.Checked = g_Config.isFullScreen;
			toolButtonFullScreen.Checked = g_Config.isFullScreen;

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
			g_Config.dualView = isDual;
			toolButtonDualMode.Checked = isDual;
			Menu_View2Page.Checked = isDual;

			//ver1.36 �X�N���[���L���b�V�����N���A
			//ClearScreenCache();
			ScreenCache.Clear();

			SetViewPage(g_pi.NowViewPage);	//ver0.988 2010�N6��20��
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
		/// �ꎞ�t�H���_�̖��O��Ԃ��B
		/// ���Ȃ��ꍇ��null���Ԃ�B
		/// </summary>
		/// <param name="isMakeDir">���O�����łȂ��쐬����ꍇ��true�B</param>
		/// <returns>�ꎞ�t�H���_�̃t���p�X�B���Ȃ��ꍇ��null</returns>
		private string makeTempDirName(bool isMakeDir)
		{
			//���݂��Ȃ������_���ȃt�H���_�������
			string tempDir;
			
			//temp�t�H���_�̃��[�g�ƂȂ�t�H���_�����߂�B
			string rootPath = g_Config.tmpFolder;
			if (string.IsNullOrEmpty(rootPath))
				rootPath = Application.StartupPath;	//�A�v���̃p�X
				//Path.GetTempPath(),		//windows�W����TempDir

			//���j�[�N�ȃt�H���_��T��
			do
			{
				tempDir = Path.Combine(
					rootPath,
					"TEMP7Z" + Path.GetRandomFileName().Substring(0, 8));
			}
			while (Directory.Exists(tempDir));

			//�f�B���N�g���쐬�t���O�������Ă�����쐬
			if (isMakeDir)
			{
				//�f�B���N�g�������Ȃ��Ƃ���null��Ԃ�
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
			//ver1.54 2013�N5��7��
			for (int cnt = 0; cnt < g_pi.Items.Count; cnt++)
			{
				//�����_���̊O�ŕ�����쐬
				string s = string.Format("�摜���ǂݍ��ݒ�...{0}/{1}", cnt + 1, g_pi.Items.Count);

				//�X�^�b�N�ɓ����
				stack.PushLow(new KeyValuePair<int, Delegate>(cnt, (MethodInvoker)(() =>
				{
					setStatusbarInfo(s);
					//�ǂݍ��񂾂��̂�Purge�Ώۂɂ���
					g_pi.FileCacheCleanUp2(g_Config.CacheSize);
				})));
				//Uty.WriteLine("{0}�̃T���l�C���쐬�o�^���I��", cnt);
			}
			//�ǂݍ��݊������b�Z�[�W��Push
			stack.PushLow(new KeyValuePair<int, Delegate>(g_pi.Items.Count - 1, (MethodInvoker)(() =>
			{
				setStatusbarInfo("���O�摜���ǂݍ��݊���");
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
		/// D&D��A�v���N�����ɌĂ΂�鏉�������[�`��
		/// �X���b�h���~�߁A���ׂĂ̏�Ԃ�����������B
		/// �ǂݍ��ݑΏۂ̃t�@�C���ɂ��Ă͈�؉������Ȃ��B
		/// Form1_Load()
		/// Form1_FormClosed()
		/// OpenFileAndStart()
		/// Form1_DragDrop()
		/// </summary>
		/// 
		private void InitControls()
		{
			//�T���l�C�����[�h�̉��
			if (g_Config.isThumbnailView)
				SetThumbnailView(false);

			//2011/08/19 �T���l�C��������
			g_ThumbPanel.Init();

			//2011�N11��11�� ver1.24 �T�C�h�o�[
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

			//2011/08/19 trackbar��������
			//InitTrackBar();
			//nullRefer�̂��߉������Ȃ����Ƃɂ���
			//g_trackbar.Initialize();


			//MRU���X�V���� ver1.73 �R�����g�A�E�g
			//UpdateMRUList();

			//�T���l�C�����o�b�N�O���E���h�ŕۑ�����
			//saveDBFile();
			g_pi.Initialize();

			//�p�b�P�[�W����������
			//�Â��̂̓X���b�h���Ŏ̂Ă�̂ŐV�����̂����
			//g_pi = new PackageInfo();

			//7z�𓀂����Ă����璆�f
			if (m_AsyncSevenZip != null)
			{
				m_AsyncSevenZip.CancelAsyncExtractAll();
			}
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
			stack.Clear();
			stack.Push(new KeyValuePair<int, Delegate>(-1, null));

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
		//        setStatusbarInfo("�T���l�C���ۑ���");
		//        t.Start();
		//    }
		//}


		private void SortPackage()
		{
			//�t�@�C�����X�g����ёւ���
			if (g_pi.Items.Count > 0)
			{
				NaturalOrderComparer2 noc = new NaturalOrderComparer2();
				g_pi.Items.Sort(noc);
			}
			return;
		}

		/// <summary>
		/// ���ɏ����擾
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>���ɓ����ɂ�����ꍇ��trye</returns>
		private bool GetArchivedFileInfo(string filename)
		{
			using (SevenZipWrapper szw = new SevenZipWrapper())
			{
				bool retval = false;

				if (szw.Open(filename) == false)
				{
					MessageBox.Show("�G���[�̂��ߏ��ɂ͊J���܂���ł����B");
					g_pi.Initialize();
					Uty.ForceGC();		//���ɂ��J���EGC����K�v������
					return false;
				}

				//Zip�t�@�C������ݒ�
				g_pi.PackageName = filename;
				FileInfo fi = new FileInfo(g_pi.PackageName);
				g_pi.size = fi.Length;
				g_pi.isSolid = szw.isSolid;

				//ver1.31 7z�t�@�C���Ȃ̂Ƀ\���b�h����Ȃ����Ƃ�����I�H
				if (Path.GetExtension(filename) == ".7z")
					g_pi.isSolid = true;

				//g_pi.isZip = true;
				g_pi.packType = PackageType.Archive;

				//�t�@�C�������X�g�ɒǉ�
				//TODO: IEnumerable ����������foreach�ɂ�����
				g_pi.Items.Clear();
				//for (int i = 0; i < szw.itemCount; i++)
				//{
				//    ArchiveItem ai = szw.Item(i);
				//    if (!ai.isDirectory && Uty.isPictureFilename(ai.filename))
				//        g_pi.Items.Add(new ImageInfo(ai.filename, ai.datetime, (long)ai.filesize));
				//    else if (Uty.isAvailableArchiveFile(ai.filename))
				//    {
				//        //ver1.26 ���ɓ����ɂ𔭌�
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
		/// ���݉{�����Ă���g_pi.PackageName��MRU�ɒǉ�����
		/// �ȑO���������Ƃ�����ꍇ�A�{�����t�������X�V
		/// </summary>
		private void UpdateMRUList()
		{
			//�Ȃɂ�������Βǉ����Ȃ�
			if (string.IsNullOrEmpty(g_pi.PackageName))
				return;

			//�f�B���N�g���ł��ǉ����Ȃ�
			if (g_pi.packType == PackageType.Directory)
				return;

		
			//MRU�ɒǉ�����K�v�����邩�m�F
			bool needMruAdd = true;
			for (int i = 0; i < g_Config.mru.Length; i++)
			{
				if (g_Config.mru[i] == null)
					continue;
				if (g_Config.mru[i].Name == g_pi.PackageName)
				{
					//�o�^�ς݂�MRU���X�V
					//���t�����X�V
					g_Config.mru[i].Date = DateTime.Now;
					//�Ō�Ɍ����y�[�W���X�V v1.37
					g_Config.mru[i].LastViewPage = g_pi.NowViewPage;
					needMruAdd = false;

					//ver1.77 Bookmark���ݒ�
					g_Config.mru[i].Bookmarks = g_pi.getBookmarks();
				}
			}
			if (needMruAdd)
			{
				//MRU��V�����o�^
				//�Â����ɕ��ׂ遨�擪�ɒǉ�
				Array.Sort(g_Config.mru);
				g_Config.mru[0] = new MRUList(
									g_pi.PackageName,
									DateTime.Now,
									g_pi.NowViewPage,
									g_pi.getBookmarks());
			}
			Array.Sort(g_Config.mru);	//���ג���


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
		/// <param name="drawOrderTick">�`�ʏ����������I�[�_�[����</param>
		public void SetViewPage(int index, long drawOrderTick)
		{
			//ver1.09 �I�v�V�����_�C�A���O�����ƕK�������ɗ��邱�Ƃɑ΂���`�F�b�N
			if (g_pi.Items == null || g_pi.Items.Count == 0)
				return;

			//ver1.36 Index�͈̓`�F�b�N
			Debug.Assert(CheckIndex(index));

			// �y�[�W�i�s���� �i�ޕ����Ȃ琳�A�߂�����Ȃ畉
			// �A�j���[�V�����ŗ��p����
			int pageDirection = index - g_pi.NowViewPage;

			//�y�[�W�ԍ����X�V
			g_pi.NowViewPage = index;
			g_trackbar.Value = index;


			//ver1.35 �X�N���[���L���b�V���`�F�b�N
			Bitmap screenImage = null;
			if (ScreenCache.TryGetValue(index, out screenImage))
			{
				//�X�N���[���L���b�V���������̂ł����ɕ`��
				SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
				Debug.WriteLine(index, "Use ScreenCache");
			}
			else
			{
				//ver1.50
				//Key��������{key,null}�L���b�V��������������B�H�ɔ������邽��
				if (ScreenCache.ContainsKey(index))
					ScreenCache.Remove(index);

				//ver1.50 �ǂݍ��ݒ��ƕ\��
				setStatusbarInfo("Now Loading ... " + (index + 1).ToString());
				Application.DoEvents();

				//�摜�쐬���X���b�h�v�[���ɓo�^
				ThreadPool.QueueUserWorkItem(dummy =>
				{
					//screenImage = MakeOriginalSizeImage(g_pi.NowViewPage);
					screenImage = MakeOriginalSizeImage(index);
					this.Invoke((MethodInvoker)(() =>
					{
						SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
					}));
				});

				//�J�[�\����Wait��
				this.Cursor = Cursors.WaitCursor;
			}
		}


		private void SetViewPage2(int index, int pageDirection, Bitmap screenImage, long orderTime)
		{
			//ver1.50 �Â�������\�����Ȃ�
			//if (index != g_pi.NowViewPage)
			//{
			//    Uty.WriteLine("Skip SetViewPage2({0}) now = {1}", index, g_pi.NowViewPage);
			//    return;
			//}

			//ver1.55 drawOrderTick�̃`�F�b�N.
			// �X���b�h�v�[���ɓ��邽�ߋH�ɏ������O�シ��B
			// �ŐV�̕`�ʂłȂ���΃X�L�b�v
			if (PicPanel.drawOrderTime > orderTime)
			{
				Uty.WriteLine("Skip SetViewPage2({0}) too old order={1} < now={2}",
					index,
					orderTime,
					PicPanel.drawOrderTime);
				return;
			}

			//�`�ʊJ�n
			PicPanel.State = PicturePanel.DrawStatus.drawing;
			PicPanel.drawOrderTime = orderTime;

			if (screenImage == null)
			{
				Uty.WriteLine("bmp��null(index={0})", index);
				PicPanel.State = PicturePanel.DrawStatus.idle;
				PicPanel.Message = "�\���G���[ �ēx�\�����Ă݂Ă�������" + index.ToString();
				PicPanel.Refresh();
				return;
			}

			//ver1.50 �\��
			PicPanel.State = PicturePanel.DrawStatus.drawing;
			PicPanel.Message = string.Empty;
			if (g_Config.pictureSwitchMode != AnimateMode.none	//�A�j���[�V�������[�h�ł���
				&& !g_Config.keepMagnification					//�{���Œ胂�[�h�ł̓A�j���[�V�������Ȃ�
				&& pageDirection != 0)
			{
				//�X���C�h�C���A�j���[�V����
				PicPanel.AnimateSlideIn(screenImage, pageDirection);
			}

			#region �y�[�W��`��
			PicPanel.bmp = screenImage;
			PicPanel.ResetView();
			PicPanel.fastDraw = false;

			//ver1.78�R�����g�A�E�g
			////��ɉ�ʐ؂�ւ�莞�̓t�B�b�g���[�h�ŋN��
			//float r = PicPanel.FittingRatio;
			//if (r > 1.0f && Form1.g_Config.noEnlargeOver100p)
			//	r = 1.0f;
			//PicPanel.ZoomRatio = r;
			//PicPanel.AjustViewAndShow();

			//ver1.78 �{�����I�v�V�����w��ł���悤�ɕύX
			if (!g_Config.keepMagnification		//�{���ێ����[�h�ł͂Ȃ�
				|| isFitToScreen())				//��ʂɃt�B�b�g���Ă���
			{
				//��ʐ؂�ւ�莞�̓t�B�b�g���[�h�ŋN��
				float r = PicPanel.FittingRatio;
				if (r > 1.0f && Form1.g_Config.noEnlargeOver100p)
					r = 1.0f;
				PicPanel.ZoomRatio = r;
			}

			PicPanel.AjustViewAndShow();
			#endregion

			//1�y�[�W�\����2�y�[�W�\����
			//viewPages = CanDualView(index) ? 2 : 1;
			g_viewPages = (int)screenImage.Tag;
			PicPanel.State = PicturePanel.DrawStatus.idle;

			#region �I������
			//�J�[�\�������ɖ߂�
			this.Cursor = Cursors.Default;

			//UI�X�V
			UpdateStatusbar();
			UpdateToolbar();

			//�T�C�h�o�[�ŃA�C�e���𒆐S��
			if (g_Sidebar.Visible)
				g_Sidebar.SetItemToCenter(g_pi.NowViewPage);

			//ver1.37
			//�X�N���[���L���b�V�����擾
			//����ɕs�v�ȃ�������������AGC����
			//ThreadPool.QueueUserWorkItem(dummy =>
			//{
			//    // ver1.38 Idle�Ŗ����炸��SetViewPage�̍Ō�ł��
			//    getScreenCache();
			//    PurgeScreenCache();
			//    //FileCache���N���A
			//    g_pi.FileCacheCleanUp2(g_Config.CacheSize);
			//    //GC
			//    //Uty.ForceGC();
			//});
			//ver1.51 Idle()�ō�邽�߂̃t���O
			needMakeScreenCache = true;
			#endregion

			//PicPanel.Message = string.Empty;
			PicPanel.State = PicturePanel.DrawStatus.idle;
			Uty.ForceGC();
		}

		//ver1.35 �O�̃y�[�W�ԍ��B���łɐ擪�y�[�W�Ȃ�-1
		private int GetPrevPage(int index)
		{
			if (index > 0)
			{
				int declimentPages = -1;
				//2�y�[�W���炷���Ƃ��o���邩
				if (CanDualView(g_pi.NowViewPage - 2))
					declimentPages = -2;

				int ret = index + declimentPages;
				return ret >= 0 ? ret : 0;
			}
			else
				//���łɐ擪�y�[�W�Ȃ̂�-1��Ԃ�
				return -1;
		}


		//ver1.36���̃y�[�W�ԍ��B���łɍŏI�y�[�W�Ȃ�-1
		private int GetNextPage(int index)
		{
			int pages = CanDualView(index) ? 2 : 1;

			if (index + pages <= g_pi.Items.Count - 1)
				return (index + pages);
			else
				//�ŏI�y�[�W
				return -1;
		}


		public void AsyncGetBitmap(int index, Delegate action)
		{
			//�L���b�V���������Ă���Δ񓯊����Ȃ�
			//Bitmap bmp = g_pi.GetBitmapFromCache(index);
			//if (bmp != null)
			if(g_pi.hasCacheImage(index))
			{
			    if (action != null)
			        ((MethodInvoker)action)();
			    return;
			}

			////ver1.57 pdf�Ή�
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

			//ver1.54 HighQueue�Ƃ��ēo�^����Ă��邩�ǂ����m�F����B
			var array = stack.ToArrayHigh();
			foreach (var elem in array)
			{
				if (elem.Key == index)
				{
					Uty.WriteLine("AsyncGetBitmap() Skip {0}", index);
					return;
				}
			}

			//�񓯊����邽�߂�Push
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
		/// Bitmap�T�C�Y���擾����
		/// Bitmap�����Ȃ����������͂��B
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private Size SyncGetBitmapSize(int index)
		{
			if (g_pi.Items[index].hasInfo)
				return g_pi.Items[index].bmpsize;
			else
			{
				//�񓯊���GetBitmap()��ǂݏI���܂ő҂�
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

			//�Ƃ肠����1���ǂ߁I
			//Bitmap bmp1 = g_pi.GetBitmap(index);
			Bitmap bmp1 = SyncGetBitmap(index);
			if (bmp1 == null)
			{
				if (g_pi.isSolid && g_Config.isExtractIfSolidArchive)
					PicPanel.Message  = "�摜�t�@�C����W�J���ł�";
				else
					PicPanel.Message = "�Ǎ��݂Ɏ��Ԃ��������Ă܂�.�����[�h���Ă�������";
				return null;
			}
			
			//ver1.81 �T���l�C���o�^
			//g_pi.AsyncThumnailMaker(index, bmp1);
			g_pi.AsyncThumnailMaker(index, bmp1.Clone() as Bitmap);

			if (g_Config.dualView && CanDualView(index))
			{
				//2���\��
				//viewPages = 2;
				//Bitmap bmp2 = g_pi.GetBitmap(index + 1);
				Bitmap bmp2 = SyncGetBitmap(index + 1);
				if (bmp2 == null)
				{
					//2���ڂ̓ǂݍ��݂��G���[�Ȃ̂�1���\���ɂ���
					//viewPages = 1;
					//g_originalSizeBitmap = bmp1;
					bmp1.Tag = 1;
					return bmp1;
				}

				//ver1.81 �T���l�C���o�^
				g_pi.AsyncThumnailMaker(index + 1, bmp2.Clone() as Bitmap);


				//�����y�[�W�����
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
						//������E��
						//2����(���j��`��
						g.DrawImage(bmp2, 0, 0, width2, height2);
						//1���ځi�E�j��`��
						g.DrawImage(bmp1, width2, 0, width1, height1);
					}
					else
					{
						//�E���獶��
						//2����(���j��`��
						g.DrawImage(bmp1, 0, 0, width1, height1);
						//1���ځi�E�j��`��
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
				//1���\��
				bmp1.Tag = 1;
				return bmp1;
			}
		}


		// ���[�e�B���e�B�n *************************************************************/
		private void UpdateToolbar()
		{

			//��ʃ��[�h�̏�Ԕ��f
			toolButtonDualMode.Checked = g_Config.dualView;
			toolButtonFullScreen.Checked = g_Config.isFullScreen;
			toolButtonThumbnail.Checked = g_Config.isThumbnailView;

			//Sidebar
			toolStripButton_Sidebar.Checked = g_Sidebar.Visible;

			if (g_pi.Items == null || g_pi.Items.Count < 1)
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


				if (g_Config.isThumbnailView)
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
					if (g_Config.isReplaceArrowButton)
					{
						//����ւ�
						toolButtonLeft.Enabled = !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N
						toolButtonRight.Enabled = (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
					}
					else
					{
						toolButtonLeft.Enabled = (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
						toolButtonRight.Enabled = !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N
					}

					//100%�Y�[��
					toolStripButton_Zoom100.Checked = isScreen100p();

					//��ʃt�B�b�g�Y�[��
					toolStripButton_ZoomFit.Checked = isFitToScreen();

					//Favorite
					if (g_pi.Items[g_pi.NowViewPage].isBookMark)
						toolStripButton_Favorite.Checked = true;
					else if (g_viewPages == 2 
						&& g_pi.NowViewPage < g_pi.Items.Count - 1		//ver1.69 �ŏI�y�[�W���O�`�F�b�N
						&& g_pi.Items[g_pi.NowViewPage + 1].isBookMark)	//
						toolStripButton_Favorite.Checked = true;
					else
						toolStripButton_Favorite.Checked = false;

					//Sidebar
					toolStripButton_Sidebar.Checked = g_Sidebar.Visible;
				}

				//TrackBar
				//�����Œ�����UI���x���Ȃ�B
				//g_trackbar.Value = g_pi.NowViewPage;
			}
		}

		//��ʂɃt�B�b�g���Ă��邩�ǂ���
		private bool isFitToScreen()
		{
			return (Math.Abs(PicPanel.ZoomRatio - PicPanel.FittingRatio) < 0.001f);
		}
		//100%�\�����ǂ���
		private bool isScreen100p()
		{
			return (Math.Abs(PicPanel.ZoomRatio - 1.0f) < 0.001f);
		}

		/// <summary>
		/// MRU���X�g���X�V����B���ۂɃ��j���[�̒��g���X�V
		/// ���̊֐����Ăяo���Ă���̂�Menu_File_DropDownOpening�̂�
		/// </summary>
		private void UpdateMruMenuListUI()
		{
			MenuItem_FileRecent.DropDownItems.Clear();

			Array.Sort(g_Config.mru);

			int menuCount = 0;

			//for (int i = 0; i < mySetting.mru.Length; i++)	//�Â���
			for (int i = g_Config.mru.Length - 1; i >= 0; i--)		//�V�������ɂ���
			{
				if (g_Config.mru[i] == null)
					continue;

				MenuItem_FileRecent.DropDownItems.Add(
					g_Config.mru[i].Name,					//�A�C�e���̃e�L�X�g
					null,									//�A�C�e���̃C���[�W
					new System.EventHandler(OnClickMRUMenu)	//�C�x���g
					);

				//ver1.73 MRU�\�����̐���
				if (++menuCount >= g_Config.numberOfMru)
					break;
			}
		}

		/// <summary>
		/// �f�B���N�g���̉摜�����X�g�ɒǉ�����B�ċA�I�ɌĂяo�����߂Ɋ֐���
		/// </summary>
		/// <param name="dirName">�ǉ��Ώۂ̃f�B���N�g����</param>
		/// <param name="isRecurse">�ċA�I�ɑ�������ꍇ��true</param>
		private void GetDirPictureList(string dirName, bool isRecurse)
		{

			string[] files = Directory.GetFiles(dirName);

			//�摜�t�@�C��������ǉ�����
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

			//�ċA�I�Ɏ擾���邩�ǂ����B
			//if (g_Config.isRecurseSearchDir)
			if (isRecurse)
			{
				string[] dirs = Directory.GetDirectories(dirName);
				foreach (string name in dirs)
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
		/// �N���C�A���g�T�C�Y�����߂�B�c�[���o�[��X�e�[�^�X�o�[�Ȃǂ����Z
		/// �c�[���o�[�̑傫���Ȃǂ��l�������N���C�A���g�̈��Ԃ�
		/// �␳�ΏہF
		///   ���j���[�o�[, �X�N���[���o�[, �T�C�h�o�[, �c�[���o�[, �X�e�[�^�X�o�[
		/// </summary>
		/// <returns>�N���C�A���g�ʒu�A�T�C�Y��\��Rectangle</returns>
		public Rectangle GetClientRectangle()
		{
			Rectangle rect = this.ClientRectangle; // this.Bounds;

			//�c�[���o�[�̍���
			int toolbarHeight = (toolStrip1.Visible && !g_Config.isFullScreen) ? toolStrip1.Height : 0;

			//���j���[�o�[�̍���
			int menubarHeight = (menuStrip1.Visible) ? menuStrip1.Height : 0;

			//�X�e�[�^�X�o�[�̍���
			int statusbarHeight = (statusbar.Visible && !g_Config.isFullScreen) ? statusbar.Height : 0;

			//�c�[���o�[����̎�����Y����T��
			if (g_Config.isToolbarTop)
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

		// �T�C�h�o�[�֘A ***************************************************************/
		/// <summary>
		/// �T�C�h�o�[�̃T�C�Y���ύX���ꂽ�i����I������j�Ƃ��ɌĂяo�����B
		/// ���ꂪ�Ăяo�����Ƃ��̓T�C�h�o�[�Œ�̎��̂݁B
		/// 2010�N6��6�� ver0.985�Ŏ���
		/// </summary>
		/// <param name="sender">���p����</param>
		/// <param name="e">���p����</param>
		void g_Sidebar_SidebarSizeChanged(object sender, EventArgs e)
		{
			OnResizeEnd(null);
		}


		// �摜���� *********************************************************************/


		private void AnimateThumbnailBlur(int index)
		{
			//ver1.25 �T���l�C�����g���č����\��
			Bitmap tempbmp = null;

			if (g_pi.Items[index].thumbnail != null)
			{
				if (g_Config.dualView && CanDualView(index))
				{
					//���摜�����T�C�Y�̉摜���T���l�C��������
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
					//ver1.29 �Ԉ���Ă���
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
				//�Y�[���䗦�ݒ�
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
			//�I���̂�҂I
			//while (!th.Join(10))
			//{}

			//������DisPose�����Ⴞ��
			//if (tempbmp != null)
			//    tempbmp.Dispose();
		}


		/// <summary>
		/// �w�肳�ꂽ�C���f�b�N�X����Q���\���ł��邩�`�F�b�N
		/// �`�F�b�N��ImageInfo�Ɏ�荞�܂ꂽ�l�𗘗p�A�c����Ŋm�F����B
		/// </summary>
		/// <param name="index">�C���f�b�N�X�l</param>
		/// <returns>2��ʕ\���ł���Ƃ���true</returns>
		private bool CanDualView(int index)
		{
			//�Ō�̃y�[�W�ɂȂ��Ă��Ȃ����m�F
			if (index >= g_pi.Items.Count - 1 || index < 0)
				return false;

			//�R���t�B�O�������m�F
			if (!g_Config.dualView)
				return false;

			//ver1.79����Ȃ���2�y�[�W�\��
			if (g_Config.dualView_Force)
				return true;

			//1���ڃ`�F�b�N
			if (!g_pi.Items[index].hasInfo)
				//SyncGetBitmap(index);
				SyncGetBitmapSize(index);
			//if (g_pi.Items[index].width > g_pi.Items[index].height)
			if (g_pi.Items[index].isFat)
				return false; //����������

			//�Q���ڃ`�F�b�N
			if (!g_pi.Items[index + 1].hasInfo)
				//SyncGetBitmap(index + 1);
				SyncGetBitmapSize(index + 1);
			//if (g_pi.Items[index + 1].width > g_pi.Items[index + 1].height)
			if (g_pi.Items[index + 1].isFat)
				return false;�@//����������

			//�S�ďc�����������̏���
			//ver1.70 �c���Ȃ�OK�Ƃ���
			//if(!g_Config.dualview_exactCheck)
			//	return true;
			//ver1.79 �ȈՃ`�F�b�N�F�c�摜2����OK
			if (g_Config.dualView_Normal)
				return true; //�c�摜2��

			//ver1.20 �قړ����T�C�Y���ǂ������`�F�b�N
			//�c�̒������قƂ�Ǖς��Ȃ����true
			const int ACCEPTABLE_RANGE = 200;
			if (Math.Abs(g_pi.Items[index].height - g_pi.Items[index + 1].height) < ACCEPTABLE_RANGE)
			    return true;
			else
			    return false;
		}

		// ���[�e�B���e�B�n�F�摜�L���b�V�� *********************************************/



		#region �X�N���[���L���b�V��
		/// <summary>
		/// �O��y�[�W�̉�ʃL���b�V�����쐬����
		/// ���݌��Ă���y�[�W�𒆐S�Ƃ���
		/// </summary>
		private void getScreenCache()
		{
			//ver1.37 �X���b�h�Ŏg�����Ƃ�O��Ƀ��b�N
			lock ((ScreenCache as ICollection).SyncRoot)
			{
				//�O�̃y�[�W
				int ix = GetPrevPage(g_pi.NowViewPage);
				if (ix >= 0 && !ScreenCache.ContainsKey(ix))
				{
					Debug.WriteLine(ix, "getScreenCache() Add Prev");
					ScreenCache.Add(ix, MakeOriginalSizeImage(ix));
				}

				//�O�̃y�[�W
				ix = GetNextPage(g_pi.NowViewPage);
				if (ix >= 0 && !ScreenCache.ContainsKey(ix))
				{
					Debug.WriteLine(ix, "getScreenCache() Add Next");
					ScreenCache.Add(ix, MakeOriginalSizeImage(ix));
				}
			}
		}
		/// <summary>
		/// �s�v�ȃX�N���[���L���b�V�����폜����
		/// </summary>
		private void PurgeScreenCache()
		{
			//�폜�������X�g�A�b�v
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
					Bitmap tempBmp = null;
					if (ScreenCache.TryGetValue(ix, out tempBmp))
					{
						ScreenCache.Remove(ix);
						//if (tempBmp != null) 
						//    tempBmp.Dispose();
						Uty.WriteLine("PurgeScreenCache({0})", ix);
					}
					else
						Uty.WriteLine("PurgeScreenCache({0})���s", ix);
				}
			}
			//ver1.37GC
			//Uty.ForceGC();
		}


		#endregion

		//�Â��L���b�V��DB�t�@�C������������BForm1_FormClosed()����Ă΂��
		private void ClearOldCacheDBFile()
		{
			string[] files = Directory.GetFiles(Application.StartupPath, "*" + CACHEEXT);
			foreach (string sz in files)
			{
				bool isDel = true;
				string file1 = Path.GetFileNameWithoutExtension(sz);

				//MRU���X�g���`�F�b�N
				//MRU���X�g�ɂȂ��L���b�V���t�@�C���͍폜����B
				int mruCount = g_Config.mru.Length;
				for (int i = 0; i < mruCount; i++)
				{
					//NullException�Ή��BNull�̉\���L ver0.982
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
						//ver1.19 �G���[�������Ă��������Ȃ�
					}
					Debug.WriteLine(sz, "Cache���폜���܂���");
				}
			}
		}


		/// <summary> 
		/// ver1.35 ���݂̃y�[�W���S�~���ɓ����
		/// </summary>
		private void RecycleBinNowPage()
		{
			//�A�C�e�����Ȃɂ��Ȃ���΂Ȃɂ����Ȃ�
			if (g_pi.Items.Count == 0)
				return;
			//2�y�[�W���[�h�̎����Ȃɂ����Ȃ�
			if (g_viewPages == 2)
				return;
			//�A�[�J�C�u�ɑ΂��Ă��Ȃɂ����Ȃ�
			if (g_pi.packType == PackageType.Archive)
				return;

			//���̃y�[�W�ԍ���ۑ�
			int now = g_pi.NowViewPage;
			string nowfile = g_pi.Items[now].filename;

			int next = GetNextPage(now);
			if (next != -1)
			{
				//���Ƀy�[�W������̂Ō��̃y�[�W��\��
				//Screen�L���b�V����L���Ɏg�����ߐ�Ƀy�[�W�ύX
				SetViewPage(next);

				//�폜���ꂽ���߃y�[�W�ԍ���߂�
				g_pi.NowViewPage = now;
			}
			else if (now > 0)
			{
				//�O�̃y�[�W�Ɉړ�
				next = now - 1;
				SetViewPage(next);
			}
			else
			{
				//�Ō�̃y�[�W��������
				Debug.WriteLine("�Ō�̃y�[�W��������");
				PicPanel.bmp = null;
				PicPanel.ResetView();
				PicPanel.Refresh();
			}


			//�S�~���֑���
			Uty.RecycleBin(nowfile);
			Debug.WriteLine(now.ToString() + "," + nowfile, "Delete");

			//pi����폜
			g_pi.Items.RemoveAt(now);


			//ScreenCache����폜
			ScreenCache.Clear();

			//Trackbar��ύX
			InitTrackbar();

			////�y�[�W��؂�ւ���
			//if (!CheckIndex(now))
			//    now--;
			//if (now < 0)
			//{
			//    //�Ō�̃y�[�W��������
			//    Debug.WriteLine("�Ō�̃y�[�W��������");
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
		// �X���C�h�V���[�֘A
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
					"�X���C�h�V���[���J�n���܂��B\r\n�}�E�X�N���b�N�܂��̓L�[���͂ŏI�����܂��B",
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
				//�X���C�h�V���[���I��������
				SlideShowTimer.Stop();
				g_ClearPanel.ShowAndClose("�X���C�h�V���[���I�����܂���", 1500);
			}
		}



		/// <summary>
		/// IPC�ŌĂ΂��C���^�[�t�F�[�X���\�b�h
		/// �R�}���h���C�������������Ă���̂�
		/// ������N���B
		/// �������A���̃��\�b�h���Ă΂��Ƃ��̓t�H�[���̃X���b�h�ł͂Ȃ��̂�
		/// Invoke���K�v
		/// </summary>
		/// <param name="args">�R�}���h���C������</param>
		void IRemoteObject.IPCMessage(string[] args)
		{
			//this.Activate();
			this.Invoke(((Action)(()=>
			{
				//������O�ʂɂ���
				this.Activate();

				//�\���Ώۃt�@�C�����擾
				if (args.Length > 1)
				{
					//1�߂Ɏ�����exe�t�@�C�����������Ă���̂ŏ���
					AsyncStart(args.Skip(1).ToArray());
				}
			})));
		}

		/// <summary>
		/// �X�^�b�N�Ƀv�b�V������
		/// Sidebar��Trackbar����摜���擾�������Ƃ��Ɏg���B
		/// </summary>
		/// <param name="index"></param>
		/// <param name="f"></param>
		public void PushLow(int index, Delegate f)
		{
			stack.PushLow( new KeyValuePair<int,Delegate>(index, f));
		}

	} // Class Form1
}
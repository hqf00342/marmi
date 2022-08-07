using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.IO;
using System.Windows.Forms;		//Application
using System.Xml.Serialization;			// XmlSerializer

namespace Marmi
{
    /********************************************************************************/
    // �ݒ��ۑ�����N���X�B
    /********************************************************************************/

    [Serializable]
    public class AppGlobalConfig //: INotifyPropertyChanged
    {
        //�R���t�B�O�t�@�C�����BXmlSerialize�ŗ��p
        private const string CONFIGNAME = "Marmi.xml";

        //�T�C�h�o�[�̊�{�̕�
        private const int SIDEBAR_INIT_WIDTH = 200;

        public static string ConfigFilename => Path.Combine(Application.StartupPath, CONFIGNAME);

        public bool DualView { get; set; }                       //2��ʕ��ׂĕ\��
        public Size windowSize;                     //�E�B���h�E�T�C�Y
        public Point windowLocation;                //�E�B���h�E�\���ʒu

        public List<MRU> Mru { get; set; } = new List<MRU>();     //MRU���X�g�p�z��

        public bool VisibleMenubar { get; set; }                 //���j���[�o�[�̕\��
        public bool VisibleToolBar { get; set; }                 //�c�[���o�[�̕\��
        public bool VisibleStatusBar { get; set; }               //�X�e�[�^�X�o�[�̕\��

        public bool IsRecurseSearchDir { get; set; }             //�f�B���N�g���̍ċA����

        public bool IsFitScreenAndImage { get; set; }            //�摜�ƃC���[�W���t�B�b�g������
        public bool IsStopPaintingAtResize { get; set; }         //���T�C�Y���̕`�ʂ���߂�
        public bool VisibleNavibar { get; set; }                 //�i�r�o�[�̕\��
        public bool IsAutoCleanOldCache { get; set; }            //�Â��L���b�V���̎����폜

        //�T�C�h�o�[�֘A
        //public bool isFixSidebar;					//�T�C�h�o�[���Œ�ɂ��邩�ǂ���
        public int SidebarWidth { get; set; }                    //�T�C�h�o�[�̕�

        //ver1.21�摜�؂�ւ����@
        public AnimateMode PictureSwitchMode { get; set; }

        ////ver1.35 �X�N���[���V���[����[ms]
        public int SlideShowTime { get; set; }

        //ver1.62 �c�[���o�[�̈ʒu
        public bool IsToolbarTop { get; set; }

        //ver1.70 2���\���̌��i��
        //public bool dualview_exactCheck;

        //ver1.77 ��ʃ��[�h�ۑ��Ώۂɂ���B
        public bool isFullScreen;

        //�T���l�C�����[�h
        [XmlIgnore]
        public bool isThumbnailView;

        //ver1.78 �{���̕ێ�
        public bool KeepMagnification { get; set; }

        //ver1.80 �_�u���N���b�N
        public bool DoubleClickToFullscreen { get; set; }

        public GeneralConfig General { get; set; } = new GeneralConfig();

        public ViewConfig View { get; set; } = new ViewConfig();
        public KeyConfig Keys { get; set; } = new KeyConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public AdvanceConfig Advance { get; set; } = new AdvanceConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        /*******************************************************************************/

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public AppGlobalConfig()
        {
            Initialize();
        }

        /// <summary>
        /// �e�p�����[�^�̏����l
        /// </summary>
        private void Initialize()
        {
            VisibleMenubar = true;
            VisibleToolBar = true;
            VisibleStatusBar = true;
            VisibleNavibar = false;

            DualView = false;
            isFullScreen = false;
            isThumbnailView = false;
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            //isSaveThumbnailCache = false;
            IsRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            IsFitScreenAndImage = true;

            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;

            //�T�C�h�o�[
            SidebarWidth = SIDEBAR_INIT_WIDTH;

            // ��ʐ؂�ւ����[�h
            PictureSwitchMode = AnimateMode.Slide;

            //���[�v���邩�ǂ���
            //isLoopToTopPage = false;

            //�X�N���[���V���[����
            SlideShowTime = 3000;

            //�c�[���o�[�̈ʒu
            IsToolbarTop = true;

            //ver1.70 2���\���̓f�t�H���g�ŊȈՃ`�F�b�N
            //dualview_exactCheck = false;

            //ver1.78 �{���̕ێ�
            KeepMagnification = false;
            //ver1.79 ���ɂ͕K���W�J

            //�_�u���N���b�N�@�\���J������
            DoubleClickToFullscreen = false;

            //ver1.91 �R���t�B�O���� 2022�N8��7��
            General.Init();
            View.Init();
            Thumbnail.Init();
            Keys.Init();
            Mouse.Init();
            Loupe.Init();
            Advance.Init();
        }

        /// <summary>
        /// XML�`���ŕۑ�����Object�����[�h����B
        /// </summary>
        /// <returns></returns>
        public static object LoadFromXmlFile()
        {
            string path = AppGlobalConfig.ConfigFilename;

            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    //�ǂݍ���ŋt�V���A��������
                    var xs = new XmlSerializer(typeof(AppGlobalConfig));
                    return xs.Deserialize(fs);
                }
            }
            return null;
        }

        /// <summary>
        /// XML�`����Object��ۑ�����
        /// </summary>
        /// <param name="obj"></param>
        public static void SaveToXmlFile(object obj)
        {
            string path = AppGlobalConfig.ConfigFilename;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var xs = new XmlSerializer(typeof(AppGlobalConfig));
                xs.Serialize(fs, obj);
            }
        }
    }
}
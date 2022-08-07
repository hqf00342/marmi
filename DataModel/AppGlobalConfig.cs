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
        public bool IsSaveConfig { get; set; }                   //�R���t�B�O�̕ۑ�

        //public bool isSaveThumbnailCache;			//�T���l�C���L���b�V���̕ۑ�
        public bool IsRecurseSearchDir { get; set; }             //�f�B���N�g���̍ċA����

        public bool IsReplaceArrowButton { get; set; }               //�c�[���o�[�̍��E�{�^�������ւ���

        public bool IsContinueZipView { get; set; }              //zip�t�@�C���͑O��̑�������
        public bool IsFitScreenAndImage { get; set; }            //�摜�ƃC���[�W���t�B�b�g������
        public bool IsStopPaintingAtResize { get; set; }         //���T�C�Y���̕`�ʂ���߂�
        public int ThumbnailSize;                   //�T���l�C���摜�̑傫��
        public bool VisibleNavibar { get; set; }                 //�i�r�o�[�̕\��
        public bool IsAutoCleanOldCache { get; set; }            //�Â��L���b�V���̎����폜

        public bool IsDrawThumbnailShadow { get; set; }          // �T���l�C���ɉe��`�ʂ��邩
        public bool IsDrawThumbnailFrame { get; set; }           // �T���l�C���ɘg��`�ʂ��邩
        public bool IsShowTPFileName { get; set; }               // �T���l�C���Ƀt�@�C������\�����邩
        public bool IsShowTPFileSize { get; set; }               // �t�@�C�����Ƀt�@�C���T�C�Y��\�����邩
        public bool IsShowTPPicSize { get; set; }                // �t�@�C�����ɉ摜�T�C�Y��\�����邩

        // ver1.35 ���������f��
        //public MemoryModel memModel;                //�������[���f��

        public int CacheSize { get; set; }                       //memModel == userDefined�̂Ƃ��̃L���b�V���T�C�Y[MB]

        //���[�y�֘A

        public bool IsFastDrawAtResize { get; set; }             // �����`�ʂ����邩�ǂ���

        //�T�C�h�o�[�֘A
        //public bool isFixSidebar;					//�T�C�h�o�[���Œ�ɂ��邩�ǂ���
        public int SidebarWidth { get; set; }                    //�T�C�h�o�[�̕�

        //ver1.09 ���Ɋ֘A
        public bool IsExtractIfSolidArchive { get; set; }        //�\���b�h���ɂȂ�ꎞ�W�J���邩

        //ver1.25
        public bool NoEnlargeOver100p { get; set; }          //��ʃt�B�b�e�B���O��100%�����ɂ���

        public bool IsDotByDotZoom { get; set; }             //Dot-by-Dot��ԃ��[�h�ɂ���

        //ver1.21�摜�؂�ւ����@
        public AnimateMode PictureSwitchMode { get; set; }

        ////ver1.35 �X�N���[���V���[����[ms]
        public int SlideShowTime { get; set; }

        //ver1.42 �T���l�C���̃t�F�[�h�C��
        [Obsolete]
        public bool IsThumbFadein { get; set; }

        //ver1.49 �E�B���h�E�̏����ʒu
        public bool IsWindowPosCenter { get; set; }

        //ver1.62 �c�[���o�[�̈ʒu
        public bool IsToolbarTop { get; set; }

        //ver1.65 �c�[���o�[�A�C�e���̕�����������
        public bool EraseToolbarItemString { get; set; }

        //ver1.70 �T�C�h�o�[�̃X���[�X�X�N���[��
        public bool Sidebar_smoothScroll { get; set; }

        //ver1.70 2���\���̌��i��
        //public bool dualview_exactCheck;

        //ver1.71 �ŏI�y�[�W�̓���
        public bool LastPage_stay { get; set; }

        public bool LastPage_toTop { get; set; }
        //public bool LastPage_toNextArchive { get; set; }

        //ver1.73 �ꎞ�W�J�t�H���_
        public string TmpFolder { get; set; }

        //ver1.73 MRU�ێ���
        public int NumberOfMru;

        [XmlIgnore]
        public Color BackColor;

        [XmlElement("XmlMainBackColor")]
        public string XmlMainColor
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }

        #region �T���l�C��

        [XmlIgnore]
        public Color ThumbnailBackColor;

        [XmlElement("XmlThumbnailBackColor")]
        public string XmlTbColor
        {
            set { ThumbnailBackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailBackColor); }
        }

        [XmlIgnore]
        public Font ThumbnailFont;

        [XmlElement("XmlThumbnailFont")]
        public string XmlTbFont
        {
            set
            {
                FontConverter fc = new FontConverter();
                ThumbnailFont = (Font)fc.ConvertFromString(value);
            }
            get
            {
                FontConverter fc = new FontConverter();
                return fc.ConvertToString(ThumbnailFont);
            }
        }

        [XmlIgnore]
        public Color ThumbnailFontColor;

        [XmlElement("XmlThumbnailFontColor")]
        public string XmlFontColor
        {
            set { ThumbnailFontColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailFontColor); }
        }

        #endregion �T���l�C��

        //ver1.77 ��ʃ��[�h�ۑ��Ώۂɂ���B
        public bool isFullScreen;

        //�T���l�C�����[�h
        [XmlIgnore]
        public bool isThumbnailView;

        //ver1.76 ���d�N���֎~�t���O
        public bool DisableMultipleStarts { get; set; }

        //ver1.77 ��ʕ\���ʒu�������ȈՂɂ��邩
        public bool SimpleCalcForWindowLocation { get; set; }

        //ver1.77 �t���X�N���[����Ԃ𕜌��ł���悤�ɂ���
        public bool SaveFullScreenMode { get; set; }

        //ver1.78 �{���̕ێ�
        public bool KeepMagnification { get; set; }

        //ver1.79 ���ɂ���ɓW�J���邩�ǂ���
        public bool AlwaysExtractArchive { get; set; }

        //ver1.79 2�y�[�W���[�h
        public bool DualView_Force { get; set; }

        public bool DualView_Normal { get; set; }
        public bool DualView_withSizeCheck { get; set; }

        //ver1.80 �_�u���N���b�N
        public bool DoubleClickToFullscreen { get; set; }

        public bool ThumbnailPanelSmoothScroll { get; set; }

        //ver1.83 �A���V���[�v�}�X�N
        public bool UseUnsharpMask { get; set; }

        public int UnsharpDepth { get; set; }

        public KeyConfig Keys { get; set; } = new KeyConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

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
            IsSaveConfig = false;
            //isSaveThumbnailCache = false;
            IsRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            BackColor = Color.LightSlateGray;
            IsReplaceArrowButton = false;
            IsFitScreenAndImage = true;

            IsContinueZipView = false;
            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;

            //�T���l�C���^�u
            ThumbnailSize = 200;                            //�T���l�C���T�C�Y
            ThumbnailBackColor = Color.White;               //�T���l�C���̃o�b�N�J���[
            ThumbnailFont = new Font("MS UI Gothic", 9);    //�T���l�C���̃t�H���g
            ThumbnailFontColor = Color.Black;               //�T���l�C���̃t�H���g�J���[
                                                            //isAutoCleanOldCache = false;					//�T���l�C���������ŃN���[�����邩
            IsDrawThumbnailShadow = true;                   //�T���l�C���ɉe��`�ʂ��邩
            IsDrawThumbnailFrame = true;                    //�T���l�C���ɘg��`�ʂ��邩
            IsShowTPFileName = true;                        //�摜����\�����邩
            IsShowTPFileSize = false;                       //�摜�̃t�@�C���T�C�Y��\�����邩
            IsShowTPPicSize = false;                        //�摜�̃s�N�Z���T�C�Y��\�����邩
            //IsThumbFadein = false;



            //�T�C�h�o�[
            SidebarWidth = SIDEBAR_INIT_WIDTH;

            //���x�Ȑݒ�
            IsFastDrawAtResize = true;                      //���T�C�Y���ɍ����`�ʂ����邩�ǂ���
                                                            //����
            IsExtractIfSolidArchive = true;
            //�N���X�t�F�[�h
            //isCrossfadeTransition = false;

            // ��ʐ؂�ւ����[�h
            PictureSwitchMode = AnimateMode.Slide;
            //zoom
            NoEnlargeOver100p = true;       //��ʃt�B�b�e�B���O��100%�����ɂ���
            IsDotByDotZoom = false;         //Dot-by-Dot��ԃ��[�h�ɂ���
            CacheSize = 100;                    //ver1.53 100MB

            //���[�v���邩�ǂ���
            //isLoopToTopPage = false;

            //�X�N���[���V���[����
            SlideShowTime = 3000;

            //��ʂ̏����ʒu
            IsWindowPosCenter = false;

            //�c�[���o�[�̈ʒu
            IsToolbarTop = true;

            //ver1.64�c�[���o�[�A�C�e���̕���������
            EraseToolbarItemString = false;

            //ver1.70 �T�C�h�o�[�̃X���[�X�X�N���[����On
            Sidebar_smoothScroll = true;

            //ver1.70 2���\���̓f�t�H���g�ŊȈՃ`�F�b�N
            //dualview_exactCheck = false;

            //ver1.71 �ŏI�y�[�W�̓���
            LastPage_stay = true;
            LastPage_toTop = false;
            //LastPage_toNextArchive = false;

            //ver1.73 �ꎞ�W�J�t�H���_
            TmpFolder = string.Empty;
            NumberOfMru = 10;

            //ver1.76 ���d�N��
            DisableMultipleStarts = false;
            //ver1.77 �E�B���h�E�ʒu���ȈՌv�Z�ɂ��邩
            SimpleCalcForWindowLocation = false;
            //ver1.77 �t���X�N���[����Ԃ𕜌��ł���悤�ɂ���
            SaveFullScreenMode = true;
            //ver1.78 �{���̕ێ�
            KeepMagnification = false;
            //ver1.79 ���ɂ͕K���W�J
            AlwaysExtractArchive = false;
            //ver1.79 2�y�[�W���[�h�A���S���Y��
            DualView_Force = false;
            DualView_Normal = true;
            DualView_withSizeCheck = false;

            //�_�u���N���b�N�@�\���J������
            DoubleClickToFullscreen = false;
            //ver1.81 �T���l�C���p�l���̃A�j���[�V����
            ThumbnailPanelSmoothScroll = true;

            //ver1.83 �A���V���[�v�}�X�N
            UseUnsharpMask = true;
            UnsharpDepth = 25;

            //ver1.91 �L�[�R���t�B�O 2022�N8��7��
            Keys.Init();

            //ver1.91 �}�E�X�R���t�B�O 2022�N8��7��
            Mouse.Init();

            //ver1.91 ���[�y�R���t�B�O 2022�N8��7��
            Loupe.Init();
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
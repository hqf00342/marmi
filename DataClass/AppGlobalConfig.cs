using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class AppGlobalConfig : INotifyPropertyChanged
    {
        //�R���t�B�O�t�@�C�����BXmlSerialize�ŗ��p
        private const string CONFIGNAME = "Marmi.xml";

        //�T�C�h�o�[�̊�{�̕�
        private const int SIDEBAR_INIT_WIDTH = 200;

        public bool dualView;                       //2��ʕ��ׂĕ\��
        public Size windowSize;                     //�E�B���h�E�T�C�Y
        public Point windowLocation;                //�E�B���h�E�\���ʒu
        //public MRU[] mru = new MRU[50];     //MRU���X�g�p�z��
        public List<MRU> mru = new List<MRU>();     //MRU���X�g�p�z��
        public bool visibleMenubar;                 //���j���[�o�[�̕\��
        public bool visibleToolBar;                 //�c�[���o�[�̕\��
        public bool visibleStatusBar;               //�X�e�[�^�X�o�[�̕\��
        public bool isSaveConfig;                   //�R���t�B�O�̕ۑ�

                                                    //public bool isSaveThumbnailCache;			//�T���l�C���L���b�V���̕ۑ�
        public bool isRecurseSearchDir;             //�f�B���N�g���̍ċA����

        public bool isReplaceArrowButton;               //�c�[���o�[�̍��E�{�^�������ւ���

        public bool isContinueZipView;              //zip�t�@�C���͑O��̑�������
        public bool isFitScreenAndImage;            //�摜�ƃC���[�W���t�B�b�g������
        public bool isStopPaintingAtResize;         //���T�C�Y���̕`�ʂ���߂�
        public int ThumbnailSize;                   //�T���l�C���摜�̑傫��
        public bool visibleNavibar;                 //�i�r�o�[�̕\��
        public bool isAutoCleanOldCache;            //�Â��L���b�V���̎����폜

        public bool isDrawThumbnailShadow;          // �T���l�C���ɉe��`�ʂ��邩
        public bool isDrawThumbnailFrame;           // �T���l�C���ɘg��`�ʂ��邩
        public bool isShowTPFileName;               // �T���l�C���Ƀt�@�C������\�����邩
        public bool isShowTPFileSize;               // �t�@�C�����Ƀt�@�C���T�C�Y��\�����邩
        public bool isShowTPPicSize;                // �t�@�C�����ɉ摜�T�C�Y��\�����邩

                                                    //
                                                    // ver1.35 ���������f��
        public MemoryModel memModel;                //�������[���f��

        public int CacheSize;                       //memModel == userDefined�̂Ƃ��̃L���b�V���T�C�Y[MB]
                                                    // ver1.35 �ŏI�y�[�W��擪�փ��[�v
                                                    //public bool isLoopToTopPage { get; set; }

        //���[�y�֘A
        public int loupeMagnifcant;                 //���[�y�{��

        public bool isOriginalSizeLoupe;            // ���[�y�������\���Ƃ��邩�ǂ����B

        public bool isFastDrawAtResize;             // �����`�ʂ����邩�ǂ���

        //�T�C�h�o�[�֘A
        //public bool isFixSidebar;					//�T�C�h�o�[���Œ�ɂ��邩�ǂ���
        public int sidebarWidth;                    //�T�C�h�o�[�̕�

        //ver1.09 ���Ɋ֘A
        public bool isExtractIfSolidArchive;        //�\���b�h���ɂȂ�ꎞ�W�J���邩

        //ver1.09 �N���X�t�F�[�h
        //public bool isCrossfadeTransition;			//��ʑJ�ڂŃN���X�t�F�[�h���邩

        //ver1.21 �L�[�R���t�B�O
        //1.81 �R�����g�A�E�g
        //public string keyConfNextPage;
        //public string keyConfPrevPage;
        //public string keyConfNextPageHalf;
        //public string keyConfPrevPageHalf;
        //public string keyConfTopPage;
        //public string keyConfLastPage;
        //public string keyConfFullScr;
        //public string keyConfPrintMode;
        //public string keyConfBookMark;
        //public string keyConfDualMode;
        //public string keyConfRecycleBin;
        //public string keyConfExitApp;	//ver1.77

        //ver1.24 �}�E�X�z�C�[��
        public string mouseConfigWheel;

        //ver1.25
        public bool noEnlargeOver100p;          //��ʃt�B�b�e�B���O��100%�����ɂ���

        public bool isDotByDotZoom;             //Dot-by-Dot��ԃ��[�h�ɂ���

        //ver1.21�摜�؂�ւ����@
        public AnimateMode pictureSwitchMode;

        ////ver1.35 �X�N���[���V���[����[ms]
        public int slideShowTime { get; set; }

        //ver1.42 �T���l�C���̃t�F�[�h�C��
        public bool isThumbFadein;

        //ver1.49 �E�B���h�E�̏����ʒu
        public bool isWindowPosCenter;

        //ver1.62 �c�[���o�[�̈ʒu
        public bool isToolbarTop;

        //ver1.64 ��ʃi�r�Q�[�V����.�E��ʃN���b�N�Ői��
        public bool RightScrClickIsNextPic;

        //ver1.64 ���Ԃ��{�ŃN���b�N�ʒu�t�]
        public bool ReverseDirectionWhenLeftBook;

        //ver1.65 �c�[���o�[�A�C�e���̕�����������
        public bool eraseToolbarItemString;

        //ver1.70 �T�C�h�o�[�̃X���[�X�X�N���[��
        public bool sidebar_smoothScroll;

        //ver1.70 2���\���̌��i��
        //public bool dualview_exactCheck;

        //ver1.71 �ŏI�y�[�W�̓���
        public bool lastPage_stay;

        public bool lastPage_toTop;
        public bool lastPage_toNextArchive;

        //ver1.73 �ꎞ�W�J�t�H���_
        public string tmpFolder;

        //ver1.73 MRU�ێ���
        public int numberOfMru;

        #region ���C����ʔw�i�F

        [XmlIgnore]
        public Color BackColor;

        [XmlElementAttribute("XmlMainBackColor")]
        public string xmlMainColor
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }

        #endregion ���C����ʔw�i�F

        #region �T���l�C���w�i�F

        [XmlIgnore]
        public Color ThumbnailBackColor;

        [XmlElementAttribute("XmlThumbnailBackColor")]
        public string xmlTbColor
        {
            set { ThumbnailBackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailBackColor); }
        }

        #endregion �T���l�C���w�i�F

        #region �T���l�C���p�t�H���g

        [XmlIgnore]
        public Font ThumbnailFont;

        [XmlElementAttribute("XmlThumbnailFont")]
        public string xmlTbFont
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

        #endregion �T���l�C���p�t�H���g

        #region �T���l�C���p�t�H���g�J���[

        [XmlIgnore]
        public Color ThumbnailFontColor;

        [XmlElementAttribute("XmlThumbnailFontColor")]
        public string xmlFontColor
        {
            set { ThumbnailFontColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailFontColor); }
        }

        #endregion �T���l�C���p�t�H���g�J���[

        //ver1.77 ��ʃ��[�h�ۑ��Ώۂɂ���B
        public bool isFullScreen;

        #region �ۑ����Ȃ��p�����[�^

        ////��ʃ��[�h
        //[XmlIgnore]
        //public bool isFullScreen;

        //�T���l�C�����[�h
        [XmlIgnore]
        public bool isThumbnailView;

        #endregion �ۑ����Ȃ��p�����[�^

        //ver1.76 ���d�N���֎~�t���O
        public bool disableMultipleStarts { get; set; }

        //ver1.77 ��ʕ\���ʒu�������ȈՂɂ��邩
        public bool simpleCalcForWindowLocation { get; set; }

        //ver1.77 �t���X�N���[����Ԃ𕜌��ł���悤�ɂ���
        public bool saveFullScreenMode { get; set; }

        //ver1.78 �{���̕ێ�
        public bool keepMagnification { get; set; }

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string s)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(s));
        }

        #endregion OnPropertyChanged

        //ver1.79 ���ɂ���ɓW�J���邩�ǂ���
        public bool AlwaysExtractArchive { get; set; }

        //ver1.79 2�y�[�W���[�h
        public bool dualView_Force { get; set; }

        public bool dualView_Normal { get; set; }
        public bool dualView_withSizeCheck { get; set; }

        //ver1.80 �L�[�R���t�B�O�Q
        public Keys ka_exit1 { get; set; }

        public Keys ka_exit2 { get; set; }
        public Keys ka_bookmark1 { get; set; }
        public Keys ka_bookmark2 { get; set; }
        public Keys ka_fullscreen1 { get; set; }
        public Keys ka_fullscreen2 { get; set; }
        public Keys ka_dualview1 { get; set; }
        public Keys ka_dualview2 { get; set; }
        public Keys ka_viewratio1 { get; set; }
        public Keys ka_viewratio2 { get; set; }
        public Keys ka_recycle1 { get; set; }
        public Keys ka_recycle2 { get; set; }

        public Keys ka_nextpage1 { get; set; }
        public Keys ka_nextpage2 { get; set; }
        public Keys ka_prevpage1 { get; set; }
        public Keys ka_prevpage2 { get; set; }
        public Keys ka_prevhalf1 { get; set; }
        public Keys ka_prevhalf2 { get; set; }
        public Keys ka_nexthalf1 { get; set; }
        public Keys ka_nexthalf2 { get; set; }
        public Keys ka_toppage1 { get; set; }
        public Keys ka_toppage2 { get; set; }
        public Keys ka_lastpage1 { get; set; }
        public Keys ka_lastpage2 { get; set; }

        //ver1.80 �_�u���N���b�N
        public bool DoubleClickToFullscreen { get; set; }

        public bool ThumbnailPanelSmoothScroll { get; set; }

        //ver1.83 �A���V���[�v�}�X�N
        public bool useUnsharpMask { get; set; }

        public int unsharpDepth { get; set; }

        /*******************************************************************************/

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public AppGlobalConfig()
        {
            Initialize();
        }

        //public AppGlobalConfig Clone()
        //{
        //	return MemberwiseClone() as AppGlobalConfig;
        //}

        /// <summary>
        /// �e�p�����[�^�̏����l
        /// </summary>
        private void Initialize()
        {
            visibleMenubar = true;
            visibleToolBar = true;
            visibleStatusBar = true;
            visibleNavibar = false;

            dualView = false;
            isFullScreen = false;
            isThumbnailView = false;
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            isSaveConfig = false;
            //isSaveThumbnailCache = false;
            isRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            BackColor = Color.LightSlateGray;
            isReplaceArrowButton = false;
            isFitScreenAndImage = true;

            isContinueZipView = false;
            isFitScreenAndImage = true;
            isStopPaintingAtResize = false;

            //�T���l�C���^�u
            ThumbnailSize = 200;                            //�T���l�C���T�C�Y
            ThumbnailBackColor = Color.White;               //�T���l�C���̃o�b�N�J���[
            ThumbnailFont = new Font("MS UI Gothic", 9);    //�T���l�C���̃t�H���g
            ThumbnailFontColor = Color.Black;               //�T���l�C���̃t�H���g�J���[
                                                            //isAutoCleanOldCache = false;					//�T���l�C���������ŃN���[�����邩
            isDrawThumbnailShadow = true;                   //�T���l�C���ɉe��`�ʂ��邩
            isDrawThumbnailFrame = true;                    //�T���l�C���ɘg��`�ʂ��邩
            isShowTPFileName = true;                        //�摜����\�����邩
            isShowTPFileSize = false;                       //�摜�̃t�@�C���T�C�Y��\�����邩
            isShowTPPicSize = false;                        //�摜�̃s�N�Z���T�C�Y��\�����邩
            isThumbFadein = true;

            //���[�y�^�u
            loupeMagnifcant = 3;
            isOriginalSizeLoupe = true;

            //�T�C�h�o�[
            //isFixSidebar = false;
            //sidebarWidth = ThumbnailSize + 50;
            sidebarWidth = SIDEBAR_INIT_WIDTH;

            //���x�Ȑݒ�
            isFastDrawAtResize = true;                      //���T�C�Y���ɍ����`�ʂ����邩�ǂ���
                                                            //����
            isExtractIfSolidArchive = true;
            //�N���X�t�F�[�h
            //isCrossfadeTransition = false;
            //�L�[�R���t�B�O
            //ver1.81�R�����g�A�E�g
            //keyConfNextPage ="��";
            //keyConfPrevPage = "��";
            //keyConfNextPageHalf = "PageDown";
            //keyConfPrevPageHalf = "PageUp";
            //keyConfTopPage = "Home";
            //keyConfLastPage = "End";
            //keyConfFullScr = "ESC";
            //keyConfPrintMode = "V";
            //keyConfBookMark = "B";
            //keyConfDualMode = "D";
            //keyConfRecycleBin = "Delete";
            //keyConfExitApp = "Q";
            //�}�E�X�R���t�B�O
            mouseConfigWheel = "�g��k��";
            // ��ʐ؂�ւ����[�h
            pictureSwitchMode = AnimateMode.Slide;
            //zoom
            noEnlargeOver100p = true;       //��ʃt�B�b�e�B���O��100%�����ɂ���
            isDotByDotZoom = false;         //Dot-by-Dot��ԃ��[�h�ɂ���
                                            //�������[���f��
                                            //memModel = MemoryModel.Small;	//�ŏ�
            memModel = MemoryModel.UserDefined; //�L���b�V�����p���[�h
            CacheSize = 100;                    //ver1.53 100MB
                                                //���[�v���邩�ǂ���
                                                //isLoopToTopPage = false;
                                                //�X�N���[���V���[����
            slideShowTime = 3000;
            //��ʂ̏����ʒu
            isWindowPosCenter = false;
            //�c�[���o�[�̈ʒu
            isToolbarTop = true;

            //ver1.64 ��ʃN���b�N�i�r�Q�[�V����
            RightScrClickIsNextPic = true;
            ReverseDirectionWhenLeftBook = true;

            //ver1.64�c�[���o�[�A�C�e���̕���������
            eraseToolbarItemString = false;

            //ver1.70 �T�C�h�o�[�̃X���[�X�X�N���[����On
            sidebar_smoothScroll = true;

            //ver1.70 2���\���̓f�t�H���g�ŊȈՃ`�F�b�N
            //dualview_exactCheck = false;

            //ver1.71 �ŏI�y�[�W�̓���
            lastPage_stay = true;
            lastPage_toTop = false;
            lastPage_toNextArchive = false;

            //ver1.73 �ꎞ�W�J�t�H���_
            tmpFolder = string.Empty;
            numberOfMru = 10;

            //ver1.76 ���d�N��
            disableMultipleStarts = false;
            //ver1.77 �E�B���h�E�ʒu���ȈՌv�Z�ɂ��邩
            simpleCalcForWindowLocation = false;
            //ver1.77 �t���X�N���[����Ԃ𕜌��ł���悤�ɂ���
            saveFullScreenMode = true;
            //ver1.78 �{���̕ێ�
            keepMagnification = false;
            //ver1.79 ���ɂ͕K���W�J
            AlwaysExtractArchive = false;
            //ver1.79 2�y�[�W���[�h�A���S���Y��
            dualView_Force = false;
            dualView_Normal = true;
            dualView_withSizeCheck = false;

            //1.80�L�[�R���t�B�O
            ka_exit1 = Keys.Q;
            ka_exit2 = Keys.None;
            ka_bookmark1 = Keys.B;
            ka_bookmark2 = Keys.None;
            ka_fullscreen1 = Keys.Escape;
            ka_fullscreen2 = Keys.None;
            ka_dualview1 = Keys.D;
            ka_dualview2 = Keys.None;
            ka_viewratio1 = Keys.V;
            ka_viewratio2 = Keys.None;
            ka_recycle1 = Keys.Delete;
            ka_recycle2 = Keys.None;
            //1.80�L�[�R���t�B�O �i�r�Q�[�V�����֘A
            ka_nextpage1 = Keys.Right;
            ka_nextpage2 = Keys.None;
            ka_prevpage1 = Keys.Left;
            ka_prevpage2 = Keys.None;
            ka_prevhalf1 = Keys.PageUp;
            ka_prevhalf2 = Keys.None;
            ka_nexthalf1 = Keys.PageDown;
            ka_nexthalf2 = Keys.None;
            ka_toppage1 = Keys.Home;
            ka_toppage2 = Keys.None;
            ka_lastpage1 = Keys.End;
            ka_lastpage2 = Keys.None;

            //�_�u���N���b�N�@�\���J������
            DoubleClickToFullscreen = false;
            //ver1.81 �T���l�C���p�l���̃A�j���[�V����
            ThumbnailPanelSmoothScroll = true;

            //ver1.83 �A���V���[�v�}�X�N
            useUnsharpMask = true;
            unsharpDepth = 25;
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string s)
        //{
        //	if (PropertyChanged != null)
        //		PropertyChanged(this, new PropertyChangedEventArgs(s));
        //}

        /// <summary>
        /// �R���t�B�O�̃t�@�C������Ԃ��B
        /// �v���p�e�B��������̂ŕs�v�Ȃ͂�
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static string getConfigFileName()
        {
            return Path.Combine(Application.StartupPath, CONFIGNAME);
        }

        public static string configFilename
        {
            get
            {
                return Path.Combine(Application.StartupPath, CONFIGNAME);
            }
        }

        /// <summary>
        /// XML�`���ŕۑ�����Object�����[�h����B
        /// </summary>
        /// <returns></returns>
        public static object LoadFromXmlFile()
        {
            //string path = getConfigFileName();
            string path = AppGlobalConfig.configFilename;

            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(AppGlobalConfig));

                    //�ǂݍ���ŋt�V���A��������
                    Object obj = xs.Deserialize(fs);
                    return obj;
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
            //string path = getConfigFileName();
            string path = AppGlobalConfig.configFilename;

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AppGlobalConfig));
                //�V���A�������ď�������
                xs.Serialize(fs, obj);
            }
        }
    }
}
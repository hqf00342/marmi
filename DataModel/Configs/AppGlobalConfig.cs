using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

/********************************************************************************
 �ݒ��ۑ�����N���X
XmlSerialize�����ݒ���Ǘ����Ă���B
********************************************************************************/

namespace Marmi
{
    [Serializable]
    public class AppGlobalConfig
    {
        public Size windowSize;                     //�E�B���h�E�T�C�Y
        public Point windowLocation;                //�E�B���h�E�\���ʒu

        /// <summary>
        /// 2��ʃ��[�h���ǂ����B
        /// �V���A���C�Y���邽�߂����ɑ���
        /// �ʏ��ViewState.DualView�ɃA�N�Z�X���邱�ƁB
        /// </summary>
        public bool DualView
        {
            get => ViewState.DualView;
            set => ViewState.DualView = value;
        }

        public bool IsRecurseSearchDir { get; set; }             //�f�B���N�g���̍ċA����

        public bool IsFitScreenAndImage { get; set; }            //�摜�ƃC���[�W���t�B�b�g������
        public bool IsStopPaintingAtResize { get; set; }         //���T�C�Y���̕`�ʂ���߂�
        public bool IsAutoCleanOldCache { get; set; }            //�Â��L���b�V���̎����폜
        public int SidebarWidth { get; set; }                    //�T�C�h�o�[�̕�

        ////ver1.35 �X�N���[���V���[����[ms]
        public int SlideShowTime { get; set; }

        //ver1.62 �c�[���o�[�̈ʒu
        public bool IsToolbarTop { get; set; }

        //ver1.78 �{���̕ێ�
        public bool KeepMagnification { get; set; }

        public GeneralConfig General { get; set; } = new GeneralConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public KeyConfig Keys { get; set; } = new KeyConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public AdvanceConfig Advance { get; set; } = new AdvanceConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        /// <summary>
        /// �X�N���[���L���b�V�����g�����ǂ���
        /// </summary>
        public bool UseScreenCache { get; set; } = false;

        /// <summary>
        /// MRU���X�g
        /// </summary>
        public List<MRU> Mru { get; set; } = new List<MRU>();

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
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            IsRecurseSearchDir = false;
            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;
            SidebarWidth = App.SIDEBAR_INIT_WIDTH;
            SlideShowTime = 3000;
            IsToolbarTop = true;
            KeepMagnification = false;

            General.Init();
            View.Init();
            Thumbnail.Init();
            Keys.Init();
            Mouse.Init();
            Loupe.Init();
            Advance.Init();
        }

        /// <summary>
        /// ���݉{�����Ă���g_pi.PackageName��MRU�ɒǉ�����
        /// �ȑO���������Ƃ�����ꍇ�A�{�����t�������X�V
        /// </summary>
        public void AddMRU(PackageInfo pi)
        {
            if (pi==null || string.IsNullOrEmpty(pi.PackageName))
                return;

            var mru = Mru.FirstOrDefault(a => a.Name == pi.PackageName);
            if (mru == null)
            {
                //�V�K�ǉ�
                Mru.Add(new MRU(pi.PackageName, DateTime.Now, pi.NowViewPage, pi.CreateBookmarkString()));
            }
            else
            {
                //�ߋ��f�[�^���X�V
                mru.Date = DateTime.Now;
                mru.LastViewPage = pi.NowViewPage;
                mru.Bookmarks = pi.CreateBookmarkString();
            }
            return;
        }
    }
}
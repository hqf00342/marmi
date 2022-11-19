using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

/********************************************************************************
 �ݒ��ۑ�����N���X
XmlSerialize�����ݒ���Ǘ����Ă���B
********************************************************************************/

namespace Marmi
{
    [Serializable]
    public class AppGlobalConfig
    {
        ///�E�B���h�E�T�C�Y
        public Size WindowSize { get; set; }

        ///�E�B���h�E�\���ʒu
        public Point WindowPos { get; set; }

        /// <summary>
        /// 2��ʃ��[�h���ǂ����B
        /// XML�V���A���C�Y���邽�߂����ɑ���
        /// �ʏ��ViewState.DualView�ɃA�N�Z�X���邱�ƁB
        /// </summary>
        public bool DualView
        {
            get => ViewState.DualView;
            set => ViewState.DualView = value;
        }

        //�f�B���N�g���̍ċA����
        public bool RecurseSearchDir { get; set; }

        ////ver1.35 �X�N���[���V���[����[ms]
        public int SlideshowTime { get; set; }

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
            Init();
        }

        /// <summary>
        /// �e�p�����[�^�̏����l
        /// </summary>
        private void Init()
        {
            WindowSize = new Size(640, 480);
            WindowPos = new Point(0, 0);
            RecurseSearchDir = false;
            SlideshowTime = 3000;

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
            if (pi == null || string.IsNullOrEmpty(pi.PackageName))
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

        public AppGlobalConfig Clone()
        {
            var xs = new XmlSerializer(typeof(AppGlobalConfig));
            using (var mem = new MemoryStream())
            {
                xs.Serialize(mem, this);
                mem.Position = 0;
                return (AppGlobalConfig)xs.Deserialize(mem);
            }
        }
    }
}
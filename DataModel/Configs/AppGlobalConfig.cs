/********************************************************************************
AppGlobalConfig
�ݒ��ۑ�����N���X
XmlSerialize�����ݒ���Ǘ����Ă���B
********************************************************************************/

using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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

        /// <summary>
        /// �f�B���N�g���̍ċA�������s����
        /// </summary>
        public bool RecurseSearchDir { get; set; }

        /// <summary>
        /// �摜�ƃC���[�W���t�B�b�g������
        /// </summary>
        public bool FitToScreen { get; set; }

        /// <summary>
        /// �T�C�h�o�[��
        /// </summary>
        public int SidebarWidth { get; set; }

        /// <summary>
        /// �X�N���[���V���[����[msec]
        /// </summary>
        public int SlideshowTime { get; set; }

        /// <summary>
        /// �c�[���o�[�̈ʒu.�㕔�Ȃ�true
        /// </summary>
        public bool ToolbarIsTop { get; set; }

        /// <summary>
        /// �\���{����ێ�����ꍇ��true
        /// </summary>
        public bool KeepMagnification { get; set; }

        /// <summary>
        /// OptionForm�_�C�A���O�́u�S�ʁv�^�u�pConfig
        /// </summary>
        public GeneralConfig General { get; set; } = new GeneralConfig();

        /// <summary>
        /// OptionForm�_�C�A���O�́u�\���v�^�u�pConfig
        /// </summary>
        public ViewConfig View { get; set; } = new ViewConfig();

        /// <summary>
        /// OptionForm�_�C�A���O�́u�L�[�R���t�B�O�v�^�u�pConfig
        /// </summary>
        public KeyConfig Keys { get; set; } = new KeyConfig();

        /// <summary>
        /// OptionForm�_�C�A���O�́u�}�E�X�v�^�u�pConfig
        /// </summary>
        public MouseConfig Mouse { get; set; } = new MouseConfig();

        /// <summary>
        /// OptionForm�_�C�A���O�́u���[�y�v�^�u�pConfig
        /// </summary>
        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        /// <summary>
        /// OptionForm�_�C�A���O�́u���x�Ȑݒ�v�^�u�pConfig
        /// </summary>
        public AdvanceConfig Advance { get; set; } = new AdvanceConfig();

        /// <summary>
        /// OptionForm�_�C�A���O�́u�T���l�C���v�^�u�pConfig
        /// </summary>
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
        /// �R���X�g���N�^�B
        /// �e�p�����[�^�������l�ɂ���B
        /// </summary>
        public AppGlobalConfig()
        {
            Initialize();
        }

        /// <summary>
        /// �S�v���p�e�B�������l�ɐݒ�
        /// </summary>
        private void Initialize()
        {
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            RecurseSearchDir = false;
            FitToScreen = true;
            SidebarWidth = App.SIDEBAR_INIT_WIDTH;
            SlideshowTime = 3000;
            ToolbarIsTop = true;
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

        /// <summary>
        /// ���̃I�u�W�F�N�g���f�B�[�v�R�s�[��Clone����B
        /// </summary>
        /// <returns></returns>
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
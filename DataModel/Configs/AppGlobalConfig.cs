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
        public Size windowSize;                     //�E�B���h�E�T�C�Y
        public Point windowLocation;                //�E�B���h�E�\���ʒu

        public List<MRU> Mru { get; set; } = new List<MRU>();     //MRU���X�g�p�z��

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

        #region UI�Ȃ�

        public bool UseScreenCache { get; set; } = false;   //�X�N���[���L���b�V�����g�����ǂ���

        #endregion UI�Ȃ�

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
            //isSaveThumbnailCache = false;
            IsRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            IsFitScreenAndImage = true;

            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;

            //�T�C�h�o�[
            SidebarWidth = App.SIDEBAR_INIT_WIDTH;

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
            string path = App.ConfigFilename;

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
            string path = App.ConfigFilename;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var xs = new XmlSerializer(typeof(AppGlobalConfig));
                xs.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// ���݉{�����Ă���g_pi.PackageName��MRU�ɒǉ�����
        /// �ȑO���������Ƃ�����ꍇ�A�{�����t�������X�V
        /// </summary>
        public void UpdateMRUList(PackageInfo pi)
        {
            if (string.IsNullOrEmpty(pi.PackageName))
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
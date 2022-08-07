using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    public class GeneralConfig
    {
        //コンフィグの保存
        public bool IsSaveConfig { get; set; }

        #region archive

        //書庫は前回の続きから
        public bool IsContinueZipView { get; set; }

        //ver1.79 書庫を常に展開する
        public bool AlwaysExtractArchive { get; set; }

        //ver1.09 ソリッド書庫を一時展開
        public bool IsExtractIfSolidArchive { get; set; }

        //ver1.73 一時展開フォルダ
        public string TmpFolder { get; set; }

        #endregion

        //ツールバーの左右ボタンを入れ替える
        public bool IsReplaceArrowButton { get; set; }

        //ver1.65 ツールバーアイテムの文字を消すか
        public bool EraseToolbarItemString { get; set; }
        //ver1.70 サイドバーのスムーススクロール
        public bool Sidebar_smoothScroll { get; set; }

        //ver1.73 MRU保持数
        public int NumberOfMru;

        [XmlIgnore]
        public Color BackColor;

        [XmlElement("XmlMainBackColor")]
        public string XmlMainColor
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }

        //ver1.76 多重起動禁止
        public bool DisableMultipleStarts { get; set; }

        //ver1.77 ウィンドウ位置復元を簡易にする
        public bool SimpleCalcForWindowLocation { get; set; }
        //ver1.77 フルスクリーン状態を復元できるようにする
        public bool SaveFullScreenMode { get; set; }
        //ver1.49 ウィンドウの初期位置
        public bool IsWindowPosCenter { get; set; }

        public void Init()
        {
            IsSaveConfig = false;
            BackColor = Color.LightSlateGray;
            IsReplaceArrowButton = false;
            IsContinueZipView = false;
            IsExtractIfSolidArchive = true;
            IsWindowPosCenter = false;
            EraseToolbarItemString = false;
            Sidebar_smoothScroll = true;
            //ver1.73 一時展開フォルダ
            TmpFolder = string.Empty;
            NumberOfMru = 10;

            //ver1.76 多重起動
            DisableMultipleStarts = false;
            //ver1.77 ウィンドウ位置を簡易計算にするか
            SimpleCalcForWindowLocation = false;
            //ver1.77 フルスクリーン状態を復元できるようにする
            SaveFullScreenMode = true;
            AlwaysExtractArchive = false;

        }
    }
}

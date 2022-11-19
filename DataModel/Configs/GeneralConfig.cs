using Marmi.Interfaces;
using System.Drawing;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    public class GeneralConfig : IConfig
    {
        //コンフィグの保存
        public bool SaveConfig { get; set; }

        //書庫は前回の続きから
        public bool ContinueReading { get; set; }

        //ver1.79 書庫を常に展開する
        public bool ExtractArchiveAlways { get; set; }

        //ver1.09 ソリッド書庫を一時展開
        public bool ExtractArchiveIfSolid { get; set; }

        //ver1.73 一時展開フォルダ
        public string TmpFolder { get; set; }

        //ツールバーの左右ボタンを入れ替える
        public bool ReplaceArrowButton { get; set; }

        //ver1.65 ツールバーアイテムの文字を消すか
        public bool HideToolbarString { get; set; }

        //ver1.70 サイドバーのスムーススクロール
        public bool SmoothScrollOnSidebar { get; set; }

        //ver1.73 MRU保持数
        public int MaxMruNumber { get; set; }

        [XmlIgnore]
        public Color BackColor { get; set; } = Color.LightSlateGray;

        [XmlElement("BackColorXml")]
        public string BackColorXml
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }

        /// <summary>
        /// ver1.76 多重起動禁止
        /// </summary>
        public bool SingleProcess { get; set; }

        /// <summary>
        /// ver1.77 フルスクリーン状態を復元できるようにする
        /// </summary>
        public bool SaveFullScreenMode { get; set; }

        //ver1.49 ウィンドウの初期位置を中央にする
        public bool CenteredAtStart { get; set; }

        public string TESTA { get; set; }
        public int TESTB { get; set; }

        public void Init()
        {
            SaveConfig = false;
            BackColor = Color.LightSlateGray;
            ReplaceArrowButton = false;
            ContinueReading = true;
            ExtractArchiveIfSolid = true;
            CenteredAtStart = false;
            HideToolbarString = false;
            SmoothScrollOnSidebar = true;
            TmpFolder = string.Empty;
            MaxMruNumber = 10;
            SingleProcess = false;
            SaveFullScreenMode = true;
            ExtractArchiveAlways = false;
        }

        public GeneralConfig Clone()
        {
            var x = (GeneralConfig)this.MemberwiseClone();
            x.BackColor = this.BackColor;
            return x;
        }
    }
}
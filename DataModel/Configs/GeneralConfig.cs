using Marmi.Interfaces;
using System.Drawing;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    public class GeneralConfig : IConfig
    {
        /// <summary>
        /// コンフィグの保存
        /// </summary>
        public bool SaveConfig { get; set; } = true;

        /// <summary>
        /// 書庫は前回の続きから
        /// </summary>
        public bool ContinueReading { get; set; } = true;

        /// <summary>
        /// 書庫を常に展開する
        /// </summary>
        public bool ExtractArchiveAlways { get; set; } = false;

        /// <summary>
        /// ソリッド書庫を一時展開
        /// </summary>
        public bool ExtractArchiveIfSolid { get; set; } = true;

        /// <summary>
        /// 一時展開フォルダ
        /// </summary>
        public string TmpFolder { get; set; } = string.Empty;

        /// <summary>
        /// ツールバーの左右ボタンを入れ替える
        /// </summary>
        public bool ReplaceArrowButton { get; set; } = false;

        /// <summary>
        /// ツールバーアイテムの文字を消す
        /// </summary>
        public bool HideToolbarString { get; set; } = false;

        /// <summary>
        /// サイドバーのスムーススクロール
        /// </summary>
        public bool SmoothScrollOnSidebar { get; set; } = true;

        /// <summary>
        /// MRU保持数
        /// </summary>
        public int MaxMruNumber { get; set; } = 20;

        /// <summary>
        /// 背景色
        /// </summary>
        [XmlIgnore]
        public Color BackColor { get; set; } = Color.LightSlateGray;

        /// <summary>
        /// 背景色XML値
        /// </summary>
        [XmlElement("BackColorXml")]
        public string BackColorXml
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }

        /// <summary>
        /// ver1.76 多重起動禁止
        /// </summary>
        public bool SingleProcess { get; set; } = false;

        /// <summary>
        /// ver1.77 フルスクリーン状態を復元できるようにする
        /// </summary>
        public bool SaveFullScreenMode { get; set; } = true;

        //ver1.49 ウィンドウの初期位置を中央にする
        public bool CenteredAtStart { get; set; } = false;

        public string TESTA { get; set; }
        public int TESTB { get; set; }

        public void Init()
        {
            SaveConfig = true;
            BackColor = Color.LightSlateGray;
            ReplaceArrowButton = false;
            ContinueReading = true;
            ExtractArchiveIfSolid = true;
            CenteredAtStart = false;
            HideToolbarString = false;
            SmoothScrollOnSidebar = true;
            TmpFolder = string.Empty;
            MaxMruNumber = 20;
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
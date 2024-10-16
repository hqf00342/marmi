﻿/********************************************************************************
GlobalConfig
一般設定

2024年10月13日
BindableBase派生にしINotifyPropertyChangedに対応する。
しかし、全部をINotifyPropertyChangedにするのは面倒なので
必要なとき(初期値に戻す)にOnPropertyChangedを手動呼び出しする。

********************************************************************************/

using Marmi.Interfaces;
using Mii;
using System.Drawing;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    public class GeneralConfig : BindableBase, IConfig
    {
        /// <summary>
        /// 終了時にコンフィグ保存するかどうか
        /// </summary>
        public bool SaveConfig { get; set; } = true;

        /// <summary>
        /// 書庫Open時に前回の続きページから開くか
        /// </summary>
        public bool ContinueReading { get; set; } = true;

        /// <summary>
        /// 書庫を常にファイルへ一時展開するか堂か
        /// </summary>
        public bool ExtractArchiveAlways { get; set; } = false;

        /// <summary>
        /// ソリッド書庫は常に一時展開するか堂か
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
        /// Marmiの多重起動禁止(ver1.76)
        /// </summary>
        public bool SingleProcess { get; set; } = false;

        /// <summary>
        /// ver1.77 フルスクリーン状態を復元できるようにする
        /// </summary>
        public bool FullScreenWhenStartup { get; set; } = true;

        //ver1.49 ウィンドウの初期位置を中央にする
        public bool CenteredAtStart { get; set; } = false;

        /// <summary>複数ページ進む/戻るのページ数</summary>
        public int MultiPageNavigationCount { get; set; } = 10;

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
            FullScreenWhenStartup = false;
            ExtractArchiveAlways = false;
            MultiPageNavigationCount = 10;

            //WinFormsのデータバインド機構は
            //1つPropertyChangedを投げると全部チェックしてくれるため
            //1つだけ投げる
            OnPropertyChanged(nameof(SaveConfig));
        }

        public GeneralConfig Clone()
        {
            var x = (GeneralConfig)this.MemberwiseClone();
            x.BackColor = this.BackColor;
            return x;
        }
    }
}
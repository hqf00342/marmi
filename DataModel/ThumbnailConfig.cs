using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    [Serializable]
    public class ThumbnailConfig
    {
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


        //サムネイル画像の大きさ
        public int ThumbnailSize;

        // サムネイルに影を描写するか
        public bool IsDrawThumbnailShadow { get; set; }

        // サムネイルに枠を描写するか
        public bool IsDrawThumbnailFrame { get; set; }

        // サムネイルにファイル名を表示するか
        public bool IsShowTPFileName { get; set; }

        // ファイル名にファイルサイズを表示するか
        public bool IsShowTPFileSize { get; set; }

        // ファイル名に画像サイズを表示するか
        public bool IsShowTPPicSize { get; set; }

        [Obsolete]
        public bool IsThumbFadein { get; set; }

        public bool ThumbnailPanelSmoothScroll { get; set; }

        //public bool isSaveThumbnailCache;			//サムネイルキャッシュの保存


        public void Init()
        {
            //サムネイルタブ
            ThumbnailSize = 200;                            //サムネイルサイズ
            ThumbnailBackColor = Color.White;               //サムネイルのバックカラー
            ThumbnailFont = new Font("MS UI Gothic", 9);    //サムネイルのフォント
            ThumbnailFontColor = Color.Black;               //サムネイルのフォントカラー
                                                            //isAutoCleanOldCache = false;					//サムネイルを自動でクリーンするか
            IsDrawThumbnailShadow = true;                   //サムネイルに影を描写するか
            IsDrawThumbnailFrame = true;                    //サムネイルに枠を描写するか
            IsShowTPFileName = true;                        //画像名を表示するか
            IsShowTPFileSize = false;                       //画像のファイルサイズを表示するか
            IsShowTPPicSize = false;                        //画像のピクセルサイズを表示するか
            //IsThumbFadein = false;
            //ver1.81 サムネイルパネルのアニメーション
            ThumbnailPanelSmoothScroll = true;

        }

    }
}

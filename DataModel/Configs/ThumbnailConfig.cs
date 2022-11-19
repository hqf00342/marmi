using Marmi.Interfaces;
using System;
using System.Drawing;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    [Serializable]
    public class ThumbnailConfig : IConfig
    {
        [XmlIgnore]
        public Color ThumbnailBackColor;

        [XmlElement("XmlThumbnailBackColor")]
        public string XmlBackColor
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
        public int ThumbnailSize { get; set; }

        // サムネイルに影を描写するか
        public bool DrawShadowdrop { get; set; }

        // サムネイルに枠を描写するか
        public bool DrawFrame { get; set; }

        // サムネイルにファイル名を表示するか
        public bool DrawFilename { get; set; }

        // ファイル名にファイルサイズを表示するか
        public bool DrawFilesize { get; set; }

        // ファイル名に画像サイズを表示するか
        public bool DrawPicsize { get; set; }

        public bool SmoothScroll { get; set; }

        public void Init()
        {
            //サムネイルタブ
            ThumbnailSize = 200;                            //サムネイルサイズ
            ThumbnailBackColor = Color.White;               //サムネイルのバックカラー
            ThumbnailFont = new Font("MS UI Gothic", 9);    //サムネイルのフォント
            ThumbnailFontColor = Color.Black;               //サムネイルのフォントカラー
                                                            //isAutoCleanOldCache = false;					//サムネイルを自動でクリーンするか
            DrawShadowdrop = true;                   //サムネイルに影を描写するか
            DrawFrame = true;                    //サムネイルに枠を描写するか
            DrawFilename = true;                        //画像名を表示するか
            DrawFilesize = false;                       //画像のファイルサイズを表示するか
            DrawPicsize = false;                        //画像のピクセルサイズを表示するか
            SmoothScroll = true;
        }
    }
}
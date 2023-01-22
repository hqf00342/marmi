/*
ThumbnalConfig
サムネイル用のコンフィグ
*/

using System;
using System.Drawing;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    [Serializable]
    public class ThumbnailConfig
    {
        /// <summary>
        /// サムネイルの背景色
        /// </summary>
        [XmlIgnore]
        public Color ThumbnailBackColor;

        /// <summary>
        /// サムネイルの背景色(XML)
        /// </summary>
        [XmlElement("XmlThumbnailBackColor")]
        public string XmlBackColor
        {
            set { ThumbnailBackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailBackColor); }
        }

        /// <summary>
        /// サムネイルのフォント
        /// </summary>
        [XmlIgnore]
        public Font ThumbnailFont;

        /// <summary>
        /// サムネイルのフォント(XML)
        /// </summary>
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

        /// <summary>
        /// サムネイルのフォント色
        /// </summary>
        [XmlIgnore]
        public Color ThumbnailFontColor;

        /// <summary>
        /// サムネイルのフォント色(XML)
        /// </summary>
        [XmlElement("XmlThumbnailFontColor")]
        public string XmlFontColor
        {
            set { ThumbnailFontColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailFontColor); }
        }

        /// <summary>
        /// サムネイル画像の大きさ
        /// </summary>
        public int ThumbnailSize { get; set; }

        /// <summary>
        /// サムネイルに影を描写
        /// </summary>
        public bool DrawShadowdrop { get; set; }

        /// <summary>
        /// サムネイルに枠を描写
        /// </summary>
        public bool DrawFrame { get; set; }

        /// <summary>
        /// サムネイルにファイル名を表示
        /// </summary>
        public bool DrawFilename { get; set; }

        /// <summary>
        /// ファイルサイズを表示
        /// </summary>
        public bool DrawFilesize { get; set; }

        /// <summary>
        /// ファイル名に画像サイズを表示するか
        /// </summary>
        public bool DrawPicsize { get; set; }

        /// <summary>
        /// サムネイルでスムーススクロールをする
        /// </summary>
        public bool SmoothScroll { get; set; }

        public void Init()
        {
            //サムネイルタブ
            ThumbnailSize = 200;
            ThumbnailBackColor = Color.White;
            ThumbnailFont = new Font("MS UI Gothic", 9);
            ThumbnailFontColor = Color.Black;
            DrawShadowdrop = true;
            DrawFrame = true;
            DrawFilename = true;
            DrawFilesize = false;
            DrawPicsize = false;
            SmoothScroll = true;
        }
    }
}
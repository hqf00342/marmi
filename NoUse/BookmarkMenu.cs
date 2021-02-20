using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Marmi
{
    public partial class BookmarkMenu : UserControl
    {
        Bitmap thumbnail = null;
        string filename = string.Empty;
        int orgIndex = 0;
        int pagenumber = 0;
        private readonly int thumbnailSize = 40;

        public BookmarkMenu()
        {
            InitializeComponent();
        }

        public BookmarkMenu(ImageInfo ii, int index) : this()
        {
            //thumbnail = BitmapUty.MakeHeightFixThumbnailImage(ii.thumbnail, 48);
            //thumbnail = BitmapUty.MakeThumbnailImage(ii.thumbnail, 48);
            thumbnail = BitmapUty.MakeSquareThumbnailImage(ii.thumbnail, thumbnailSize);
            filename = ii.filename;
            orgIndex = ii.nOrgIndex;
            pagenumber = index;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            //Rectangle r = this.ClientRectangle;
            //r.Width--;
            //r.Height--;

            BackColor = SystemColors.MenuHighlight;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //画像があれば描写
            if (thumbnail != null)
                e.Graphics.DrawImage(thumbnail, 1, 1);
            //else
            {
                //枠線描写
                Rectangle r = new Rectangle(1, 1, thumbnailSize, thumbnailSize);
                e.Graphics.DrawRectangle(Pens.Gray, r);
            }

            //ページ番号を描写
            e.Graphics.DrawString(
                pagenumber.ToString(),
                SystemFonts.MenuFont,
                Brushes.Blue,
                new PointF(thumbnailSize + 2, 4));
            SizeF size = e.Graphics.MeasureString(pagenumber.ToString(), SystemFonts.MenuFont);

            //ファイル名を描写 文字列が表示しきれないときに"..."を表示する
            StringFormat sf = new StringFormat();
            sf.Trimming = StringTrimming.EllipsisPath;
            e.Graphics.DrawString(
                Path.GetFileName(filename),
                SystemFonts.MenuFont,
                Brushes.Black,
                new PointF(2 + thumbnailSize, 4 + size.Height),
                sf);
        }
    }
}
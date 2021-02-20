using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public class FormPictureInfoWhite : Form
    {
        private Point m_mouseDownPoint;     //マウスクリック（Down）されたときのマウス位置（クライアント座標）
        private bool m_formMove;            //マウスアップしたときのフラグ。フォームが移動したか。
        private ImageInfo ImgInfo = null;
        private Timer timer = new Timer();

        private const int PADDING = 10;         //定数：画像の上下パディング
        private const int THUMBSIZE = 120;      //定数：サムネイルサイズ
        private const int LINEPADDING = 2;      //定数：行間
        private const int FORMWIDTH = 480;      //定数：フォームの幅
        private const int FORMHEIGHT = PADDING * 2 + THUMBSIZE;     //定数：フォームの高さ
        private Font fontL = new Font("ＭＳ Ｐ ゴシック", 12F, FontStyle.Bold);
        private Font fontS = new Font("ＭＳ Ｐ ゴシック", 10F);

        #region public

        public FormPictureInfoWhite(ImageInfo imgInfo)
        {
            InitializeComponent();
            ImgInfo = imgInfo;
        }

        public void Show(Form parent) //(Form parent, ImageInfo imgInfo)
        {
            this.Parent = Parent;
            SetFormLocation(parent, DiagLocate.Right, DiagLocate.Bottom);
            this.Opacity = 0F;
            this.Show(this.Parent);
            this.Refresh();     //Invalidate()じゃだめ

            for (double o = 0.1F; o <= 1.0F; o += 0.05F)
            {
                this.Opacity = o;
                System.Threading.Thread.Sleep(5);
            }

            timer.Interval = 2000;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                this.Close();
                //FadeOut();
            };
            timer.Start();
        }

        #endregion public

        #region Event Override

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && !m_formMove)
                this.Close();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                this.SetDesktopLocation(
                    this.Left - m_mouseDownPoint.X + e.X,
                    this.Top - m_mouseDownPoint.Y + e.Y);

                //フォーカスが当たるとき必ずMouseDown->MouseMove->MouseUpになる模様
                //本当にDragしたかチェックする
                if (m_mouseDownPoint.X != e.X || m_mouseDownPoint.Y != e.Y)
                    m_formMove = true;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                m_mouseDownPoint = new Point(e.X, e.Y);
                m_formMove = false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            //オーナーが閉じるときはフェードアウトしない
            if (e.CloseReason == CloseReason.FormOwnerClosing)
                return;
            else
                FadeOut();
        }

        #endregion Event Override

        #region private Drawing Method

        private void Draw(Graphics g)
        {
            //背景色
            Rectangle drawRect = this.ClientRectangle;
            //g.FillRectangle(Brushes.Black, drawRect);
            g.Clear(Color.White);

            using (Pen p = new Pen(Color.FromArgb(128, 128, 128, 128)))
            {
                //枠を書く
                Rectangle rc = drawRect; // this.ClientRectangle;
                rc.Width--;
                rc.Height--;
                rc.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightGray, rc);
            }

            if (ImgInfo == null)
                return;

            //文字高さの確認
            string sz = string.Format("9999");
            SizeF size = g.MeasureString(sz, fontL);
            int HeightL = (int)size.Height;
            size = g.MeasureString(sz, fontS);
            int HeightS = (int)size.Height;

            //StringFormatを作っておく
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.EllipsisPath;  //表示しきれないときは・・・表記

            //画像の描写
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            drawRect.Inflate(-PADDING, -PADDING);
            int x = drawRect.X;
            int y = drawRect.Y;
            if (ImgInfo.thumbnail != null)
            {
                //サムネイルサイズが120ではないので倍率を計算
                float mag = BitmapUty.GetMagnificationWithFixAspectRatio(ImgInfo.thumbnail.Size, THUMBSIZE);
                int ThumbWidth = (int)(ImgInfo.thumbnail.Width * mag);
                int ThumbHeight = (int)(ImgInfo.thumbnail.Height * mag);

                Rectangle imgRect = new Rectangle(
                    x + (THUMBSIZE - ThumbWidth) / 2,     // X位置（左上の）
                    y + (THUMBSIZE - ThumbHeight) / 2,      // Y位置（左上の）
                    ThumbWidth,
                    ThumbHeight
                    );
                g.DrawImage(ImgInfo.thumbnail, imgRect);
                //x + (THUMBSIZE - ThumbWidth) / 2,     // X位置（左上の）
                //y + (THUMBSIZE - ThumbHeight) / 2,      // Y位置（左上の）
                //ThumbWidth,
                //ThumbHeight
                //);

                imgRect.Inflate(1, 1);
                g.DrawRectangle(Pens.Gray, imgRect);
            }

            //文字の描写:ファイル名
            x += THUMBSIZE + PADDING;
            sz = string.Format("{0}", Path.GetFileName(ImgInfo.filename));
            size = g.MeasureString(sz, fontL, drawRect.Width - x, sf);
            Rectangle FilenameRect = new Rectangle(
                x, y,
                (int)Math.Ceiling(size.Width),  //小数点以下切り上げ
                (int)Math.Ceiling(size.Height)  //小数点以下切り上げ
                );
            g.DrawString(sz, fontL, Brushes.SteelBlue, FilenameRect, sf);
            y += (int)size.Height + LINEPADDING;    //2行分
            x += 10;

            ////文字の描写:サイズ
            sz = string.Format("{0:N0}bytes", ImgInfo.length);
            g.DrawString(sz, fontS, Brushes.Black, x, y);
            y += HeightS + LINEPADDING;

            //文字の描写:ピクセル数
            sz = string.Format(
                "{0:N0} x {1:N0} pixels",
                ImgInfo.width,
                ImgInfo.height
                );
            g.DrawString(sz, fontS, Brushes.Black, x, y);
            y += HeightS + LINEPADDING;

            //文字の描写:ファイルパス
            sz = Path.GetDirectoryName(ImgInfo.filename);
            if (!string.IsNullOrEmpty(sz))
            {
                //描写サイズを確認
                size = g.MeasureString(sz, fontS, drawRect.Width - x, sf);
                Rectangle rc = new Rectangle(x, y, drawRect.Width - x, HeightS * 2);
                g.DrawString(sz, fontS, Brushes.Black, rc, sf);
                y += (int)size.Height + LINEPADDING;
            }

            //文字の描写:日付
            sz = string.Format("{0}", ImgInfo.createDate);
            g.DrawString(sz, fontS, Brushes.DarkGray, x, y);
            y += HeightS + LINEPADDING;

            //sz = string.Format(
            //    "{0:N0} x {1:N0} pixels, {2:N0}bytes",
            //    ImgInfo.originalWidth,
            //    ImgInfo.originalHeight,
            //    ImgInfo.length);
            //g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
            //y += HeightS + LINEPADDING;

            //文字の描写:Exifその他
            sz = string.Format(
                "{0} {1}",
                ImgInfo.ExifMake,
                ImgInfo.ExifModel);
            if (ImgInfo.ExifISO != 0)
                sz = string.Format("ISO={0} {1}", ImgInfo.ExifISO, sz);
            g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
        }

        public void FadeOut()
        {
            //フェードアウト処理
            double opa = this.Opacity;
            for (double o = opa; o > 0; o -= 0.02F)
            {
                this.Opacity = o;
                System.Threading.Thread.Sleep(5);
            }
            this.Visible = false;
        }

        #endregion private Drawing Method

        private void SetFormLocation(Form parent, DiagLocate LX, DiagLocate LY)
        {
            Rectangle pRect = ((Form1)parent).RectangleToScreen(((Form1)parent).GetClientRectangle());
            switch (LX)
            {
                case DiagLocate.Left:
                    this.Left = pRect.Left;
                    break;

                case DiagLocate.Right:
                    this.Left = pRect.Right - this.Width;
                    break;

                case DiagLocate.Middle:
                default:
                    this.Left = pRect.Left + (pRect.Width - this.Width) / 2;
                    break;
            }

            switch (LY)
            {
                case DiagLocate.Top:
                    this.Top = pRect.Top;
                    break;

                case DiagLocate.Bottom:
                    this.Top = pRect.Bottom - this.Height;
                    break;

                case DiagLocate.Middle:
                default:
                    this.Top = pRect.Top + (pRect.Height - this.Height) / 2;
                    break;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Name = "PictureInfo";
            this.Text = "PictureInfo";
            this.Opacity = 0F;
            this.Width = FORMWIDTH;
            this.Height = FORMHEIGHT;
            this.BackColor = System.Drawing.Color.Black;

            this.ShowInTaskbar = false;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(FORMWIDTH, FORMHEIGHT);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.StartPosition = FormStartPosition.Manual;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint, true);

            this.ResumeLayout(false);
        }
    }
}
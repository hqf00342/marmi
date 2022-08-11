using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;					//ThreadPool, WaitCallback
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormPictureInfo : Form
    {
        private BufferedGraphics m_offScreen = null;
        private readonly BufferedGraphicsContext m_offScreenContext = null;
        private Point m_mouseDownPoint;     //�}�E�X�N���b�N�iDown�j���ꂽ�Ƃ��̃}�E�X�ʒu�i�N���C�A���g���W�j
        private bool m_formMove;            //�}�E�X�A�b�v�����Ƃ��̃t���O�B�t�H�[�����ړ��������B

        private const int PADDING = 10;         //�萔�F�摜�̏㉺�p�f�B���O
        private const int THUMBSIZE = 120;      //�萔�F�T���l�C���T�C�Y
        private const int LINEPADDING = 2;      //�萔�F�s��
        private const int FORMWIDTH = 480;      //�萔�F�t�H�[���̕�
        private const int FORMHEIGHT = PADDING * 2 + THUMBSIZE;     //�萔�F�t�H�[���̍���

        //private const uint WM_MOUSEACTIVATE = 0x21;
        //private const uint MA_ACTIVATE = 1;
        //private const uint MA_ACTIVATEANDEAT = 2;

        private readonly Font fontL = new Font("�l�r �o �S�V�b�N", 12F, FontStyle.Bold);
        private readonly Font fontS = new Font("�l�r �o �S�V�b�N", 9F);

        public FormPictureInfo()
        {
            InitializeComponent();

            if (m_offScreenContext == null)
                m_offScreenContext = BufferedGraphicsManager.Current;
            m_offScreen = m_offScreenContext.Allocate(this.CreateGraphics(), this.ClientRectangle);

            this.BackColor = Color.Black;
            this.Opacity = 0F;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint, true);

            this.ShowInTaskbar = false;
            this.Width = FORMWIDTH;
            this.Height = FORMHEIGHT;
        }

        private void PictureInfo_Paint(object sender, PaintEventArgs e)
        {
            m_offScreen.Render(e.Graphics);
        }

        private void PictureInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            //�I�[�i�[������Ƃ��̓t�F�[�h�A�E�g���Ȃ�
            if (e.CloseReason == CloseReason.FormOwnerClosing)
                return;

            FadeOut();
        }

        private void PictureInfo_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Debug.WriteLine(e.Clicks, "MouseMove");
                this.SetDesktopLocation(
                    this.Left - m_mouseDownPoint.X + e.X,
                    this.Top - m_mouseDownPoint.Y + e.Y);

                //�t�H�[�J�X��������Ƃ��K��MouseDown->MouseMove->MouseUp�ɂȂ�͗l
                //�{����Drag�������`�F�b�N����
                if (m_mouseDownPoint.X != e.X || m_mouseDownPoint.Y != e.Y)
                    m_formMove = true;
            }
        }

        private void PictureInfo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_mouseDownPoint = new Point(e.X, e.Y);
                m_formMove = false;
            }
        }

        private void PictureInfo_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !m_formMove)
                this.Close();
        }

        private void DrawImageInfo(Graphics g, ImageInfo imgInfo, Rectangle drawRect)
        {
            //�w�i�F
            //Rectangle drawRect = this.ClientRectangle;
            g.FillRectangle(Brushes.Black, drawRect);

            //�t���[��
            //drawRect.Width -= 1;
            //drawRect.Height -= 1;
            //g.DrawRectangle(Pens.DarkGray, drawRect);

            if (imgInfo == null)
                return;

            //���������̊m�F
            string sz = string.Format("9999");
            SizeF size = g.MeasureString(sz, fontL);
            int HeightL = (int)size.Height;
            size = g.MeasureString(sz, fontS);
            int HeightS = (int)size.Height;
            //g.DrawString(sz, fontS, Brushes.DarkGray, x, y);

            //StringFormat������Ă���
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.EllipsisPath;  //�\��������Ȃ��Ƃ��́E�E�E�\�L

            //�摜�̕`��
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            drawRect.Inflate(-PADDING, -PADDING);
            int x = drawRect.X;
            int y = drawRect.Y;
            if (imgInfo.Thumbnail != null)
            {
                //�T���l�C���T�C�Y��120�ł͂Ȃ��̂Ŕ{�����v�Z
                float mag = BitmapUty.GetMagnificationWithFixAspectRatio(imgInfo.Thumbnail.Size, THUMBSIZE);
                int ThumbWidth = (int)(imgInfo.Thumbnail.Width * mag);
                int ThumbHeight = (int)(imgInfo.Thumbnail.Height * mag);

                g.DrawImage(
                    imgInfo.Thumbnail,
                    x + (THUMBSIZE - ThumbWidth) / 2,     // X�ʒu�i����́j
                    y + (THUMBSIZE - ThumbHeight) / 2,      // Y�ʒu�i����́j
                    ThumbWidth,
                    ThumbHeight
                    );
            }

            //�����̕`��:�t�@�C����
            x += THUMBSIZE + PADDING;
            sz = string.Format("{0}", Path.GetFileName(imgInfo.Filename));
            size = g.MeasureString(sz, fontL, drawRect.Width - x, sf);
            Rectangle FilenameRect = new Rectangle(
                x, y,
                (int)Math.Ceiling(size.Width),  //�����_�ȉ��؂�グ
                (int)Math.Ceiling(size.Height)  //�����_�ȉ��؂�グ
                );
            //g.DrawString(sz, fontL, Brushes.White, x, y);
            g.DrawString(sz, fontL, Brushes.White, FilenameRect, sf);
            //y += HeightL + LINEPADDING;
            y += (int)size.Height + LINEPADDING;    //2�s��

            //�����̕`��:�t�@�C���p�X
            sz = Path.GetDirectoryName(imgInfo.Filename);
            if (!string.IsNullOrEmpty(sz))
            {
                //�`�ʃT�C�Y���m�F
                size = g.MeasureString(sz, fontS, drawRect.Width - x, sf);
                Rectangle rc = new Rectangle(x, y, drawRect.Width - x, HeightS * 2);
                g.DrawString(sz, fontS, Brushes.DarkGray, rc, sf);
                y += (int)size.Height + LINEPADDING;
            }

            //�����̕`��:���t
            x += 10;
            sz = string.Format("{0}", imgInfo.CreateDate);
            g.DrawString(sz, fontS, Brushes.DarkGray, x, y);
            y += HeightS + LINEPADDING;

            ////�����̕`��:�T�C�Y
            //sz = string.Format("{0:N0}bytes", imgInfo.length);
            //g.DrawString(sz, fontS, Brushes.DarkGray, x, y);
            //y += HeightS + LINEPADDING;

            //�����̕`��:�s�N�Z����
            //sz = string.Format(
            //    "{0:N0} x {1:N0} pixels",
            //    imgInfo.originalWidth,
            //    imgInfo.originalHeight
            //    );
            sz = string.Format(
                "{0:N0} x {1:N0} pixels, {2:N0}bytes",
                imgInfo.Width,
                imgInfo.Height,
                imgInfo.FileLength);
            g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
            y += HeightS + LINEPADDING;

            //�����̕`��:Exif���̑�
            sz = string.Format(
                "{0} {1}",
                imgInfo.Exif.Maker,
                imgInfo.Exif.Model);
            if (imgInfo.Exif.ISO != 0)
                sz = string.Format("ISO={0} {1}", imgInfo.Exif.ISO, sz);
            g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
        }

        public void Show(Form parent, ImageInfo i1, ImageInfo i2)
        {
            this.Opacity = 0F;
            MakeOffScreen(i1, i2);
            SetFormLocation(parent, DiagLocate.Middle, DiagLocate.Middle);
            this.Show(parent);
            this.Refresh();     //Invalidate()���Ⴞ��

            for (double o = 0.1F; o <= 0.8F; o += 0.05F)
            {
                this.Opacity = o;
                Thread.Sleep(5);
            }
        }

        private void MakeOffScreen(ImageInfo i1, ImageInfo i2)
        {
            if (i2 == null)
            {
                this.Width = FORMWIDTH;
                this.Height = FORMHEIGHT;
                m_offScreen = m_offScreenContext.Allocate(this.CreateGraphics(), this.ClientRectangle);

                Graphics g = m_offScreen.Graphics;
                Rectangle rc = this.ClientRectangle;

                DrawImageInfo(g, i1, rc);

                using (Pen p = new Pen(Color.FromArgb(128, 64, 64, 64)))
                {
                    //�g������
                    rc = this.ClientRectangle;
                    rc.Width--;
                    rc.Height--;
                    rc.Inflate(-2, -2);
                    g.DrawRectangle(p, rc);
                }
            }
            else
            {
                this.Width = FORMWIDTH;
                this.Height = FORMHEIGHT * 2;
                m_offScreen = m_offScreenContext.Allocate(this.CreateGraphics(), this.ClientRectangle);

                Graphics g = m_offScreen.Graphics;
                Rectangle rc = this.ClientRectangle;

                //�P��
                rc.Height = FORMHEIGHT;
                DrawImageInfo(g, i1, rc);

                //�Q��
                rc.Y = FORMHEIGHT;
                DrawImageInfo(g, i2, rc);

                using (Pen p = new Pen(Color.FromArgb(224, 64, 64, 64)))
                {
                    //�g������
                    rc = this.ClientRectangle;
                    rc.Width--;
                    rc.Height--;
                    rc.Inflate(-2, -2);
                    g.DrawRectangle(p, rc);

                    //�^�񒆂̐�������
                    rc.Height /= 2;
                    g.DrawLine(p, PADDING, rc.Height, rc.Width - PADDING, rc.Height);
                }
            }
        }

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

        public void FadeIn(Form parent, ImageInfo i1, ImageInfo i2)
        {
            MakeOffScreen(i1, i2);
            SetFormLocation(parent, DiagLocate.Right, DiagLocate.Bottom);
            this.Refresh();     //Invalidate()���Ⴞ��

            this.TopLevel = false;
            this.Parent = parent;

            this.Opacity = 0F;
            this.Visible = true;
            for (double o = 0.1F; o <= 0.8F; o += 0.05F)
            {
                this.Opacity = o;
                Thread.Sleep(5);
            }
        }

        public void FadeOut()
        {
            //�t�F�[�h�A�E�g����
            double opa = this.Opacity;
            for (double o = opa; o > 0; o -= 0.05F)
            {
                this.Opacity = o;
                Thread.Sleep(5);
            }
            this.Visible = false;
        }
    }
}
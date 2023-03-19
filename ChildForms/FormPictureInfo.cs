using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormPictureInfo : Form
    {
        private BufferedGraphics _offScreen = null;
        private readonly BufferedGraphicsContext _offScreenContext = null;
        private Point _mouseDownPoint;          //�}�E�X�N���b�N�iDown�j���ꂽ�Ƃ��̃}�E�X�ʒu�i�N���C�A���g���W�j
        private bool _formMove;                 //�}�E�X�A�b�v�����Ƃ��̃t���O�B�t�H�[�����ړ��������B
        private const int PADDING = 10;         //�萔�F�摜�̏㉺�p�f�B���O
        private const int THUMBSIZE = 120;      //�萔�F�T���l�C���T�C�Y
        private const int LINEPADDING = 2;      //�萔�F�s��
        private const int FORMWIDTH = 480;      //�萔�F�t�H�[���̕�
        private const int FORMHEIGHT = PADDING * 2 + THUMBSIZE;     //�萔�F�t�H�[���̍���

        public FormPictureInfo()
        {
            InitializeComponent();

            if (_offScreenContext == null)
                _offScreenContext = BufferedGraphicsManager.Current;
            _offScreen = _offScreenContext.Allocate(this.CreateGraphics(), this.ClientRectangle);

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
            _offScreen.Render(e.Graphics);
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
                this.SetDesktopLocation(
                    this.Left - _mouseDownPoint.X + e.X,
                    this.Top - _mouseDownPoint.Y + e.Y);

                //�t�H�[�J�X��������Ƃ��K��MouseDown->MouseMove->MouseUp�ɂȂ�͗l
                //�{����Drag�������`�F�b�N����
                if (_mouseDownPoint.X != e.X || _mouseDownPoint.Y != e.Y)
                    _formMove = true;
            }
        }

        private void PictureInfo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseDownPoint = new Point(e.X, e.Y);
                _formMove = false;
            }
        }

        private void PictureInfo_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !_formMove)
                this.Close();
        }

        private void DrawImageInfo(Graphics g, ImageInfo imgInfo, Rectangle drawRect)
        {
            //�w�i�F
            g.FillRectangle(Brushes.Black, drawRect);

            if (imgInfo == null)
                return;

            //���������̊m�F
            string sz = "9999";
            //SizeF size = g.MeasureString(sz, App.Font12B);
            //int HeightL = (int)size.Height;
            var size = g.MeasureString(sz, App.Font9);
            int HeightS = (int)size.Height;

            //StringFormat������Ă���
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisPath  //�\��������Ȃ��Ƃ��́E�E�E�\�L
            };

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
            sz = Path.GetFileName(imgInfo.Filename);
            size = g.MeasureString(sz, App.Font12B, drawRect.Width - x, sf);
            Rectangle FilenameRect = new Rectangle(
                x, y,
                (int)Math.Ceiling(size.Width),  //�����_�ȉ��؂�グ
                (int)Math.Ceiling(size.Height)  //�����_�ȉ��؂�グ
                );
            g.DrawString(sz, App.Font12B, Brushes.White, FilenameRect, sf);
            y += (int)size.Height + LINEPADDING;    //2�s��

            //�����̕`��:�t�@�C���p�X
            sz = Path.GetDirectoryName(imgInfo.Filename);
            if (!string.IsNullOrEmpty(sz))
            {
                //�`�ʃT�C�Y���m�F
                size = g.MeasureString(sz, App.Font9, drawRect.Width - x, sf);
                Rectangle rc = new Rectangle(x, y, drawRect.Width - x, HeightS * 2);
                g.DrawString(sz, App.Font9, Brushes.DarkGray, rc, sf);
                y += (int)size.Height + LINEPADDING;
            }

            //�����̕`��:���t
            x += 10;
            sz = $"{imgInfo.CreateDate}";
            g.DrawString(sz, App.Font9, Brushes.DarkGray, x, y);
            y += HeightS + LINEPADDING;

            //�����̕`��:�s�N�Z����,�t�@�C���T�C�Y
            sz = $"{imgInfo.Width:N0} x {imgInfo.Height:N0} pixels, {imgInfo.FileLength:N0}bytes";
            g.DrawString(sz, App.Font9, Brushes.SteelBlue, x, y);
            y += HeightS + LINEPADDING;

            //�����̕`��:Exif���̑�
            sz = $"{imgInfo.Exif.Maker} {imgInfo.Exif.Model}";
            if (imgInfo.Exif.ISO != 0)
                sz = $"ISO={imgInfo.Exif.ISO} {sz}";
            g.DrawString(sz, App.Font9, Brushes.SteelBlue, x, y);
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
                _offScreen = _offScreenContext.Allocate(this.CreateGraphics(), this.ClientRectangle);

                Graphics g = _offScreen.Graphics;
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
                _offScreen = _offScreenContext.Allocate(this.CreateGraphics(), this.ClientRectangle);

                Graphics g = _offScreen.Graphics;
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
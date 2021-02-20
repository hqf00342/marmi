using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Drawing.Imaging;			//PixelFormat, ColorMatrix
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /********************************************************************************/
    //���[�y
    /********************************************************************************/

    /// <summary>
    /// Loupe ���[�y�R���g���[���N���X
    ///
    /// ���[�y�@�\��񋟂���B
    /// �R���X�g���N�^�Ŏw�肵��parent�R���g���[�����L���v�`����
    /// �w��{��������Bitmap�����R���g���[���ɕ`�ʂ���B
    ///
    /// �g�����F
    ///   �R���X�g���N�^�ŃL���v�`�����ׂ��e�E�B���h�E�A���g�̑傫���A���[�y�{�����w��
    ///   ���̌�K�XDrawLoupeFast2()���Ăяo���Ď����̉摜��Update����B
    ///   �R���g���[���̕\���ʒu�͌Ăяo�����ŁATop/Left�Ŏw�肷��
    /// </summary>
    public class Loupe : UserControl
    {
        private Bitmap m_loupeBmp = null;       //���[�y�\���p��Bitmap
        private Bitmap m_captureBmp = null;     //�����ێ���Bitmap
        private int m_magnification;            //�{��
        private System.Drawing.Imaging.BitmapData srcBmpData = null;

        /// <summary>
        /// �R���X�g���N�^
        /// �قڑS�Ẵp�����[�^�������Ŏw�肷��
        /// </summary>
        /// <param name="parent">�L���v�`������e�E�B���h�E</param>
        /// <param name="width">���R���g���[���̕����w��</param>
        /// <param name="height">���R���g���[���̍������w��</param>
        /// <param name="mag">���[�y�{��</param>
        public Loupe(Control parent, int width, int height, int mag)
        {
            m_magnification = mag;
            this.Width = width;
            this.Height = height;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint,
                true);
            this.DoubleBuffered = true;

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);

            //�e���L���v�`��
            //m_captureBmp = BitmapUty.CaptureWindow(parent);
            m_captureBmp = BitmapUty.CaptureWindow((Form1._instance).PicPanel);

            //�L���v�`������Bitmap�����b�N
            Rectangle sRect = new Rectangle(0, 0, m_captureBmp.Width, m_captureBmp.Height);
            srcBmpData = m_captureBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            this.Parent = parent;       //�L���v�`���O�ɐݒ肷��Ƃ��̃R���g���[�����L���v�`������Ă��܂��B
                                        //parent.Controls.Add(this);
            this.Visible = true;

            m_loupeBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            //this.MouseMove += new MouseEventHandler(Loupe_MouseMove);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //ver1.05 �o�O�Ή�
            //�ǂ������^�C�~���O���Ń��[�y���o���ςȂ��ɂȂ�B
            //�E�{�^���������Ă��Ȃ�������j������B
            //if (e.Button != MouseButtons.Right)
            //{
            //    Debug.WriteLine("Loupe: force disposed.");
            //    this.Visible = false;
            //    //this.Close();
            //    //this.Dispose();	//������Dispose�͊댯
            //}

            //ver1.12 �o�O�Ή�
            //�����ɗ���ƌ������Ƃ̓��[�y�Ƀt�H�[�J�X�����������ƌ�������
            //�܂�E�h���b�O����Ȃ��獶�N���b�N���ꂽ
            //���̏�Ԃ͂����j������
            this.Visible = false;
        }

        public void Close()
        {
            //�\������߂�
            this.Visible = false;

            //���[�y�\���pbmp�����
            if (m_loupeBmp != null)
            {
                m_loupeBmp.Dispose();
                m_loupeBmp = null;
            }

            //�L���v�`������Bitmap�����
            if (m_captureBmp != null)
            {
                if (srcBmpData != null)
                    m_captureBmp.UnlockBits(srcBmpData);
                m_captureBmp.Dispose();
                m_captureBmp = null;
            }

            //this.Parent.Controls.Remove(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.Clear(Color.White);
            //e.Graphics.DrawImage(bmp, 0, 0);
            //e.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            e.Graphics.DrawImageUnscaled(m_loupeBmp, 0, 0);

            //2011�N7��22�� ���[�y�̑S��ʉ��ɔ����t���[���`�ʖ���
            //�t���[����`��
            //e.Graphics.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);
        }

        /// <summary>
        /// Window/Control�̑S����L���v�`������
        /// �L���v�`�������摜��m_captureBmp�Ɋi�[�����B
        /// </summary>
        /// <param name="wnd">�L���v�`���Ώۂ�Window/Control</param>
        private void CaptureWindow(Control wnd)
        {
            //Rectangle rc = parent.RectangleToScreen(parent.DisplayRectangle);	//�c�[���o�[���݂ŃL���v�`��
            //Rectangle rc = parent.Bounds;	//���ꂾ�ƃ^�C�g���o�[���L���v�`�����Ă��܂��B
            Rectangle rc = ((Form1)wnd).GetClientRectangle();   //�N���C�A���g���W�Ŏ擾�B�c�[���o�[����
            rc = wnd.RectangleToScreen(rc);                     //�X�N���[�����W��

            m_captureBmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);
            //parentBmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppRgb);

            using (Graphics g = Graphics.FromImage(m_captureBmp))
            {
                g.CopyFromScreen(
                    rc.X, rc.Y,
                    0, 0,
                    rc.Size,
                    CopyPixelOperation.SourceCopy);
            }
        }

        /// <summary>
        /// �}�l�[�W�h�Ń��[�y�`�ʃ��[�`��
        /// loupeBmp�ɕ`�ʂ���B
        /// �x�����邽�ߌ��݂͗��p���Ă��Ȃ��B
        /// </summary>
        /// <param name="b">���摜������Bitmap</param>
        /// <param name="x">���摜�ɑ΂��郋�[�y���S�ʒuX</param>
        /// <param name="y">���摜�ɑ΂��郋�[�y���S�ʒuY</param>
        //public void DrawLoupe(Bitmap b, int x, int y)
        //{
        //    if (b == null)
        //        return;

        //    if (m_loupeBmp == null)
        //        return;

        //    using (Graphics g = Graphics.FromImage(m_loupeBmp))
        //    {
        //        //�w��ʒu���L���v�`���̒��S���ɕϊ�
        //        int sx = x - this.Width / m_magnification / 2;	//�␳�A�{�����̕������Z�A/2�Œ�����
        //        int sy = y - this.Height / m_magnification / 2;
        //        Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width / m_magnification, m_loupeBmp.Height / m_magnification);
        //        Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

        //        //DrawImage���g���Ċg��`��
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
        //        g.DrawImage(
        //            b,
        //            dRect,
        //            sRect,
        //            GraphicsUnit.Pixel
        //        );
        //    }
        //}

        /// <summary>
        /// �A���Z�[�t�Ń��[�y�`�ʃ��[�`��
        /// srcBmpData�i�R���X�g���N�^�Œ�`�j�ɑ΂����[�y�@�\���
        /// �`�ʑ��x�𓾂邽��unsafe���p
        /// </summary>
        /// <param name="x">���摜�ɑ΂��郋�[�y���S�ʒuX</param>
        /// <param name="y">���摜�ɑ΂��郋�[�y���S�ʒuY</param>
        public void DrawLoupeFast2(int x, int y)
        {
            int sWidth = m_captureBmp.Width;                //�L���v�`���ϐe��ʂ̕�
            int sHeight = m_captureBmp.Height;              //�L���v�`���ϐe��ʂ̍���
            int capWidth = m_loupeBmp.Width / m_magnification;      //�L���v�`���͈́F��
            int capHeight = m_loupeBmp.Height / m_magnification;        //�L���v�`���͈́F����

            //�w��ʒu���L���v�`���̒��S���ɕϊ�
            //unsafe�ɑΉ����邽�߂�����ƃL���v�`���͈͂𐳋K������B
            int sx = x - capWidth / 2;
            int sy = y - capHeight / 2;
            sx = (sx > 0) ? sx : 0;
            sy = (sy > 0) ? sy : 0;
            if (sx > sWidth - capWidth)
                sx = sWidth - capWidth;
            if (sy > sHeight - capHeight)
                sy = sHeight - capHeight;

            Rectangle sRect = new Rectangle(0, 0, sWidth, sHeight);
            Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

            //System.Drawing.Imaging.BitmapData srcBmpData = parentBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData dstBmpData = m_loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //System.Drawing.Imaging.BitmapData srcBmpData = parentBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            //System.Drawing.Imaging.BitmapData dstBmpData = loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            //Stopwatch sw = Stopwatch.StartNew();
            unsafe
            {
                byte* pSrc = (byte*)srcBmpData.Scan0;
                byte* pDest = (byte*)dstBmpData.Scan0;
                int pos;
                byte B, G, R;

                for (int ry = 0; ry < capHeight; ry++)
                {
                    for (int rx = 0; rx < capWidth; rx++)
                    {
                        //�ǂݍ���
                        pos = (sx + rx) * 3 + srcBmpData.Stride * (sy + ry);
                        B = pSrc[pos + 0];
                        G = pSrc[pos + 1];
                        R = pSrc[pos + 2];

                        //��������.MAG�{����
                        for (int my = 0; my < m_magnification; my++)
                        {
                            for (int mx = 0; mx < m_magnification; mx++)
                            {
                                //pos = (rx) * 3 + dstBd.Stride * (ry);					//���{
                                pos = (rx * m_magnification + mx) * 3 + dstBmpData.Stride * (ry * m_magnification + my);    //MAG�{
                                pDest[pos + 0] = B;
                                pDest[pos + 1] = G;
                                pDest[pos + 2] = R;
                            }
                        }
                    }
                }
            }
            //sw.Stop();
            //Debug.WriteLine(sw.ElapsedTicks);

            m_loupeBmp.UnlockBits(dstBmpData);
            //parentBmp.UnlockBits(srcBmpData);
        }

        /// <summary>
        /// �A���Z�[�t�Ń��[�y�`�ʃ��[�`��
        /// ����̍��W���w�肷��ver
        /// </summary>
        /// <param name="leftX">������W</param>
        /// <param name="topY">������W</param>
        public void DrawLoupeFast3(int leftX, int topY)
        {
            int srcWidth = m_captureBmp.Width;                      //�L���v�`���ϐe��ʂ̕�
            int srcHeight = m_captureBmp.Height;                    //�L���v�`���ϐe��ʂ̍���
            int viewWidth = m_loupeBmp.Width / m_magnification;     //�L���v�`���͈́F��
            int viewHeight = m_loupeBmp.Height / m_magnification;   //�L���v�`���͈́F����

            //�w��ʒu���L���v�`���̒��S���ɕϊ�
            //unsafe�ɑΉ����邽�߂�����ƃL���v�`���͈͂𐳋K������B
            int sx = leftX;
            int sy = topY;
            sx = (sx > 0) ? sx : 0;
            sy = (sy > 0) ? sy : 0;
            if (sx > srcWidth - viewWidth)
                sx = srcWidth - viewWidth;
            if (sy > srcHeight - viewHeight)
                sy = srcHeight - viewHeight;

            Rectangle sRect = new Rectangle(0, 0, srcWidth, srcHeight);
            Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

            System.Drawing.Imaging.BitmapData dstBmpData = m_loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* pSrc = (byte*)srcBmpData.Scan0;
                byte* pDest = (byte*)dstBmpData.Scan0;
                int pos;
                byte B, G, R;

                for (int ry = 0; ry < viewHeight; ry++)
                {
                    for (int rx = 0; rx < viewWidth; rx++)
                    {
                        //�ǂݍ���
                        pos = (sx + rx) * 3 + srcBmpData.Stride * (sy + ry);
                        B = pSrc[pos + 0];
                        G = pSrc[pos + 1];
                        R = pSrc[pos + 2];

                        //��������.MAG�{����
                        for (int my = 0; my < m_magnification; my++)
                        {
                            for (int mx = 0; mx < m_magnification; mx++)
                            {
                                //pos = (rx) * 3 + dstBd.Stride * (ry);					//���{
                                pos = (rx * m_magnification + mx) * 3 + dstBmpData.Stride * (ry * m_magnification + my);    //MAG�{
                                pDest[pos + 0] = B;
                                pDest[pos + 1] = G;
                                pDest[pos + 2] = R;
                            }
                        }
                    }
                }
            }
            m_loupeBmp.UnlockBits(dstBmpData);
        }

        /// <summary>
        /// ���{���[�y
        /// </summary>
        /// <param name="x">���S�Ƃ���ʒuX</param>
        /// <param name="y">���S�Ƃ���ʒuY</param>
        /// <param name="orgBitmap">���摜</param>
        public void DrawOriginalSizeLoupe(int x, int y, Bitmap orgBitmap)
        {
            if (orgBitmap == null)
                return;

            using (Graphics g = Graphics.FromImage(m_loupeBmp))
            {
                //��������N���A
                g.Clear(App.Config.BackColor);

                //�w��ʒu���L���v�`���̒��S���ɕϊ�
                int sx = x - this.Width / 2;    // 1/2�Œ�����
                int sy = y - this.Height / 2;
                Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width, m_loupeBmp.Height);
                Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

                //DrawImage���g���Ċg��`��
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(
                    orgBitmap,      // �`�ʌ��̉摜
                    dRect,          // �`�ʐ��Rect�w��
                    sRect,          // �`�ʌ��摜��Rect�w��
                    GraphicsUnit.Pixel
                );
            }//using
        }

        /// <summary>
        /// ���{���[�y
        /// �w��ʒu��������W�ɂ�������
        /// </summary>
        /// <param name="left">�n�_�F��</param>
        /// <param name="top">�n�_�F��</param>
        /// <param name="orgBitmap">���摜</param>
        public void DrawOriginalSizeLoupe2(int left, int top, Bitmap orgBitmap)
        {
            if (orgBitmap == null)
                return;

            using (Graphics g = Graphics.FromImage(m_loupeBmp))
            {
                //��������N���A
                g.Clear(App.Config.BackColor);

                //�w��ʒu���L���v�`���̒��S���ɕϊ�
                int sx = left;
                int sy = top;
                Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width, m_loupeBmp.Height);
                Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

                //DrawImage���g���Ċg��`��
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(
                    orgBitmap,      // �`�ʌ��̉摜
                    dRect,          // �`�ʐ��Rect�w��
                    sRect,          // �`�ʌ��摜��Rect�w��
                    GraphicsUnit.Pixel
                );
            }//using
        }
    }
}
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

/********************************************************************************
Loupe

���[�y�@�\��񋟂���B
�R���X�g���N�^�Ŏw�肵��parent�R���g���[�����L���v�`����
�w��{��������Bitmap�����R���g���[���ɕ`�ʂ���B

�g�����F
  �R���X�g���N�^�ŃL���v�`�����ׂ��e�E�B���h�E�A���g�̑傫���A���[�y�{�����w��
  ���̌�K�XDrawLoupeFast2()���Ăяo���Ď����̉摜��Update����B
  �R���g���[���̕\���ʒu�͌Ăяo�����ŁATop/Left�Ŏw�肷��

********************************************************************************/

namespace Marmi
{

    public class Loupe : UserControl
    {
        private Bitmap m_loupeBmp = null;       //���[�y�\���p��Bitmap
        private Bitmap m_captureBmp = null;     //�����ێ���Bitmap
        private readonly int m_magnification;   //�{���B�R���X�g���N�^�Ō���
        private readonly BitmapData srcBmpData = null;

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

            //���S�ȃI�[�i�[�h���[�R���g���[���ɂ���
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint,
                true);
            this.DoubleBuffered = true;

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);

            //�e���L���v�`��
            m_captureBmp = BitmapUty.CaptureWindow((Form1._instance).PicPanel);

            //�L���v�`������Bitmap�����b�N
            var sRect = new Rectangle(0, 0, m_captureBmp.Width, m_captureBmp.Height);
            srcBmpData = m_captureBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            this.Parent = parent;
            //�L���v�`���O�ɐݒ肷��Ƃ��̃R���g���[�����L���v�`������Ă��܂��B
            this.Visible = true;

            //PicPanel�Ɠ����傫����Bitmap��p�ӂ���
            m_loupeBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

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
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawImageUnscaled(m_loupeBmp, 0, 0);
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

            var sRect = new Rectangle(0, 0, srcWidth, srcHeight);
            var dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

            BitmapData dstBmpData = m_loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

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
        /// �w��ʒu��������W�ɂ�������
        /// </summary>
        /// <param name="left">�n�_�F��</param>
        /// <param name="top">�n�_�F��</param>
        /// <param name="orgBitmap">���摜</param>
        public void DrawOriginalSizeLoupe2(int left, int top, Bitmap orgBitmap)
        {
            if (orgBitmap == null)
                return;

            using (var g = Graphics.FromImage(m_loupeBmp))
            {
                //��������N���A
                g.Clear(App.Config.General.BackColor);

                //�w��ʒu���L���v�`���̒��S���ɕϊ�
                int sx = left;
                int sy = top;
                Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width, m_loupeBmp.Height);
                Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

                //DrawImage���g���Ċg��`��
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(orgBitmap, dRect, sRect, GraphicsUnit.Pixel);
            }//using
        }
    }
}
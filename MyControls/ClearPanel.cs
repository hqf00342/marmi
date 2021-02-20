using System;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /// <summary>
    /// ��������Bitmap���p�l���Ƃ��ė��p����N���X
    /// �t�F�[�h�C���E�t�F�[�h�A�E�g���Ȃ���\������B
    ///
    /// </summary>
    internal class ClearPanel : UserControl
    {
        private Bitmap m_srcImage = null;
        private Bitmap m_bitmap = null;
        private System.Windows.Forms.Timer m_HoldTimer = null;      //�ێ�����

        private System.Windows.Forms.Timer m_FadeInTimer = null;    //�t�F�[�h�C�����̕`�ʃ^�C�}�[
        private System.Windows.Forms.Timer m_FadeOutTimer = null;   //�t�F�[�h�A�E�g�̕`�ʃ^�C�}�[
        private float m_AlphaValue;     //�t�F�[�h���̔������x 0.0f�`1.0f
        private float m_AlphaDiff = 0.2f;   //�A���t�@�l�̑�������

        // ������ ***********************************************************************/

        public ClearPanel(Control parent)
        {
            //�������ɂ��� 2011�N7��19��
            //http://youryella.wankuma.com/Library/Extensions/Control/Transparent.aspx
            //
            //�e�R���g���[���ɑ΂��ē���/�������ɂȂ�
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.BackColor = Color.Transparent; // ����
            //this.BackColor = Color.FromArgb(100, 255, 255, 255); // ������

            this.Visible = false;
            this.BackColor = Color.Transparent;     //�w�i�͓����ɁB�d�v�ݒ�
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.SupportsTransparentBackColor    //2011-7-19 �ǉ��B�Ȃ�œ����Ă��Ȃ��̂��s��
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint,
                true);                              //�`�ʂ̓I�[�i�[�h���[

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);

            //�C�x���g
            //this.Paint += new PaintEventHandler(ClearPanel_Paint);

            //Timer
            m_HoldTimer = new System.Windows.Forms.Timer();
            m_HoldTimer.Tick += new EventHandler(m_timer_Tick);

            //�t�F�[�h�C���^�C�}�[
            m_FadeInTimer = new System.Windows.Forms.Timer();
            m_FadeInTimer.Tick += new EventHandler(m_FadeInTimer_Tick);
            m_FadeInTimer.Interval = 10;

            m_FadeOutTimer = new System.Windows.Forms.Timer();
            m_FadeOutTimer.Tick += new EventHandler(m_FadeOutTimer_Tick);
            m_FadeOutTimer.Interval = 10;

            SetParent(parent);
        }

        ~ClearPanel()
        {
            this.Visible = false;
            //this.Paint -= new PaintEventHandler(ClearPanel_Paint);
            if (Parent.Controls.Contains(this))
                Parent.Controls.Remove(this);
            if (m_bitmap != null)
                m_bitmap.Dispose();
            if (m_HoldTimer != null)
            {
                m_HoldTimer.Stop();
                m_HoldTimer.Dispose();
            }

            m_FadeInTimer.Stop();
            m_FadeInTimer.Dispose();
            m_FadeOutTimer.Stop();
            m_FadeOutTimer.Dispose();
        }

        #region �^�C�}�[�C�x���g

        //***********************************************************************
        private void m_timer_Tick(object sender, EventArgs e)
        {
            m_HoldTimer.Stop();
            FadeOut();
        }

        private void m_FadeInTimer_Tick(object sender, EventArgs e)
        {
            m_AlphaValue += m_AlphaDiff;
            if (m_AlphaValue <= 1.0f)
            {
                using (Graphics g = Graphics.FromImage(m_bitmap))
                {
                    g.Clear(Color.Transparent);
                    BitmapUty.alphaDrawImage(g, m_srcImage, m_AlphaValue);
                }
            }
            else
            {
                m_FadeInTimer.Stop();
            }
            this.Refresh();
        }

        private void m_FadeOutTimer_Tick(object sender, EventArgs e)
        {
            m_AlphaValue -= m_AlphaDiff;
            if (m_AlphaValue > 0.0f)
            {
                using (Graphics g = Graphics.FromImage(m_bitmap))
                {
                    g.Clear(Color.Transparent);
                    BitmapUty.alphaDrawImage(g, m_srcImage, m_AlphaValue);
                }
                //this.Top--;	//ver1.27�R�����g�A�E�g
            }
            else
            {
                m_FadeOutTimer.Stop();
                this.Visible = false;
            }
            this.Refresh();
        }

        #endregion �^�C�}�[�C�x���g

        #region override

        //***********************************************************************
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_bitmap == null)
                return;

            //�`��
            e.Graphics.DrawImage(
                m_bitmap,
                e.ClipRectangle,
                e.ClipRectangle,
                GraphicsUnit.Pixel);
        }

        #endregion override

        #region public���\�b�h

        //***********************************************************************
        public void SetParent(Control parent)
        {
            this.Parent = parent;
            parent.Controls.Add(this);
        }

        public void SetBitmap(Bitmap bmp)
        {
            m_srcImage = bmp;
            this.Width = m_srcImage.Width;
            this.Height = m_srcImage.Height;

            if (m_bitmap != null)
                m_bitmap.Dispose();
            m_bitmap = new Bitmap(this.Width, this.Height);
        }

        public void FadeIn(Point pt)
        {
            this.Location = pt;
            this.Visible = true;
            //Graphics g = Graphics.FromImage(m_bitmap);
            //for (float a = 0.0F; a <= 1.0F; a += 0.05F)
            //{
            //    g.Clear(Color.Transparent);
            //    BitmapUty.alphaDrawImage(g, m_srcImage, a);
            //    this.Refresh();
            //    Thread.Sleep(15);
            //}

            if (m_FadeInTimer.Enabled)
                m_FadeInTimer.Stop();

            m_AlphaValue = 0.0f;
            m_FadeInTimer.Start();
        }

        public void FadeIn()
        {
            //�\���ʒu��ݒ�F������
            Point pt = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);

            FadeIn(pt);
        }

        public void FadeOut()
        {
            //�t�F�[�h�C�����~�߂�
            //����A�d�v�B����Ă����Ȃ���Out��In�ŉi�v�Ɏ~�܂�Ȃ�
            if (m_FadeInTimer.Enabled)
                m_FadeInTimer.Stop();

            //�t�F�[�h�A�E�g
            if (m_FadeOutTimer.Enabled)
                m_FadeOutTimer.Stop();

            m_AlphaValue = 1.0f;
            m_FadeOutTimer.Start();
        }

        /// <summary>
        /// �t�F�[�h�����ɑ����ɕ\��
        /// �ŏ��̓ǂݍ��݂ł͑����ɕ\���������������B
        /// ver0.987 ����
        /// </summary>
        public void ShowJustNow()
        {
            //�\���ʒu��ݒ�F������
            Point pt = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);
            this.Location = pt;
            this.Visible = true;

            using (Graphics g = Graphics.FromImage(m_bitmap))
            {
                g.DrawImage(m_srcImage, 0, 0);
            }
            this.Refresh();
        }

        /// <summary>
        /// Bitmap��ClearPanel�ɕ\������
        /// �w�莞�Ԃ��߂�����͎����I�ɕ���
        /// �ʒu�����R�Ɏw��\
        /// </summary>
        /// <param name="pt">�\���ʒu</param>
        /// <param name="holdtime">�\�����ԁi�t�F�[�h���Ԃ͏����j�B0�ȏ�̃~���b</param>
        public void ShowAndClose(Point pt, int holdtime)
        {
            if (holdtime <= 0)
                holdtime = 1000;
            m_HoldTimer.Interval = holdtime;

            FadeIn(pt);
            m_HoldTimer.Start();
        }

        /// <summary>
        /// �w�肳�ꂽ�������ClearPanel�ɕ\������
        /// �w�莞�Ԃ��߂�����͎����I�ɕ���
        /// �\���ʒu�͒����ɌŒ�
        /// </summary>
        /// <param name="text">�\�����镶����</param>
        /// <param name="holdtime">�\�����ԁi�t�F�[�h���Ԃ͏����j�B0�ȏ�̃~���b</param>
        public void ShowAndClose(string text, int holdtime)
        {
            SetBitmap(BitmapUty.Text2Bitmap(text, true));

            //�\���ʒu��ݒ�F������
            Point pt = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);

            ShowAndClose(pt, holdtime);
        }

        #endregion public���\�b�h
    }
}
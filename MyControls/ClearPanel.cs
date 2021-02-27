using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

/*
��������Bitmap���p�l���Ƃ��ė��p����N���X
�t�F�[�h�C���E�t�F�[�h�A�E�g���Ȃ���\������B

2021�N2��26�� ��������߂��B�������[���[�N���[�Ȃ�

�������ɂ��� 2011�N7��19��
http://youryella.wankuma.com/Library/Extensions/Control/Transparent.aspx
*/

namespace Marmi
{
    internal class ClearPanel : UserControl
    {
        //��ʂɕ\�������C���[�W�B
        private Bitmap _screenImage = null;

        //��莞�ԂŔ�\���ɂ���^�C�}�[
        private readonly Timer _hideTimer;

        public ClearPanel(Control parent)
        {
            //������Ԃ͔�\���A�w�i�͓����ɁB�d�v
            this.Visible = false;
            this.BackColor = Color.Transparent;

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);

            //Timer
            _hideTimer = new Timer();
            _hideTimer.Tick += HideTimer_Tick;

            //�e�R���g���[���̓o�^
            this.Parent = parent;
            parent.Controls.Add(this);
        }

        ~ClearPanel()
        {
            this.Visible = false;
            _screenImage?.Dispose();

            if (Parent.Controls.Contains(this))
                Parent.Controls.Remove(this);

            if (_hideTimer != null)
            {
                _hideTimer.Stop();
                _hideTimer.Dispose();
                _hideTimer.Tick -= HideTimer_Tick;
            }
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            _hideTimer.Stop();
            this.Visible = false;
            _screenImage?.Dispose();
            _screenImage = null;
            //Uty.ForceGC();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Debug.WriteLine("ClearPanel::OnPaint()");
            base.OnPaint(e);

            if (_screenImage != null)
            {
                e.Graphics.DrawImage(_screenImage, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            }
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
            if (_hideTimer.Enabled)
            {
                _hideTimer.Stop();
            }

            _screenImage?.Dispose();
            _screenImage = BitmapUty.Text2Bitmap(text, true);
            //this.CreateGraphics().DrawImage(_screenImage, Point.Empty);

            //�\���ʒu�𒆉��ɂ��ĕ\��
            this.Width = _screenImage.Width;
            this.Height = _screenImage.Height;
            this.Location = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);
            this.Visible = true;

            //��莞�ԂŔ�\���ɂ���^�C�}�[���N��
            _hideTimer.Interval = holdtime <= 100 ? 1000 : holdtime;
            _hideTimer.Start();
        }
    }
}
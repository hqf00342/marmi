using System;
using System.Drawing;
using System.Windows.Forms;

/*
TrackBar��ToolStrip�ɍڂ��邽�߂̃N���X�B

���p���@�F
Form1()��Form1_Load()�Ŏ���������B
  private ToolStripTrackBar g_trackbar;
  g_trackbar = new ToolStripTrackBar();
  g_trackbar.Minimum = 0;
  g_trackbar.Maximum = 0;		//�����O�ɂ���Ɠ����Ȃ��o�[�ɂȂ�B
  g_trackbar.ValueChanged += new EventHandler(g_trackbar_ValueChanged);
  toolStrip1.Items.Add(g_trackbar);
*/

namespace Marmi
{
    public class ToolStripTrackBar : ToolStripControlHost
    {
        /// <summary>
        /// Trackbar�p�R���X�g���N�^
        /// </summary>
        public ToolStripTrackBar() : base(new TrackBar())
        {
            this.BackColor = SystemColors.Control;
            Initialize();   //2011�N8��19�� �ǉ�
        }

        /// <summary>
        /// TrackBar�R���g���[����Ԃ��v���p�e�B
        /// </summary>
        public TrackBar TrackBar => (TrackBar)Control;

        /// <summary>
        /// TickFrequency��set/get�p�v���p�e�B
        /// </summary>
        public int TickFrequency
        {
            get => TrackBar.TickFrequency;
            set => TrackBar.TickFrequency = value;
        }

        /// <summary>
        /// �ŏ��l�̃v���p�e�B
        /// </summary>
        public int Minimum
        {
            get => TrackBar.Minimum;
            set => TrackBar.Minimum = value;
        }

        /// <summary>
        /// �ő�l�̃v���p�e�B
        /// </summary>
        public int Maximum
        {
            get => TrackBar.Maximum;
            set => TrackBar.Maximum = value;
        }

        /// <summary>
        /// ���݂̒l
        /// </summary>
        public int Value
        {
            get => TrackBar.Value;
            set => TrackBar.Value = value;
        }

        //TODO: ������new�͕K�v�H
        //�@new �̓v���p�e�B���I�[�o�[���C�h����Ƃ��ɕK�v
        //�@�ǂ����sealed�ŉB������Ă�����̂��㏑������Ƃ��ɕK�v�H
        //�@�e�N���X�̃��\�b�h��"�I�[�o�[���C�h"(���B��)���鎞��override�L�[���[�h�B
        //�@�e�N���X�̃��\�b�h��"�B��"���鎞��new�L�[���[�h�B
        public new int Width
        {
            get => TrackBar.Width;
            set => TrackBar.Width = value;
        }

        public int SmallChange
        {
            get => TrackBar.SmallChange;
            set => TrackBar.SmallChange = value;
        }

        public int LargeChange
        {
            get => TrackBar.LargeChange;
            set => TrackBar.LargeChange = value;
        }

        //ValueChanged�C�x���g���T�u�X�N���C�u�i�o�^�j����B
        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            TrackBar tb = (TrackBar)Control;
            tb.ValueChanged += new EventHandler(Tb_ValueChanged);
            tb.MouseWheel += new MouseEventHandler(Tb_MouseWheel);
        }

        //ValueChanged�C�x���g���A���T�u�X�N���C�u�i�o�^�����j����B
        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            TrackBar tb = (TrackBar)Control;
            tb.ValueChanged -= new EventHandler(Tb_ValueChanged);
            tb.MouseWheel -= new MouseEventHandler(Tb_MouseWheel);
        }

        //�C�x���g��`�BValueChanged()
        public event EventHandler ValueChanged;

        //�T�u�X�N���C�u�Ŏ����������ꂽ�R�[�h�B
        //ValueChaned()���N�����悤�ɒǋL
        private void Tb_ValueChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        //2011/09/01
        //�}�E�X�z�C�[���C�x���g�̎���
        public event EventHandler<MouseEventArgs> MouseWheel;

        private void Tb_MouseWheel(object sender, MouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(null, e);
        }

        public void Initialize()
        {
            Minimum = 0;
            Value = 0;
            Maximum = 0;
        }
    }
}
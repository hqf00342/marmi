using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					//SystemColors.Control
using System.Windows.Forms;			//PictureBox, ScrollBar



namespace Marmi
{

	/// <summary>
	/// TrackBar��ToolStrip�ɍڂ��邽�߂̃N���X�B
	/// 
	/// ���p���@�F
	/// Form1()��Form1_Load()�Ŏ���������B
	/// 
	///	private ToolStripTrackBar g_trackbar;
	///		g_trackbar = new ToolStripTrackBar();
	///		g_trackbar.Minimum = 0;
	///		g_trackbar.Maximum = 0;		//�����O�ɂ���Ɠ����Ȃ��o�[�ɂȂ�B
	///		g_trackbar.ValueChanged += new EventHandler(g_trackbar_ValueChanged);
	///		toolStrip1.Items.Add(g_trackbar);
	/// 
	/// </summary>
	public class ToolStripTrackBar : ToolStripControlHost
	{
		/// <summary>
		/// Trackbar�p�R���X�g���N�^
		/// </summary>
		public ToolStripTrackBar() : base(new TrackBar())
		{
			this.BackColor = SystemColors.Control;
			Initialize();	//2011�N8��19�� �ǉ�


			//debug�p�B������Wheel�C�x���g��T���B
			//TrackBar.MouseWheel += new MouseEventHandler(TrackBar_MouseWheel);
			/*
			 * 2011�N9��1��
			 * �����Ƃ��Ă��邱�Ƃ��m�F�����̂ŃR�����g�A�E�g
			 * �����ŃC�x���g���������A�h����ŏ������������̂�
			 * OnSubscribe����
			 */
			//TrackBar.MouseWheel += (s, e) =>
			//    {
			//        Debug.WriteLine(e.Delta, "ToolStripTrackBar::MouseWheel()");
			//    };
		}


		/// <summary>
		/// TrackBar�R���g���[����Ԃ��v���p�e�B
		/// </summary>
		public TrackBar TrackBar
		{
			get { return (TrackBar)Control; }
		}

		/// <summary>
		/// TickFrequency��set/get�p�v���p�e�B
		/// </summary>
		public int TickFrequency
		{
			get { return TrackBar.TickFrequency; }
			set { TrackBar.TickFrequency = value; }
		}

		/// <summary>
		/// �ŏ��l�̃v���p�e�B
		/// </summary>
		public int Minimum
		{
			//get { return this.Minimum; }
			//set { this.Minimum = value; }
			get { return TrackBar.Minimum; }
			set { TrackBar.Minimum = value; }
		}

		/// <summary>
		/// �ő�l�̃v���p�e�B
		/// </summary>
		public int Maximum
		{
			get { return TrackBar.Maximum; }
			set { TrackBar.Maximum = value; }
		}

		/// <summary>
		/// ���݂̒l
		/// </summary>
		public int Value
		{
			get { return TrackBar.Value; }
			set { TrackBar.Value = value; }
		}

		//TODO: ������new�͕K�v�H
		//�@new �̓v���p�e�B���I�[�o�[���C�h����Ƃ��ɕK�v
		//�@�ǂ����sealed�ŉB������Ă�����̂��㏑������Ƃ��ɕK�v�H
		//
		//�@�e�N���X�̃��\�b�h��"�I�[�o�[���C�h"(���B��)���鎞��override�L�[���[�h�B
		//�@�e�N���X�̃��\�b�h��"�B��"���鎞��new�L�[���[�h�B
		public new int Width
		{
			get { return TrackBar.Width; }
			set { TrackBar.Width = value; }
		}

		public int SmallChange
		{
			get { return TrackBar.SmallChange; }
			set { TrackBar.SmallChange = value; }
		}

		public int LargeChange
		{
			get { return TrackBar.LargeChange; }
			set { TrackBar.LargeChange = value; }
		}

		//ValueChanged�C�x���g���T�u�X�N���C�u�i�o�^�j����B
		protected override void OnSubscribeControlEvents(Control control)
		{
			base.OnSubscribeControlEvents(control);
			TrackBar tb = (TrackBar)Control;
			tb.ValueChanged += new EventHandler(tb_ValueChanged);
			tb.MouseWheel += new MouseEventHandler(tb_MouseWheel);
		}


		//ValueChanged�C�x���g���A���T�u�X�N���C�u�i�o�^�����j����B
		protected override void OnUnsubscribeControlEvents(Control control)
		{
			base.OnUnsubscribeControlEvents(control);
			TrackBar tb = (TrackBar)Control;
			tb.ValueChanged -= new EventHandler(tb_ValueChanged);
			tb.MouseWheel -= new MouseEventHandler(tb_MouseWheel);
		}

		//�C�x���g��`�BValueChanged()
		public event EventHandler ValueChanged;

		//�T�u�X�N���C�u�Ŏ����������ꂽ�R�[�h�B
		//ValueChaned()���N�����悤�ɒǋL
		void tb_ValueChanged(object sender, EventArgs e)
		{
			if (ValueChanged != null)
			{
				ValueChanged(this, e);
			}
		}

		//2011/09/01
		//�}�E�X�z�C�[���C�x���g�̎���
		public event EventHandler<MouseEventArgs> MouseWheel;
		void tb_MouseWheel(object sender, MouseEventArgs e)
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

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;


namespace Marmi
{
	public class MomentumScrollPanel : Control
	{
		#region private�ϐ�
		// �c�X�N���[���o�[
		private VScrollBar m_vBar = new VScrollBar();
		
		// ���X�N���[���o�[
		private HScrollBar m_hBar = new HScrollBar();

		// �����X�N���[���@�\�p�̃^�C�}�[
		private Timer m_scrollTimer = new Timer();


		//MomentumScrollPanel�R���g���[���Ƃ��Ď����Ă���X�N���[���l
		//�X�N���[���o�[�̎��ۂ̒l�ƈقȂ�
		//�X�N���[���o�[�̒l�������X�N���[����̖ړI�l
		//���ݎw�������Ă���l�ł���A�����X�N���[�����͕ς���Ă����B
		private int m_vScrollValue;
		private int m_hScrollValue;

		//�^�C�}�[�̊Ԋu[msec]
		private const int SCROLLTIMER_TICK = 30;

		//������. 0.0�`1.0�܂ł̊Ԃ̐��l�B�傫���قǑ�����������B
		private const double MOMENTUM_FORCE = 0.3;
		private const int MINIMUM_SCROLL = 5;

		//�X�N���[���o�[�̖��{�^����z�C�[���ł̍Œ�X�N���[����
		private const int SCROLL_SMALLCHANGE = 20;
		#endregion

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public MomentumScrollPanel()
		{
			//�f�t�H���g�ݒ�
			UseAnimation = true;

			//�R���g���[�����g�̏�����
			this.DoubleBuffered = true;
			this.ResizeRedraw = true;
			//this.MouseWheel += new MouseEventHandler(OnMouseWheel);

			//�c�X�N���[���o�[�̏�����
			m_vBar.Dock = DockStyle.Right;
			m_vBar.Minimum = 0;
			m_vBar.Value = 0;
			m_vBar.Visible = false;
			m_vBar.ValueChanged += new EventHandler(m_vBar_ValueChanged);
			Controls.Add(m_vBar);
			m_vScrollValue = 0;

			//���X�N���[���o�[�̏�����
			m_hBar.Dock = DockStyle.Bottom;
			m_hBar.Minimum = 0;
			m_hBar.Value = 0;
			m_hBar.Visible = false;
			m_hBar.ValueChanged += new EventHandler(m_hBar_ValueChanged);
			m_vScrollValue = 0;

			//�X�N���[���p�^�C�}�[�̏�����
			m_scrollTimer.Interval = SCROLLTIMER_TICK;
			m_scrollTimer.Tick += new EventHandler(m_scrollTimer_Tick);
			Controls.Add(m_hBar);

			//�X�N���[���o�[�̕\���v�ۊm�F
			CheckScrollbarDraw();
		}

		
		///////////////////////////////////////////////////////////////
		// �v���p�e�B
		///////////////////////////////////////////////////////////////
		#region Properties
		/// <summary>
		/// �N���C�A���g�`�ʗ̈��Ԃ��v���p�e�B
		/// �X�N���[���o�[������ꍇ�͂��̕����T�������l��Ԃ�
		/// </summary>
		public new Rectangle ClientRectangle
		{
			get
			{
				Rectangle r = base.ClientRectangle;

				if (m_vBar.Visible)
					r.Width -= m_vBar.Width;

				if (m_hBar.Visible)
					r.Height -= m_hBar.Height;

				return r;
			}
		}


		/// <summary>
		/// �N���C�A���g�`�ʗ̈��Ԃ�
		/// �X�N���[���o�[�������Ă��T�������X�N���[���o�[�̈���܂�
		/// �l��Ԃ��B���܂�g����ƍl���Ă͂��Ȃ��B
		/// </summary>
		public Rectangle ClientRectangleWithScrollbar
		{
			get { return base.ClientRectangle; }
		}

		/// <summary>
		/// �����X�N���[�������邩�ǂ����̃t���O
		/// </summary>
		public bool UseAnimation { get; set; }

		/// <summary>
		/// �X�N���[���o�[��\�����邩�ǂ���
		/// </summary>
		public bool AutoScroll { get; set; }

		/// <summary>
		/// �X�N���[���o�[�̉��͈͂�set/get����
		/// �X�N���[���̈�̎��ۂ̃s�N�Z�������Z�b�g����
		/// </summary>
		public Size AutoScrollMinSize
		{
			get
			{
				return new Size(m_hBar.Maximum, m_vBar.Maximum);
			}
			set
			{
				m_vBar.Minimum = 0;
				m_hBar.Minimum = 0;
				m_vBar.Maximum = value.Height;
				m_hBar.Maximum = value.Width;
				m_vBar.LargeChange = this.Height;
				m_hBar.LargeChange = this.Width;
				CheckScrollbarDraw();
			}
		}

		/// <summary>
		/// �X�N���[���o�[�̒l
		/// ���ۂɎw�������Ă���l�̂��ߊ����X�N���[�����͏��X�ɕω����Ă���B
		/// set�����l�̓^�[�Q�b�g�l�B
		/// �����X�N���[���ɏ]���ď��X�ɒl���߂Â��Ă����B
		/// </summary>
		public Point AutoScrollPosition
		{
			get
			{
				//return new Point(-m_hBar.Value, -m_vBar.Value);
				return new Point(-m_hScrollValue, -m_vScrollValue);
			}
			set 
			{
				//validation X
				if (m_hBar.Visible)
				{
					if (value.X < 0)
						m_hBar.Value = 0;
					else if (m_hBar.Maximum <= m_hBar.LargeChange)
						m_hBar.Value = 0;
					else if (value.X > m_hBar.Maximum - m_hBar.LargeChange)
						m_hBar.Value = m_hBar.Maximum - m_hBar.LargeChange;
					else
						m_hBar.Value = value.X;
				}
				//validation Y
				if (m_vBar.Visible)
				{
					if (value.Y < 0)
						m_vBar.Value = 0;
					else if (m_vBar.Maximum <= m_vBar.LargeChange)
						m_vBar.Value = 0;
					else if (value.Y > m_vBar.Maximum - m_vBar.LargeChange)
						m_vBar.Value = m_vBar.Maximum - m_vBar.LargeChange;
					else
						m_vBar.Value = value.Y;
				}
			}
		}
		#endregion


		private void CheckScrollbarDraw()
		{
			//LargeChange��ݒ�
			m_vBar.LargeChange = this.Height;
			m_hBar.LargeChange = this.Width;
			//if (m_vBar.LargeChange > m_vBar.Maximum)
			//    m_vBar.LargeChange = m_vBar.Maximum;
			//if (m_hBar.LargeChange > m_hBar.Maximum)
			//    m_hBar.LargeChange = m_hBar.Maximum;

			//SmallChange��ݒ�
			m_vBar.SmallChange = m_vBar.LargeChange / 10;
			m_hBar.SmallChange = m_hBar.LargeChange / 10;
			if (m_vBar.SmallChange < SCROLL_SMALLCHANGE)
				m_vBar.SmallChange = SCROLL_SMALLCHANGE;
			if (m_hBar.SmallChange < SCROLL_SMALLCHANGE)
				m_hBar.SmallChange = SCROLL_SMALLCHANGE;

			//�\�����邩�ǂ������m�F
			m_vBar.Visible = m_vBar.Maximum > this.Height;
			m_hBar.Visible = m_hBar.Maximum > this.Width;
		}

		///////////////////////////////////////////////////////////////
		// �C�x���g����
		///////////////////////////////////////////////////////////////

		/// <summary>
		/// �}�E�X�z�C�[���C�x���g
		/// �z�C�[���ɂ��X�N���[���������X�N���[���ΏۂƂ���
		/// </summary>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (AutoScrollMinSize.Height > this.Height)
			{
				int delta = m_vBar.SmallChange * 3;
				delta = (delta < SCROLL_SMALLCHANGE) ? SCROLL_SMALLCHANGE : delta;
				if (e.Delta > 0)
					delta *= -1;


				//�X�N���[���ʒu����
				Point p = AutoScrollPosition;
				p.Y = -p.Y + delta;
				AutoScrollPosition = p;
			}
		}


		/// <summary>
		///�X�N���[���o�[�l�ύX�C�x���g
		///MomentumScrollPanel�R���g���[���Ƃ��Ă̒l���ύX����
		/// </summary>
		void m_vBar_ValueChanged(object sender, EventArgs e)
		{
			//VScrollValue = m_vBar.Value;
			if (UseAnimation)
			{
				if (!m_scrollTimer.Enabled)
					m_scrollTimer.Start();
			}
			else
			{
				m_vScrollValue = m_vBar.Value;
				this.Refresh();
			}
		}

		void m_hBar_ValueChanged(object sender, EventArgs e)
		{
			//HScrollValue = m_hBar.Value;
			if (UseAnimation)
			{
				if (!m_scrollTimer.Enabled)
					m_scrollTimer.Start();
			}
			else
			{
				m_hScrollValue = m_hBar.Value;
				this.Refresh();
			}
		}

		/// <summary>
		/// �����X�N���[���p�^�C�}�[�C�x���g
		/// ����̃��C�����[�`��
		/// �^�[�Q�b�g�l(m_vBar.Value)�֊������Ȃ���߂Â��Ă���
		/// </summary>
		void m_scrollTimer_Tick(object sender, EventArgs e)
		{
			//�c����
			//diff: �O��̕`�ʈʒu�Ƃ̍���
			//	m_vBar.Value : �^�[�Q�b�g�l
			//  m_ScrollValue: �O��̕`�ʒl�A�`�ʂ����l
			int diff = m_vBar.Value - m_vScrollValue;

			//if (Math.Abs(diff) <= 1)
			if (Math.Abs(diff) <= MINIMUM_SCROLL)
			{
				//1�h�b�g�ȉ��̃X�N���[���ƂȂ�����^�C�}�[�I��
				m_vScrollValue = m_vBar.Value;
			}
			else
			{
				//�����X�N���[���������F�ȈՎ���
				int add = (int)(diff * MOMENTUM_FORCE);
				//if (add == 0)
				//    add = Math.Sign(diff);	//1�C-1��������
				if (Math.Abs(add) < MINIMUM_SCROLL)
					add = Math.Sign(add)*MINIMUM_SCROLL;

				m_vScrollValue += add;
			}


			//������
			//diff: �O��̕`�ʈʒu�Ƃ̍���
			//	m_vBar.Value : �^�[�Q�b�g�l
			//  m_ScrollValue: �O��̕`�ʒl�A�`�ʂ����l
			diff = m_hBar.Value - m_hScrollValue;

			if (Math.Abs(diff) <= 1)
			{
				//1�h�b�g�ȉ��̃X�N���[���ƂȂ�����^�C�}�[�I��
				m_hScrollValue = m_hBar.Value;
			}
			else
			{
				//�����X�N���[���������F�ȈՎ���
				int add = (int)(diff * MOMENTUM_FORCE);
				if (add == 0)
					add = Math.Sign(diff);	//1�C-1��������
				m_hScrollValue += add;
			}

			//�I���m�F
			if (m_vScrollValue == m_vBar.Value &&
				m_hScrollValue == m_hBar.Value)
			{
				m_scrollTimer.Stop();
				m_scrollTimer.Enabled = false;
			}

			this.Refresh();
		}

		/// <summary>
		/// �T�C�Y�ύX�C�x���g����
		/// �T�C�Y�ύX���������Ƃ��̓X�N���[���o�[��LargeChange��ύX
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			m_vBar.LargeChange = this.Height;
			m_hBar.LargeChange = this.Width;
			CheckScrollbarDraw();
		}
	}
}

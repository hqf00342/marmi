using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					//Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//UserControl
using System.Drawing.Imaging;			//PixelFormat, ColorMatrix
using System.Drawing.Drawing2D;			//GraphicsPath
using System.IO;						//Directory, File
using System.Threading;					//ThreadPool, WaitCallback


namespace Marmi
{

	enum ThumnailMode
	{
		Square,		// �����`
		Height		// �����������킹��
	}

	/// <summary>
	/// �T���l�C����p�C�x���g�̒�`
	///   �T���l�C�����Ƀ}�E�X�z�o�[���N�����Ƃ��̂��߂̃C�x���g
	///   ���̃C�x���g��ThumbnailPanel::ThumbnailPanel_MouseMove()�Ŕ������Ă���
	///   �󂯂鑤�͂���EventArgs���g���Ď󂯂�ƃA�C�e����������B
	/// </summary>
	public class ThumbnailEventArgs : EventArgs
	{
		public int HoverItemNumber;		//Hover���̃A�C�e���ԍ�
		public string HoverItemName;	//Hover���̃A�C�e����
	}

	
	public class ThumbnailPanel : UserControl
	{
		//���ʕϐ��̒�`
		private Bitmap m_offScreen;					//Bitmap. new���Ċm�ۂ����
		private VScrollBar m_vScrollBar;			//�X�N���[���o�[�R���g���[��
		private List<ImageInfo> m_thumbnailSet;		//ImageInfo�̃��X�g
		private FormSaveThumbnail m_saveForm;		//�T���l�C���ۑ��p�_�C�A���O
		private Size m_virtualScreenSize;			//���z�T���l�C���̃T�C�Y
		
		//ver 0.994 �g��Ȃ����Ƃɂ���
		//private int m_nItemsX;						//offScreen�ɕ��ԃA�C�e���̐�: SetScrollBar()�Ōv�Z
		//private int m_nItemsY;						//offScreen�ɕ��ԃA�C�e���̐�: SetScrollBar()�Ōv�Z

		enum ThreadStatus
		{
			STOP,
			RUNNING,
			REQUEST_STOP
		}
		static ThreadStatus tStatus;				//�X���b�h�̏󋵂�����
		private bool m_needHQDraw;					//�n�C�N�I���e�B�`�ʂ����{�ς݂�
		private int m_mouseHoverItem = -1;			//���݃}�E�X���z�o�[���Ă���A�C�e��
		private Font m_font;						//���x����������̂͂��������Ȃ��̂�
		private Color m_fontColor;					//�t�H���g�̐F
		private ToolTip m_tooltip;					//�c�[���`�b�v�B�摜����\������
		private System.Windows.Forms.Timer m_timer;	//�c�[���`�b�v�\���p�^�C�}�[

		//ver0.994 �T���l�C�����[�h
		private ThumnailMode m_thumbnailMode;

		private NamedBuffer<int, Bitmap> m_HQcache;		//�傫�ȃT���l�C���p�L���b�V��

		//�v���p�e�B�̐ݒ�
		public List<ImageInfo> thumbnailImageSet
		{
			set { m_thumbnailSet = value; }
		}

		//const int PADDING = 10;
		const int PADDING = 3;	//2011�N7��25���ύX�B������ƊԊu�J������
		//const int DEFAULT_THUMBNAIL_SIZE = 160;

		private int THUMBNAIL_SIZE;	//�T���l�C���̑傫���B���ƍ����͓���l
		private int BOX_WIDTH;		//�{�b�N�X�̕��BPADDING + THUMBNAIL_SIZE + PADDING
		private int BOX_HEIGHT;		//�{�b�N�X�̍����BPADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING
		private int FONT_HEIGHT;	//FONT�̍����B

		//��p�C�x���g�̒�`
		public delegate void ThumbnailEventHandler(object obj, ThumbnailEventArgs e);
		public event ThumbnailEventHandler OnHoverItemChanged;	//�}�E�XHover�ŃA�C�e�����ւ�������Ƃ�m�点��B
		public event ThumbnailEventHandler SavedItemChanged;	//


		//*** �R���X�g���N�^ ********************************************************************

		public ThumbnailPanel()
		{
			//������
			this.BackColor = Color.White;	//Color.FromArgb(100, 64, 64, 64);

			//m_offScreen = null;
			tStatus = ThreadStatus.STOP;
			m_thumbnailMode = ThumnailMode.Square;

			//�c�[���`�b�v�̏�����
			m_tooltip = new ToolTip();		//ToolTip�𐶐�
			m_tooltip.InitialDelay = 500;	//ToolTip���\�������܂ł̎���
			m_tooltip.ReshowDelay = 500;	//ToolTip���\������Ă��鎞�ɁA�ʂ�ToolTip��\������܂ł̎���
			m_tooltip.AutoPopDelay = 1000;	//ToolTip��\�����鎞��
			m_tooltip.ShowAlways = false;	//�t�H�[�����A�N�e�B�u�łȂ����ł�ToolTip��\������

			//�c�[���`�b�v�^�C�}�[�̏�����
			m_timer = new System.Windows.Forms.Timer();
			m_timer.Interval = 1000;
			m_timer.Tick += new EventHandler(m_timer_Tick);

			//�C�x���g�����ݒ�
			//this.Paint += new PaintEventHandler(OnPaint);
			//this.Resize += new EventHandler(OnResize);
			//this.MouseMove += new MouseEventHandler(ThumbnailPanel_MouseMove);
			//this.MouseLeave += new EventHandler(ThumbnailPanel_MouseLeave);
			//this.MouseWheel += new MouseEventHandler(ThumbnailPanel_MouseWheel);
			//this.MouseHover += new EventHandler(ThumbnailPanel_MouseHover);


			//�X�N���[���o�[�̏�����
			m_vScrollBar = new VScrollBar();
			this.Controls.Add(m_vScrollBar);
			m_vScrollBar.Dock = DockStyle.Right;
			m_vScrollBar.Visible = false;
			m_vScrollBar.Enabled = false;
			//m_vScrollBar.Scroll += new ScrollEventHandler(vScrollBar1_Scroll);	//ver0.9832
			m_vScrollBar.ValueChanged += new EventHandler(m_vScrollBar_ValueChanged);	//ver0.9832

			//�_�u���o�b�t�@
			this.SetStyle(
				ControlStyles.AllPaintingInWmPaint
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.UserPaint,
				true);

			//�t�H���g����
			SetFont(new Font("�l�r �S�V�b�N", 9), Color.Black);

			//�T���l�C���T�C�Y����BOX�̒l�����肷��B
			SetThumbnailSize(Form1.DEFAULT_THUMBNAIL_SIZE);

			//�傫�ȃT���l�C���p�L���b�V��
			m_HQcache = new NamedBuffer<int, Bitmap>();
		}


		~ThumbnailPanel()
		{
			//this.Paint -= new PaintEventHandler(OnPaint);
			//this.Resize -= new EventHandler(OnResize);
			//this.MouseMove -= new MouseEventHandler(ThumbnailPanel_MouseMove);
			//this.MouseLeave -= new EventHandler(ThumbnailPanel_MouseLeave);
			//this.MouseWheel -= new MouseEventHandler(ThumbnailPanel_MouseWheel);

			//m_vScrollBar.Scroll -= new ScrollEventHandler(vScrollBar1_Scroll);
			m_vScrollBar.ValueChanged -= new EventHandler(m_vScrollBar_ValueChanged);	//ver0.9832
			m_vScrollBar.Dispose();
			m_font.Dispose();
			m_tooltip.Dispose();

			m_timer.Tick -= new EventHandler(m_timer_Tick);
			m_timer.Dispose();

			m_HQcache.Clear();
		}


		// override
		//***************************************************************
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			//���T�C�Y���s��ꂽ��o�b�N�X�N���[������蒼���B

			//�E�B���h�E�T�C�Y���O�ɂȂ邱�Ƃ�z��B
			//TODO:�E�B���h�E�T�C�Y�̍ŏ��l�����߂�K�v�L��B
			if (this.Width == 0 || this.Height == 0)
				return;

			//�I�t�X�N���[�����Đݒ�
			if (m_offScreen == null)
				m_offScreen = new Bitmap(this.Width, this.Height);
			else
			{
				lock (m_offScreen)
				{
					m_offScreen.Dispose();
					m_offScreen = new Bitmap(this.Width, this.Height);
				}
			}

			//���T�C�Y�ɔ�����ʂɕ\���A�C�e�������ς��̂ōČv�Z
			setScrollBar();

			MakeThumbnailScreen();
			this.Invalidate();
			return;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);
			if (m_offScreen == null)
			{
				//�\�����������Ǎ���ĂȂ��݂����E�E�E
				m_offScreen = new Bitmap(this.Width, this.Height);
				setScrollBar();
				MakeThumbnailScreen();
				//return;
			}
			lock (m_offScreen)
			{
				e.Graphics.DrawImageUnscaled(m_offScreen, 0, 0);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			//�A�C�e����1���Ȃ��Ƃ��͉������Ȃ�
			if (m_thumbnailSet == null)
				return;

			//�}�E�X�ʒu���N���C�A���g���W�Ŏ擾
			Point pos = this.PointToClient(Cursor.Position);
			int ItemIndex = GetHoverItem(pos);	//�z�o�[���̃A�C�e���ԍ�
			if (ItemIndex == m_mouseHoverItem)
			{
				//�}�E�X���z�o�[���Ă���A�C�e�����ς��Ȃ��Ƃ��͉������Ȃ��B
				return;
			}

			//�z�o�[�A�C�e�����ς���Ă���̂Ńc�[���`�b�v�p�^�C�}�[���~�߂�
			if (m_timer.Enabled)
				m_timer.Stop();

			//ToolTip������
			m_tooltip.Hide(this);

			using (Graphics g = this.CreateGraphics())
			{
				//�܂����܂ł̐�������
				if (m_mouseHoverItem != -1)
				{

					Rectangle vanishRect = GetThumbboxRectanble(m_mouseHoverItem);
					//this.Invalidate(vanishRect);		//�Ώۋ�`���̂�`�ʂ�����
					lock (m_offScreen)
					{
						g.DrawImage(m_offScreen, vanishRect, vanishRect, GraphicsUnit.Pixel);
					}
				}

				//�w��|�C���g�ɃA�C�e�������邩
				//if (nx > m_nItemsX - 1 || ItemIndex > m_thumbnailSet.Count - 1 || ItemIndex < 0)
				if (ItemIndex < 0)
				{
					m_mouseHoverItem = -1;
					return;
				}


				//�t�H�[�J�X�g������
				// �摜�T�C�Y�ɍ��킹�ĕ`��
				Rectangle r = GetThumbImageRectangle(ItemIndex);
				//r.Inflate(2, 2);	//2�s�N�Z���g��
				g.DrawRectangle(new Pen(Color.IndianRed, 2.5F), r);

			}

			//�z�o�[�A�C�e�����ւ�������Ƃ�`����
			m_mouseHoverItem = ItemIndex;

			//Hover���Ă���A�C�e�����ւ�������Ƃ������C�x���g�𔭐�������
			//���̃C�x���g�̓��C��Form�Ŏ󂯎��StatusBar�̕\����ς���B
			ThumbnailEventArgs he = new ThumbnailEventArgs();
			he.HoverItemNumber = m_mouseHoverItem;
			he.HoverItemName = m_thumbnailSet[m_mouseHoverItem].filename;
			this.OnHoverItemChanged(this, he);

			//ToolTip��\������
			string sz = String.Format(
				"{0}\n ���t: {1:yyyy�NM��d�� H:m:s}\n �傫��: {2:N0}bytes\n �T�C�Y: {3:N0}x{4:N0}�s�N�Z��",
				m_thumbnailSet[ItemIndex].filename,
				m_thumbnailSet[ItemIndex].CreateDate,
				m_thumbnailSet[ItemIndex].length,
				m_thumbnailSet[ItemIndex].originalWidth,
				m_thumbnailSet[ItemIndex].originalHeight
			);
			//m_tooltip.Show(sz, this, e.Location, 3000);
			m_tooltip.Tag = sz;
			m_timer.Start();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			//�A�C�e����1���Ȃ��Ƃ��͉������Ȃ�
			if (m_thumbnailSet == null)
				return;

			if (m_mouseHoverItem != -1)
			{
				this.Invalidate();		//�Ώۋ�`���̂�`�ʂ�����
				m_mouseHoverItem = -1;
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			//base.OnMouseWheel(e);
			if (m_vScrollBar.Enabled)
			{
				//���݂̒l������Ă���
				int preValue = m_vScrollBar.Value;

				//�ړ�������������擾�Be.Delta�͉��X�N���[����-120�A���120���o���̂Ŕ��]
				int delta = 1;
				if (e.Delta > 0) delta = -1;

				//�V�����l���v�Z�B �ړ��l��SmallChange�̂Q�`�R�{���x���悳����
				//���Ȃ��l�ƂȂ�悤�Ɍ��؂���
				int newValue = preValue + delta * m_vScrollBar.SmallChange * 2;
				if (newValue < m_vScrollBar.Minimum)
					newValue = m_vScrollBar.Minimum;
				if (newValue > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
					newValue = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;

				//�X�N���[������K�v������΃X�N���[���C�x���g�𒼐ڌĂ�
				if (preValue != newValue)
				{
					m_vScrollBar.Value = newValue;
					//vScrollBar1_Scroll(null, null);
					//m_vScrollBar_ValueChanged(null, null);	//ver0.9832
				}
			}
		}

		public void OnMouseWheel(object sender, MouseEventArgs e)
		{
			OnMouseWheel(e);
		}

		//*** �C�x���g�ݒ� **********************************************************************


		//void OnResize(object sender, EventArgs e)
		//{
		//    //���T�C�Y���s��ꂽ��o�b�N�X�N���[������蒼���B

		//    //�E�B���h�E�T�C�Y���O�ɂȂ邱�Ƃ�z��B
		//    //TODO:�E�B���h�E�T�C�Y�̍ŏ��l�����߂�K�v�L��B
		//    if (this.Width == 0 || this.Height == 0)
		//        return;

		//    //�I�t�X�N���[�����Đݒ�
		//    if (m_offScreen == null)
		//        m_offScreen = new Bitmap(this.Width, this.Height);
		//    else
		//    {
		//        lock (m_offScreen)
		//        {
		//            m_offScreen.Dispose();
		//            m_offScreen = new Bitmap(this.Width, this.Height);
		//        }
		//    }

		//    //���T�C�Y�ɔ�����ʂɕ\���A�C�e�������ς��̂ōČv�Z
		//    setScrollBar();

		//    MakeThumbnailScreen();
		//    this.Invalidate();
		//    return;
		//}

		//void OnPaint(object sender, PaintEventArgs e)
		//{

		//    if (m_offScreen == null)
		//    {
		//        //�\�����������Ǎ���ĂȂ��݂����E�E�E
		//        m_offScreen = new Bitmap(this.Width, this.Height);
		//        setScrollBar();
		//        MakeThumbnailScreen();
		//        //return;
		//    }
		//    lock (m_offScreen)
		//    {
		//        e.Graphics.DrawImageUnscaled(m_offScreen, 0, 0);
		//    }
		//}

		//void ThumbnailPanel_MouseLeave(object sender, EventArgs e)
		//{
		//    //�A�C�e����1���Ȃ��Ƃ��͉������Ȃ�
		//    if (m_thumbnailSet == null)
		//        return;

		//    if (m_mouseHoverItem != -1)
		//    {
		//        this.Invalidate();		//�Ώۋ�`���̂�`�ʂ�����
		//        m_mouseHoverItem = -1;
		//    }
		//}

		//void ThumbnailPanel_MouseMove(object sender, MouseEventArgs e)
		//{
		//    //�A�C�e����1���Ȃ��Ƃ��͉������Ȃ�
		//    if (m_thumbnailSet == null)
		//        return;

		//    //�}�E�X�ʒu���N���C�A���g���W�Ŏ擾
		//    Point pos = this.PointToClient(Cursor.Position);
		//    int ItemIndex = GetHoverItem(pos);	//�z�o�[���̃A�C�e���ԍ�
		//    if (ItemIndex == m_mouseHoverItem)
		//    {
		//        //�}�E�X���z�o�[���Ă���A�C�e�����ς��Ȃ��Ƃ��͉������Ȃ��B
		//        return;
		//    }

		//    //�z�o�[�A�C�e�����ς���Ă���̂Ńc�[���`�b�v�p�^�C�}�[���~�߂�
		//    if (m_timer.Enabled)
		//        m_timer.Stop();

		//    //ToolTip������
		//    m_tooltip.Hide(this);

		//    using (Graphics g = this.CreateGraphics())
		//    {
		//        //�܂����܂ł̐�������
		//        if (m_mouseHoverItem != -1)
		//        {

		//            Rectangle vanishRect = GetThumbboxRectanble(m_mouseHoverItem);
		//            //this.Invalidate(vanishRect);		//�Ώۋ�`���̂�`�ʂ�����
		//            lock (m_offScreen)
		//            {
		//                g.DrawImage(m_offScreen, vanishRect, vanishRect, GraphicsUnit.Pixel);
		//            }
		//        }

		//        //�w��|�C���g�ɃA�C�e�������邩
		//        //if (nx > m_nItemsX - 1 || ItemIndex > m_thumbnailSet.Count - 1 || ItemIndex < 0)
		//        if (ItemIndex < 0)
		//        {
		//            m_mouseHoverItem = -1;
		//            return;
		//        }


		//        //�t�H�[�J�X�g������
		//        // �摜�T�C�Y�ɍ��킹�ĕ`��
		//        Rectangle r = GetThumbImageRectangle(ItemIndex);
		//        //r.Inflate(2, 2);	//2�s�N�Z���g��
		//        g.DrawRectangle(new Pen(Color.IndianRed, 2.5F), r);

		//    }

		//    //�z�o�[�A�C�e�����ւ�������Ƃ�`����
		//    m_mouseHoverItem = ItemIndex;

		//    //Hover���Ă���A�C�e�����ւ�������Ƃ������C�x���g�𔭐�������
		//    //���̃C�x���g�̓��C��Form�Ŏ󂯎��StatusBar�̕\����ς���B
		//    ThumbnailEventArgs he = new ThumbnailEventArgs();
		//    he.HoverItemNumber = m_mouseHoverItem;
		//    he.HoverItemName = m_thumbnailSet[m_mouseHoverItem].filename;
		//    this.OnHoverItemChanged(this, he);

		//    //ToolTip��\������
		//    string sz = String.Format(
		//        "{0}\n ���t: {1:yyyy�NM��d�� H:m:s}\n �傫��: {2:N0}bytes\n �T�C�Y: {3:N0}x{4:N0}�s�N�Z��",
		//        m_thumbnailSet[ItemIndex].filename,
		//        m_thumbnailSet[ItemIndex].CreateDate,
		//        m_thumbnailSet[ItemIndex].length,
		//        m_thumbnailSet[ItemIndex].originalWidth,
		//        m_thumbnailSet[ItemIndex].originalHeight
		//    );
		//    //m_tooltip.Show(sz, this, e.Location, 3000);
		//    m_tooltip.Tag = sz;
		//    m_timer.Start();
		//}

		//public void ThumbnailPanel_MouseWheel(object sender, MouseEventArgs e)
		//{
		//    if (m_vScrollBar.Enabled)
		//    {
		//        //���݂̒l������Ă���
		//        int preValue = m_vScrollBar.Value;

		//        //�ړ�������������擾�Be.Delta�͉��X�N���[����-120�A���120���o���̂Ŕ��]
		//        int delta = 1;
		//        if (e.Delta > 0) delta = -1;

		//        //�V�����l���v�Z�B �ړ��l��SmallChange�̂Q�`�R�{���x���悳����
		//        //���Ȃ��l�ƂȂ�悤�Ɍ��؂���
		//        int newValue = preValue + delta * m_vScrollBar.SmallChange * 2;
		//        if (newValue < m_vScrollBar.Minimum)
		//            newValue = m_vScrollBar.Minimum;
		//        if (newValue > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
		//            newValue = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;

		//        //�X�N���[������K�v������΃X�N���[���C�x���g�𒼐ڌĂ�
		//        if (preValue != newValue)
		//        {
		//            m_vScrollBar.Value = newValue;
		//            //vScrollBar1_Scroll(null, null);
		//            m_vScrollBar_ValueChanged(null, null);	//ver0.9832
		//        }
		//    }
		//}

		/// <summary>
		/// �A�C�h������Form����Ăяo����郋�[�`��
		/// ���i���`�ʂ����������B
		/// </summary>
		public void Application_Idle()
		{
			//�T���l�C���\���̏������o���Ă��邩
			//if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
			if (m_offScreen == null)
			{
				//�������o���Ă��Ȃ��ꍇ�͓��ɉ��������I��
				Debug.WriteLine("�����ł��ĂȂ���", " Application_Idle()");
				return;
			}

			if (THUMBNAIL_SIZE > Form1.DEFAULT_THUMBNAIL_SIZE
				&& m_needHQDraw == true)
			{
				WaitCallback callback = new WaitCallback(callbackHQThumbnailThreadProc);
				ThreadPool.QueueUserWorkItem(callback);
				//ThreadProc(null);	//�X���b�h�������ɂ��̂܂܌Ăяo���B
				m_needHQDraw = false;	//���i���`�ʂ͎n�܂����̂Ńt���O������
			}
		}


		//*** ������ ****************************************************************************

		public void Init()
		{
			//�t�@�C�����ēǂݍ��݂��ꂽ�Ƃ��ȂǂɌĂяo�����
			m_vScrollBar.Value = 0;
			m_vScrollBar.Visible = false;
			m_vScrollBar.Enabled = false;
			m_needHQDraw = false;

			//m_nItemsX = 0;
			//m_nItemsY = 0;

			m_HQcache.Clear();			//ver0.974
			//m_thumbnailSet.Clear();		//ver0.974 �|�C���^�����Ă��邾���Ȃ̂ł����ł��Ȃ�
		}

		/// <summary>
		/// �T���l�C���摜�P�̃T�C�Y��ύX����
		/// option Form�ŕύX���ꂽ���ƍĐݒ肳��邱�Ƃ�z��
		/// </summary>
		/// <param name="ThumbnailSize">�V�����T���l�C���T�C�Y</param>
		public void SetThumbnailSize(int ThumbnailSize)
		{
			//ver0.982 HQcache�������N���A�����̂ŕύX
			//�T���l�C���T�C�Y���ς���Ă�����ύX����
			if (THUMBNAIL_SIZE != ThumbnailSize)
			{
				THUMBNAIL_SIZE = ThumbnailSize;

				//���𑜓x�L���b�V�����N���A
				if (m_HQcache != null)
					m_HQcache.Clear();
			}

			//BOX�T�C�Y���m��
			BOX_WIDTH = THUMBNAIL_SIZE + PADDING * 2;
			//BOX_HEIGHT = THUMBNAIL_SIZE + PADDING * 3 + TEXT_HEIGHT;
			BOX_HEIGHT = THUMBNAIL_SIZE + PADDING * 2;

			//ver0.982�t�@�C�����Ȃǂ̕�����\����؂�ւ�����悤�ɂ���
			#region ver0.982
			if (Form1.g_Config.isShowTPFileName)
				BOX_HEIGHT += PADDING + FONT_HEIGHT;

			if(Form1.g_Config.isShowTPFileSize)
				BOX_HEIGHT += PADDING + FONT_HEIGHT;

			if(Form1.g_Config.isShowTPPicSize)
				BOX_HEIGHT += PADDING + FONT_HEIGHT;
			#endregion


			//�T���l�C���T�C�Y���ς��Ɖ�ʂɕ\���ł���
			//�A�C�e�������ς��̂ōČv�Z
			setScrollBar();
		}

		public void SetFont(Font f, Color fc)
		{
			m_font = f;
			m_fontColor = fc;

			//TEXT_HEIGHT�̌���
			using (Bitmap bmp = new Bitmap(100, 100))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					SizeF sf = g.MeasureString("�e�X�g������", m_font);
					FONT_HEIGHT = (int)sf.Height;
				}
			}

			//�t�H���g���ς��ƃT���l�C���T�C�Y���ς��̂Ōv�Z
			SetThumbnailSize(THUMBNAIL_SIZE);
		}

		public int GetHoverItem(Point pos)
		{
			//�c�X�N���[���o�[���\������Ă���Ƃ��͊��Z
			if (m_vScrollBar.Enabled)
				pos.Y += m_vScrollBar.Value;

			int nx = pos.X / BOX_WIDTH;		//�}�E�X�ʒu��BOX���W���Z�FX
			int ny = pos.Y / BOX_HEIGHT;	//�}�E�X�ʒu��BOX���W���Z�FY

			//���ɕ��ׂ��鐔�B�Œ�P
			int numX;						//�������̃A�C�e�����B�ŏ��l���P
			if (m_vScrollBar.Enabled)
				numX = (this.ClientRectangle.Width - m_vScrollBar.Width) / BOX_WIDTH;
			else
				numX = this.ClientRectangle.Width / BOX_WIDTH;
			if (numX <= 0)
				numX = 1;

			int num = ny * numX + nx;		//�z�o�[���̃A�C�e���ԍ�

			//�w��|�C���g�ɃA�C�e�������邩
			if (nx > numX - 1 || num > m_thumbnailSet.Count - 1)
				return -1;
			else
				return num;
		}


		//*** �X�N���[���o�[ ********************************************************************

		/// <summary>
		/// �X�N���[���o�[�̊�{�ݒ�
		/// �X�N���[���o�[��\�����邩�ǂ����𔻕ʂ��A�K�v�ɉ����ĕ\���A�ݒ肷��B
		/// �K�v���Ȃ��ꍇ��Value���O�ɐݒ肵�Ă����B
		/// ��Ƀ��T�C�Y�C�x���g�����������Ƃ��ɌĂяo�����
		/// </summary>
		private void setScrollBar()
		{
			//�X�N���[���o�[��value�̂Ƃ�l��
			// Minimum �` (value) �` (Maximum-LargeChange)
			//
			//�܂�Maximum�ɂ͖{���̍ő�l��ݒ肷��B�i��P�O�O�j
			//LargeChange�͕\���\����ݒ肷��B�i��F�P�O�j
			//�����Value�͂O�`�X�P�������悤�ɂȂ�B

			//�������ς݂��m�F
			if (m_thumbnailSet == null)
				return;

			//�A�C�e�������m�F
			int ItemCount = m_thumbnailSet.Count;

			//�`�ʂɕK�v�ȃT�C�Y���m�F����B
			//�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
			m_virtualScreenSize = calcScreenSize();

			//offScreen�̕����傫���ꍇ�̓X�N���[���o�[���K�v�B
			if (m_virtualScreenSize.Height > this.Height)
			{
				//�X�N���[���o�[�̃v���p�e�B��ݒ�
				m_vScrollBar.Minimum = 0;						//�ŏ��l
				m_vScrollBar.Maximum = m_virtualScreenSize.Height;	//�ő�l
				m_vScrollBar.LargeChange = this.Height;			//�󔒕������������Ƃ�
				m_vScrollBar.SmallChange = this.Height / 10;	//�����������Ƃ�
				if (m_vScrollBar.Value > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
					m_vScrollBar.Value = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;

				//�L���E����
				m_vScrollBar.Enabled = true;
				m_vScrollBar.Visible = true;
			}
			else
			{
				//�X�N���[���o�[�s�v�BValue=0�ɂ��Ă���
				m_vScrollBar.Visible = false;
				m_vScrollBar.Enabled = false;
				m_vScrollBar.Value = 0;
			}

			//Debug.WriteLine(string.Format("setScrollBar(value,min,max,)=({0},{1},{2})", m_vScrollBar.Value, m_vScrollBar.Minimum, m_vScrollBar.Maximum));
		}


		//ver0.994 �I���W�i����ۑ�
		// �T���l�C���摜�\���̍œK��������
		//
		///// <summary>
		///// �X�N���[���o�[�̊�{�ݒ�
		///// �X�N���[���o�[��\�����邩�ǂ����𔻕ʂ��A�K�v�ɉ����ĕ\���A�ݒ肷��B
		///// �K�v���Ȃ��ꍇ��Value���O�ɐݒ肵�Ă����B
		///// ��Ƀ��T�C�Y�C�x���g�����������Ƃ��ɌĂяo�����
		///// </summary>
		//private void setScrollBar()
		//{
		//    //�X�N���[���o�[��value�̂Ƃ�l��
		//    // Minimum �` (value) �` (Maximum-LargeChange)
		//    //
		//    //�܂�Maximum�ɂ͖{���̍ő�l��ݒ肷��B�i��P�O�O�j
		//    //LargeChange�͕\���\����ݒ肷��B�i��F�P�O�j
		//    //�����Value�͂O�`�X�P�������悤�ɂȂ�B

		//    //�������ς݂��m�F
		//    if (m_thumbnailSet == null)
		//        return;

		//    //�A�C�e�������m�F
		//    int ItemCount = m_thumbnailSet.Count;

		//    //�`�ʂɕK�v�ȃT�C�Y���m�F����B
		//    //�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
		//    int screenWidth = this.Width;
		//    int screenHeight = this.Height;
		//    if (screenWidth < 1) screenWidth = 1;
		//    if (screenHeight < 1) screenHeight = 1;

		//    //���ɕ��ׂ��鐔�B�Œ�P
		//    m_nItemsX = screenWidth / BOX_WIDTH;	//���ɕ��ԃA�C�e����
		//    if (m_nItemsX == 0) m_nItemsX = 1;		//�Œ�ł��P�ɂ���

		//    //�c�ɕK�v�Ȑ��B�J��グ��
		//    m_nItemsY = ItemCount / m_nItemsX;	//�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
		//    if (ItemCount % m_nItemsX > 0)
		//        m_nItemsY++;						//����؂�Ȃ������ꍇ��1�s�ǉ�

		//    //offScreen�̕����������ꍇ�̓X�N���[���o�[���K�v�B�Čv�Z
		//    if (screenHeight < m_nItemsY * BOX_HEIGHT)
		//    {
		//        //�X�N���[���o�[���K�v�Ȃ̂ōČv�Z
		//        m_nItemsX = (screenWidth - m_vScrollBar.Width) / BOX_WIDTH;	//�Čv�Z
		//        if (m_nItemsX == 0) m_nItemsX = 1;		//�Œ�P
		//        m_nItemsY = (ItemCount + m_nItemsX - 1) / m_nItemsX;	//(numX-1)�����炩���ߑ����Ă������ƂŌJ��グ

		//        //�X�N���[���o�[�̃v���p�e�B��ݒ�
		//        m_vScrollBar.Minimum = 0;						//�ŏ��l
		//        m_vScrollBar.Maximum = m_nItemsY * BOX_HEIGHT;	//�ő�l
		//        m_vScrollBar.LargeChange = screenHeight;			//�󔒕������������Ƃ�
		//        m_vScrollBar.SmallChange = screenHeight / 10;	//�����������Ƃ�
		//        if (m_vScrollBar.Value > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
		//            m_vScrollBar.Value = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;
		//        m_vScrollBar.Enabled = true;
		//        m_vScrollBar.Visible = true;
		//    }
		//    else
		//    {
		//        //�X�N���[���o�[�s�v�BValue=0�ɂ��Ă���
		//        m_vScrollBar.Visible = false;
		//        m_vScrollBar.Enabled = false;
		//        m_vScrollBar.Value = 0;
		//    }

		//    Debug.WriteLine(string.Format("setScrollBar(value,min,max,)=({0},{1},{2})", m_vScrollBar.Value, m_vScrollBar.Minimum, m_vScrollBar.Maximum));
		//}



		/// <summary>
		/// �X�N���[���T�C�Y���v�Z����
		/// �c�������傫����΃X�N���[���o�[���K�v�Ƃ�������
		/// �X�N���[���o�[�͍ŏ�����T�C�Y�Ƃ��čl��
		/// TODO �X�N���[���o�[���͕K�v�ɉ����čl������
		/// </summary>
		private Size calcScreenSize()
		{
			//�A�C�e�������m�F
			int ItemCount = m_thumbnailSet.Count;

			//�`�ʂɕK�v�ȃT�C�Y���m�F����B
			//�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
			int screenWidth = this.Width;
			int screenHeight = this.Height;
			if (screenWidth < 1) screenWidth = 1;
			if (screenHeight < 1) screenHeight = 1;

			//�e�A�C�e���̈ʒu�����肷��
			int tempx = 0;
			int tempy = 0;

			//TODO:�X�N���[���T�C�Y��160�ȏ゠�邱�Ƃ��O��
			
			for (int i = 0; i < ItemCount; i++)
			{
				if ((tempx+THUMBNAIL_SIZE+PADDING) > (screenWidth - m_vScrollBar.Width))
				{
					tempx = 0;
					tempy += BOX_HEIGHT;
				}
				m_thumbnailSet[i].posX = tempx;
				m_thumbnailSet[i].posY = tempy;
				//Debug.WriteLine("ItemPos =" + tempx.ToString() +","+ tempy.ToString());
				tempx += THUMBNAIL_SIZE + PADDING;
			}

			//�摜�̍�������ǉ�
			screenHeight = tempy + BOX_HEIGHT;
			return new Size(screenWidth, screenHeight);
		}

		void m_vScrollBar_ValueChanged(object sender, EventArgs e)
		{
			//Debug.WriteLine(vScrollBar1.Value, "Value");
			MakeThumbnailScreen();	//�����`��
			this.Refresh();
		}


		//*** �`�ʃ��[�`�� **********************************************************************

		/// <summary>
		/// �S��ʏ����������[�`��
		/// OnResize, onPaint,�X�N���[�����ɌĂяo�����
		/// ver0.97�R�[�h�m�F�ς�
		/// </summary>
		private void MakeThumbnailScreen()
		{
			//�`�ʑΏۂ����邩�`�F�b�N����B������Δw�i�F�ɂ��Ė߂�
			if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
			{
				lock (m_offScreen)
				{
					using (Graphics g = Graphics.FromImage(m_offScreen))
					{
						g.Clear(this.BackColor);
					}
				}
				return;
			}

			//�X���b�h���~�߂�
			CancelHQThumbnailThread();

			//�A�C�e�������J�E���g
			int ItemCount = m_thumbnailSet.Count;

			calcScreenSize();

			//m_offScreen�ւ̕`��
			//�h��Ԃ�����ɕK�v�ȃA�C�e����`�ʂ��Ă���
			lock (m_offScreen)
			{
				using (Graphics g = Graphics.FromImage(m_offScreen))
				{
					//�w�i�F�œh��Ԃ�
					g.Clear(this.BackColor);

					//�`�ʐ擪�A�C�e���ƍŏI�A�C�e�����v�Z
					// X:�`�ʂ��ׂ�����̃A�C�e���ԍ�
					// Z:�`�ʂ��ׂ��E���̃A�C�e���ԍ��{�P
					//int vScValue = m_vScrollBar.Value;				//�X�N���[���o�[�̒l�B�悭�g���̂ŕϐ���
					//int X = (vScValue / BOX_HEIGHT) * m_nItemsX;
					//int Z = (vScValue + Height) / BOX_HEIGHT * m_nItemsX + m_nItemsX;
					//if (Z > ItemCount)
					//    Z = ItemCount;
					////�֌W�������ȃA�C�e�������`��
					//for (int Item = X; Item < Z; Item++)
					//{
					//    DrawItem3(g, Item);
					//}

					//�`�ʂ��ׂ��A�C�e�������`�ʂ���
					for (int item = 0; item < m_thumbnailSet.Count; item++)
					{
						//int tempY = m_thumbnailSet[item].posY;
						//int vScValue = m_vScrollBar.Value;				//�X�N���[���o�[�̒l�B�悭�g���̂ŕϐ���

						//if (tempY > vScValue - BOX_HEIGHT
						//    && tempY < vScValue + this.Height)
						//{
						//    DrawItem3(g, item);
						//}
						if(CheckNecessaryToDrawItem(item) == true)
							DrawItem3(g,item);

					}

				} //using(Graphics)
			}//lock

			//�~�߂��X���b�h��Application_Idle()�ōĊJ�����B
			//�����ł͉��������I������B
		}

		//�����`�ʑΉ�DrawItem.�O�����[�`����
		private void DrawItem3(Graphics g, int Item)
		{
			//�������o���Ă��邩
			//if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
			if (m_offScreen == null)
			{
				Debug.WriteLine("�����ł��ĂȂ���", " DrawItem3()");
				return;
			}

			//�`�ʕi��
			if (THUMBNAIL_SIZE > Form1.DEFAULT_THUMBNAIL_SIZE)
			{
				//�`�������̂ōŒ�i���ŕ`�ʂ���
				//g.InterpolationMode = InterpolationMode.NearestNeighbor;	
				g.InterpolationMode = InterpolationMode.Bilinear;			//���ꂮ�炢�̕i���ł�OK���H
				m_needHQDraw = true;	//�`�������t���O
			}
			else
				//�ŕW���T���l�C���T�C�Y�ȉ��͍��i���ŕ`��
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;

			//�`�ʈʒu�̌���
			Rectangle boxRect = GetThumbboxRectanble(Item);
		
			//�������`�������ʒu
			Rectangle tRect = new Rectangle(
				boxRect.X + PADDING,
				boxRect.Y + PADDING + THUMBNAIL_SIZE + PADDING,
				THUMBNAIL_SIZE,
				FONT_HEIGHT);
			
			//�Ώۋ�`��w�i�F�œh��Ԃ�.
			//�������Ȃ��ƑO�ɕ`�����A�C�R�����c���Ă��܂��\���L��
			g.FillRectangle(new SolidBrush(BackColor), boxRect);


			//�L���b�V��������΂��������g�� ver0.971
			if (m_needHQDraw && m_HQcache.ContainsKey(Item))
			{
				//�L���b�V�����ꂽ���i���摜��`��
				g.DrawImageUnscaled(m_HQcache[Item], GetThumbboxRectanble(Item));

				//�摜����`��
				//DrawTextInfo(g, Item, tRect); 
				DrawTextInfo(g, Item, boxRect);
				return;
			}

			Image DrawBitmap = m_thumbnailSet[Item].ThumbImage;
			bool drawFrame = true;
			if (DrawBitmap == null)
			{
				//�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
				DrawBitmap = Properties.Resources.rc_tif32;
				drawFrame = false;
			}
			Rectangle imageRect = GetThumbImageRectangle(Item);


			//�e��`�ʂ���.�A�C�R�����i��drawFrame==false�j�ŕ`�ʂ��Ȃ�
			if (Form1.g_Config.isDrawThumbnailShadow && drawFrame)
			{
				Rectangle frameRect = imageRect;
				BitmapUty.drawDropShadow(g, frameRect);
			}

			//�O�g������
			if (Form1.g_Config.isDrawThumbnailFrame  && drawFrame)
			{
				Rectangle frameRect = imageRect;
				//�g�����������̂Ŋg�債�Ȃ�
				//frameRect.Inflate(2, 2);
				g.FillRectangle(Brushes.White, frameRect);
				g.DrawRectangle(Pens.LightGray, frameRect);
			}



			//�摜������
			g.DrawImage(DrawBitmap, imageRect);

			//�摜��񕶎����`��
			//DrawTextInfo(g, Item, tRect);
			DrawTextInfo(g, Item, boxRect);

		}


		//���i����p�`��DrawItem. 
		//�_�~�[BMP�ɕ`�ʂ��邽�ߕ`�ʈʒu���Œ�Ƃ���B
		private void DrawItemHQ2(Graphics g, int Item)
		{
			//�������o���Ă��邩
			//if (m_nItemsX == 0 || m_nItemsY == 0 )
			if (m_offScreen == null)
			{
				Debug.WriteLine("�����ł��ĂȂ���", " DrawItemHQ2()");
				return;
			}

			//�Ώۋ�`��w�i�F�œh��Ԃ�.
			//�������Ȃ��ƑO�ɕ`�����A�C�R�����c���Ă��܂��\���L��
			g.FillRectangle(
				new SolidBrush(BackColor),
				//Brushes.LightYellow,
				0, 0, BOX_WIDTH, BOX_HEIGHT);

			//�`�ʕi�����ō���
			//���t�@�C���������Ă���. Bitmap��new���Ď����Ă���
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			//ver0.993 ���ꂾ��SevenZipSharp�ŃG���[���o��
			//Image DrawBitmap = new Bitmap(((Form1)Parent).GetBitmapWithoutCache(Item));

			//ver0.993 nullRefer�̌����ǋy
			//Ver0.993 2011�N7��31�����낢�남������
			//�܂��Ȃ��ocache����Ȃ��ƃ_���������̂�������Ȃ�
			//�G���[���o�錴���͂���ς�ʃX���b�h������̌Ăяo���݂���
			Bitmap DrawBitmap = null;

			if (Parent == null)
				//�e�E�B���h�E���Ȃ��Ȃ��Ă���̂ŉ������Ȃ�
				return;

			if (InvokeRequired)
			{
				this.Invoke(new MethodInvoker(delegate
				{
					DrawBitmap = (Bitmap)(((Form1)Parent).GetBitmap(Item)).Clone();
				}));
			}
			else
			{
				DrawBitmap = (Bitmap)(((Form1)Parent).GetBitmap(Item)).Clone();
			}


			//�t���O�ݒ�
			bool drawFrame = true;			//�g����`�ʂ��邩
			bool isResize = true;			//���T�C�Y���K�v���i�\���j�ǂ����̃t���O
			int w;							//�`�ʉ摜�̕�
			int h;							//�`�ʉ摜�̍���

			if (DrawBitmap == null)
			{
				//�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
				Debug.WriteLine(Item, "Image is not Ready");
				DrawBitmap = Properties.Resources.rc_tif32;
				drawFrame = false;
				isResize = false;
				w = DrawBitmap.Width;	//�`�ʉ摜�̕�
				h = DrawBitmap.Height;	//�`�ʉ摜�̍���
			}
			else
			{
				w = DrawBitmap.Width;	//�`�ʉ摜�̕�
				h = DrawBitmap.Height;	//�`�ʉ摜�̍���

				//���T�C�Y���ׂ����ǂ����m�F����B
				if (w <= THUMBNAIL_SIZE && h <= THUMBNAIL_SIZE)
					isResize = false;
			}


			//�����\�������郂�m�͏o���邾�������Ƃ���
			//if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
			if (isResize)
			{
				float ratio = 1;
				if (w > h)
					ratio = (float)THUMBNAIL_SIZE / (float)w;
				else
					ratio = (float)THUMBNAIL_SIZE / (float)h;
				//if (ratio > 1)			//������R�����g�������
				//    ratio = 1.0F;		//�g��`�ʂ��s��
				w = (int)(w * ratio);
				h = (int)(h * ratio);
			}

			int sx = (BOX_WIDTH - w) / 2;			//�摜�`��X�ʒu
			int sy = THUMBNAIL_SIZE + PADDING - h;	//�摜�`��Y�ʒu�F������

			//�ʐ^���ɊO�g������
			if (drawFrame)
			{
				Rectangle r = new Rectangle(sx, sy, w, h);
				//r.Inflate(2, 2);
				//g.FillRectangle(Brushes.White, r);
				//g.DrawRectangle(Pens.LightGray, r);
				BitmapUty.drawDropShadow(g, r);
			}

			//�摜������
			g.DrawImage(DrawBitmap, sx, sy, w, h);


			////�摜���𕶎��`�ʂ���
			//RectangleF tRect = new RectangleF(PADDING, PADDING + THUMBNAIL_SIZE + PADDING, THUMBNAIL_SIZE, TEXT_HEIGHT);
			//DrawTextInfo(g, Item, tRect);


			//Bitmap�̔j���BGetBitmapWithoutCache()�Ŏ���Ă�������
			if (DrawBitmap != null && (string)(DrawBitmap.Tag) != Properties.Resources.TAG_PICTURECACHE)
				DrawBitmap.Dispose();
		}


		//*** �X���b�h���� ********************************************************************

		/// <summary>
		/// Application_Idle()����X���b�h�v�[���o�^�����
		/// �Ăяo�����callback�֐�
		/// ��ʕ`�ʔ͈͂̃T���l�C����HQ�`�ʂ���B
		/// HQ�摜������ꍇ�͓������`�ʂ���Ă���͂�����
		/// �����ꍇ�͂����ō쐬����A�`�ʂ����B
		/// </summary>
		/// <param name="dummy">�g���Ȃ�</param>
		private void callbackHQThumbnailThreadProc(object dummy)
		{
			////�����ڕs��
			if (tStatus == ThreadStatus.RUNNING)
			{
				Debug.WriteLine("���s���Ȃ̂ɌĂ΂ꂽ", "ThreadProc()");
				return;
			}

			//�e�̃T���l�C���쐬�������Ă�����e����PAUSE
			//((Form1)Parent).PauseThreadPool();
			Form1.PauseThumbnailMakerThread();	//ver1.10 2011/08/19 static��


			//������
			int ItemCount = m_thumbnailSet.Count;
			tStatus = ThreadStatus.RUNNING;

			//�֌W�������ȃA�C�e�����v�Z
			int Width = this.Width;
			int Height = this.Height;
			int numItemX = Width / BOX_WIDTH;	//�������ɕ��ԃA�C�e����
			if (numItemX < 1) numItemX = 1;		//�Œ�P
			int vScValue = m_vScrollBar.Value;	//�X�N���[���o�[�̒l�B�悭�g���̂�int��

			//�`�ʐ擪�A�C�e���ƍŏI�A�C�e�����v�Z
			// X:�`�ʂ��ׂ�����̃A�C�e���ԍ�
			// Z:�`�ʂ��ׂ��E���̃A�C�e���ԍ��{�P
			int X = (vScValue / BOX_HEIGHT) * numItemX;
			int Z = (vScValue + Height) / BOX_HEIGHT * numItemX + numItemX;
			if (Z > ItemCount)
				Z = ItemCount;

			for (int item = 0; item < ItemCount; item++)
			{
				//�X���b�h���~���m�F
				if (tStatus == ThreadStatus.REQUEST_STOP)
					break;

				//�`�ʑΏۂ��ǂ����m�F
				if (CheckNecessaryToDrawItem(item))
				{
					//�`�ʑΏۃA�C�e���Ȃ̂ŕ`�ʂ���
					//���i���T���l�C���������Ă���΂���ŕ`��
					if (m_HQcache.ContainsKey(item))
					{
						Bitmap b = m_HQcache[item];
						if (b != null)
						{
							Rectangle rect = GetThumbboxRectanble(item);
							lock (m_offScreen)
							{
								using (Graphics g = Graphics.FromImage(m_offScreen))
								{
									g.DrawImageUnscaled(b, rect);
									DrawTextInfo(g, item, rect);
								}
							}
							this.Invalidate();
							continue;
						}
					}
					else
					{
						//���i���L���b�V�����Ȃ��̂Ő���������ŕ`�ʂ���B
						//���肬��̃T�C�Y�ɂ���
						using (Bitmap dummyBmp = new Bitmap(BOX_WIDTH, THUMBNAIL_SIZE + PADDING * 2))
						{
							using (Graphics g = Graphics.FromImage(dummyBmp))
							{
								//���i���摜�𐶐��A�e�L�X�g���͕`�ʂ���Ă��Ȃ�
								DrawItemHQ2(g, item);
							}
							m_HQcache.Add(item, (Bitmap)dummyBmp.Clone());


							Rectangle rect = GetThumbboxRectanble(item);
							lock (m_offScreen)
							{
								using (Graphics g = Graphics.FromImage(m_offScreen))
								{
									//�����A�ۑ��������i���摜��`��
									g.DrawImageUnscaled(dummyBmp, rect);

									//�摜����`��
									DrawTextInfo(g, item, rect);
								}
							}
							this.Invalidate();
						}
					}//if
				}//if
			}//for

			////�֌W�������ȃA�C�e�������`��
			//for (int Item = X; Item < Z; Item++)
			//{
			//    if (tStatus == ThreadStatus.REQUEST_STOP)
			//        break;

			//    ////ver0.95�ŃR�����g�A�E�g
			//    ////��������ڎw��
			//    ////
			//    //lock (m_offScreen)
			//    //{
			//    //    DrawItemHQ(Graphics.FromImage(m_offScreen), Item);	//���i���`��
			//    //    this.Invalidate();
			//    //}

			//    //ver0.95
			//    //m_offScreen��lock()��Z�����邽�߂Ƀ_�~�[�ɕ`��
			//    //�_�~�[���R�s�[����Ƃ�����lock()����B
			//    if (m_HQcache.ContainsKey(Item))
			//    {
			//        Bitmap b = m_HQcache[Item];
			//        if (b != null)
			//        {
			//            Rectangle rect = GetThumbboxRectanble(Item);
			//            lock (m_offScreen)
			//            {
			//                using (Graphics g = Graphics.FromImage(m_offScreen))
			//                {
			//                    //�ۑ��ς݂̍��i���摜��`��
			//                    g.DrawImageUnscaled(b, rect);

			//                    //�e�L�X�g��`��
			//                    //Rectangle tRect = rect;	//TODO: ������������R�s�[���Ȃ��Ƒʖ�
			//                    //tRect.X += PADDING;
			//                    //tRect.Y += PADDING + THUMBNAIL_SIZE + PADDING;
			//                    //tRect.Width = THUMBNAIL_SIZE;
			//                    //tRect.Height = TEXT_HEIGHT;
			//                    //DrawTextInfo(g, Item, tRect);
			//                    DrawTextInfo(g, Item, rect);

			//                }

			//            }
			//            this.Invalidate();
			//            //Debug.WriteLine(Item, "HQDraw() �L���b�V���ŕ`��");
			//            continue;
			//        }
			//    }

			//    //�ۑ��ς̃L���b�V�����Ȃ��̂Ő���������ŕ`�ʂ���B
			//    //using (Bitmap dummyBmp = new Bitmap(BOX_WIDTH, BOX_HEIGHT)
			//    //���肬��̃T�C�Y�ɂ���
			//    using (Bitmap dummyBmp = new Bitmap(BOX_WIDTH, THUMBNAIL_SIZE+PADDING*2))
			//    {
			//        using (Graphics g = Graphics.FromImage(dummyBmp))
			//        {
			//            //���i���摜�𐶐��A�e�L�X�g���͕`�ʂ���Ă��Ȃ�
			//            DrawItemHQ2(g, Item);
			//        }
			//        m_HQcache.Add(Item, (Bitmap)dummyBmp.Clone());


			//        //������Ǝ��Ԃ̂����鏈���������̂Ŋm�F
			//        if (tStatus == ThreadStatus.REQUEST_STOP)
			//            break;

			//        Rectangle rect = GetThumbboxRectanble(Item);
			//        lock (m_offScreen)
			//        {
			//            using (Graphics g = Graphics.FromImage(m_offScreen))
			//            {
			//                //�����A�ۑ��������i���摜��`��
			//                g.DrawImageUnscaled(dummyBmp, rect);

			//                //�摜����`��
			//                DrawTextInfo(g, Item, rect);
			//            }
			//        }
			//        this.Invalidate();
			//    }
			//    //Debug.WriteLine(Item, "HQDraw");

			//}
			tStatus = ThreadStatus.STOP;

			//�e�̃T���l�C���쐬��PAUSE���Ă�����ĊJ
			//((Form1)Parent).ContinueThreadPool();	//NullRefer������
			Form1.ResumeThumbnailMakerThread();				//Static�ɕύX�����̂ł��̂܂ܗ��p

			Debug.WriteLine("Thumbnail ThreadProc() end");

		}

		/// <summary>
		/// �o�b�N�O���E���h�œ����Ă���X���b�h��STOP���߂��o��
		/// STOP����܂ő҂��Ă��烊�^�[��
		/// </summary>
		public void CancelHQThumbnailThread()
		{
			if (tStatus == ThreadStatus.STOP)
				return;

			tStatus = ThreadStatus.REQUEST_STOP;
			while (tStatus != ThreadStatus.STOP)
				Application.DoEvents();
		}


		//*** �`�ʎx�����[�`�� ****************************************************************

		/// <summary>
		/// �ĕ`�ʊ֐�
		/// �`�ʂ���镔�������ׂčĕ`�ʂ���B
		/// ���̃N���X�A�t�H�[������Ăяo�����B���̂���public
		/// ��Ƀ��j���[�Ń\�[�g���ꂽ�肵���Ƃ��ɌĂяo�����
		/// </summary>
		public void ReDraw()
		{
			setScrollBar();
			MakeThumbnailScreen();
			this.Invalidate();
		}




		/// <summary>
		/// 1�A�C�e���̉�ʓ��ł̘g��Ԃ��B
		/// Thumbbox = �摜�{�����̑傫�Șg
		/// �X�N���[���o�[���̑��ɂ��Ă��D�荞�ݍ�
		/// m_offScreen�����ʂɑ΂��Ďg���邱�Ƃ�z��
		/// </summary>
		private Rectangle GetThumbboxRectanble(int ItemIndex)
		{
			Rectangle r = new Rectangle(
				//(ItemIndex % m_nItemsX) * BOX_WIDTH,
				//(ItemIndex / m_nItemsX) * BOX_HEIGHT - m_vScrollBar.Value,
				m_thumbnailSet[ItemIndex].posX,
				m_thumbnailSet[ItemIndex].posY - m_vScrollBar.Value,
				BOX_WIDTH,
				BOX_HEIGHT);
			return r;
		}


		/// <summary>
		/// THUMBNAIL�C���[�W�̉�ʓ��ł̘g��Ԃ��B
		/// ThumbImage = �摜�����̂�
		/// �X�N���[���o�[���̑��ɂ��Ă��D�荞�ݍ�
		/// m_offScreen�����ʂɑ΂��Ďg���邱�Ƃ�z��
		/// </summary>
		private Rectangle GetThumbImageRectangle(int ItemIndex)
		{
			Image DrawBitmap = m_thumbnailSet[ItemIndex].ThumbImage;
			bool canExpand = true;	//�g��ł��邩�ǂ����̃t���O

			int w;	//�`�ʉ摜�̕�
			int h;	//�`�ʉ摜�̍���

			if (DrawBitmap == null)
			{
				//�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
				DrawBitmap = Properties.Resources.rc_tif32;
				canExpand = false;
				w = DrawBitmap.Width;	//�`�ʉ摜�̕�
				h = DrawBitmap.Height;	//�`�ʉ摜�̍���
			}
			else
			{
				//�T���l�C���͂���
				w = DrawBitmap.Width;	//�`�ʉ摜�̕�
				h = DrawBitmap.Height;	//�`�ʉ摜�̍���

				//���T�C�Y���ׂ����ǂ����m�F����B
				if (m_thumbnailSet[ItemIndex].originalWidth <= THUMBNAIL_SIZE
					&& m_thumbnailSet[ItemIndex].originalHeight <= THUMBNAIL_SIZE)
				{
					//�I���W�i�����T���l�C����菬�����̂Ń��T�C�Y���Ȃ��B
					w = m_thumbnailSet[ItemIndex].originalWidth;
					h = m_thumbnailSet[ItemIndex].originalHeight;
					canExpand = false;
				}
			}


			//�����\�������郂�m�͏o���邾�������Ƃ���
			if (THUMBNAIL_SIZE != Form1.DEFAULT_THUMBNAIL_SIZE)
			{
				//�g��k�����s��
				float ratio = 1;
				if (w > h)
					ratio = (float)THUMBNAIL_SIZE / (float)w;
				else
					ratio = (float)THUMBNAIL_SIZE / (float)h;

				if (ratio > 1 && !canExpand)
				{
					//�g�又���͂��Ȃ�
				}
				else
				{
					w = (int)(w * ratio);
					h = (int)(h * ratio);
				}
				////�I���W�i���T�C�Y���傫���ꍇ�̓I���W�i���T�C�Y�ɂ���
				//if (w > m_thumbnailSet[ItemIndex].originalWidth || h > m_thumbnailSet[ItemIndex].originalHeight)
				//{
				//    w = m_thumbnailSet[ItemIndex].originalWidth;
				//    h = m_thumbnailSet[ItemIndex].originalHeight;
				//}
			}

			Rectangle rect = GetThumbboxRectanble(ItemIndex);
			rect.X += (BOX_WIDTH - w) / 2;	//�摜�`��X�ʒu
			rect.Y += THUMBNAIL_SIZE + PADDING - h; 	//�摜�`��X�ʒu�F������
			//rect.Y -= m_vScrollBar.Value;
			rect.Width = w;
			rect.Height = h;

			return rect;
		}


		/// <summary>
		/// �t�@�C�����A�t�@�C���T�C�Y�A�摜�T�C�Y���e�L�X�g�`�ʂ���
		/// </summary>
		/// <param name="g">�`�ʐ��Graphics</param>
		/// <param name="Item">�`�ʃA�C�e��</param>
		/// <param name="thumbnailRect">�`�ʂ����̃T���l�C��BOX��`�B�e�L�X�g�ʒu�ł͂Ȃ�</param>
		private void DrawTextInfo(Graphics g, int Item, Rectangle thumbnailBoxRect)
		{
			//�e�L�X�g�`�ʈʒu��␳
			Rectangle textRect = thumbnailBoxRect;
			textRect.X += PADDING;								//���ɗ]����ǉ�
			textRect.Y += PADDING + THUMBNAIL_SIZE + PADDING;	//�㉺�ɗ]����ǉ�
			textRect.Width = THUMBNAIL_SIZE;					//�����̓T���l�C���T�C�Y�Ɠ���
			textRect.Height = FONT_HEIGHT;

			//�e�L�X�g�`�ʗp�̏����t�H�[�}�b�g
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Center;			//��������
			sf.Trimming = StringTrimming.EllipsisPath;		//���Ԃ̏ȗ�

			//�t�@�C����������
			if (Form1.g_Config.isShowTPFileName)
			{
				string drawString = Path.GetFileName(m_thumbnailSet[Item].filename);
				g.DrawString(drawString, m_font, new SolidBrush(m_fontColor), textRect, sf);
				textRect.Y += FONT_HEIGHT;
			}

			//�t�@�C���T�C�Y������
			if (Form1.g_Config.isShowTPFileSize)
			{
				string s = String.Format("{0:#,0} bytes", m_thumbnailSet[Item].length);
				g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
				textRect.Y += FONT_HEIGHT;
			}

			//�摜�T�C�Y������
			if (Form1.g_Config.isShowTPPicSize)
			{
				string s = String.Format(
					"{0:#,0}x{1:#,0} px",
					m_thumbnailSet[Item].originalWidth,
					m_thumbnailSet[Item].originalHeight);
				g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
				textRect.Y += FONT_HEIGHT;
			}
		}



		/// <summary>
		/// Form1�ŃT���l�C���쐬���X�V���ꂽ�Ƃ��ɌĂяo�����
		/// �A�b�v�f�[�g����K�v�����邩�ǂ������`�F�b�N��
		/// �K�v�ł���Ε`�ʂ���
		/// Form1����Ăяo����郋�[�`��
		/// </summary>
		/// <param name="Item"></param>
		public void CheckUpdateAndDraw(int Item)
		{
			//���I�ɍX�V����K�v�����邩�ǂ���
			if (m_offScreen == null)
				return;

			//�܂��\������Ă��Ȃ�
			//if (m_nItemsX == 0)
			//    return;

			//���i���`�ʂ̏ꍇ�͉������Ȃ�
			//�T���l�C���������Ă����i���ŏ��������K�v���o�Ă��邽��
			if (THUMBNAIL_SIZE > Form1.DEFAULT_THUMBNAIL_SIZE)
				return;

			//�`�ʂ����摜���X�N���[���`�ʑΏۂ��m�F
			Rectangle rect = GetThumbboxRectanble(Item);

			//if ((sy + BOX_HEIGHT) > m_vScrollBar.Value && sy < (m_vScrollBar.Value + this.Height))
			if (rect.Bottom > 0 && rect.Top < this.Height)
			{
				lock (m_offScreen)
				{
					using (Graphics g = Graphics.FromImage(m_offScreen))
					{
						DrawItem3(g, Item);	//�ʏ�T���l�C���`��
					} //using(Graphics)

					//ver0.97 ����ŏI���B
					using (Graphics g = this.CreateGraphics())
					{
						g.DrawImage(m_offScreen, rect, rect, GraphicsUnit.Pixel);
					}
				}
			}
			else
				Debug.WriteLine(Item, "CheckUpdateAndDraw() �`�ʂ��܂���ł���");

		}

		/// <summary>
		/// �w�肵���A�C�e���͕`�ʑΏۂ��ǂ������`�F�b�N����
		/// ����ɂ�item����posX�AposY�𗘗p���Ă���
		/// </summary>
		/// <param name="item">�`�F�b�N����A�C�e��</param>
		/// <returns>�`�ʑΏۂł����true</returns>
		private bool CheckNecessaryToDrawItem(int item)
		{
			int tempY = m_thumbnailSet[item].posY;
			int vScValue = m_vScrollBar.Value;				//�X�N���[���o�[�̒l�B�悭�g���̂ŕϐ���

			if (tempY > vScValue - BOX_HEIGHT
				&& tempY < vScValue + this.Height)
			{
				return true;
			}
			else
				return false;
		}

		//*** �^�C�}�[ **********************************************************

		
		/// <summary>
		/// �c�[���`�b�v��\������^�C�}�[
		/// ������^�C�}�[���~�߂ăc�[���`�b�v��\������
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void m_timer_Tick(object sender, EventArgs e)
		{
			//�^�C�}�[���~�߂�
			m_timer.Stop();

			//�\���ʒu�����
			Point pt = PointToClient(MousePosition);
			pt.Offset(8, 8);

			//�\��
			m_tooltip.Show((string)m_tooltip.Tag, this, pt, 2000);
		}


		//*** �T���l�C���ۑ����[�`�� **********************************************************

		
		/// <summary>
		/// �T���l�C���摜��ۑ�����B
		/// �����ł͕ۑ��p�_�C�A���O��\�����邾���B
		/// �_�C�A���O����SaveThumbnailImage()���Ăяo�����B
		/// </summary>
		/// <param name="FilenameCandidate">�ۑ��t�@�C�����̌��</param>
		public void SaveThumbnail(string FilenameCandidate)
		{
			if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
				return;

			//��������ۑ�
			int tmpThumbnailSize = THUMBNAIL_SIZE;
			int tmpScrollbarValue = m_vScrollBar.Value;

			m_saveForm = new FormSaveThumbnail(this, m_thumbnailSet, FilenameCandidate);
			m_saveForm.ShowDialog(this);
			m_saveForm.Dispose();

			//���ɖ߂�
			SetThumbnailSize(tmpThumbnailSize);
			m_vScrollBar.Value = tmpScrollbarValue;

		}

		/// <summary>
		/// �T���l�C���摜�ꗗ���쐬�A�ۑ�����B
		/// ���̊֐��̒��ŕۑ�Bitmap�𐶐����A�����png�`���ŕۑ�����
		/// </summary>
		/// <param name="thumbSize">�T���l�C���摜�̃T�C�Y</param>
		/// <param name="numX">�T���l�C���̉������̉摜��</param>
		/// <param name="FilenameCandidate">�ۑ�����t�@�C����</param>
		/// <returns>����true�A�ۑ����Ȃ������ꍇ��false</returns>
		public bool SaveThumbnailImage(int thumbSize, int numX, string FilenameCandidate)
		{
			//�������ς݂��m�F
			if (m_thumbnailSet == null)
				return false;

			//�A�C�e�������m�F
			int ItemCount = m_thumbnailSet.Count;
			if (ItemCount <= 0)
				return false;

			//�T���l�C�������邩�m�F
			//foreach (ImageInfo ti in m_thumbnailSet)
			//    if (ti.ThumbImage == null)
			//        return false;

			////�T���l�C���쐬��҂�
			//while (tStatus != ThreadStatus.STOP)
			//    Application.DoEvents();

			//�X�N���[���o�[�̈ʒu��␳
			//�d�v�FGetThumbboxRectanble(Item)�Ōv�Z�ɗ��p���Ă���B
			m_vScrollBar.Value = 0;


			//�T���l�C���T�C�Y��ݒ�.�Čv�Z
			SetThumbnailSize(thumbSize);

			//�A�C�e������ݒ�
			//m_nItemsX = numX;
			//m_nItemsY = ItemCount / m_nItemsX;	//�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
			//if (ItemCount % m_nItemsX > 0)
			//    m_nItemsY++;						//����؂�Ȃ������ꍇ��1�s�ǉ�

			Size offscreenSize = calcScreenSize();

			//Bitmap�𐶐�
			Bitmap saveBmp = new Bitmap(offscreenSize.Width, offscreenSize.Height);
			Bitmap dummyBmp = new Bitmap(BOX_WIDTH, BOX_HEIGHT);

			using (Graphics g = Graphics.FromImage(saveBmp))
			{
				//�Ώۋ�`��w�i�F�œh��Ԃ�.
				g.Clear(BackColor);


				for (int item = 0; item < m_thumbnailSet.Count; item++)
				{
					using (Graphics dummyg = Graphics.FromImage(dummyBmp))
					{
						//���i���摜��`��
						DrawItemHQ2(dummyg, item);

						//�_�~�[�ɕ`�ʂ����摜��`�ʂ���B
						Rectangle r = GetThumbboxRectanble(item);
						g.DrawImageUnscaled(dummyBmp, r);

						//�摜���𕶎��`�ʂ���
						DrawTextInfo(g, item, r);
					}

					ThumbnailEventArgs ev = new ThumbnailEventArgs();
					ev.HoverItemNumber = item;
					ev.HoverItemName = m_thumbnailSet[item].filename;
					this.SavedItemChanged(null, ev);
					Application.DoEvents();

					//�L�����Z������
					if (m_saveForm.isCancel)
						return false;
				}

				//for (int iy = 0; iy < m_nItemsY; iy++)
				//{
				//    for (int ix = 0; ix < m_nItemsX; ix++)
				//    {
				//        int Item = iy * m_nItemsX + ix;
				//        if (Item >= ItemCount)
				//            break;	//ix�����������Ȃ������v�Ȃ͂�

				//        //if (THUMBNAIL_SIZE > DEFAULT_THUMBNAIL_SIZE)
				//        using (Graphics dummyg = Graphics.FromImage(dummyBmp))
				//        {
				//            //���i���摜��`��
				//            DrawItemHQ2(dummyg, Item);

				//            //�_�~�[�ɕ`�ʂ����摜��`�ʂ���B
				//            Rectangle r = GetThumbboxRectanble(Item);
				//            g.DrawImageUnscaled(dummyBmp, r);

				//            //�摜���𕶎��`�ʂ���
				//            DrawTextInfo(g, Item, r);
							
				//        }
				//        //else
				//        //    DrawItem3(g, Item);

				//        ThumbnailEventArgs ev = new ThumbnailEventArgs();
				//        ev.HoverItemNumber = Item;
				//        ev.HoverItemName = m_thumbnailSet[Item].filename;
				//        this.SavedItemChanged(null, ev);
				//        Application.DoEvents();

				//        //�L�����Z������
				//        if(m_saveForm.isCancel)
				//            return false;
				//    }
				//}
			}

			saveBmp.Save(FilenameCandidate);
			saveBmp.Dispose();
			return true;
		}
	}

	/// <summary>
	/// �ǉ���p�̃L���b�V���B
	/// Dictionary<>���g�����A�z�z��Ńf�[�^�ɖ��O�����ĕۑ��ł���B
	/// ��{�͒ǉ��ƎQ�Ƃ̂݁B�����Ƃ��͑S������
	/// 
	/// �T���l�C���ō��i���T���l�C�����ꎞ�ێ����邽�߂ɗ��p
	/// </summary>
	public class NamedBuffer<TKey, TValue>
	{
		// �L���b�V����ۑ�����Dictionary
		static Dictionary<TKey, TValue> _cache;

		//�R���X�g���N�^
		public NamedBuffer()
		{
			_cache = new Dictionary<TKey, TValue>();
		}

		public void Add(TKey key, TValue obj)
		{
			//�L�[�̏d���������
			if (_cache.ContainsKey(key))
				_cache.Remove(key);

			_cache.Add(key, obj);
		}

		public void Delete(TKey key)
		{
			_cache.Remove(key);
		}


		/// <summary>
		/// �w�肵���L�[�̃A�C�e����Ԃ�
		/// </summary>
		/// <param name="key">�A�C�e�����w�肷��L�[</param>
		/// <returns>�A�C�e���I�u�W�F�N�g�B���ł��Ă���ꍇ��null��Ԃ�</returns>
		public TValue this[TKey key]
		{
			get
			{
				try
				{
					TValue d = (TValue)_cache[key];
					return d;
				}
				catch
				{
					// �L�[�����݂��Ȃ��ꍇ�Ȃ�
					return default(TValue);
				}
			}

		}

		public bool ContainsKey(TKey key)
		{
			return _cache.ContainsKey(key);
		}

		public void Clear()
		{
			_cache.Clear();
		}
	}
}


///// <summary>
///// �������̃t�H�[�J�X�i�I���j�g��`�ʂ���B
///// �O���t�B�b�N�J�[�h�ɂ���Ă͒x���̂Ŏg��Ȃ�
///// </summary>
///// <param name="ItemIndex">�`�ʑΏۂ̃A�C�e���ԍ�</param>
///// <param name="g">�`�ʂ��ׂ�Graphic</param>
//private void DrawSemiTransparentBox(int ItemIndex, Graphics g)
//{
//    using (GraphicsPath gp = new GraphicsPath())
//    {
//        float arc = 5.0f;
//        Rectangle rect = GetThumbboxRectanble(ItemIndex);
//        rect.Inflate(-1, -1);
//        gp.StartFigure();
//        gp.AddArc(rect.Right - arc, rect.Bottom - arc, arc, arc, 0.0f, 90.0f);  // �E��
//        gp.AddArc(rect.Left, rect.Bottom - arc, arc, arc, 90.0f, 90.0f);      // ����
//        gp.AddArc(rect.Left, rect.Top, arc, arc, 180.0f, 90.0f);            // ����
//        gp.AddArc(rect.Right - arc, rect.Top, arc, arc, 270.0f, 90.0f);       // �E��
//        gp.CloseFigure();

//        //�V������������
//        //g.DrawRectangle(Pens.LightBlue, nx * BOX_WIDTH, ny * BOX_HEIGHT - vScrollBar1.Value, BOX_WIDTH, BOX_HEIGHT);

//        using (SolidBrush brs = new SolidBrush(Color.FromArgb(32, Color.RoyalBlue)))
//        {
//            g.FillPath(brs, gp);				//�h��Ԃ�
//            g.DrawPath(Pens.RoyalBlue, gp);		//�g��������
//        }//using brs
//    }//using gp
//}
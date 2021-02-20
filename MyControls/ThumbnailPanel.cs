using System;
using System.Collections.Generic;
using System.Drawing;					//Size, Bitmap, Font , Point, Graphics
using System.Drawing.Drawing2D;			//GraphicsPath
using System.IO;						//Directory, File
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /// <summary>
    /// �T���l�C����p�C�x���g�̒�`
    ///   �T���l�C�����Ƀ}�E�X�z�o�[���N�����Ƃ��̂��߂̃C�x���g
    ///   ���̃C�x���g��ThumbnailPanel::ThumbnailPanel_MouseMove()�Ŕ������Ă���
    ///   �󂯂鑤�͂���EventArgs���g���Ď󂯂�ƃA�C�e����������B
    /// </summary>
    public class ThumbnailEventArgs : EventArgs
    {
        public int HoverItemNumber;     //Hover���̃A�C�e���ԍ�
        public string HoverItemName;    //Hover���̃A�C�e����
    }

    //public sealed class ThumbnailPanel : MomentumScrollPanel
    public sealed class ThumbnailPanel : UserControl
    {
        //���ʕϐ��̒�`
        private List<ImageInfo> m_thumbnailSet;     //ImageInfo�̃��X�g, = g_pi.Items

        private FormSaveThumbnail m_saveForm;       //�T���l�C���ۑ��p�_�C�A���O
                                                    //private Bitmap m_offScreen;					//Bitmap. new���Ċm�ۂ����
                                                    //private VScrollBar m_vScrollBar;			//�X�N���[���o�[�R���g���[��
                                                    //private Size m_virtualScreenSize;			//���z�T���l�C���̃T�C�Y
                                                    //ver 0.994 �g��Ȃ����Ƃɂ���
                                                    //private int m_nItemsX;						//offScreen�ɕ��ԃA�C�e���̐�: SetScrollBar()�Ōv�Z
                                                    //private int m_nItemsY;						//offScreen�ɕ��ԃA�C�e���̐�: SetScrollBar()�Ōv�Z
                                                    //static ThreadStatus tStatus;				//�X���b�h�̏󋵂�����
                                                    //private bool m_needHQDraw;					//�n�C�N�I���e�B�`�ʂ����{�ς݂�

        private int m_mouseHoverItem = -1;          //���݃}�E�X���z�o�[���Ă���A�C�e��
        private Font m_font;                        //���x����������̂͂��������Ȃ��̂�
        private Color m_fontColor;                  //�t�H���g�̐F
                                                    //private ToolTip m_tooltip;					//�c�[���`�b�v�B�摜����\������
                                                    //private System.Windows.Forms.Timer m_tooltipTimer
                                                    //    = new System.Windows.Forms.Timer();		//�c�[���`�b�v�\���p�^�C�}�[

        //ver0.994 �T���l�C�����[�h
        //private ThumnailMode m_thumbnailMode;

        //�傫�ȃT���l�C���p�L���b�V��
        private NamedBuffer<int, Bitmap> m_HQcache
            = new NamedBuffer<int, Bitmap>();

        private const long ANIMATE_DURATION = 1000; //�t�F�[�h�C���A�j���[�V��������
        private const int PADDING = 10;     //2014�N3��23���ύX�B�Ԋu��������
        private int THUMBNAIL_SIZE;         //�T���l�C���̑傫���B���ƍ����͓���l
        private int BOX_WIDTH;              //�{�b�N�X�̕��BPADDING + THUMBNAIL_SIZE + PADDING
        private int BOX_HEIGHT;             //�{�b�N�X�̍����BPADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING
        private int FONT_HEIGHT;            //FONT�̍����B

        //��p�C�x���g�̒�`
        public delegate void ThumbnailEventHandler(object obj, ThumbnailEventArgs e);

        //public event ThumbnailEventHandler OnHoverItemChanged;	//�}�E�XHover�ŃA�C�e�����ւ�������Ƃ�m�点��B
        public event ThumbnailEventHandler SavedItemChanged;    //

        //�R���e�L�X�g���j���[
        private ContextMenuStrip m_ContextMenu = new ContextMenuStrip();

        private bool fastDraw = false;

        //�X�N���[���^�C�}�[
        private System.Windows.Forms.Timer m_scrollTimer = null;

        private int m_targetScrollposY = 0;

        //***************************************************************************************

        #region �R���X�g���N�^

        //***************************************************************************************
        public ThumbnailPanel()
        {
            //������
            this.BackColor = Color.White;   //Color.FromArgb(100, 64, 64, 64);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            //�_�u���o�b�t�@�ǉ��B�̂̕��@�������Ă���
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            //tStatus = ThreadStatus.STOP;

            //�c�[���`�b�v�̏�����
            //m_tooltip = new ToolTip();
            ////�t�H�[�����A�N�e�B�u�łȂ����ł�ToolTip��\������
            //m_tooltip.ShowAlways = false;

            ////�c�[���`�b�v�^�C�}�[�̏�����
            //m_tooltipTimer.Interval = 700;
            //m_tooltipTimer.Tick += new EventHandler((o,e)=>
            //    {
            //        Point pt = PointToClient(MousePosition);
            //        pt.Offset(8, 8);
            //        m_tooltip.Show(m_tooltip.Tag as string, this, pt, 3000);
            //        m_tooltipTimer.Stop();
            //    });

            //�X�N���[���o�[�̏�����
            this.AutoScroll = true;

            //�t�H���g����
            SetFont(new Font("�l�r �S�V�b�N", 9), Color.Black);

            //�T���l�C���T�C�Y����BOX�̒l�����肷��B
            SetThumbnailSize(App.DEFAULT_THUMBNAIL_SIZE);

            //�R���e�L�X�g���j���[�̏�����
            InitContextMenu();

            //�X�N���[���^�C�}�[
            m_scrollTimer = new System.Windows.Forms.Timer();
            m_scrollTimer.Interval = 50;
            m_scrollTimer.Tick += m_scrollTimer_Tick;
        }

        ~ThumbnailPanel()
        {
            m_font.Dispose();
            //m_tooltip.Dispose();
            m_HQcache.Clear();
            //m_timer.Tick -= new EventHandler(m_timer_Tick);
            //m_timer.Dispose();
        }

        #endregion �R���X�g���N�^

        //***************************************************************************************

        #region public���\�b�h

        public void Init()
        {
            //m_needHQDraw = false;
            m_HQcache.Clear();          //ver0.974
                                        //m_thumbnailSet.Clear();		//ver0.974 �|�C���^�����Ă��邾���Ȃ̂ł����ł��Ȃ�

            //�X�N���[���ʒu�̏�����
            AutoScrollPosition = Point.Empty;
        }

        /// <summary>
        /// �T���l�C���摜�P�̃T�C�Y��ύX����
        /// option Form�ŕύX���ꂽ���ƍĐݒ肳��邱�Ƃ�z��
        /// ���F�T���l�C���̗��e��PADDING�����ǉ�
        /// ���F�T���l�C���̏㉺��PADDING�����ǉ�
        /// ���ɂ��\��̕�����͓����Ă��Ȃ�
        /// </summary>
        /// <param name="thumbnailSize">�V�����T���l�C���T�C�Y</param>
        public void SetThumbnailSize(int thumbnailSize)
        {
            //ver0.982 HQcache�������N���A�����̂ŕύX
            //�T���l�C���T�C�Y���ς���Ă�����ύX����
            if (THUMBNAIL_SIZE != thumbnailSize)
            {
                THUMBNAIL_SIZE = thumbnailSize;

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

            if (App.Config.isShowTPFileName)
                BOX_HEIGHT += PADDING + FONT_HEIGHT;

            if (App.Config.isShowTPFileSize)
                BOX_HEIGHT += PADDING + FONT_HEIGHT;

            if (App.Config.isShowTPPicSize)
                BOX_HEIGHT += PADDING + FONT_HEIGHT;

            #endregion ver0.982

            //�T���l�C���T�C�Y���ς��Ɖ�ʂɕ\���ł���
            //�A�C�e�������ς��̂ōČv�Z
            SetScrollBar();
        }

        public void SetFont(Font f, Color fc)
        {
            m_font = f;
            m_fontColor = fc;

            //TEXT_HEIGHT�̌���
            using (Bitmap bmp = new Bitmap(100, 100))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SizeF sf = g.MeasureString("�e�X�g������", m_font);
                FONT_HEIGHT = (int)sf.Height;
            }

            //�t�H���g���ς��ƃT���l�C���T�C�Y���ς��̂Ōv�Z
            SetThumbnailSize(THUMBNAIL_SIZE);
        }

        /// <summary>
        /// ���̑傫���̃T�C�Y��T���Đݒ肷��
        /// </summary>
        public void ThumbSizeZoomIn()
        {
            Array TSizes = Enum.GetValues(typeof(DefaultThumbSize));
            foreach (DefaultThumbSize d in TSizes)
            {
                int size = (int)d;
                if (size > THUMBNAIL_SIZE)
                {
                    SetThumbnailSize(size);
                    App.Config.ThumbnailSize = size;
                    fastDraw = false;
                    this.Invalidate();
                    return;
                }
            }
            //������Ȃ���΂Ȃɂ����Ȃ�
        }

        /// <summary>
        /// 1�O�̃T���l�C���T�C�Y�������Đݒ肷��
        /// </summary>
        public void ThumbSizeZoomOut()
        {
            Array TSizes = Enum.GetValues(typeof(DefaultThumbSize));
            Array.Sort(TSizes);
            Array.Reverse(TSizes);
            foreach (DefaultThumbSize d in TSizes)
            {
                if ((int)d < THUMBNAIL_SIZE)
                {
                    SetThumbnailSize((int)d);
                    App.Config.ThumbnailSize = (int)d;
                    fastDraw = false;
                    this.Invalidate();
                    return;
                }
            }
            //������Ȃ���΂Ȃɂ����Ȃ�
        }

        #endregion public���\�b�h

        //***************************************************************************************

        #region �R���e�L�X�g���j���[

        private void InitContextMenu()
        {
            //�R���e�L�X�g���j���[�̏���
            this.ContextMenuStrip = m_ContextMenu;
            m_ContextMenu.ShowImageMargin = false;
            m_ContextMenu.ShowCheckMargin = true;
            ToolStripSeparator separator = new ToolStripSeparator();
            //ToolStripMenuItem filename = new ToolStripMenuItem("");
            ToolStripMenuItem addBookmark = new ToolStripMenuItem("��������͂���");
            ToolStripMenuItem Bookmarks = new ToolStripMenuItem("������ꗗ");
            ToolStripMenuItem thumbnailLabel = new ToolStripMenuItem("�T���l�C���T�C�Y") { Enabled = false };

            ToolStripMenuItem thumbSizeBig = new ToolStripMenuItem("�ő�");
            ToolStripMenuItem thumbSizeLarge = new ToolStripMenuItem("��");
            ToolStripMenuItem thumbSizeNormal = new ToolStripMenuItem("��");
            ToolStripMenuItem thumbSizeSmall = new ToolStripMenuItem("��");
            ToolStripMenuItem thumbSizeTiny = new ToolStripMenuItem("�ŏ�");

            ToolStripMenuItem thumbShadow = new ToolStripMenuItem("�e������");
            ToolStripMenuItem thumbFrame = new ToolStripMenuItem("�g��");

            m_ContextMenu.Items.Add("�L�����Z��");
            m_ContextMenu.Items.Add(separator);

            m_ContextMenu.Items.Add(addBookmark);
            m_ContextMenu.Items.Add(Bookmarks);
            m_ContextMenu.Items.Add(separator);

            m_ContextMenu.Items.Add(thumbShadow);
            m_ContextMenu.Items.Add(thumbFrame);
            m_ContextMenu.Items.Add("-");

            //m_ContextMenu.Items.Add(thumbSizeDropDown);
            m_ContextMenu.Items.Add(thumbnailLabel);
            m_ContextMenu.Items.Add(thumbSizeBig);
            m_ContextMenu.Items.Add(thumbSizeLarge);
            m_ContextMenu.Items.Add(thumbSizeNormal);
            m_ContextMenu.Items.Add(thumbSizeSmall);
            m_ContextMenu.Items.Add(thumbSizeTiny);
            //m_ContextMenu.Items.Add(Info);

            //Open�����Ƃ��̏�����
            m_ContextMenu.Opening += new System.ComponentModel.CancelEventHandler((s, e) =>
            {
                ////�c�[���`�b�v�������������
                //if (m_tooltip.Active)
                //    m_tooltip.Hide(this);

                ////�c�[���`�b�v�^�C�}�[������
                //if (m_tooltipTimer.Enabled)
                //    m_tooltipTimer.Stop();

                int index = GetHoverItem(PointToClient(MousePosition));
                m_ContextMenu.Tag = index;
                if (index >= 0)
                {
                    //filename.Text = Path.GetFileName(m_thumbnailSet[index].filename);
                    //filename.Enabled = true;
                    addBookmark.Checked = m_thumbnailSet[index].isBookMark;
                    addBookmark.Enabled = true;
                }
                else
                {
                    //filename.Enabled = false;
                    addBookmark.Enabled = false;
                }
                //�T���l�C���T�C�Y�Ƀ`�F�b�N��
                thumbSizeTiny.Checked = false;
                thumbSizeSmall.Checked = false;
                thumbSizeNormal.Checked = false;
                thumbSizeLarge.Checked = false;
                thumbSizeBig.Checked = false;
                switch (THUMBNAIL_SIZE)
                {
                    case (int)DefaultThumbSize.minimum:
                        thumbSizeTiny.Checked = true;
                        break;

                    case (int)DefaultThumbSize.small:
                        thumbSizeSmall.Checked = true;
                        break;

                    case (int)DefaultThumbSize.normal:
                        thumbSizeNormal.Checked = true;
                        break;

                    case (int)DefaultThumbSize.large:
                        thumbSizeLarge.Checked = true;
                        break;

                    case (int)DefaultThumbSize.big:
                        thumbSizeBig.Checked = true;
                        break;
                }

                //�e�E�g�Ƀ`�F�b�N
                thumbFrame.Checked = App.Config.isDrawThumbnailFrame;
                thumbShadow.Checked = App.Config.isDrawThumbnailShadow;

                //m_tooltip.Disposed += new EventHandler((se, ee) => { m_tooltip.Active = true; });

                //������ꗗ
                Bookmarks.DropDownItems.Clear();
                foreach (ImageInfo i in m_thumbnailSet)
                    if (i.isBookMark)
                        Bookmarks.DropDownItems.Add(i.filename);
            });
            m_ContextMenu.ItemClicked += new ToolStripItemClickedEventHandler(m_ContextMenu_ItemClicked);
        }

        private void m_ContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "�ŏ�":
                    SetThumbnailSize((int)DefaultThumbSize.minimum);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.minimum;
                    break;

                case "��":
                    SetThumbnailSize((int)DefaultThumbSize.small);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.small;
                    break;

                case "��":
                    SetThumbnailSize((int)DefaultThumbSize.normal);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.normal;
                    break;

                case "��":
                    SetThumbnailSize((int)DefaultThumbSize.large);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.large;
                    break;

                case "�ő�":
                    SetThumbnailSize((int)DefaultThumbSize.big);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.big;
                    break;

                case "�e������":
                    App.Config.isDrawThumbnailShadow = !App.Config.isDrawThumbnailShadow;
                    //Invalidate();
                    break;

                case "�g��":
                    App.Config.isDrawThumbnailFrame = !App.Config.isDrawThumbnailFrame;
                    //Invalidate();
                    break;

                case "��������͂���":
                    int index = (int)m_ContextMenu.Tag;
                    m_thumbnailSet[index].isBookMark = !m_thumbnailSet[index].isBookMark;
                    //this.Invalidate();
                    break;

                case "������ꗗ":
                    break;

                default:
                    break;
            }
            //��ʂ���������
            fastDraw = false;
            this.Invalidate();
        }

        #endregion �R���e�L�X�g���j���[

        //***************************************************************************************

        #region override�֐�

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            //TODO ����m_thumbnailSet���Ȃ����B
            //ver1.41 m_thumbnailSet�͂����ŃZ�b�g����.
            if (Visible)
                m_thumbnailSet = Form1.g_pi.Items;
        }

        protected override void OnResize(EventArgs e)
        {
            //�X�N���[���o�[�̕\�����X�V
            SetScrollBar();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Uty.WriteLine("ThumbnailPanel::OnPaint() ClipRect={0}", e.ClipRectangle);
            //�w�i�F�œh��Ԃ�
            e.Graphics.Clear(this.BackColor);

            //�`�ʑΏۂ����邩�`�F�b�N����B������ΏI��
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //�`�ʕi���̌���
            e.Graphics.InterpolationMode =
                fastDraw ?
                InterpolationMode.NearestNeighbor :
                InterpolationMode.HighQualityBicubic;

            //�`�ʂ��ׂ��A�C�e�������`�ʂ���
            //for (int item = 0; item < m_thumbnailSet.Count; item++)
            //{
            //    if (CheckNecessaryToDrawItem(item, e.ClipRectangle))
            //    {
            //        //count++;
            //        DrawItem3(e.Graphics, item);
            //    }
            //}

            //ver1.41 �����`��
            ////����̃A�C�e���ԍ�
            //int horizonItems = this.ClientRectangle.Width / BOX_WIDTH;
            //if (horizonItems < 1) horizonItems = 1;
            //int startitem = (-AutoScrollPosition.Y / BOX_HEIGHT) * horizonItems;
            ////�E���̃A�C�e���ԍ���(�X�N���[���ʁ{��ʏc�j��BOX�c �̐؂�グ�~���A�C�e����
            //int enditem = (int)Math.Ceiling((double)(-AutoScrollPosition.Y + ClientRectangle.Height) / (double)BOX_HEIGHT) * horizonItems;
            //if (enditem >= m_thumbnailSet.Count)
            //    enditem = m_thumbnailSet.Count - 1;

            //�f�o�b�O�p�F�N���b�v�̈��\��
            //e.Graphics.DrawRectangle(Pens.Red, e.ClipRectangle);

            //ver1.41a �����ClipRectangle�������či�荞��
            int horizonItems = this.ClientRectangle.Width / BOX_WIDTH;
            if (horizonItems < 1) horizonItems = 1;
            int startitem = ((-AutoScrollPosition.Y + e.ClipRectangle.Y) / BOX_HEIGHT) * horizonItems;
            //�E���̃A�C�e���ԍ���(�X�N���[���ʁ{��ʏc�j��BOX�c �̐؂�グ�~���A�C�e����
            int enditem = (int)Math.Ceiling((double)(-AutoScrollPosition.Y + e.ClipRectangle.Bottom) / (double)BOX_HEIGHT) * horizonItems;
            if (enditem >= m_thumbnailSet.Count)
                enditem = m_thumbnailSet.Count - 1;
            //Uty.WriteLine("OnPaint Item = {0} to {1}", startitem, enditem);
            //�K�v�Ȃ��̂�`��
            for (int item = startitem; item <= enditem; item++)
            {
                DrawItem3(e.Graphics, item);
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
            int itemIndex = GetHoverItem(pos);  //�z�o�[���̃A�C�e���ԍ�
            if (itemIndex == m_mouseHoverItem)
            {
                //�}�E�X���z�o�[���Ă���A�C�e�����ς��Ȃ��Ƃ��͉������Ȃ��B
                return;
            }

            //�z�o�[�A�C�e�����ւ���Ă���̂ōĕ`��
            fastDraw = false;
            int temp = m_mouseHoverItem;
            m_mouseHoverItem = itemIndex;
            if (temp >= 0)
                this.Invalidate(GetThumbboxRectanble(temp));
            if (itemIndex >= 0)
                this.Invalidate(GetThumbboxRectanble(itemIndex));
            this.Update();
            //this.Invalidate();
            //this.Refresh();

            //�z�o�[�A�C�e�����ւ�������Ƃ�`����
            //m_mouseHoverItem = itemIndex;

            //ver1.20 2011�N10��9�� �ĕ`�ʂ��I�������A���Ă���
            if (itemIndex < 0)
                return;

            //Hover���Ă���A�C�e�����ւ�������Ƃ������C�x���g�𔭐�������
            //���̃C�x���g�̓��C��Form�Ŏ󂯎��StatusBar�̕\����ς���B
            //ThumbnailEventArgs he = new ThumbnailEventArgs();
            //he.HoverItemNumber = m_mouseHoverItem;
            //he.HoverItemName = m_thumbnailSet[m_mouseHoverItem].filename;
            //this.OnHoverItemChanged(this, he);

            //ver1.20 �C�x���g�ʒm����߂�
            //�X�e�[�^�X�o�[��ύX
            string s = string.Format(
                "[{0}]{1}",
                itemIndex + 1,
                m_thumbnailSet[m_mouseHoverItem].filename);
            Form1._instance.setStatusbarInfo(s);

            //ToolTip��\������
            //string sz = String.Format(
            //    "{0}\n {1}\n ���t: {2:yyyy�NM��d�� H:m:s}\n �t�@�C���T�C�Y: {3:N0}bytes\n �摜�T�C�Y: {4:N0}x{5:N0}�s�N�Z��",
            //    Path.GetFileName(m_thumbnailSet[itemIndex].filename),
            //    Path.GetDirectoryName(m_thumbnailSet[itemIndex].filename),
            //    m_thumbnailSet[itemIndex].CreateDate,
            //    m_thumbnailSet[itemIndex].length,
            //    m_thumbnailSet[itemIndex].originalWidth,
            //    m_thumbnailSet[itemIndex].originalHeight
            //);
            //m_tooltip.Show(sz, this, e.Location, 3000);

            //Timer�ŕ\��
            //m_tooltip.Tag = sz;
            ////ToolTip�ɂ�Text���Ȃ��̂�Tag�ɕۑ�����BTimer���ŗ��p
            //if (m_tooltipTimer.Enabled)
            //    m_tooltipTimer.Stop();
            //m_tooltipTimer.Start();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                //�N���b�N�ʒu�̉摜���擾
                int index = GetHoverItem(PointToClient(Cursor.Position));       //m_thumbnailSet���̔ԍ�
                if (index < 0)
                    return;
                else
                {
                    (Form1._instance).SetViewPage(index);
                    //�\������ߏI��
                    Form1._instance.SetThumbnailView(false);
                    return;
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.Focus();
            base.OnMouseEnter(e);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            //fastDraw = true;
            base.OnScroll(se);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);

            var y = -this.AutoScrollPosition.Y;
            if (e.Delta > 0)
            {
                y = y - 250;
                if (y < 0)
                    y = 0;
            }
            else
            {
                y = y + 250;
                Size virtualScreenSize = CalcScreenSize();
                int availablerange = virtualScreenSize.Height - this.ClientRectangle.Height;
                if (y > availablerange)
                    y = availablerange;
            }
            if (App.Config.ThumbnailPanelSmoothScroll)
            {
                //�A�j���[�V����������B
                //�X�N���[���^�C�}�[�̋N��
                m_targetScrollposY = y;
                if (m_scrollTimer.Enabled == false)
                    m_scrollTimer.Start();
            }
            else
            {
                //�����ɃX�N���[��
                AutoScrollPosition = new Point(0, y);
                return;
            }
        }

        private void m_scrollTimer_Tick(object sender, EventArgs e)
        {
            //���݂̃X�N���[���o�[�̈ʒu�B�{�ɒ����B
            var y = -this.AutoScrollPosition.Y;

            //�X�N���[���ʂ̌v�Z
            var delta = (m_targetScrollposY - y) / 2;
            if (delta > 100) delta = 70;
            if (delta < -100) delta = -70;
            if (delta == 0) delta = (m_targetScrollposY - y);
            var newY = y + delta;

            //�X�N���[��
            AutoScrollPosition = new Point(0, newY);
            if (newY == y)
                m_scrollTimer.Stop();
            //throw new NotImplementedException();
        }

        #endregion override�֐�

        public void Application_Idle()
        {
            if (fastDraw)
            {
                fastDraw = false;
                Invalidate();
            }
        }

        //*** �X�N���[���o�[ ********************************************************************

        /// <summary>
        /// �X�N���[���o�[�̊�{�ݒ�
        /// �X�N���[���o�[��\�����邩�ǂ����𔻕ʂ��A�K�v�ɉ����ĕ\���A�ݒ肷��B
        /// �K�v���Ȃ��ꍇ��Value���O�ɐݒ肵�Ă����B
        /// ��Ƀ��T�C�Y�C�x���g�����������Ƃ��ɌĂяo�����
        /// </summary>
        private void SetScrollBar()
        {
            //�������ς݂��m�F
            if (m_thumbnailSet == null)
            {
                this.AutoScrollMinSize = this.ClientRectangle.Size;
                return;
            }

            //�`�ʂɕK�v�ȃT�C�Y���m�F����B
            //�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
            Size virtualScreenSize = CalcScreenSize();
            this.AutoScrollMinSize = new Size(1, virtualScreenSize.Height);
            Uty.WriteLine("virtualScreenSize" + virtualScreenSize.ToString());
            Uty.WriteLine("AutoScrollMinSize=" + this.AutoScrollMinSize.ToString());
            Uty.WriteLine("this.clientrect=" + this.ClientRectangle.ToString());
        }

        /// <summary>
        /// �X�N���[���T�C�Y���v�Z����
        /// �c�������傫����΃X�N���[���o�[���K�v�Ƃ�������
        /// �X�N���[���o�[�͍ŏ�����T�C�Y�Ƃ��čl��
        /// </summary>
        private Size CalcScreenSize()
        {
            //�A�C�e�������m�F
            int itemCount = m_thumbnailSet.Count;

            //ver1.20ClientRectangle���g�����ƂŃX�N���[���o�[�l��
            //const int scrollControllWidth = 20;

            //�`�ʂɕK�v�ȃT�C�Y���m�F����B
            //�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
            int screenWidth = this.ClientRectangle.Width;
            if (screenWidth < 1) screenWidth = 1;
            int screenHeight = this.ClientRectangle.Height;
            if (screenHeight < 1) screenHeight = 1;

            //�e�A�C�e���̈ʒu�����肷��
            int tempx = 0;
            int tempy = 0;

            //TODO:�X�N���[���T�C�Y��160�ȏ゠�邱�Ƃ��O��
            //Debug.Assert(screenWidth > 160);

            for (int i = 0; i < itemCount; i++)
            {
                //if ((tempx + THUMBNAIL_SIZE + PADDING*2) > (screenWidth - scrollControllWidth))
                if ((tempx + THUMBNAIL_SIZE + PADDING * 2) > screenWidth)
                {
                    //�L�����b�W���^�[��
                    tempx = 0;
                    tempy += BOX_HEIGHT;
                }

                //�A�C�e���̈ʒu��ۑ����Ă���
                tempx += PADDING;
                //m_thumbnailSet[i].posX = tempx;
                //m_thumbnailSet[i].posY = tempy;

                //X�����̈ʒu�Ɉړ�������
                tempx += THUMBNAIL_SIZE + PADDING;
            }//for

            //�Ō�̗�ɉ摜�̍�������ǉ�
            screenHeight = tempy + BOX_HEIGHT;
            return new Size(screenWidth, screenHeight);
        }

        //***************************************************************************************

        #region �A�C�e���`��

        private void DrawItem3(Graphics g, int item)
        {
            //Uty.WriteLine("DrawItem3({0}", item);

            //�`�ʈʒu�̌���
            Rectangle thumbnailBoxRect = GetThumbboxRectanble(item);

            //�Ώۋ�`��w�i�F�œh��Ԃ�.
            //�������Ȃ��ƑO�ɕ`�����A�C�R�����c���Ă��܂��\���L��
            //using (SolidBrush s = new SolidBrush(BackColor))
            //{
            //    g.FillRectangle(s, thumbnailBoxRect);
            //}

            //�`�ʂ���r�b�g�}�b�v������
            //bool isDrawFrame = true;
            Bitmap drawBitmap = m_thumbnailSet[item].thumbnail as Bitmap;
            Rectangle imageRect = GetThumbImageRectangle(item);

            if (drawBitmap == null)
            {
                //�摜���Ȃ��Ƃ��͔񓯊��Ŏ���Ă���
                //�X�^�b�N�^�̔񓯊�GetBitmap�ɕύX
                Form1._instance.AsyncGetBitmap(item, (MethodInvoker)(() =>
                {
                    //ver1.75 �T���l�C�����Ȃ��̂ō��
                    Form1.g_pi.AsyncThumnailMaker(item);

                    if (this.Visible)
                    {
                        if (App.Config.isThumbFadein)
                        {
                            //�t�F�[�h�C���A�j���[�V�����ŕ\��
                            m_thumbnailSet[item].animateStartTime = DateTime.Now.Ticks;
                            var timer = new System.Windows.Forms.Timer();
                            timer.Interval = 50;
                            timer.Tick += (s, e) =>
                            {
                                this.Invalidate(GetThumbboxRectanble(item));
                                //this.Update();
                                TimeSpan tp = new TimeSpan(DateTime.Now.Ticks - m_thumbnailSet[item].animateStartTime);
                                if (tp.TotalMilliseconds > ANIMATE_DURATION)
                                {
                                    timer.Stop();
                                    timer.Dispose();
                                }
                            };
                            timer.Start();
                        }
                        else
                        {
                            //�����ɕ`��
                            this.Invalidate(GetThumbboxRectanble(item));
                        }
                    }
                }));

                thumbnailBoxRect.Inflate(-PADDING, -PADDING);
                thumbnailBoxRect.Height = THUMBNAIL_SIZE;
                g.FillRectangle(Brushes.White, thumbnailBoxRect);
                thumbnailBoxRect.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightGray, thumbnailBoxRect);
                return;
            }

            //�摜��`��
            TimeSpan diff = new TimeSpan(DateTime.Now.Ticks - m_thumbnailSet[item].animateStartTime);
            if (diff.TotalMilliseconds < 0 || diff.TotalMilliseconds > ANIMATE_DURATION)
            {
                //�ʏ�`��
                m_thumbnailSet[item].animateStartTime = 0;

                //�e�̕`��
                Rectangle frameRect = imageRect;
                if (App.Config.isDrawThumbnailShadow) // && isDrawFrame)
                {
                    BitmapUty.drawDropShadow(g, frameRect);
                }
                g.FillRectangle(Brushes.White, imageRect);

                //�摜��`��
                g.DrawImage(drawBitmap, imageRect);

                //�O�g�������B
                if (App.Config.isDrawThumbnailFrame) // && isDrawFrame)
                {
                    g.DrawRectangle(Pens.LightGray, frameRect);
                }

                //Bookmark�������}�[�N��`��
                if (m_thumbnailSet[item].isBookMark)
                {
                    using (Pen p = new Pen(Color.DarkRed, 2f))
                        g.DrawRectangle(p, frameRect);
                    g.FillEllipse(Brushes.Red, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                    g.DrawEllipse(Pens.White, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                }
            }
            else
            {
                //�o�ߎ����ɏ]���Ĕ������`��
                float a = (float)diff.TotalMilliseconds / ANIMATE_DURATION;
                if (a > 1)
                    a = 1.0f;
                BitmapUty.alphaDrawImage(g, drawBitmap, imageRect, a);
            }

            //�t�H�[�J�X�g
            if (item == m_mouseHoverItem)
            {
                using (Pen p = new Pen(Color.DodgerBlue, 3f))
                    g.DrawRectangle(p, imageRect);
            }

            //�摜��񕶎����`��
            DrawTextInfo(g, item, thumbnailBoxRect);
        }

        #endregion �A�C�e���`��

        //���i����p�`��DrawItem.
        //�_�~�[BMP�ɕ`�ʂ��邽�ߕ`�ʈʒu���Œ�Ƃ���B
        private void DrawItemHQ2(Graphics g, int item)
        {
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
            if (Parent == null)
                //�e�E�B���h�E���Ȃ��Ȃ��Ă���̂ŉ������Ȃ�
                return;

            Bitmap drawBitmap = GetBitmap(item);

            //�t���O�ݒ�
            bool drawFrame = true;          //�g����`�ʂ��邩
            bool isResize = true;           //���T�C�Y���K�v���i�\���j�ǂ����̃t���O
            int w;                          //�`�ʉ摜�̕�
            int h;                          //�`�ʉ摜�̍���

            if (drawBitmap == null)
            {
                //�T���l�C���͏����ł��Ă��Ȃ�
                drawBitmap = getDummyBitmap();
                drawFrame = false;
                isResize = false;
                w = drawBitmap.Width;   //�`�ʉ摜�̕�
                h = drawBitmap.Height;  //�`�ʉ摜�̍���
            }
            else
            {
                w = drawBitmap.Width;   //�`�ʉ摜�̕�
                h = drawBitmap.Height;  //�`�ʉ摜�̍���

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

            int sx = (BOX_WIDTH - w) / 2;           //�摜�`��X�ʒu
            int sy = THUMBNAIL_SIZE + PADDING - h;  //�摜�`��Y�ʒu�F������

            Rectangle imageRect = new Rectangle(sx, sy, w, h);

            //�e��`�ʂ���.�A�C�R�����i��drawFrame==false�j�ŕ`�ʂ��Ȃ�
            if (App.Config.isDrawThumbnailShadow && drawFrame)
            {
                Rectangle frameRect = imageRect;
                BitmapUty.drawDropShadow(g, frameRect);
            }

            //�摜������
            //g.DrawImage(drawBitmap, sx, sy, w, h);
            //�t�H�[�J�X�̂Ȃ��摜��`��
            g.FillRectangle(Brushes.White, imageRect);
            g.DrawImage(drawBitmap, imageRect);

            //�ʐ^���ɊO�g������
            if (App.Config.isDrawThumbnailFrame && drawFrame)
            {
                Rectangle frameRect = imageRect;
                //�g�����������̂Ŋg�債�Ȃ�
                //frameRect.Inflate(2, 2);
                //g.FillRectangle(Brushes.White, frameRect);//ver1.15 �R�����g�A�E�g�A�Ȃ񂾂����H
                g.DrawRectangle(Pens.LightGray, frameRect);
            }

            //�t�H�[�J�X�g������
            // �摜�T�C�Y�ɍ��킹�ĕ`��
            //if (item == m_mouseHoverItem)
            //{
            //    g.DrawRectangle(
            //        new Pen(Color.IndianRed, 2.5F),
            //        GetThumbImageRectangle(item));
            //}

            ////�摜���𕶎��`�ʂ���
            //RectangleF tRect = new RectangleF(PADDING, PADDING + THUMBNAIL_SIZE + PADDING, THUMBNAIL_SIZE, TEXT_HEIGHT);
            //DrawTextInfo(g, Item, tRect);

            //Bitmap�̔j���BGetBitmapWithoutCache()�Ŏ���Ă�������
            if (drawBitmap != null
                && (string)(drawBitmap.Tag) != Properties.Resources.TAG_PICTURECACHE)
                drawBitmap.Dispose();
        }

        private Bitmap getDummyBitmap()
        {
            return Properties.Resources.rc_tif32;
            //drawBitmap = new Bitmap(THUMBNAIL_SIZE, THUMBNAIL_SIZE);
            //using (Graphics g2 = Graphics.FromImage(drawBitmap))
            //{
            //    g2.Clear(Color.LightGray);
            //    g2.DrawRectangle(Pens.DarkGray, thumbnailBoxRect);
            //    var temprect = thumbnailBoxRect;
            //    temprect.Inflate(-1, -1);
            //    g2.DrawRectangle(Pens.White, temprect);
            //}
        }

        //*** �`�ʎx�����[�`�� ****************************************************************

        private Bitmap GetBitmap(int item)
        {
            //Form1::GetBitmap()���g���̂Őe�E�B���h�E�`�F�b�N
            if (Parent == null)
                return null;

            //�摜�ǂݍ���
            Bitmap orgBitmap = null;
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    //orgBitmap = ((Form1)Parent).GetBitmap(item);
                    //orgBitmap = Form1.g_pi.GetBitmap(item);
                    //ver1.50
                    orgBitmap = ((Form1)Parent).SyncGetBitmap(item);
                }));
            }
            else
            {
                //orgBitmap = ((Form1)Parent).GetBitmap(item);
                //orgBitmap = Form1.g_pi.GetBitmap(item);
                //ver1.50
                orgBitmap = ((Form1)Parent).SyncGetBitmap(item);
            }

            return orgBitmap;
        }

        /// <summary>
        /// �ĕ`�ʊ֐�
        /// �`�ʂ���镔�������ׂčĕ`�ʂ���B
        /// ���̃N���X�A�t�H�[������Ăяo�����B���̂���public
        /// ��Ƀ��j���[�Ń\�[�g���ꂽ�肵���Ƃ��ɌĂяo�����
        /// </summary>
        public void ReDraw()
        {
            //MakeThumbnailScreen();
            SetScrollBar(); //�X�N���[���o�[�̐ݒ�ƃT���l�C���ւ̏ꏊ�o�^
            fastDraw = false;
            this.Invalidate();
        }

        /// <summary>
        /// �w��T���l�C���̉�ʓ��ł̘g��Ԃ��B
        /// Thumbbox = �摜�{�����̑傫�Șg
        /// �X�N���[���o�[�ɂ��Ă��D�荞�ݍ�
        /// m_offScreen�����ʂɑ΂��Ďg���邱�Ƃ�z��
        /// </summary>
        private Rectangle GetThumbboxRectanble(int itemIndex)
        {
            // ver1.20 �������̃A�C�e���� ClientRectangle�ŃX�N���[���o�[�l��
            //int horizonItems = this.Width/BOX_WIDTH;
            int horizonItems = this.ClientRectangle.Width / BOX_WIDTH;
            if (horizonItems <= 0) horizonItems = 1;

            //�A�C�e���̈ʒu�i�A�C�e�����ɂ�鉼�z���W�j
            int vx = itemIndex % horizonItems;
            int vy = itemIndex / horizonItems;

            return new Rectangle(
                vx * BOX_WIDTH,
                vy * BOX_HEIGHT + AutoScrollPosition.Y,
                BOX_WIDTH,
                BOX_HEIGHT
                );
        }

        /// <summary>
        /// THUMBNAIL�C���[�W�̉�ʓ��ł̘g��Ԃ��B
        /// ThumbImage = �摜�����̂݁B�C���[�W�҂�����̃T�C�Y
        /// �X�N���[���o�[�ʒu���D�荞�ݍ�
        /// m_offScreen�����ʂɑ΂��Ďg���邱�Ƃ�z��
        /// </summary>
        private Rectangle GetThumbImageRectangle(int itemIndex)
        {
            //bool canExpand = true;	//�g��ł��邩�ǂ����̃t���O
            int w;                      //�`�ʉ摜�̕�
            int h;                      //�`�ʉ摜�̍���

            Image drawBitmap = m_thumbnailSet[itemIndex].thumbnail;
            if (drawBitmap == null)
            {
                //�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
                drawBitmap = getDummyBitmap();
                //canExpand = false;
                w = drawBitmap.Width;
                h = drawBitmap.Height;
            }
            else if (m_thumbnailSet[itemIndex].width <= THUMBNAIL_SIZE
                     && m_thumbnailSet[itemIndex].height <= THUMBNAIL_SIZE)
            {
                //�I���W�i�����������̂Ń��T�C�Y���Ȃ��B
                //canExpand = false;
                w = m_thumbnailSet[itemIndex].width;
                h = m_thumbnailSet[itemIndex].height;
            }
            else
            {
                //�T���l�C���͂���.�傫���̂ŏk��
                //canExpand = true;
                float fw = drawBitmap.Width;    //�`�ʉ摜�̕�
                float fh = drawBitmap.Height;   //�`�ʉ摜�̍���

                //�g��k�����s��
                float ratio = (fw > fh) ? (float)THUMBNAIL_SIZE / fw : (float)THUMBNAIL_SIZE / fh;
                w = (int)(fw * ratio);
                h = (int)(fh * ratio);
            }

            Rectangle rect = GetThumbboxRectanble(itemIndex);
            rect.X += (BOX_WIDTH - w) / 2;  //�摜�`��X�ʒu
            rect.Y += THUMBNAIL_SIZE + PADDING - h;     //�摜�`��X�ʒu�F������
                                                        //rect.Y -= m_vScrollBar.Value;
            rect.Width = w;
            rect.Height = h;
            return rect;
        }

        /// <summary>
        /// �t�@�C�����A�t�@�C���T�C�Y�A�摜�T�C�Y���e�L�X�g�`�ʂ���
        /// </summary>
        /// <param name="g">�`�ʐ��Graphics</param>
        /// <param name="item">�`�ʃA�C�e��</param>
        /// <param name="thumbnailBoxRect">�`�ʂ����̃T���l�C��BOX��`�B�e�L�X�g�ʒu�ł͂Ȃ�</param>
        private void DrawTextInfo(Graphics g, int item, Rectangle thumbnailBoxRect)
        {
            //�e�L�X�g�`�ʈʒu��␳
            Rectangle textRect = thumbnailBoxRect;
            //textRect.Inflate(-PADDING, 0);     //���E�̗]�����폜
            //textRect.Y += BOX_HEIGHT;	        //�摜����ǉ�
            //textRect.Height = FONT_HEIGHT;      //�t�H���g�̍����ɍ��킹��
            textRect.X += PADDING;                              //���ɗ]����ǉ�
            textRect.Y += PADDING + THUMBNAIL_SIZE + PADDING;   //�㉺�ɗ]����ǉ�
            textRect.Width = THUMBNAIL_SIZE;                    //�����̓T���l�C���T�C�Y�Ɠ���
            textRect.Height = FONT_HEIGHT;

            //�e�L�X�g�`�ʗp�̏����t�H�[�}�b�g
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;          //��������
            sf.Trimming = StringTrimming.EllipsisPath;      //���Ԃ̏ȗ�

            //�t�@�C����������
            if (App.Config.isShowTPFileName)
            {
                string filename = Path.GetFileName(m_thumbnailSet[item].filename);
                if (filename != null)
                {
                    g.DrawString(filename, m_font, new SolidBrush(m_fontColor), textRect, sf);
                    textRect.Y += FONT_HEIGHT;
                }
            }

            //�t�@�C���T�C�Y������
            if (App.Config.isShowTPFileSize)
            {
                string s = String.Format("{0:#,0} bytes", m_thumbnailSet[item].length);
                g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }

            //�摜�T�C�Y������
            if (App.Config.isShowTPPicSize)
            {
                string s = String.Format(
                    "{0:#,0}x{1:#,0} px",
                    m_thumbnailSet[item].width,
                    m_thumbnailSet[item].height);
                g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }
        }

        /// <summary>
        /// �T���l�C���A�C�e�����`�ʑΏۂ��ǂ����`�F�b�N����
        /// OnPaint()�Ŏg���邱�Ƃ��l������
        /// �`�ʗ̈���w��ł���悤�ɂ���
        /// </summary>
        /// <param name="item"></param>
        /// <param name="screenRect"></param>
        /// <returns></returns>
        private bool CheckNecessaryToDrawItem(int item, Rectangle screenRect)
        {
            Rectangle itemRect = GetThumbboxRectanble(item);
            return screenRect.IntersectsWith(itemRect);
        }

        /// <summary>
        /// �w�肵���ʒu�ɂ���A�C�e���ԍ���Ԃ�
        /// MouseHover�ł̗��p��z��B
        /// �X�N���[���o�[�̈ʒu�����p���ĕ␳�����l��Ԃ�
        /// </summary>
        /// <param name="pos">���ׂ����ʒu</param>
        /// <returns>���̏ꏊ�ɂ���A�C�e���ԍ��B�Ȃ��ꍇ��-1</returns>
        private int GetHoverItem(Point pos)
        {
            //�c�X�N���[���o�[���\������Ă���Ƃ��͊��Z
            //if (m_vScrollBar.Enabled)
            //    pos.Y += m_vScrollBar.Value;
            pos.Y -= AutoScrollPosition.Y;

            int itemPointX = pos.X / BOX_WIDTH;     //�}�E�X�ʒu��BOX���W���Z�FX
            int itemPointY = pos.Y / BOX_HEIGHT;    //�}�E�X�ʒu��BOX���W���Z�FY

            //���ɕ��ׂ��鐔�B�Œ�P
            int horizonItems = (this.ClientRectangle.Width) / BOX_WIDTH;
            if (horizonItems <= 0) horizonItems = 1;

            //�z�o�[���̃A�C�e���ԍ�
            int index = itemPointY * horizonItems + itemPointX;

            //�w��|�C���g�ɃA�C�e�������邩
            if (itemPointX > horizonItems - 1
                || index > m_thumbnailSet.Count - 1)
                return -1;
            else
                return index;
        }

        //***************************************************************************************

        #region �T���l�C���̃t�@�C���ۑ�

        /// <summary>
        /// �T���l�C���摜��ۑ�����B
        /// �����ł͕ۑ��p�_�C�A���O��\�����邾���B
        /// �_�C�A���O����SaveThumbnailImage()���Ăяo�����B
        /// </summary>
        /// <param name="filenameCandidate">�ۑ��t�@�C�����̌��</param>
        public void SaveThumbnail(string filenameCandidate)
        {
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //��������ۑ�
            int tmpThumbnailSize = THUMBNAIL_SIZE;
            //int tmpScrollbarValue = m_vScrollBar.Value;

            m_saveForm = new FormSaveThumbnail(this, m_thumbnailSet, filenameCandidate);
            m_saveForm.ShowDialog(this);
            m_saveForm.Dispose();

            //���ɖ߂�
            SetThumbnailSize(tmpThumbnailSize);
            //m_vScrollBar.Value = tmpScrollbarValue;
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

            //�T���l�C���T�C�Y��ݒ�.�Čv�Z
            SetThumbnailSize(thumbSize);

            //�A�C�e������ݒ�
            //m_nItemsX = numX;
            //m_nItemsY = ItemCount / m_nItemsX;	//�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
            //if (ItemCount % m_nItemsX > 0)
            //    m_nItemsY++;						//����؂�Ȃ������ꍇ��1�s�ǉ�

            Size offscreenSize = CalcScreenSize();

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

                    //ver1.31 null�`�F�b�N
                    if (SavedItemChanged != null)
                        this.SavedItemChanged(null, ev);
                    Application.DoEvents();

                    //�L�����Z������
                    if (m_saveForm.isCancel)
                        return false;
                }
            }

            saveBmp.Save(FilenameCandidate);
            saveBmp.Dispose();
            return true;
        }

        #endregion �T���l�C���̃t�@�C���ۑ�
    }
}
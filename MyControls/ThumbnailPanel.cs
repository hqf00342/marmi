using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
�T���l�C���p�l��
�L���b�V����Bitmap�Ȃǂ͂��炸�ɒ��ڃT���l�C����`�ʂ��Ă���B

TBOX: �T���l�C���̍ő�T�C�Y�l�p�`�B
  �� �� thumbnailSize + ���EPADDING
  �� �� thumbnailSize + �㉺PADDING +����������

*/

namespace Marmi
{
    public sealed class ThumbnailPanel : UserControl
    {
        private List<ImageInfo> m_ImgSet => App.g_pi.Items; //ImageInfo�̃��X�g, = g_pi.Items
        //private FormSaveThumbnail m_saveForm;   //�T���l�C���ۑ��p�_�C�A���O
        private int m_mouseHoverItem = -1;      //���݃}�E�X���z�o�[���Ă���A�C�e��

        private const int PADDING = 10;         //�T���l�C���̗]���B2014�N3��23���ύX�B�Ԋu��������
        private int _thumbnailSize;             //�T���l�C���̑傫���B��������
        private int _tboxWidth;                 //�T���l�C��BOX�̃T�C�Y�F�� = PADDING + THUMBNAIL_SIZE + PADDING
        private int _tboxHeight;                //�T���l�C��BOX�̃T�C�Y�F���� = PADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING

        //�t�H���g
        private Font _font;

        private Color _fontColor;
        private const string FONTNAME = "�l�r �S�V�b�N";
        private const int FONTSIZE = 9;
        private static int FONT_HEIGHT; //SetFont()���Őݒ肳���B

        //�T���l�C���ۑ��_�C�A���O�ɒm�点��C�x���g�n���h���[
        //public event EventHandler<ThumbnailEventArgs> SavedItemChanged;

        //�R���e�L�X�g���j���[
        private readonly ContextMenuStrip m_ContextMenu = new ContextMenuStrip();

        //�X�N���[���^�C�}�[
        private readonly Timer m_scrollTimer = new Timer();

        private int m_targetScrollposY = 0;

        private Bitmap DummyImage => Properties.Resources.rc_tif32;

        public ThumbnailPanel()
        {
            //������
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            //�_�u���o�b�t�@�B�̂̕��@�������Ă���
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            //�X�N���[���o�[�̏�����
            this.AutoScroll = true;

            //�t�H���g����
            SetFont(new Font(FONTNAME, FONTSIZE), Color.Black);

            //�T���l�C���T�C�Y����BOX�̒l�����肷��B
            SetThumbnailSize(App.DEFAULT_THUMBNAIL_SIZE);

            //�R���e�L�X�g���j���[�̏�����
            InitContextMenu();

            //�X�N���[���^�C�}�[
            m_scrollTimer.Interval = 50;
            m_scrollTimer.Tick += ScrollTimer_Tick;
        }

        public void Init()
        {
            //�X�N���[���ʒu�̏�����
            AutoScrollPosition = Point.Empty;
        }

        /// <summary>
        /// �T���l�C���T�C�Y��ݒ肷��B
        /// �ݒ�Ɠ����ɃX�N���[���o�[�Ȃǂ̃T�C�Y����������B
        /// </summary>
        /// <param name="thumbnailSize">TBOX�T�C�Y</param>
        public void SetThumbnailSize(int thumbnailSize)
        {
            _thumbnailSize = thumbnailSize;
            var size = CalcTboxSize(thumbnailSize);
            _tboxWidth = size.Width;
            _tboxHeight = size.Height;

            //�T���l�C���T�C�Y���ς�����̂ōČv�Z
            SetScrollBar();
        }

        /// <summary>
        /// TBOX�T�C�Y���v�Z����B
        /// �P����PADDING���ƕ����񕪂𑫂������́B
        /// </summary>
        public static Size CalcTboxSize(int thumbnailSize)
        {
            //TBOX�T�C�Y���m��
            var w = thumbnailSize + (PADDING * 2);
            var h = thumbnailSize + (PADDING * 2);

            //�����񕔒ǉ�
            if (App.Config.Thumbnail.DrawFilename)
                h += PADDING + FONT_HEIGHT;

            if (App.Config.Thumbnail.DrawFilesize)
                h += PADDING + FONT_HEIGHT;

            if (App.Config.Thumbnail.DrawPicsize)
                h += PADDING + FONT_HEIGHT;

            return new Size(w, h);
        }

        public void SetFont(Font font, Color color)
        {
            _font = font;
            _fontColor = color;

            //TEXT_HEIGHT�̌v�Z
            using (var bmp = new Bitmap(100, 100))
            using (var g = Graphics.FromImage(bmp))
            {
                SizeF sf = g.MeasureString("�e�X�g������", _font);
                FONT_HEIGHT = (int)sf.Height;
            }

            //�t�H���g���ς��ƃT���l�C���T�C�Y���ς��̂Ōv�Z
            SetThumbnailSize(_thumbnailSize);
        }

        /// <summary>
        /// ���̑傫���̃T�C�Y��T���Đݒ肷��
        /// </summary>
        public void ThumbSizeZoomIn()
        {
            foreach (ThumbnailSize d in Enum.GetValues(typeof(ThumbnailSize)))
            {
                int size = (int)d;
                if (size > _thumbnailSize)
                {
                    SetThumbnailSize(size);
                    App.Config.Thumbnail.ThumbnailSize = size;
                    this.Invalidate();
                    return;
                }
            }
        }

        /// <summary>
        /// 1�O�̃T���l�C���T�C�Y�������Đݒ肷��
        /// </summary>
        public void ThumbSizeZoomOut()
        {
            Array TSizes = Enum.GetValues(typeof(ThumbnailSize));
            Array.Sort(TSizes);
            Array.Reverse(TSizes);
            foreach (ThumbnailSize d in TSizes)
            {
                if ((int)d < _thumbnailSize)
                {
                    SetThumbnailSize((int)d);
                    App.Config.Thumbnail.ThumbnailSize = (int)d;
                    this.Invalidate();
                    return;
                }
            }
            //������Ȃ���΂Ȃɂ����Ȃ�
        }

        #region �R���e�L�X�g���j���[

        private void InitContextMenu()
        {
            //�R���e�L�X�g���j���[�̏���
            this.ContextMenuStrip = m_ContextMenu;
            m_ContextMenu.ShowImageMargin = false;
            m_ContextMenu.ShowCheckMargin = true;

            var separator = new ToolStripSeparator();
            var addBookmark = new ToolStripMenuItem("��������͂���");
            var Bookmarks = new ToolStripMenuItem("������ꗗ");
            var thumbnailLabel = new ToolStripMenuItem("�T���l�C���T�C�Y") { Enabled = false };
            var thumbSizeBig = new ToolStripMenuItem("�ő�");
            var thumbSizeLarge = new ToolStripMenuItem("��");
            var thumbSizeNormal = new ToolStripMenuItem("��");
            var thumbSizeSmall = new ToolStripMenuItem("��");
            var thumbSizeTiny = new ToolStripMenuItem("�ŏ�");
            var thumbShadow = new ToolStripMenuItem("�e������");
            var thumbFrame = new ToolStripMenuItem("�g��");

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
                    addBookmark.Checked = m_ImgSet[index].IsBookMark;
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
                switch (_thumbnailSize)
                {
                    case (int)ThumbnailSize.Minimum:
                        thumbSizeTiny.Checked = true;
                        break;

                    case (int)ThumbnailSize.Small:
                        thumbSizeSmall.Checked = true;
                        break;

                    case (int)ThumbnailSize.Normal:
                        thumbSizeNormal.Checked = true;
                        break;

                    case (int)ThumbnailSize.Large:
                        thumbSizeLarge.Checked = true;
                        break;

                    case (int)ThumbnailSize.XLarge:
                        thumbSizeBig.Checked = true;
                        break;
                }

                //�e�E�g�Ƀ`�F�b�N
                thumbFrame.Checked = App.Config.Thumbnail.DrawFrame;
                thumbShadow.Checked = App.Config.Thumbnail.DrawShadowdrop;

                //m_tooltip.Disposed += new EventHandler((se, ee) => { m_tooltip.Active = true; });

                //������ꗗ
                Bookmarks.DropDownItems.Clear();
                foreach (ImageInfo i in m_ImgSet)
                {
                    if (i.IsBookMark)
                    {
                        Bookmarks.DropDownItems.Add(i.Filename);
                    }
                }
            });
            m_ContextMenu.ItemClicked += ContextMenu_ItemClicked;
        }

        private void ContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "�ŏ�":
                    SetThumbnailSize((int)ThumbnailSize.Minimum);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Minimum;
                    break;

                case "��":
                    SetThumbnailSize((int)ThumbnailSize.Small);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Small;
                    break;

                case "��":
                    SetThumbnailSize((int)ThumbnailSize.Normal);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Normal;
                    break;

                case "��":
                    SetThumbnailSize((int)ThumbnailSize.Large);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Large;
                    break;

                case "�ő�":
                    SetThumbnailSize((int)ThumbnailSize.XLarge);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.XLarge;
                    break;

                case "�e������":
                    App.Config.Thumbnail.DrawShadowdrop = !App.Config.Thumbnail.DrawShadowdrop;
                    //Invalidate();
                    break;

                case "�g��":
                    App.Config.Thumbnail.DrawFrame = !App.Config.Thumbnail.DrawFrame;
                    //Invalidate();
                    break;

                case "��������͂���":
                    int index = (int)m_ContextMenu.Tag;
                    m_ImgSet[index].IsBookMark = !m_ImgSet[index].IsBookMark;
                    //this.Invalidate();
                    break;

                case "������ꗗ":
                    break;

                default:
                    break;
            }
            //��ʂ���������
            this.Invalidate();
        }

        #endregion �R���e�L�X�g���j���[

        #region override�֐�

        protected override void OnResize(EventArgs e)
        {
            //�X�N���[���o�[�̕\�����X�V
            SetScrollBar();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Debug.WriteLine($"ThumbnailPanel::OnPaint() ClipRect={e.ClipRectangle}");

            //�w�i�F�h��
            e.Graphics.Clear(this.BackColor);

            //�`�ʑΏۃ`�F�b�N�B������ΏI��
            if (m_ImgSet == null || m_ImgSet.Count == 0)
                return;

            //�`�ʕi���̌���
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //�N���b�v�̈���̃A�C�e���ԍ����Z�o
            int xItemsCount = this.ClientRectangle.Width / _tboxWidth;
            if (xItemsCount < 1) xItemsCount = 1;
            int startItem = (-AutoScrollPosition.Y + e.ClipRectangle.Y) / _tboxHeight * xItemsCount;

            //�E���̃A�C�e���ԍ���(�X�N���[���ʁ{��ʏc�j��BOX�c �̐؂�グ�~���A�C�e����
            int endItem = (int)Math.Ceiling((-AutoScrollPosition.Y + e.ClipRectangle.Bottom) / (double)_tboxHeight) * xItemsCount;
            if (endItem >= m_ImgSet.Count)
                endItem = m_ImgSet.Count - 1;

            //�K�v�Ȃ��̂�`��
            for (int item = startItem; item <= endItem; item++)
            {
                DrawItem3(e.Graphics, item);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //�A�C�e����1���Ȃ��Ƃ��͉������Ȃ�
            if (m_ImgSet == null)
                return;

            //�}�E�X�ʒu���N���C�A���g���W�Ŏ擾
            Point pos = this.PointToClient(Cursor.Position);

            //�z�o�[���̃A�C�e���ԍ�
            int itemIndex = GetHoverItem(pos);
            if (itemIndex == m_mouseHoverItem)
            {
                //�}�E�X���z�o�[���Ă���A�C�e�����ς��Ȃ��Ƃ��͉������Ȃ��B
                return;
            }

            //�z�o�[�A�C�e�����ւ���Ă���̂ōĕ`��
            int prevIndex = m_mouseHoverItem;
            m_mouseHoverItem = itemIndex;
            if (prevIndex >= 0)
                this.Invalidate(GetTboxRectanble(prevIndex));
            if (itemIndex >= 0)
                this.Invalidate(GetTboxRectanble(itemIndex));
            this.Update();

            //ver1.20 2011�N10��9�� �ĕ`�ʂ��I�������A���Ă���
            if (itemIndex < 0)
                return;

            //�X�e�[�^�X�o�[�ύX
            string s = $"[{itemIndex + 1}]{m_ImgSet[m_mouseHoverItem].Filename}";
            Form1._instance.SetStatusbarInfo(s);
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                //�N���b�N�ʒu�̉摜���擾
                int index = GetHoverItem(PointToClient(Cursor.Position));
                if (index < 0)
                {
                    return;
                }
                else
                {
                    await (Form1._instance).SetViewPageAsync(index);
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

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);

            var y = -this.AutoScrollPosition.Y;
            if (e.Delta > 0)
            {
                y -= 250;
                if (y < 0)
                    y = 0;
            }
            else
            {
                y += 250;
                Size virtualScreenSize = CalcScreenSize();
                int availablerange = virtualScreenSize.Height - this.ClientRectangle.Height;
                if (y > availablerange)
                    y = availablerange;
            }
            if (App.Config.Thumbnail.SmoothScroll)
            {
                //�A�j���[�V����������B
                //�X�N���[���^�C�}�[�̋N��
                m_targetScrollposY = y;
                if (!m_scrollTimer.Enabled)
                    m_scrollTimer.Start();
            }
            else
            {
                //�����ɃX�N���[��
                AutoScrollPosition = new Point(0, y);
                return;
            }
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
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
            if (m_ImgSet == null)
            {
                this.AutoScrollMinSize = this.ClientRectangle.Size;
                return;
            }

            //�S�A�C�e���`�ʂɕK�v�ȃT�C�Y���m�F�B
            Size vScreenSize = CalcScreenSize();

            //AutoScrollMinSize��ݒ肷��
            //���̃T�C�Y�������ƃX�N���[���o�[�������B
            //���X�N���[���o�[�͂���Ȃ��̂�X=1�Œ�
            //�c�X�N���[���o�[�͉��z��ʂ̍���
            this.AutoScrollMinSize = new Size(1, vScreenSize.Height);
        }

        /// <summary>
        /// �S�A�C�e���`�ʂɕK�v�ȃX�N���[���T�C�Y���v�Z����
        /// �X�N���[���o�[�͍ŏ�����T�C�Y�Ƃ��čl��
        /// </summary>
        private Size CalcScreenSize()
        {
            //�`�ʗ̈敝=�N���C�A���g�̈�𓾂�
            //ClientRectangle���g�����ƂŃX�N���[���o�[�����l������Ă���B
            int screenWidth = this.ClientRectangle.Width;
            if (screenWidth < 1) screenWidth = 1;

            //�A�C�e���̂��������XY�ŋ��߂�B
            var numX = screenWidth / _tboxWidth;
            if (numX == 0) numX = 1;
            var numY = ((m_ImgSet.Count - 1) / numX) + 1;

            return new Size(this.ClientRectangle.Width, _tboxHeight * numY);
        }

        /// <summary>
        /// �A�C�e���`�ʈʒu���v�Z����
        /// dot���W�ł͂Ȃ��A�A�C�e�����W
        /// </summary>
        /// <param name="index">�C���f�b�N�X�ԍ�</param>
        /// <returns>�`�ʈʒu</returns>
        private (int x, int y) CalcItemPosition(int index)
        {
            var numX = this.ClientRectangle.Width / _tboxWidth;
            if (numX == 0) numX = 1;
            return (index % numX, index / numX);
        }

        /// <summary>
        /// �w��A�C�e����`�ʂ���K�v�����邩�`�F�b�N
        /// DrawItem3()�Ŏg�����肾������������ClipRect�Ń`�F�b�N���Ă���̂�
        /// �s�v�ɂȂ���
        /// </summary>
        /// <param name="index"></param>
        /// <returns>���̈���Ȃ�true</returns>
        private bool NeedToDraw(int index)
        {
            //�ΏۃA�C�e����Rect
            //���ꂪ��ʓ��ʒu�ɕϊ��ς̂���ClientRect�ƒ��ڔ�r�ł���B
            var itemRect = GetTboxRectanble(index);

            //�������邩�`�F�b�N
            return this.ClientRectangle.IntersectsWith(itemRect);

            //�f�o�b�O
            //�X�N���[����Rect�B�X�N���[���o�[�l����
            //var screenTop = -this.AutoScrollPosition.Y; //�����Ȃ̂ŕ␳
            //var screenRect = new Rectangle(0, screenTop, ClientRectangle.Width, ClientRectangle.Height);
            //this.AutoScrollPosition �g�b�v��{X=0,Y=0}�A����{X=0,Y=-9498}
            //this.AutoScrollMargin    ���{Width=0, Height=0}
            //this.AutoScrollOffset    ���{X=0,Y=0}
            //this.AutoScrollMinSize   {Width=1, Height=13455}
        }

        //***************************************************************************************

        #region �A�C�e���`��

        /// <summary>
        /// �w��C���f�b�N�X�̃A�C�e�����R���g���[���ɕ`�ʂ���B
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="item">�A�C�e���ԍ�</param>
        private void DrawItem3(Graphics g, int item)
        {
            //�`�ʈʒu�̌���
            Rectangle tboxRect = GetTboxRectanble(item);

            //�N���b�v�͈͓����`�F�b�N
            if (!g.ClipBounds.IntersectsWith(tboxRect))
            {
                Debug.WriteLine($"�N���b�v�̈�O: {item}");
                return;
            }

            //�`�ʂ���r�b�g�}�b�v������
            Bitmap drawBitmap = m_ImgSet[item].Thumbnail;
            Rectangle imageRect = GetThumbImageRectangle(item);

            if (drawBitmap == null)
            {
                //�g�����`��
                tboxRect.Inflate(-PADDING, -PADDING);
                tboxRect.Height = _thumbnailSize;
                g.FillRectangle(Brushes.White, tboxRect);
                tboxRect.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightGray, tboxRect);

                //�T���l�C�����쐬
                Bmp.LoadBitmapAsync(item, true)
                    .ContinueWith(_ =>
                    {
                        //�R���g���[���\�����A���A�`�ʔ͈͓����`�F�b�N
                        //�`�ʔ͈͓��Ȃ�`�ʂ�����
                        if (this.Visible && NeedToDraw(item))
                        {
                            this.Invalidate(GetTboxRectanble(item));
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                return;
            }
            else
            {
                //�ʏ�`��
                //�e�̕`��
                Rectangle frameRect = imageRect;
                if (App.Config.Thumbnail.DrawShadowdrop)
                {
                    BitmapUty.DrawDropShadow(g, frameRect);
                }
                g.FillRectangle(Brushes.White, imageRect);

                //�摜��`��
                g.DrawImage(drawBitmap, imageRect);

                //�O�g�������B
                if (App.Config.Thumbnail.DrawFrame)
                {
                    g.DrawRectangle(Pens.LightGray, frameRect);
                }

                //Bookmark�}�[�N��`��
                if (m_ImgSet[item].IsBookMark)
                {
                    using (Pen p = new Pen(Color.DarkRed, 2f))
                        g.DrawRectangle(p, frameRect);
                    g.FillEllipse(Brushes.Red, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                    g.DrawEllipse(Pens.White, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                }
            }

            //�t�H�[�J�X�g
            if (item == m_mouseHoverItem)
            {
                using (Pen p = new Pen(Color.DodgerBlue, 3f))
                    g.DrawRectangle(p, imageRect);
            }

            //�摜��񕶎����`��
            DrawTextInfo(g, item, tboxRect);
        }

        #endregion �A�C�e���`��

        //*** �`�ʎx�����[�`�� ****************************************************************

        /// <summary>
        /// �ĕ`�ʊ֐�
        /// �`�ʂ���镔�������ׂčĕ`�ʂ���B
        /// ���̃N���X�A�t�H�[������Ăяo�����B���̂���public
        /// ��Ƀ��j���[�Ń\�[�g���ꂽ�肵���Ƃ��ɌĂяo�����
        /// </summary>
        public void ReDraw()
        {
            if (this.Visible)
            {
                //MakeThumbnailScreen();
                SetScrollBar(); //�X�N���[���o�[�̐ݒ�ƃT���l�C���ւ̏ꏊ�o�^
                this.Invalidate();
            }
        }

        /// <summary>
        /// �w��TBOX�́u��ʓ��ł̘g�ʒu�v��Ԃ��B
        /// Tbox = �摜�{�����̑傫�Șg
        /// �X�N���[���o�[�ɂ��ʒu�����������Ă���B
        /// </summary>
        private Rectangle GetTboxRectanble(int index)
        {
            // �A�C�e�����W
            (int itemx, int itemy) = CalcItemPosition(index);

            return new Rectangle(
                itemx * _tboxWidth,
                (itemy * _tboxHeight) + AutoScrollPosition.Y,
                _tboxWidth,
                _tboxHeight);

            //AutoScrollPosition.Y �̓X�N���[������ƕ��̒l�ɂȂ�B
            //����𑫂����ނ��Ƃŉ�ʓ��̈ʒu�ɕϊ����Ă���B
        }

        /// <summary>
        /// THUMBNAIL�C���[�W�̉�ʓ��ł̘g��Ԃ��B
        /// ThumbImage = �摜�����̂݁B�C���[�W�҂�����̃T�C�Y
        /// �X�N���[���o�[�ʒu���D�荞�ݍ�
        /// m_offScreen�����ʂɑ΂��Ďg���邱�Ƃ�z��
        /// </summary>
        private Rectangle GetThumbImageRectangle(int itemIndex)
        {
            int w;                      //�`�ʉ摜�̕�
            int h;                      //�`�ʉ摜�̍���

            Image drawBitmap = m_ImgSet[itemIndex].Thumbnail;
            if (drawBitmap == null)
            {
                //�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
                drawBitmap = DummyImage;
                //canExpand = false;
                w = drawBitmap.Width;
                h = drawBitmap.Height;
            }
            else if (m_ImgSet[itemIndex].Width <= _thumbnailSize
                     && m_ImgSet[itemIndex].Height <= _thumbnailSize)
            {
                //�I���W�i�����������̂Ń��T�C�Y���Ȃ��B
                //canExpand = false;
                w = m_ImgSet[itemIndex].Width;
                h = m_ImgSet[itemIndex].Height;
            }
            else
            {
                //�T���l�C���͂���.�傫���̂ŏk��
                //canExpand = true;
                float fw = drawBitmap.Width;    //�`�ʉ摜�̕�
                float fh = drawBitmap.Height;   //�`�ʉ摜�̍���

                //�g��k�����s��
                float ratio = (fw > fh) ? (float)_thumbnailSize / fw : (float)_thumbnailSize / fh;
                w = (int)(fw * ratio);
                h = (int)(fh * ratio);
            }

            Rectangle rect = GetTboxRectanble(itemIndex);
            rect.X += (_tboxWidth - w) / 2;  //�摜�`��X�ʒu
            rect.Y += _thumbnailSize + PADDING - h;     //�摜�`��X�ʒu�F������
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
            textRect.Y += PADDING + _thumbnailSize + PADDING;   //�㉺�ɗ]����ǉ�
            textRect.Width = _thumbnailSize;                    //�����̓T���l�C���T�C�Y�Ɠ���
            textRect.Height = FONT_HEIGHT;

            //�e�L�X�g�`�ʗp�̏����t�H�[�}�b�g
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,          //��������
                Trimming = StringTrimming.EllipsisPath      //���Ԃ̏ȗ�
            };

            //�t�@�C����������
            if (App.Config.Thumbnail.DrawFilename)
            {
                string filename = Path.GetFileName(m_ImgSet[item].Filename);
                if (filename != null)
                {
                    g.DrawString(filename, _font, new SolidBrush(_fontColor), textRect, sf);
                    textRect.Y += FONT_HEIGHT;
                }
            }

            //�t�@�C���T�C�Y������
            if (App.Config.Thumbnail.DrawFilesize)
            {
                string s = $"{m_ImgSet[item].FileLength:#,0} bytes";
                g.DrawString(s, _font, new SolidBrush(_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }

            //�摜�T�C�Y������
            if (App.Config.Thumbnail.DrawPicsize)
            {
                string s = $"{m_ImgSet[item].Width:#,0}x{m_ImgSet[item].Height:#,0} px";
                g.DrawString(s, _font, new SolidBrush(_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }
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
            pos.Y -= AutoScrollPosition.Y;

            int itemPointX = pos.X / _tboxWidth;     //�}�E�X�ʒu��BOX���W���Z�FX
            int itemPointY = pos.Y / _tboxHeight;    //�}�E�X�ʒu��BOX���W���Z�FY

            //���ɕ��ׂ��鐔�B�Œ�P
            int horizonItems = (this.ClientRectangle.Width) / _tboxWidth;
            if (horizonItems <= 0) horizonItems = 1;

            //�z�o�[���̃A�C�e���ԍ�
            int index = (itemPointY * horizonItems) + itemPointX;

            //�w��|�C���g�ɃA�C�e�������邩
            return itemPointX > horizonItems - 1 || index > m_ImgSet.Count - 1 ? -1 : index;
        }
    }
}
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
SideBar�N���X

�������̉摜�ꗗ�����[�ɕ\������o�[/�p�l���B
�摜���̂�PackageInfo���̂��̂̃|�C���^��������ĕ`�ʂ��Ă���B
�^�C�}�[�x���ɂ���ĕ���^�C�~���O���w��\�B
*/

namespace Marmi
{
    public class SideBar : UserControl
    {
        private const int GRIP_WIDTH = 8;        //�O���b�v�S�̂̕�
        private const int GRIP_HEIGHT = 32;      //�O���b�v�`�ʕ����̍���
        private const int THUMBSIZE = 120;       //�T���l�C���T�C�Y
        private const int PADDING = 2;           //�e��̗]��
        private const int NUM_WIDTH = 32;        //�ԍ������̕�
        private readonly int BOX_HEIGHT;            //BOX�T�C�Y�F�R���X�g���N�^�Ōv�Z
        private int m_hoverItem;                    //�I������Ă���A�C�e��
        private Point m_mouseDragPoint;             //�O���b�v���h���b�O���ꂽ�Ƃ���Point
        private PackageInfo m_packageInfo;          //g_pi���̂��̂�}��
        private int m_drawScrollValue;              //�X�N���[���ʒu�B��m_vsBar.Value�����������l���BDraw()�Q��
        private readonly Timer m_scrollTimer;       //�X�N���[���Ɋ��������邽�߂̃^�C�}�[
        private readonly Color m_NormalBackColor = Color.Black;
        private readonly Brush m_brNormalBack = Brushes.Black;
        private readonly SolidBrush m_brSelectBack = new SolidBrush(Color.FromArgb(224, Color.RoyalBlue));
        private readonly SolidBrush m_brHoverBack = new SolidBrush(Color.FromArgb(128, Color.RoyalBlue));
        private readonly VScrollBar m_vsBar = new VScrollBar();          //�X�N���[���o�[
        private readonly ToolTip m_tooltip = null;   //�c�[���`�b�v

        private readonly Font FONTL = App.Font10;
        private readonly Font FONTS = App.Font9;
        private readonly int FONTL_HEIGHT = App.Font10_Height;
        private readonly int FONTS_HEIGHT = App.Font9_Height;

        //�����`��/HQ�`�ʔ���p�t���O
        private bool fastDraw = false;

        // �T�C�Y�ύX�ʒm�C�x���g�pdelegate
        public event EventHandler SidebarSizeChanged;

        // ������ ***********************************************************************/

        public SideBar()
        {
            //�N���X�����o�[�ϐ��̐ݒ�
            BOX_HEIGHT = THUMBSIZE + (PADDING * 2);
            m_hoverItem = -1;
            m_mouseDragPoint = Point.Empty;

            //���̃R���g���[���̐ݒ�
            this.BackColor = Color.Transparent;         //�w�i�F�͓�����
            this.MinimumSize = new Size(GRIP_WIDTH, 1); //�ŏ�����ݒ�
            this.Width = 200;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer,
                true);

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);

            //�c�X�N���[���o�[�̐ݒ�
            //m_vsBar = new VScrollBar();
            m_vsBar.Visible = false;
            m_vsBar.ValueChanged += new EventHandler(VsBar_ValueChanged);
            m_vsBar.Value = 0;
            m_drawScrollValue = 0;
            this.Controls.Add(m_vsBar);

            //�X�N���[���^�C�}�[�̐ݒ�
            m_scrollTimer = new Timer { Interval = 10 };
            m_scrollTimer.Tick += new EventHandler(ScrollTimer_Tick);

            //�c�[���`�b�v�̐ݒ�
            m_tooltip = new ToolTip();

            //�������Ă���̂�DPI�X�P�[�����O�͖����ɂ���
            //�������A�@�\���Ă��Ȃ��͗l�Ȃ̂ŃR�����g�A�E�g���Ă���
            //this.AutoScaleMode = AutoScaleMode.None;
            //this.AutoScaleDimensions = new SizeF(0.0F, 0.0F);
        }

        /// <summary>PackageInfo��o�^����B
        /// </summary>
        /// <param name="pi">�o�^����PackageInfo</param>
        public void Init(PackageInfo pi)
        {
            m_packageInfo = pi;
            //m_packageInfo = null;
            m_vsBar.Visible = false;
            m_vsBar.Value = 0;

            SetScrollbar();
        }

        /// <summary>
        /// �w�肵���A�C�e���𒆐S�ʒu�ɂ���
        /// </summary>
        /// <param name="item">���S�ɂ������A�C�e���ԍ�</param>
        public void SetItemToCenter(int item)
        {
            if (!this.Visible)
                return;
            if (m_packageInfo == null)
                return;

            //ver1.30 2012�N2��19�� item�͈̔̓`�F�b�N
            if (item < 0 || item > m_packageInfo.Items.Count - 1)
                return;

            if (m_packageInfo != null)
            {
                //�X�N���[���o�[��\���A�C�e����������ʒu�ɂ���
                //�o����ΐ^�񒆓�����ɂ���
                int val = (item * BOX_HEIGHT) - (this.Height - BOX_HEIGHT) / 2;
                if (val < 0)
                    m_vsBar.Value = 0;
                else if (val > m_vsBar.Maximum - m_vsBar.LargeChange)
                    m_vsBar.Value = m_vsBar.Maximum - m_vsBar.LargeChange;
                else
                    m_vsBar.Value = val;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(m_NormalBackColor);

            if (m_packageInfo != null)
                //�\���ʒu��m_vs.Value�ł͂Ȃ������͂����邽��
                //m_drawScrollValue�ɂ���B
                DrawPanel(e.Graphics, m_drawScrollValue);

            //ver1.30 2012/02/19 �O���b�v�͍Ō�ɕ`��
            DrawGrip(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!m_mouseDragPoint.IsEmpty)
            {
                fastDraw = true;
                //�h���b�O���Ȃ̂łȂɂ����Ȃ�
                //m_vsBar.Visible = false;
            }
            else
            {
                SetScrollbar();
                fastDraw = false;
            }
            this.Refresh();
        }

        protected override async void OnClick(EventArgs e)
        {
            base.OnClick(e);

            //�m�[�}���T�C�Y�ł̓N���b�N�ʒu�ֈړ�
            int item = MousePointToItemNumber();
            if (m_packageInfo == null || item < 0 || item >= m_packageInfo.Items.Count)
                return;

            //((Form1)Parent).SetViewPage(item);
            await ((Form1)Form1._instance).SetViewPageAsync(item);

            //�A�C�e���𒆉��Ɏ����Ă���
            SetItemToCenter(item);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (this.Visible && m_vsBar.Visible)
            {
                int delta = BOX_HEIGHT;
                if (e.Delta < 0)
                    m_vsBar.Value = (m_vsBar.Value + delta <= m_vsBar.Maximum - m_vsBar.LargeChange)
                        ? m_vsBar.Value + delta
                        : m_vsBar.Maximum - m_vsBar.LargeChange;
                else
                    m_vsBar.Value = (m_vsBar.Value - delta > 0) ? m_vsBar.Value - delta : 0;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left
                && e.X >= this.Width - GRIP_WIDTH)
            {
                m_mouseDragPoint = this.PointToClient(MousePosition);
                m_vsBar.Visible = false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            //�h���b�O���삩�ǂ����`�F�b�N
            if (e.Button == MouseButtons.Left
                && !m_mouseDragPoint.IsEmpty)
            {
                m_mouseDragPoint = Point.Empty;
                m_vsBar.Visible = true;
                SetScrollbar();
                fastDraw = false;
                this.Refresh();

                //ver1.31 nullcheck
                if (SidebarSizeChanged != null)
                    this.SidebarSizeChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //�J�[�\���̐ݒ�
            if (e.X > this.Width - GRIP_WIDTH)
                Cursor.Current = Cursors.VSplit;
            else
                Cursor.Current = Cursors.Default;

            //�h���b�O���삩�ǂ����`�F�b�N
            if (e.Button == MouseButtons.Left
                && !m_mouseDragPoint.IsEmpty)
            {
                Point pt = this.PointToClient(MousePosition);

                int dx = pt.X - m_mouseDragPoint.X;
                this.Width += dx;
                App.Config.SidebarWidth = this.Width;

                m_mouseDragPoint = pt;

                //PicPanel�Ƃ̈ʒu�֌W�𒲐�
                ((Form1)Form1._instance).AjustSidebarArrangement();
            }

            //�A�C�e�����Ȃ���Ή������Ȃ�
            if (m_packageInfo == null)
                return;

            //�}�E�X�z�o�[���ς���Ă���΍ĕ`�ʂ���B
            int item = MousePointToItemNumber();
            if (item < 0)
                return;
            else if (m_hoverItem != item)
            {
                m_hoverItem = item;
                this.Invalidate();

                //ToolTip��\������
                //ToolTip�p�̕�����ݒ�
                string sz = $"{m_packageInfo.Items[item].Filename}\n ���t: {m_packageInfo.Items[item].CreateDate:yyyy�NM��d�� H:m:s}\n �傫��: {m_packageInfo.Items[item].FileLength:N0}bytes\n �T�C�Y: {m_packageInfo.Items[item].Width:N0}x{m_packageInfo.Items[item].Height:N0}�s�N�Z��";

                //ToolTip�̈ʒu��ݒ� ver0.9833
                int dispY = item * BOX_HEIGHT - m_vsBar.Value;
                if (dispY < 0)
                    dispY = 0;

                //ToolTip�\�� ver0.9833
                //m_tooltip.Show(sz, this, this.Width, e.Y, 3000);
                m_tooltip.Show(sz, this, this.Width, dispY, 3000);
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            //�t�H�[�J�X�𓖂Ă�
            this.Focus();
            this.Select();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            //ver1.30 2012�N2��24��
            //�}�E�X���O�ɏo����z�o�[������
            base.OnMouseLeave(e);
            m_hoverItem = -1;
            this.Invalidate();
        }

        private int MousePointToItemNumber()
        {
            Point pt = PointToClient(MousePosition);

            //�T�C�Y�ύX�O���b�v��ł̓N���b�N�C�x���g�͖���
            if (pt.X > this.Width - GRIP_WIDTH)
                return -1;

            //�A�C�e�����Ȃ��Ƃ��͖��� 2010/06/04
            if (m_packageInfo == null)
                return -1;

            //�N���b�N�����A�C�e����\��
            int y = pt.Y;
            if (m_vsBar.Visible)    //�X�N���[���o�[����␳
                y += m_vsBar.Value;

            int index = y / BOX_HEIGHT;
            if (index < 0 || index >= m_packageInfo.Items.Count)
                return -1;
            else
                return index;
        }

        // �X�N���[���o�[�֘A ***********************************************************/

        private void VsBar_ValueChanged(object sender, EventArgs e)
        {
            //ver 0.985 2010/06/03 �`�ʃ^�C�}�[���N��
            if (!m_scrollTimer.Enabled)
                m_scrollTimer.Start();
        }

        private void SetScrollbar()
        {
            if (m_packageInfo != null)
            {
                if (m_packageInfo.Items.Count * BOX_HEIGHT > this.Height)
                {
                    m_vsBar.Top = 0;
                    m_vsBar.Left = this.Width - GRIP_WIDTH - m_vsBar.Width;
                    m_vsBar.Height = this.Height;
                    //m_vsBar.Value = 0;
                    m_vsBar.Minimum = 0;
                    m_vsBar.Maximum = m_packageInfo.Items.Count * BOX_HEIGHT;
                    m_vsBar.LargeChange = this.Height;
                    m_vsBar.SmallChange = this.Height / 10;
                    m_vsBar.Show();
                }
                else
                    m_vsBar.Visible = false;
            }
            else
            {
                m_vsBar.Visible = false;
            }
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            int algorithm = 1;  //�`�ʃA���S���Y���B����switch���ŗ��p
            int mag = 10;       //�����́Balgorithm = 1�ŗ��p
            int diff = m_vsBar.Value - m_drawScrollValue;   //�ŏI�`�ʈʒu�Ƃ̍���

            //ver1.70 �X���[�X�X�N���[����Off�ɂ���
            //0: �A�j���[�V���������ŕ\���B1:�X���[�X�X�N���[��
            if (!App.Config.General.SmoothScrollOnSidebar)
                algorithm = 0;

            //�c�肪1�ȉ��Ȃ�ړ������ă^�C�}�[�X�g�b�v
            if (Math.Abs(diff) <= 1)
            {
                m_scrollTimer.Stop();
                m_drawScrollValue = m_vsBar.Value;
                fastDraw = false;
                this.Refresh();
                return;
            }

            //�X���[�X�ɓ�����
            fastDraw = true;
            switch (algorithm)
            {
                case 0:
                    //�W���A���S���Y���B�������Ȃ�
                    m_scrollTimer.Stop();
                    m_drawScrollValue = m_vsBar.Value;
                    this.Refresh();
                    break;

                case 1:
                    //����Z�őΉ��B�ȒP�Ȋ����͂�����
                    int adddiff = diff / mag;
                    if (adddiff == 0)
                        adddiff = Math.Sign(diff);  //1�C-1��������

                    //�X�N���[�����x�����ɂ���
                    //if (Math.Abs(adddiff) > m_vsBar.SmallChange*2)
                    //    adddiff = Math.Sign(diff) * m_vsBar.SmallChange*2;

                    //�`��
                    m_drawScrollValue += adddiff;
                    //Draw(this.CreateGraphics(), m_drawScrollValue);
                    this.Refresh();
                    break;
            }
        }

        // �I�[�i�[�h���[ ***************************************************************/

        /// <summary>
        /// �I�[�i�[�h���[�{��
        /// �\���Ώۂ̃A�C�e����S�ĕ`�ʂ���B
        /// �������ɂ͖��Ή��̂��ߌĂяo����NaviBar_Paint()�ł̑Ή����K�{�B
        /// </summary>
        /// <param name="g">�����o����Graphics</param>
        /// <param name="top">�`�ʂ�������Ƃ���̈ʒu�B��m_vsBar.Value</param>
        private void DrawPanel(Graphics g, int top)
        {
            //�O���b�v�����\������Ă��Ȃ��ꍇ��A�C�e���������ꍇ��
            //�`�ʂ��Ȃ��B
            if (this.Width <= GRIP_WIDTH            //�O���b�v�����\������Ă��Ȃ�
                || m_packageInfo == null            //�A�C�e����������Ă��Ȃ�
                || m_packageInfo.Items.Count < 1    //�A�C�e����1���o�^����Ă��Ȃ�
                )
            {
                //�O���b�v�g��`�ʂ���
                //DrawGrip(g);
                return;
            }

            //�A�C�e���`�ʂ̂��߂̕ϐ���`
            int scbarWidth = (m_vsBar.Visible) ? m_vsBar.Width : 0; //�c�X�N���[���o�[�̕�
            int ItemCount = m_packageInfo.Items.Count;              //���A�C�e����
            int startItem = top / BOX_HEIGHT;                       //��ԏ�̃A�C�e���C���f�b�N�X
            if (startItem < 0)
                startItem = 0;
            int endItem = (top + this.Height) / BOX_HEIGHT + 1;     //��ԉ��̃A�C�e���C���f�b�N�X
            if (endItem > ItemCount)
                endItem = ItemCount;

            //�摜�̕`�ʁ@ver1.37DrawItem����ړ�
            if (fastDraw)
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            else
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //�A�C�e����\�������􂷃��[�`��
            for (int index = startItem; index <= endItem; index++)
            {
                var rc = new Rectangle(
                    0,                                      // �`�ʊJ�n�ʒux = 0;
                    index * BOX_HEIGHT - top,               // �`�ʊJ�n�ʒuy = �{�b�N�X�̐�΍���-�X�N���[���l
                    this.Width - GRIP_WIDTH - scbarWidth,   // ���̓O���b�v�ƃX�N���[���o�[�̕�������
                    (THUMBSIZE + PADDING * 2)               // ������THUMBSIZE��
                    );

#pragma warning disable CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
                //����ɕ`�ʂ���邱�Ƃ�z�肵�Aawait���Ȃ�
                DrawItemAsync(index, g, rc);
#pragma warning restore CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
            }

            //�w�i�������Ȃ̂ŃA�C�e�������Ȃ��Ɠ����̂܂܂ɂȂ�
            //���̕�����w�i�F�œh��
            int endY = (endItem) * BOX_HEIGHT - top;
            if (endY < this.Height)
            {
                g.FillRectangle(m_brNormalBack, 0, endY, this.Width - GRIP_WIDTH, this.Height - endY);
            }
        }

        /// <summary>
        /// �ΏۂƂ���A�C�e�����w��ʒu�ɕ`�ʂ���
        /// </summary>
        /// <param name="index">�ΏۃA�C�e���ԍ�</param>
        /// <param name="g">�����o����Graphics</param>
        /// <param name="rect">�`�ʂ���ʒu</param>
        private async Task DrawItemAsync(int index, Graphics g, Rectangle rect)
        {
            if (m_packageInfo == null)
                return;

            //�C���f�b�N�X�͈̓`�F�b�N
            if (index < 0 || index >= m_packageInfo.Items.Count)
                return;

            //�w�i�F�����O�ŕ`��.���ʂ͓h��Ȃ��Ă�����
            if (index == m_hoverItem)
            {
                g.FillRectangle(m_brHoverBack, rect);
            }
            else if (index == m_packageInfo.NowViewPage)
            {
                g.FillRectangle(m_brSelectBack, rect);
            }

            //�����̕`��:�摜�ʂ��ԍ�
            int x = rect.X + 2;
            int y = rect.Y + 20;
            string sz = $"{index + 1}";

            g.DrawString(sz, FONTS, Brushes.DarkGray, x, y);

            //����`�ʑΏۂ̃A�C�e��
            var imgInfo = m_packageInfo.Items[index];

            x = rect.X + PADDING + NUM_WIDTH;
            y = rect.Y + PADDING;
            if (imgInfo.Thumbnail != null)
            {
                int tWidth = imgInfo.Thumbnail.Width;
                int tHeight = imgInfo.Thumbnail.Height;

                //�g��k������
                var ratio = 1.0F;
                if (tWidth > tHeight && tWidth > THUMBSIZE)
                {
                    ratio = (float)THUMBSIZE / tWidth;
                }
                else if (tHeight > THUMBSIZE)
                {
                    ratio = (float)THUMBSIZE / tHeight;
                }

                var drawImageRect = new RectangleF(
                    x + (THUMBSIZE - tWidth * ratio) / 2,   // �n�_X
                    y + (THUMBSIZE - tHeight * ratio) / 2,  // �n�_Y�ʒu
                    tWidth * ratio,                         // �摜��
                    tHeight * ratio                         // �摜��
                    );

                //�T���l�C���摜�̕`��
                g.DrawImage(
                    imgInfo.Thumbnail,
                    Rectangle.Round(drawImageRect));

                //�g�̕`��
                g.DrawRectangle(
                    Pens.LightGray,
                    Rectangle.Round(drawImageRect));
            }
            else
            {
                //�摜�������Ă��Ȃ��̂Řg�����`��
                var drawImageRect = new RectangleF(x, y, THUMBSIZE, THUMBSIZE);
                g.DrawRectangle(
                    Pens.LightGray,
                    Rectangle.Round(drawImageRect));

                //ver1.81 �摜�����ɍs��
                //���̌�T���l�C���o�^.�^�C�}�[���~�܂��Ă�����s
                if (m_scrollTimer == null || !m_scrollTimer.Enabled)
                {
                    await Bmp.LoadBitmapToCacheAsync(index, false);
                    if (this.Visible)
                        this.Invalidate();
                }
            }

            //�����̕`��:�t�@�C����
            Rectangle strRect = rect;
            strRect.X = x + PADDING + NUM_WIDTH + THUMBSIZE;
            strRect.Width = rect.Width - strRect.Left;
            strRect.Y = y;
            strRect.Height = FONTL_HEIGHT;
            sz = $"{Path.GetFileName(imgInfo.Filename)}";
            g.DrawString(sz, FONTL, Brushes.White, strRect);
            strRect.Y += FONTL_HEIGHT + PADDING;

            //�����̕`��:�T�C�Y, ���t
            //x += 10;
            strRect.X += PADDING;
            strRect.Width = rect.Width - strRect.Left;
            strRect.Height = FONTS_HEIGHT;
            sz = $"{imgInfo.FileLength:N0}bytes,   {imgInfo.CreateDate}";
            g.DrawString(sz, FONTS, Brushes.LightGray, strRect);
            strRect.Y += FONTS_HEIGHT + PADDING;

            //�����̕`��:�s�N�Z����
            sz = $"{imgInfo.Width:N0}x{imgInfo.Height:N0}pixels";
            g.DrawString(sz, FONTS, Brushes.RoyalBlue, strRect);
        }

        /// <summary>
        /// �O���b�v�����̕`��
        /// �`�ʈʒu�͎��g�̉E�[��GRIP_WIDTH�ŕ`�ʂ���
        /// </summary>
        /// <param name="g">�����o����Graphics</param>
        private void DrawGrip(Graphics g)
        {
            Rectangle r = new Rectangle(this.Width - GRIP_WIDTH, 0, GRIP_WIDTH, this.Height);

            g.FillRectangle(SystemBrushes.Control, r);
            g.DrawLine(SystemPens.ControlDark, r.Left, r.Top, r.Left, r.Bottom);
            g.DrawLine(SystemPens.ControlDark, r.Right, r.Top, r.Right, r.Bottom);

            //�������`��
            int sx = this.Width - GRIP_WIDTH + 2;
            int sy = (this.Height - GRIP_HEIGHT) / 2;
            g.DrawLine(SystemPens.ControlLightLight, sx, sy, sx, sy + GRIP_HEIGHT);
            sx++;
            g.DrawLine(SystemPens.ControlDark, sx, sy, sx, sy + GRIP_HEIGHT);
            sx += 2;
            g.DrawLine(SystemPens.ControlLightLight, sx, sy, sx, sy + GRIP_HEIGHT);
            sx++;
            g.DrawLine(SystemPens.ControlDark, sx, sy, sx, sy + GRIP_HEIGHT);
        }
    }
}
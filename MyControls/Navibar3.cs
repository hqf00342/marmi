using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public class NaviBar3 : UserControl
    {
        //�T���l�C���T�C�Y
        private const int THUMBSIZE = 200; //320;

                                           //�e��]��
                                           //private const int PADDING = 6;
        private const int PADDING = 2;

        //���E�̉摜�̈Â���
        private const int DARKPERCENT = 50;

        //BOX�T�C�Y�F�R���X�g���N�^�Ōv�Z
        private int BOX_HEIGHT;

        //�I������Ă���A�C�e��
        public int m_selectedItem;

        //g_pi���̂��̂�}��
        private PackageInfo m_packageInfo;

        //�w�i�F
        private SolidBrush m_BackBrush = new SolidBrush(Color.FromArgb(192, 48, 48, 48));

        //�e�L�X�g�`�ʃt�H���g
        private Font fontS = new Font("�l�r �o �S�V�b�N", 9F);

        private Font fontL = new Font("Century Gothic", 16F);

        //�e�L�X�g�`�ʃt�H�[�}�b�g
        private StringFormat sfCenterUp = new StringFormat() { Alignment = StringAlignment.Center };

        private StringFormat sfCenterDown = new StringFormat()
        { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        //�e�L�X�g�̍���
        private int FONT_HEIGHT;

        //�����x
        private float alpha = 1.0F;     //�`�ʎ��̓����x�BOnPaint()�Ɍ����Ă���

                                        //�T���l�C���̈ʒu
        private List<ItemPos> thumbnailPos = null;

        // Loading�C���[�W
        private Bitmap dummyImage = null;

        //�A�j���[�V�����^�C�}�[
        private System.Windows.Forms.Timer timer = null;

        //���݂̕`�ʈʒu
        private int nowOffset;

        // ������ ***********************************************************************/

        public NaviBar3(PackageInfo pi)
        {
            m_packageInfo = pi;
            m_selectedItem = -1;

            //�w�i�F
            this.BackColor = Color.Transparent;
            //�_�u���o�b�t�@��L����
            this.DoubleBuffered = true;

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);
            //DPI�X�P�[�����O�͖����ɂ���
            this.AutoScaleMode = AutoScaleMode.None;
            //�����x��1.0
            alpha = 1.0F;

            //�������Z�o
            BOX_HEIGHT = PADDING + THUMBSIZE;
            //new���ꂽ���Ƃɍ�����K�v�Ƃ����̂ō�����������Ă����B
            this.Height = BOX_HEIGHT        //�摜����
                                            //+ PADDING
                + PADDING;

            //font�̍����𑪂�
            using (Bitmap bmp = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    SizeF sf = g.MeasureString("�e�X�g������", fontS);
                    FONT_HEIGHT = (int)sf.Height;
                }
            }

            //Loading�ƕ\������C���[�W
            dummyImage = BitmapUty.LoadingImage(THUMBSIZE * 2 / 3, THUMBSIZE);

            //�^�C�}�[�̏����ݒ�
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;
            timer.Tick += new EventHandler(timer_Tick);

            //�I�t�Z�b�g��ݒ�
            nowOffset = 0;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            int diff = (GetOffset(m_selectedItem) - nowOffset);
            //THUMBSIZE�ȏ㗣��Ă����炷����THUMBSIZE�ɋ߂Â���
            //if (diff > THUMBSIZE*2)
            //    diff = diff - THUMBSIZE*1;
            //else
            diff = diff * 2 / 7;
            nowOffset += diff;

            if (diff == 0)
            {
                timer.Stop();
                CalcAllItemPos();
                Debug.WriteLine("Timer Stop diff 0");
            }
            //�`��
            this.Refresh();
        }

        // public���\�b�h/�v���p�e�B ****************************************************/

        public void OpenPanel(Rectangle rect, int index)
        {
            //�T���l�C���쐬���Ȃ��x�~�߂�
            //Form1.PauseThumbnailMakerThread();

            this.Top = rect.Top;
            this.Left = rect.Left;
            this.Width = rect.Width;
            //this.Height = BOX_HEIGHT        //�摜����
            //    + PADDING + FONT_HEIGHT      //�摜�ԍ���
            //    + PADDING + FONT_HEIGHT      //�t�@�C����
            //    + PADDING;
            this.Height = BOX_HEIGHT        //�摜����
                                            //+PADDING
                + PADDING;

            m_selectedItem = index;

            #region �������`�ʂ��Ȃ���X���C�h

            //alpha = 0.0F;
            //this.Visible = true;
            //for (int i = 1; i <= 5; i++)
            //{
            //    alpha = i * 0.2F;					//�����x��ݒ�
            //    //this.Top = rect.Top + i - 10;		//�X���C�h�C��������

            //    this.Refresh();
            //    Application.DoEvents();
            //}

            #endregion �������`�ʂ��Ȃ���X���C�h

            //�A�C�e���ʒu���v�Z
            CalcAllItemPos();

            //�����ʒu������
            nowOffset = GetOffset(index);

            this.Visible = true;
            alpha = 1.0F;
            this.Refresh();

            //timer���~�߂�
            if (timer != null)
                timer.Stop();
        }

        public void ClosePanel()
        {
            //�T���l�C���쐬���Ȃ��x�~�߂�
            //Form1.PauseThumbnailMakerThread();

            #region �������`�ʂ��Ȃ���fede out

            //for (int i = 1; i <= 5; i++)
            //{
            //    alpha = 1 - i * 0.2F;		//�����x��ݒ�
            //    //this.Top--;				//�X���C�h�A�E�g������

            //    this.Refresh();
            //    Application.DoEvents();		//���ꂪ�Ȃ��ƃt�F�[�h�A�E�g���Ȃ�
            //}

            #endregion �������`�ʂ��Ȃ���fede out

            this.Visible = false;
            alpha = 1.0F;

            //timer���~�߂�
            if (timer != null)
                timer.Stop();
        }

        public void SetCenterItem(int index)
        {
            m_selectedItem = index;

            //ver1.37�o�O�Ώ�:��\���ł͂Ȃɂ����Ȃ�
            if (!Visible)
                return;

            if (timer != null)
            {
                //�^�C�}�[�ŕ`��
                timer.Enabled = true;
                Debug.WriteLine("TimerStart at SetCenterItem");
            }
            else
            {
                //�^�C�}�[���g�킸�ɍĕ`��
                nowOffset = GetOffset(index);
                this.Refresh();
            }
        }

        // �I�[�i�[�h���[ ***************************************************************/

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            //g.Clear(m_NormalBackColor);

            if (alpha >= 1.0F)
            {
                //DrawItems(g);
                DrawItemAll(g);
            }
            else
            {
                using (Bitmap bmp = new Bitmap(this.Width, this.Height))
                {
                    Graphics.FromImage(bmp).Clear(Color.Transparent);
                    //DrawItems(Graphics.FromImage(bmp));
                    DrawItemAll(Graphics.FromImage(bmp));
                    BitmapUty.alphaDrawImage(g, bmp, alpha);
                }
            }
        }

        /// <summary>
        /// ver1.36
        /// �V�o�[�W�����̃A�C�e���`�ʃ��[�`��
        /// </summary>
        /// <param name="g"></param>
        private void DrawItemAll(Graphics g)
        {
            //�w�i�F�Ƃ��č��œh��Ԃ�
            g.FillRectangle(m_BackBrush, this.DisplayRectangle);

            //�\�����ׂ��A�C�e�����Ȃ��ꍇ�͔w�i����
            if (m_packageInfo == null || m_packageInfo.Items.Count < 1)
                return;

            //���ׂẴA�C�e���̈ʒu���X�V
            //CalcAllItemPos();

            //�I�t�Z�b�g���v�Z
            int offset;
            if (timer == null)
                //�^�C�}�[�������Ă��Ȃ��Ƃ��͂������̏ꏊ��
                offset = GetOffset(m_selectedItem);
            else
                //�^�C�}�[�������Ă���Ƃ���offset��Timer���X�V
                offset = nowOffset;

            //�S�A�C�e���`��
            for (int item = 0; item < m_packageInfo.Items.Count; item++)
                DrawItem(g, item, offset);
            return;
        }

        /// <summary>
        /// �T���l�C���̕\���ʒu�����ׂčX�V
        /// </summary>
        private void CalcAllItemPos()
        {
            if (thumbnailPos == null)
            {
                //�V�K�Ɉʒu���X�g���쐬
                thumbnailPos = new List<ItemPos>();
                int X = 0;
                for (int i = 0; i < m_packageInfo.Items.Count; i++)
                {
                    ItemPos item = new ItemPos();
                    item.pos.X = X;
                    item.pos.Y = PADDING;
                    if (m_packageInfo.Items[i].thumbnail != null)
                    {
                        item.size = BitmapUty.calcHeightFixImageSize(m_packageInfo.Items[i].thumbnail.Size, THUMBSIZE);
                    }
                    else
                        item.size = new System.Drawing.Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    thumbnailPos.Add(item);
                    X += item.size.Width + PADDING;
                }
            }
            else
            {
                //���łɂ�����̂��X�V
                int X = 0;
                for (int i = 0; i < m_packageInfo.Items.Count; i++)
                {
                    //ItemPos item = thumbnailPos[i];
                    thumbnailPos[i].pos.X = X;
                    thumbnailPos[i].pos.Y = PADDING;
                    if (m_packageInfo.Items[i].thumbnail != null)
                        thumbnailPos[i].size = BitmapUty.calcHeightFixImageSize(m_packageInfo.Items[i].thumbnail.Size, THUMBSIZE);
                    else
                        thumbnailPos[i].size = new System.Drawing.Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    X += thumbnailPos[i].size.Width + PADDING;
                }
            }
        }

        /// <summary>
        /// �����̃A�C�e���ԍ�����I�t�Z�b�g���ׂ��s�N�Z�������v�Z
        /// </summary>
        /// <param name="centerItem">�����̃A�C�e���ԍ�</param>
        /// <returns></returns>
        private int GetOffset(int centerItem)
        {
            int X = thumbnailPos[centerItem].pos.X;
            int center = X + thumbnailPos[centerItem].size.Width / 2;
            int offset = center - this.Width / 2;
            return offset;
        }

        /// <summary>
        /// �w�肵��1�A�C�e����`�ʂ���
        /// </summary>
        /// <param name="g">�`�ʐ��Graphics</param>
        /// <param name="index">�`�ʃA�C�e���ԍ�</param>
        private void DrawItem(Graphics g, int index, int offset)
        {
            //Index�`�F�b�N
            //if (index < 0 || index >= m_packageInfo.Items.Count)
            //    return;
            //���v�Z��������v�Z
            if (thumbnailPos == null)
                CalcAllItemPos();

            //�`�ʈʒu������
            int x = thumbnailPos[index].pos.X - offset;

            //�`�ʑΏۊO���͂���
            if (x > this.Width)
                return;
            if (x + thumbnailPos[index].size.Width < 0)
                return;

            //�T�C�Y����
            Bitmap img = BitmapUty.MakeHeightFixThumbnailImage(
                m_packageInfo.Items[index].thumbnail as Bitmap,
                THUMBSIZE);

            if (img == null)
            {
                img = dummyImage;
                //�񓯊��ŉ摜���擾���Ă���
                //�^�C�}�[���~�܂��Ă�����񓯊��ŉ摜�擾
                //if (timer == null || !timer.Enabled)
                //{
                //	Form1._instance.AsyncGetBitmap(index, (MethodInvoker)(() =>
                //		{
                //			Debug.WriteLine("Navibar3::DrawItem() GoGO");
                //			//�Čv�Z
                //			//CalcAllItemPos();

                //			//GUI�X���b�h�ōĕ`��
                //			//��\���ł͉������Ȃ�
                //			if (!this.Visible)
                //				return;

                //			//�^�C�}�[���쒆���������Ȃ�
                //			//if (timer != null && timer.Enabled)
                //			//    return;

                //			//�ĕ`��
                //			CalcAllItemPos();
                //			nowOffset = GetOffset(m_selectedItem);
                //			if(this.Visible)
                //				this.Invalidate();
                //		}));
                //}

                //ver1.81 �ǂݍ��݃��[�`����PushLow()�ɕύX
                if (timer == null || !timer.Enabled)
                {
                    //Form1.PushLow(index, (Action)(() =>
                    //{
                    //    var bmp = Form1.SyncGetBitmap(index);
                    //    App.g_pi.ThumnailMaker(index, bmp);
                    //    CalcAllItemPos();
                    //    if (this.Visible)
                    //        this.Invalidate();
                    //}));
                    AsyncIO.AddJobLow(index, () =>
                    {
                        var bmp = Form1.SyncGetBitmap(index);
                        App.g_pi.ThumnailMaker(index, bmp);
                        CalcAllItemPos();
                        if (this.Visible)
                            this.Invalidate();
                    });
                }
            }

            Rectangle cRect = new Rectangle(
                thumbnailPos[index].pos.X - offset,
                thumbnailPos[index].pos.Y + BOX_HEIGHT - img.Height - PADDING,      //������
                thumbnailPos[index].size.Width,
                thumbnailPos[index].size.Height);

            //�`��
            if (index == m_selectedItem)
            {
                //�����̃A�C�e��
                //ver1.17�ǉ� �t�H�[�J�X�g
                BitmapUty.drawBlurEffect(g, cRect, Color.LightBlue);
                //������`��
                g.DrawImage(img, cRect);

                //ver1.62 �R�����g�A�E�g
                ////�摜�ԍ���`��
                //Rectangle stringRect = new Rectangle(
                //    0,
                //    BOX_HEIGHT + PADDING,
                //    this.Width,
                //    FONT_HEIGHT);

                //g.DrawString(
                //    string.Format("{0}", m_selectedItem + 1),
                //    fontS,
                //    Brushes.White,
                //    stringRect,
                //    sfCenterUp);

                ////�����̕������`��
                //stringRect.X = 0;
                //stringRect.Y = BOX_HEIGHT + PADDING + FONT_HEIGHT + PADDING;
                //stringRect.Width = this.Width;
                //stringRect.Height = FONT_HEIGHT;

                //g.DrawString(
                //    Path.GetFileName(m_packageInfo.Items[m_selectedItem].filename),
                //    fontS,
                //    Brushes.White,
                //    stringRect,
                //    sfCenterUp);
            }
            else
            {
                //�����ȊO�̉摜��`��
                g.DrawImage(
                    BitmapUty.BitmapToDark(img, DARKPERCENT),
                    cRect);

                ////�摜�ԍ���`��
                //cRect.Y = BOX_HEIGHT + PADDING;
                //g.DrawString(
                //    string.Format("{0}", index + 1),
                //    fontS,
                //    Brushes.LightGray,
                //    cRect,
                //    sfCenter);
            }

            //�摜�ԍ����摜��ɕ\��
            g.DrawString(
                string.Format("{0}", index + 1),
                fontL,
                Brushes.LightGray,
                cRect,
                sfCenterDown);
        }

        /// <summary>
        /// �񓯊��ŃT���l�C�����쐬�A�`�ʂ܂ōs��
        /// �����ThreadPool�ɓ����
        /// </summary>
        /// <param name="index"></param>
        //private void AsyncGetBitmapAndDraw(int index)
        //{
        //    //2�d�ɔ񓯊��擾�����s����邱�Ƃ�����̂Ń`�F�b�N
        //    if (m_packageInfo.Items[index].ThumbImage != null)
        //        return;

        //    //Bitmap���擾���T���l�C���쐬
        //    //Form1._instance.GetBitmap(index);
        //    m_packageInfo.GetBitmap(index);	//ver1.39

        //    //�Čv�Z
        //    //CalcAllItemPos();

        //    //GUI�X���b�h�ōĕ`��
        //    BeginInvoke((MethodInvoker)(() =>
        //    {
        //        //��\���ł͉������Ȃ�
        //        if (!this.Visible)
        //            return;

        //        //�^�C�}�[���쒆���������Ȃ�
        //        //if (timer != null && timer.Enabled)
        //        //    return;

        //        //�ĕ`�ʂ̕K�v�L
        //        CalcAllItemPos();
        //        nowOffset = GetOffset(m_selectedItem);
        //        this.Invalidate();
        //    }));
        //}
    }

    /// <summary>
    /// �`�ʈʒu��ۑ����邽�߂̍\���́i�N���X�j
    /// </summary>
    public class ItemPos
    {
        public Point pos;
        public Size size;
        public float brightness;
    }
}
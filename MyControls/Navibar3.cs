/********************************************************************************
NaviBar3

�g���b�N�o�[�ƘA�����ăT���l�C���\������p�l���B
������ς����ق���������������Ȃ�
�^�C�}�[���g���ăA�j���[�V���������Ȃ���`�ʂ��Ă���B
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormTimer = System.Windows.Forms.Timer;

namespace Marmi
{
    public class NaviBar3 : UserControl
    {
        private const int THUMBSIZE = 200;  //�T���l�C���T�C�Y
        private const int PADDING = 2;      //�e��]��
        private const int DARKPERCENT = 50; //���E�̉摜�̈Â���
        private readonly int BOX_HEIGHT;    //BOX�T�C�Y�F�R���X�g���N�^�Ōv�Z
        public int _selectedItem;           //�I������Ă���A�C�e��
        private float _alpha = 1.0F;        //�`�ʎ��̓����x�BOnPaint()�ŗ��p
        private int _offset;                //���݂̕`�ʈʒu.�s�N�Z����

        private readonly PackageInfo _packageInfo;        //g_pi���̂��̂�}��
        private readonly SolidBrush _BackBrush = new SolidBrush(Color.FromArgb(192, 48, 48, 48));        //�w�i�F

        //�t�H���g,�t�H�[�}�b�g
        private readonly Font fontL = new Font("Century Gothic", 16F);

        private readonly StringFormat sfCenterDown = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        private List<ItemPos> _thumbnailPosList = null; //�T���l�C���̈ʒu
        private readonly Bitmap _dummyImage = null;     // Loading�C���[�W
        private readonly FormTimer _timer = null;       //�A�j���[�V�����^�C�}�[

        // ������ ***********************************************************************/

        public NaviBar3(PackageInfo pi)
        {
            _packageInfo = pi;
            _selectedItem = -1;

            //�w�i�F
            this.BackColor = Color.Transparent;
            //�_�u���o�b�t�@��L����
            this.DoubleBuffered = true;

            //ver1.19 �t�H�[�J�X�𓖂ĂȂ��悤�ɂ���
            this.SetStyle(ControlStyles.Selectable, false);
            //DPI�X�P�[�����O�͖����ɂ���
            this.AutoScaleMode = AutoScaleMode.None;
            //�����x��1.0
            _alpha = 1.0F;

            //�������Z�o
            BOX_HEIGHT = PADDING + THUMBSIZE;
            //new���ꂽ���Ƃɍ�����K�v�Ƃ����̂ō�����������Ă����B
            this.Height = BOX_HEIGHT        //�摜����
                + PADDING;

            //Loading�ƕ\������C���[�W
            _dummyImage = BitmapUty.LoadingImage(THUMBSIZE * 2 / 3, THUMBSIZE);

            //�^�C�}�[�̏����ݒ�
            _timer = new FormTimer
            {
                Interval = 20
            };
            _timer.Tick += new EventHandler(Timer_Tick);

            //�I�t�Z�b�g��ݒ�
            _offset = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //�A�j���[�V���������Ȃ���߂Â���B
            int diff = (GetOffset(_selectedItem) - _offset);
            diff = diff * 2 / 7;
            _offset += diff;

            if (diff == 0)
            {
                _timer.Stop();
                CalcAllItemPos();
            }
            //�`��
            this.Refresh();
        }

        // public���\�b�h/�v���p�e�B ****************************************************/

        public void OpenPanel(Rectangle rect, int index)
        {
            this.Top = rect.Top;
            this.Left = rect.Left;
            this.Width = rect.Width;
            this.Height = BOX_HEIGHT + PADDING;

            _selectedItem = index;

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
            _offset = GetOffset(index);

            this.Visible = true;
            _alpha = 1.0F;
            this.Refresh();

            //timer���~�߂�
            _timer?.Stop();
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
            _alpha = 1.0F;

            //timer���~�߂�
            _timer?.Stop();

            //BitmapCache���폜
            App.BmpCache.Clear(CacheTag.NaviPanel);
        }

        public void SetCenterItem(int index)
        {
            _selectedItem = index;

            //ver1.37�o�O�Ώ�:��\���ł͂Ȃɂ����Ȃ�
            if (!Visible)
                return;

            if (_timer != null)
            {
                //�^�C�}�[�ŕ`��
                _timer.Enabled = true;
            }
            else
            {
                //�^�C�}�[���g�킸�ɍĕ`��
                _offset = GetOffset(index);
                this.Refresh();
            }
        }

        // �I�[�i�[�h���[ ***************************************************************/

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            //g.Clear(m_NormalBackColor);

            if (_alpha >= 1.0F)
            {
                //DrawItems(g);
                DrawItemAll(g);
            }
            else
            {
                //�����x������ꍇ�͈�x�ʂ�Bitmap�ɕ`�ʂ��ĕ`�ʂ���B
                using (var bmp = new Bitmap(this.Width, this.Height))
                {
                    Graphics.FromImage(bmp).Clear(Color.Transparent);
                    DrawItemAll(Graphics.FromImage(bmp));
                    BitmapUty.AlphaDrawImage(g, bmp, _alpha);
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
            g.FillRectangle(_BackBrush, this.DisplayRectangle);

            //�\�����ׂ��A�C�e�����Ȃ��ꍇ�͔w�i����
            if (_packageInfo == null || _packageInfo.Items.Count < 1)
                return;

            //���ׂẴA�C�e���̈ʒu���X�V
            //CalcAllItemPos();

            //�I�t�Z�b�g���v�Z
            int offset;
            if (_timer == null)
            {
                //�^�C�}�[�������Ă��Ȃ��Ƃ��͂������̏ꏊ��
                offset = GetOffset(_selectedItem);
            }
            else
            {
                //�^�C�}�[�������Ă���Ƃ���offset��Timer���X�V
                offset = _offset;
            }

            //�S�A�C�e���`��
            for (int item = 0; item < _packageInfo.Items.Count; item++)
            {
                //�킴�ƕ���ŕ`�ʂ���
                // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
#pragma warning disable CS4014
                DrawItemAsync(g, item, offset);
#pragma warning restore CS4014
            }
            return;
        }

        /// <summary>
        /// �T���l�C���̕\���ʒu�����ׂčX�V
        /// </summary>
        private void CalcAllItemPos()
        {
            if (_thumbnailPosList == null)
            {
                //�V�K�Ɉʒu���X�g���쐬
                _thumbnailPosList = new List<ItemPos>();
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    ItemPos item = new ItemPos();
                    item.pos.X = X;
                    item.pos.Y = PADDING;
                    if (_packageInfo.Items[i].Thumbnail != null)
                    {
                        item.size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                    }
                    else
                        item.size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    _thumbnailPosList.Add(item);
                    X += item.size.Width + PADDING;
                }
            }
            else
            {
                //���łɂ�����̂��X�V
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    //ItemPos item = thumbnailPos[i];
                    _thumbnailPosList[i].pos.X = X;
                    _thumbnailPosList[i].pos.Y = PADDING;
                    if (_packageInfo.Items[i].Thumbnail != null)
                        _thumbnailPosList[i].size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                    else
                        _thumbnailPosList[i].size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    X += _thumbnailPosList[i].size.Width + PADDING;
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
            int X = _thumbnailPosList[centerItem].pos.X;
            int center = X + _thumbnailPosList[centerItem].size.Width / 2;
            int offset = center - this.Width / 2;
            return offset;
        }

        /// <summary>
        /// 1�A�C�e����`�ʂ���
        /// </summary>
        /// <param name="g">�`�ʐ��Graphics</param>
        /// <param name="index">�`�ʃA�C�e���ԍ�</param>
        private async Task DrawItemAsync(Graphics g, int index, int offset)
        {
            if (g == null)
                throw new ArgumentNullException(nameof(g));
            App.g_pi.ThrowIfOutOfRange(index);

            //���v�Z��������v�Z
            if (_thumbnailPosList == null)
                CalcAllItemPos();

            //�`�ʈʒu������
            int x = _thumbnailPosList[index].pos.X - offset;

            //�`�ʑΏۊO���͂���
            if (x > this.Width)
                return;
            if (x + _thumbnailPosList[index].size.Width < 0)
                return;

            //�摜�擾
            Bitmap img = App.BmpCache.GetBitmap(index, CacheTag.NaviPanel);
            if (img == null)
            {
                img = BitmapUty.MakeHeightFixThumbnailImage(
                    _packageInfo.Items[index].Thumbnail,
                    THUMBSIZE);
                App.BmpCache.Add(index, CacheTag.NaviPanel, img);
            }

            if (img == null)
            {
                img = _dummyImage;

                //ver1.81 �ǂݍ��݃��[�`����PushLow()�ɕύX
                if (_timer == null || !_timer.Enabled)
                {
                    await Bmp.LoadBitmapAsync(index, false);
                    CalcAllItemPos();
                    if (this.Visible)
                        this.Invalidate();
                }
            }

            Rectangle cRect = new Rectangle(
                _thumbnailPosList[index].pos.X - offset,
                _thumbnailPosList[index].pos.Y + BOX_HEIGHT - img.Height - PADDING,      //������
                _thumbnailPosList[index].size.Width,
                _thumbnailPosList[index].size.Height);

            //�`��
            if (index == _selectedItem)
            {
                //�����̃A�C�e��
                //ver1.17�ǉ� �t�H�[�J�X�g
                BitmapUty.DrawBlurEffect(g, cRect, Color.LightBlue);
                //������`��
                g.DrawImage(img, cRect);
            }
            else
            {
                //�����ȊO�̉摜��`��
                g.DrawImage(
                    BitmapUty.BitmapToDark(img, DARKPERCENT),
                    cRect);
            }

            //�摜�ԍ����摜��ɕ\��
            g.DrawString(
                $"{index + 1}",
                fontL,
                Brushes.LightGray,
                cRect,
                sfCenterDown);
        }
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
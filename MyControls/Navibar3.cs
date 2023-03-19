/********************************************************************************
NaviBar3

�g���b�N�o�[�ƘA�����ăT���l�C���\������p�l���B
������ς����ق���������������Ȃ�
�^�C�}�[���g���ăA�j���[�V���������Ȃ���`�ʂ��Ă���B
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormTimer = System.Windows.Forms.Timer;

namespace Marmi
{
    public class NaviBar3 : UserControl
    {
        private const int THUMBSIZE = 200;  //�T���l�C���T�C�Y
        private const int PADDING = 2;      //�e��]��
        private readonly int BOX_HEIGHT;    //BOX�T�C�Y�F�R���X�g���N�^�Ōv�Z
        public int _selectedItem;           //�I������Ă���A�C�e��
        private int _offset;                //���݂̕`�ʈʒu.�s�N�Z����

        private readonly PackageInfo _packageInfo;        //g_pi���̂��̂�}��

        //�w�i�F
        private readonly SolidBrush _BackBrush = new SolidBrush(Color.FromArgb(192, 48, 48, 48));

        //�t�H���g,�t�H�[�}�b�g
        private readonly Font fontL = new Font("Century Gothic", 16F);

        private readonly StringFormat sfCenterDown = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        private List<ItemPos> _posList = null; //�T���l�C���̈ʒu

        //�X�N���[���p�A�j���[�V�����^�C�}�[:
        private readonly FormTimer _timer = null;

        //�Â߂̉摜�p��ImageAttribute.DrawItem()�ŗ��p
        private ImageAttributes _darkAttribute = new ImageAttributes();

        //�I���摜����������Rect�̃y��
        private readonly Pen _borderPen = new Pen(Color.Pink, 2);

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

            //�������Z�o
            BOX_HEIGHT = PADDING + THUMBSIZE;

            //new���ꂽ���Ƃɍ�����K�v�Ƃ����̂ō�����������Ă����B
            this.Height = BOX_HEIGHT + PADDING;

            //Loading�ƕ\������C���[�W
            //_dummyImage = BitmapUty.LoadingImage(THUMBSIZE * 2 / 3, THUMBSIZE);

            //�^�C�}�[�̏����ݒ�
            _timer = new FormTimer
            {
                Interval = 20
            };
            _timer.Tick += new EventHandler(Timer_Tick);

            //�I�t�Z�b�g��ݒ�
            _offset = 0;

            //���邳������ImageAttribute��������
            var cm = new ColorMatrix
            {
                Matrix00 = 0.5f,    // R
                Matrix11 = 0.5f,    // G
                Matrix22 = 0.5f,    // B
                Matrix33 = 1f,  // Alpha
                Matrix44 = 1f
            };
            _darkAttribute.SetColorMatrix(cm);
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

            //�A�C�e���ʒu���v�Z
            CalcAllItemPos();

            //�����ʒu������
            _offset = GetOffset(index);

            this.Visible = true;
            this.Refresh();

            //timer���~�߂�
            _timer?.Stop();
        }

        public void ClosePanel()
        {
            //�T���l�C���쐬���Ȃ��x�~�߂�
            //Form1.PauseThumbnailMakerThread();

            this.Visible = false;

            //timer���~�߂�
            _timer?.Stop();

            //BitmapCache���폜
            //App.BmpCache.Clear(CacheTag.NaviPanel);
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
            DrawItemAll(g);
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
            //�^�C�}�[�������Ă��Ȃ��Ƃ���GetOffset()�ł������̏ꏊ��
            //�^�C�}�[���쒆��_offset
            int offset = (_timer == null) ? GetOffset(_selectedItem) : _offset;

            //�S�A�C�e���`��
            for (int item = 0; item < _packageInfo.Items.Count; item++)
            {
                //�킴�ƕ���ŕ`�ʂ���
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
            if (_posList == null)
            {
                //�V�K�Ɉʒu���X�g���쐬
                _posList = new List<ItemPos>();
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    ItemPos item = new ItemPos();
                    item.pos.X = X;
                    item.pos.Y = PADDING;
                    if (_packageInfo.Items[i].Thumbnail != null)
                    {
                        item.size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                        item.Fixed = true;
                    }
                    else
                    {
                        item.size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    }

                    _posList.Add(item);
                    X += item.size.Width + PADDING;
                }
            }
            else
            {
                //���łɂ�����̂��X�V
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    _posList[i].pos.X = X;
                    _posList[i].pos.Y = PADDING;

                    if (_posList[i].Fixed == false)
                    {
                        if (_packageInfo.Items[i].Thumbnail != null)
                        {
                            _posList[i].size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                            _posList[i].Fixed = true;
                        }
                        else
                            _posList[i].size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    }
                    X += _posList[i].size.Width + PADDING;
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
            int X = _posList[centerItem].pos.X;
            int center = X + _posList[centerItem].size.Width / 2;
            int offset = center - this.Width / 2;
            return offset;
        }

        private bool IsDrawItem(int index, int offset)
        {
            if (_posList == null)
                return false;

            var item = _posList[index];
            int x = item.pos.X - offset;
            return (x > this.Width || x + item.size.Width < 0);
        }

        /// <summary>
        /// 1�A�C�e����`�ʂ���
        /// </summary>
        /// <param name="g">�`�ʐ��Graphics</param>
        /// <param name="index">�`�ʃA�C�e���ԍ�</param>
        private async Task DrawItemAsync(Graphics g, int index, int offset)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            App.g_pi.ThrowIfOutOfRange(index);

            //���v�Z���������ɑS�A�C�e���ʒu���v�Z
            //�Ăяo�����ŕK�����s���Ă���̂ŃR�����g�A�E�g
            //if (_thumbnailPosList == null)
            //    CalcAllItemPos();

            //�Ώۃf�[�^
            var item = _posList[index];

            //�`��X�ʒu
            int x = item.X - offset;

            //�`�ʑΏۊO���͂���
            if (x > this.Width || x + item.Width < 0)
                return;

            var cRect = new Rectangle(
                item.X - offset,
                item.Y + BOX_HEIGHT - item.Height - PADDING,      //������
                item.Width,
                item.Height);

            //�摜�擾
            var img = _packageInfo.Items[index].Thumbnail;

            if (img == null)
            {
                //�ǂݍ���
                //_ = await Bmp.LoadBitmapAsync(index, false);
                //CalcAllItemPos();

                ////ver1.81 �ǂݍ��݃��[�`����PushLow()�ɕύX
                //if (_timer == null || !_timer.Enabled)
                //{
                //    await Bmp.LoadBitmapAsync(index, false);
                //    CalcAllItemPos();
                //    if (this.Visible)
                //        this.Invalidate();
                //    return;
                //}
            }
            else
            {
                //�`��
                if (index == _selectedItem)
                {
                    //�����̃A�C�e��
                    g.DrawImage(img, cRect);
                }
                else
                {
                    //�����ȊO�̉摜�F�Â߂ɕ`��
                    g.DrawImage(img, cRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, _darkAttribute);
                }
            }

            //�g�`��
            var penColor = (index == _selectedItem) ? _borderPen : Pens.Gray;
            g.DrawRectangle(penColor, cRect);

            //�摜�ԍ����摜��ɕ\��
            g.DrawString($"{index + 1}", fontL, Brushes.LightGray, cRect, sfCenterDown);
        }
    }

    /// <summary>
    /// �`�ʈʒu��ۑ����邽�߂̍\���́i�N���X�j
    /// </summary>
    public class ItemPos
    {
        public Point pos;
        public Size size;
        public bool Fixed = false;
        public int X => pos.X;
        public int Y => pos.Y;
        public int Width => size.Width;
        public int Height => size.Height;
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormSaveThumbnail : Form
    {
        private const int PADDING = 10;         //�T���l�C���̗]���B2014�N3��23���ύX�B�Ԋu��������

        //private readonly bool saveConf_isDrawFilename;      //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
        //private readonly bool saveConf_isDrawFileSize;      //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
        //private readonly bool saveConf_isDrawPicSize;       //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�

        private bool _isDrawFilename = false;
        private bool _isDrawFileSize = false;
        private bool _isDrawPicSize = false;

        private string saveFilename;                        //�ۑ��t�@�C����
        private readonly List<ImageInfo> _imageList;        //���X�g�ւ̃|�C���^
        private bool m_Saving = false;                      //�ۑ����������ǂ�����\���t���O

        public bool IsCancel => !m_Saving;

        public FormSaveThumbnail(List<ImageInfo> imageList, string Filename)
        {
            InitializeComponent();
            _imageList = imageList;
            //_tpanel.SavedItemChanged += ThumbPanel_SavedItemChanged;

            //�ۑ��t�@�C����
            saveFilename = SuggestFilename(Filename);

            //�O���[�o���R���t�B�O���ꎞ�ۑ�
            //saveConf_isDrawFilename = App.Config.Thumbnail.DrawFilename;
            //saveConf_isDrawFileSize = App.Config.Thumbnail.DrawFilesize;
            //saveConf_isDrawPicSize = App.Config.Thumbnail.DrawPicsize;
        }

        ~FormSaveThumbnail()
        {
            //_tpanel.SavedItemChanged -= ThumbPanel_SavedItemChanged;
        }

        private void FormSaveThumbnail_Load(object sender, EventArgs e)
        {
            //�v���O���X�o�[�̏�����
            tsProgressBar1.Visible = false;

            //�`�F�b�N�{�b�N�X�̏�����
            isDrawFileName.Checked = true;
            isDrawFileSize.Checked = false;
            isDrawPicSize.Checked = false;
        }

        private void FormSaveThumbnail_FormClosed(object sender, FormClosedEventArgs e)
        {
            //�O���[�o���R���t�B�O�����ɖ߂�
            //App.Config.Thumbnail.DrawFilename = saveConf_isDrawFilename;
            //App.Config.Thumbnail.DrawFilesize = saveConf_isDrawFileSize;
            //App.Config.Thumbnail.DrawPicsize = saveConf_isDrawPicSize;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (m_Saving)
            {
                m_Saving = false;
                btExcute.Enabled = true;
                thumbPixels.Enabled = true;
                itemNumsX.Enabled = true;
            }
            else
            {
                this.Close();
            }
        }

        private async void BtnExcute_Click(object sender, EventArgs e)
        {
            //�t�@�C�����̊m�F
            var sf = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = saveFilename,
                InitialDirectory = Path.GetDirectoryName(saveFilename),
                Filter = "png�t�@�C��|*.png|�S�Ẵt�@�C��|*.*",
                FilterIndex = 1,
                OverwritePrompt = true
            };
            if (sf.ShowDialog() == DialogResult.OK)
                saveFilename = sf.FileName;
            else
                return; //�L�����Z������

            tbInfo.Text += "�ۑ��� : " + saveFilename + "\r\n"
                        + "�A�C�e���� : " + _imageList.Count + "\r\n";

            tsProgressBar1.Minimum = 0;
            tsProgressBar1.Maximum = _imageList.Count - 1;  //0�n�܂�
            tsProgressBar1.Value = 0;
            tsProgressBar1.Visible = true;

            _isDrawFilename = isDrawFileName.Checked;
            _isDrawFileSize = isDrawFileSize.Checked;
            _isDrawPicSize = isDrawPicSize.Checked;

            //�T���l�C����ۑ�����
            btExcute.Enabled = false;
            thumbPixels.Enabled = false;
            itemNumsX.Enabled = false;
            m_Saving = true;
            await SaveBitmapAsync((int)thumbPixels.Value, (int)itemNumsX.Value, saveFilename);
            this.Close();
        }

        private static string SuggestFilename(string orgName)
        {
            //�w��Ȃ��Ƃ��̓f�X�N�g�b�v/thumbnaul.png����
            if (string.IsNullOrEmpty(orgName))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "Thumbnail.png");
            }

            //�g���q��؂�o��
            string suggest = Path.Combine(
                Path.GetDirectoryName(orgName),
                Path.GetFileNameWithoutExtension(orgName));
            suggest += ".png";
            return suggest;
        }

        private void UpdateUI(int itemNumber)
        {
            int num = itemNumber;
            toolStripStatusLabel1.Text = $"������ : {num + 1} / {_imageList.Count}";
            if (tsProgressBar1.Visible)
                tsProgressBar1.Value = num;

            if (num + 1 >= _imageList.Count)
            {
                //btExcute.Enabled = true;
                //tbPixels.Enabled = true;
                //tbVnum.Enabled = true;
                toolStripStatusLabel1.Text = "�������܂���";
            }
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            var tpixels = (int)thumbPixels.Value;

            //���ɕ��Ԍ��̐ݒ�
            var nItemsX = (int)itemNumsX.Value;
            if (nItemsX == 0) nItemsX = 1;

            //Bitmap�̑z��T�C�Y���v�Z
            int ItemCount = _imageList.Count;
            int nItemsY = ItemCount / nItemsX;  //�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
            if (ItemCount % nItemsX > 0)        //����؂�Ȃ������ꍇ��1�s�ǉ�
                nItemsY++;

            tbInfo.Text = $"�o�͉摜�T�C�Y : {nItemsX * tpixels:N0} x {nItemsY * tpixels:N0} [pixels]\r\n";
        }

        #region �T���l�C���̃t�@�C���ۑ�

        /// <summary>
        /// �T���l�C���摜�ꗗ���쐬�A�ۑ�����B
        /// ���̊֐��̒��ŕۑ�Bitmap�𐶐����A�����png�`���ŕۑ�����
        /// </summary>
        /// <param name="thumbSize">�T���l�C���摜�̃T�C�Y�B�����`�̈�ӂ̒���</param>
        /// <param name="numX">�T���l�C���̉������̉摜��</param>
        /// <param name="saveFilename">�ۑ�����t�@�C����</param>
        /// <returns>����true�A�ۑ����Ȃ������ꍇ��false</returns>
        public async Task<bool> SaveBitmapAsync(int thumbSize, int numX, string saveFilename)
        {
            //�������ς݂��m�F
            if (_imageList == null || _imageList.Count == 0)
                return false;

            if (thumbSize < 10 || numX < 1)
                return false;

            //�T���l�C���T�C�Y��ݒ�.�Čv�Z
            var tboxSize = CalcTboxSize(thumbSize);

            //Bitmap�T�C�Y���v�Z
            var numY = ((_imageList.Count - 1) / numX) + 1;
            Size bmpSize = new Size(tboxSize.Width * numX, tboxSize.Height * numY); ;

            //Bitmap����
            var saveBmp = new Bitmap(bmpSize.Width, bmpSize.Height);
            //var dummyBmp = new Bitmap(tboxSize.Width, tboxSize.Height);

            using (var g = Graphics.FromImage(saveBmp))
            {
                //�Ώۋ�`��w�i�F�œh��Ԃ�.
                g.Clear(BackColor);

                int x = -1;
                int y = 0;
                for (int ix = 0; ix < _imageList.Count; ix++)
                {
                    if (++x >= numX)
                    {
                        x = 0;
                        y++;
                    }

                    //using (var dummyg = Graphics.FromImage(dummyBmp))
                    //{
                    //    dummyg.Clear(Color.White);

                    Rectangle tboxRect = new Rectangle(
                        x * tboxSize.Width,
                        y * tboxSize.Height,
                        tboxSize.Width,
                        tboxSize.Height);

                    //���i���摜��`��
                    await DrawItemHQ2Async(g, ix, thumbSize, tboxRect.X, tboxRect.Y);

                    //�摜���𕶎��`�ʂ���
                    DrawTextInfo(g, ix, tboxRect, thumbSize);
                    //}

                    UpdateUI(ix);

                    //ver1.31 null�`�F�b�N
                    Application.DoEvents();

                    //�L�����Z������
                    if (IsCancel)
                        return false;
                }
            }

            saveBmp.Save(saveFilename);
            saveBmp.Dispose();
            return true;
        }

        /// <summary>
        /// ���i���`��DrawItem.
        /// ���摜������B�T���l�C���ꗗ�ۑ��p�ɗ��p�B
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="itemNum">�A�C�e���ԍ�</param>
        /// <param name="thumbSize">�T���l�C���̃T�C�Y�B��Ђ̒���</param>
        /// <param name="px">�`�ʈʒuX</param>
        /// <param name="py">�`�ʈʒuY</param>
        /// <returns></returns>
        public static async Task DrawItemHQ2Async(Graphics g, int itemNum, int thumbSize, int px, int py)
        {
            //�`�ʕi��:�ō�
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var drawBitmap = await Bmp.GetBitmapAsync(itemNum, false);
            if (drawBitmap == null)
            {
                //�T���l�C���͏����ł��Ă��Ȃ�
                return;
            }

            bool drawFrame = true;      //�t���O:�g����`�ʂ��邩
            int w = drawBitmap.Width;   //�摜�̕�
            int h = drawBitmap.Height;  //�摜�̍���

            //�k�����K�v�ȏꍇ�̓T�C�Y�ύX
            if (w > thumbSize || h > thumbSize)
            {
                float ratio = (w > h) ?
                    (float)thumbSize / (float)w :
                    (float)thumbSize / (float)h;
                w = (int)(w * ratio);
                h = (int)(h * ratio);
            }

            int sx = px + (thumbSize + PADDING * 2 - w) / 2; //�摜�`��X�ʒu
            int sy = py + thumbSize + PADDING - h;           //�摜�`��Y�ʒu�F������

            var imageRect = new Rectangle(sx, sy, w, h);

            //�e��`��
            //if (App.Config.Thumbnail.DrawShadowdrop && drawFrame)
            //{
            //    Rectangle frameRect = imageRect;
            //    BitmapUty.DrawDropShadow(g, frameRect);
            //}

            //�摜������
            //�t�H�[�J�X�̂Ȃ��摜��`��
            g.FillRectangle(Brushes.White, imageRect);
            g.DrawImage(drawBitmap, imageRect);

            //�O�g
            if (drawFrame)
            {
                g.DrawRectangle(Pens.LightGray, imageRect);
            }
        }

        /// <summary>
        /// TBOX�T�C�Y���v�Z����B
        /// �P����PADDING���ƕ����񕪂𑫂������́B
        /// </summary>
        public Size CalcTboxSize(int thumbnailSize)
        {
            var fontHeight = App.Font9_Height;

            //TBOX�T�C�Y���m��
            var w = thumbnailSize + (PADDING * 2);
            var h = thumbnailSize + (PADDING * 2);

            //�����񕔒ǉ�
            if (_isDrawFilename)
                h += PADDING + fontHeight;

            if (_isDrawFileSize)
                h += PADDING + fontHeight;

            if (_isDrawPicSize)
                h += PADDING + fontHeight;

            return new Size(w, h);
        }

        private void DrawTextInfo(Graphics g, int item, Rectangle tboxRect, int thumbSize)
        {
            var font = App.Font9;
            var fontColor = Brushes.Black;
            var fontHeight = App.Font9_Height;

            //�e�L�X�g�`�ʈʒu��␳
            Rectangle textRect = tboxRect;
            textRect.X += PADDING;                              //���ɗ]����ǉ�
            textRect.Y += PADDING + thumbSize + PADDING;   //�㉺�ɗ]����ǉ�
            textRect.Width = thumbSize;                    //�����̓T���l�C���T�C�Y�Ɠ���
            textRect.Height = fontHeight;

            //�e�L�X�g�`�ʗp�̏����t�H�[�}�b�g
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,          //��������
                Trimming = StringTrimming.EllipsisPath      //���Ԃ̏ȗ�
            };

            //�t�@�C����������
            if (_isDrawFilename)
            {
                string filename = Path.GetFileName(_imageList[item].Filename);
                if (filename != null)
                {
                    g.DrawString(filename, font, fontColor, textRect, sf);
                    textRect.Y += fontHeight;
                }
            }

            //�t�@�C���T�C�Y������
            if (_isDrawFileSize)
            {
                string s = $"{_imageList[item].FileLength:#,0} bytes";
                g.DrawString(s, font, fontColor, textRect, sf);
                textRect.Y += fontHeight;
            }

            //�摜�T�C�Y������
            if (_isDrawPicSize)
            {
                string s = $"{_imageList[item].Width:#,0}x{_imageList[item].Height:#,0} px";
                g.DrawString(s, font, fontColor, textRect, sf);
                textRect.Y += fontHeight;
            }
        }

        #endregion �T���l�C���̃t�@�C���ۑ�

        private void NumUpdown_ValueChanged(object sender, EventArgs e)
        {
            Textbox_TextChanged(sender, e);
        }
    }
}
using Marmi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormSaveThumbnail : Form
    {
        //private readonly bool saveConf_isDrawFilename;      //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
        //private readonly bool saveConf_isDrawFileSize;      //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
        //private readonly bool saveConf_isDrawPicSize;       //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�

        private string saveFilename;                        //�ۑ��t�@�C����
        private readonly List<ImageInfo> _imageList;        //���X�g�ւ̃|�C���^

        private bool m_Saving = false;                      //�ۑ����������ǂ�����\���t���O

        private CancellationTokenSource _cts = null;

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
                _cts?.Cancel();
            }
            else
            {
                this.Close();
            }
        }

        private async void BtnExcute_Click(object sender, EventArgs e)
        {
            //�ۑ��_�C�A���O
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

            if (sf.ShowDialog() != DialogResult.OK)
                return;

            saveFilename = sf.FileName;

            //�t�H�[�����i�̐ݒ�
            tbInfo.Text += $"�ۑ��� : {saveFilename}\r\n�A�C�e���� : {_imageList.Count}\r\n";
            tsProgressBar1.Minimum = 0;
            tsProgressBar1.Maximum = _imageList.Count - 1;  //0�n�܂�
            tsProgressBar1.Value = 0;
            tsProgressBar1.Visible = true;
            btExcute.Enabled = false;
            thumbPixels.Enabled = false;
            itemNumsX.Enabled = false;
            m_Saving = true;

            _cts = new CancellationTokenSource();
            var progress = new Progress<int>(UpdateUI);

            //�T���l�C���𐶐��A�ۑ�
            var bmpMaker = new ThumbnailPictureMaker(
                            _imageList,
                            isDrawFileName.Checked,
                            isDrawFileSize.Checked,
                            isDrawPicSize.Checked);
            await bmpMaker.SaveBitmapAsync((int)thumbPixels.Value, (int)itemNumsX.Value, saveFilename, _cts.Token, progress);

            //�ۑ����������炱�̃t�H�[�������
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

        private void NumUpdown_ValueChanged(object sender, EventArgs e)
        {
            Textbox_TextChanged(sender, e);
        }
    }
}
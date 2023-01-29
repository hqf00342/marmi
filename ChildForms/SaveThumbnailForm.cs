using Marmi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class SaveThumbnailForm : Form
    {
        private string saveFilename;
        private readonly IReadOnlyList<ImageInfo> _imageList;

        //�ۑ����������ǂ�����\���t���O
        private bool _processing = false;

        private CancellationTokenSource _cts = null;

        public SaveThumbnailForm(List<ImageInfo> imageList, string Filename)
        {
            InitializeComponent();
            _imageList = imageList;
            saveFilename = SuggestFilename(Filename);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_processing)
            {
                //�쐬���Ȃ�L�����Z��
                _processing = false;
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

            _processing = true;

            _cts = new CancellationTokenSource();
            var progress = new Progress<int>(UpdateUI);

            //�T���l�C���𐶐��A�ۑ�
            var bmpMaker = new ThumbnailPictureMaker(
                            _imageList,
                            isDrawFileName.Checked,
                            isDrawFileSize.Checked,
                            isDrawPicSize.Checked);
            var ret = await bmpMaker.SaveBitmapAsync((int)thumbPixels.Value, (int)itemNumsX.Value, saveFilename, _cts.Token, progress);

            if (ret)
            {
                //���튮���B���̃t�H�[�������
                this.Close();
            }
            else
            {
                //�L�����Z��
                _processing = false;
                btExcute.Enabled = true;
                thumbPixels.Enabled = true;
                itemNumsX.Enabled = true;
            }
        }

        private static string SuggestFilename(string orgName)
        {
            //�w��Ȃ��Ƃ��̓f�X�N�g�b�v/thumbnail.png����
            if (string.IsNullOrEmpty(orgName))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "thumbnail.png");
            }

            //�g���q��؂�o��
            string suggest = Path.ChangeExtension(orgName, ".png");
            return suggest;
        }

        private void UpdateUI(int itemNumber)
        {
            int num = itemNumber;
            toolStripStatusLabel1.Text = $"������ : {num + 1} / {_imageList.Count}";
            if (tsProgressBar1.Visible)
                tsProgressBar1.Value = num;
        }

        private void NumUpdown_ValueChanged(object sender, EventArgs e)
        {
            var tpixels = (int)thumbPixels.Value;

            //XY������
            var nItemsX = (int)itemNumsX.Value;
            int nItemsY = (int)Math.Ceiling(_imageList.Count / (double)nItemsX);

            tbInfo.Text = $"�o�͉摜�T�C�Y : {nItemsX * tpixels:N0} x {nItemsY * tpixels:N0} [pixels]\r\n";
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormSaveThumbnail : Form
    {
        //const int DEFAULT_THUMBNAIL_SIZE = 120; //App.DEFAULT_THUMBNAIL_SIZE�ɓ���;
        private const int DEFAULT_VERTICAL_WIDTH = 640;

        private readonly bool saveConf_isDrawFilename;       //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
        private readonly bool saveConf_isDrawFileSize;       //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
        private readonly bool saveConf_isDrawPicSize;        //�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�

        private string savename;                            //�ۑ��t�@�C����
        private readonly List<ImageInfo> m_thumbnailSet;     //���X�g�ւ̃|�C���^
        private readonly ThumbnailPanel m_tPanel = null;     //�e�̃p�l����\��
        private bool m_Saving = false;              //�ۑ����������ǂ�����\���t���O

        public bool IsCancel => !m_Saving;

        public FormSaveThumbnail(ThumbnailPanel tp, List<ImageInfo> lii, string Filename)
        {
            InitializeComponent();
            m_thumbnailSet = lii;
            m_tPanel = tp;
            m_tPanel.SavedItemChanged += ThumbPanel_SavedItemChanged;

            //�ۑ��t�@�C����
            savename = SuggestFilename(Filename);

            //�O���[�o���R���t�B�O���ꎞ�ۑ�
            saveConf_isDrawFilename = App.Config.IsShowTPFileName;
            saveConf_isDrawFileSize = App.Config.IsShowTPFileSize;
            saveConf_isDrawPicSize = App.Config.IsShowTPPicSize;
        }

        ~FormSaveThumbnail()
        {
            m_tPanel.SavedItemChanged -= ThumbPanel_SavedItemChanged;
        }

        private void FormSaveThumbnail_Load(object sender, EventArgs e)
        {
            //�e�L�X�g�{�b�N�X�̏������F�摜�T�C�Y�A�摜��
            tbPixels.Text = App.DEFAULT_THUMBNAIL_SIZE.ToString();
            int vertical = DEFAULT_VERTICAL_WIDTH / App.DEFAULT_THUMBNAIL_SIZE;
            tbnItemX.Text = vertical.ToString();

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
            App.Config.IsShowTPFileName = saveConf_isDrawFilename;
            App.Config.IsShowTPFileSize = saveConf_isDrawFileSize;
            App.Config.IsShowTPPicSize = saveConf_isDrawPicSize;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (m_Saving)
            {
                m_Saving = false;
                btExcute.Enabled = true;
                tbPixels.Enabled = true;
                tbnItemX.Enabled = true;
            }
            else
            {
                this.Close();
            }
        }

        private async void BtnExcute_Click(object sender, EventArgs e)
        {
            //�T���l�C���T�C�Y�̐ݒ�
            if (!Int32.TryParse(tbPixels.Text, out int ThumbnailSize))
                ThumbnailSize = App.DEFAULT_THUMBNAIL_SIZE;
            tbPixels.Text = ThumbnailSize.ToString();

            //���ɕ��Ԍ��̐ݒ�
            if (!Int32.TryParse(tbnItemX.Text, out int nItemX))
                nItemX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
            tbnItemX.Text = nItemX.ToString();

            //�t�@�C�����̊m�F
            var sf = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = savename,
                InitialDirectory = Path.GetDirectoryName(savename),
                Filter = "png�t�@�C��|*.png|�S�Ẵt�@�C��|*.*",
                FilterIndex = 1,
                OverwritePrompt = true
            };
            if (sf.ShowDialog() == DialogResult.OK)
                savename = sf.FileName;
            else
                return; //�L�����Z������

            tbInfo.Text += "�ۑ��� : " + savename + "\r\n"
                        + "�A�C�e���� : " + m_thumbnailSet.Count + "\r\n";

            tsProgressBar1.Minimum = 0;
            tsProgressBar1.Maximum = m_thumbnailSet.Count - 1;  //0�n�܂�
            tsProgressBar1.Value = 0;
            tsProgressBar1.Visible = true;

            //�O���[�o���R���t�B�O���ꎞ�I�ɕύX
            //FormClosed()�Ō��ɖ߂�
            App.Config.IsShowTPFileName = isDrawFileName.Checked;
            App.Config.IsShowTPFileSize = isDrawFileSize.Checked;
            App.Config.IsShowTPPicSize = isDrawPicSize.Checked;

            //�T���l�C����ۑ�����
            btExcute.Enabled = false;
            tbPixels.Enabled = false;
            tbnItemX.Enabled = false;
            m_Saving = true;
            await m_tPanel.SaveThumbnailImageAsync(ThumbnailSize, nItemX, savename);
            this.Close();
        }

        private string SuggestFilename(string orgName)
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
                Path.GetDirectoryName(orgName), Path.GetFileNameWithoutExtension(orgName));
            suggest += ".png";
            return suggest;
        }

        private void ThumbPanel_SavedItemChanged(object obj, ThumbnailEventArgs e)
        {
            int num = e.HoverItemNumber;
            toolStripStatusLabel1.Text = string.Format("������ : {0} / {1}", num + 1, m_thumbnailSet.Count);
            if (tsProgressBar1.Visible)
                tsProgressBar1.Value = num;

            if (num + 1 >= m_thumbnailSet.Count)
            {
                //btExcute.Enabled = true;
                //tbPixels.Enabled = true;
                //tbVnum.Enabled = true;
                toolStripStatusLabel1.Text = "�������܂���";
            }
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            //�T���l�C���T�C�Y�̐ݒ�
            if (!Int32.TryParse(tbPixels.Text, out int ThumbnailSize))
                ThumbnailSize = App.DEFAULT_THUMBNAIL_SIZE;
            tbPixels.Text = ThumbnailSize.ToString();

            //���ɕ��Ԍ��̐ݒ�
            if (!Int32.TryParse(tbnItemX.Text, out int nItemsX))
                nItemsX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
            tbnItemX.Text = nItemsX.ToString();

            //Bitmap�̑z��T�C�Y���v�Z
            int ItemCount = m_thumbnailSet.Count;
            int nItemsY = ItemCount / nItemsX;  //�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
            if (ItemCount % nItemsX > 0)        //����؂�Ȃ������ꍇ��1�s�ǉ�
                nItemsY++;

            tbInfo.Text = string.Format("�o�͉摜�T�C�Y : {0:N0} x {1:N0} [pixels]\r\n",
                nItemsX * ThumbnailSize, nItemsY * ThumbnailSize);
        }
    }
}
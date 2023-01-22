using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace Marmi
{
	public partial class FormSaveThumbnail : Form
	{
		//const int DEFAULT_THUMBNAIL_SIZE = 120; //Form1.DEFAULT_THUMBNAIL_SIZE�ɓ���;
		const int DEFAULT_VERTICAL_WIDTH = 640;

		private bool saveConf_isDrawFilename;		//�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
		private bool saveConf_isDrawFileSize;		//�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�
		private bool saveConf_isDrawPicSize;		//�O���[�o���R���t�B�O�̈ꎞ�Ҕ�̈�

		string savename;							//�ۑ��t�@�C����
		private List<ImageInfo> m_thumbnailSet;		//���X�g�ւ̃|�C���^
		private ThumbnailPanel m_tPanel = null;		//�e�̃p�l����\��
		private bool m_Saving = false;				//�ۑ����������ǂ�����\���t���O

		public bool isCancel
		{
			get { return !m_Saving; }
		}


		public FormSaveThumbnail(ThumbnailPanel tp, List<ImageInfo> lii, string Filename)
		{
			InitializeComponent();
			m_thumbnailSet = lii;
			m_tPanel = tp;
			m_tPanel.SavedItemChanged += new ThumbnailPanel.ThumbnailEventHandler(tPanel_SavedItemChanged);

			//�ۑ��t�@�C����
			savename = suggestFilename(Filename);

			//�O���[�o���R���t�B�O���ꎞ�ۑ�
			saveConf_isDrawFilename = Form1.g_Config.isShowTPFileName;
			saveConf_isDrawFileSize = Form1.g_Config.isShowTPFileSize;
			saveConf_isDrawPicSize = Form1.g_Config.isShowTPPicSize;
		}


		~FormSaveThumbnail()
		{
			m_tPanel.SavedItemChanged -= new ThumbnailPanel.ThumbnailEventHandler(tPanel_SavedItemChanged);
		}

		private void FormSaveThumbnail_Load(object sender, EventArgs e)
		{
			//�e�L�X�g�{�b�N�X�̏������F�摜�T�C�Y�A�摜��
			tbPixels.Text = Form1.DEFAULT_THUMBNAIL_SIZE.ToString();
			int vertical = DEFAULT_VERTICAL_WIDTH / Form1.DEFAULT_THUMBNAIL_SIZE;
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
			Form1.g_Config.isShowTPFileName = saveConf_isDrawFilename;
			Form1.g_Config.isShowTPFileSize = saveConf_isDrawFileSize;
			Form1.g_Config.isShowTPPicSize = saveConf_isDrawPicSize;

		}

		private void btCancel_Click(object sender, EventArgs e)
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


		private void btExcute_Click(object sender, EventArgs e)
		{
			//�T���l�C���T�C�Y�̐ݒ�
			int ThumbnailSize = 0;
			if (!Int32.TryParse(tbPixels.Text, out ThumbnailSize))
				ThumbnailSize = Form1.DEFAULT_THUMBNAIL_SIZE;
			tbPixels.Text = ThumbnailSize.ToString();

			//���ɕ��Ԍ��̐ݒ�
			int nItemX = 0;
			if(!Int32.TryParse(tbnItemX.Text, out nItemX))
				nItemX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
			tbnItemX.Text = nItemX.ToString();

			//�t�@�C�����̊m�F
			SaveFileDialog sf = new SaveFileDialog();
			sf.AddExtension = true;
			sf.DefaultExt = "png";
			sf.FileName = savename;
			sf.InitialDirectory = Path.GetDirectoryName(savename);
			sf.Filter = "png�t�@�C��|*.png|�S�Ẵt�@�C��|*.*";
			sf.FilterIndex = 1;
			sf.OverwritePrompt = true;
			if (sf.ShowDialog() == DialogResult.OK)
				savename = sf.FileName;
			else
				return;	//�L�����Z������


			tbInfo.Text += "�ۑ��� : " + savename + "\r\n"
						+"�A�C�e���� : " + m_thumbnailSet.Count + "\r\n";

			tsProgressBar1.Minimum = 0;
			tsProgressBar1.Maximum = m_thumbnailSet.Count -1;	//0�n�܂�
			tsProgressBar1.Value = 0;
			tsProgressBar1.Visible = true;

			//�O���[�o���R���t�B�O���ꎞ�I�ɕύX
			//FormClosed()�Ō��ɖ߂�
			Form1.g_Config.isShowTPFileName = isDrawFileName.Checked;
			Form1.g_Config.isShowTPFileSize = isDrawFileSize.Checked;
			Form1.g_Config.isShowTPPicSize = isDrawPicSize.Checked;

			//�T���l�C����ۑ�����
			btExcute.Enabled = false;
			tbPixels.Enabled = false;
			tbnItemX.Enabled = false;
			m_Saving = true;
			m_tPanel.SaveThumbnailImage(ThumbnailSize, nItemX, savename);
			this.Close();
		}


		private string suggestFilename(string orgName)
		{
			//�g���qpng���Ă���B

			//�����Ȃ��Ƃ��̓f�X�N�g�b�v/thumbnaul.png����
			if (string.IsNullOrEmpty(orgName))
			{
				string sz = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				sz += @"\Thumbnail.png";
				return sz;
			}

			//�g���q��؂�o��
			string suggest = Path.Combine(
				Path.GetDirectoryName(orgName), Path.GetFileNameWithoutExtension(orgName));
			suggest += ".png";
			return suggest;
		}


		void tPanel_SavedItemChanged(object obj, ThumbnailEventArgs e)
		{
			int num = e.HoverItemNumber;
			toolStripStatusLabel1.Text = string.Format("������ : {0} / {1}", num+1, m_thumbnailSet.Count);
			if(tsProgressBar1.Visible)
				tsProgressBar1.Value = num;

			if (num+1 >= m_thumbnailSet.Count)
			{
				//btExcute.Enabled = true;
				//tbPixels.Enabled = true;
				//tbVnum.Enabled = true;
				toolStripStatusLabel1.Text = "�������܂���";
			}
		}


		private void textbox_TextChanged(object sender, EventArgs e)
		{
			//�T���l�C���T�C�Y�̐ݒ�
			int ThumbnailSize = 0;
			if (!Int32.TryParse(tbPixels.Text, out ThumbnailSize))
				ThumbnailSize = Form1.DEFAULT_THUMBNAIL_SIZE;
			tbPixels.Text = ThumbnailSize.ToString();

			//���ɕ��Ԍ��̐ݒ�
			int nItemsX = 0;
			if (!Int32.TryParse(tbnItemX.Text, out nItemsX))
				nItemsX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
			tbnItemX.Text = nItemsX.ToString();

			//Bitmap�̑z��T�C�Y���v�Z
			int ItemCount = m_thumbnailSet.Count;
			int nItemsY = ItemCount / nItemsX;	//�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
			if (ItemCount % nItemsX > 0)		//����؂�Ȃ������ꍇ��1�s�ǉ�
				nItemsY++;

			tbInfo.Text = string.Format("�o�͉摜�T�C�Y : {0:N0} x {1:N0} [pixels]\r\n",
				nItemsX * ThumbnailSize, nItemsY * ThumbnailSize);

		}


	}
}
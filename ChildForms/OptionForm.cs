using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Marmi
{
    public partial class OptionForm : Form
    {
        //static List<KeyConfig> keyConfigList = new List<KeyConfig>();
        private bool KeyDuplicationError = false;

        private AppGlobalConfig _config = null;

        public OptionForm()
        {
            InitializeComponent();
        }

        public void LoadConfig(AppGlobalConfig set)
        {
            _config = set.Clone();

            generalConfigBindingSource.DataSource = _config.General;
            advanceConfigBindingSource.DataSource = _config.Advance;
            loupeConfigBindingSource.DataSource = _config.Loupe;
            mouseConfigBindingSource.DataSource = _config.Mouse;
            thumbnailConfigBindingSource.DataSource = _config.Thumbnail;
            viewConfigBindingSource.DataSource = _config.View;
            keyConfigBindingSource.DataSource = _config.Keys;

            //���x�Ȑݒ�^�u
            bStopPaintingAtResize.Checked = set.StopPaintingAtResize; //���T�C�Y�`��
        }


        public void SaveConfig(ref AppGlobalConfig set)
        {
            set.General = _config.General;
            set.Advance = _config.Advance;
            set.Loupe = _config.Loupe;
            set.Mouse = _config.Mouse;
            set.Thumbnail= _config.Thumbnail;
            set.View= _config.View;
            set.Keys= _config.Keys;

            //���x�Ȑݒ�^�u
            set.StopPaintingAtResize = bStopPaintingAtResize.Checked;

        }


        private void InitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "�S�Ă̐ݒ肪�����l�ɖ߂�܂������s���܂����H",
                    "�m�F",
                    MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                LoadConfig(new AppGlobalConfig());      //�����l�����o��
                this.Refresh();
            }
        }

        private void SaveConfig_CheckedChanged(object sender, EventArgs e)
        {
            //�S�Ă�TabPage1���̃A�C�e���̓���ݒ�
            //�ݒ��ۑ�����Ƃ���Enable�A�����łȂ��Ƃ���Disable
            foreach (Control o in General.Controls)
            {
                switch (o.Name)
                {
                    case "bSaveConfig":
                        break;

                    case "loupeUserSetting":
                        break;

                    default:
                        o.Enabled = bSaveConfig.Checked;
                        break;
                }
            }
        }

        private void PictureBoxBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox_BackColor.BackColor = colorDialog1.Color;
            }
        }

        private void TextBox1_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog(this) == DialogResult.OK)
            {
                linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            }
        }

        private void ThumbnailFontColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ThumbnailFontColor.BackColor = colorDialog1.Color;
            }
        }

        private void ThumbnailBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ThumbnailBackColor.BackColor = colorDialog1.Color;
            }
        }

        private void OnFocus_Enter(object sender, EventArgs e)
        {
            HelpBox.Text = (string)(((Control)sender).Tag);
            //toolTip1.Show((string)(((Control)sender).Tag), this);
            toolTip1.Show("hai", this, 5000);
            toolTip1.SetToolTip((Control)sender, (string)((Control)sender).Tag);
        }

        //ver1.81 KeyAccelerator ���p�ɔ��� Validating �Ɉڍs
        /// <summary>
        /// �L�[�d���`�F�b�N���[�`��
        /// �R���g���[���̒l���r����
        /// </summary>
        /// <returns>�d�����Ă����ꍇ��true</returns>
        //private bool CheckKeyDuplicate()
        //{
        //	//�L�[�R���t�B�O�ɏd�����Ȃ����Ƃ��`�F�b�N
        //	List<string> checkkey = new List<string>();

        //	foreach (Control c in keyConfigGroupBox.Controls)
        //	{
        //		if (c is ComboBox)
        //		{
        //			if (c.Text.Contains("�Ȃ�"))
        //				continue;
        //			if (checkkey.Contains(c.Text))
        //				return true;
        //			else
        //				checkkey.Add(c.Text);
        //		}
        //	}
        //	return false;
        //}

        /// <summary>
        /// ���̂܂܃t�H�[������Ă������`�F�b�N
        /// ver1.21�̃L�[�R���t�B�O�d���`�F�b�N�̂��ߒǉ�
        /// </summary>
        private void FormOption_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (KeyDuplicationError)
                e.Cancel = true;
        }

        /// <summary>
        /// OK�{�^�����������ۂɕs����Ȃ��������`�F�b�N
        /// ver1.21�ł̓L�[�R���t�B�O�d���`�F�b�N
        /// </summary>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            //if (CheckKeyDuplicate())
            //{
            //	MessageBox.Show("�L�[�ݒ肪�d�����Ă��܂�");
            //	KeyDuplicationError = true;
            //}
            //else
            KeyDuplicationError = false;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //Cancel�{�^�������������̓G���[����
            KeyDuplicationError = false;
        }

        private void RadioRightScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //�N���b�N��ʕ\�����X�V
            pictureBoxRightScr.Image = Properties.Resources.ScrNext;
            pictureBoxLeftScr.Image = Properties.Resources.ScrPrev;
        }

        private void RadioLeftScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //�N���b�N��ʕ\�����X�V
            pictureBoxRightScr.Image = Properties.Resources.ScrPrev;
            pictureBoxLeftScr.Image = Properties.Resources.ScrNext;
        }

        private void PictureBoxRightScr_Click(object sender, EventArgs e)
        {
            //���W�I�{�b�N�X��A��->�摜���ς��
            radioRightScrToNextPic.Checked = true;
            //radioLeftScrToNextPic.Checked = false;
        }

        private void PictureBoxLeftScr_Click(object sender, EventArgs e)
        {
            //���W�I�{�b�N�X��A��->�摜���ς��
            //radioRightScrToNextPic.Checked = false;
            radioLeftScrToNextPic.Checked = true;
        }

        private void TmpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tmpFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        //private void Label35_Click(object sender, EventArgs e)
        //{
        //}

        //private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        //{
        //}

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == 7)
                e.Cancel = true;
        }

        /// <summary>
        /// KeyAccelerator�̓��͒l����
        /// ����̓��͒l������ꍇ�̓L�����Z������B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyAcc_Validating(object sender, CancelEventArgs e)
        {
            //�������ƂȂ�KeyAccelerator
            var org = sender as KeyAccelerator;

            //�ݒ肪�Ȃ��̂Ȃ牽�����Ȃ��B
            if (org.keyData == Keys.None)
                return;

            //�R���g���[�����
            foreach (var c in tableLayoutPanel1.Controls)
                if (c is KeyAccelerator)
                {
                    KeyAccelerator testing = c as KeyAccelerator;
                    if (testing == org)
                        //�������g�̓`�F�b�N�ΏۊO
                        continue;
                    else
                        //�`�F�b�N
                        if (testing.keyData == org.keyData)
                    {
                        //�d�����Ă���
                        var ret = MessageBox.Show(
                            string.Format("�u{0}�v�Ɛݒ肪�d�����Ă��܂��B�㏑�����܂����H", testing.Tag),
                            "�L�[�ݒ�m�F",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                        if (ret == DialogResult.Yes)
                        {
                            //�ق��̃R���g���[����ύX����
                            testing.keyData = Keys.None;
                            testing.Invalidate();
                        }
                        else
                        {
                            //Cancel����B
                            e.Cancel = true;
                        }
                    }
                }
        }
    }//class
}//namespace
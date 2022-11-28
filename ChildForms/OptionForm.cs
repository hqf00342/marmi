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

            //���x�Ȑݒ�^�u
            bStopPaintingAtResize.Checked = set.StopPaintingAtResize; //���T�C�Y�`��

            //�T���l�C���^�u
            //LoadGeneralConfig(set);
            //LoadViewConfig(set);
            //LoadThumbnailConfig(set);
            //LoadMouseConfig(set);
            LoadKeyConfig(set);
            //LoadAdvanceConfig(set);

            //ver1.70 2���\���̌����`�F�b�N
            //dualview_exactCheck.Checked = set.dualview_exactCheck;

            //ver1.78 �{���̕ێ�
            keepMagnification.Checked = set.KeepMagnification;
        }

        private void LoadKeyConfig(AppGlobalConfig set)
        {
            ka_exit1.KeyData = set.Keys.Key_Exit1;
            ka_exit2.KeyData = set.Keys.Key_Exit2;
            ka_bookmark1.KeyData = set.Keys.Key_Bookmark1;
            ka_fullscreen1.KeyData = set.Keys.Key_Fullscreen1;
            ka_dualview1.KeyData = set.Keys.Key_Dualview1;
            ka_viewratio1.KeyData = set.Keys.Key_ViewRatio1;
            ka_recycle1.KeyData = set.Keys.Key_Recycle1;
            ka_rotate1.KeyData = set.Keys.Key_Rotate1;
            ka_nextpage1.KeyData = set.Keys.Key_Nextpage1;
            ka_nextpage2.KeyData = set.Keys.Key_Nextpage2;
            ka_prevpage1.KeyData = set.Keys.Key_Prevpage1;
            ka_prevpage2.KeyData = set.Keys.Key_Prevpage2;
            ka_prevhalf1.KeyData = set.Keys.Key_Prevhalf1;
            ka_nexthalf1.KeyData = set.Keys.Key_Nexthalf1;
            ka_toppage1.KeyData = set.Keys.Key_Toppage1;
            ka_lastpage1.KeyData = set.Keys.Key_Lastpage1;
            ka_thunbnail.KeyData = set.Keys.Key_Thumbnail;
            ka_sidebar.KeyData = set.Keys.Key_Sidebar;
            ka_minWindow.KeyData = set.Keys.Key_MinWindow;
        }

        public void SaveConfig(ref AppGlobalConfig set)
        {
            //SaveGnereralConfig(ref set);
            set.General = _config.General;
            set.Advance = _config.Advance;
            set.Loupe = _config.Loupe;
            set.Mouse = _config.Mouse;
            set.Thumbnail= _config.Thumbnail;

            //���x�Ȑݒ�^�u
            set.StopPaintingAtResize = bStopPaintingAtResize.Checked;

            //�T���l�C���^�u
            //SaveThumbnailConfig(ref set);

            //�g��\���֘A
            //SaveViewConfig(ref set);

            //ver1.35 ���[�v
            //set.isLoopToTopPage = isLoopToTopPage.Checked;

            //ver1.70 2���\���̌����`�F�b�N
            //set.dualview_exactCheck = dualview_exactCheck.Checked;

            //set.LastPage_toNextArchive = lastPage_toNextArchive.Checked;

            //ver1.78 �{���̕ێ�
            set.KeepMagnification = keepMagnification.Checked;

            //ver1.91 �L�[�R���t�B�O
            SaveKeyConfig(ref set);
            //SaveMouseConfig(ref set);
        }

        private void SaveKeyConfig(ref AppGlobalConfig set)
        {
            set.Keys.Key_Exit1 = ka_exit1.KeyData;
            set.Keys.Key_Exit2 = ka_exit2.KeyData;
            set.Keys.Key_Bookmark1 = ka_bookmark1.KeyData;
            set.Keys.Key_Fullscreen1 = ka_fullscreen1.KeyData;
            set.Keys.Key_Dualview1 = ka_dualview1.KeyData;
            set.Keys.Key_ViewRatio1 = ka_viewratio1.KeyData;
            set.Keys.Key_Recycle1 = ka_recycle1.KeyData;
            set.Keys.Key_Rotate1 = ka_rotate1.KeyData;
            //1.80�L�[�R���t�B�O�i�r�Q�[�V�����֘A;
            set.Keys.Key_Nextpage1 = ka_nextpage1.KeyData;
            set.Keys.Key_Nextpage2 = ka_nextpage2.KeyData;
            set.Keys.Key_Prevpage1 = ka_prevpage1.KeyData;
            set.Keys.Key_Prevpage2 = ka_prevpage2.KeyData;
            set.Keys.Key_Prevhalf1 = ka_prevhalf1.KeyData;
            set.Keys.Key_Nexthalf1 = ka_nexthalf1.KeyData;
            set.Keys.Key_Toppage1 = ka_toppage1.KeyData;
            set.Keys.Key_Lastpage1 = ka_lastpage1.KeyData;
            set.Keys.Key_Thumbnail = ka_thunbnail.KeyData;
            set.Keys.Key_Sidebar = ka_sidebar.KeyData;
            set.Keys.Key_MinWindow = ka_minWindow.KeyData;
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
            if (org.KeyData == Keys.None)
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
                        if (testing.KeyData == org.KeyData)
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
                            testing.KeyData = Keys.None;
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
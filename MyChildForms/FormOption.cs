using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormOption : Form
    {
        //static List<KeyConfig> keyConfigList = new List<KeyConfig>();
        private bool KeyDuplicationError = false;

        //AppGlobalConfig config = new AppGlobalConfig();

        public FormOption()
        {
            InitializeComponent();

            //TODO�L�[�R���t�B�O�pComboBox��������������
            //alwaysExtractArchive.DataBindings.Add("Checked", config, "AlwaysExtractArchive");
        }

        public void LoadConfig(AppGlobalConfig set)
        {
            //config = set.Clone();

            //�S�ʃ^�u

            //���x�Ȑݒ�^�u
            bStopPaintingAtResize.Checked = set.IsStopPaintingAtResize; //���T�C�Y�`��

            //�T���l�C���^�u
            LoadGeneralConfig(set);
            LoadViewConfig(set);
            LoadThumbnailConfig(set);
            LoadLoupeConfig(set);
            LoadMouseConfig(set);
            LoadKeyConfig(set);
            LoadAdvanceConfig(set);

            //ver1.70 2���\���̌����`�F�b�N
            //dualview_exactCheck.Checked = set.dualview_exactCheck;

            //ver1.78 �{���̕ێ�
            keepMagnification.Checked = set.KeepMagnification;
        }

        private void LoadViewConfig(AppGlobalConfig set)
        {
            noEnlargeOver100p.Checked = set.View.NoEnlargeOver100p;
            isDotByDotZoom.Checked = set.View.IsDotByDotZoom;
            lastPage_stay.Checked = set.View.LastPage_stay;
            lastPage_toTop.Checked = set.View.LastPage_toTop;
            dualView_Force.Checked = set.View.DualView_Force;
            dualView_Normal.Checked = set.View.DualView_Normal;
            dualView_withSizeCheck.Checked = set.View.DualView_withSizeCheck;
            SwitchPicMode.SelectedIndex = (int)(set.View.PictureSwitchMode);
        }

        private void LoadGeneralConfig(AppGlobalConfig set)
        {
            bSaveConfig.Checked = set.General.IsSaveConfig;
            bContinueZip.Checked = set.General.IsContinueZipView;
            isExtractIfSolidArchive.Checked = set.General.IsExtractIfSolidArchive;
            bReplaceArrowButton.Checked = set.General.IsReplaceArrowButton;
            pictureBox_BackColor.BackColor = set.General.BackColor;
            isWindowPosCenter.Checked = set.General.IsWindowPosCenter;
            eraseToolbarItemString.Checked = set.General.EraseToolbarItemString;
            sidebar_smoothscroll.Checked = set.General.Sidebar_smoothScroll;
            tmpFolder.Text = set.General.TmpFolder;
            numOfMru.Text = set.General.NumberOfMru.ToString();
            disableMultipleStarts.Checked = set.General.DisableMultipleStarts;
            simpleCalcWindowPos.Checked = set.General.SimpleCalcForWindowLocation;
            saveFullScreenMode.Checked = set.General.SaveFullScreenMode;
            alwaysExtractArchive.Checked = set.General.AlwaysExtractArchive;
        }

        private void LoadThumbnailConfig(AppGlobalConfig set)
        {
            thumbnailSize.Text = set.Thumbnail.ThumbnailSize.ToString();
            ThumbnailBackColor.BackColor = set.Thumbnail.ThumbnailBackColor;
            fontDialog1.Font = set.Thumbnail.ThumbnailFont;
            linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            ThumbnailFontColor.BackColor = set.Thumbnail.ThumbnailFontColor;
            isDrawThumbnailFrame.Checked = set.Thumbnail.IsDrawThumbnailFrame;
            isDrawThumbnailShadow.Checked = set.Thumbnail.IsDrawThumbnailShadow;
            isShowTPFileName.Checked = set.Thumbnail.IsShowTPFileName;
            isShowTPFileSize.Checked = set.Thumbnail.IsShowTPFileSize;
            isShowTPPicSize.Checked = set.Thumbnail.IsShowTPPicSize;
            isThumbFadein.Checked = set.Thumbnail.IsThumbFadein;
            //ver1.81�T���l�C���̃A�j���[�V��������
            ThumbnailPanelSmoothScroll.Checked = set.Thumbnail.ThumbnailPanelSmoothScroll;
        }

        private void LoadAdvanceConfig(AppGlobalConfig set)
        {
            isFastDraw.Checked = set.Advance.IsFastDrawAtResize;
            tb_cachesize.Text = set.Advance.CacheSize.ToString();
            useUnsharpMask.Checked = set.Advance.UseUnsharpMask;
            unsharpDepth.Value = (decimal)set.Advance.UnsharpDepth;
        }

        private void LoadLoupeConfig(AppGlobalConfig set)
        {
            isOriginalSizeLoupe.Checked = set.Loupe.IsOriginalSizeLoupe;
            loupeMag.Text = set.Loupe.loupeMagnifcant.ToString();
        }

        private void LoadMouseConfig(AppGlobalConfig set)
        {
            mouseConfigWheel.Text = set.Mouse.MouseConfigWheel;

            //ver1.64 ��ʃi�r
            radioRightScrToNextPic.Checked = set.Mouse.RightScrClickIsNextPic;
            radioLeftScrToNextPic.Checked = !set.Mouse.RightScrClickIsNextPic;
            reverseClickPointWhenLeftBook.Checked = set.Mouse.ReverseDirectionWhenLeftBook;
            DoubleClickToFullscreen.Checked = set.Mouse.DoubleClickToFullscreen;
        }

        private void LoadKeyConfig(AppGlobalConfig set)
        {
            ka_exit1.keyData = set.Keys.Key_Exit1;
            ka_exit2.keyData = set.Keys.Key_Exit2;
            ka_bookmark1.keyData = set.Keys.Key_Bookmark1;
            ka_fullscreen1.keyData = set.Keys.Key_Fullscreen1;
            ka_dualview1.keyData = set.Keys.Key_Dualview1;
            ka_viewratio1.keyData = set.Keys.Key_ViewRatio1;
            ka_recycle1.keyData = set.Keys.Key_Recycle1;
            ka_rotate1.keyData = set.Keys.Key_Rotate1;
            ka_nextpage1.keyData = set.Keys.Key_Nextpage1;
            ka_nextpage2.keyData = set.Keys.Key_Nextpage2;
            ka_prevpage1.keyData = set.Keys.Key_Prevpage1;
            ka_prevpage2.keyData = set.Keys.Key_Prevpage2;
            ka_prevhalf1.keyData = set.Keys.Key_Prevhalf1;
            ka_nexthalf1.keyData = set.Keys.Key_Nexthalf1;
            ka_toppage1.keyData = set.Keys.Key_Toppage1;
            ka_lastpage1.keyData = set.Keys.Key_Lastpage1;
        }

        public void SaveConfig(ref AppGlobalConfig set)
        {
            SaveGnereralConfig(ref set);

            //���x�Ȑݒ�^�u
            set.IsStopPaintingAtResize = bStopPaintingAtResize.Checked;

            //�T���l�C���^�u
            SaveThumbnailConfig(ref set);

            //�g��\���֘A
            SaveViewConfig(ref set);

            //ver1.35 ���[�v
            //set.isLoopToTopPage = isLoopToTopPage.Checked;

            //ver1.70 2���\���̌����`�F�b�N
            //set.dualview_exactCheck = dualview_exactCheck.Checked;

            //set.LastPage_toNextArchive = lastPage_toNextArchive.Checked;

            //ver1.78 �{���̕ێ�
            set.KeepMagnification = keepMagnification.Checked;

            //ver1.91 �L�[�R���t�B�O
            SaveKeyConfig(ref set);
            SaveMouseConfig(ref set);
            SaveLoupeConfig(ref set);
            SaveAdvanceConfig(ref set);
        }

        private void SaveViewConfig(ref AppGlobalConfig set)
        {
            set.View.NoEnlargeOver100p = noEnlargeOver100p.Checked;
            set.View.IsDotByDotZoom = isDotByDotZoom.Checked;
            set.View.LastPage_stay = lastPage_stay.Checked;
            set.View.LastPage_toTop = lastPage_toTop.Checked;
            set.View.DualView_Force = dualView_Force.Checked;
            set.View.DualView_Normal = dualView_Normal.Checked;
            set.View.DualView_withSizeCheck = dualView_withSizeCheck.Checked;
            set.View.PictureSwitchMode = (AnimateMode)SwitchPicMode.SelectedIndex;
        }

        private void SaveGnereralConfig(ref AppGlobalConfig set)
        {
            set.General.IsSaveConfig = bSaveConfig.Checked;
            set.General.IsContinueZipView = bContinueZip.Checked;
            set.General.IsReplaceArrowButton = bReplaceArrowButton.Checked;
            set.General.BackColor = pictureBox_BackColor.BackColor;
            set.General.IsWindowPosCenter = isWindowPosCenter.Checked;
            set.General.IsExtractIfSolidArchive = isExtractIfSolidArchive.Checked;
            set.General.EraseToolbarItemString = eraseToolbarItemString.Checked;
            set.General.Sidebar_smoothScroll = sidebar_smoothscroll.Checked;
            set.General.TmpFolder = tmpFolder.Text;
            set.General.NumberOfMru = int.TryParse(numOfMru.Text, out var n) ? n : 10;
            set.General.DisableMultipleStarts = disableMultipleStarts.Checked;
            set.General.SimpleCalcForWindowLocation = simpleCalcWindowPos.Checked;
            set.General.SaveFullScreenMode = saveFullScreenMode.Checked;
            set.General.AlwaysExtractArchive = alwaysExtractArchive.Checked;
        }

        private void SaveThumbnailConfig(ref AppGlobalConfig set)
        {
            set.Thumbnail.ThumbnailSize = int.TryParse(thumbnailSize.Text, out var s) ? s : 120;
            set.Thumbnail.ThumbnailBackColor = ThumbnailBackColor.BackColor;
            set.Thumbnail.ThumbnailFont = fontDialog1.Font;
            set.Thumbnail.ThumbnailFontColor = ThumbnailFontColor.BackColor;
            set.Thumbnail.IsDrawThumbnailFrame = isDrawThumbnailFrame.Checked;
            set.Thumbnail.IsDrawThumbnailShadow = isDrawThumbnailShadow.Checked;
            set.Thumbnail.IsShowTPFileName = isShowTPFileName.Checked;
            set.Thumbnail.IsShowTPFileSize = isShowTPFileSize.Checked;
            set.Thumbnail.IsShowTPPicSize = isShowTPPicSize.Checked;
            set.Thumbnail.IsThumbFadein = isThumbFadein.Checked;
            //ver1.81�T���l�C���̃A�j���[�V��������
            set.Thumbnail.ThumbnailPanelSmoothScroll = ThumbnailPanelSmoothScroll.Checked;
        }

        private void SaveAdvanceConfig(ref AppGlobalConfig set)
        {
            set.Advance.IsFastDrawAtResize = isFastDraw.Checked;
            set.Advance.CacheSize = int.TryParse(tb_cachesize.Text, out var cs) ? cs : 500;
            set.Advance.UseUnsharpMask = useUnsharpMask.Checked;
            set.Advance.UnsharpDepth = (int)unsharpDepth.Value;
        }

        private void SaveLoupeConfig(ref AppGlobalConfig set)
        {
            set.Loupe.IsOriginalSizeLoupe = isOriginalSizeLoupe.Checked;
            set.Loupe.loupeMagnifcant = int.TryParse(loupeMag.Text, out var m) ? m : 3;
        }

        private void SaveMouseConfig(ref AppGlobalConfig set)
        {
            set.Mouse.MouseConfigWheel = mouseConfigWheel.Text;

            //ver1.64 ��ʃi�r
            set.Mouse.RightScrClickIsNextPic = radioRightScrToNextPic.Checked;
            set.Mouse.ReverseDirectionWhenLeftBook = reverseClickPointWhenLeftBook.Checked;
            set.Mouse.DoubleClickToFullscreen = DoubleClickToFullscreen.Checked;
        }

        private void SaveKeyConfig(ref AppGlobalConfig set)
        {
            set.Keys.Key_Exit1 = ka_exit1.keyData;
            set.Keys.Key_Exit2 = ka_exit2.keyData;
            set.Keys.Key_Bookmark1 = ka_bookmark1.keyData;
            set.Keys.Key_Fullscreen1 = ka_fullscreen1.keyData;
            set.Keys.Key_Dualview1 = ka_dualview1.keyData;
            set.Keys.Key_ViewRatio1 = ka_viewratio1.keyData;
            set.Keys.Key_Recycle1 = ka_recycle1.keyData;
            set.Keys.Key_Rotate1 = ka_rotate1.keyData;
            //1.80�L�[�R���t�B�O�i�r�Q�[�V�����֘A;
            set.Keys.Key_Nextpage1 = ka_nextpage1.keyData;
            set.Keys.Key_Nextpage2 = ka_nextpage2.keyData;
            set.Keys.Key_Prevpage1 = ka_prevpage1.keyData;
            set.Keys.Key_Prevpage2 = ka_prevpage2.keyData;
            set.Keys.Key_Prevhalf1 = ka_prevhalf1.keyData;
            set.Keys.Key_Nexthalf1 = ka_nexthalf1.keyData;
            set.Keys.Key_Toppage1 = ka_toppage1.keyData;
            set.Keys.Key_Lastpage1 = ka_lastpage1.keyData;
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
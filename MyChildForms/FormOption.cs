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
            bSaveConfig.Checked = set.isSaveConfig;                     //�ݒ�̕ۑ�
                                                                        //bSaveThumbnailCache.Checked = set.isSaveThumbnailCache;		//�L���b�V���̕ۑ�
            bContinueZip.Checked = set.isContinueZipView;               //zip�t�@�C���͑O��̑�������
                                                                        //bDeleteOldCache.Checked = set.isAutoCleanOldCache;			//�Â��L���b�V���̍폜
            bReplaceArrowButton.Checked = set.isReplaceArrowButton;     //���{�^���̓���ւ�
            pictureBox_BackColor.BackColor = set.BackColor;             //�w�i�F
            isFastDraw.Checked = set.isFastDrawAtResize;
            isWindowPosCenter.Checked = set.isWindowPosCenter;

            //���x�Ȑݒ�^�u
            bStopPaintingAtResize.Checked = set.isStopPaintingAtResize; //���T�C�Y�`��

            //�T���l�C���^�u
            thumbnailSize.Text = set.ThumbnailSize.ToString();
            ThumbnailBackColor.BackColor = set.ThumbnailBackColor;
            fontDialog1.Font = set.ThumbnailFont;
            linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            ThumbnailFontColor.BackColor = set.ThumbnailFontColor;
            isDrawThumbnailFrame.Checked = set.isDrawThumbnailFrame;
            isDrawThumbnailShadow.Checked = set.isDrawThumbnailShadow;
            isShowTPFileName.Checked = set.isShowTPFileName;
            isShowTPFileSize.Checked = set.isShowTPFileSize;
            isShowTPPicSize.Checked = set.isShowTPPicSize;
            isThumbFadein.Checked = set.isThumbFadein;

            //���[�y�֘A
            isOriginalSizeLoupe.Checked = set.isOriginalSizeLoupe;
            loupeMag.Text = set.loupeMagnifcant.ToString();

            //ver1.09 ���Ɋ֘A
            isExtractIfSolidArchive.Checked = set.isExtractIfSolidArchive;

            //ver1.09 �N���X�t�F�[�h
            //isCrossfadeTransition.Checked = set.isCrossfadeTransition;

            //ver1.21 �L�[�R���t�B�O
            //ver1.81�R�����g�A�E�g
            //keyConfBookmark.Text = set.keyConfBookMark;
            //keyConfFullScr.Text = set.keyConfFullScr;
            //keyConfLastPage.Text = set.keyConfLastPage;
            //keyConfNextPage.Text = set.keyConfNextPage;
            //keyConfNextPageHalf.Text = set.keyConfNextPageHalf;
            //keyConfPrevPage.Text = set.keyConfPrevPage;
            //keyConfPrevPageHalf.Text = set.keyConfPrevPageHalf;
            //keyConfPrintMode.Text = set.keyConfPrintMode;
            //keyConfTopPage.Text = set.keyConfTopPage;
            //keyConfDualMode.Text = set.keyConfDualMode;
            //keyConfRecycleBin.Text = set.keyConfRecycleBin;
            //keyConfExitApp.Text = set.keyConfExitApp;

            //�}�E�X�R���t�B�O
            mouseConfigWheel.Text = set.mouseConfigWheel;

            //��ʐ؂�ւ����@
            SwitchPicMode.SelectedIndex = (int)set.pictureSwitchMode;

            //�g��\���֘A
            noEnlargeOver100p.Checked = set.noEnlargeOver100p;
            isDotByDotZoom.Checked = set.isDotByDotZoom;

            //ver1.35 ���[�v
            //isLoopToTopPage.Checked = set.isLoopToTopPage;

            //ver1.64 ��ʃi�r
            radioRightScrToNextPic.Checked = set.RightScrClickIsNextPic;
            radioLeftScrToNextPic.Checked = !set.RightScrClickIsNextPic;
            reverseClickPointWhenLeftBook.Checked = set.ReverseDirectionWhenLeftBook;

            //ver1.65�c�[���o�[�A�C�e���̕�������
            eraseToolbarItemString.Checked = set.eraseToolbarItemString;

            //ver1.70 2���\���̌����`�F�b�N
            //dualview_exactCheck.Checked = set.dualview_exactCheck;

            //ver1.70 �T�C�h�o�[�̃X���[�X�X�N���[���@�\
            sidebar_smoothscroll.Checked = set.sidebar_smoothScroll;

            //ver1.71 �ŏI�y�[�W�̓���
            lastPage_stay.Checked = set.lastPage_stay;
            lastPage_toTop.Checked = set.lastPage_toTop;
            lastPage_toNextArchive.Checked = set.lastPage_toNextArchive;

            //ver1.73 �ꎞ�t�H���_
            tmpFolder.Text = set.tmpFolder;

            //ver1.73 MRU�ێ���
            numOfMru.Text = set.numberOfMru.ToString();

            //ver1.76���d�N��
            disableMultipleStarts.Checked = set.disableMultipleStarts;
            //ver1.77 �E�B���h�E�\���ʒu���ȈՂɂ��邩
            simpleCalcWindowPos.Checked = set.simpleCalcForWindowLocation;
            //ver1.77 �t���X�N���[����Ԃ𕜌��ł���悤�ɂ���
            saveFullScreenMode.Checked = set.saveFullScreenMode;
            //ver1.78 �{���̕ێ�
            keepMagnification.Checked = set.keepMagnification;
            //ver1.79 ���ɂ͕K���W�J
            alwaysExtractArchive.Checked = set.AlwaysExtractArchive;
            //ver1.79 2�y�[�W���[�h�A���S���Y��
            dualView_Force.Checked = set.dualView_Force;
            dualView_Normal.Checked = set.dualView_Normal;
            dualView_withSizeCheck.Checked = set.dualView_withSizeCheck;

            //ver1.80 �L�[�R���t�B�O
            ka_exit1.keyData = set.ka_exit1;
            ka_exit2.keyData = set.ka_exit2;
            ka_bookmark1.keyData = set.ka_bookmark1;
            ka_bookmark2.keyData = set.ka_bookmark2;
            ka_fullscreen1.keyData = set.ka_fullscreen1;
            ka_fullscreen2.keyData = set.ka_fullscreen2;
            ka_dualview1.keyData = set.ka_dualview1;
            ka_dualview2.keyData = set.ka_dualview2;
            ka_viewratio1.keyData = set.ka_viewratio1;
            ka_viewratio2.keyData = set.ka_viewratio2;
            ka_recycle1.keyData = set.ka_recycle1;
            ka_recycle2.keyData = set.ka_recycle2;
            //1.80�L�[�R���t�B�O�i�r�Q�[�V�����֘A;
            ka_nextpage1.keyData = set.ka_nextpage1;
            ka_nextpage2.keyData = set.ka_nextpage2;
            ka_prevpage1.keyData = set.ka_prevpage1;
            ka_prevpage2.keyData = set.ka_prevpage2;
            ka_prevhalf1.keyData = set.ka_prevhalf1;
            ka_prevhalf2.keyData = set.ka_prevhalf2;
            ka_nexthalf1.keyData = set.ka_nexthalf1;
            ka_nexthalf2.keyData = set.ka_nexthalf2;
            ka_toppage1.keyData = set.ka_toppage1;
            ka_toppage2.keyData = set.ka_toppage2;
            ka_lastpage1.keyData = set.ka_lastpage1;
            ka_lastpage2.keyData = set.ka_lastpage2;
            //�_�u���N���b�N�őS���
            DoubleClickToFullscreen.Checked = set.DoubleClickToFullscreen;
            //ver1.81�T���l�C���̃A�j���[�V��������
            ThumbnailPanelSmoothScroll.Checked = set.ThumbnailPanelSmoothScroll;
            //ver1.83 �A���V���[�v�}�X�N
            useUnsharpMask.Checked = set.useUnsharpMask;
            unsharpDepth.Value = (decimal)set.unsharpDepth;
        }

        public void SaveConfig(ref AppGlobalConfig set)
        {
            //�S�ʃ^�u
            set.isSaveConfig = bSaveConfig.Checked;
            //set.isSaveThumbnailCache = bSaveThumbnailCache.Checked;
            set.isContinueZipView = bContinueZip.Checked;
            //set.isAutoCleanOldCache = bDeleteOldCache.Checked;
            set.isReplaceArrowButton = bReplaceArrowButton.Checked;
            set.BackColor = pictureBox_BackColor.BackColor;
            set.isFastDrawAtResize = isFastDraw.Checked;
            set.isWindowPosCenter = isWindowPosCenter.Checked;

            //���x�Ȑݒ�^�u
            set.isStopPaintingAtResize = bStopPaintingAtResize.Checked;

            //�T���l�C���^�u
            if (!int.TryParse(thumbnailSize.Text, out set.ThumbnailSize)) set.ThumbnailSize = 120;
            set.ThumbnailBackColor = ThumbnailBackColor.BackColor;
            set.ThumbnailFont = fontDialog1.Font;
            set.ThumbnailFontColor = ThumbnailFontColor.BackColor;
            set.isDrawThumbnailFrame = isDrawThumbnailFrame.Checked;
            set.isDrawThumbnailShadow = isDrawThumbnailShadow.Checked;
            set.isShowTPFileName = isShowTPFileName.Checked;
            set.isShowTPFileSize = isShowTPFileSize.Checked;
            set.isShowTPPicSize = isShowTPPicSize.Checked;
            set.isThumbFadein = isThumbFadein.Checked;

            //���[�y�֘A
            set.isOriginalSizeLoupe = isOriginalSizeLoupe.Checked;
            if (!int.TryParse(loupeMag.Text, out set.loupeMagnifcant))
                set.loupeMagnifcant = 3;

            //ver1.09 ���Ɋ֘A
            set.isExtractIfSolidArchive = isExtractIfSolidArchive.Checked;

            //ver1.09 �N���X�t�F�[�h
            //set.isCrossfadeTransition = isCrossfadeTransition.Checked;

            //ver1.21 �L�[�R���t�B�O
            //ver1.81�R�����g�A�E�g
            //set.keyConfBookMark = keyConfBookmark.Text;
            //set.keyConfFullScr = keyConfFullScr.Text;
            //set.keyConfLastPage = keyConfLastPage.Text;
            //set.keyConfNextPage = keyConfNextPage.Text;
            //set.keyConfNextPageHalf = keyConfNextPageHalf.Text;
            //set.keyConfPrevPage = keyConfPrevPage.Text;
            //set.keyConfPrevPageHalf = keyConfPrevPageHalf.Text;
            //set.keyConfPrintMode = keyConfPrintMode.Text;
            //set.keyConfTopPage = keyConfTopPage.Text;
            //set.keyConfDualMode = keyConfDualMode.Text;
            //set.keyConfRecycleBin = keyConfRecycleBin.Text;
            //set.keyConfExitApp = keyConfExitApp.Text;

            //�}�E�X�R���t�B�O
            set.mouseConfigWheel = mouseConfigWheel.Text;

            //��ʐ؂�ւ����@
            set.pictureSwitchMode =
                //(AppGlobalConfig.AnimateMode)SwitchPicMode.SelectedIndex;
                (AnimateMode)SwitchPicMode.SelectedIndex;
            //�g��\���֘A
            set.noEnlargeOver100p = noEnlargeOver100p.Checked;
            set.isDotByDotZoom = isDotByDotZoom.Checked;

            //ver1.35 ���[�v
            //set.isLoopToTopPage = isLoopToTopPage.Checked;

            //ver1.64 ��ʃi�r
            set.RightScrClickIsNextPic = radioRightScrToNextPic.Checked;
            set.ReverseDirectionWhenLeftBook = reverseClickPointWhenLeftBook.Checked;

            //ver1.65�c�[���o�[�A�C�e���̕�������
            set.eraseToolbarItemString = eraseToolbarItemString.Checked;

            //ver1.70 2���\���̌����`�F�b�N
            //set.dualview_exactCheck = dualview_exactCheck.Checked;

            //ver1.70 �T�C�h�o�[�̃X���[�X�X�N���[���@�\
            set.sidebar_smoothScroll = sidebar_smoothscroll.Checked;

            //ver1.71 �ŏI�y�[�W�̓���
            set.lastPage_stay = lastPage_stay.Checked;
            set.lastPage_toTop = lastPage_toTop.Checked;
            set.lastPage_toNextArchive = lastPage_toNextArchive.Checked;

            //ver1.73 �ꎞ�t�H���_
            set.tmpFolder = tmpFolder.Text;
            //ver1.73 MRU�ێ���
            if (!int.TryParse(numOfMru.Text, out set.numberOfMru))
                set.numberOfMru = 10;   //�f�t�H���g�l

            //ver1.76���d�N��
            set.disableMultipleStarts = disableMultipleStarts.Checked;
            //ver1.77 �E�B���h�E�\���ʒu���ȈՂɂ��邩
            set.simpleCalcForWindowLocation = simpleCalcWindowPos.Checked;
            //ver1.77 �t���X�N���[����Ԃ𕜌��ł���悤�ɂ���
            set.saveFullScreenMode = saveFullScreenMode.Checked;
            //ver1.78 �{���̕ێ�
            set.keepMagnification = keepMagnification.Checked;
            //ver1.79 ���ɂ͕K���W�J
            set.AlwaysExtractArchive = alwaysExtractArchive.Checked;
            //ver1.79 2�y�[�W���[�h�A���S���Y��
            set.dualView_Force = dualView_Force.Checked;
            set.dualView_Normal = dualView_Normal.Checked;
            set.dualView_withSizeCheck = dualView_withSizeCheck.Checked;

            //ver1.80 �L�[�R���t�B�O
            set.ka_exit1 = ka_exit1.keyData;
            set.ka_exit2 = ka_exit2.keyData;
            set.ka_bookmark1 = ka_bookmark1.keyData;
            set.ka_bookmark2 = ka_bookmark2.keyData;
            set.ka_fullscreen1 = ka_fullscreen1.keyData;
            set.ka_fullscreen2 = ka_fullscreen2.keyData;
            set.ka_dualview1 = ka_dualview1.keyData;
            set.ka_dualview2 = ka_dualview2.keyData;
            set.ka_viewratio1 = ka_viewratio1.keyData;
            set.ka_viewratio2 = ka_viewratio2.keyData;
            set.ka_recycle1 = ka_recycle1.keyData;
            set.ka_recycle2 = ka_recycle2.keyData;
            //1.80�L�[�R���t�B�O�i�r�Q�[�V�����֘A;
            set.ka_nextpage1 = ka_nextpage1.keyData;
            set.ka_nextpage2 = ka_nextpage2.keyData;
            set.ka_prevpage1 = ka_prevpage1.keyData;
            set.ka_prevpage2 = ka_prevpage2.keyData;
            set.ka_prevhalf1 = ka_prevhalf1.keyData;
            set.ka_prevhalf2 = ka_prevhalf2.keyData;
            set.ka_nexthalf1 = ka_nexthalf1.keyData;
            set.ka_nexthalf2 = ka_nexthalf2.keyData;
            set.ka_toppage1 = ka_toppage1.keyData;
            set.ka_toppage2 = ka_toppage2.keyData;
            set.ka_lastpage1 = ka_lastpage1.keyData;
            set.ka_lastpage2 = ka_lastpage2.keyData;
            //1.80 �_�u���N���b�N�őS���
            set.DoubleClickToFullscreen = DoubleClickToFullscreen.Checked;
            //ver1.81�T���l�C���̃A�j���[�V��������
            set.ThumbnailPanelSmoothScroll = ThumbnailPanelSmoothScroll.Checked;
            //ver1.83 �A���V���[�v�}�X�N
            set.useUnsharpMask = useUnsharpMask.Checked;
            set.unsharpDepth = (int)unsharpDepth.Value;
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

        private void bSaveConfig_CheckedChanged(object sender, EventArgs e)
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

        private void pictureBoxBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox_BackColor.BackColor = colorDialog1.Color;
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
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
        private void btnOK_Click(object sender, EventArgs e)
        {
            //if (CheckKeyDuplicate())
            //{
            //	MessageBox.Show("�L�[�ݒ肪�d�����Ă��܂�");
            //	KeyDuplicationError = true;
            //}
            //else
            KeyDuplicationError = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Cancel�{�^�������������̓G���[����
            KeyDuplicationError = false;
        }

        private void radioRightScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //�N���b�N��ʕ\�����X�V
            pictureBoxRightScr.Image = Properties.Resources.ScrNext;
            pictureBoxLeftScr.Image = Properties.Resources.ScrPrev;
        }

        private void radioLeftScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //�N���b�N��ʕ\�����X�V
            pictureBoxRightScr.Image = Properties.Resources.ScrPrev;
            pictureBoxLeftScr.Image = Properties.Resources.ScrNext;
        }

        private void pictureBoxRightScr_Click(object sender, EventArgs e)
        {
            //���W�I�{�b�N�X��A��->�摜���ς��
            radioRightScrToNextPic.Checked = true;
            //radioLeftScrToNextPic.Checked = false;
        }

        private void pictureBoxLeftScr_Click(object sender, EventArgs e)
        {
            //���W�I�{�b�N�X��A��->�摜���ς��
            //radioRightScrToNextPic.Checked = false;
            radioLeftScrToNextPic.Checked = true;
        }

        private void tmpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tmpFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void label35_Click(object sender, EventArgs e)
        {
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
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
        private void ka_Validating(object sender, CancelEventArgs e)
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
using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // ���[�e�B���e�B�n�FConfig�t�@�C�� *********************************************/

        /// <summary>
        /// ���[�h�����R���t�B�O���A�v���ɓK�p���Ă���
        /// </summary>
        private void ApplySettingToApplication()
        {
            //�o�[�֘A
            menuStrip1.Visible = g_Config.visibleMenubar;
            toolStrip1.Visible = g_Config.visibleToolBar;
            statusbar.Visible = g_Config.visibleStatusBar;

            //�i�r�o�[
            //g_Sidebar.SetSizeAndDock(GetClientRectangle());
            g_Sidebar.Visible = g_Config.visibleNavibar;

            //ver1.77 ��ʈʒu����F�f���A���f�B�X�v���C�Ή�
            if (g_Config.simpleCalcForWindowLocation)
            {
                //�ȈՁFas is
                this.Size = g_Config.windowSize;
                this.Location = g_Config.windowLocation;
            }
            else
                SetFormPosLocation();

            //ver1.77�S��ʃ��[�h�Ή�
            if (g_Config.saveFullScreenMode && g_Config.isFullScreen)
                SetFullScreen(true);

            //2���\��
            toolButtonDualMode.Checked = g_Config.dualView;

            //MRU���f
            //�I�[�v������Ƃ��Ɏ��{����̂ŃR�����g�A�E�g
            //UpdateMruMenuListUI();

            //�ċA����
            Menu_OptionRecurseDir.Checked = g_Config.isRecurseSearchDir;

            //���E�������Ή�
            if (g_Config.isReplaceArrowButton)
            {
                toolButtonLeft.Tag = "���̃y�[�W�Ɉړ����܂�";
                toolButtonLeft.Text = "����";
                toolButtonRight.Tag = "�O�̃y�[�W�Ɉړ����܂�";
                toolButtonRight.Text = "�O��";
            }
            else
            {
                toolButtonLeft.Tag = "�O�̃y�[�W�Ɉړ����܂�";
                toolButtonLeft.Text = "�O��";
                toolButtonRight.Tag = "���̃y�[�W�Ɉړ����܂�";
                toolButtonRight.Text = "����";
            }

            //�T���l�C���֘A
            if (g_ThumbPanel != null)
            {
                g_ThumbPanel.BackColor = g_Config.ThumbnailBackColor;
                g_ThumbPanel.SetThumbnailSize(g_Config.ThumbnailSize);
                g_ThumbPanel.SetFont(g_Config.ThumbnailFont, g_Config.ThumbnailFontColor);
            }
        }

        private void SetFormPosLocation()
        {
            //�f���A���f�B�X�v���C�Ή�
            //���オ��ʓ��ɂ���X�N���[����T��
            foreach (var scr in Screen.AllScreens)
            {
                if (scr.WorkingArea.Contains(g_Config.windowLocation))
                {
                    SetFormPosLocation2(scr);
                    return;
                }
            }
            //�����ɗ������͂ǂ̃f�B�X�v���C�ɂ������Ȃ������Ƃ�

            //�ǂ̉�ʂɂ������Ȃ��̂Ńv���C�}���ɍs���Ă��炤
            //setFormPosLocation2(Screen.PrimaryScreen);
            //return;
            //�ǂ̉�ʂɂ������Ȃ��̂ň�ԋ߂��f�B�X�v���C��T��
            var pos = g_Config.windowLocation;
            double distance = double.MaxValue;
            int target = 0;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                var scr = Screen.AllScreens[i];
                //�ȈՌv�Z
                var d = Math.Abs(pos.X - scr.Bounds.X) + Math.Abs(pos.Y - scr.Bounds.Y);
                if (d < distance)
                {
                    distance = d;
                    target = i;
                }
            }
            SetFormPosLocation2(Screen.AllScreens[target]);
            return;
        }

        /// <summary>
        /// g_Config�̓��e����\���ʒu�����肷��
        /// �f���A���f�B�X�v���C�ɑΉ�
        /// ��ʊO�ɕ\�������Ȃ��B
        /// </summary>
        /// <param name="scr"></param>
        private void SetFormPosLocation2(Screen scr)
        {
            //���̃X�N���[���̃��[�L���O�G���A���`�F�b�N����
            var disp = scr.WorkingArea;

            //ver1.77 �E�B���h�E�T�C�Y�̒���(����������Ƃ��j
            if (g_Config.windowSize.Width < this.MinimumSize.Width)
                g_Config.windowSize.Width = this.MinimumSize.Width;
            if (g_Config.windowSize.Height < this.MinimumSize.Height)
                g_Config.windowSize.Height = this.MinimumSize.Height;

            //�E�B���h�E�T�C�Y�̒���(�傫������Ƃ��j
            if (disp.Width < g_Config.windowSize.Width)
            {
                g_Config.windowLocation.X = 0;
                g_Config.windowSize.Width = disp.Width;
            }
            if (disp.Height < g_Config.windowSize.Height)
            {
                g_Config.windowLocation.Y = 0;
                g_Config.windowSize.Height = disp.Height;
            }

            //�E�B���h�E�ʒu�̒����i��ʊO:�}�C�i�X�����j
            if (g_Config.windowLocation.X < disp.X)
                g_Config.windowLocation.X = disp.X;
            if (g_Config.windowLocation.Y < disp.Y)
                g_Config.windowLocation.Y = disp.Y;

            //�E������ʊO�ɕ\�������Ȃ�
            var right = g_Config.windowLocation.X + g_Config.windowSize.Width;
            var bottom = g_Config.windowLocation.Y + g_Config.windowSize.Height;
            if (right > disp.X + disp.Width)
                g_Config.windowLocation.X = disp.X + disp.Width - g_Config.windowSize.Width;
            if (bottom > disp.Y + disp.Height)
                g_Config.windowLocation.Y = disp.Y + disp.Height - g_Config.windowSize.Height;

            //�����\���������ǂ���
            if (g_Config.isWindowPosCenter)
            {
                g_Config.windowLocation.X = disp.X + (disp.Width - g_Config.windowSize.Width) / 2;
                g_Config.windowLocation.Y = disp.Y + (disp.Height - g_Config.windowSize.Height) / 2;
            }
            //�T�C�Y�̓K�p
            this.Size = g_Config.windowSize;
            //���������\��
            this.Location = g_Config.windowLocation;
        }
    }
}
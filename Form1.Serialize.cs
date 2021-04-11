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
        private void ApplyConfigToWindow()
        {
            //�o�[�֘A
            menuStrip1.Visible = App.Config.VisibleMenubar;
            toolStrip1.Visible = App.Config.VisibleToolBar;
            statusbar.Visible = App.Config.VisibleStatusBar;

            //�i�r�o�[
            //g_Sidebar.SetSizeAndDock(GetClientRectangle());
            _sidebar.Visible = App.Config.VisibleNavibar;

            //ver1.77 ��ʈʒu����F�f���A���f�B�X�v���C�Ή�
            if (App.Config.SimpleCalcForWindowLocation)
            {
                //�ȈՁFas is
                this.Size = App.Config.windowSize;
                this.Location = App.Config.windowLocation;
            }
            else
                SetFormPosLocation();

            //ver1.77�S��ʃ��[�h�Ή�
            if (App.Config.SaveFullScreenMode && App.Config.isFullScreen)
                SetFullScreen(true);

            //2���\��
            toolButtonDualMode.Checked = App.Config.DualView;

            //MRU���f
            //�I�[�v������Ƃ��Ɏ��{����̂ŃR�����g�A�E�g
            //UpdateMruMenuListUI();

            //�ċA����
            Menu_OptionRecurseDir.Checked = App.Config.IsRecurseSearchDir;

            //���E�������Ή�
            if (App.Config.IsReplaceArrowButton)
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
            if (_thumbPanel != null)
            {
                _thumbPanel.BackColor = App.Config.ThumbnailBackColor;
                _thumbPanel.CalcThumbboxSize(App.Config.ThumbnailSize);
                _thumbPanel.SetFont(App.Config.ThumbnailFont, App.Config.ThumbnailFontColor);
            }
        }

        private void SetFormPosLocation()
        {
            //�f���A���f�B�X�v���C�Ή�
            //���オ��ʓ��ɂ���X�N���[����T��
            foreach (var scr in Screen.AllScreens)
            {
                if (scr.WorkingArea.Contains(App.Config.windowLocation))
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
            var pos = App.Config.windowLocation;
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
        /// App.Config�̓��e����\���ʒu�����肷��
        /// �f���A���f�B�X�v���C�ɑΉ�
        /// ��ʊO�ɕ\�������Ȃ��B
        /// </summary>
        /// <param name="scr"></param>
        private void SetFormPosLocation2(Screen scr)
        {
            //���̃X�N���[���̃��[�L���O�G���A���`�F�b�N����
            var disp = scr.WorkingArea;

            //ver1.77 �E�B���h�E�T�C�Y�̒���(����������Ƃ��j
            if (App.Config.windowSize.Width < this.MinimumSize.Width)
                App.Config.windowSize.Width = this.MinimumSize.Width;
            if (App.Config.windowSize.Height < this.MinimumSize.Height)
                App.Config.windowSize.Height = this.MinimumSize.Height;

            //�E�B���h�E�T�C�Y�̒���(�傫������Ƃ��j
            if (disp.Width < App.Config.windowSize.Width)
            {
                App.Config.windowLocation.X = 0;
                App.Config.windowSize.Width = disp.Width;
            }
            if (disp.Height < App.Config.windowSize.Height)
            {
                App.Config.windowLocation.Y = 0;
                App.Config.windowSize.Height = disp.Height;
            }

            //�E�B���h�E�ʒu�̒����i��ʊO:�}�C�i�X�����j
            if (App.Config.windowLocation.X < disp.X)
                App.Config.windowLocation.X = disp.X;
            if (App.Config.windowLocation.Y < disp.Y)
                App.Config.windowLocation.Y = disp.Y;

            //�E������ʊO�ɕ\�������Ȃ�
            var right = App.Config.windowLocation.X + App.Config.windowSize.Width;
            var bottom = App.Config.windowLocation.Y + App.Config.windowSize.Height;
            if (right > disp.X + disp.Width)
                App.Config.windowLocation.X = disp.X + disp.Width - App.Config.windowSize.Width;
            if (bottom > disp.Y + disp.Height)
                App.Config.windowLocation.Y = disp.Y + disp.Height - App.Config.windowSize.Height;

            //�����\���������ǂ���
            if (App.Config.IsWindowPosCenter)
            {
                App.Config.windowLocation.X = disp.X + (disp.Width - App.Config.windowSize.Width) / 2;
                App.Config.windowLocation.Y = disp.Y + (disp.Height - App.Config.windowSize.Height) / 2;
            }
            //�T�C�Y�̓K�p
            this.Size = App.Config.windowSize;
            //���������\��
            this.Location = App.Config.windowLocation;
        }
    }
}
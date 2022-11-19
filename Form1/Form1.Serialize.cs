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
            //�w�i�F
            PicPanel.BackColor = App.Config.General.BackColor;

            //���j���[�o�[�A�c�[���o�[�A�X�e�[�^�X�o�[
            menuStrip1.Visible = ViewState.VisibleMenubar;
            toolStrip1.Visible = ViewState.VisibleToolBar;
            statusbar.Visible = ViewState.VisibleStatusBar;

            //�i�r�o�[
            _sidebar.Visible = ViewState.VisibleSidebar;

            //ver1.77 ��ʈʒu����F�f���A���f�B�X�v���C�Ή�
            //�f���A���f�B�X�v���C���l�����Ĕz�u
            SetFormPosition();

            //ver1.77�S��ʃ��[�h�Ή�
            if (App.Config.General.SaveFullScreenMode && ViewState.FullScreen)
                SetFullScreen(true);

            //2���\��
            toolButtonDualMode.Checked = ViewState.DualView;

            //MRU���f
            //�I�[�v������Ƃ��Ɏ��{����̂ŃR�����g�A�E�g
            //UpdateMruMenuListUI();

            //�ċA����
            Menu_OptionRecurseDir.Checked = App.Config.RecurseSearchDir;

            //���E�������Ή�
            if (App.Config.General.ReplaceArrowButton)
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
                _thumbPanel.BackColor = App.Config.Thumbnail.ThumbnailBackColor;
                _thumbPanel.SetThumbnailSize(App.Config.Thumbnail.ThumbnailSize);
                _thumbPanel.SetFont(App.Config.Thumbnail.ThumbnailFont, App.Config.Thumbnail.ThumbnailFontColor);
            }

            //�L�[�R���t�B�O���f
            SetKeyConfig2();

            //ver1.65 �c�[���o�[�̕����͂������f
            SetToolbarString();
            ResizeTrackBar();

            //�T���l�C���r���[���Ȃ炷���ɍĕ`��
            if (ViewState.ThumbnailView)
            {
                _thumbPanel.ReDraw();
            }

            //ver1.79 ScreenCache���N���A����B
            //ScreenCache.Clear();
        }

        /// <summary>
        /// �f���A���f�B�X�v���C�Ή��̃t�H�[���ʒu�w��
        /// 2��ʂ��܂�����Ȃ��悤�ɂ���B
        /// </summary>
        private void SetFormPosition()
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

            //�����ɗ������͂ǂ̃f�B�X�v���C�ɂ������Ȃ������Ƃ��B
            //->�v���C�}���ɍs���Ă��炤
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
            var dispRect = scr.WorkingArea;

            //ver1.77 �E�B���h�E�T�C�Y�̒���(����������Ƃ��j
            if (App.Config.windowSize.Width < this.MinimumSize.Width)
                App.Config.windowSize.Width = this.MinimumSize.Width;
            if (App.Config.windowSize.Height < this.MinimumSize.Height)
                App.Config.windowSize.Height = this.MinimumSize.Height;

            //�E�B���h�E�T�C�Y�̒���(�傫������Ƃ��j
            if (dispRect.Width < App.Config.windowSize.Width)
            {
                App.Config.windowLocation.X = 0;
                App.Config.windowSize.Width = dispRect.Width;
            }
            if (dispRect.Height < App.Config.windowSize.Height)
            {
                App.Config.windowLocation.Y = 0;
                App.Config.windowSize.Height = dispRect.Height;
            }

            //�E�B���h�E�ʒu�̒����i��ʊO:�}�C�i�X�����j
            if (App.Config.windowLocation.X < dispRect.X)
                App.Config.windowLocation.X = dispRect.X;
            if (App.Config.windowLocation.Y < dispRect.Y)
                App.Config.windowLocation.Y = dispRect.Y;

            //�E������ʊO�ɕ\�������Ȃ�
            var right = App.Config.windowLocation.X + App.Config.windowSize.Width;
            var bottom = App.Config.windowLocation.Y + App.Config.windowSize.Height;
            if (right > dispRect.X + dispRect.Width)
                App.Config.windowLocation.X = dispRect.X + dispRect.Width - App.Config.windowSize.Width;
            if (bottom > dispRect.Y + dispRect.Height)
                App.Config.windowLocation.Y = dispRect.Y + dispRect.Height - App.Config.windowSize.Height;

            //�����\���������ǂ���
            if (App.Config.General.CenteredAtStart)
            {
                App.Config.windowLocation.X = dispRect.X + (dispRect.Width - App.Config.windowSize.Width) / 2;
                App.Config.windowLocation.Y = dispRect.Y + (dispRect.Height - App.Config.windowSize.Height) / 2;
            }
            //�T�C�Y�̓K�p
            this.Size = App.Config.windowSize;
            //���������\��
            this.Location = App.Config.windowLocation;
        }
    }
}
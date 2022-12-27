using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

/*
�c�[���o�[
Trackbar���܂�
*/

namespace Marmi
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// �c�[���o�[�̏�Ԃ��R���t�B�O�ɍ��킹��
        /// </summary>
        private void UpdateToolbar()
        {
            //��ʃ��[�h�̏�Ԕ��f
            toolButtonDualMode.Checked = ViewState.DualView;
            toolButtonFullScreen.Checked = ViewState.FullScreen;
            toolButtonThumbnail.Checked = ViewState.ThumbnailView;

            //Sidebar
            toolStripButton_Sidebar.Checked = _sidebar.Visible;

            if (App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //�t�@�C�����{�����Ă��Ȃ��ꍇ�̃c�[���o�[
                _trackbar.Enabled = false;
                toolButtonLeft.Enabled = false;
                toolButtonRight.Enabled = false;
                toolButtonThumbnail.Enabled = false;
                toolStripButton_Zoom100.Checked = false;
                toolStripButton_ZoomFit.Checked = false;
                toolStripButton_ZoomOut.Enabled = false;
                toolStripButton_ZoomIn.Enabled = false;
                toolStripButton_Zoom100.Enabled = false;
                toolStripButton_ZoomFit.Enabled = false;
                toolStripButton_Favorite.Enabled = false;
                toolStripButton_Rotate.Enabled = false;
                return;
            }
            else
            {
                //�T���l�C���{�^��
                toolButtonThumbnail.Enabled = true;
                //if(g_makeThumbnail)
                //    toolButtonThumbnail.Enabled = true;
                //else
                //    toolButtonThumbnail.Enabled = false;

                if (ViewState.ThumbnailView)
                {
                    //�T���l�C���\����
                    toolButtonLeft.Enabled = false;
                    toolButtonRight.Enabled = false;
                    toolStripButton_Zoom100.Enabled = false;
                    toolStripButton_ZoomFit.Enabled = false;
                    toolStripButton_Favorite.Enabled = false;
                    toolStripButton_Sidebar.Enabled = false;
                    //toolStripButton_Zoom100.Checked = false;
                    //toolStripButton_ZoomFit.Checked = false;
                }
                else
                {
                    //�ʏ�\����
                    toolStripButton_ZoomIn.Enabled = true;
                    toolStripButton_ZoomOut.Enabled = true;
                    toolStripButton_Zoom100.Enabled = true;
                    toolStripButton_ZoomFit.Enabled = true;
                    toolStripButton_Favorite.Enabled = true;
                    toolStripButton_Sidebar.Enabled = true;
                    toolStripButton_Rotate.Enabled = true;

                    //���E�{�^���̗L������
                    if (App.Config.General.ReplaceArrowButton)
                    {
                        //����ւ�
                        toolButtonLeft.Enabled = !IsLastPageViewing();      //�ŏI�y�[�W�`�F�b�N
                        toolButtonRight.Enabled = (bool)(App.g_pi.NowViewPage != 0);    //�擪�y�[�W�`�F�b�N
                    }
                    else
                    {
                        toolButtonLeft.Enabled = (bool)(App.g_pi.NowViewPage != 0); //�擪�y�[�W�`�F�b�N
                        toolButtonRight.Enabled = !IsLastPageViewing();     //�ŏI�y�[�W�`�F�b�N
                    }

                    //100%�Y�[��
                    toolStripButton_Zoom100.Checked = PicPanel.IsScreen100p;

                    //��ʃt�B�b�g�Y�[��
                    toolStripButton_ZoomFit.Checked = PicPanel.IsFitToScreen;

                    //Favorite
                    if (App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark)
                    {
                        toolStripButton_Favorite.Checked = true;
                    }
                    else if (g_viewPages == 2
                        && App.g_pi.NowViewPage < App.g_pi.Items.Count - 1      //ver1.69 �ŏI�y�[�W���O�`�F�b�N
                        && App.g_pi.Items[App.g_pi.NowViewPage + 1].IsBookMark) //
                    {
                        toolStripButton_Favorite.Checked = true;
                    }
                    else
                    {
                        toolStripButton_Favorite.Checked = false;
                    }

                    //Sidebar
                    toolStripButton_Sidebar.Checked = _sidebar.Visible;
                }

                //TrackBar
                //�����Œ�����UI���x���Ȃ�B
                //g_trackbar.Value = g_pi.NowViewPage;
            }
        }

        /// <summary>
        /// ver1.67 �c�[���o�[�̕�����\��/��\������B
        /// </summary>
        private void SetToolbarString()
        {
            if (App.Config.General.HideToolbarString)
            {
                toolButtonClose.DisplayStyle = ToolStripItemDisplayStyle.Image;
                toolButtonFullScreen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            }
            else
            {
                toolButtonClose.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                toolButtonFullScreen.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }
        }

        /// <summary>
        /// TrackBar���������B�摜�����𔽉f������
        /// </summary>
        private void InitTrackbar()
        {
            _trackbar.Minimum = 0;
            if (App.g_pi.Items.Count > 0)
            {
                _trackbar.Maximum = App.g_pi.Items.Count - 1;
                _trackbar.Enabled = true;

                if (App.g_pi.NowViewPage < 0)
                {
                    App.g_pi.NowViewPage = 0;
                }

                if (App.g_pi.NowViewPage > App.g_pi.Items.Count - 1)
                {
                    App.g_pi.NowViewPage = App.g_pi.Items.Count - 1;
                }

                _trackbar.Value = App.g_pi.NowViewPage;
            }
            else
            {
                _trackbar.Maximum = 0;
                _trackbar.Value = 0;
                _trackbar.Enabled = false;
            }
        }

        /// <summary>
        /// �c�[���o�[���T�C�Y�C�x���g����
        /// ���T�C�Y���͉��������A���T�C�Y����������TrackBar�T�C�Y��ύX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStrip1_Resize(object sender, EventArgs e)
        {
            //���T�C�Y�h���b�O���͕\�������Ȃ�
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                return;

            //TrackBar�T�C�Y���X�V
            //�ő剻����ɌĂ΂��݂���
            ResizeTrackBar();
        }

        /// <summary>
        /// �g���b�N�o�[�T�C�Y���v�Z�����f
        /// </summary>
        private void ResizeTrackBar()
        {
            //g_trackbar�̃T�C�Y���v�Z
            //�c�[���o�[�̋󂫗̈�T�C�Y���v�Z����B
            int trackbarWidth = toolStrip1.Width;
            foreach (ToolStripItem o in toolStrip1.Items)
            {
                if (o.Name != "MarmiTrackBar")
                    trackbarWidth -= o.Width;
            }

            toolStrip1.CanOverflow = false;
            if (_trackbar != null) //�N�����ɃG���[���o��̂�
            {
                //10�̓O���b�v�̑傫�������炢�E�E�E
                if (trackbarWidth > 10)
                    _trackbar.Width = trackbarWidth - 10;
                else
                    _trackbar.Width = 0;
            }
        }

        private async void ToolButtonLeft_Click(object sender, EventArgs e)
        {
            if (App.Config.General.ReplaceArrowButton)
                await NavigateToForwordAsync();
            else
                await NavigateToBackAsync();
        }

        private async void ToolButtonRight_Click(object sender, EventArgs e)
        {
            if (App.Config.General.ReplaceArrowButton)
                await NavigateToBackAsync();
            else
                await NavigateToForwordAsync();
        }

        private void ToolButton_MouseHover(object sender, EventArgs e)
        {
            Statusbar_InfoLabel.Text = (string)((ToolStripButton)sender).Tag;

            //�t�H�[���Ƀt�H�[�J�X���Ȃ��Ƃ����̃C�x���g�͗���
            //1�N���b�N�Ή��p�ɕێ����Ă���
            _hoverStripItem = sender;
        }

        private void ToolButton_MouseLeave(object sender, EventArgs e)
        {
            //�t�@�C�����\���ɖ߂�
            SetStatusbarFilename();

            //1�N���b�N�Ή��p�ɏ���
            _hoverStripItem = null;
        }

        // Zoom�{�^���֘A *********************************************************/

        private void ToolStripButton_ZoomIn_Click(object sender, EventArgs e)
        {
            if (ViewState.ThumbnailView)
            {
                //�T���l�C���Ή�
                _thumbPanel.ThumbSizeZoomIn();
                _thumbPanel.Refresh();
                return;
            }

            //ZoomIn();
            PicPanel.ZoomIn();
        }

        private void ToolStripButton_ZoomOut_Click(object sender, EventArgs e)
        {
            if (ViewState.ThumbnailView)
            {
                //�T���l�C���Ή�
                _thumbPanel.ThumbSizeZoomOut();
                _thumbPanel.Refresh();
                return;
            }

            //ZoomOut();
            PicPanel.ZoomOut();
        }

        private void ToolStripButton_Zoom100_Click(object sender, EventArgs e)
        {
            //if (PicPanel.FittingRatio == 1.0f)
            //    PicPanel.isAutoFit = true;
            //else
            //    PicPanel.isAutoFit = false;
            PicPanel.IsAutoFit = false;
            PicPanel.ZoomRatio = 1.0f;
            //PicPanel.Refresh();
            PicPanel.AjustViewAndShow();
        }

        private void ToolStripButton_ZoomFit_Click(object sender, EventArgs e)
        {
            PicPanel.IsAutoFit = true;
            //�\���{���̒���
            float r = PicPanel.FittingRatio;
            if (r > 1.0f && App.Config.View.ProhigitExpansionOver100p)
                r = 1.0f;
            PicPanel.ZoomRatio = r;
            //PicPanel.Refresh();
            PicPanel.AjustViewAndShow();
        }

        private void ToolStripButton_Rotate_Click(object sender, EventArgs e)
        {
            PicPanel.Rotate(90);
            PicPanel.Refresh();

            //�摜���ɉ�]����������
            var info = App.g_pi.Items[App.g_pi.NowViewPage];
            info.Rotate += 90;
        }

        // �R���g���[���C�x���g *********************************************************/

        private void Trackbar_ValueChanged(object sender, EventArgs e)
        {
            //�I���A�C�e���𒆉��ɕ\��
            if (_trackNaviPanel != null)
                _trackNaviPanel.SetCenterItem(_trackbar.Value);
        }

        private void Trackbar_MouseDown(object sender, MouseEventArgs e)
        {
            //Navibar3�𐶐�
            if (_trackNaviPanel == null)
                _trackNaviPanel = new NaviBar3(App.g_pi);

            if (!PicPanel.Controls.Contains(_trackNaviPanel))
            {
                PicPanel.Controls.Add(_trackNaviPanel);
            }

            //Navibar�̈ʒu�����߂�
            Rectangle r = PicPanel.ClientRectangle;
            if (toolStrip1.Dock == DockStyle.Top)
            {
                if (ViewState.FullScreen)
                    r.Y += toolStrip1.Height;
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }
            else
            {
                //ToolStrip�����ɂ���
                r.Y = r.Height - _trackNaviPanel.Height;
                if (r.Y < 0) r.Y = 0;
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }

            //���݂̃A�C�e���𒆉��ɕ\��
            Trackbar_ValueChanged(null, null);
        }

        private async void Trackbar_MouseUp(object sender, MouseEventArgs e)
        {
            //ValueChanged()�̑���ɂ��̃C�x���g�ŏ���

            //�g���b�N�o�[�p�T���l�C��������ꍇ�͕���
            if (_trackNaviPanel != null)
            {
                _trackNaviPanel.ClosePanel();
                PicPanel.Controls.Remove(_trackNaviPanel);
                Debug.WriteLine("remove navigatebar");
            }

            //�}�E�X�����ꂽ�Ƃ���Ŋm�肷��
            if (_trackbar.Value != App.g_pi.NowViewPage)
            {
                //�y�[�W�ʒu�m��
                App.g_pi.NowViewPage = _trackbar.Value;
                await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010�N6��20��
            }
        }

        private void Trackbar_MouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
            Debug.WriteLine("g_trackbar_MouseWheel");
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}
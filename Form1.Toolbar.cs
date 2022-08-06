using System;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // �c�[���o�[ *******************************************************************/

        private void ToolStrip1_Resize(object sender, EventArgs e)
        {
            //���T�C�Y�h���b�O���͕\�������Ȃ�
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                return;

            //�ő剻����ɌĂ΂��݂���
            ResizeTrackBar();
        }

        private void ResizeTrackBar()
        {
            //g_trackbar�̃T�C�Y�����肷��
            //g_trackbar.width = toolbar.width - sum(items.width)
            int trackbarWidth = toolStrip1.Width;
            foreach (ToolStripItem o in toolStrip1.Items)
            {
                if (o.Name != "MarmiTrackBar")
                    trackbarWidth -= o.Width;
            }

            Debug.WriteLine($"ResizeTrackbar:{trackbarWidth}");

            toolStrip1.CanOverflow = false;
            if (_trackbar != null) //�N�����ɃG���[���o��̂�
            {
                if (trackbarWidth > 10)
                    _trackbar.Width = trackbarWidth - 10;  //10�̓O���b�v�̑傫�������炢�E�E�E
                else
                    _trackbar.Width = 0;
            }
        }

        private void ToolButtonLeft_Click(object sender, EventArgs e)
        {
            if (App.Config.IsReplaceArrowButton)
                NavigateToForword();
            else
                NavigateToBack();
        }

        private void ToolButtonRight_Click(object sender, EventArgs e)
        {
            if (App.Config.IsReplaceArrowButton)
                NavigateToBack();
            else
                NavigateToForword();
        }

        private void ToolButton_MouseHover(object sender, EventArgs e)
        {
            Statusbar_InfoLabel.Text = (string)((ToolStripButton)sender).Tag;

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
            if (App.Config.isThumbnailView)
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
            if (App.Config.isThumbnailView)
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
            if (r > 1.0f && App.Config.NoEnlargeOver100p)
                r = 1.0f;
            PicPanel.ZoomRatio = r;
            //PicPanel.Refresh();
            PicPanel.AjustViewAndShow();
        }

        private void ToolStripButton_Rotate_Click(object sender, EventArgs e)
        {
            PicPanel.Rotate();
            PicPanel.Refresh();
        }

        // �R���g���[���C�x���g *********************************************************/

        private void Trackbar_ValueChanged(object sender, EventArgs e)
        {
            //Debug.WriteLine(g_trackbar.Value, "g_trackbar_ValueChanged");
            if (_trackbar.Value != App.g_pi.NowViewPage)
            {
                ////�g���b�N�o�[�p�̃c�[���`�b�v��\������B
                //Point p = PointToClient(MousePosition);
                //p.Y += 16;	//�\���ʒu��������Ɖ���
                //g_toolTip.Location = p;

                //int ix = g_trackbar.Value;
                //string s = string.Format(
                //    "{0} : {1}",
                //    ix+1,	//�y�[�W�ԍ�
                //    Path.GetFileName(g_pi.Items[ix].filename)	//�t�@�C����
                //    );
                //g_toolTip.Text = s;
            }

            //�T���l�C���\��������Ă����璆�����X�V
            if (_trackNaviPanel != null)
                _trackNaviPanel.SetCenterItem(_trackbar.Value);
        }

        private void Trackbar_MouseDown(object sender, MouseEventArgs e)
        {
            if (_trackNaviPanel == null)
                _trackNaviPanel = new NaviBar3(App.g_pi);

            //g_n3.Parent = this;
            //if(!this.Controls.Contains(g_n3))
            //    this.Controls.Add(g_n3);

            if (!PicPanel.Controls.Contains(_trackNaviPanel))
            {
                PicPanel.Controls.Add(_trackNaviPanel);
                //Debug.WriteLine("add navigatebar");
            }

            //Navibar�̈ʒu�����߂�
            //Rectangle r = GetClientRectangle();
            Rectangle r = PicPanel.ClientRectangle;
            if (toolStrip1.Dock == DockStyle.Top)
            {
                if (App.Config.isFullScreen)
                    r.Y += toolStrip1.Height;
                //Debug.WriteLine(r, "add navigatebar");
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }
            else
            {
                //ToolStrip�����ɂ���
                r.Y = r.Height - _trackNaviPanel.Height;
                if (r.Y < 0) r.Y = 0;
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }

            Trackbar_ValueChanged(null, null);

            //�c�[���`�b�v�̕\��
            //g_toolTip.BringToFront();		//�őO�ʂ�
            //g_toolTip.Show();
        }

        private void Trackbar_MouseUp(object sender, MouseEventArgs e)
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
                SetViewPage(App.g_pi.NowViewPage);  //ver0.988 2010�N6��20��

                //�c�[���`�b�v���B���B
                //g_toolTip.Hide();
                //g_toolTip.Visible = false;
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
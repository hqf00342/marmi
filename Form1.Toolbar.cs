using System;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // ツールバー *******************************************************************/

        private void ToolStrip1_Resize(object sender, EventArgs e)
        {
            //リサイズドラッグ中は表示させない
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                return;

            //最大化直後に呼ばれるみたい
            ResizeTrackBar();
        }

        private void ResizeTrackBar()
        {
            //g_trackbarのサイズを決定する
            //g_trackbar.width = toolbar.width - sum(items.width)
            int trackbarWidth = toolStrip1.Width;
            foreach (ToolStripItem o in toolStrip1.Items)
            {
                if (o.Name != "MarmiTrackBar")
                    trackbarWidth -= o.Width;
            }

            Debug.WriteLine($"ResizeTrackbar:{trackbarWidth}");

            toolStrip1.CanOverflow = false;
            if (_trackbar != null) //起動時にエラーが出るので
            {
                if (trackbarWidth > 10)
                    _trackbar.Width = trackbarWidth - 10;  //10はグリップの大きさ分ぐらい・・・
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

            //1クリック対応用に保持しておく
            _hoverStripItem = sender;
        }

        private void ToolButton_MouseLeave(object sender, EventArgs e)
        {
            //ファイル名表示に戻す
            SetStatusbarFilename();

            //1クリック対応用に処理
            _hoverStripItem = null;
        }

        // Zoomボタン関連 *********************************************************/

        private void ToolStripButton_ZoomIn_Click(object sender, EventArgs e)
        {
            if (App.Config.isThumbnailView)
            {
                //サムネイル対応
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
                //サムネイル対応
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
            //表示倍率の調整
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

        // コントロールイベント *********************************************************/

        private void Trackbar_ValueChanged(object sender, EventArgs e)
        {
            //Debug.WriteLine(g_trackbar.Value, "g_trackbar_ValueChanged");
            if (_trackbar.Value != App.g_pi.NowViewPage)
            {
                ////トラックバー用のツールチップを表示する。
                //Point p = PointToClient(MousePosition);
                //p.Y += 16;	//表示位置をちょっと下に
                //g_toolTip.Location = p;

                //int ix = g_trackbar.Value;
                //string s = string.Format(
                //    "{0} : {1}",
                //    ix+1,	//ページ番号
                //    Path.GetFileName(g_pi.Items[ix].filename)	//ファイル名
                //    );
                //g_toolTip.Text = s;
            }

            //サムネイル表示がされていたら中央を更新
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

            //Navibarの位置を決める
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
                //ToolStripが下にいる
                r.Y = r.Height - _trackNaviPanel.Height;
                if (r.Y < 0) r.Y = 0;
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }

            Trackbar_ValueChanged(null, null);

            //ツールチップの表示
            //g_toolTip.BringToFront();		//最前面に
            //g_toolTip.Show();
        }

        private void Trackbar_MouseUp(object sender, MouseEventArgs e)
        {
            //ValueChanged()の代わりにこのイベントで処理

            //トラックバー用サムネイルがある場合は閉じる
            if (_trackNaviPanel != null)
            {
                _trackNaviPanel.ClosePanel();
                PicPanel.Controls.Remove(_trackNaviPanel);
                Debug.WriteLine("remove navigatebar");
            }

            //マウスが離れたところで確定する
            if (_trackbar.Value != App.g_pi.NowViewPage)
            {
                //ページ位置確定
                App.g_pi.NowViewPage = _trackbar.Value;
                SetViewPage(App.g_pi.NowViewPage);  //ver0.988 2010年6月20日

                //ツールチップを隠す。
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
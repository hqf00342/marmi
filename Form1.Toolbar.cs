using System;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // ツールバー *******************************************************************/

        private void toolStrip1_Resize(object sender, EventArgs e)
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

            Uty.WriteLine("ResizeTrackbar:{0}", trackbarWidth);

            toolStrip1.CanOverflow = false;
            if (g_trackbar != null) //起動時にエラーが出るので
            {
                if (trackbarWidth > 10)
                    g_trackbar.Width = trackbarWidth - 10;  //10はグリップの大きさ分ぐらい・・・
                else
                    g_trackbar.Width = 0;
            }
        }

        private void toolButtonLeft_Click(object sender, EventArgs e)
        {
            if (g_Config.isReplaceArrowButton)
                NavigateToForword();
            else
                NavigateToBack();
        }

        private void toolButtonRight_Click(object sender, EventArgs e)
        {
            if (g_Config.isReplaceArrowButton)
                NavigateToBack();
            else
                NavigateToForword();
        }

        private void toolButton_MouseHover(object sender, EventArgs e)
        {
            Statusbar_InfoLabel.Text = (string)((ToolStripButton)sender).Tag;

            //1クリック対応用に保持しておく
            g_hoverStripItem = sender;
        }

        private void toolButton_MouseLeave(object sender, EventArgs e)
        {
            //ファイル名表示に戻す
            setStatusbarFilename();

            //1クリック対応用に処理
            g_hoverStripItem = null;
        }

        // Zoomボタン関連 *********************************************************/

        private void toolStripButton_ZoomIn_Click(object sender, EventArgs e)
        {
            if (g_Config.isThumbnailView)
            {
                //サムネイル対応
                g_ThumbPanel.ThumbSizeZoomIn();
                g_ThumbPanel.Refresh();
                return;
            }

            //ZoomIn();
            PicPanel.ZoomIn();
        }

        private void toolStripButton_ZoomOut_Click(object sender, EventArgs e)
        {
            if (g_Config.isThumbnailView)
            {
                //サムネイル対応
                g_ThumbPanel.ThumbSizeZoomOut();
                g_ThumbPanel.Refresh();
                return;
            }

            //ZoomOut();
            PicPanel.ZoomOut();
        }

        private void toolStripButton_Zoom100_Click(object sender, EventArgs e)
        {
            //if (PicPanel.FittingRatio == 1.0f)
            //    PicPanel.isAutoFit = true;
            //else
            //    PicPanel.isAutoFit = false;
            PicPanel.isAutoFit = false;
            PicPanel.ZoomRatio = 1.0f;
            //PicPanel.Refresh();
            PicPanel.AjustViewAndShow();
        }

        private void toolStripButton_ZoomFit_Click(object sender, EventArgs e)
        {
            PicPanel.isAutoFit = true;
            //表示倍率の調整
            float r = PicPanel.FittingRatio;
            if (r > 1.0f && Form1.g_Config.noEnlargeOver100p)
                r = 1.0f;
            PicPanel.ZoomRatio = r;
            //PicPanel.Refresh();
            PicPanel.AjustViewAndShow();
        }

        private void toolStripButton_Rotate_Click(object sender, EventArgs e)
        {
            PicPanel.Rotate();
            PicPanel.Refresh();
        }

        // コントロールイベント *********************************************************/

        private void g_trackbar_ValueChanged(object sender, EventArgs e)
        {
            //Debug.WriteLine(g_trackbar.Value, "g_trackbar_ValueChanged");
            if (g_trackbar.Value != g_pi.NowViewPage)
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
            if (g_trackNaviPanel != null)
                g_trackNaviPanel.SetCenterItem(g_trackbar.Value);
        }

        private void g_trackbar_MouseDown(object sender, MouseEventArgs e)
        {
            if (g_trackNaviPanel == null)
                g_trackNaviPanel = new NaviBar3(g_pi);

            //g_n3.Parent = this;
            //if(!this.Controls.Contains(g_n3))
            //    this.Controls.Add(g_n3);

            if (!PicPanel.Controls.Contains(g_trackNaviPanel))
            {
                PicPanel.Controls.Add(g_trackNaviPanel);
                //Debug.WriteLine("add navigatebar");
            }

            //Navibarの位置を決める
            //Rectangle r = GetClientRectangle();
            Rectangle r = PicPanel.ClientRectangle;
            if (toolStrip1.Dock == DockStyle.Top)
            {
                if (g_Config.isFullScreen)
                    r.Y += toolStrip1.Height;
                //Debug.WriteLine(r, "add navigatebar");
                g_trackNaviPanel.OpenPanel(r, g_pi.NowViewPage);
            }
            else
            {
                //ToolStripが下にいる
                r.Y = r.Height - g_trackNaviPanel.Height;
                if (r.Y < 0) r.Y = 0;
                g_trackNaviPanel.OpenPanel(r, g_pi.NowViewPage);
            }

            g_trackbar_ValueChanged(null, null);

            //ツールチップの表示
            //g_toolTip.BringToFront();		//最前面に
            //g_toolTip.Show();
        }

        private void g_trackbar_MouseUp(object sender, MouseEventArgs e)
        {
            //ValueChanged()の代わりにこのイベントで処理

            //トラックバー用サムネイルがある場合は閉じる
            if (g_trackNaviPanel != null)
            {
                g_trackNaviPanel.ClosePanel();
                PicPanel.Controls.Remove(g_trackNaviPanel);
                Debug.WriteLine("remove navigatebar");
            }

            //マウスが離れたところで確定する
            if (g_trackbar.Value != g_pi.NowViewPage)
            {
                //ページ位置確定
                g_pi.NowViewPage = g_trackbar.Value;
                SetViewPage(g_pi.NowViewPage);  //ver0.988 2010年6月20日

                //ツールチップを隠す。
                //g_toolTip.Hide();
                //g_toolTip.Visible = false;
            }
        }

        private void g_trackbar_MouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
            Debug.WriteLine("g_trackbar_MouseWheel");
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}
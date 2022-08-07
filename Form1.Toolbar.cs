using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

/*
ツールバー
*/

namespace Marmi
{
    public partial class Form1 : Form
    {

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

        private async void ToolButtonLeft_Click(object sender, EventArgs e)
        {
            if (App.Config.IsReplaceArrowButton)
                await NavigateToForwordAsync();
            else
                await NavigateToBackAsync();
        }

        private async void ToolButtonRight_Click(object sender, EventArgs e)
        {
            if (App.Config.IsReplaceArrowButton)
                await NavigateToBackAsync();
            else
                await NavigateToForwordAsync();
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
            //選択アイテムを中央に表示
            if (_trackNaviPanel != null)
                _trackNaviPanel.SetCenterItem(_trackbar.Value);
        }

        private void Trackbar_MouseDown(object sender, MouseEventArgs e)
        {
            //Navibar3を生成
            if (_trackNaviPanel == null)
                _trackNaviPanel = new NaviBar3(App.g_pi);

            if (!PicPanel.Controls.Contains(_trackNaviPanel))
            {
                PicPanel.Controls.Add(_trackNaviPanel);
            }

            //Navibarの位置を決める
            Rectangle r = PicPanel.ClientRectangle;
            if (toolStrip1.Dock == DockStyle.Top)
            {
                if (App.Config.isFullScreen)
                    r.Y += toolStrip1.Height;
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }
            else
            {
                //ToolStripが下にいる
                r.Y = r.Height - _trackNaviPanel.Height;
                if (r.Y < 0) r.Y = 0;
                _trackNaviPanel.OpenPanel(r, App.g_pi.NowViewPage);
            }

            //現在のアイテムを中央に表示
            Trackbar_ValueChanged(null, null);
        }

        private async void Trackbar_MouseUp(object sender, MouseEventArgs e)
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
                await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010年6月20日
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
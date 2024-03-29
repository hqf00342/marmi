using System;
using System.Drawing;
using System.Windows.Forms;

/*
ツールバー
Trackbarも含む
*/

namespace Marmi
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// ツールバーの状態をコンフィグに合わせる
        /// </summary>
        private void UpdateToolbar()
        {
            //画面モードの状態反映
            toolbar_DualMode.Checked = ViewState.DualView;
            toolbar_FullScreen.Checked = ViewState.FullScreen;
            toolbar_Thumbnail.Checked = ViewState.ThumbnailView;

            //Sidebar
            toolbar_Sidebar.Checked = _sidebar.Visible;

            if (App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //ファイルを閲覧していない場合のツールバー
                _trackbar.Enabled = false;
                toolbar_Left.Enabled = false;
                toolbar_Right.Enabled = false;
                toolbar_Thumbnail.Enabled = false;
                toolbar_Zoom100.Checked = false;
                toolbar_ZoomFit.Checked = false;
                toolbar_ZoomOut.Enabled = false;
                toolbar_ZoomIn.Enabled = false;
                toolbar_Zoom100.Enabled = false;
                toolbar_ZoomFit.Enabled = false;
                toolbar_Favorite.Enabled = false;
                toolbar_Rotate.Enabled = false;
                return;
            }
            else
            {
                //サムネイルボタン
                toolbar_Thumbnail.Enabled = true;

                if (ViewState.ThumbnailView)
                {
                    //サムネイル表示中
                    toolbar_Left.Enabled = false;
                    toolbar_Right.Enabled = false;
                    toolbar_Zoom100.Enabled = false;
                    toolbar_ZoomFit.Enabled = false;
                    toolbar_Favorite.Enabled = false;
                    toolbar_Sidebar.Enabled = false;
                }
                else
                {
                    //通常表示中
                    toolbar_ZoomIn.Enabled = true;
                    toolbar_ZoomOut.Enabled = true;
                    toolbar_Zoom100.Enabled = true;
                    toolbar_ZoomFit.Enabled = true;
                    toolbar_Favorite.Enabled = true;
                    toolbar_Sidebar.Enabled = true;
                    toolbar_Rotate.Enabled = true;

                    //左右ボタンの有効無効
                    if (App.Config.General.ReplaceArrowButton)
                    {
                        //入れ替え
                        toolbar_Left.Enabled = !IsLastPageViewing();      //最終ページチェック
                        toolbar_Right.Enabled = (bool)(App.g_pi.NowViewPage != 0);    //先頭ページチェック
                    }
                    else
                    {
                        toolbar_Left.Enabled = (bool)(App.g_pi.NowViewPage != 0); //先頭ページチェック
                        toolbar_Right.Enabled = !IsLastPageViewing();     //最終ページチェック
                    }

                    //100%ズーム
                    toolbar_Zoom100.Checked = PicPanel.IsScreen100p;

                    //画面フィットズーム
                    toolbar_ZoomFit.Checked = PicPanel.IsFitToScreen;

                    //Favorite
                    if (App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark)
                    {
                        toolbar_Favorite.Checked = true;
                    }
                    else if (g_viewPages == 2
                        && App.g_pi.NowViewPage < App.g_pi.Items.Count - 1      //ver1.69 最終ページより前チェック
                        && App.g_pi.Items[App.g_pi.NowViewPage + 1].IsBookMark) //
                    {
                        toolbar_Favorite.Checked = true;
                    }
                    else
                    {
                        toolbar_Favorite.Checked = false;
                    }

                    //Sidebar
                    toolbar_Sidebar.Checked = _sidebar.Visible;
                }

                //TrackBar
                //ここで直すとUIが遅くなる。
                //g_trackbar.Value = g_pi.NowViewPage;
            }
        }

        /// <summary>
        /// ver1.67 ツールバーの文字を表示/非表示する。
        /// </summary>
        private void SetToolbarString()
        {
            if (App.Config.General.HideToolbarString)
            {
                toolbar_Close.DisplayStyle = ToolStripItemDisplayStyle.Image;
                toolbar_FullScreen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            }
            else
            {
                toolbar_Close.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                toolbar_FullScreen.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }
        }

        /// <summary>
        /// TrackBarを初期化。画像枚数を反映させる
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
        /// ツールバーリサイズイベント処理
        /// リサイズ中は何もせず、リサイズ完了したらTrackBarサイズを変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStrip1_Resize(object sender, EventArgs e)
        {
            //リサイズドラッグ中は表示させない
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                return;

            //TrackBarサイズを更新
            //最大化直後に呼ばれるみたい
            ResizeTrackBar();
        }

        /// <summary>
        /// トラックバーサイズを計算し反映
        /// </summary>
        private void ResizeTrackBar()
        {
            //g_trackbarのサイズを計算
            //ツールバーの空き領域サイズを計算する。
            int trackbarWidth = toolStrip1.Width;
            foreach (ToolStripItem o in toolStrip1.Items)
            {
                if (o.Name != "MarmiTrackBar")
                    trackbarWidth -= o.Width;
            }

            toolStrip1.CanOverflow = false;
            if (_trackbar != null) //起動時にエラーが出るので
            {
                //10はグリップの大きさ分ぐらい・・・
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

            //フォームにフォーカスがなくともこのイベントは来る
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
            if (ViewState.ThumbnailView)
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
            if (ViewState.ThumbnailView)
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
            PicPanel.IsAutoFit = false;
            PicPanel.ZoomRatio = 1.0f;
            PicPanel.AjustViewAndShow();
        }

        private void ToolStripButton_ZoomFit_Click(object sender, EventArgs e)
        {
            PicPanel.IsAutoFit = true;
            //表示倍率の調整
            float r = PicPanel.JustFitRatio;
            if (r > 1.0f && App.Config.View.ProhigitExpansionOver100p)
                r = 1.0f;
            PicPanel.ZoomRatio = r;
            PicPanel.AjustViewAndShow();
        }

        private void ToolStripButton_Rotate_Click(object sender, EventArgs e)
        {
            PicPanel.Rotate(90);
            PicPanel.Refresh();

            //画像情報に回転情報を加える
            var info = App.g_pi.Items[App.g_pi.NowViewPage];
            info.Rotate += 90;
        }

        // コントロールイベント *********************************************************/

        private void Trackbar_ValueChanged(object sender, EventArgs e)
        {
            //選択アイテムを中央に表示
            _trackNaviPanel?.SetCenterItem(_trackbar.Value);
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
                if (ViewState.FullScreen)
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
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}
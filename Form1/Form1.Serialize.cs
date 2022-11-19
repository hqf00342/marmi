using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // ユーティリティ系：Configファイル *********************************************/

        /// <summary>
        /// ロードしたコンフィグをアプリに適用していく
        /// </summary>
        private void ApplyConfigToWindow()
        {
            //背景色
            PicPanel.BackColor = App.Config.General.BackColor;

            //メニューバー、ツールバー、ステータスバー
            menuStrip1.Visible = ViewState.VisibleMenubar;
            toolStrip1.Visible = ViewState.VisibleToolBar;
            statusbar.Visible = ViewState.VisibleStatusBar;

            //ナビバー
            _sidebar.Visible = ViewState.VisibleSidebar;

            //ver1.77 画面位置決定：デュアルディスプレイ対応
            //デュアルディスプレイを考慮して配置
            SetFormPosition();

            //ver1.77全画面モード対応
            if (App.Config.General.SaveFullScreenMode && ViewState.FullScreen)
                SetFullScreen(true);

            //2枚表示
            toolButtonDualMode.Checked = ViewState.DualView;

            //MRU反映
            //オープンするときに実施するのでコメントアウト
            //UpdateMruMenuListUI();

            //再帰検索
            Menu_OptionRecurseDir.Checked = App.Config.RecurseSearchDir;

            //左右矢印交換対応
            if (App.Config.General.ReplaceArrowButton)
            {
                toolButtonLeft.Tag = "次のページに移動します";
                toolButtonLeft.Text = "次へ";
                toolButtonRight.Tag = "前のページに移動します";
                toolButtonRight.Text = "前へ";
            }
            else
            {
                toolButtonLeft.Tag = "前のページに移動します";
                toolButtonLeft.Text = "前へ";
                toolButtonRight.Tag = "次のページに移動します";
                toolButtonRight.Text = "次へ";
            }

            //サムネイル関連
            if (_thumbPanel != null)
            {
                _thumbPanel.BackColor = App.Config.Thumbnail.ThumbnailBackColor;
                _thumbPanel.SetThumbnailSize(App.Config.Thumbnail.ThumbnailSize);
                _thumbPanel.SetFont(App.Config.Thumbnail.ThumbnailFont, App.Config.Thumbnail.ThumbnailFontColor);
            }

            //キーコンフィグ反映
            SetKeyConfig2();

            //ver1.65 ツールバーの文字はすぐ反映
            SetToolbarString();
            ResizeTrackBar();

            //サムネイルビュー中ならすぐに再描写
            if (ViewState.ThumbnailView)
            {
                _thumbPanel.ReDraw();
            }

            //ver1.79 ScreenCacheをクリアする。
            //ScreenCache.Clear();
        }

        /// <summary>
        /// デュアルディスプレイ対応のフォーム位置指定
        /// 2画面をまたがらないようにする。
        /// </summary>
        private void SetFormPosition()
        {
            //デュアルディスプレイ対応
            //左上が画面内にいるスクリーンを探す
            foreach (var scr in Screen.AllScreens)
            {
                if (scr.WorkingArea.Contains(App.Config.WindowPos))
                {
                    SetFormPosLocation2(scr);
                    return;
                }
            }

            //ここに来た時はどのディスプレイにも属さなかったとき。
            //->プライマリに行ってもらう
            //setFormPosLocation2(Screen.PrimaryScreen);
            //return;
            //どの画面にも属さないので一番近いディスプレイを探す
            var pos = App.Config.WindowPos;
            double distance = double.MaxValue;
            int target = 0;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                var scr = Screen.AllScreens[i];
                //簡易計算
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
        /// App.Configの内容から表示位置を決定する
        /// デュアルディスプレイに対応
        /// 画面外に表示させない。
        /// </summary>
        /// <param name="scr"></param>
        private void SetFormPosLocation2(Screen scr)
        {
            //このスクリーンのワーキングエリアをチェックする
            var dispRect = scr.WorkingArea;

            var winSize = App.Config.WindowSize;
            var winPos = App.Config.WindowPos;

            //ver1.77 ウィンドウサイズの調整(小さすぎるとき）
            if (winSize.Width < this.MinimumSize.Width)
                winSize.Width = this.MinimumSize.Width;
            if (winSize.Height < this.MinimumSize.Height)
                winSize.Height = this.MinimumSize.Height;

            //ウィンドウサイズの調整(大きすぎるとき）
            if (dispRect.Width < winSize.Width)
            {
                winPos.X = 0;
                winSize.Width = dispRect.Width;
            }
            if (dispRect.Height < winSize.Height)
            {
                winPos.Y = 0;
                winSize.Height = dispRect.Height;
            }

            //ウィンドウ位置の調整（画面外:マイナス方向）
            if (winPos.X < dispRect.X)
                winPos.X = dispRect.X;
            if (winPos.Y < dispRect.Y)
                winPos.Y = dispRect.Y;

            //右下も画面外に表示させない
            var right = winPos.X + winSize.Width;
            var bottom = winPos.Y + winSize.Height;
            if (right > dispRect.X + dispRect.Width)
                winPos.X = dispRect.X + dispRect.Width - winSize.Width;
            if (bottom > dispRect.Y + dispRect.Height)
                winPos.Y = dispRect.Y + dispRect.Height - winSize.Height;

            //中央表示強制かどうか
            if (App.Config.General.CenteredAtStart)
            {
                winPos.X = dispRect.X + (dispRect.Width - winSize.Width) / 2;
                winPos.Y = dispRect.Y + (dispRect.Height - winSize.Height) / 2;
            }
            //サイズの適用
            App.Config.WindowSize = winSize;
            App.Config.WindowPos = winPos;
            this.Size = winSize;
            this.Location = winPos;
        }
    }
}
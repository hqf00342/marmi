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
            //バー関連
            menuStrip1.Visible = App.Config.VisibleMenubar;
            toolStrip1.Visible = App.Config.VisibleToolBar;
            statusbar.Visible = App.Config.VisibleStatusBar;

            //ナビバー
            //g_Sidebar.SetSizeAndDock(GetClientRectangle());
            _sidebar.Visible = App.Config.VisibleNavibar;

            //ver1.77 画面位置決定：デュアルディスプレイ対応
            if (App.Config.SimpleCalcForWindowLocation)
            {
                //簡易：as is
                this.Size = App.Config.windowSize;
                this.Location = App.Config.windowLocation;
            }
            else
                SetFormPosLocation();

            //ver1.77全画面モード対応
            if (App.Config.SaveFullScreenMode && App.Config.isFullScreen)
                SetFullScreen(true);

            //2枚表示
            toolButtonDualMode.Checked = App.Config.DualView;

            //MRU反映
            //オープンするときに実施するのでコメントアウト
            //UpdateMruMenuListUI();

            //再帰検索
            Menu_OptionRecurseDir.Checked = App.Config.IsRecurseSearchDir;

            //左右矢印交換対応
            if (App.Config.IsReplaceArrowButton)
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
                _thumbPanel.BackColor = App.Config.ThumbnailBackColor;
                _thumbPanel.CalcThumbboxSize(App.Config.ThumbnailSize);
                _thumbPanel.SetFont(App.Config.ThumbnailFont, App.Config.ThumbnailFontColor);
            }
        }

        private void SetFormPosLocation()
        {
            //デュアルディスプレイ対応
            //左上が画面内にいるスクリーンを探す
            foreach (var scr in Screen.AllScreens)
            {
                if (scr.WorkingArea.Contains(App.Config.windowLocation))
                {
                    SetFormPosLocation2(scr);
                    return;
                }
            }
            //ここに来た時はどのディスプレイにも属さなかったとき

            //どの画面にも属さないのでプライマリに行ってもらう
            //setFormPosLocation2(Screen.PrimaryScreen);
            //return;
            //どの画面にも属さないので一番近いディスプレイを探す
            var pos = App.Config.windowLocation;
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
            var disp = scr.WorkingArea;

            //ver1.77 ウィンドウサイズの調整(小さすぎるとき）
            if (App.Config.windowSize.Width < this.MinimumSize.Width)
                App.Config.windowSize.Width = this.MinimumSize.Width;
            if (App.Config.windowSize.Height < this.MinimumSize.Height)
                App.Config.windowSize.Height = this.MinimumSize.Height;

            //ウィンドウサイズの調整(大きすぎるとき）
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

            //ウィンドウ位置の調整（画面外:マイナス方向）
            if (App.Config.windowLocation.X < disp.X)
                App.Config.windowLocation.X = disp.X;
            if (App.Config.windowLocation.Y < disp.Y)
                App.Config.windowLocation.Y = disp.Y;

            //右下も画面外に表示させない
            var right = App.Config.windowLocation.X + App.Config.windowSize.Width;
            var bottom = App.Config.windowLocation.Y + App.Config.windowSize.Height;
            if (right > disp.X + disp.Width)
                App.Config.windowLocation.X = disp.X + disp.Width - App.Config.windowSize.Width;
            if (bottom > disp.Y + disp.Height)
                App.Config.windowLocation.Y = disp.Y + disp.Height - App.Config.windowSize.Height;

            //中央表示強制かどうか
            if (App.Config.IsWindowPosCenter)
            {
                App.Config.windowLocation.X = disp.X + (disp.Width - App.Config.windowSize.Width) / 2;
                App.Config.windowLocation.Y = disp.Y + (disp.Height - App.Config.windowSize.Height) / 2;
            }
            //サイズの適用
            this.Size = App.Config.windowSize;
            //強制中央表示
            this.Location = App.Config.windowLocation;
        }
    }
}
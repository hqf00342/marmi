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
        private void ApplySettingToApplication()
        {
            //バー関連
            menuStrip1.Visible = g_Config.visibleMenubar;
            toolStrip1.Visible = g_Config.visibleToolBar;
            statusbar.Visible = g_Config.visibleStatusBar;

            //ナビバー
            //g_Sidebar.SetSizeAndDock(GetClientRectangle());
            g_Sidebar.Visible = g_Config.visibleNavibar;

            //ver1.77 画面位置決定：デュアルディスプレイ対応
            if (g_Config.simpleCalcForWindowLocation)
            {
                //簡易：as is
                this.Size = g_Config.windowSize;
                this.Location = g_Config.windowLocation;
            }
            else
                SetFormPosLocation();

            //ver1.77全画面モード対応
            if (g_Config.saveFullScreenMode && g_Config.isFullScreen)
                SetFullScreen(true);

            //2枚表示
            toolButtonDualMode.Checked = g_Config.dualView;

            //MRU反映
            //オープンするときに実施するのでコメントアウト
            //UpdateMruMenuListUI();

            //再帰検索
            Menu_OptionRecurseDir.Checked = g_Config.isRecurseSearchDir;

            //左右矢印交換対応
            if (g_Config.isReplaceArrowButton)
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
            if (g_ThumbPanel != null)
            {
                g_ThumbPanel.BackColor = g_Config.ThumbnailBackColor;
                g_ThumbPanel.SetThumbnailSize(g_Config.ThumbnailSize);
                g_ThumbPanel.SetFont(g_Config.ThumbnailFont, g_Config.ThumbnailFontColor);
            }
        }

        private void SetFormPosLocation()
        {
            //デュアルディスプレイ対応
            //左上が画面内にいるスクリーンを探す
            foreach (var scr in Screen.AllScreens)
            {
                if (scr.WorkingArea.Contains(g_Config.windowLocation))
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
            var pos = g_Config.windowLocation;
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
        /// g_Configの内容から表示位置を決定する
        /// デュアルディスプレイに対応
        /// 画面外に表示させない。
        /// </summary>
        /// <param name="scr"></param>
        private void SetFormPosLocation2(Screen scr)
        {
            //このスクリーンのワーキングエリアをチェックする
            var disp = scr.WorkingArea;

            //ver1.77 ウィンドウサイズの調整(小さすぎるとき）
            if (g_Config.windowSize.Width < this.MinimumSize.Width)
                g_Config.windowSize.Width = this.MinimumSize.Width;
            if (g_Config.windowSize.Height < this.MinimumSize.Height)
                g_Config.windowSize.Height = this.MinimumSize.Height;

            //ウィンドウサイズの調整(大きすぎるとき）
            if (disp.Width < g_Config.windowSize.Width)
            {
                g_Config.windowLocation.X = 0;
                g_Config.windowSize.Width = disp.Width;
            }
            if (disp.Height < g_Config.windowSize.Height)
            {
                g_Config.windowLocation.Y = 0;
                g_Config.windowSize.Height = disp.Height;
            }

            //ウィンドウ位置の調整（画面外:マイナス方向）
            if (g_Config.windowLocation.X < disp.X)
                g_Config.windowLocation.X = disp.X;
            if (g_Config.windowLocation.Y < disp.Y)
                g_Config.windowLocation.Y = disp.Y;

            //右下も画面外に表示させない
            var right = g_Config.windowLocation.X + g_Config.windowSize.Width;
            var bottom = g_Config.windowLocation.Y + g_Config.windowSize.Height;
            if (right > disp.X + disp.Width)
                g_Config.windowLocation.X = disp.X + disp.Width - g_Config.windowSize.Width;
            if (bottom > disp.Y + disp.Height)
                g_Config.windowLocation.Y = disp.Y + disp.Height - g_Config.windowSize.Height;

            //中央表示強制かどうか
            if (g_Config.isWindowPosCenter)
            {
                g_Config.windowLocation.X = disp.X + (disp.Width - g_Config.windowSize.Width) / 2;
                g_Config.windowLocation.Y = disp.Y + (disp.Height - g_Config.windowSize.Height) / 2;
            }
            //サイズの適用
            this.Size = g_Config.windowSize;
            //強制中央表示
            this.Location = g_Config.windowLocation;
        }
    }
}
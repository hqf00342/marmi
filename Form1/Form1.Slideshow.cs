using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        //ver1.35 スライドショータイマー
        private readonly Timer SlideShowTimer = new Timer();

        //スライドショー中かどうか
        public bool IsSlideShow => SlideShowTimer.Enabled;

        private void Menu_SlideShow_Click(object sender, EventArgs e)
        {
            if (SlideShowTimer.Enabled)
            {
                StopSlideShow();
            }
            else
            {
                if (App.g_pi.Items.Count == 0)
                    return;

                _clearPanel.ShowAndClose(
                    "スライドショーを開始します。\r\nマウスクリックまたはキー入力で終了します。",
                    1500);
                SlideShowTimer.Interval = App.Config.SlideshowTime;
                SlideShowTimer.Start();
            }
        }

        private async void SlideShowTimer_Tick(object sender, EventArgs e)
        {
            if (await GetNextPageIndexAsync(App.g_pi.NowViewPage) == -1)
            {
                StopSlideShow();
            }
            else
            {
                await NavigateToForwordAsync();
            }
        }

        private void StopSlideShow()
        {
            if (SlideShowTimer.Enabled)
            {
                //スライドショーを終了させる
                SlideShowTimer.Stop();
                _clearPanel.ShowAndClose("スライドショーを終了しました", 1500);
            }
        }
    }
}
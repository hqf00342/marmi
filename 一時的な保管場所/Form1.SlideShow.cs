/****************************************************************************
Form1.cs
スライドショー関連
****************************************************************************/

using System;

namespace Marmi;

public partial class Form1
{
    #region スライドショー

    private void Menu_SlideShow_Click(object sender, EventArgs e)
    {
        if (SlideShowTimer.Enabled)
        {
            StopSlideShow();
        }
        else
        {
            if (App.TotalPages == 0)
                return;

            _clearPanel.ShowAndClose(
                "スライドショーを開始します。\r\nマウスクリックまたはキー入力で終了します。",
                1500);
            SlideShowTimer.Interval = App.Config.SlideShowTime;
            //SlideShowTimer.Tick += new EventHandler(SlideShowTimer_Tick);
            SlideShowTimer.Start();
        }
    }

    private void SlideShowTimer_Tick(object sender, EventArgs e)
    {
        if (GetNextPageIndex(App.CurrentPage) == -1)
            StopSlideShow();
        else
            NavigateToForword();
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

    #endregion スライドショー
}
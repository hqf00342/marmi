using System;

using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // ステータスバー関連 ***********************************************************/

        private void setStatusbarPages()
        {
            if (g_pi.Items.Count == 0)
            {
                Statusbar_PageLabel.Text = "0/0";
            }
            else
            {
                if (g_viewPages == 1)
                    Statusbar_PageLabel.Text = String.Format(
                        "{0} / {1}pages",
                        g_pi.NowViewPage + 1,
                        g_pi.Items.Count);
                else
                    Statusbar_PageLabel.Text = String.Format(
                        "[{0},{1}] / {2}pages",
                        g_pi.NowViewPage + 1,
                        g_pi.NowViewPage + 2,
                        g_pi.Items.Count);
            }
        }

        private void setStatubarRatio(float ratio)
        {
            string s = string.Format("{0}%", Math.Floor(ratio * 100));
            Statusbar_ratio.Text = s;
        }

        public void setStatubarRatio(string s)
        {
#if DEBUG
            s = s + " : " + Uty.GetUsedMemory();
#endif
            // 指定した文字列を記述
            Statusbar_ratio.Text = s;
        }

        private void setStatusbarFilename()
        {
            if (g_pi.Items.Count == 0)
            {
                Statusbar_InfoLabel.Text = "";
                return;
            }

            //if (g_viewPages == 1)
            //ver1.81 最終ページのときも1ページだけに変更
            if (g_viewPages == 1 || g_pi.NowViewPage == g_pi.Items.Count - 1)
            {
                Statusbar_InfoLabel.Text = g_pi.Items[g_pi.NowViewPage].filename;
            }
            else
            {
                Statusbar_InfoLabel.Text =
                    g_pi.Items[g_pi.NowViewPage + 1].filename
                    + "  |  "
                    + g_pi.Items[g_pi.NowViewPage].filename;
            }
        }

        public void setStatusbarInfo(string s)
        {
            Statusbar_InfoLabel.Text = s;
        }

        private void UpdateStatusbar()
        {
            //StatusBar ページ更新
            setStatusbarPages();
            //StatusBar ファイル名更新
            setStatusbarFilename();
            //setStatubarRatio(g_viewRatio);	//StatusBar 表示比率更新
        }
    }
}
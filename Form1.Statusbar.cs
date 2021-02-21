using System;

using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // �X�e�[�^�X�o�[�֘A ***********************************************************/

        private void setStatusbarPages()
        {
            if (App.g_pi.Items.Count == 0)
            {
                Statusbar_PageLabel.Text = "0/0";
            }
            else
            {
                if (g_viewPages == 1)
                    Statusbar_PageLabel.Text = String.Format(
                        "{0} / {1}pages",
                        App.g_pi.NowViewPage + 1,
                        App.g_pi.Items.Count);
                else
                    Statusbar_PageLabel.Text = String.Format(
                        "[{0},{1}] / {2}pages",
                        App.g_pi.NowViewPage + 1,
                        App.g_pi.NowViewPage + 2,
                        App.g_pi.Items.Count);
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
            // �w�肵����������L�q
            Statusbar_ratio.Text = s;
        }

        private void setStatusbarFilename()
        {
            if (App.g_pi.Items.Count == 0)
            {
                Statusbar_InfoLabel.Text = "";
                return;
            }

            //if (g_viewPages == 1)
            //ver1.81 �ŏI�y�[�W�̂Ƃ���1�y�[�W�����ɕύX
            if (g_viewPages == 1 || App.g_pi.NowViewPage == App.g_pi.Items.Count - 1)
            {
                Statusbar_InfoLabel.Text = App.g_pi.Items[App.g_pi.NowViewPage].filename;
            }
            else
            {
                Statusbar_InfoLabel.Text =
                    App.g_pi.Items[App.g_pi.NowViewPage + 1].filename
                    + "  |  "
                    + App.g_pi.Items[App.g_pi.NowViewPage].filename;
            }
        }

        public void SetStatusbarInfo(string s)
        {
            Statusbar_InfoLabel.Text = s;
            Application.DoEvents();
        }

        private void UpdateStatusbar()
        {
            //StatusBar �y�[�W�X�V
            setStatusbarPages();
            //StatusBar �t�@�C�����X�V
            setStatusbarFilename();
            //setStatubarRatio(g_viewRatio);	//StatusBar �\���䗦�X�V
        }
    }
}
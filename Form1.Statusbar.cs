using System;

using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        private void SetStatusbarPages()
        {
            if (App.g_pi.Items.Count == 0)
            {
                Statusbar_PageLabel.Text = "0/0";
            }
            else
            {
                Statusbar_PageLabel.Text = (g_viewPages == 1)
                    ? $"{App.g_pi.NowViewPage + 1} / {App.g_pi.Items.Count}pages"
                    : $"[{App.g_pi.NowViewPage + 1},{App.g_pi.NowViewPage + 2}] / {App.g_pi.Items.Count}pages";
            }
        }

        public void SetStatubarRatio(string s)
        {
            Statusbar_ratio.Text = s + " : " + Uty.GetUsedMemory();
        }

        private void SetStatusbarFilename()
        {
            if (App.g_pi.Items.Count == 0)
            {
                Statusbar_InfoLabel.Text = "";
                return;
            }

            if (g_viewPages == 1 || App.g_pi.NowViewPage == App.g_pi.Items.Count - 1)
            {
                //1�y�[�W�̂�
                Statusbar_InfoLabel.Text = App.g_pi.Items[App.g_pi.NowViewPage].Filename;
            }
            else
            {
                //2�y�[�W
                Statusbar_InfoLabel.Text =
                    App.g_pi.Items[App.g_pi.NowViewPage + 1].Filename
                    + "  |  "
                    + App.g_pi.Items[App.g_pi.NowViewPage].Filename;
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
            SetStatusbarPages();
            //StatusBar �t�@�C�����X�V
            SetStatusbarFilename();
            //setStatubarRatio(g_viewRatio);	//StatusBar �\���䗦�X�V
        }
    }
}
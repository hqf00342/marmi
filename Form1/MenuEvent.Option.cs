using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        private void Menu_Option_Click(object sender, EventArgs e)
        {
            var fo = new OptionForm();
            fo.LoadConfig(App.Config);
            if (fo.ShowDialog() == DialogResult.OK)
            {
                //App.Configに取り込み
                fo.SaveConfig(ref App.Config);
                //App.Configをウィンドウに反映
                ApplyConfigToWindow();
            }
        }

        private void Menu_RecurseDir_Click(object sender, EventArgs e)
        {
            App.Config.RecurseSearchDir = !App.Config.RecurseSearchDir;
            Menu_OptionRecurseDir.Checked = App.Config.RecurseSearchDir;
        }

        private void Menu_keepMagnification_Click(object sender, EventArgs e)
        {
            App.Config.KeepMagnification = !App.Config.KeepMagnification;
        }

        private void Menu_UseBicubic_Click(object sender, EventArgs e)
        {
            App.Config.View.DotByDotZoom = !App.Config.View.DotByDotZoom;
        }

        private async void Menu_DontEnlargeOver100percent_Click(object sender, EventArgs e)
        {
            App.Config.View.ProhigitExpansionOver100p = !App.Config.View.ProhigitExpansionOver100p;
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }
    }
}
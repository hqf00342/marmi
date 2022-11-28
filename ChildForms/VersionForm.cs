using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Marmi
{
    public partial class VersionForm : Form
    {
        public VersionForm()
        {
            InitializeComponent();
        }

        private void Version_Load(object sender, EventArgs e)
        {
            //ÉoÅ[ÉWÉáÉìèÓïÒÇÃéÊìæ
            System.Diagnostics.FileVersionInfo fv =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);

            string buildconf = fv.IsDebug ? "Debug" : "Release";

            label1.Text = $"{fv.ProductName} ver{fv.ProductVersion}({buildconf})\n"
                        + $"{GitInfo.BranchName}-{GitInfo.CommitId}";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }
    }
}
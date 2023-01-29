using System;
using System.Diagnostics;
using System.Reflection;
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
            //ƒo[ƒWƒ‡ƒ“î•ñæ“¾
            var fv = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var buildenv = fv.IsDebug ? "Debug" : "Release";

            label1.Text = $"{fv.ProductName} ver{fv.ProductVersion}({buildenv})\n"
                        + $"{GitInfo.BranchName}-{GitInfo.CommitId}";
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(((LinkLabel)sender).Text);
        }
    }
}
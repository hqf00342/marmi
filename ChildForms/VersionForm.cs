using Mii;
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
            //ÉoÅ[ÉWÉáÉìèÓïÒÇÃéÊìæ
            var fv = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            var configuration = fv.IsDebug ? "Debug" : "Release";

            Debug.Assert(label1 != null, "label1 != null");
            label1.Text = $"{fv.ProductName} ver{fv.ProductVersion} [{configuration}]";
            label_gitinfo.Text = $"({GitInfo.BranchName}-{GitInfo.CommitId})";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }
    }
}
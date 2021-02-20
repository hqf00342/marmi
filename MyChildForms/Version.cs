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
			//ƒo[ƒWƒ‡ƒ“î•ñ‚Ìæ“¾
			System.Diagnostics.FileVersionInfo fv =
				System.Diagnostics.FileVersionInfo.GetVersionInfo(
					System.Reflection.Assembly.GetExecutingAssembly().Location);

			string buildInfo = fv.IsDebug ? "(Debug)" : "(Release)";

			Debug.Assert(label1 != null, "label1 != null");
			label1.Text = string.Format(
				"{0} ver{1} {2}",
				fv.ProductName,
				fv.ProductVersion,
				buildInfo);

		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
		}
	}
}
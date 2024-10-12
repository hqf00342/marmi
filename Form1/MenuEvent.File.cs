using System;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        private async void Menu_FileOpen_Click(object sender, EventArgs e)
        {
            await OpenDialog();
        }

        private void Menu_SaveThumbnail_Click(object sender, EventArgs e)
        {
            _thumbPanel.Location = GetClientRectangle().Location;
            _thumbPanel.Size = GetClientRectangle().Size;
            _thumbPanel.Parent = this;
            var form = new SaveThumbnailForm(App.g_pi.Items, App.g_pi.PackageName);
            form.ShowDialog(this);
        }

        private void Menu_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void Menu_ClearMRU_Click(object sender, EventArgs e)
        {
            App.Config.Mru.Clear();
            //for (int i = 0; i < App.Config.mru.Length; i++)
            //{
            //    App.Config.mru[i] = null;
            //}
        }

        private void Menu_File_DropDownOpening(object sender, EventArgs e)
        {
            //MRUを追加
            UpdateMruMenuListUI();

            //ファイルを閲覧していない場合,サムネイル保存を無効にする
            MenuItem_FileSaveThumbnail.Enabled = (App.g_pi?.Items?.Count > 0);
        }

        private async void OnClickMRUMenu(object sender, EventArgs e)
        {
            var filename = ((ToolStripDropDownItem)sender).Text;

            if (File.Exists(filename) || Directory.Exists(filename))
            {
                await StartAsync(new string[] { filename });
            }
            else
            {
                MessageBox.Show($"ファイルが見つかりませんでした\n{filename}", "ファイルオープンエラー");

                //MRUリストから削除
                var target = App.Config.Mru.Find(a => a.Name == filename);
                if (target != null)
                {
                    App.Config.Mru.Remove(target);
                }
            }
        }
    }
}
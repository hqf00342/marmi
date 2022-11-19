using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Marmi
{
    public partial class OptionForm : Form
    {
        //static List<KeyConfig> keyConfigList = new List<KeyConfig>();
        private bool KeyDuplicationError = false;

        private AppGlobalConfig _config = null;

        public OptionForm()
        {
            InitializeComponent();
        }

        public void LoadConfig(AppGlobalConfig set)
        {
            _config = set.Clone();

            generalConfigBindingSource.DataSource = _config.General;
            advanceConfigBindingSource.DataSource = _config.Advance;
            loupeConfigBindingSource.DataSource = _config.Loupe;
            mouseConfigBindingSource.DataSource = _config.Mouse;
            thumbnailConfigBindingSource.DataSource = _config.Thumbnail;
            viewConfigBindingSource.DataSource = _config.View;
            keyConfigBindingSource.DataSource = _config.Keys;

            //高度な設定タブ
            bStopPaintingAtResize.Checked = set.StopPaintingAtResize; //リサイズ描写
        }


        public void SaveConfig(ref AppGlobalConfig set)
        {
            set.General = _config.General;
            set.Advance = _config.Advance;
            set.Loupe = _config.Loupe;
            set.Mouse = _config.Mouse;
            set.Thumbnail= _config.Thumbnail;
            set.View= _config.View;
            set.Keys= _config.Keys;

            //高度な設定タブ
            set.StopPaintingAtResize = bStopPaintingAtResize.Checked;

        }


        private void InitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "全ての設定が初期値に戻りますが実行しますか？",
                    "確認",
                    MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                LoadConfig(new AppGlobalConfig());      //初期値を作り出す
                this.Refresh();
            }
        }

        private void SaveConfig_CheckedChanged(object sender, EventArgs e)
        {
            //全てのTabPage1内のアイテムの動作設定
            //設定を保存するときはEnable、そうでないときはDisable
            foreach (Control o in General.Controls)
            {
                switch (o.Name)
                {
                    case "bSaveConfig":
                        break;

                    case "loupeUserSetting":
                        break;

                    default:
                        o.Enabled = bSaveConfig.Checked;
                        break;
                }
            }
        }

        private void PictureBoxBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox_BackColor.BackColor = colorDialog1.Color;
            }
        }

        private void TextBox1_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog(this) == DialogResult.OK)
            {
                linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            }
        }

        private void ThumbnailFontColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ThumbnailFontColor.BackColor = colorDialog1.Color;
            }
        }

        private void ThumbnailBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ThumbnailBackColor.BackColor = colorDialog1.Color;
            }
        }

        private void OnFocus_Enter(object sender, EventArgs e)
        {
            HelpBox.Text = (string)(((Control)sender).Tag);
            //toolTip1.Show((string)(((Control)sender).Tag), this);
            toolTip1.Show("hai", this, 5000);
            toolTip1.SetToolTip((Control)sender, (string)((Control)sender).Tag);
        }

        //ver1.81 KeyAccelerator 利用に伴い Validating に移行
        /// <summary>
        /// キー重複チェックルーチン
        /// コントロールの値を比較する
        /// </summary>
        /// <returns>重複していた場合はtrue</returns>
        //private bool CheckKeyDuplicate()
        //{
        //	//キーコンフィグに重複がないことをチェック
        //	List<string> checkkey = new List<string>();

        //	foreach (Control c in keyConfigGroupBox.Controls)
        //	{
        //		if (c is ComboBox)
        //		{
        //			if (c.Text.Contains("なし"))
        //				continue;
        //			if (checkkey.Contains(c.Text))
        //				return true;
        //			else
        //				checkkey.Add(c.Text);
        //		}
        //	}
        //	return false;
        //}

        /// <summary>
        /// このままフォームを閉じていいかチェック
        /// ver1.21のキーコンフィグ重複チェックのため追加
        /// </summary>
        private void FormOption_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (KeyDuplicationError)
                e.Cancel = true;
        }

        /// <summary>
        /// OKボタンを押した際に不具合がなかったかチェック
        /// ver1.21ではキーコンフィグ重複チェック
        /// </summary>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            //if (CheckKeyDuplicate())
            //{
            //	MessageBox.Show("キー設定が重複しています");
            //	KeyDuplicationError = true;
            //}
            //else
            KeyDuplicationError = false;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //Cancelボタンを押した時はエラー無し
            KeyDuplicationError = false;
        }

        private void RadioRightScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //クリック画面表示を更新
            pictureBoxRightScr.Image = Properties.Resources.ScrNext;
            pictureBoxLeftScr.Image = Properties.Resources.ScrPrev;
        }

        private void RadioLeftScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //クリック画面表示を更新
            pictureBoxRightScr.Image = Properties.Resources.ScrPrev;
            pictureBoxLeftScr.Image = Properties.Resources.ScrNext;
        }

        private void PictureBoxRightScr_Click(object sender, EventArgs e)
        {
            //ラジオボックスを連動->画像も変わる
            radioRightScrToNextPic.Checked = true;
            //radioLeftScrToNextPic.Checked = false;
        }

        private void PictureBoxLeftScr_Click(object sender, EventArgs e)
        {
            //ラジオボックスを連動->画像も変わる
            //radioRightScrToNextPic.Checked = false;
            radioLeftScrToNextPic.Checked = true;
        }

        private void TmpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tmpFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        //private void Label35_Click(object sender, EventArgs e)
        //{
        //}

        //private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        //{
        //}

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == 7)
                e.Cancel = true;
        }

        /// <summary>
        /// KeyAcceleratorの入力値検証
        /// 同一の入力値がある場合はキャンセルする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyAcc_Validating(object sender, CancelEventArgs e)
        {
            //調査元となるKeyAccelerator
            var org = sender as KeyAccelerator;

            //設定がないのなら何もしない。
            if (org.keyData == Keys.None)
                return;

            //コントロールを列挙
            foreach (var c in tableLayoutPanel1.Controls)
                if (c is KeyAccelerator)
                {
                    KeyAccelerator testing = c as KeyAccelerator;
                    if (testing == org)
                        //自分自身はチェック対象外
                        continue;
                    else
                        //チェック
                        if (testing.keyData == org.keyData)
                    {
                        //重複している
                        var ret = MessageBox.Show(
                            string.Format("「{0}」と設定が重複しています。上書きしますか？", testing.Tag),
                            "キー設定確認",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                        if (ret == DialogResult.Yes)
                        {
                            //ほかのコントロールを変更する
                            testing.keyData = Keys.None;
                            testing.Invalidate();
                        }
                        else
                        {
                            //Cancelする。
                            e.Cancel = true;
                        }
                    }
                }
        }
    }//class
}//namespace
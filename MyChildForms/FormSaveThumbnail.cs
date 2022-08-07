using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormSaveThumbnail : Form
    {
        //const int DEFAULT_THUMBNAIL_SIZE = 120; //App.DEFAULT_THUMBNAIL_SIZEに統合;
        private const int DEFAULT_VERTICAL_WIDTH = 640;

        private readonly bool saveConf_isDrawFilename;       //グローバルコンフィグの一時待避領域
        private readonly bool saveConf_isDrawFileSize;       //グローバルコンフィグの一時待避領域
        private readonly bool saveConf_isDrawPicSize;        //グローバルコンフィグの一時待避領域

        private string savename;                            //保存ファイル名
        private readonly List<ImageInfo> m_thumbnailSet;     //リストへのポインタ
        private readonly ThumbnailPanel m_tPanel = null;     //親のパネルを表す
        private bool m_Saving = false;              //保存処理中かどうかを表すフラグ

        public bool IsCancel => !m_Saving;

        public FormSaveThumbnail(ThumbnailPanel tp, List<ImageInfo> lii, string Filename)
        {
            InitializeComponent();
            m_thumbnailSet = lii;
            m_tPanel = tp;
            m_tPanel.SavedItemChanged += ThumbPanel_SavedItemChanged;

            //保存ファイル名
            savename = SuggestFilename(Filename);

            //グローバルコンフィグを一時保存
            saveConf_isDrawFilename = App.Config.IsShowTPFileName;
            saveConf_isDrawFileSize = App.Config.IsShowTPFileSize;
            saveConf_isDrawPicSize = App.Config.IsShowTPPicSize;
        }

        ~FormSaveThumbnail()
        {
            m_tPanel.SavedItemChanged -= ThumbPanel_SavedItemChanged;
        }

        private void FormSaveThumbnail_Load(object sender, EventArgs e)
        {
            //テキストボックスの初期化：画像サイズ、画像個数
            tbPixels.Text = App.DEFAULT_THUMBNAIL_SIZE.ToString();
            int vertical = DEFAULT_VERTICAL_WIDTH / App.DEFAULT_THUMBNAIL_SIZE;
            tbnItemX.Text = vertical.ToString();

            //プログレスバーの初期化
            tsProgressBar1.Visible = false;

            //チェックボックスの初期化
            isDrawFileName.Checked = true;
            isDrawFileSize.Checked = false;
            isDrawPicSize.Checked = false;
        }

        private void FormSaveThumbnail_FormClosed(object sender, FormClosedEventArgs e)
        {
            //グローバルコンフィグを元に戻す
            App.Config.IsShowTPFileName = saveConf_isDrawFilename;
            App.Config.IsShowTPFileSize = saveConf_isDrawFileSize;
            App.Config.IsShowTPPicSize = saveConf_isDrawPicSize;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (m_Saving)
            {
                m_Saving = false;
                btExcute.Enabled = true;
                tbPixels.Enabled = true;
                tbnItemX.Enabled = true;
            }
            else
            {
                this.Close();
            }
        }

        private async void BtnExcute_Click(object sender, EventArgs e)
        {
            //サムネイルサイズの設定
            if (!Int32.TryParse(tbPixels.Text, out int ThumbnailSize))
                ThumbnailSize = App.DEFAULT_THUMBNAIL_SIZE;
            tbPixels.Text = ThumbnailSize.ToString();

            //横に並ぶ個数の設定
            if (!Int32.TryParse(tbnItemX.Text, out int nItemX))
                nItemX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
            tbnItemX.Text = nItemX.ToString();

            //ファイル名の確認
            var sf = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = savename,
                InitialDirectory = Path.GetDirectoryName(savename),
                Filter = "pngファイル|*.png|全てのファイル|*.*",
                FilterIndex = 1,
                OverwritePrompt = true
            };
            if (sf.ShowDialog() == DialogResult.OK)
                savename = sf.FileName;
            else
                return; //キャンセル相当

            tbInfo.Text += "保存先 : " + savename + "\r\n"
                        + "アイテム数 : " + m_thumbnailSet.Count + "\r\n";

            tsProgressBar1.Minimum = 0;
            tsProgressBar1.Maximum = m_thumbnailSet.Count - 1;  //0始まり
            tsProgressBar1.Value = 0;
            tsProgressBar1.Visible = true;

            //グローバルコンフィグを一時的に変更
            //FormClosed()で元に戻す
            App.Config.IsShowTPFileName = isDrawFileName.Checked;
            App.Config.IsShowTPFileSize = isDrawFileSize.Checked;
            App.Config.IsShowTPPicSize = isDrawPicSize.Checked;

            //サムネイルを保存する
            btExcute.Enabled = false;
            tbPixels.Enabled = false;
            tbnItemX.Enabled = false;
            m_Saving = true;
            await m_tPanel.SaveThumbnailImageAsync(ThumbnailSize, nItemX, savename);
            this.Close();
        }

        private string SuggestFilename(string orgName)
        {
            //指定ないときはデスクトップ/thumbnaul.pngを提案
            if (string.IsNullOrEmpty(orgName))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "Thumbnail.png");
            }

            //拡張子を切り出し
            string suggest = Path.Combine(
                Path.GetDirectoryName(orgName), Path.GetFileNameWithoutExtension(orgName));
            suggest += ".png";
            return suggest;
        }

        private void ThumbPanel_SavedItemChanged(object obj, ThumbnailEventArgs e)
        {
            int num = e.HoverItemNumber;
            toolStripStatusLabel1.Text = string.Format("完了数 : {0} / {1}", num + 1, m_thumbnailSet.Count);
            if (tsProgressBar1.Visible)
                tsProgressBar1.Value = num;

            if (num + 1 >= m_thumbnailSet.Count)
            {
                //btExcute.Enabled = true;
                //tbPixels.Enabled = true;
                //tbVnum.Enabled = true;
                toolStripStatusLabel1.Text = "完了しました";
            }
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            //サムネイルサイズの設定
            if (!Int32.TryParse(tbPixels.Text, out int ThumbnailSize))
                ThumbnailSize = App.DEFAULT_THUMBNAIL_SIZE;
            tbPixels.Text = ThumbnailSize.ToString();

            //横に並ぶ個数の設定
            if (!Int32.TryParse(tbnItemX.Text, out int nItemsX))
                nItemsX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
            tbnItemX.Text = nItemsX.ToString();

            //Bitmapの想定サイズを計算
            int ItemCount = m_thumbnailSet.Count;
            int nItemsY = ItemCount / nItemsX;  //縦に並ぶアイテム数はサムネイルの数による
            if (ItemCount % nItemsX > 0)        //割り切れなかった場合は1行追加
                nItemsY++;

            tbInfo.Text = string.Format("出力画像サイズ : {0:N0} x {1:N0} [pixels]\r\n",
                nItemsX * ThumbnailSize, nItemsY * ThumbnailSize);
        }
    }
}
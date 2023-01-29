using Marmi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormSaveThumbnail : Form
    {
        //private readonly bool saveConf_isDrawFilename;      //グローバルコンフィグの一時待避領域
        //private readonly bool saveConf_isDrawFileSize;      //グローバルコンフィグの一時待避領域
        //private readonly bool saveConf_isDrawPicSize;       //グローバルコンフィグの一時待避領域

        private string saveFilename;                        //保存ファイル名
        private readonly List<ImageInfo> _imageList;        //リストへのポインタ

        private bool m_Saving = false;                      //保存処理中かどうかを表すフラグ

        private CancellationTokenSource _cts = null;

        public bool IsCancel => !m_Saving;

        public FormSaveThumbnail(List<ImageInfo> imageList, string Filename)
        {
            InitializeComponent();
            _imageList = imageList;
            //_tpanel.SavedItemChanged += ThumbPanel_SavedItemChanged;

            //保存ファイル名
            saveFilename = SuggestFilename(Filename);

            //グローバルコンフィグを一時保存
            //saveConf_isDrawFilename = App.Config.Thumbnail.DrawFilename;
            //saveConf_isDrawFileSize = App.Config.Thumbnail.DrawFilesize;
            //saveConf_isDrawPicSize = App.Config.Thumbnail.DrawPicsize;
        }

        ~FormSaveThumbnail()
        {
            //_tpanel.SavedItemChanged -= ThumbPanel_SavedItemChanged;
        }

        private void FormSaveThumbnail_Load(object sender, EventArgs e)
        {
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
            //App.Config.Thumbnail.DrawFilename = saveConf_isDrawFilename;
            //App.Config.Thumbnail.DrawFilesize = saveConf_isDrawFileSize;
            //App.Config.Thumbnail.DrawPicsize = saveConf_isDrawPicSize;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (m_Saving)
            {
                m_Saving = false;
                btExcute.Enabled = true;
                thumbPixels.Enabled = true;
                itemNumsX.Enabled = true;
                _cts?.Cancel();
            }
            else
            {
                this.Close();
            }
        }

        private async void BtnExcute_Click(object sender, EventArgs e)
        {
            //保存ダイアログ
            var sf = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = saveFilename,
                InitialDirectory = Path.GetDirectoryName(saveFilename),
                Filter = "pngファイル|*.png|全てのファイル|*.*",
                FilterIndex = 1,
                OverwritePrompt = true
            };

            if (sf.ShowDialog() != DialogResult.OK)
                return;

            saveFilename = sf.FileName;

            //フォーム部品の設定
            tbInfo.Text += $"保存先 : {saveFilename}\r\nアイテム数 : {_imageList.Count}\r\n";
            tsProgressBar1.Minimum = 0;
            tsProgressBar1.Maximum = _imageList.Count - 1;  //0始まり
            tsProgressBar1.Value = 0;
            tsProgressBar1.Visible = true;
            btExcute.Enabled = false;
            thumbPixels.Enabled = false;
            itemNumsX.Enabled = false;
            m_Saving = true;

            _cts = new CancellationTokenSource();
            var progress = new Progress<int>(UpdateUI);

            //サムネイルを生成、保存
            var bmpMaker = new ThumbnailPictureMaker(
                            _imageList,
                            isDrawFileName.Checked,
                            isDrawFileSize.Checked,
                            isDrawPicSize.Checked);
            await bmpMaker.SaveBitmapAsync((int)thumbPixels.Value, (int)itemNumsX.Value, saveFilename, _cts.Token, progress);

            //保存完了したらこのフォームを閉じる
            this.Close();
        }

        private static string SuggestFilename(string orgName)
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
                Path.GetDirectoryName(orgName),
                Path.GetFileNameWithoutExtension(orgName));
            suggest += ".png";
            return suggest;
        }

        private void UpdateUI(int itemNumber)
        {
            int num = itemNumber;
            toolStripStatusLabel1.Text = $"完了数 : {num + 1} / {_imageList.Count}";
            if (tsProgressBar1.Visible)
                tsProgressBar1.Value = num;

            if (num + 1 >= _imageList.Count)
            {
                //btExcute.Enabled = true;
                //tbPixels.Enabled = true;
                //tbVnum.Enabled = true;
                toolStripStatusLabel1.Text = "完了しました";
            }
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            var tpixels = (int)thumbPixels.Value;

            //横に並ぶ個数の設定
            var nItemsX = (int)itemNumsX.Value;
            if (nItemsX == 0) nItemsX = 1;

            //Bitmapの想定サイズを計算
            int ItemCount = _imageList.Count;
            int nItemsY = ItemCount / nItemsX;  //縦に並ぶアイテム数はサムネイルの数による
            if (ItemCount % nItemsX > 0)        //割り切れなかった場合は1行追加
                nItemsY++;

            tbInfo.Text = $"出力画像サイズ : {nItemsX * tpixels:N0} x {nItemsY * tpixels:N0} [pixels]\r\n";
        }

        private void NumUpdown_ValueChanged(object sender, EventArgs e)
        {
            Textbox_TextChanged(sender, e);
        }
    }
}
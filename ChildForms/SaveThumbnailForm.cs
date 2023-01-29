using Marmi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class SaveThumbnailForm : Form
    {
        private string saveFilename;
        private readonly IReadOnlyList<ImageInfo> _imageList;

        //保存処理中かどうかを表すフラグ
        private bool _processing = false;

        private CancellationTokenSource _cts = null;

        public SaveThumbnailForm(List<ImageInfo> imageList, string Filename)
        {
            InitializeComponent();
            _imageList = imageList;
            saveFilename = SuggestFilename(Filename);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_processing)
            {
                //作成中ならキャンセル
                _processing = false;
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

            _processing = true;

            _cts = new CancellationTokenSource();
            var progress = new Progress<int>(UpdateUI);

            //サムネイルを生成、保存
            var bmpMaker = new ThumbnailPictureMaker(
                            _imageList,
                            isDrawFileName.Checked,
                            isDrawFileSize.Checked,
                            isDrawPicSize.Checked);
            var ret = await bmpMaker.SaveBitmapAsync((int)thumbPixels.Value, (int)itemNumsX.Value, saveFilename, _cts.Token, progress);

            if (ret)
            {
                //正常完了。このフォームを閉じる
                this.Close();
            }
            else
            {
                //キャンセル
                _processing = false;
                btExcute.Enabled = true;
                thumbPixels.Enabled = true;
                itemNumsX.Enabled = true;
            }
        }

        private static string SuggestFilename(string orgName)
        {
            //指定ないときはデスクトップ/thumbnail.pngを提案
            if (string.IsNullOrEmpty(orgName))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "thumbnail.png");
            }

            //拡張子を切り出し
            string suggest = Path.ChangeExtension(orgName, ".png");
            return suggest;
        }

        private void UpdateUI(int itemNumber)
        {
            int num = itemNumber;
            toolStripStatusLabel1.Text = $"完了数 : {num + 1} / {_imageList.Count}";
            if (tsProgressBar1.Visible)
                tsProgressBar1.Value = num;
        }

        private void NumUpdown_ValueChanged(object sender, EventArgs e)
        {
            var tpixels = (int)thumbPixels.Value;

            //XY方向個数
            var nItemsX = (int)itemNumsX.Value;
            int nItemsY = (int)Math.Ceiling(_imageList.Count / (double)nItemsX);

            tbInfo.Text = $"出力画像サイズ : {nItemsX * tpixels:N0} x {nItemsY * tpixels:N0} [pixels]\r\n";
        }
    }
}
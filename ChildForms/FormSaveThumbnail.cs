using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormSaveThumbnail : Form
    {
        private const int PADDING = 10;         //サムネイルの余白。2014年3月23日変更。間隔狭すぎた

        //private readonly bool saveConf_isDrawFilename;      //グローバルコンフィグの一時待避領域
        //private readonly bool saveConf_isDrawFileSize;      //グローバルコンフィグの一時待避領域
        //private readonly bool saveConf_isDrawPicSize;       //グローバルコンフィグの一時待避領域

        private bool _isDrawFilename = false;
        private bool _isDrawFileSize = false;
        private bool _isDrawPicSize = false;

        private string saveFilename;                        //保存ファイル名
        private readonly List<ImageInfo> _imageList;        //リストへのポインタ
        private bool m_Saving = false;                      //保存処理中かどうかを表すフラグ

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
            }
            else
            {
                this.Close();
            }
        }

        private async void BtnExcute_Click(object sender, EventArgs e)
        {
            //ファイル名の確認
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
            if (sf.ShowDialog() == DialogResult.OK)
                saveFilename = sf.FileName;
            else
                return; //キャンセル相当

            tbInfo.Text += "保存先 : " + saveFilename + "\r\n"
                        + "アイテム数 : " + _imageList.Count + "\r\n";

            tsProgressBar1.Minimum = 0;
            tsProgressBar1.Maximum = _imageList.Count - 1;  //0始まり
            tsProgressBar1.Value = 0;
            tsProgressBar1.Visible = true;

            _isDrawFilename = isDrawFileName.Checked;
            _isDrawFileSize = isDrawFileSize.Checked;
            _isDrawPicSize = isDrawPicSize.Checked;

            //サムネイルを保存する
            btExcute.Enabled = false;
            thumbPixels.Enabled = false;
            itemNumsX.Enabled = false;
            m_Saving = true;
            await SaveBitmapAsync((int)thumbPixels.Value, (int)itemNumsX.Value, saveFilename);
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

        #region サムネイルのファイル保存

        /// <summary>
        /// サムネイル画像一覧を作成、保存する。
        /// この関数の中で保存Bitmapを生成し、それをpng形式で保存する
        /// </summary>
        /// <param name="thumbSize">サムネイル画像のサイズ。正方形の一辺の長さ</param>
        /// <param name="numX">サムネイルの横方向の画像数</param>
        /// <param name="saveFilename">保存するファイル名</param>
        /// <returns>原則true、保存しなかった場合はfalse</returns>
        public async Task<bool> SaveBitmapAsync(int thumbSize, int numX, string saveFilename)
        {
            //初期化済みか確認
            if (_imageList == null || _imageList.Count == 0)
                return false;

            if (thumbSize < 10 || numX < 1)
                return false;

            //サムネイルサイズを設定.再計算
            var tboxSize = CalcTboxSize(thumbSize);

            //Bitmapサイズを計算
            var numY = ((_imageList.Count - 1) / numX) + 1;
            Size bmpSize = new Size(tboxSize.Width * numX, tboxSize.Height * numY); ;

            //Bitmap生成
            var saveBmp = new Bitmap(bmpSize.Width, bmpSize.Height);
            //var dummyBmp = new Bitmap(tboxSize.Width, tboxSize.Height);

            using (var g = Graphics.FromImage(saveBmp))
            {
                //対象矩形を背景色で塗りつぶす.
                g.Clear(BackColor);

                int x = -1;
                int y = 0;
                for (int ix = 0; ix < _imageList.Count; ix++)
                {
                    if (++x >= numX)
                    {
                        x = 0;
                        y++;
                    }

                    //using (var dummyg = Graphics.FromImage(dummyBmp))
                    //{
                    //    dummyg.Clear(Color.White);

                    Rectangle tboxRect = new Rectangle(
                        x * tboxSize.Width,
                        y * tboxSize.Height,
                        tboxSize.Width,
                        tboxSize.Height);

                    //高品質画像を描写
                    await DrawItemHQ2Async(g, ix, thumbSize, tboxRect.X, tboxRect.Y);

                    //画像情報を文字描写する
                    DrawTextInfo(g, ix, tboxRect, thumbSize);
                    //}

                    UpdateUI(ix);

                    //ver1.31 nullチェック
                    Application.DoEvents();

                    //キャンセル処理
                    if (IsCancel)
                        return false;
                }
            }

            saveBmp.Save(saveFilename);
            saveBmp.Dispose();
            return true;
        }

        /// <summary>
        /// 高品質描写DrawItem.
        /// 元画像から作る。サムネイル一覧保存用に利用。
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="itemNum">アイテム番号</param>
        /// <param name="thumbSize">サムネイルのサイズ。一片の長さ</param>
        /// <param name="px">描写位置X</param>
        /// <param name="py">描写位置Y</param>
        /// <returns></returns>
        public static async Task DrawItemHQ2Async(Graphics g, int itemNum, int thumbSize, int px, int py)
        {
            //描写品質:最高
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var drawBitmap = await Bmp.GetBitmapAsync(itemNum, false);
            if (drawBitmap == null)
            {
                //サムネイルは準備できていない
                return;
            }

            bool drawFrame = true;      //フラグ:枠線を描写するか
            int w = drawBitmap.Width;   //画像の幅
            int h = drawBitmap.Height;  //画像の高さ

            //縮小が必要な場合はサイズ変更
            if (w > thumbSize || h > thumbSize)
            {
                float ratio = (w > h) ?
                    (float)thumbSize / (float)w :
                    (float)thumbSize / (float)h;
                w = (int)(w * ratio);
                h = (int)(h * ratio);
            }

            int sx = px + (thumbSize + PADDING * 2 - w) / 2; //画像描写X位置
            int sy = py + thumbSize + PADDING - h;           //画像描写Y位置：下揃え

            var imageRect = new Rectangle(sx, sy, w, h);

            //影を描写
            //if (App.Config.Thumbnail.DrawShadowdrop && drawFrame)
            //{
            //    Rectangle frameRect = imageRect;
            //    BitmapUty.DrawDropShadow(g, frameRect);
            //}

            //画像を書く
            //フォーカスのない画像を描写
            g.FillRectangle(Brushes.White, imageRect);
            g.DrawImage(drawBitmap, imageRect);

            //外枠
            if (drawFrame)
            {
                g.DrawRectangle(Pens.LightGray, imageRect);
            }
        }

        /// <summary>
        /// TBOXサイズを計算する。
        /// 単純にPADDING分と文字列分を足したもの。
        /// </summary>
        public Size CalcTboxSize(int thumbnailSize)
        {
            var fontHeight = App.Font9_Height;

            //TBOXサイズを確定
            var w = thumbnailSize + (PADDING * 2);
            var h = thumbnailSize + (PADDING * 2);

            //文字列部追加
            if (_isDrawFilename)
                h += PADDING + fontHeight;

            if (_isDrawFileSize)
                h += PADDING + fontHeight;

            if (_isDrawPicSize)
                h += PADDING + fontHeight;

            return new Size(w, h);
        }

        private void DrawTextInfo(Graphics g, int item, Rectangle tboxRect, int thumbSize)
        {
            var font = App.Font9;
            var fontColor = Brushes.Black;
            var fontHeight = App.Font9_Height;

            //テキスト描写位置を補正
            Rectangle textRect = tboxRect;
            textRect.X += PADDING;                              //左に余白を追加
            textRect.Y += PADDING + thumbSize + PADDING;   //上下に余白を追加
            textRect.Width = thumbSize;                    //横幅はサムネイルサイズと同じ
            textRect.Height = fontHeight;

            //テキスト描写用の初期フォーマット
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,          //中央揃え
                Trimming = StringTrimming.EllipsisPath      //中間の省略
            };

            //ファイル名を書く
            if (_isDrawFilename)
            {
                string filename = Path.GetFileName(_imageList[item].Filename);
                if (filename != null)
                {
                    g.DrawString(filename, font, fontColor, textRect, sf);
                    textRect.Y += fontHeight;
                }
            }

            //ファイルサイズを書く
            if (_isDrawFileSize)
            {
                string s = $"{_imageList[item].FileLength:#,0} bytes";
                g.DrawString(s, font, fontColor, textRect, sf);
                textRect.Y += fontHeight;
            }

            //画像サイズを書く
            if (_isDrawPicSize)
            {
                string s = $"{_imageList[item].Width:#,0}x{_imageList[item].Height:#,0} px";
                g.DrawString(s, font, fontColor, textRect, sf);
                textRect.Y += fontHeight;
            }
        }

        #endregion サムネイルのファイル保存

        private void NumUpdown_ValueChanged(object sender, EventArgs e)
        {
            Textbox_TextChanged(sender, e);
        }
    }
}
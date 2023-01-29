/********************************************************************************
ThumbnailPictureMaker

サムネイルの一覧画像を生成する
Thumbnailコントロールから切り出し
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi.Models
{
    internal class ThumbnailPictureMaker
    {
        private const int PADDING = 10;
        private readonly bool _isDrawFilename = false;
        private readonly bool _isDrawFileSize = false;
        private readonly bool _isDrawPicSize = false;

        private readonly IReadOnlyList<ImageInfo> _imageList;

        public ThumbnailPictureMaker(IReadOnlyList<ImageInfo> imageList, bool drawfilename, bool drawFilesize, bool drawPicsize)
        {
            _imageList = imageList;
            _isDrawFilename = drawfilename;
            _isDrawFileSize = drawFilesize;
            _isDrawPicSize = drawPicsize;
        }

        /// <summary>
        /// サムネイル画像一覧を作成、保存する。
        /// この関数の中で保存Bitmapを生成し、それをpng形式で保存する
        /// </summary>
        /// <param name="thumbSize">サムネイル画像のサイズ。正方形の一辺の長さ</param>
        /// <param name="numX">サムネイルの横方向の画像数</param>
        /// <param name="saveFilename">保存するファイル名</param>
        /// <returns>原則true、保存しなかった場合はfalse</returns>
        public async Task<bool> SaveBitmapAsync(int thumbSize, int numX, string saveFilename, CancellationToken token, IProgress<int> progress)
        {
            if (_imageList == null
                || _imageList.Count == 0
                || thumbSize < 10
                || numX < 1)
                return false;

            var backColor = Color.White;

            //サムネイルサイズを設定.再計算
            var tboxSize = CalcTboxSize(thumbSize);

            //Bitmapサイズを計算
            var numY = ((_imageList.Count - 1) / numX) + 1;
            Size bmpSize = new Size(tboxSize.Width * numX, tboxSize.Height * numY); ;

            //Bitmap生成
            var saveBmp = new Bitmap(bmpSize.Width, bmpSize.Height);

            using (var g = Graphics.FromImage(saveBmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(backColor);

                int x = -1;
                int y = 0;
                for (int ix = 0; ix < _imageList.Count; ix++)
                {
                    if (++x >= numX)
                    {
                        x = 0;
                        y++;
                    }

                    var tboxRect = new Rectangle(
                        x * tboxSize.Width,
                        y * tboxSize.Height,
                        tboxSize.Width,
                        tboxSize.Height);

                    //高品質画像を描写
                    await DrawItemAsync(g, ix, thumbSize, tboxRect.X, tboxRect.Y);

                    //画像情報を文字描写する
                    DrawText(g, ix, tboxRect, thumbSize);

                    progress.Report(ix);
                    Application.DoEvents();

                    //キャンセル処理
                    if (token.IsCancellationRequested)
                    {
                        return false;
                    }
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
        private static async Task DrawItemAsync(Graphics g, int itemNum, int thumbSize, int px, int py)
        {
            var drawBitmap = await Bmp.GetBitmapAsync(itemNum, false);
            if (drawBitmap == null)
            {
                //画像準備できていない
                return;
            }

            bool drawFrame = true;      //枠線描写
            int w = drawBitmap.Width;   //画像幅
            int h = drawBitmap.Height;  //画像高

            //縮小が必要な場合はサイズ変更
            if (w > thumbSize || h > thumbSize)
            {
                var ratio = (w > h) ? (float)thumbSize / w : (float)thumbSize / (float)h;
                w = (int)(w * ratio);
                h = (int)(h * ratio);
            }

            int sx = px + (thumbSize + PADDING * 2 - w) / 2; //画像描写X位置
            int sy = py + thumbSize + PADDING - h;           //画像描写Y位置：下揃え

            var imageRect = new Rectangle(sx, sy, w, h);

            //画像
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
        private Size CalcTboxSize(int thumbnailSize)
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

        /// <summary>
        /// 画像情報をTboxに描写する
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="itemNum">アイテム番号</param>
        /// <param name="tboxRect">Tboxの位置</param>
        /// <param name="thumbSize">サムネイル画像サイズ。正方形一辺の長さ</param>
        private void DrawText(Graphics g, int itemNum, Rectangle tboxRect, int thumbSize)
        {
            var font = App.Font9;
            var fontColor = Brushes.Black;
            var fontHeight = App.Font9_Height;

            //テキスト描写位置
            var textRect = new Rectangle(
                x: tboxRect.X + PADDING,
                y: tboxRect.Y + PADDING + thumbSize + PADDING,
                width: thumbSize,
                height: fontHeight
                );

            //テキスト描写フォーマット:中央揃え,中間の省略
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisPath
            };

            //ファイル名
            if (_isDrawFilename)
            {
                string filename = Path.GetFileName(_imageList[itemNum].Filename);
                if (filename != null)
                {
                    g.DrawString(filename, font, fontColor, textRect, sf);
                    textRect.Y += fontHeight;
                }
            }

            //ファイルサイズ
            if (_isDrawFileSize)
            {
                string s = $"{_imageList[itemNum].FileLength:#,0} bytes";
                g.DrawString(s, font, fontColor, textRect, sf);
                textRect.Y += fontHeight;
            }

            //画像サイズ
            if (_isDrawPicSize)
            {
                string s = $"{_imageList[itemNum].Width:#,0}x{_imageList[itemNum].Height:#,0} px";
                g.DrawString(s, font, fontColor, textRect, sf);
                textRect.Y += fontHeight;
            }
        }
    }
}
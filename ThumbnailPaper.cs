/****************************************************************************
ThumbnailPaper.cs

サムネイルパネル一覧を画像保存するためのクラス

2022年5月 ThumbnailPanelから独立。
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;

namespace Marmi;

internal class ThumbnailPaper
{
    private List<ImageInfo> ImageList => App.g_pi.Items;   //ImageInfoのリスト, = g_pi.Items
    private const int PADDING = 10;     //サムネイルの余白。2014年3月23日変更。間隔狭すぎた
    private readonly Color BackColor = Color.White;

    private int _numX = 0;          // アイテム数：横
    private int _numY = 0;          // アイテム数：縦
    private int _tboxHeight = 0;    // サムネイルBOXのサイズ
    private int _thumbSize = 0;     // サムネイルのサイズ
    private readonly int _fontHeight;        // フォントの高さ
    private readonly bool _drawFilename;
    private readonly bool _drawFilesize;
    private readonly bool _drawImagesize;
    private readonly Font _font;
    private readonly Brush _fontbrush;

    public ThumbnailPaper(AppGlobalConfig config, bool drawFilename, bool drawFilesize, bool drawImagesize)
    {
        //_thumbSize = config.Thumbnail.TSize;
        _font = config.Thumbnail.Font ?? App.Font9R;
        _fontHeight = Bmp.GetFontHeight(_font);
        _fontbrush = (config.Thumbnail.FontColor != null)
            ? new SolidBrush(config.Thumbnail.FontColor)
            : Brushes.Black;
        _drawFilename = drawFilename;
        _drawFilesize = drawFilesize;
        _drawImagesize = drawImagesize;
    }

    /// <summary>
    /// サムネイル画像一覧を作成、保存する。
    /// この関数の中で保存Bitmapを生成し、それをpng形式で保存する
    /// </summary>
    /// <param name="thumbSize">サムネイル画像のサイズ</param>
    /// <param name="numX">サムネイルの横方向の画像数</param>
    /// <param name="filename">保存するファイル名</param>
    /// <returns>原則true、保存しなかった場合はfalse</returns>
    public async Task MakeAndSave(int thumbSize, int numX, string filename)
    {
        if (ImageList == null || ImageList.Count == 0)
            throw new NullReferenceException("画像がありません");

        if (numX < 1)
            throw new ArgumentOutOfRangeException(nameof(numX), "画像横方向の数がおかしい");

        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException("ファイル名がおかしい");

        using var bmp = await MakeBitmapAsync(thumbSize, numX);
        if (File.Exists(filename))
            File.Delete(filename);
        //bmp.Save(filename);
        using var st = File.OpenWrite(filename);
        RawImageHelper.WriteAsJpeg(bmp, 80, st);
    }

    private async Task<Bitmap> MakeBitmapAsync(int thumbSize, int numX)
    {
        if (ImageList == null || ImageList.Count == 0)
            throw new NullReferenceException("画像がありません");

        if (numX < 1)
            throw new ArgumentOutOfRangeException(nameof(numX), "画像横方向の数がおかしい");

        //サムネイルサイズを設定.再計算
        _numX = numX;
        _numY = (int)Math.Ceiling(ImageList.Count / (double)numX);
        _tboxHeight = CalcTboxHeight(thumbSize);
        _thumbSize = thumbSize;
        var screenW = numX * _tboxHeight;
        var screenH = _numY * _tboxHeight;

        //Bitmap生成
        var saveBmp = new Bitmap(screenW, screenH);
        using (var g = Graphics.FromImage(saveBmp))
        {
            g.Clear(BackColor);
            for (int item = 0; item < ImageList.Count; item++)
            {
                var rect = GetTboxRect(item);
                await DrawItemHQ2(g, item, thumbSize, rect);
                DrawTextInfo(g, item, rect);
            }
        }

        return saveBmp;
    }

    /// <summary>
    /// サムネイル画像サイズからthumbboxサイズを取得
    /// </summary>
    /// <param name="thumbnailHeight">tboxサイズ</param>
    private int CalcTboxHeight(int thumbnailHeight)
    {
        //BOXサイズを確定
        var height = thumbnailHeight + (PADDING * 2);

        if (_drawFilename)
            height += PADDING + _fontHeight;

        if (_drawFilesize)
            height += PADDING + _fontHeight;

        if (_drawImagesize)
            height += PADDING + _fontHeight;
        return height;
    }

    /// <summary>
    /// 高品質専用描写DrawItem.
    /// サムネイル一覧保存用に利用。
    /// ダミーBMPに描写するため描写位置を固定とする。
    /// </summary>
    /// <param name="g"></param>
    /// <param name="item"></param>
    private async Task DrawItemHQ2(Graphics g, int item, int thumbSize, Rectangle tboxRect)
    {
        // var drawBitmap = Bmp.GetBitmap(item);
        var drawBitmap = await Bmp.GetBitmapForceAsync(item);
        if (drawBitmap == null)
            throw new InvalidOperationException("画像が準備できない");

        var ratio = BitmapUty.GetMagnificationWithFixAspectRatio(drawBitmap.Size, thumbSize);
        var w = (int)(drawBitmap.Width * ratio);
        var h = (int)(drawBitmap.Height * ratio);
        int sx = (thumbSize - w) / 2;
        int sy = thumbSize + PADDING - h;
        var rect = new Rectangle(sx + tboxRect.X, sy + tboxRect.Y, w, h);

        //画像を書く
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.FillRectangle(Brushes.White, tboxRect);
        g.DrawImage(drawBitmap, rect);

        //外枠
        if (App.Config.Thumbnail.DrawFrame)
        {
            g.DrawRectangle(Pens.LightGray, rect);
        }

        //Bitmapの破棄。GetBitmapWithoutCache()で取ってきたため
        if (drawBitmap != null && (string)(drawBitmap.Tag) != App.TAG_PICTURECACHE)
        {
            drawBitmap.Dispose();
        }
    }

    /// <summary>
    /// 指定サムネイルの画面内での枠を返す。
    /// Thumbbox = 画像＋文字の大きな枠
    /// スクロールバーについても織り込み済
    /// m_offScreenや実画面に対して使われることを想定
    /// </summary>
    private Rectangle GetTboxRect(int index)
    {
        int sx = index % _numX;
        int sy = index / _numX;
        return new Rectangle(sx * _tboxHeight, (sy * _tboxHeight), _tboxHeight, _tboxHeight);
    }

    private void DrawTextInfo(Graphics g, int item, Rectangle tboxRect)
    {
        //テキスト描写位置を決定
        var sx = tboxRect.X + PADDING;
        var sy = tboxRect.Y + _thumbSize + PADDING;
        var width = _thumbSize;
        var height = tboxRect.Height - PADDING - PADDING - _thumbSize;
        var textRect = new Rectangle(sx, sy, width, height);

        //テキスト描写用の初期フォーマット
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,         // 中央揃え
            Trimming = StringTrimming.EllipsisPath      // 中間の省略
        };

        //ファイル名
        if (_drawFilename)
        {
            var filename = Path.GetFileName(ImageList[item].Filename);
            g.DrawString(filename, _font, _fontbrush, textRect, sf);
            textRect.Y += _fontHeight;
        }

        //ファイルサイズ
        if (_drawFilesize)
        {
            var s = $"{ImageList[item].FileLength:#,0} bytes";
            g.DrawString(s, _font, _fontbrush, textRect, sf);
            textRect.Y += _fontHeight;
        }

        //画像サイズ
        if (_drawImagesize)
        {
            var s = $"{ImageList[item].ImgSize.Width:#,0}x{ImageList[item].ImgSize.Height:#,0} px";
            g.DrawString(s, _font, _fontbrush, textRect, sf);
            textRect.Y += _fontHeight;
        }
    }
}
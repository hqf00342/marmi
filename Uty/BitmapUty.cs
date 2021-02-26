using System;
using System.Collections.Generic;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Drawing.Drawing2D;			// GraphicsPath
using System.Drawing.Imaging;			// ColorMatrix
using System.IO;						// Directory, File, Stream

//using System.Text;

using System.Windows.Forms;

namespace Marmi
{
    public class BitmapUty
    {
        private static int BoxMargin = 20;                      //ボックス外周部のマージン
        private static int padding = 5;                         //Key-Value間のPadding
        private static string SepareteString = " : ";           //2列表示実施時のセパレーター文字列
        private static float LinePadding = 1f;                  //行間

        private static string FontName = "MS PGothic";          //フォント名
        private static int FontPoint = 10;                      //フォントのポイント
        private static Color FontColorKey = Color.RoyalBlue;    //キーの色
        private static Color FontColorValue = Color.Black;      //値の色（通常の色）
        private static Color BackColor = Color.White;           //背景色
        private static Color BorderColor = Color.DarkGray;      //ボーダーカラー

        public static Bitmap Text2Bitmap(string str, bool isRoundCorner)
        {
            string[] sz = new string[] { str };
            return Text2Bitmap(sz, isRoundCorner);
        }

        public static Bitmap Text2Bitmap(string[] str, bool isRoundCorner)
        {
            float height = 0f;      //計測された高さ
            float width = 0f;       //計測された幅

            using (Font font = new Font(FontName, FontPoint))
            {
                //サイズを測る
                using (Bitmap b = new Bitmap(10, 10))
                using (Graphics g = Graphics.FromImage(b))
                {
                    foreach (string s in str)
                    {
                        SizeF sizef = g.MeasureString(s, font);
                        if (sizef.Width > width)
                            width = sizef.Width;
                        height += sizef.Height + LinePadding;
                    }
                }

                height += BoxMargin * 2;
                width += BoxMargin * 2;

                //Bitmapを作る
                Bitmap returnbmp = new Bitmap((int)width, (int)height);
                using (Graphics g = Graphics.FromImage(returnbmp))
                {
                    float y = BoxMargin;
                    //Brush brush = Brushes.Black;
                    SolidBrush brush = new SolidBrush(FontColorValue);

                    //箱の初期化
                    InitBackImage(g, width, height, isRoundCorner);

                    foreach (string s in str)
                    {
                        SizeF sizef = g.MeasureString(s, font);

                        g.DrawString(s, font, brush, BoxMargin, y);
                        y += sizef.Height + LinePadding;
                    }
                }
                return returnbmp;
            }//using font
        }

        public static Bitmap Text2Bitmap(KeyValuePair<string, string>[] kv, bool isRoundCorner)
        {
            float keyWidth = 0;
            float keyHeight = 0;
            float valueWidth = 0;
            float valueHeight = 0;

            using (Font font = new Font(FontName, FontPoint))
            {
                using (Bitmap TempBmp = new Bitmap(10, 10))
                using (Graphics g = Graphics.FromImage(TempBmp))
                {
                    SizeF size;
                    foreach (KeyValuePair<string, string> s in kv)
                    {
                        //if (s == null)
                        //    continue;

                        //Keyの大きさを確認
                        size = g.MeasureString(s.Key + SepareteString, font);
                        keyWidth = (keyWidth > size.Width) ? keyWidth : size.Width;
                        keyHeight = (keyHeight > size.Height) ? keyHeight : size.Height;

                        //Valueの大きさを確認
                        size = g.MeasureString(s.Value, font);
                        valueWidth = (valueWidth > size.Width) ? valueWidth : size.Width;
                        valueHeight = (valueHeight > size.Height) ? valueHeight : size.Height;
                    }
                }

                float height = (valueHeight > keyHeight) ? valueHeight : keyHeight;
                RectangleF rcKey = new RectangleF(0, 0, keyWidth, height);
                RectangleF rcValue = new RectangleF(0, 0, valueWidth, height);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Near;

                Bitmap returnBitmap = new Bitmap(
                    (int)(keyWidth + valueWidth + padding + BoxMargin * 2),
                    (int)((height + LinePadding) * kv.Length + BoxMargin * 2));

                using (Graphics g = Graphics.FromImage(returnBitmap))
                {
                    //箱の初期化
                    InitBackImage(g, returnBitmap.Width, returnBitmap.Height, isRoundCorner);

                    SolidBrush keyBrush = new SolidBrush(FontColorKey);
                    SolidBrush valueBrush = new SolidBrush(FontColorValue);

                    //文字を描く
                    for (int i = 0; i < kv.Length; i++)
                    {
                        //if (sd[i] == null)
                        //    continue;

                        //g.DrawString(
                        //    sd[i].Key,
                        //    font,
                        //    Brushes.SteelBlue,
                        //    BoxMargin,
                        //    height * i);

                        rcKey.X = BoxMargin;
                        rcKey.Y = (height + LinePadding) * i + BoxMargin;

                        //Keyを描写
                        g.DrawString(
                            kv[i].Key + SepareteString,
                            font,
                            //Brushes.SteelBlue,
                            keyBrush,
                            rcKey,
                            sf);

                        //Valueを描写
                        g.DrawString(
                            kv[i].Value,
                            font,
                            valueBrush,
                            BoxMargin + keyWidth + padding,
                            (height + LinePadding) * i + BoxMargin);
                    }
                }
                return returnBitmap;
            }//using font
        }

        //Text2Bitmap()から呼び出される背景作成ルーチン
        private static void InitBackImage(Graphics g, float width, float height, bool isRoundCorner)
        {
            if (isRoundCorner)
            {
                int arc = 10;
                GraphicsPath gp = CreateRoundedRectangle((int)width - 1, (int)height - 1, arc);
                g.FillPath(new SolidBrush(BackColor), gp);
                g.DrawPath(new Pen(BorderColor), gp);
            }
            else
            {
                g.Clear(BackColor);
                g.DrawRectangle(new Pen(BorderColor), 0, 0, width - 1, height - 1);
            }
        }

        //角丸のGraphicPathを作る
        private static GraphicsPath CreateRoundedRectangle(int width, int height, int arc)
        {
            //透明部分を作る
            int padding = 1;

            GraphicsPath path = new GraphicsPath(FillMode.Winding);
            path.AddArc(width - arc - padding, padding, arc, arc, 270, 90);
            path.AddArc(width - arc - padding, height - arc - padding, arc, arc, 0, 90);
            path.AddArc(padding, height - arc - padding, arc, arc, 90, 90);
            path.AddArc(padding, padding, arc, arc, 180, 90);
            path.AddArc(width - arc - padding, padding, arc, arc, 270, 90);
            //path.AddArc(width - arc, 0, arc, arc, 270, 90);
            //path.AddArc(width - arc, height - arc, arc, arc, 0, 90);
            //path.AddArc(0, height - arc, arc, arc, 90, 90);
            //path.AddArc(0, 0, arc, arc, 180, 90);
            //path.AddArc(width - arc, 0, arc, arc, 270, 90);
            return path;
        }

        public static GraphicsPath GetRoundRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            // 左上の角丸
            path.AddArc(rect.Left, rect.Top, radius * 2, radius * 2,
                180, 90);
            // 上の線
            path.AddLine(rect.Left + radius, rect.Top,
                rect.Right - radius, rect.Top);
            // 右上の角丸
            path.AddArc(rect.Right - radius * 2, rect.Top,
                radius * 2, radius * 2,
                270, 90);
            // 右の線
            path.AddLine(rect.Right, rect.Top + radius,
                rect.Right, rect.Bottom - radius);
            // 右下の角丸
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2,
                radius * 2, radius * 2,
                0, 90);
            // 下の線
            path.AddLine(rect.Right - radius, rect.Bottom,
                rect.Left + radius, rect.Bottom);
            // 左下の角丸
            path.AddArc(rect.Left, rect.Bottom - radius * 2,
                radius * 2, radius * 2,
                90, 90);
            // 左の線
            path.AddLine(rect.Left, rect.Bottom - radius,
                rect.Left, rect.Top + radius);

            path.CloseFigure();

            return path;
        }

        //半透明版のDrawImage
        public static void alphaDrawImage(Graphics g, Image img, float alpha)
        {
            var cm = new ColorMatrix
            {
                Matrix00 = 1,
                Matrix11 = 1,
                Matrix22 = 1,
                Matrix33 = alpha,
                Matrix44 = 1
            };

            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            //アルファブレンドしながら描写
            g.DrawImage(
                img,
                new Rectangle(0, 0, img.Width, img.Height),
                0, 0,
                img.Width, img.Height,
                GraphicsUnit.Pixel,
                ia);
        }

        //半透明版のDrawImage
        public static void alphaDrawImage(Graphics g, Image img, Rectangle rect, float alpha)
        {
            var cm = new ColorMatrix
            {
                Matrix00 = 1,
                Matrix11 = 1,
                Matrix22 = 1,
                Matrix33 = alpha,
                Matrix44 = 1
            };

            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            //アルファブレンドしながら描写
            g.DrawImage(
                img,
                rect,
                0, 0, img.Width, img.Height,
                GraphicsUnit.Pixel,
                ia);
        }

        //アイコンファイル解析版
        public static Bitmap LoadIcon3(Stream fs)
        {
            // アイコンファイルを解析しGDI+で対応できていない
            // 大きなサイズのアイコン、VistaのPNG型アイコンに対応する
            //
            // アイコンファイルの構造
            //   ICONDIR構造体(6byte)
            //   ICONDIRENTRY構造体(16byte)×アイコン数
            //   ICONIMAGE構造体×アイコン数
            //

            //SharpZipのストリームはSeek()に対応できない。
            //なのでMemoryStreamに一度取り込んで利用する
            using (MemoryStream ms = new MemoryStream())
            {
                //MemoryStreamに取り込み
                int len;
                byte[] buffer = new byte[16384];
                while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, len);

                //先頭にRewind
                ms.Seek(0, 0);  //重要！

                //アイコン読み取り開始
                //ICONDIR構造体を読み取り
                Byte[] ICONDIR = new byte[6];
                ms.Read(ICONDIR, 0, 6);

                //アイコンファイルチェック
                if (ICONDIR[0] != 0
                    || ICONDIR[1] != 0
                    || ICONDIR[2] != 1
                    || ICONDIR[3] != 0)
                    return null;

                //内包されるアイコンの数を取得
                //int idCount = ICONDIR[4] + ICONDIR[5] * 256;
                int idCount = (int)BitConverter.ToInt16(ICONDIR, 4);

                //ICONDIRENTRY構造体の読み取り
                //一番大きく、色深度の高いアイコンを取得する
                Byte[] ICONDIRENTRY = new byte[16];
                int bWidth = 0;                 //アイコンの幅
                int bHeight = 0;                //アイコンの高さ
                int Item;                       //対象のアイテム番号（意味なし）
                int wBitCount = 0;              //色深度
                UInt32 dwBytesInRes = 0;        //対象イメージのバイト数
                UInt32 dwImageOffset = 0;       //対象イメージのオフセット

                for (int i = 0; i < idCount; i++)
                {
                    //一番大きい,色深度の高いアイコンを探せ
                    ms.Read(ICONDIRENTRY, 0, 16);
                    int width = (int)ICONDIRENTRY[0];
                    if (width == 0)
                        width = 256;    //0は256を意味する。ほぼ確実にPNG
                    int height = (int)ICONDIRENTRY[1];
                    if (height == 0)
                        height = 256;
                    width = width >= height ? width : height;   //大きい方を取る
                    int colorDepth = BitConverter.ToUInt16(ICONDIRENTRY, 6);
                    if (width >= bWidth && colorDepth >= wBitCount)
                    {
                        Item = i;
                        bWidth = width;
                        wBitCount = colorDepth;
                        dwBytesInRes = BitConverter.ToUInt32(ICONDIRENTRY, 8);
                        dwImageOffset = BitConverter.ToUInt32(ICONDIRENTRY, 12);
                        Debug.WriteLine(string.Format(
                            "Item={0}, bWidth={1}, dwimageOffset={2}, dwBytesInRes={3}",
                            Item,
                            bWidth,
                            dwImageOffset,
                            dwBytesInRes),
                            "ICONDIRENTRY");
                    }
                }

                //BITMAPINFOHEADER構造体
                Byte[] BITMAPINFOHEADER = new byte[40];
                ms.Seek(dwImageOffset, SeekOrigin.Begin);
                ms.Read(BITMAPINFOHEADER, 0, 40);
                if (BITMAPINFOHEADER[1] == (byte)'P'
                    && BITMAPINFOHEADER[2] == (byte)'N'
                    && BITMAPINFOHEADER[3] == (byte)'G')
                {
                    //PNGデータでした。
                    ms.Seek(dwImageOffset, SeekOrigin.Begin);
                    using (MemoryStream PngStream = new MemoryStream(ms.GetBuffer(), (int)dwImageOffset, (int)dwBytesInRes))
                    {
                        PngStream.Seek(0, SeekOrigin.Begin);
                        Bitmap png = new Bitmap(PngStream);
                        return png;
                    }
                }
                else
                {
                    UInt16 biBitCount = BitConverter.ToUInt16(BITMAPINFOHEADER, 14);
                    UInt32 biCompression = BitConverter.ToUInt32(BITMAPINFOHEADER, 16);
                    Debug.WriteLine(string.Format(
                        "biBitCount={0}, biCompression={1}",
                        biBitCount,
                        biCompression),
                        "BITMAPINFOHEADER");

                    //色数からパレット数を計算
                    int PALLET = 0;
                    if (biBitCount > 0 && biBitCount <= 8)
                        PALLET = (int)Math.Pow(2, biBitCount);

                    //BITMAPFILEHEADER(14)を作り、Bitmapクラスが読み取れるように
                    //Bitmapデータを作る。
                    //構造は
                    // BIMAPFILEHEADER(14)	:手動で作成
                    // BITMAPINFOHEADER(40)	:そのまま利用
                    // RGBQUAD(PALLET*4)	:そのまま利用
                    // IMAGEDATA + MASK		:そのまま利用
                    //
                    byte[] BMP = new byte[14 + dwBytesInRes];
                    Array.Clear(BMP, 0, 14);    //先頭14バイトは確実に０に
                    BMP[0] = (byte)'B';
                    BMP[1] = (byte)'M';
                    UInt32 dwSize = 14 + dwBytesInRes;
                    byte[] tmp1 = BitConverter.GetBytes(dwSize);
                    BMP[2] = tmp1[0];
                    BMP[3] = tmp1[1];
                    BMP[4] = tmp1[2];
                    BMP[5] = tmp1[3];
                    int bfOffBits = 14 + 40 + PALLET * 4;//BITMAPFILEHEADER(14) + BitmapInfoHeader(40) + PALLET*4
                    byte[] tmp = BitConverter.GetBytes(bfOffBits);
                    BMP[10] = tmp[0];
                    BMP[11] = tmp[1];
                    BMP[12] = tmp[2];
                    BMP[13] = tmp[3];
                    ms.Seek(dwImageOffset, SeekOrigin.Begin);
                    ms.Read(BMP, 14, (int)dwBytesInRes);

                    //高さを強制書き換え,マスクで倍になっているので半分に
                    int bmpWidth = BitConverter.ToInt32(BMP, 14 + 4);
                    int bmpHeight = BitConverter.ToInt32(BMP, 14 + 8);
                    bmpHeight /= 2;
                    byte[] hArray = BitConverter.GetBytes(bmpHeight);
                    BMP[14 + 8] = hArray[0];
                    BMP[14 + 9] = hArray[1];
                    BMP[14 + 10] = hArray[2];
                    BMP[14 + 11] = hArray[3];
                    //BMP[14 + 9] = BMP[14 + 9 - 4];
                    //BMP[14 + 10] = BMP[14 + 10 - 4];
                    //BMP[14 + 11] = BMP[14 + 11 - 4];

                    //一番大きいアイコンを取得する
                    //ms.Seek(dwImageOffset, SeekOrigin.Begin);
                    MemoryStream ImageStream = new MemoryStream(BMP);
                    ImageStream.Seek(0, SeekOrigin.Begin);
                    //Bitmap newbmp = new Bitmap(ImageStream);
                    Bitmap newbmp;
                    if (biBitCount == 32 && biCompression == 0)
                    {
                        //32bitなのでアルファチャネルを読み込む

                        //UnSafe版
                        newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                        ImageStream.Seek(14 + 40 + PALLET * 4, SeekOrigin.Begin);
                        Rectangle lockRect = new Rectangle(0, 0, bmpWidth, bmpHeight);
                        BitmapData bmpData = newbmp.LockBits(lockRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        unsafe
                        {
                            byte* offset = (byte*)bmpData.Scan0;
                            int writePos;
                            for (int y = bmpHeight - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < bmpWidth; x++)
                                {
                                    //4byte分を処理する
                                    writePos = x * 4 + bmpData.Stride * y;
                                    offset[writePos + 0] = (byte)ImageStream.ReadByte(); // B;
                                    offset[writePos + 1] = (byte)ImageStream.ReadByte(); // G;
                                    offset[writePos + 2] = (byte)ImageStream.ReadByte(); // R;
                                    offset[writePos + 3] = (byte)ImageStream.ReadByte(); // A;
                                }//for x
                            }//for y
                        }//unsafe
                        newbmp.UnlockBits(bmpData);

                        ////Manage(Safe)版
                        ////32bitなのでアルファチャネルを読み込む
                        //newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                        //ImageStream.Seek(14 + 40 + PALLET * 4, SeekOrigin.Begin);
                        //for (int y = bmpHeight - 1; y >= 0; y--)
                        //{
                        //    for (int x = 0; x < bmpWidth; x++)
                        //    {
                        //        //8bit分を処理する
                        //        int B = ImageStream.ReadByte();
                        //        int G = ImageStream.ReadByte();
                        //        int R = ImageStream.ReadByte();
                        //        int A = ImageStream.ReadByte();
                        //        newbmp.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                        //    }//for x
                        //}//for y
                    }
                    else
                    {
                        newbmp = new Bitmap(ImageStream, true);
                    }

                    //マスクbit対応
                    //32bit画像の場合は画像側でアルファチャネルを持っているので無視

                    //Manage版
                    //ver：色深度が低い場合SetPixcel()がエラーを吐く
                    //SetPixel は、インデックス付きピクセル形式のイメージに対してサポートされていません。
                    //if (biBitCount < 32)
                    //{
                    //    Rectangle rc = new Rectangle(0, 0, bmpWidth, bmpHeight);
                    //    if (newbmp.PixelFormat != PixelFormat.Format32bppArgb)
                    //    {
                    //        Bitmap tmpBmp = newbmp;
                    //        newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                    //        using (Graphics g = Graphics.FromImage(newbmp))
                    //        {
                    //            g.DrawImage(tmpBmp, rc);
                    //        }
                    //        tmpBmp.Dispose();
                    //    }

                    //    int maskSize = bmpWidth * bmpHeight / 8;
                    //    long maskOffset = dwImageOffset + dwBytesInRes - maskSize;
                    //    ms.Seek(maskOffset, SeekOrigin.Begin);
                    //    for (int y = bmpHeight - 1; y >= 0; y--)
                    //        for (int x8 = 0; x8 < bmpWidth / 8; x8++)
                    //        {
                    //            //8bit分を処理する
                    //            byte mask = (byte)ms.ReadByte();
                    //            byte checkBit = 0x80;
                    //            for (int xs = 0; xs < 8; xs++)
                    //            {
                    //                if ((mask & checkBit) != 0)
                    //                {
                    //                    newbmp.SetPixel(x8 * 8 + xs, y, Color.Transparent);
                    //                }
                    //                checkBit /= 2;
                    //            }
                    //        }
                    //}

                    //マスクbit対応
                    //32bit画像の場合は画像側でアルファチャネルを持っているので無視
                    //unsafe版
                    //lockBites()はPixelFormatでIndexedに対応していないので変換する必要がある。
                    Rectangle rc = new Rectangle(0, 0, bmpWidth, bmpHeight);
                    if (biBitCount < 32)
                    {
                        //PixelFormatを強制的にFormat32bppArgbに変換する
                        if (newbmp.PixelFormat != PixelFormat.Format32bppArgb)
                        {
                            Bitmap tmpBmp = newbmp;
                            newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(newbmp))
                            {
                                g.DrawImage(tmpBmp, rc);
                            }
                            tmpBmp.Dispose();
                        }

                        //マスクを読み込む
                        Debug.WriteLine("Load Mask");
                        int maskSize = bmpWidth / 8 * bmpHeight;
                        if (bmpWidth % 32 != 0)         //1ライン4バイト（32bit）単位にする。
                            maskSize = (bmpWidth / 32 + 1) * 4 * bmpHeight;
                        long maskOffset = dwImageOffset + dwBytesInRes - maskSize;
                        ms.Seek(maskOffset, SeekOrigin.Begin);
                        BitmapData bd = newbmp.LockBits(rc, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        unsafe
                        {
                            byte* pos = (byte*)bd.Scan0;
                            for (int y = bmpHeight - 1; y >= 0; y--)
                            {
                                int bytes = 0;  //1ライン中のバイト数を数える
                                for (int x8 = 0; x8 < bmpWidth / 8; x8++)
                                {
                                    //8bit分を処理する
                                    byte mask = (byte)ms.ReadByte();
                                    bytes++;
                                    Debug.Write(mask.ToString("X2"));
                                    byte checkBit = 0x80;
                                    for (int xs = 0; xs < 8; xs++)
                                    {
                                        if ((mask & checkBit) != 0)
                                        {
                                            pos[(x8 * 8 + xs) * 4 + (bd.Stride * y) + 3] = 0;
                                        }
                                        checkBit /= 2;
                                    }
                                }
                                Debug.Write("|");
                                while ((bytes % 4) != 0)
                                {
                                    byte b = (byte)ms.ReadByte();   //捨てる
                                    Debug.Write(b.ToString("X2"));
                                    bytes++;
                                }
                                Debug.WriteLine("");
                            }
                        }//unsafe
                        newbmp.UnlockBits(bd);
                    }//if (biBitCount < 32)

                    //処理完了
                    return newbmp;
                }
            }
        }

        //ドロップシャドウの描写
        //指定した矩形に影を落とす。距離は３
        public static void drawDropShadow(Graphics g, Rectangle r)
        {
            //ver0.975光彩をつける
            SolidBrush b1 = new SolidBrush(Color.FromArgb(96, Color.DimGray));
            SolidBrush b2 = new SolidBrush(Color.FromArgb(64, Color.DimGray));
            SolidBrush b3 = new SolidBrush(Color.FromArgb(32, Color.DimGray));
            SolidBrush b4 = new SolidBrush(Color.FromArgb(16, Color.DimGray));

            //g.CompositingMode = CompositingMode.SourceOver;
            r.Inflate(3, 3);
            r.X++;
            r.Y++;
            drawRoundRectangle(g, b4, r, 5);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b4, r, 4);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b3, r, 3);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b3, r, 2);
            //g.CompositingMode = CompositingMode.SourceCopy;
        }

        //指定した矩形に周りにエフェクトを描写
        public static void drawBlurEffect(Graphics g, Rectangle r, Color c)
        {
            //ver0.975光彩をつける
            SolidBrush b1 = new SolidBrush(Color.FromArgb(192, c));
            SolidBrush b2 = new SolidBrush(Color.FromArgb(96, c));
            SolidBrush b3 = new SolidBrush(Color.FromArgb(32, c));
            SolidBrush b4 = new SolidBrush(Color.FromArgb(16, c));

            //g.CompositingMode = CompositingMode.SourceOver;
            //r.Width += 1;	//足しておかないと1ドット欠ける
            //r.Height += 1;	//足しておかないと1ドット欠ける
            r.Inflate(4, 4);
            //r.X++;
            //r.Y++;
            drawRoundRectangle(g, b4, r, 5);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b3, r, 4);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b2, r, 3);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b1, r, 2);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b1, r, 2);
            //g.CompositingMode = CompositingMode.SourceCopy;
        }

        //角丸を描写
        //ドロップシャドウで使う
        private static void drawRoundRectangle(Graphics g, Brush br, Rectangle rect, int arc)
        {
            using (GraphicsPath gp = new GraphicsPath(FillMode.Winding))
            {
                gp.AddArc(rect.Right - arc, rect.Top, arc, arc, 270, 90);
                gp.AddArc(rect.Right - arc, rect.Bottom - arc, arc, arc, 0, 90);
                gp.AddArc(rect.Left, rect.Bottom - arc, arc, arc, 90, 90);
                gp.AddArc(rect.Left, rect.Top, arc, arc, 180, 90);
                gp.AddArc(rect.Right - arc, rect.Top, arc, arc, 270, 90);
                //g.DrawPath(pen, gp);
                g.FillPath(br, gp);
            }
        }

        /// <summary>
        /// Window/Controlをキャプチャする。
        /// 対象となるWindow/Controlの全域をキャプチャする
        /// </summary>
        /// <param name="wnd">キャプチャする対象のForm/Control</param>
        /// <returns>キャプチャされた画像</returns>
        public static Bitmap CaptureWindow(Control wnd)
        {
            //Rectangle rc = parent.RectangleToScreen(parent.DisplayRectangle);	//ツールバー込みでキャプチャ
            //Rectangle rc = parent.Bounds;	//これだとタイトルバーもキャプチャしてしまう。
            //Rectangle rc = ((Form1)wnd).GetClientRectangle();	//クライアント座標で取得。ツールバー無し
            Rectangle rc = wnd.ClientRectangle;
            rc = wnd.RectangleToScreen(rc);                     //スクリーン座標に

            Bitmap cauptureBmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(cauptureBmp))
            {
                g.CopyFromScreen(
                    rc.X, rc.Y,
                    0, 0,
                    rc.Size,
                    CopyPixelOperation.SourceCopy);
            }
            return cauptureBmp;
        }

        /// <summary>
        ///  ファイル名からBitmapを返す
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Bitmap LoadImageFromFile(string filename)
        {
            if (File.Exists(filename) && Uty.IsPictureFilename(filename))
            {
                using (FileStream fs = File.OpenRead(filename))
                {
                    return BitmapUty.LoadImageFromStream(filename, fs);
                }
            }
            return null;
        }

        /// <summary>
        /// StreamからBitmapを返す。
        /// ファイル名は拡張子をみてicoかどうかを確認している
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="fs">ストリーム</param>
        /// <returns></returns>
        public static Bitmap LoadImageFromStream(string filename, Stream fs)
        {
            try
            {
                if (Path.GetExtension(filename).ToLower() == ".ico")
                {
                    //return LoadIcon2(fs);
                    return LoadIcon3(fs);
                }
                else
                {
                    return new Bitmap(Bitmap.FromStream(fs, false, false));
                }
            }
            catch //(Exception e)
            {
                //string[] s = { "読み込みに失敗しました", filename, "", e.Message };
                //return BitmapUty.Text2Bitmap(s, true);
                return null;
            }
        }

        //ver1.10
        //2011年8月19日
        //指定サイズのBitmapに文字を描写
        public static Bitmap Text2Bitmap(string[] str, int bmpwidth, int bmpheight, Color backColor)
        {
            //Bitmapを作る
            Bitmap _bmp = new Bitmap(bmpwidth, bmpheight);

            using (Graphics g = Graphics.FromImage(_bmp))
            using (Font font = new Font(FontName, FontPoint))
            using (SolidBrush brush = new SolidBrush(FontColorValue))
            {
                g.Clear(backColor);

                //テキストサイズを測る
                float textHeight = 0f;      //計測された高さ
                float textHeight1line = 0f;     //計測された一行分の高さ
                float textWidth = 0f;       //計測された幅
                foreach (string s in str)
                {
                    SizeF sizef = g.MeasureString(s, font);
                    if (sizef.Width > textWidth)
                        textWidth = sizef.Width;
                    textHeight1line = sizef.Height;
                    textHeight += sizef.Height + LinePadding;
                }

                RectangleF rect = new RectangleF(
                    (bmpwidth - textWidth) / 2,
                    (bmpheight - textHeight) / 2,
                    textWidth,
                    textHeight1line
                    );

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;      //上下中央
                sf.LineAlignment = StringAlignment.Center;  //センタリング

                foreach (string s in str)
                {
                    SizeF sizef = g.MeasureString(s, font);

                    g.DrawString(s, font, brush, rect, sf);
                    rect.Y += sizef.Height + LinePadding;
                }
            }//using
            return _bmp;
        }

        /// <summary>
        /// 画像サイズを指定の大きさ以下のサイズになるように調整する
        /// 縦横比は固定
        /// </summary>
        /// <param name="orgSize">元のサイズ</param>
        /// <param name="maxLength">一番大きな辺の長さ</param>
        /// <returns>倍率を返す。1倍以上にはならない</returns>
        public static float GetMagnificationWithFixAspectRatio(Size orgSize, int maxLength)
        {
            float ratio = 1.0F;

            if (orgSize.Height > orgSize.Width)
            {
                if (orgSize.Height > maxLength)
                    ratio = (float)maxLength / (float)orgSize.Height;
            }
            else
            {
                if (orgSize.Width > maxLength)
                    ratio = (float)maxLength / (float)orgSize.Width;
            }

            return ratio;
        }

        /// <summary>
        /// Bitmapのコントラストを下げ、白飛ぶする方向に調整
        /// 色合い以外の大きさなどは変更なし
        /// </summary>
        /// <param name="srcBitmap">元のBitmap</param>
        /// <returns>調整された新しいBitmap。</returns>
        public static Bitmap LowContrastBitmap(Bitmap srcBitmap)
        {
            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int writePos;
                for (int y = 0; y < bmpRect.Height; y++)
                {
                    for (int x = 0; x < bmpRect.Width; x++)
                    {
                        //3byte分を処理する
                        writePos = x * 3 + bmpData.Stride * y;
                        offset[writePos + 0] = (byte)(((int)offset[writePos + 0] + 255) / 2); // B;
                        offset[writePos + 1] = (byte)(((int)offset[writePos + 1] + 255) / 2); // G;
                        offset[writePos + 2] = (byte)(((int)offset[writePos + 2] + 255) / 2); // R;
                    }//for x
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// Bitmapの輝度を半分にする。
        /// 単純に半分のため暗くなる
        /// 色合い以外の大きさなどは変更なし
        /// </summary>
        /// <param name="srcBitmap">元のBitmap</param>
        /// <returns>半分の明るさのBitmap</returns>
        public static Bitmap HalfBrightnessBitmap(Bitmap srcBitmap)
        {
            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int endPos = bmpData.Stride * bmpRect.Height - 1;
                for (int pos = 0; pos < endPos; pos++)
                {
                    offset[pos] = (byte)(offset[pos] / 2); // B;
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// コントラストを半分にし、グレイ(128,128,128)に近づくように調整されたBitmapを作成
        /// </summary>
        /// <param name="srcBitmap">元のBitmap</param>
        /// <returns>調整されたBitmap</returns>
        public static Bitmap HalfBrightnessBitmap2(Bitmap srcBitmap)
        {
            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int endPos = bmpData.Stride * bmpRect.Height - 1;
                for (int pos = 0; pos < endPos; pos++)
                {
                    offset[pos] = (byte)((offset[pos] / 2) + 64); // B;
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// ビットマップを暗くする
        /// ０～２５５の範囲で調整
        /// </summary>
        /// <param name="srcBitmap">元となるBitmap</param>
        /// <param name="Level">0～100。０で真っ黒、１００で元の明るさ</param>
        /// <returns></returns>
        public static Bitmap BitmapToDark(Bitmap srcBitmap, int Level)
        {
            if (srcBitmap == null)
                return null;

            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int endPos = bmpData.Stride * bmpRect.Height - 1;
                for (int pos = 0; pos < endPos; pos++)
                {
                    int data = (int)offset[pos] * Level / 100;
                    offset[pos] = (byte)data;
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// サムネイルサイズを求める。
        /// 縦横同一縮尺で高さ、幅が指定サイズを超えないようにする。
        /// </summary>
        /// <param name="x">横の長さ</param>
        /// <param name="y">縦の長さ</param>
        /// <param name="maxLength">最大長</param>
        /// <returns>新しいサイズ</returns>
        public static Size calcThumbnailSize(int x, int y, int maxLength)
        {
            //比率を確定
            double ratioX = (double)maxLength / (double)x;
            double ratioY = (double)maxLength / (double)y;
            double ratio = ratioX < ratioY ? ratioX : ratioY;
            if (ratio > 1) ratio = 1;

            //サイズを確定
            int lx = (int)((double)x * ratio);
            int ly = (int)((double)y * ratio);
            if (lx == 0) lx = 1;
            if (ly == 0) ly = 1;
            return new Size(lx, ly);
        }

        public static Size calcThumbnailSize(Size s, int maxLength)
        {
            return calcThumbnailSize(s.Width, s.Height, maxLength);
        }

        /// <summary>
        /// 指定サイズのサムネイルを作成
        /// </summary>
        /// <param name="orgImage">元の画像</param>
        /// <param name="maxLength">最大長</param>
        /// <returns>新しいサムネイルBitmap</returns>
        public static Bitmap MakeThumbnailImage(Bitmap orgImage, int maxLength)
        {
            if (orgImage == null)
                return null;

            if (orgImage.Width <= maxLength
                && orgImage.Height <= maxLength)
                return orgImage.Clone() as Bitmap;

            Size s = BitmapUty.calcThumbnailSize(orgImage.Width, orgImage.Height, maxLength);
            Bitmap _bmp = new Bitmap(s.Width, s.Height);

            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(orgImage, 0, 0, s.Width, s.Height);
            }
            return _bmp;
        }

        public static Bitmap MakeHeightFixThumbnailImage(Bitmap orgImage, int maxHeight)
        {
            if (orgImage == null)
                return null;

            //小さい場合はコピーを作って返す
            if (orgImage.Height <= maxHeight)
                return orgImage.Clone() as Bitmap;

            //新しいサイズを計算
            Size s = BitmapUty.calcHeightFixImageSize(orgImage.Size, maxHeight);

            //サムネイルを作成
            Bitmap _bmp = new Bitmap(s.Width, s.Height);
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(orgImage, 0, 0, s.Width, s.Height);
            }

            return _bmp;
        }

        public static Size calcHeightFixImageSize(Size orgSize, int maxHeight)
        {
            if (orgSize.Height <= maxHeight)
                return orgSize;

            double ratio = (double)maxHeight / (double)orgSize.Height;
            int width = (int)((double)orgSize.Width * ratio);
            if (width == 0)
                width = 1;
            return new Size(width, maxHeight);
        }

        /// <summary>
        /// Loadingと表示するダミーのBitmapを作成する
        /// サムネイルとして利用
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>作成されたBitmap</returns>
        public static Bitmap LoadingImage(int width, int height)
        {
            Bitmap _bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(_bmp))
            using (Font fontS = new Font("ＭＳ Ｐ ゴシック", 9F, FontStyle.Bold))
            {
                //ver1.29
                //ダミーサムネイルのサイズ
                Rectangle rect = new Rectangle(
                    1, 1,
                    _bmp.Width - 1, _bmp.Height - 1);
                //グレイで塗りつぶし
                g.FillRectangle(Brushes.Gray, rect);
                //明るいグレイで枠描写
                g.DrawRectangle(Pens.LightGray, rect);

                StringFormat sf = new StringFormat()
                {
                    //上下左右中央
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    //文字列が表示しきれないときに"..."を表示する
                    Trimming = StringTrimming.EllipsisPath,
                };

                //テキスト描写フォント

                //Loadingと表示
                g.DrawString(
                    "Loading",
                    fontS,
                    Brushes.LightGray,
                    rect,
                    sf);
            }
            return _bmp;
        }

        /// <summary>
        /// スクリーンサイズに応じたサイズに画像を縮小する。
        /// 縦横比を一定にして縮小
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="screenSize"></param>
        /// <returns></returns>
        public static Bitmap MakeFittedBitmap(Bitmap bmp, Size screenSize)
        {
            if (bmp == null)
                return null;

            //縮尺サイズを得る
            double rx = (double)screenSize.Width / (double)bmp.Width;
            double ry = (double)screenSize.Height / (double)bmp.Height;

            //小さいほうを使う
            double r = rx < ry ? rx : ry;
            if (r > 1.0f && App.Config.noEnlargeOver100p)
                r = 1.0f;

            Bitmap newbmp = new Bitmap(screenSize.Width, screenSize.Height);
            using (Graphics g = Graphics.FromImage(newbmp))
            {
                //画像描写モード：低品質に
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.Clear(App.Config.BackColor);
                //中心に描写
                double width = (double)bmp.Width * r;
                double height = (double)bmp.Height * r;
                double sx = (newbmp.Width - width) / 2;
                double sy = (newbmp.Height - height) / 2;
                g.DrawImage(bmp, (int)sx, (int)sy, (int)width, (int)height);
            }
            return newbmp;
        }

        /// <summary>
        /// ver1.79 真四角なサムネイルを作る
        /// 幅を見て可能な限り小さくした後真ん中を切りぬく
        /// </summary>
        /// <param name="orgImage">元となる画像</param>
        /// <param name="maxLen">一辺の長さ</param>
        /// <returns></returns>
        public static Bitmap MakeSquareThumbnailImage(Bitmap orgImage, int maxLen)
        {
            if (orgImage == null)
                return null;

            //小さい場合はコピーを作って返す
            if (orgImage.Width <= maxLen && orgImage.Height <= maxLen)
                return orgImage.Clone() as Bitmap;

            Bitmap bmp = null;  //大きめのサムネイル
            Rectangle srcRect;  //切り抜く位置
            Rectangle destRect = new Rectangle(0, 0, maxLen, maxLen);
            if (orgImage.Width >= orgImage.Height)
            {
                //横長画像なので高さをmaxLenに
                bmp = MakeHeightFixThumbnailImage(orgImage, maxLen);
                srcRect = new Rectangle((bmp.Width - maxLen) / 2, 0, maxLen, maxLen);
            }
            else
            {
                //縦長画像
                bmp = MakeWidthFixThumbnailImage(orgImage, maxLen);
                srcRect = new Rectangle(0, (bmp.Height - maxLen) / 2, maxLen, maxLen);
            }

            //真四角のサムネイルを作る
            Bitmap retbmp = new Bitmap(maxLen, maxLen);
            using (Graphics g = Graphics.FromImage(retbmp))
            {
                g.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
            }
            return retbmp;
        }

        //
        /// <summary>
        /// ver1.79 幅固定のサムネイルを作る
        /// SquareThumbnail専用に利用
        /// 高さ固定は既にあり。
        /// </summary>
        /// <param name="orgImage">元となる画像</param>
        /// <param name="maxWidth">横幅</param>
        /// <returns></returns>
        private static Bitmap MakeWidthFixThumbnailImage(Bitmap orgImage, int maxWidth)
        {
            if (orgImage == null)
                return null;

            //小さい場合はコピーを作って返す
            if (orgImage.Width <= maxWidth)
                return orgImage.Clone() as Bitmap;

            //新しいサイズを計算
            double ratio = (double)maxWidth / (double)orgImage.Width;
            Size s = new Size((int)(orgImage.Width * ratio), (int)(orgImage.Height * ratio));

            //サムネイルを作成
            Bitmap _bmp = new Bitmap(s.Width, s.Height);
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(orgImage, 0, 0, s.Width, s.Height);
            }
            return _bmp;
        }

        /// <summary>
        /// ver1.83 アンシャープ処理
        /// ラプラシアン処理をして差分を取ることでエッジを作る。
        /// unsafeなので注意
        /// 経験的には係数k=30 =(0.3倍）ぐらいがよさそう。
        ///
        /// 並列処理でさらに早くなるが
        /// .net3.5 ではParallelをコメントアウト
        /// </summary>
        /// <param name="srcbmp">元となる画像</param>
        /// <param name="k">アンシャープの強さ。100まで</param>
        /// <returns></returns>
        public static Bitmap Unsharpness_unsafe(Bitmap srcbmp, int k)
        {
            int lx = srcbmp.Width;      //対象画像のサイズ：横
            int ly = srcbmp.Height;     //対象画像のサイズ：縦
            Bitmap destbmp = new Bitmap(lx, ly);
            Stopwatch sw = Stopwatch.StartNew();

            //3x3ラプラシアン用マトリクス
            //ループ内部で展開したので使わない
            //int[,] m = new int[3, 3]{
            //	{-1,-1,-1},
            //	{-1, 9,-1},
            //	{-1,-1,-1}
            //};

            //画像をロックするための下準備
            Rectangle sRect = new Rectangle(0, 0, lx, ly);
            Rectangle dRect = new Rectangle(0, 0, lx, ly);
            BitmapData srcData = srcbmp.LockBits(
                                    sRect,
                                    ImageLockMode.ReadOnly,
                                    PixelFormat.Format24bppRgb);
            BitmapData destData = destbmp.LockBits(
                                    dRect,
                                    ImageLockMode.WriteOnly,
                                    PixelFormat.Format24bppRgb);

            unsafe
            {
                //ポインタの取得
                byte* pSrc = (byte*)srcData.Scan0;      //読み込み位置の先頭を表す
                byte* pDest = (byte*)destData.Scan0;    //書き込み位置の先頭を表す

                //クリッピング処理 最上段はただのコピー;
                for (int x = 0; x < lx * 3; x++)
                    pDest[x] = pSrc[x];
                //クリッピング処理 最下段もただのコピー;
                int start = (ly - 1) * srcData.Stride;
                for (int x = start; x < start + lx * 3; x++)
                    pDest[x] = pSrc[x];

                //Parallel.For(1, ly - 1, y =>
                for (int y = 1; y < ly - 1; y++)
                {
                    //Parallelのためにループの中に記載
                    int R = 0;          //RGB計算用変数
                    int G = 0;          //RGB計算用変数
                    int B = 0;          //RGB計算用変数
                    int readPos = 0;    //ドット読み込み位置（byte offset）
                    int writePos = 0;   //ドット書き込み位置（byte offset）
                    int RR, GG, BB;     //中心ドット保存用。

                    //クリッピング x=0
                    readPos = srcData.Stride * y;
                    writePos = srcData.Stride * y;
                    pDest[writePos + 0] = pSrc[readPos + 0];
                    pDest[writePos + 1] = pSrc[readPos + 1];
                    pDest[writePos + 2] = pSrc[readPos + 2];

                    //クリッピング x=lx-1
                    readPos = lx - 1 + srcData.Stride * y;
                    writePos = lx - 1 + srcData.Stride * y;
                    pDest[writePos + 0] = pSrc[readPos + 0];
                    pDest[writePos + 1] = pSrc[readPos + 1];
                    pDest[writePos + 2] = pSrc[readPos + 2];

                    for (int x = 1; x < lx - 1; x++)
                    {
                        R = G = B = 0;

                        //上の列
                        readPos = (x - 1) * 3 + srcData.Stride * (y - 1);
                        B += -1 * pSrc[readPos + 0];
                        G += -1 * pSrc[readPos + 1];
                        R += -1 * pSrc[readPos + 2];
                        B += -1 * pSrc[readPos + 3];
                        G += -1 * pSrc[readPos + 4];
                        R += -1 * pSrc[readPos + 5];
                        B += -1 * pSrc[readPos + 6];
                        G += -1 * pSrc[readPos + 7];
                        R += -1 * pSrc[readPos + 8];
                        //中段
                        readPos = (x - 1) * 3 + srcData.Stride * (y);
                        B += -1 * pSrc[readPos + 0];
                        G += -1 * pSrc[readPos + 1];
                        R += -1 * pSrc[readPos + 2];
                        B += 8 * pSrc[readPos + 3];
                        G += 8 * pSrc[readPos + 4];
                        R += 8 * pSrc[readPos + 5];
                        BB = pSrc[readPos + 3];
                        GG = pSrc[readPos + 4];
                        RR = pSrc[readPos + 5];
                        //B += 9 * pSrc[readPos + 3];
                        //G += 9 * pSrc[readPos + 4];
                        //R += 9 * pSrc[readPos + 5];
                        B += -1 * pSrc[readPos + 6];
                        G += -1 * pSrc[readPos + 7];
                        R += -1 * pSrc[readPos + 8];
                        //下段
                        readPos = (x - 1) * 3 + srcData.Stride * (y + 1);
                        B += -1 * pSrc[readPos + 0];
                        G += -1 * pSrc[readPos + 1];
                        R += -1 * pSrc[readPos + 2];
                        B += -1 * pSrc[readPos + 3];
                        G += -1 * pSrc[readPos + 4];
                        R += -1 * pSrc[readPos + 5];
                        B += -1 * pSrc[readPos + 6];
                        G += -1 * pSrc[readPos + 7];
                        R += -1 * pSrc[readPos + 8];

                        //ラプラシアンに倍率をかけ、引き算をする
                        R = RR + (int)(R * k / 100);
                        G = GG + (int)(G * k / 100);
                        B = BB + (int)(B * k / 100);

                        //補正
                        if (R < 0) R = 0;
                        if (G < 0) G = 0;
                        if (B < 0) B = 0;
                        if (R > 255) R = 255;
                        if (G > 255) G = 255;
                        if (B > 255) B = 255;

                        //書き込み：３はRGBの3byteを表すMagicNumber
                        writePos = (x) * 3 + srcData.Stride * (y);
                        pDest[writePos + 0] = (byte)B;
                        pDest[writePos + 1] = (byte)G;
                        pDest[writePos + 2] = (byte)R;
                    }
                }
                //); //Parallel.For
            }//unsafe

            //Bitmapの解放
            srcbmp.UnlockBits(srcData);
            destbmp.UnlockBits(destData);

            Debug.WriteLine(sw.ElapsedMilliseconds, "unsharpness_unsafe()");
            return destbmp;
        }
    }
}
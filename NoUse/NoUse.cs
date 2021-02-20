using System;
using System.Collections.Generic;
using System.Text;

namespace Marmi
{
    class NoUse
    {
        /// <summary>
        /// ステータスバーの表示。
        /// 使えるかと思ったが全画面モードの特殊性に対応できず
        /// グローバルコンフィグも書き換えてしまうのであとで戻せない
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        private void setStatusbarVisible(bool isVisible)
        {
            statusStrip1.Visible = isVisible;
            g_Config.visibleStatusBar = isVisible;
            MenuItem_ViewStatusbar.Checked = isVisible;
            MenuItem_ContextStatusbar.Checked = isVisible;
        }

        /// <summary>
        /// ツールバーの表示。全画面モードに対応できず
        /// グローバルコンフィグも書き換えてしまうのであとで戻せない
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        private void setToolbarVisible(bool isVisible)
        {
            toolStrip1.Visible = isVisible;
            g_Config.visibleToolBar = isVisible;
            MenuItem_ViewToolbar.Checked = isVisible;
            MenuItem_ContextToolbar.Checked = isVisible;
        }

        /// <summary>
        /// メニューバーの表示。全画面モードに対応できず
        /// グローバルコンフィグも書き換えてしまうのであとで戻せない
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        private void setMenubarVisible(bool isVisible)
        {
            menuStrip1.Visible = isVisible;
            g_Config.visibleMenubar = isVisible;
            MenuItem_ViewMenubar.Checked = isVisible;
            MenuItem_ContextMenubar.Checked = isVisible;
        }

        /// <summary>
        /// TEMPフォルダを作っている場合は削除する
        /// </summary>
        private void DeleteTempFolder()
        {
            if (Directory.Exists(TEMP_FOLDER))
            {
                try
                {
                    Directory.Delete(TEMP_FOLDER, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        //ver0.62 画面サイズのg_bmpへの描写.2枚描写、逆方向対応.現在使っていない
        private void DrawImageToScreen3(int nIndex, int direction)
        {
            Cursor cc = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //引数の正規化
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;
            direction = (direction >= 0) ? 1 : -1;

            //とりあえず1枚読め！
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                //TODO:ファイル先頭末尾以降の参照。エラー処理する。
                viewPages = 0;
                DrawImageToGBMP3(null, null);
                return;
            }

            if (!g_Config.dualView
                || bmp1.Width > bmp1.Height)
            {
                //1画面モード確定
                viewPages = 1;      //1枚表示モード
                DrawImageToGBMP3(bmp1, null);
                this.Refresh();
            }
            else
            {
                //2画面モードの疑い有り
                int next = nIndex + direction;
                Bitmap bmp2 = GetBitmap(nIndex + direction);
                if (bmp2 == null || bmp2.Width > bmp2.Height)
                {
                    //1画面モード確定
                    viewPages = 1;      //1枚表示モード
                    DrawImageToGBMP3(bmp1, null);
                    this.Refresh();
                }
                else
                {
                    //2画面モード確定
                    viewPages = 2;      //2枚表示モード
                                        //g_bmp = new Bitmap(
                                        //    bmp1.Width + bmp2.Width,
                                        //    bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height);
                    using (Graphics g = Graphics.FromImage(g_bmp))
                    {
                        if (direction > 0)
                        {
                            //正方向2枚表示
                            DrawImageToGBMP3(bmp1, bmp2);
                        }
                        else
                        {
                            //逆方向2枚表示
                            DrawImageToGBMP3(bmp2, bmp1);
                            g_pi.ViewPage--;    //1つ前にしておく
                        }
                    }
                    //bmp1.Dispose();	//破棄しちゃ駄目！cacheがおかしくなる
                    //bmp2.Dispose();	//破棄しちゃ駄目！cacheから消えてしまう
                    this.Refresh();
                }
            }

            Cursor.Current = cc;
            return;
        }

        //ver0.62 DrawImageToScreen3()から呼び出される。
        private void DrawImageToGBMP3(Bitmap bmp1, Bitmap bmp2)
        {
            if (g_bmp == null)
                g_bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height, PixelFormat.Format24bppRgb);

            Graphics g = Graphics.FromImage(g_bmp);
            Rectangle cr = ClientRectangle; // this.Bounds; //this.DisplayRectangle;

            //高さ補正。ツールバーの高さをClientRectanble/Boundsから補正
            int toolbarHeight = 0;
            //if (toolStrip1.Visible && !toolButtonFullScreen.Checked)	//全画面モードではなく、toolbarが表示されているとき
            if (toolStrip1.Visible && !g_Config.isFullScreen)   //全画面モードではなく、toolbarが表示されているとき
                toolbarHeight = toolStrip1.Height;
            cr.Height -= toolbarHeight;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(g_Config.BackColor);    //これは必要か？
            if (bmp1 == null)
                return;

            float ratio = 1.0f;
            if (bmp2 == null)
            {
                //1枚描写モード
                //g_bmpを縦横比１：１で表示する。100%未満は縮小表示
                //ratioは小さいほうにあわせる
                viewPages = 1;      //1枚表示モード
                float ratioX = (float)cr.Width / (float)bmp1.Width;
                float ratioY = (float)cr.Height / (float)bmp1.Height;
                ratio = (ratioX > ratioY) ? ratioY : ratioX;
                if (ratio >= 1) ratio = 1.0F;
                if (ratio == 0) ratio = 1.0F;

                int width = (int)(bmp1.Width * ratio);
                int height = (int)(bmp1.Height * ratio);

                g.DrawImage(
                    bmp1,                                       //描写イメージ
                    (cr.Width - width) / 2,                     //始点X
                    (cr.Height - height) / 2 + toolbarHeight,   //始点Y
                    width,                                      //幅
                    height                                      //高さ
                );
            }
            else
            {
                //2枚描写モード
                viewPages = 2;      //2枚表示モード
                int width = bmp1.Width + bmp2.Width;
                int height = bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height;
                float ratioX = (float)cr.Width / (float)width;
                float ratioY = (float)cr.Height / (float)height;
                ratio = (ratioX > ratioY) ? ratioY : ratioX;
                if (ratio >= 1) ratio = 1.0F;
                if (ratio == 0) ratio = 1.0F;

                //真ん中のためのoffset計算
                float offsetX = ((float)cr.Width - width * ratio) / 2;
                float offsetY = ((float)cr.Height - height * ratio) / 2 + toolbarHeight;

                //bmp2は左に描写
                g.DrawImage(
                    bmp2,
                    0 + offsetX,
                    0 + offsetY,
                    bmp2.Width * ratio,
                    bmp2.Height * ratio
                    );

                //bmp1は右に描写
                g.DrawImage(
                    bmp1,
                    bmp2.Width * ratio + offsetX,
                    0 + offsetY,
                    bmp1.Width * ratio,
                    bmp1.Height * ratio
                    );
            }

            //ステータスバーに倍率表示
            setStatubarRatio(ratio);
        }

        //ver0.81 もっと簡潔に
        /// <summary>
        /// 画面ぴったりサイズのg_bmpを生成する
        /// </summary>
        /// <param name="nIndex"></param>
        private void DrawImageToGBMP4(int nIndex)
        {
            //引数の正規化
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;

            //カーソルの設定
            Cursor.Current = Cursors.WaitCursor;

            //1枚は先読みしておく
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                viewPages = 0;
                DrawGBMP4(null, null);
                Cursor.Current = Cursors.Default;
                return;
            }

            if (g_Config.dualView && CanDualView(nIndex))
            {
                //2枚表示
                Bitmap bmp2 = GetBitmap(nIndex + 1);
                DrawGBMP4(bmp1, bmp2);
                this.Refresh();
            }
            else
            {
                //1枚表示
                DrawGBMP4(bmp1, null);
                this.Refresh();
            }

            Cursor.Current = Cursors.Default;
            return;
        }

        //ver0.81 もっと簡潔に
        /// <summary>
        /// DrawImageToGBMP4()から呼び出しされるルーチン
        /// </summary>
        /// <param name="bmp1"></param>
        /// <param name="bmp2"></param>
        private void DrawGBMP4(Bitmap bmp1, Bitmap bmp2)
        {
            Rectangle cr = GetClientRectangle();
            if (g_bmp == null || g_bmp.Size != cr.Size)
                g_bmp = new Bitmap(cr.Width, cr.Height, PixelFormat.Format24bppRgb);

            if (bmp1 == null)
                return;

            using (Graphics g = Graphics.FromImage(g_bmp))
            {
                g.Clear(g_Config.BackColor);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                float ratio = 1.0f;
                if (bmp2 == null)
                {
                    //1枚描写モード
                    //g_bmpを縦横比１：１で表示する。100%未満は縮小表示
                    //ratioは小さいほうにあわせる
                    viewPages = 1;      //1枚表示モード
                    float ratioX = (float)cr.Width / (float)bmp1.Width;
                    float ratioY = (float)cr.Height / (float)bmp1.Height;
                    ratio = (ratioX > ratioY) ? ratioY : ratioX;
                    if (ratio >= 1) ratio = 1.0F;
                    if (ratio == 0) ratio = 1.0F;

                    int width = (int)(bmp1.Width * ratio);
                    int height = (int)(bmp1.Height * ratio);

                    g.DrawImage(
                        bmp1,                       //描写イメージ
                        (cr.Width - width) / 2,     //始点X
                        (cr.Height - height) / 2,   //始点Y
                        width,                      //幅
                        height                      //高さ
                    );
                }
                else
                {
                    //2枚描写モード
                    viewPages = 2;      //2枚表示モード
                    int width = bmp1.Width + bmp2.Width;
                    int height = bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height;
                    float ratioX = (float)cr.Width / (float)width;
                    float ratioY = (float)cr.Height / (float)height;
                    ratio = (ratioX > ratioY) ? ratioY : ratioX;
                    if (ratio >= 1) ratio = 1.0F;
                    if (ratio == 0) ratio = 1.0F;

                    //真ん中のためのoffset計算
                    float offsetX = ((float)cr.Width - width * ratio) / 2;
                    float offsetY = ((float)cr.Height - height * ratio) / 2;

                    //bmp2は左に描写
                    g.DrawImage(
                        bmp2,
                        0 + offsetX,
                        0 + offsetY,
                        bmp2.Width * ratio,
                        bmp2.Height * ratio
                        );

                    //bmp1は右に描写
                    g.DrawImage(
                        bmp1,
                        bmp2.Width * ratio + offsetX,
                        0 + offsetY,
                        bmp1.Width * ratio,
                        bmp1.Height * ratio
                        );
                }
                //ステータスバーに倍率表示
                setStatubarRatio(ratio);
            }//using
        }

        /// <summary>
        /// Form1_Paintから呼ばれているモジュール。
        /// PaintGBMP()でスクロールバー対応したため取って代われた。
        /// </summary>
        /// <param name="g"></param>
        private void RenderGBMP_FittingSize(Graphics g)
        {
            //Rectangle rect = this.ClientRectangle; // this.Bounds;

            ////高さ補正。ツールバーの高さをClientRectanble/Boundsから補正
            //int toolbarHeight = 0;
            //if (toolStrip1.Visible && !toolButtonFullScreen.Checked)	//全画面モードではなく、toolbarが表示されているとき
            //    toolbarHeight = toolStrip1.Height;
            //rect.Height -= toolbarHeight;

            Rectangle rect = GetClientRectangle();

            //g_bmpを縦横比１：１で表示する。100%未満は縮小表示
            //ratioは小さいほうにあわせる
            float ratioX = (float)rect.Width / (float)g_bmp.Width;
            float ratioY = (float)rect.Height / (float)g_bmp.Height;
            float ratio = (ratioX > ratioY) ? ratioY : ratioX;
            if (ratio >= 1 || ratio <= 0) ratio = 1.0F;

            int width = (int)(g_bmp.Width * ratio);
            int height = (int)(g_bmp.Height * ratio);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(g_Config.BackColor);
            g.DrawImage(
                g_bmp,                                      //描写イメージ
                (rect.Width - width) / 2,                   //始点X
                (rect.Height - height) / 2 + rect.Top,      //始点Y
                width,                                      //幅
                height                                      //高さ
            );

            //ステータスバーに倍率表示
            setStatubarRatio(ratio);
        }

        //g_bmpへの描写.2枚描写、逆方向対応
        private void DrawImageToGBMP(int nIndex, int direction)
        {
            //引数の正規化
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;

            Cursor cc = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            direction = (direction >= 0) ? 1 : -1;

            //とりあえず1枚読め！
            //g_bmp = GetBitmap(nIndex);
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                //TODO:ファイル先頭末尾以降の参照。エラー処理する。
                viewPages = 0;
                return;
            }

            //表示するモノはあるのでg_bmpをクリアする。
            if (g_bmp != null)
                g_bmp.Dispose();

            if (!g_Config.dualView
                || bmp1.Width > bmp1.Height)
            {
                //1画面モード確定
                viewPages = 1;      //1枚表示モード
                g_bmp = (Bitmap)bmp1.Clone();
                this.Refresh();
            }
            else
            {
                //2画面モードの疑い有り
                int next = nIndex + direction;
                Bitmap bmp2 = GetBitmap(nIndex + direction);
                if (bmp2 == null || bmp2.Width > bmp2.Height)
                {
                    //1画面モード確定
                    viewPages = 1;      //1枚表示モード
                    g_bmp = (Bitmap)bmp1.Clone();
                    this.Refresh();
                }
                else
                {
                    //2画面モード確定
                    viewPages = 2;      //2枚表示モード
                    g_bmp = new Bitmap(
                        bmp1.Width + bmp2.Width,
                        bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height);
                    using (Graphics g = Graphics.FromImage(g_bmp))
                    {
                        if (direction > 0)
                        {
                            //正方向2枚表示
                            g.DrawImage(bmp2, 0, 0, bmp2.Width, bmp2.Height);
                            g.DrawImage(bmp1, bmp2.Width, 0, bmp1.Width, bmp1.Height);
                        }
                        else
                        {
                            //逆方向2枚表示
                            g.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);
                            g.DrawImage(bmp2, bmp1.Width, 0, bmp2.Width, bmp2.Height);
                            g_pi.ViewPage--;    //1つ前にしておく
                        }
                    }
                    //bmp1.Dispose();	//破棄しちゃ駄目！cacheがおかしくなる
                    //bmp2.Dispose();	//破棄しちゃ駄目！cacheから消えてしまう
                    this.Refresh();
                }
            }

            Cursor.Current = cc;
            return;
        }

        //このバージョンではSharpZipからのストリームに対応できない
        private static Bitmap LoadIcon(Stream fs)
        {
            try
            {
                using (Icon ico = new Icon(fs))
                {
                    return ico.ToBitmap();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("アイコン生成失敗。リトライ");
                fs.Seek(0, 0);  //'System.NotSupportedException' の初回例外が ICSharpCode.SharpZipLib.dll で発生しました。
                Bitmap b = (Bitmap)Bitmap.FromStream(fs, false, false);
                if (b == null)
                    throw e;
                else
                {
                    Debug.WriteLine("アイコン再生成成功");
                    return b;
                }
            }
        }

        //SharpZipからのストリームに対応
        private static Bitmap LoadIcon2(Stream fs)
        {
            //SharpZipのストリームはSeek()などに対応できない。
            //なのでMemoryStreamに一度取り込んで利用する
            using (MemoryStream ms = new MemoryStream())
            {
                //取り込み
                int len;
                byte[] buffer = new byte[16384];
                while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, len);

                //先頭にRewind
                ms.Seek(0, SeekOrigin.Begin);   //重要！
                                                //アイコン読み取り開始
                try
                {
                    using (Icon ico = new Icon(ms))
                    {
                        return ico.ToBitmap();
                    }
                }
                catch (Exception e)
                {
                    ms.Seek(0, 0);  //重要！
                    Debug.WriteLine("アイコン生成失敗。リトライ");
                    Bitmap b = new Bitmap(Bitmap.FromStream(ms, false, false));
                    if (b == null)
                        throw e;
                    else
                    {
                        Debug.WriteLine("アイコン再生成成功");
                        return b;
                    }
                }
            }
        }

        //アイコンファイル解析版
        private static Bitmap LoadIcon3(Stream fs)
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
    }

    /********************************************************************************/
    // ソート用比較クラス
    // 自然言語ソートするための比較クラス
    /********************************************************************************/
    public class NaturalOrderComparer : IComparer<string>
    {
        private int col;            //コラム：利用しない
        private int SortOrder;      //ソートの方向。正順が1、逆順は-1

        //private enum SortModeEnum : int
        //{
        //    None = 0,			//ソートしない
        //    ascending = 1,		//ascendng:昇順
        //    descending = 2		//descending:降順
        //}

        public NaturalOrderComparer()
        {
            col = 0;
            SortOrder = 1;
        }

        public int Compare(string x, string y)
        {
            return SortOrder * NaturalOrderCompare(x, y);
        }

        public int SimpleCompare(object x, object y)
        {
            //単純比較関数
            return String.Compare(
                ((ListViewItem)x).SubItems[col].Text,
                ((ListViewItem)y).SubItems[col].Text
            );
        }

        public int NaturalOrderCompare(string s1, string s2)
        {
            //数値を一度変換し長さによらない比較をする。
            //XP以降のソート相当に対応したはず・・・

            //階層をチェック
            int lev1 = 0;   //xの階層
            int lev2 = 0;   //yの階層
            for (int i = 0; i < s1.Length; i++)
                if (s1[i] == '/' || s1[i] == '\\') lev1++;
            for (int i = 0; i < s2.Length; i++)
                if (s2[i] == '/' || s2[i] == '\\') lev2++;

            if (lev1 != lev2)
                return lev1 - lev2;

            //
            // 同一階層なので1文字ずつチェックを開始する
            //
            int p1 = 0;     // s1を指すポインタ
            int p2 = 0;     // s2を指すポインタ
            long num1 = 0;  // s1に含まれる数値。大きな数値に対応させるためlong
            long num2 = 0;  // s2に含まれる数値。大きな数値に対応させるためlong

            do
            {
                char c1 = s1[p1];
                char c2 = s2[p2];

                //c1とc2の比較を開始する前に数字だったら数値モードへ
                //数値モードの場合は数値に変換して比較
                if (c1 >= '0' && c1 <= '9' && c2 >= '0' && c2 <= '9')
                {
                    //s1系列の文字を数値num1に変換
                    num1 = 0;
                    while (c1 >= '0' && c1 <= '9')
                    {
                        num1 = num1 * 10 + c1 - '0';
                        ++p1;
                        if (p1 >= s1.Length)
                            break;
                        c1 = s1[p1];
                    }

                    //s2系列の文字を数値num2に変換
                    num2 = 0;
                    while (c2 >= '0' && c2 <= '9')
                    {
                        num2 = num2 * 10 + c2 - '0';
                        ++p2;
                        if (p2 >= s2.Length)
                            break;
                        c2 = s2[p2];
                    }

                    //数値として比較
                    if (num1 != num2)
                        return (int)(num1 - num2);
                }
                else
                {
                    //単一文字として比較
                    if (c1 != c2)
                        return (int)(c1 - c2);
                    ++p1;
                    ++p2;
                }
            }
            while (p1 < s1.Length && p2 < s2.Length);

            //どちらかが終端に達した。あとは長い方が後ろ。
            return s1.Length - s2.Length;
        }
    }

    /********************************************************************************/
    //時間が来ると自動的に消えるラベル
    /********************************************************************************/
    public class InformationLabel : Label
    {
        private System.Windows.Forms.Timer t1 = null;
        private const int TimerInterval = 1000;

        /// <summary>
        /// コンストラクタ。位置指定可能
        /// </summary>
        /// <param name="parentControl">Labelを追加するコントロール</param>
        /// <param name="x">表示位置x(Left)</param>
        /// <param name="y">表示位置y(Top)</param>
        /// <param name="sz">表示する文字列</param>
        public InformationLabel(Control parentControl, int x, int y, string sz)
        {
            this.Name = sz;
            this.Text = sz;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Left = x;
            this.Top = y;
            this.Enabled = true;
            this.Visible = true;
            parentControl.Controls.Add(this);
            this.Show();

            t1 = new System.Windows.Forms.Timer();
            t1.Interval = TimerInterval;
            t1.Tick += new EventHandler(t1_Tick);
            t1.Start();
            Debug.WriteLine("Show InformationLabel");
        }

        /// <summary>
        /// コンストラクタ。オブジェクトの真ん中に表示する
        /// </summary>
        /// <param name="parentControl">Labelを追加するオブジェクト</param>
        /// <param name="sz">表示する文字列</param>
        public InformationLabel(Control parentControl, string sz)
        {
            this.Name = sz;
            this.Text = sz;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Enabled = true;
            this.Visible = true;

            int cx = parentControl.Width;
            int cy = parentControl.Height;
            this.Left = (cx - this.PreferredWidth) / 2;
            this.Top = (cy - this.PreferredHeight) / 2;

            parentControl.Controls.Add(this);
            this.Show();

            t1 = new System.Windows.Forms.Timer();
            t1.Interval = TimerInterval;
            t1.Tick += new EventHandler(t1_Tick);
            t1.Start();
            Debug.WriteLine("Show InformationLabel");
        }

        /// <summary>
        /// タイマーTick呼び出し用関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void t1_Tick(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            t1.Stop();
            t1.Dispose();
            this.Dispose();
            Debug.WriteLine("Dispose InformationLabel");
        }
    }

    /// <summary>
    /// 弱い参照によるキャッシュクラス
    /// うまく作ったつもりだが、どうしてもBitmapが意図せず参照できないことがあるので
    /// 利用中止
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Cache<TKey, TValue>
    {
        // キャッシュを保存するDictionary
        static Dictionary<TKey, WeakReference> _cache;

        //コンストラクタ
        public Cache()
        {
            _cache = new Dictionary<TKey, WeakReference>();
        }

        /// <summary>
        /// キャッシュへのアイテムの追加
        /// </summary>
        /// <param name="key">アイテムを識別するキー</param>
        /// <param name="obj">アイテム</param>
        /// <param name="isLongCache">キャッシュ保持を長くするアイテムの場合はtrue</param>
        public void Add(TKey key, TValue obj, bool isLongCache)
        {
            _cache.Add(
                key,
                new WeakReference(obj, isLongCache)
                );
        }

        public void Add(TKey key, TValue obj)
        {
            Add(key, obj, true);
        }

        /// <summary>
        /// キャッシュに含まれている数量
        /// </summary>
        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        /// <summary>
        /// 指定したキーのアイテムを返す
        /// </summary>
        /// <param name="key">アイテムを指定するキー</param>
        /// <returns>アイテムオブジェクト。消滅している場合はnullを返す</returns>
        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    TValue d = (TValue)_cache[key].Target;
                    return d;
                }
                catch
                {
                    // キーが存在しない場合など
                    return default(TValue);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (!_cache.ContainsKey(key))
                return false;
            try
            {
                TValue d = (TValue)_cache[key].Target;
                if (d != null)
                    return true;
                else
                    return false;
            }
            catch
            {
                // キーが存在しない場合など
                return false;
            }
        }
    }

    /// <summary>
    /// 高速化のためにいらなくなったモジュールをこちらに
    /// </summary>
    public class ThumbnailPanel : UserControl
    {
        /// <summary>
        /// サムネイル一覧の作成メイン関数
        /// 当初安定版。
        /// スレッドなどは使わずにサムネイル一覧をひたすら作る。
        /// リサイズに高速対応しきれないため引退
        /// </summary>
        public void drawThumbnailToOffScreen()
        {
            //描写対象があるかチェックする
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            InitilizeOffScreen();
            using (Graphics g = Graphics.FromImage(m_bmpOffScreen))
            {
                //背景色で塗りつぶす
                g.Clear(this.BackColor);
                for (int i = 0; i < m_thumbnailSet.Count; i++)
                {
                    DrawItem(g, i);
                }//foreach
            } //using(Graphics)

            //スクロールバーを設定
            setScrollBar();
        }

        /// <summary>
        /// サムネイルを描写する
        /// OnResize()専用として画面描写に必要な部分だけ描写ている
        /// </summary>
        public void drawThumbnailToOffScreenFast()
        {
            //描写対象があるかチェックする
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //InitilizeOffScreen();
            int ItemCount = m_thumbnailSet.Count;

            //描写に必要なサイズを確認する。
            //描写領域の大きさ。まずは自分のクライアント領域を得る
            int width = this.ClientRectangle.Width;
            int height = this.ClientRectangle.Height;

            //横に並べられる数。最低１
            int OldNumItemX = numItemX;
            int OldNumItemY = numItemY;
            numItemX = width / BOX_WIDTH;   //横に並ぶアイテム数
            if (numItemX == 0)
                numItemX = 1;

            //縦に必要な数。繰り上げる
            numItemY = ItemCount / numItemX;    //縦に並ぶアイテム数はサムネイルの数による
            if (ItemCount % numItemX > 0)
                numItemY++;

            if (height < numItemY * BOX_HEIGHT)
            {
                //スクロールバーが必要なので再計算
                width -= vScrollBar1.Width;
                numItemX = width / BOX_WIDTH;
                if (numItemX == 0)
                    numItemX = 1;
                numItemY = (ItemCount + numItemX - 1) / numItemX;   //(numX-1)をあらかじめ足しておくことで繰り上げ
                height = numItemY * BOX_HEIGHT;
                vScrollBar1.Visible = true;
                vScrollBar1.Enabled = true;
            }
            else
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Enabled = false;
                vScrollBar1.Value = 0;
            }
            if (width < 1) width = 1;
            if (height < 1) height = 1;

            //if (width <= m_bmpOffScreen.Width
            //    && height <= m_bmpOffScreen.Height
            //    && OldNumItemX == numItemX)
            //{
            //    return;
            //}

            //再描写が必要
            //m_bmpOffScreenを破棄、生成する。再利用できる場合は破棄しない。
            if (m_bmpOffScreen != null)
                m_bmpOffScreen.Dispose();
            m_bmpOffScreen = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(m_bmpOffScreen))
            {
                //背景色で塗りつぶす
                g.Clear(this.BackColor);

                //関係しそうなアイテムだけ描写
                //再描写対象か確認
                for (int Item = 0; Item < ItemCount; Item++)
                {
                    //int ItemX = Item % numItemX;	//アイテムのX描写位置。ドットではなくアイテム番号位置
                    //int sx = ItemX * BOX_WIDTH;		//画像描写X位置
                    int ItemY = Item / numItemX;    //アイテムのY描写位置。ドットではなくアイテム番号位置
                    int sy = ItemY * BOX_HEIGHT;    //画像描写X位置

                    if ((sy + BOX_HEIGHT) > vScrollBar1.Value && sy < (vScrollBar1.Value + this.Height))
                    {
                        DrawItem(g, Item);
                    }
                }
            } //using(Graphics)

            //スクロールバーを設定
            setScrollBar();
        }

        private void drawDropShadow(Graphics g, int sx, int sy, int w, int h)
        {
            GraphicsPath Path = new GraphicsPath(FillMode.Winding);

            //角の丸さ
            Size arcSize = new Size(4, 4);

            //3ドットずらして書く
            //Rectangle rect = new Rectangle(sx + 2, sy + 2, w, h);
            Rectangle rect = new Rectangle(sx + 3, sy + 3, w, h);

            Path.AddArc(rect.Right - arcSize.Width, rect.Top, arcSize.Width, arcSize.Height, 270, 90);
            Path.AddArc(rect.Right - arcSize.Width, rect.Bottom - arcSize.Height, arcSize.Width, arcSize.Height, 0, 90);
            Path.AddArc(rect.Left, rect.Bottom - arcSize.Height, arcSize.Width, arcSize.Height, 90, 90);
            Path.AddArc(rect.Left, rect.Top, arcSize.Width, arcSize.Height, 180, 90);
            Path.AddArc(rect.Right - arcSize.Width, rect.Top, arcSize.Width, arcSize.Height, 270, 90);

            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            using (PathGradientBrush br = new PathGradientBrush(Path))
            {
                // set the wrapmode so that the colors will layer themselves
                // from the outer edge in
                br.WrapMode = WrapMode.Clamp;

                // Create a color blend to manage our colors and positions and
                // since we need 3 colors set the default length to 3
                ColorBlend _ColorBlend = new ColorBlend(3);

                // here is the important part of the shadow making process, remember
                // the clamp mode on the colorblend object layers the colors from
                // the outside to the center so we want our transparent color first
                // followed by the actual shadow color. Set the shadow color to a
                // slightly transparent DimGray, I find that it works best.|
                _ColorBlend.Colors = new Color[]
                        {
                            Color.Transparent,
                            Color.FromArgb(180, Color.DimGray),
                            Color.FromArgb(180, Color.DimGray)
                        };

                // our color blend will control the distance of each color layer
                // we want to set our transparent color to 0 indicating that the
                // transparent color should be the outer most color drawn, then
                // our Dimgray color at about 10% of the distance from the edge
                _ColorBlend.Positions = new float[] { 0f, .1f, 1f };

                // assign the color blend to the pathgradientbrush
                br.InterpolationColors = _ColorBlend;

                // fill the shadow with our pathgradientbrush
                g.FillPath(br, Path);
            }
        }

        private void InitilizeOffScreen()
        {
            int ItemCount = m_thumbnailSet.Count;

            //描写に必要なサイズを確認する。

            //描写領域の大きさ。まずは自分のクライアント領域を得る
            int width = this.ClientRectangle.Width;
            int height = this.ClientRectangle.Height;

            //横に並べられる数。最低１
            numItemX = width / BOX_WIDTH;   //横に並ぶアイテム数
            if (numItemX == 0)
                numItemX = 1;

            //縦に必要な数。繰り上げる
            numItemY = ItemCount / numItemX;    //縦に並ぶアイテム数はサムネイルの数による
            if (ItemCount % numItemX > 0)
                numItemY++;

            if (height < numItemY * BOX_HEIGHT)
            {
                //スクロールバーが必要なので再計算
                width -= vScrollBar1.Width;
                numItemX = width / BOX_WIDTH;
                if (numItemX == 0)
                    numItemX = 1;
                numItemY = (ItemCount + numItemX - 1) / numItemX;   //(numX-1)をあらかじめ足しておくことで繰り上げ
                height = numItemY * BOX_HEIGHT;
                vScrollBar1.Visible = true;
                vScrollBar1.Enabled = true;
            }
            else
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Enabled = false;
                vScrollBar1.Value = 0;
            }
            if (width < 1) width = 1;
            if (height < 1) height = 1;

            //m_bmpOffScreenを破棄、生成する。再利用できる場合は破棄しない。
            if (m_bmpOffScreen != null)
            {
                if (m_bmpOffScreen.Width != width
                    || m_bmpOffScreen.Height != height)
                {
                    m_bmpOffScreen.Dispose();
                    m_bmpOffScreen = null;
                }
            }
            if (m_bmpOffScreen == null)
                m_bmpOffScreen = new Bitmap(width, height);
        }

        /// <summary>
        /// サムネイル一覧の作成メイン関数
        /// スレッド対応版。高速化を目指していたら結局こうなった。
        /// ソート処理など強制再描写する必要がある場合はフラグをtrueにする
        /// </summary>
        /// <param name="isForceRedraw">強制再描写する場合はtrue</param>
        public void MakeThumbnailScreen(bool isForceRedraw)
        {
            //描写対象があるかチェックする
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //InitilizeOffScreen();
            int ItemCount = m_thumbnailSet.Count;

            //描写に必要なサイズを確認する。
            //描写領域の大きさ。まずは自分のクライアント領域を得る
            int width = this.ClientRectangle.Width;
            int height = this.ClientRectangle.Height;

            //横に並べられる数。最低１
            int OldNumItemX = numItemX;
            int OldNumItemY = numItemY;
            numItemX = width / BOX_WIDTH;   //横に並ぶアイテム数
            if (numItemX == 0)
                numItemX = 1;

            //縦に必要な数。繰り上げる
            numItemY = ItemCount / numItemX;    //縦に並ぶアイテム数はサムネイルの数による
            if (ItemCount % numItemX > 0)
                numItemY++;

            if (height < numItemY * BOX_HEIGHT)
            {
                //スクロールバーが必要なので再計算
                width -= vScrollBar1.Width;
                numItemX = width / BOX_WIDTH;
                if (numItemX == 0)
                    numItemX = 1;
                numItemY = (ItemCount + numItemX - 1) / numItemX;   //(numX-1)をあらかじめ足しておくことで繰り上げ
                height = numItemY * BOX_HEIGHT;
                vScrollBar1.Visible = true;
                vScrollBar1.Enabled = true;
            }
            else
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Enabled = false;
                vScrollBar1.Value = 0;
            }
            if (width < 1) width = 1;
            if (height < 1) height = 1;

            //幅が変わらないときは何もせずバイバイ
            if (OldNumItemX == numItemX && isForceRedraw == false)
            {
                return;
            }

            //再描写が必要
            //スレッドを殺す
            if (tStatus == ThreadStatus.RUNNING)
            {
                tStatus = ThreadStatus.REQUEST_STOP;
                WaitForMakeThumbnailThread();
            }

            //m_bmpOffScreenを破棄、生成する。
            if (m_offScreen == null)
                m_offScreen = new Bitmap(width, height);
            else
            {
                lock (m_offScreen)
                {
                    m_offScreen.Dispose();
                    m_offScreen = new Bitmap(width, height);
                }
            }

            //スクロールバーを設定
            setScrollBar();

            //スレッド生成
            //if(mrEvent == null)
            //    mrEvent = new ManualResetEvent(false);
            WaitCallback callback = new WaitCallback(MakeThumbnailThreadProc);
            ThreadPool.QueueUserWorkItem(callback);
        }

        public void WaitForMakeThumbnailThread()
        {
            while (tStatus != ThreadStatus.STOP)
            {
                Application.DoEvents(); //他の描写メッセージが処理されるのは良くない
                                        //Thread.Sleep(10);
            }
        }

        //サムネイル作成をスレッド化するために作った関数
        //大きいアイコン対応のためお蔵入り
        private void MakeThumbnailThreadProc(object o)
        {
            Debug.WriteLine("start MakeThumbnailThreadProc(object o)");
            tStatus = ThreadStatus.RUNNING;

            for (int Item = 0; Item < m_thumbnailSet.Count; Item++)
            {
                if (tStatus != ThreadStatus.RUNNING)
                    break;

                lock (m_offScreen)
                {
                    using (Graphics g = Graphics.FromImage(m_offScreen))    //ここもlockする必要有り
                    {
                        DrawItem(g, Item);      //lock
                                                //this.Invalidate();	//本当は無しにしたいけど時折真っ白なんだよね
                    }//using
                }
            }//for

            this.Invalidate();
            tStatus = ThreadStatus.STOP;
            Debug.WriteLine("stop MakeThumbnailThreadProc(object o)");
        }

        //アイテム描写ルーチン。かなり力作
        //大きいアイコン対応のためお蔵入り
        private void DrawItem(Graphics g, int i)
        {
            //描写品質
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //画像に枠を書くか
            bool drawFrame = true;

            //描写すべきイメージの確認
            Image DrawBitmap = m_thumbnailSet[i].ThumbImage;
            if (DrawBitmap == null)
            {
                DrawBitmap = Properties.Resources.rc_tif48;
                drawFrame = false;
            }

            //描写位置（アイテム番号位置）の決定
            int ItemX = i % numItemX;   //アイテムのX描写位置。ドットではなくアイテム番号位置
            int ItemY = i / numItemX;   //アイテムのY描写位置。ドットではなくアイテム番号位置

            //描写アイテムの大きさを確定
            int w = DrawBitmap.Width;   //描写画像の幅
            int h = DrawBitmap.Height;  //描写画像の高さ
            float ratio = 1;

            if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE)
            {
                if (w > h)
                    ratio = (float)THUMBNAIL_SIZE / (float)w;
                else
                    ratio = (float)THUMBNAIL_SIZE / (float)h;
                //if (ratio > 1)		//これをコメント化すると
                //    ratio = 1.0F;		//拡大描写も行う
                w = (int)(w * ratio);
                h = (int)(h * ratio);
            }

            //画像位置：横方向は真ん中を選出する
            int sx = ItemX * BOX_WIDTH + (BOX_WIDTH - w) / 2;   //画像描写X位置

            //画像位置：縦方向は下揃えする
            //int sy = y * BOX_HEIGHT + (BOX_HEIGHT - h) / 2;	//中央揃え
            int sy = ItemY * BOX_HEIGHT + THUMBNAIL_SIZE + PADDING - h; //画像描写X位置：下揃え

            //影を書く
            //g.FillRectangle(Brushes.LightGray, sx + 1, sy + 1, w, h);  //簡易版
            //drawDropShadow(g, sx, sy, w, h);	//通常版

            //対象矩形を背景色で塗りつぶす.
            //そうしないと前に描いたアイコンが残ってしまう可能性有り
            Brush br = new SolidBrush(BackColor);
            g.FillRectangle(br, ItemX * BOX_WIDTH, ItemY * BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);

            //写真風に外枠を書く
            if (drawFrame)
            {
                Rectangle r = new Rectangle(sx, sy, w, h);
                r.Inflate(2, 2);
                g.FillRectangle(Brushes.White, r);
                g.DrawRectangle(Pens.LightGray, r);
            }

            //画像を書く
            //TODO:このlock()は不要なはずなので問題なければ消す。2009年8月10日
            lock (DrawBitmap)
            {
                g.DrawImage(DrawBitmap, sx, sy, w, h);
            }

            //説明文字を書く
            sx = ItemX * BOX_WIDTH + PADDING;
            sy = ItemY * BOX_HEIGHT + PADDING + THUMBNAIL_SIZE + PADDING;
            string drawString = Path.GetFileName(m_thumbnailSet[i].filename);
            RectangleF rect = new RectangleF(sx, sy, THUMBNAIL_SIZE, TEXT_HEIGHT);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;          //中央揃え
            sf.Trimming = StringTrimming.EllipsisPath;      //中間の省略
                                                            //sf.FormatFlags = StringFormatFlags.NoWrap;		//折り返しを禁止
            g.DrawString(drawString, font, Brushes.Black, rect, sf);
        }

        //サムネイル作成ルーチン（スレッド版）
        //全アイテムをサーチしている。
        //ぎりぎりのアイテムだけ描写するルーチンに取って代わられた。
        private void ThreadProc_Old1(object dummy)
        {
            ////効き目不明
            //if (tStatus == ThreadStatus.RUNNING)
            //    return;

            int ItemCount = m_thumbnailSet.Count;
            tStatus = ThreadStatus.RUNNING;

            //関係しそうなアイテムだけ描写
            for (int Item = 0; Item < ItemCount; Item++)
            {
                if (tStatus == ThreadStatus.REQUEST_STOP)
                    break;

                lock (m_offScreen)
                {
                    DrawItemHQ(Graphics.FromImage(m_offScreen), Item);
                    this.Invalidate();
                }
            }
            tStatus = ThreadStatus.STOP;
        }

        //高速描写対応DrawItem
        //Imageの大きさなど全部自前で処理している
        //外部にはき出したモノがDrawImage3()
        private void DrawItem2(Graphics g, int Item)
        {
            //準備が出来ているか
            if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
            {
                Debug.WriteLine("準備できてないよ", " DrawItem2()");
                return;
            }

            //描写品質
            if (THUMBNAIL_SIZE > DEFAULT_THUMBNAIL_SIZE)
            {
                //描き直すので最低品質で描写する
                //g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.InterpolationMode = InterpolationMode.Bilinear;           //これぐらいの品質でもOKか？
                m_needHQDraw = true;    //描き直しフラグ
            }
            else
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; //最高品質で

            int ItemX = Item % m_nItemsX;   //アイテムのX描写位置。ドットではなくアイテム番号位置
            int sx = ItemX * BOX_WIDTH;     //画像描写X位置
            int ItemY = Item / m_nItemsX;   //アイテムのY描写位置。ドットではなくアイテム番号位置
            int sy = ItemY * BOX_HEIGHT;    //画像描写X位置

            if ((sy + BOX_HEIGHT) > m_vScrollBar.Value && sy < (m_vScrollBar.Value + this.Height))
            {
                //対象矩形を背景色で塗りつぶす.
                //そうしないと前に描いたアイコンが残ってしまう可能性有り
                g.FillRectangle(new SolidBrush(BackColor), sx, sy, BOX_WIDTH, BOX_HEIGHT);

                bool drawFrame = true;

                Image DrawBitmap = m_thumbnailSet[Item].ThumbImage;
                bool isResize = true;   //リサイズが必要か（可能か）どうかのフラグ

                int w;  //描写画像の幅
                int h;  //描写画像の高さ

                if (DrawBitmap == null)
                {
                    //まだサムネイルは準備できていないので画像マークを呼んでおく
                    DrawBitmap = Properties.Resources.rc_tif32;
                    drawFrame = false;
                    isResize = false;
                    w = DrawBitmap.Width;   //描写画像の幅
                    h = DrawBitmap.Height;  //描写画像の高さ
                }
                else
                {
                    //サムネイルはある
                    w = DrawBitmap.Width;   //描写画像の幅
                    h = DrawBitmap.Height;  //描写画像の高さ

                    //リサイズすべきかどうか確認する。
                    if (m_thumbnailSet[Item].originalWidth <= DEFAULT_THUMBNAIL_SIZE
                        && m_thumbnailSet[Item].originalHeight <= DEFAULT_THUMBNAIL_SIZE)
                        isResize = false;
                    //if (w <= THUMBNAIL_SIZE && h <= THUMBNAIL_SIZE)
                    //    isResize = false;
                }

                //原寸表示させるモノは出来るだけ原寸とする
                if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
                //if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE
                //    && (w >= DEFAULT_THUMBNAIL_SIZE - 1 || h >= DEFAULT_THUMBNAIL_SIZE - 1))
                {
                    //拡大縮小を行う
                    float ratio = 1;
                    if (w > h)
                        ratio = (float)THUMBNAIL_SIZE / (float)w;
                    else
                        ratio = (float)THUMBNAIL_SIZE / (float)h;
                    //if (ratio > 1)			//これをコメント化すると
                    //    ratio = 1.0F;		//拡大描写も行う
                    w = (int)(w * ratio);
                    h = (int)(h * ratio);

                    //オリジナルサイズより大きい場合はオリジナルサイズにする
                    if (w > m_thumbnailSet[Item].originalWidth || h > m_thumbnailSet[Item].originalHeight)
                    {
                        w = m_thumbnailSet[Item].originalWidth;
                        h = m_thumbnailSet[Item].originalHeight;
                    }
                }

                sx = sx + (BOX_WIDTH - w) / 2;  //画像描写X位置
                sy = sy + THUMBNAIL_SIZE + PADDING - h; //画像描写X位置：下揃え
                sy = sy - m_vScrollBar.Value;   //画像描写X位置：下揃え

                //写真風に外枠を書く
                if (drawFrame)
                {
                    Rectangle r = new Rectangle(sx, sy, w, h);
                    r.Inflate(2, 2);
                    g.FillRectangle(Brushes.White, r);
                    g.DrawRectangle(Pens.LightGray, r);
                }

                //画像を書く
                g.DrawImage(DrawBitmap, sx, sy, w, h);

                //説明文字を書く
                sx = ItemX * BOX_WIDTH + PADDING;
                sy = ItemY * BOX_HEIGHT + PADDING + THUMBNAIL_SIZE + PADDING;
                sy = sy - m_vScrollBar.Value;   //画像描写X位置：下揃え
                string drawString = Path.GetFileName(m_thumbnailSet[Item].filename);
                RectangleF rect = new RectangleF(sx, sy, THUMBNAIL_SIZE, TEXT_HEIGHT);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;          //中央揃え
                sf.Trimming = StringTrimming.EllipsisPath;      //中間の省略
                                                                //g.DrawString(drawString, font, Brushes.Black, rect, sf);
                g.DrawString(drawString, m_font, new SolidBrush(m_fontColor), rect, sf);
            }
        }

        //高品質専用描写DrawItem
        //GetBipmap()から描写している
        //m_offScreenに直接描写しているが、dummyBmpに描写することで
        //m_offScreenのlockを短くするため引退
        private void DrawItemHQ(Graphics g, int Item)
        {
            //準備が出来ているか
            if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
            {
                Debug.WriteLine("準備できてないよ", " DrawItemHQ()");
                return;
            }

            //元々120x120より小さいのであれば無視
            if (m_thumbnailSet[Item].originalWidth <= DEFAULT_THUMBNAIL_SIZE
                && m_thumbnailSet[Item].originalHeight <= DEFAULT_THUMBNAIL_SIZE)
                return;

            //120dotサムネイルからでなく、元画像からサムネイルを再生成する。
            //描写品質を最高に
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int ItemX = Item % m_nItemsX;   //アイテムのX描写位置。ドットではなくアイテム番号位置
            int sx = ItemX * BOX_WIDTH;     //画像描写X位置
            int ItemY = Item / m_nItemsX;   //アイテムのY描写位置。ドットではなくアイテム番号位置
            int sy = ItemY * BOX_HEIGHT;    //画像描写X位置

            //描写範囲であることを確認
            if ((sy + BOX_HEIGHT) > m_vScrollBar.Value && sy < (m_vScrollBar.Value + this.Height))
            {
                //対象矩形を背景色で塗りつぶす.
                //元のBOXを消す
                //そうしないと前に描いたアイコンが残ってしまう可能性有り
                g.FillRectangle(
                    new SolidBrush(BackColor),
                    //Brushes.LightYellow,
                    sx,
                    sy - m_vScrollBar.Value,
                    BOX_WIDTH,
                    BOX_HEIGHT);

                ////サイズが120以上の時は元ファイルから取ってくる
                //Image DrawBitmap = m_thumbnailSet[Item].ThumbImage;
                //Image DrawBitmap = ((Form1)Parent).GetBitmapWithoutCache(Item);
                //TODO:Zipストリームを正しいストリームに直す必要有り
                Image DrawBitmap = new Bitmap(((Form1)Parent).GetBitmapWithoutCache(Item));
                bool drawFrame = true;
                bool isResize = true;   //リサイズが必要か（可能か）どうかのフラグ
                bool isDisposeBitmap = true;
                int w;  //描写画像の幅
                int h;  //描写画像の高さ

                if (DrawBitmap == null)
                {
                    //まだサムネイルは準備できていないので画像マークを呼んでおく
                    DrawBitmap = Properties.Resources.rc_tif32;
                    drawFrame = false;
                    isResize = false;
                    isDisposeBitmap = false;
                    w = DrawBitmap.Width;   //描写画像の幅
                    h = DrawBitmap.Height;  //描写画像の高さ
                }
                else
                {
                    //サムネイルはある
                    w = DrawBitmap.Width;   //描写画像の幅
                    h = DrawBitmap.Height;  //描写画像の高さ

                    //リサイズすべきかどうか確認する。
                    //if (m_thumbnailSet[Item].originalWidth <= THUMBNAIL_SIZE
                    //    && m_thumbnailSet[Item].originalHeight <= THUMBNAIL_SIZE)
                    if (w <= THUMBNAIL_SIZE && h <= THUMBNAIL_SIZE)
                        isResize = false;
                }

                //原寸表示させるモノは出来るだけ原寸とする
                if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
                {
                    float ratio = 1;
                    if (w > h)
                        ratio = (float)THUMBNAIL_SIZE / (float)w;
                    else
                        ratio = (float)THUMBNAIL_SIZE / (float)h;
                    //if (ratio > 1)			//これをコメント化すると
                    //    ratio = 1.0F;		//拡大描写も行う
                    w = (int)(w * ratio);
                    h = (int)(h * ratio);
                }

                sx = sx + (BOX_WIDTH - w) / 2;          //画像描写X位置
                sy = sy + THUMBNAIL_SIZE + PADDING - h; //画像描写Y位置：下揃え
                sy = sy - m_vScrollBar.Value;           //画像描写Y位置：スクロールバー補正

                //写真風に外枠を書く
                if (drawFrame)
                {
                    Rectangle r = new Rectangle(sx, sy, w, h);
                    r.Inflate(2, 2);
                    g.FillRectangle(Brushes.White, r);
                    g.DrawRectangle(Pens.LightGray, r);
                }

                //画像を書く
                g.DrawImage(DrawBitmap, sx, sy, w, h);

                //説明文字を書く
                sx = ItemX * BOX_WIDTH + PADDING;
                sy = ItemY * BOX_HEIGHT + PADDING + THUMBNAIL_SIZE + PADDING;
                sy = sy - m_vScrollBar.Value;   //画像描写X位置：下揃え
                string drawString = Path.GetFileName(m_thumbnailSet[Item].filename);
                RectangleF rect = new RectangleF(sx, sy, THUMBNAIL_SIZE, TEXT_HEIGHT);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;          //中央揃え
                sf.Trimming = StringTrimming.EllipsisPath;      //中間の省略
                                                                //g.DrawString(drawString, font, Brushes.Black, rect, sf);
                g.DrawString(drawString, m_font, new SolidBrush(m_fontColor), rect, sf);

                //Bitmapの破棄。GetBitmapWithoutCache()で取ってきたため
                //TODO:Properties.Resources.rc_tif32;も破棄していいのか？
                if (isDisposeBitmap)
                    DrawBitmap.Dispose();
            }
        }

        //ver0.988
        //画像描写関連
        //原寸サイズのg_bmpを生成、描写.2枚描写対応
        //逆方向対応は別の場所でやっているため無視
        //
        // ver0.987 メモリ節約のためclone()をできるだけ排除
        private void DrawImageToGBMP(int nIndex)
        {
            //引数の正規化
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;

            Cursor cc = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //とりあえず1枚読め！
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                //TODO:ファイル先頭末尾以降の参照。エラー処理する。
                viewPages = 1;
                string[] sz = { "ファイル読み込みエラー", g_pi.Items[nIndex].filename };
                g_bmp = BitmapUty.Text2Bitmap(sz, true);
                return;
            }

            //表示するモノはあるのでg_bmpをクリアする。
            if (g_bmp != null)
                g_bmp.Dispose();

            if (!g_Config.dualView
                || bmp1.Width > bmp1.Height)
            {
                //1画面モード確定
                viewPages = 1;      //1枚表示モード
                                    //g_bmp = (Bitmap)bmp1.Clone();		//ver0.987でコメントアウト
                g_bmp = (Bitmap)bmp1;           //ver0.987で追加 2010/06/19」
            }
            else
            {
                //2画面モードの疑い有り
                Bitmap bmp2 = GetBitmap(nIndex + 1);
                if (bmp2 == null || bmp2.Width > bmp2.Height)
                {
                    //1画面モード確定
                    viewPages = 1;      //1枚表示モード
                                        //g_bmp = (Bitmap)bmp1.Clone();		//ver0.987でコメントアウト
                    g_bmp = (Bitmap)bmp1;           //ver0.987で追加 2010/06/19」
                }
                else
                {
                    //2画面モード確定
                    viewPages = 2;      //2枚表示モード
                    g_bmp = new Bitmap(
                        bmp1.Width + bmp2.Width,
                        bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height);
                    using (Graphics g = Graphics.FromImage(g_bmp))
                    {
                        //正方向2枚表示
                        g.DrawImage(bmp2, 0, 0, bmp2.Width, bmp2.Height);
                        g.DrawImage(bmp1, bmp2.Width, 0, bmp1.Width, bmp1.Height);
                    }
                    //this.Refresh();
                }
            }

            Cursor.Current = cc;
            return;
        }

        //PaintBufferedGraphics()から呼び出される
        //g_bmpを指定gデバイスに書き込む。
        //このときのgはオフスクリーンg_bgを想定
        //画面サイズを特定、g_bmpを再生成後に描写する（高品質）
        private void PaintGBMP(Graphics g, bool isScreenFitting)
        {
            Rectangle cRect = GetClientRectangle();

            //g_bmpを縦横比１：１で表示する。100%未満は縮小表示
            //ratioは小さいほうにあわせる
            float ratioX = (float)cRect.Width / (float)g_bmp.Width;
            float ratioY = (float)cRect.Height / (float)g_bmp.Height;
            float ratio = (ratioX > ratioY) ? ratioY : ratioX;
            if (ratio >= 1 || ratio <= 0) ratio = 1.0F;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(g_Config.BackColor);

            if (isScreenFitting || ratio == 1.0F)
            {
                //画面サイズ自動調整、もしくは画像が画面サイズ内の場合

                //画像サイズを変更し縮尺表示する。
                int width = (int)(g_bmp.Width * ratio);
                int height = (int)(g_bmp.Height * ratio);

                g.DrawImage(
                    g_bmp,                                      //描写イメージ
                                                                //(cRect.Width - width) / 2,					//描写先　始点X
                    (cRect.Width - width) / 2 + cRect.Left,     //描写先　始点X, 2010/03/22 サイドバードックのため変更
                    (cRect.Height - height) / 2 + cRect.Top,    //描写先　始点Y
                    width,                                      //描写イメージの幅
                    height                                      //描写イメージの高さ
                );

                //ステータスバーに倍率表示
                setStatubarRatio(ratio);
                g_viewRatio = ratio;

                // 2010/03/14 ver0.9833
                //スクロールバーが表示されていたら非表示にする
                if (g_vScrollBar.Visible)
                    g_vScrollBar.Visible = false;
                if (g_hScrollBar.Visible)
                    g_hScrollBar.Visible = false;
            }
            else
            {
                //縮尺100%で表示する.必要に応じてスクロールバーを表示
                g_viewRatio = 1.0F; //2010/03/14 ver0.9833

                //スクロールバーを生成する
                if (g_vScrollBar.Visible == false && g_bmp.Height > cRect.Height)
                {
                    g_vScrollBar.Minimum = 0;
                    g_vScrollBar.Maximum = g_bmp.Height;
                    g_vScrollBar.LargeChange = cRect.Height;
                    g_vScrollBar.SmallChange = cRect.Height / 10;
                    g_vScrollBar.Top = cRect.Top;
                    g_vScrollBar.Left = cRect.Right - g_vScrollBar.Width;
                    g_vScrollBar.Height = cRect.Height;
                    g_vScrollBar.Value = 0;
                    g_vScrollBar.Visible = true;
                }
                if (g_hScrollBar.Visible == false && g_bmp.Width > cRect.Width)
                {
                    g_hScrollBar.Minimum = 0;
                    g_hScrollBar.Maximum = g_bmp.Width;
                    g_hScrollBar.LargeChange = cRect.Width;
                    g_hScrollBar.SmallChange = cRect.Width / 10;
                    g_hScrollBar.Top = cRect.Bottom - g_hScrollBar.Height;
                    g_hScrollBar.Left = cRect.Left;
                    g_hScrollBar.Width = cRect.Width;
                    g_hScrollBar.Value = 0;
                    g_hScrollBar.Visible = true;
                }
                //スクロールバー有り状態のcRectを再生成する。
                cRect = GetClientRectangle();

                //2本同時表示状態の右下部分を補正する
                if (g_vScrollBar.Visible && g_hScrollBar.Visible)
                {
                    //スクロールバーの高さ補正をする
                    g_vScrollBar.Height = cRect.Height;
                    g_vScrollBar.LargeChange = cRect.Height;    //ver0.974
                                                                //幅補正する
                    g_hScrollBar.Width = cRect.Width;
                    g_hScrollBar.LargeChange = cRect.Width;     //ver0.974
                }

                //スクロールバーがないときは中央表示
                if (!g_vScrollBar.Visible)
                    cRect.Y = (cRect.Height - g_bmp.Height) / 2 + cRect.Top;
                if (!g_hScrollBar.Visible)
                    //cRect.X = (cRect.Width - g_bmp.Width) / 2;
                    cRect.X = (cRect.Width - g_bmp.Width) / 2 + cRect.Left; //ver0.986サイドバー補正
                Debug.WriteLine(cRect.X, "cRect.X");

                //部分表示させる範囲を確定
                Rectangle sRect = new Rectangle(
                    g_hScrollBar.Value,
                    g_vScrollBar.Value,
                    cRect.Width,
                    cRect.Height);

                g.DrawImage(
                    g_bmp,          //描写イメージ
                    cRect,          //destRect
                    sRect,          //srcRect
                    GraphicsUnit.Pixel
                    );

                //ステータスバーに倍率表示
                //setStatubarRatio(1.0F);
                //g_viewRatio = ratio;	//2010/03/14 ver0.9833おかしくない？
                setStatubarRatio(g_viewRatio);
                //g_viewRatio = 1.0F;		//2010/03/14 ver0.9833おかしくない？
            }
        }

        /// <summary>
        /// オフスクリーン(BufferedGraphics) g_bgに描写する。
        /// 表示させる使い方は以下の通り。
        ///   DrawImageToGBMP(index);			//g_bmp生成。原寸
        ///	  PaintBufferedGraphics();			//g_bmpをオフスクリーンにDrawする
        ///   this.Invalidate();				//画面更新
        /// </summary>
        private void PaintBufferedGraphics()
        {
            //ver0.987 最後の描写モードを覚えておく
            m_lastDrawMode = LastDrawMode.HighQuality;

            if (g_bmp != null)
                PaintGBMP(g_bg.Graphics, g_Config.isFitScreenAndImage);
        }

        /// <summary>
        /// Form1_Resize()から呼び出されるリサイズ用の高速再描写
        /// オフスクリーン(BufferedGraphics) g_bgに描写する。
        /// </summary>
        private void PaintBufferedGraphicsFast()
        {
            //ver0.987 最後の描写モードを覚えておく
            m_lastDrawMode = LastDrawMode.Fast;

            if (g_bmp != null)
            {
                Graphics g = g_bg.Graphics;
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(g_Config.BackColor);

                Rectangle cRect = GetClientRectangle();

                //g_bmpを縦横比１：１で表示する。100%未満は縮小表示
                float ratioX = (float)cRect.Width / (float)g_bmp.Width;
                float ratioY = (float)cRect.Height / (float)g_bmp.Height;
                float ratio = (ratioX > ratioY) ? ratioY : ratioX;
                if (ratio >= 1 || ratio <= 0) ratio = 1.0F;

                if (g_Config.isFitScreenAndImage)
                {
                    //画像サイズを変更し縮尺表示する。
                    int width = (int)(g_bmp.Width * ratio);
                    int height = (int)(g_bmp.Height * ratio);

                    g.DrawImage(
                        g_bmp,                                      //描写イメージ
                                                                    //(cRect.Width - width) / 2,					//描写先　始点X
                        (cRect.Width - width) / 2 + cRect.Left,     //描写先　始点X
                        (cRect.Height - height) / 2 + cRect.Top,    //描写先　始点Y
                        width,                                      //描写イメージの幅
                        height                                      //描写イメージの高さ
                    );
                    g_viewRatio = ratio;
                }
                else
                {
                    //縮尺100%で表示する.必要に応じてスクロールバーを表示
                    cRect = GetClientRectangle();

                    //左上原点を計算。スクロールバーの位置を変えるかチェック
                    int newX = g_bmp.Width - cRect.Width;
                    if (newX < 0)
                    {
                        cRect.X += newX / 2 * -1;   //整数に直して始点を中央に
                        newX = 0;
                    }
                    int newY = g_bmp.Height - cRect.Height;
                    if (newY < 0)
                    {
                        cRect.Y += -1 * newY / 2;
                        newY = 0;
                    }

                    if (g_hScrollBar.Value > newX)
                        g_hScrollBar.Value = newX;
                    if (g_vScrollBar.Value > newY)
                        g_vScrollBar.Value = newY;

                    //部分表示させる範囲を確定
                    Rectangle sRect = new Rectangle(
                        g_hScrollBar.Value,
                        g_vScrollBar.Value,
                        cRect.Width,
                        cRect.Height);

                    g.DrawImage(
                        g_bmp,          //描写イメージ
                        cRect,          //destRect
                        sRect,          //srcRect
                        GraphicsUnit.Pixel
                        );
                    g_viewRatio = 1.0F; ;
                }
                Debug.WriteLine("PaintBufferedGraphicsFast()");
            }
        }
    }//public class ThumbnailPanel
}
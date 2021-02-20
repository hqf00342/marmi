using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Drawing.Imaging;			//PixelFormat, ColorMatrix
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /********************************************************************************/
    //ルーペ
    /********************************************************************************/

    /// <summary>
    /// Loupe ルーペコントロールクラス
    ///
    /// ルーペ機能を提供する。
    /// コンストラクタで指定したparentコントロールをキャプチャし
    /// 指定倍率化してBitmapを自コントロールに描写する。
    ///
    /// 使い方：
    ///   コンストラクタでキャプチャすべき親ウィンドウ、自身の大きさ、ルーペ倍率を指定
    ///   その後適宜DrawLoupeFast2()を呼び出して自分の画像をUpdateする。
    ///   コントロールの表示位置は呼び出し側で、Top/Leftで指定する
    /// </summary>
    public class Loupe : UserControl
    {
        private Bitmap m_loupeBmp = null;       //ルーペ表示用のBitmap
        private Bitmap m_captureBmp = null;     //内部保持のBitmap
        private int m_magnification;            //倍率
        private System.Drawing.Imaging.BitmapData srcBmpData = null;

        /// <summary>
        /// コンストラクタ
        /// ほぼ全てのパラメータをここで指定する
        /// </summary>
        /// <param name="parent">キャプチャする親ウィンドウ</param>
        /// <param name="width">自コントロールの幅を指定</param>
        /// <param name="height">自コントロールの高さを指定</param>
        /// <param name="mag">ルーペ倍率</param>
        public Loupe(Control parent, int width, int height, int mag)
        {
            m_magnification = mag;
            this.Width = width;
            this.Height = height;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint,
                true);
            this.DoubleBuffered = true;

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);

            //親をキャプチャ
            //m_captureBmp = BitmapUty.CaptureWindow(parent);
            m_captureBmp = BitmapUty.CaptureWindow((Form1._instance).PicPanel);

            //キャプチャしたBitmapをロック
            Rectangle sRect = new Rectangle(0, 0, m_captureBmp.Width, m_captureBmp.Height);
            srcBmpData = m_captureBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            this.Parent = parent;       //キャプチャ前に設定するとこのコントロールがキャプチャされてしまう。
                                        //parent.Controls.Add(this);
            this.Visible = true;

            m_loupeBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            //this.MouseMove += new MouseEventHandler(Loupe_MouseMove);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //ver1.05 バグ対応
            //どういうタイミングかでルーペが出っぱなしになる。
            //右ボタンを押していなかったら破棄する。
            //if (e.Button != MouseButtons.Right)
            //{
            //    Debug.WriteLine("Loupe: force disposed.");
            //    this.Visible = false;
            //    //this.Close();
            //    //this.Dispose();	//自分をDisposeは危険
            //}

            //ver1.12 バグ対応
            //ここに来ると言うことはルーペにフォーカスが当たったと言うこと
            //つまり右ドラッグされながら左クリックされた
            //この状態はすぐ破棄する
            this.Visible = false;
        }

        public void Close()
        {
            //表示をやめる
            this.Visible = false;

            //ルーペ表示用bmpを解放
            if (m_loupeBmp != null)
            {
                m_loupeBmp.Dispose();
                m_loupeBmp = null;
            }

            //キャプチャしたBitmapを解放
            if (m_captureBmp != null)
            {
                if (srcBmpData != null)
                    m_captureBmp.UnlockBits(srcBmpData);
                m_captureBmp.Dispose();
                m_captureBmp = null;
            }

            //this.Parent.Controls.Remove(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.Clear(Color.White);
            //e.Graphics.DrawImage(bmp, 0, 0);
            //e.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            e.Graphics.DrawImageUnscaled(m_loupeBmp, 0, 0);

            //2011年7月22日 ルーペの全画面化に伴いフレーム描写無し
            //フレームを描写
            //e.Graphics.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);
        }

        /// <summary>
        /// Window/Controlの全域をキャプチャする
        /// キャプチャした画像はm_captureBmpに格納される。
        /// </summary>
        /// <param name="wnd">キャプチャ対象のWindow/Control</param>
        private void CaptureWindow(Control wnd)
        {
            //Rectangle rc = parent.RectangleToScreen(parent.DisplayRectangle);	//ツールバー込みでキャプチャ
            //Rectangle rc = parent.Bounds;	//これだとタイトルバーもキャプチャしてしまう。
            Rectangle rc = ((Form1)wnd).GetClientRectangle();   //クライアント座標で取得。ツールバー無し
            rc = wnd.RectangleToScreen(rc);                     //スクリーン座標に

            m_captureBmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);
            //parentBmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppRgb);

            using (Graphics g = Graphics.FromImage(m_captureBmp))
            {
                g.CopyFromScreen(
                    rc.X, rc.Y,
                    0, 0,
                    rc.Size,
                    CopyPixelOperation.SourceCopy);
            }
        }

        /// <summary>
        /// マネージド版ルーペ描写ルーチン
        /// loupeBmpに描写する。
        /// 遅すぎるため現在は利用していない。
        /// </summary>
        /// <param name="b">元画像を示すBitmap</param>
        /// <param name="x">元画像に対するルーペ中心位置X</param>
        /// <param name="y">元画像に対するルーペ中心位置Y</param>
        //public void DrawLoupe(Bitmap b, int x, int y)
        //{
        //    if (b == null)
        //        return;

        //    if (m_loupeBmp == null)
        //        return;

        //    using (Graphics g = Graphics.FromImage(m_loupeBmp))
        //    {
        //        //指定位置をキャプチャの中心部に変換
        //        int sx = x - this.Width / m_magnification / 2;	//補正、倍率分の幅を換算、/2で中央に
        //        int sy = y - this.Height / m_magnification / 2;
        //        Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width / m_magnification, m_loupeBmp.Height / m_magnification);
        //        Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

        //        //DrawImageを使って拡大描写
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
        //        g.DrawImage(
        //            b,
        //            dRect,
        //            sRect,
        //            GraphicsUnit.Pixel
        //        );
        //    }
        //}

        /// <summary>
        /// アンセーフ版ルーペ描写ルーチン
        /// srcBmpData（コンストラクタで定義）に対しルーペ機能を提供
        /// 描写速度を得るためunsafe利用
        /// </summary>
        /// <param name="x">元画像に対するルーペ中心位置X</param>
        /// <param name="y">元画像に対するルーペ中心位置Y</param>
        public void DrawLoupeFast2(int x, int y)
        {
            int sWidth = m_captureBmp.Width;                //キャプチャ済親画面の幅
            int sHeight = m_captureBmp.Height;              //キャプチャ済親画面の高さ
            int capWidth = m_loupeBmp.Width / m_magnification;      //キャプチャ範囲：幅
            int capHeight = m_loupeBmp.Height / m_magnification;        //キャプチャ範囲：高さ

            //指定位置をキャプチャの中心部に変換
            //unsafeに対応するためきちんとキャプチャ範囲を正規化する。
            int sx = x - capWidth / 2;
            int sy = y - capHeight / 2;
            sx = (sx > 0) ? sx : 0;
            sy = (sy > 0) ? sy : 0;
            if (sx > sWidth - capWidth)
                sx = sWidth - capWidth;
            if (sy > sHeight - capHeight)
                sy = sHeight - capHeight;

            Rectangle sRect = new Rectangle(0, 0, sWidth, sHeight);
            Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

            //System.Drawing.Imaging.BitmapData srcBmpData = parentBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData dstBmpData = m_loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //System.Drawing.Imaging.BitmapData srcBmpData = parentBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            //System.Drawing.Imaging.BitmapData dstBmpData = loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            //Stopwatch sw = Stopwatch.StartNew();
            unsafe
            {
                byte* pSrc = (byte*)srcBmpData.Scan0;
                byte* pDest = (byte*)dstBmpData.Scan0;
                int pos;
                byte B, G, R;

                for (int ry = 0; ry < capHeight; ry++)
                {
                    for (int rx = 0; rx < capWidth; rx++)
                    {
                        //読み込み
                        pos = (sx + rx) * 3 + srcBmpData.Stride * (sy + ry);
                        B = pSrc[pos + 0];
                        G = pSrc[pos + 1];
                        R = pSrc[pos + 2];

                        //書き込み.MAG倍する
                        for (int my = 0; my < m_magnification; my++)
                        {
                            for (int mx = 0; mx < m_magnification; mx++)
                            {
                                //pos = (rx) * 3 + dstBd.Stride * (ry);					//等倍
                                pos = (rx * m_magnification + mx) * 3 + dstBmpData.Stride * (ry * m_magnification + my);    //MAG倍
                                pDest[pos + 0] = B;
                                pDest[pos + 1] = G;
                                pDest[pos + 2] = R;
                            }
                        }
                    }
                }
            }
            //sw.Stop();
            //Debug.WriteLine(sw.ElapsedTicks);

            m_loupeBmp.UnlockBits(dstBmpData);
            //parentBmp.UnlockBits(srcBmpData);
        }

        /// <summary>
        /// アンセーフ版ルーペ描写ルーチン
        /// 左上の座標を指定するver
        /// </summary>
        /// <param name="leftX">左上座標</param>
        /// <param name="topY">左上座標</param>
        public void DrawLoupeFast3(int leftX, int topY)
        {
            int srcWidth = m_captureBmp.Width;                      //キャプチャ済親画面の幅
            int srcHeight = m_captureBmp.Height;                    //キャプチャ済親画面の高さ
            int viewWidth = m_loupeBmp.Width / m_magnification;     //キャプチャ範囲：幅
            int viewHeight = m_loupeBmp.Height / m_magnification;   //キャプチャ範囲：高さ

            //指定位置をキャプチャの中心部に変換
            //unsafeに対応するためきちんとキャプチャ範囲を正規化する。
            int sx = leftX;
            int sy = topY;
            sx = (sx > 0) ? sx : 0;
            sy = (sy > 0) ? sy : 0;
            if (sx > srcWidth - viewWidth)
                sx = srcWidth - viewWidth;
            if (sy > srcHeight - viewHeight)
                sy = srcHeight - viewHeight;

            Rectangle sRect = new Rectangle(0, 0, srcWidth, srcHeight);
            Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

            System.Drawing.Imaging.BitmapData dstBmpData = m_loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* pSrc = (byte*)srcBmpData.Scan0;
                byte* pDest = (byte*)dstBmpData.Scan0;
                int pos;
                byte B, G, R;

                for (int ry = 0; ry < viewHeight; ry++)
                {
                    for (int rx = 0; rx < viewWidth; rx++)
                    {
                        //読み込み
                        pos = (sx + rx) * 3 + srcBmpData.Stride * (sy + ry);
                        B = pSrc[pos + 0];
                        G = pSrc[pos + 1];
                        R = pSrc[pos + 2];

                        //書き込み.MAG倍する
                        for (int my = 0; my < m_magnification; my++)
                        {
                            for (int mx = 0; mx < m_magnification; mx++)
                            {
                                //pos = (rx) * 3 + dstBd.Stride * (ry);					//等倍
                                pos = (rx * m_magnification + mx) * 3 + dstBmpData.Stride * (ry * m_magnification + my);    //MAG倍
                                pDest[pos + 0] = B;
                                pDest[pos + 1] = G;
                                pDest[pos + 2] = R;
                            }
                        }
                    }
                }
            }
            m_loupeBmp.UnlockBits(dstBmpData);
        }

        /// <summary>
        /// 等倍ルーペ
        /// </summary>
        /// <param name="x">中心とする位置X</param>
        /// <param name="y">中心とする位置Y</param>
        /// <param name="orgBitmap">元画像</param>
        public void DrawOriginalSizeLoupe(int x, int y, Bitmap orgBitmap)
        {
            if (orgBitmap == null)
                return;

            using (Graphics g = Graphics.FromImage(m_loupeBmp))
            {
                //いったんクリア
                g.Clear(App.Config.BackColor);

                //指定位置をキャプチャの中心部に変換
                int sx = x - this.Width / 2;    // 1/2で中央に
                int sy = y - this.Height / 2;
                Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width, m_loupeBmp.Height);
                Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

                //DrawImageを使って拡大描写
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(
                    orgBitmap,      // 描写元の画像
                    dRect,          // 描写先のRect指定
                    sRect,          // 描写元画像のRect指定
                    GraphicsUnit.Pixel
                );
            }//using
        }

        /// <summary>
        /// 等倍ルーペ
        /// 指定位置を左上座標にしたもの
        /// </summary>
        /// <param name="left">始点：左</param>
        /// <param name="top">始点：上</param>
        /// <param name="orgBitmap">元画像</param>
        public void DrawOriginalSizeLoupe2(int left, int top, Bitmap orgBitmap)
        {
            if (orgBitmap == null)
                return;

            using (Graphics g = Graphics.FromImage(m_loupeBmp))
            {
                //いったんクリア
                g.Clear(App.Config.BackColor);

                //指定位置をキャプチャの中心部に変換
                int sx = left;
                int sy = top;
                Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width, m_loupeBmp.Height);
                Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

                //DrawImageを使って拡大描写
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(
                    orgBitmap,      // 描写元の画像
                    dRect,          // 描写先のRect指定
                    sRect,          // 描写元画像のRect指定
                    GraphicsUnit.Pixel
                );
            }//using
        }
    }
}
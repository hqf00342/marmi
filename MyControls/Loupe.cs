using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

/********************************************************************************
Loupe

ルーペ機能を提供する。
コンストラクタで指定したparentコントロールをキャプチャし
指定倍率化してBitmapを自コントロールに描写する。

使い方：
  コンストラクタでキャプチャすべき親ウィンドウ、自身の大きさ、ルーペ倍率を指定
  その後適宜DrawLoupeFast2()を呼び出して自分の画像をUpdateする。
  コントロールの表示位置は呼び出し側で、Top/Leftで指定する

********************************************************************************/

namespace Marmi
{

    public class Loupe : UserControl
    {
        private Bitmap m_loupeBmp = null;       //ルーペ表示用のBitmap
        private Bitmap m_captureBmp = null;     //内部保持のBitmap
        private readonly int m_magnification;   //倍率。コンストラクタで決定
        private readonly BitmapData srcBmpData = null;

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

            //完全なオーナードローコントロールにする
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint,
                true);
            this.DoubleBuffered = true;

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);

            //親をキャプチャ
            m_captureBmp = BitmapUty.CaptureWindow((Form1._instance).PicPanel);

            //キャプチャしたBitmapをロック
            var sRect = new Rectangle(0, 0, m_captureBmp.Width, m_captureBmp.Height);
            srcBmpData = m_captureBmp.LockBits(sRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            this.Parent = parent;
            //キャプチャ前に設定するとこのコントロールがキャプチャされてしまう。
            this.Visible = true;

            //PicPanelと同じ大きさのBitmapを用意する
            m_loupeBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

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
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawImageUnscaled(m_loupeBmp, 0, 0);
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

            var sRect = new Rectangle(0, 0, srcWidth, srcHeight);
            var dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

            BitmapData dstBmpData = m_loupeBmp.LockBits(dRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

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
        /// 指定位置を左上座標にしたもの
        /// </summary>
        /// <param name="left">始点：左</param>
        /// <param name="top">始点：上</param>
        /// <param name="orgBitmap">元画像</param>
        public void DrawOriginalSizeLoupe2(int left, int top, Bitmap orgBitmap)
        {
            if (orgBitmap == null)
                return;

            using (var g = Graphics.FromImage(m_loupeBmp))
            {
                //いったんクリア
                g.Clear(App.Config.General.BackColor);

                //指定位置をキャプチャの中心部に変換
                int sx = left;
                int sy = top;
                Rectangle sRect = new Rectangle(sx, sy, m_loupeBmp.Width, m_loupeBmp.Height);
                Rectangle dRect = new Rectangle(0, 0, m_loupeBmp.Width, m_loupeBmp.Height);

                //DrawImageを使って拡大描写
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(orgBitmap, dRect, sRect, GraphicsUnit.Pixel);
            }//using
        }
    }
}
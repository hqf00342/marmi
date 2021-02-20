using System;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /// <summary>
    /// 半透明のBitmapをパネルとして利用するクラス
    /// フェードイン・フェードアウトしながら表示する。
    ///
    /// </summary>
    internal class ClearPanel : UserControl
    {
        private Bitmap m_srcImage = null;
        private Bitmap m_bitmap = null;
        private System.Windows.Forms.Timer m_HoldTimer = null;      //保持時間

        private System.Windows.Forms.Timer m_FadeInTimer = null;    //フェードイン時の描写タイマー
        private System.Windows.Forms.Timer m_FadeOutTimer = null;   //フェードアウトの描写タイマー
        private float m_AlphaValue;     //フェード時の半透明度 0.0f～1.0f
        private float m_AlphaDiff = 0.2f;   //アルファ値の増減差分

        // 初期化 ***********************************************************************/

        public ClearPanel(Control parent)
        {
            //半透明について 2011年7月19日
            //http://youryella.wankuma.com/Library/Extensions/Control/Transparent.aspx
            //
            //親コントロールに対して透明/半透明になる
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.BackColor = Color.Transparent; // 透明
            //this.BackColor = Color.FromArgb(100, 255, 255, 255); // 半透明

            this.Visible = false;
            this.BackColor = Color.Transparent;     //背景は透明に。重要設定
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.SupportsTransparentBackColor    //2011-7-19 追加。なんで入っていないのか不明
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint,
                true);                              //描写はオーナードロー

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);

            //イベント
            //this.Paint += new PaintEventHandler(ClearPanel_Paint);

            //Timer
            m_HoldTimer = new System.Windows.Forms.Timer();
            m_HoldTimer.Tick += new EventHandler(m_timer_Tick);

            //フェードインタイマー
            m_FadeInTimer = new System.Windows.Forms.Timer();
            m_FadeInTimer.Tick += new EventHandler(m_FadeInTimer_Tick);
            m_FadeInTimer.Interval = 10;

            m_FadeOutTimer = new System.Windows.Forms.Timer();
            m_FadeOutTimer.Tick += new EventHandler(m_FadeOutTimer_Tick);
            m_FadeOutTimer.Interval = 10;

            SetParent(parent);
        }

        ~ClearPanel()
        {
            this.Visible = false;
            //this.Paint -= new PaintEventHandler(ClearPanel_Paint);
            if (Parent.Controls.Contains(this))
                Parent.Controls.Remove(this);
            if (m_bitmap != null)
                m_bitmap.Dispose();
            if (m_HoldTimer != null)
            {
                m_HoldTimer.Stop();
                m_HoldTimer.Dispose();
            }

            m_FadeInTimer.Stop();
            m_FadeInTimer.Dispose();
            m_FadeOutTimer.Stop();
            m_FadeOutTimer.Dispose();
        }

        #region タイマーイベント

        //***********************************************************************
        private void m_timer_Tick(object sender, EventArgs e)
        {
            m_HoldTimer.Stop();
            FadeOut();
        }

        private void m_FadeInTimer_Tick(object sender, EventArgs e)
        {
            m_AlphaValue += m_AlphaDiff;
            if (m_AlphaValue <= 1.0f)
            {
                using (Graphics g = Graphics.FromImage(m_bitmap))
                {
                    g.Clear(Color.Transparent);
                    BitmapUty.alphaDrawImage(g, m_srcImage, m_AlphaValue);
                }
            }
            else
            {
                m_FadeInTimer.Stop();
            }
            this.Refresh();
        }

        private void m_FadeOutTimer_Tick(object sender, EventArgs e)
        {
            m_AlphaValue -= m_AlphaDiff;
            if (m_AlphaValue > 0.0f)
            {
                using (Graphics g = Graphics.FromImage(m_bitmap))
                {
                    g.Clear(Color.Transparent);
                    BitmapUty.alphaDrawImage(g, m_srcImage, m_AlphaValue);
                }
                //this.Top--;	//ver1.27コメントアウト
            }
            else
            {
                m_FadeOutTimer.Stop();
                this.Visible = false;
            }
            this.Refresh();
        }

        #endregion タイマーイベント

        #region override

        //***********************************************************************
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_bitmap == null)
                return;

            //描写
            e.Graphics.DrawImage(
                m_bitmap,
                e.ClipRectangle,
                e.ClipRectangle,
                GraphicsUnit.Pixel);
        }

        #endregion override

        #region publicメソッド

        //***********************************************************************
        public void SetParent(Control parent)
        {
            this.Parent = parent;
            parent.Controls.Add(this);
        }

        public void SetBitmap(Bitmap bmp)
        {
            m_srcImage = bmp;
            this.Width = m_srcImage.Width;
            this.Height = m_srcImage.Height;

            if (m_bitmap != null)
                m_bitmap.Dispose();
            m_bitmap = new Bitmap(this.Width, this.Height);
        }

        public void FadeIn(Point pt)
        {
            this.Location = pt;
            this.Visible = true;
            //Graphics g = Graphics.FromImage(m_bitmap);
            //for (float a = 0.0F; a <= 1.0F; a += 0.05F)
            //{
            //    g.Clear(Color.Transparent);
            //    BitmapUty.alphaDrawImage(g, m_srcImage, a);
            //    this.Refresh();
            //    Thread.Sleep(15);
            //}

            if (m_FadeInTimer.Enabled)
                m_FadeInTimer.Stop();

            m_AlphaValue = 0.0f;
            m_FadeInTimer.Start();
        }

        public void FadeIn()
        {
            //表示位置を設定：中央に
            Point pt = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);

            FadeIn(pt);
        }

        public void FadeOut()
        {
            //フェードインを止める
            //これ、重要。やっておかないとOutとInで永久に止まらない
            if (m_FadeInTimer.Enabled)
                m_FadeInTimer.Stop();

            //フェードアウト
            if (m_FadeOutTimer.Enabled)
                m_FadeOutTimer.Stop();

            m_AlphaValue = 1.0f;
            m_FadeOutTimer.Start();
        }

        /// <summary>
        /// フェードせずに即座に表示
        /// 最初の読み込みでは即座に表示した方がいい。
        /// ver0.987 今日
        /// </summary>
        public void ShowJustNow()
        {
            //表示位置を設定：中央に
            Point pt = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);
            this.Location = pt;
            this.Visible = true;

            using (Graphics g = Graphics.FromImage(m_bitmap))
            {
                g.DrawImage(m_srcImage, 0, 0);
            }
            this.Refresh();
        }

        /// <summary>
        /// BitmapをClearPanelに表示する
        /// 指定時間が過ぎた後は自動的に閉じる
        /// 位置を自由に指定可能
        /// </summary>
        /// <param name="pt">表示位置</param>
        /// <param name="holdtime">表示時間（フェード時間は除く）。0以上のミリ秒</param>
        public void ShowAndClose(Point pt, int holdtime)
        {
            if (holdtime <= 0)
                holdtime = 1000;
            m_HoldTimer.Interval = holdtime;

            FadeIn(pt);
            m_HoldTimer.Start();
        }

        /// <summary>
        /// 指定された文字列をClearPanelに表示する
        /// 指定時間が過ぎた後は自動的に閉じる
        /// 表示位置は中央に固定
        /// </summary>
        /// <param name="text">表示する文字列</param>
        /// <param name="holdtime">表示時間（フェード時間は除く）。0以上のミリ秒</param>
        public void ShowAndClose(string text, int holdtime)
        {
            SetBitmap(BitmapUty.Text2Bitmap(text, true));

            //表示位置を設定：中央に
            Point pt = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);

            ShowAndClose(pt, holdtime);
        }

        #endregion publicメソッド
    }
}
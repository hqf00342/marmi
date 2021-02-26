using System;
using System.Diagnostics;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /// <summary>
    /// 半透明のBitmapをパネルとして利用するクラス
    /// フェードイン・フェードアウトしながら表示する。
    /// 2021年2月26日 半透明やめた。メモリーリーク半端ない
    /// </summary>
    internal class ClearPanel : UserControl
    {
        //画面に表示されるイメージ。
        private Bitmap _screenImage = null;

        //閉じるタイミングのためのタイマー
        private readonly Timer _hideTimer;

        // 初期化 ***********************************************************************/

        public ClearPanel(Control parent)
        {
            //半透明について 2011年7月19日
            //http://youryella.wankuma.com/Library/Extensions/Control/Transparent.aspx

            this.Visible = false;
            this.BackColor = Color.Transparent;     //背景は透明に。重要設定

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);

            //Timer
            _hideTimer = new Timer();
            _hideTimer.Tick += HideTimer_Tick;

            //親コントロールの登録
            this.Parent = parent;
            parent.Controls.Add(this);
        }

        ~ClearPanel()
        {
            this.Visible = false;
            if (Parent.Controls.Contains(this))
                Parent.Controls.Remove(this);
            _screenImage?.Dispose();
            if (_hideTimer != null)
            {
                _hideTimer.Stop();
                _hideTimer.Dispose();
            }
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            _hideTimer.Stop();
            this.Visible = false;
            _screenImage?.Dispose();
            _screenImage = null;
            Uty.ForceGC();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Debug.WriteLine("ClearPanel::OnPaint()");
            base.OnPaint(e);

            if (_screenImage != null)
            {
                e.Graphics.DrawImage(_screenImage, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            }
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
            if(_hideTimer.Enabled)
            {
                _hideTimer.Stop();
            }

            _screenImage?.Dispose();
            _screenImage = BitmapUty.Text2Bitmap(text, true);
            //this.CreateGraphics().DrawImage(_screenImage, Point.Empty);

            //表示位置を中央にして表示
            this.Width = _screenImage.Width;
            this.Height = _screenImage.Height;
            this.Location = new Point(
                (this.Parent.Width - this.Width) / 2,
                (this.Parent.Height - this.Height) / 2);
            this.Visible = true;

            //一定時間で非表示にする
            if (holdtime <= 0) holdtime = 1000;
            _hideTimer.Interval = holdtime;
            _hideTimer.Start();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Marmi
{
	class MaskPanel : UserControl
	{

		//画像描写関連
		//private Rectangle clientRect;	// クライアント領域
		//private SolidBrush m_brush;			// ブラシ
		//private float alpha;				// 描写時の透明度。OnPaint()で利用している
		private Bitmap _bmp;				// 真っ黒に塗られたBitmap
		
		//表示するテキスト
		private List<string> texts = new List<string>();

		//private System.Threading.Timer timer;

		//フェード配列群
		float[] ToBlackAlphas = { 0.0f, 0.4f, 0.7f, 0.9f, 1.0f };
		float[] ToClearAlphas = { 1.0f, 0.6f, 0.3f, 0.2f, 0.1f};

		public float alpha { get; set; }

		// 初期化 ***********************************************************************/


		//TODO: サイドバー固定の時にホイールのフォーカスが常にサイドバーになってしまう。



		public MaskPanel()
		{
			//透明度は不透明に
			alpha = 1.0F;

			//textsを初期化
			texts.Clear();

			//ダブルバッファを有効に
			this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);


			//背景色は透明に
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
			//this.BackColor = Color.FromArgb(128, Color.SlateBlue);

			//画像を準備
			_bmp = null;
			//_bmp = new Bitmap(clientRect.Width, clientRect.Height);
			//using (Graphics g = Graphics.FromImage(_bmp))
			//{
			//    g.Clear(this.BackColor);
			//}

			//Timer
			//timer = new System.Threading.Timer(callbackTimer);
		}

		//位置指定のコンストラクタ
		public MaskPanel(Rectangle cRect)
			: this()
		{
			//先に引数なしのコンストラクタが実行される
			SetPosition(cRect);
		}

	
		~MaskPanel()
		{
			_bmp.Dispose();
			//m_brush.Dispose();
		}



		// publicメソッド/プロパティ ****************************************************/

		public void SetPosition(Rectangle cRect)
		{
			this.Left   = cRect.X;
			this.Top    = cRect.Y;
			this.Width  = cRect.Width;
			this.Height = cRect.Height;

			//Bitmapを準備
			if (_bmp != null)
			{
				_bmp.Dispose();
			}
			_bmp = new Bitmap(cRect.Width, cRect.Height);
			using (Graphics g = Graphics.FromImage(_bmp))
			{
				g.Clear(this.BackColor);
			}
		}


		/// <summary>
		/// 透明からalpha=1にする。
		/// </summary>
		public void FadeIn()
		{
			alpha = 0.0F;
			this.Refresh();
			this.Visible = true;

			foreach(float a in ToBlackAlphas)
			{
				alpha = a;		//透明度を設定
				this.Refresh();
				//Thread.Sleep(30);
			}
			alpha = 1.0F;
		}

		/// <summary>
		/// 透明にする。
		/// </summary>
		public void FadeOut()
		{
			foreach (float a in ToClearAlphas)
			{
				alpha = a;		//透明度を設定
				this.Refresh();
				//Thread.Sleep(100);
				//Debug.WriteLine(a, "fadeout()");
			}
			this.Visible = false;
			alpha = 0.0F;
		}

		// 文字表示できるようにする
		// 表示位置は右下
		public void SetInformationText(string s)
		{
			string FontName = "MS PGothic";			//フォント名
			int FontPoint = 11;
			int MARGIN = 3;

			using (Graphics g = Graphics.FromImage(_bmp))
			using (SolidBrush brush = new SolidBrush(Color.White))
			using (Font font = new Font(FontName, FontPoint))
			{
				g.Clear(this.BackColor);

				//サイズを測る
				SizeF size = g.MeasureString(s, font);

				float x = _bmp.Width - size.Width - MARGIN;
				float y = _bmp.Height - size.Height - MARGIN;
				x = x > 0 ? x : 0;
				y = y > 0 ? y : 0;
				g.DrawString(s, font, brush, x, y);
			}//using	
		}

		//テキストを追加する
		public void addText(string s)
		{
			texts.Add(s);
			MakeBitmap();
		}

		/// <summary>
		/// テキストをBitmapに描写する
		/// </summary>
		private void MakeBitmap()
		{
			string FontName = "MS PGothic";			//フォント名
			int FontPoint = 12;
			int MARGIN = 5;

			using (Graphics g = Graphics.FromImage(_bmp))
			using (SolidBrush brush = new SolidBrush(Color.White))
			using (Font font = new Font(FontName, FontPoint))
			{
				g.Clear(this.BackColor);

				//逆順に処理する
				texts.Reverse();

				float x = _bmp.Width - MARGIN;
				float y = _bmp.Height - MARGIN;
				foreach (string s in texts)
				{
					//サイズを測る
					SizeF size = g.MeasureString(s, font);
					x = _bmp.Width - size.Width - MARGIN;
					y = y - size.Height - MARGIN;
					x = x > 0 ? x : 0;
					y = y > 0 ? y : 0;

					g.DrawString(s, font, brush, x, y);
				}

				//元の順番に戻す
				texts.Reverse();
			}//using	

		}



		// オーナードロー ***************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);

			//e.Graphics.Clear(Color.Transparent);
			if (alpha >= 1.0F)
			{
				//透明描写しない
				e.Graphics.DrawImageUnscaled(_bmp, 0, 0);
			}
			else
			{
				//半透明描写
				BitmapUty.alphaDrawImage(e.Graphics, _bmp, alpha);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//base.OnPaintBackground(e);
			// 背面のコントロールを描画しない Or 背景色が不透明なので背面のコントロールを描画する必要なし
			if (this.BackColor.A == 255) 
			{
				base.OnPaintBackground(e);
				return;
			}

			// 背面のコントロールを描画
			this.DrawParentWithBackControl(e);


			// 背景を描画
			this.DrawBackground(e);
		}




		/// <summary>
		/// コントロールの背景を描画します。
		/// </summary>
		/// <param name="pevent">描画先のコントロールに関連する情報</param>
		private void DrawBackground(System.Windows.Forms.PaintEventArgs pevent)
		{
			// 背景色
			using (SolidBrush sb = new SolidBrush(this.BackColor)) {
				pevent.Graphics.FillRectangle(sb, this.ClientRectangle);
			}

			//// 背景画像
			//if (this.BackgroundImage != null) {
			//    this.DrawBackgroundImage(pevent.Graphics, this.BackgroundImage, this.BackgroundImageLayout);
			//}
		}

		/// <summary>
		/// コントロールの背景画像を描画します。
		/// </summary>
		/// <param name="g">描画に使用するグラフィックス オブジェクト</param>
		/// <param name="img">描画する画像</param>
		/// <param name="layout">画像のレイアウト</param>
		//private void DrawBackgroundImage(Graphics g, Image img, ImageLayout layout) 
		//{
		//    Size imgSize = img.Size;

		//    switch (layout) {
		//        case ImageLayout.None:
		//            g.DrawImage(img, 0, 0, imgSize.Width, imgSize.Height);

		//            break;
		//        case ImageLayout.Tile:
		//            int xCount = Convert.ToInt32(Math.Ceiling((double)this.ClientRectangle.Width / (double)imgSize.Width));
		//            int yCount = Convert.ToInt32(Math.Ceiling((double)this.ClientRectangle.Height / (double)imgSize.Height));
		//            for (int x = 0; x <= xCount - 1; x++) {
		//                for (int y = 0; y <= yCount - 1; y++) {
		//                    g.DrawImage(img, imgSize.Width * x, imgSize.Height * y, imgSize.Width, imgSize.Height);
		//                }
		//            }

		//            break;
		//        case ImageLayout.Center: {
		//                int x = 0;
		//                if (this.ClientRectangle.Width > imgSize.Width) {
		//                    x = (int)Math.Floor((double)(this.ClientRectangle.Width - imgSize.Width) / 2.0);
		//                }
		//                int y = 0;
		//                if (this.ClientRectangle.Height > imgSize.Height) {
		//                    y = (int)Math.Floor((double)(this.ClientRectangle.Height - imgSize.Height) / 2.0);
		//                }
		//                g.DrawImage(img, x, y, imgSize.Width, imgSize.Height);

		//                break;
		//            }
		//        case ImageLayout.Stretch:
		//            g.DrawImage(img, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);

		//            break;
		//        case ImageLayout.Zoom: {
		//                double xRatio = (double)this.ClientRectangle.Width / (double)imgSize.Width;
		//                double yRatio = (double)this.ClientRectangle.Height / (double)imgSize.Height;
		//                double minRatio = Math.Min(xRatio, yRatio);
						
		//                Size zoomSize = new Size(Convert.ToInt32(Math.Ceiling(imgSize.Width * minRatio)), Convert.ToInt32(Math.Ceiling(imgSize.Height * minRatio)));

		//                int x = 0;
		//                if (this.ClientRectangle.Width > zoomSize.Width) {
		//                    x = (int)Math.Floor((double)(this.ClientRectangle.Width - zoomSize.Width) / 2.0);
		//                }
		//                int y = 0;
		//                if (this.ClientRectangle.Height > zoomSize.Height) {
		//                    y = (int)Math.Floor((double)(this.ClientRectangle.Height - zoomSize.Height) / 2.0);
		//                }
		//                g.DrawImage(img, x, y, zoomSize.Width, zoomSize.Height);

		//                break;
		//            }
		//    }
		//}

		/// <summary>
		/// 親コントロールと背面にあるコントロールを描画します。
		/// </summary>
		/// <param name="pevent">描画先のコントロールに関連する情報</param>
		private void DrawParentWithBackControl(System.Windows.Forms.PaintEventArgs pevent) {
			// 親コントロールを描画
			this.DrawParentControl(this.Parent, pevent);

			// 親コントロールとの間のコントロールを親側から描画
			for (int i = this.Parent.Controls.Count - 1; i >= 0; i--)
			{
				Control c = this.Parent.Controls[i];
				//if (c.Name != "PicPanel")
				//    continue;
				if (c == this)
				{
					break;
				}
				if (this.Bounds.IntersectsWith(c.Bounds) == false)
				{
					continue;
				}

				Debug.WriteLine(c.Name, "ParentControl");
				this.DrawBackControl(c, pevent);
			}
		}

		/// <summary>
		/// 親コントロールを描画します。
		/// </summary>
		/// <param name="c">親コントロール</param>
		/// <param name="pevent">描画先のコントロールに関連する情報</param>
		private void DrawParentControl(Control c, System.Windows.Forms.PaintEventArgs pevent)
		{
			using (Bitmap bmp = new Bitmap(c.Width, c.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			using (Graphics g = Graphics.FromImage(bmp))
			using (PaintEventArgs p = new PaintEventArgs(g, c.ClientRectangle)) 
			{
				this.InvokePaintBackground(c, p);
				this.InvokePaint(c, p);

				int offsetX = this.Left + (int)Math.Floor((double)(this.Bounds.Width - this.ClientRectangle.Width) / 2.0);
				int offsetY = this.Top + (int)Math.Floor((double)(this.Bounds.Height - this.ClientRectangle.Height) / 2.0);
				pevent.Graphics.DrawImage(
					bmp,
					this.ClientRectangle,
					new Rectangle(offsetX, offsetY, this.ClientRectangle.Width, this.ClientRectangle.Height),
					GraphicsUnit.Pixel);
			}
		}

		/// <summary>
		/// 背面のコントロールを描画します。
		/// </summary>
		/// <param name="c">背面のコントロール</param>
		/// <param name="pevent">描画先のコントロールに関連する情報</param>
		private void DrawBackControl(Control c, System.Windows.Forms.PaintEventArgs pevent)
		{
			using (Bitmap bmp = new Bitmap(c.Width, c.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			{
				c.DrawToBitmap(bmp, new Rectangle(0, 0, c.Width, c.Height));

				int offsetX = (c.Left - this.Left) - (int)Math.Floor((double)(this.Bounds.Width - this.ClientRectangle.Width) / 2.0);
				int offsetY = (c.Top - this.Top) - (int)Math.Floor((double)(this.Bounds.Height - this.ClientRectangle.Height) / 2.0);
				pevent.Graphics.DrawImage(bmp, offsetX, offsetY, c.Width, c.Height);
			}
		}
	}
}

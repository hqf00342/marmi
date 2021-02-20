using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//PictureBox, ScrollBar
using System.Drawing.Imaging;			//fadePictureBox ColorMatrix
using System.Drawing.Drawing2D;			//GraphicsPath


namespace Marmi
{

	/********************************************************************************/
	//時間が来ると自動的に消えるPictureBox
	/********************************************************************************/
	/// <summary>
	/// fadePictureBox
	/// 指定時間で消えるPictureBox。
	/// 使い方：生成後に表示位置、大きさおよび画像を指定してShow()。
	/// </summary>
	class FadePictureBox : UserControl // : PictureBox
	{
		private Bitmap m_bgImage = null;			//背景保存用。処理中にキャプチャしてくる。
		private Bitmap m_fgImage = null;			//表示する画像。
		private Bitmap m_ShowImage = null;			//混ざったBmp。これを表示する。
		private System.Windows.Forms.Timer m_timer = null;		//Holdtimeを処理するためのタイマー
		//private BitmapPanel bp = new BitmapPanel();

		#region alphaF フェードイン・アウト用の透過値。
		float[] alphaF = { 
			0.1F,
			0.11F,
			0.13F,
			0.26F,
			0.3F,
			0.35F,
			0.4F,
			0.5F,
			0.6F,
			0.7F,
			0.75F,
			0.8F,
			0.85F,
			0.9F,
			0.95F,
			//0.85F,
			//0.9F,
			//0.95F,
			//1.0F
		};
		#endregion


		public FadePictureBox(Control parent)
		{
			this.Visible = false;
			this.BackColor = Color.Transparent;	//これ、重要だね。
			this.Parent = parent;
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			parent.Controls.Add(this);

			//タイマー設定
			m_timer = new System.Windows.Forms.Timer();
			m_timer.Tick += new EventHandler(timer_Tick);

			//イベント設定
			this.Paint += new PaintEventHandler(fadePictureBox_Paint);

		}

		~FadePictureBox()
		{
			if (this.Visible)
				fadeOut();

			this.Parent.Controls.Remove(this);
			m_timer.Tick -= new EventHandler(timer_Tick);
			this.Paint -= new PaintEventHandler(fadePictureBox_Paint);
		}

		void fadePictureBox_Paint(object sender, PaintEventArgs e)
		{
			if (m_ShowImage != null)
				e.Graphics.DrawImage(m_ShowImage, this.ClientRectangle);
			else
				e.Graphics.Clear(this.BackColor);
		}


		private void timer_Tick(object sender, EventArgs e)
		{
			m_timer.Stop();
			m_timer.Dispose();

			fadeOut();

			//リソースを開放する
			m_bgImage.Dispose();
			m_bgImage = null;
			//m_fgImage.Dispose();	//これはクラス外リソースかもしれないのでDispose()しない
			m_fgImage = null;
			m_ShowImage.Dispose();
			m_ShowImage = null;
		}

		public void ShowAuto(string sz, int holdtime)
		{
			BitmapPanel bp = new BitmapPanel();
			Image img = bp.TextBoard(sz, true);
			ShowAuto(img, holdtime);
		}

		public void ShowAuto(Image img, int holdtime)
		{
			//まずフェードインさせる
			ShowIn(img);

			//timerの準備
			m_timer.Interval = holdtime;
			m_timer.Start();
		}

		public void ShowIn(string sz)
		{
			BitmapPanel bp = new BitmapPanel();
			Image img = bp.TextBoard(sz, true);
			ShowIn(img);
		}

		public void ShowIn(Image img)
		{
			//タイマー動作中なら何もしない
			if (m_timer.Enabled)
				return;

			//画像を設定
			m_fgImage = (Bitmap)img;
			this.Width = img.Width;
			this.Height = img.Height;

			//表示位置を設定
			int cx = this.Parent.Width;
			int cy = this.Parent.Height;
			this.Left = (cx - this.Width) / 2;
			this.Top = (cy - this.Height) / 2;

			//表示位置の背景を取得
			captureBackGroundImage();
			using (Graphics g = this.CreateGraphics())
			{
				g.DrawImage(m_bgImage, 0, 0);
			}
			this.Visible = true;
			fadeIn();
		}

		public void ShowOut()
		{
			fadeOut();
		}


		/// <summary>
		/// 画像をフェードインさせる。最終的にはImageが表示される
		/// </summary>
		private void fadeIn()
		{
			//フェードイン
			for (int i = 0; i < alphaF.Length; i++)
			{
				DrawBrendImage(m_fgImage, alphaF[i]);
				System.Threading.Thread.Sleep(10);
			}
		}


		/// <summary>
		/// フェードアウトする。最後は非表示となる。
		/// 表示する画像はthis.Image（をバックアップしたImageBackup）
		/// </summary>
		private void fadeOut()
		{
			//フェードアウト
			for (int i = alphaF.Length - 1; i >= 0; i--)
			{
				DrawBrendImage(m_fgImage, alphaF[i]);
				System.Threading.Thread.Sleep(10);
			}
			this.Visible = false;
		}


		/// <summary>
		/// アルファブレンドした画像を描写する
		/// </summary>
		/// <param name="img">ブレンドする画像。背景はbgImage</param>
		/// <param name="alpha">透過値。０～１の間</param>
		private void DrawBrendImage(Image img, float alpha)
		{
			//ブレンドして表示する背景を準備
			m_ShowImage = (Bitmap)m_bgImage.Clone();
			Graphics brendG = Graphics.FromImage(m_ShowImage);

			//System.Drawing.Imaging.ColorMatrixオブジェクトの作成
			//ColorMatrixの行列の値を変更して、アルファ値がalphaに変更されるようにする
			ColorMatrix cm = new ColorMatrix();
			cm.Matrix00 = 1;
			cm.Matrix11 = 1;
			cm.Matrix22 = 1;
			cm.Matrix33 = alpha;
			cm.Matrix44 = 1;

			//System.Drawing.Imaging.ImageAttributesオブジェクトの作成
			//ColorMatrixを設定、ImageAttributesを使用して背景に描画
			ImageAttributes ia = new ImageAttributes();
			ia.SetColorMatrix(cm);
			//ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			//アルファブレンドしながら描写
			brendG.DrawImage(
				img,
				new Rectangle(0, 0, img.Width, img.Height),
				0, 0,
				img.Width, img.Height,
				GraphicsUnit.Pixel, ia);

			//合成された画像を表示
			using (Graphics g = this.CreateGraphics())
			{
				g.DrawImage(m_ShowImage, 0, 0);
			}
			//リソースを開放する
			brendG.Dispose();
			//brendBmp.Dispose();
			//this.Image = brendBmp;	//ver0.83でコメントアウト。なにしているんだっけ？
		}



		/// <summary>
		/// 背景となる画像を取得する。
		/// 取得した画像はbgImageに保存。
		/// </summary>
		private void captureBackGroundImage()
		{
			//表示位置の背景を取得
			Rectangle rc;
			//rc = this.RectangleToScreen(this.Bounds);
			//this(PictureBox)に対してRectangleToScreen（）するのでパラメータはthis.Boundsでは駄目
			rc = this.RectangleToScreen(new Rectangle(0, 0, this.Width, this.Height));

			m_bgImage = new Bitmap(
			  rc.Width,
			  rc.Height,
			  PixelFormat.Format32bppArgb
			  );
			using (Graphics gBgImage = Graphics.FromImage(m_bgImage))
			{
				gBgImage.CopyFromScreen(
					rc.X, rc.Y,
					0, 0,
					rc.Size,
					CopyPixelOperation.SourceCopy);
			}

		}
	}
}

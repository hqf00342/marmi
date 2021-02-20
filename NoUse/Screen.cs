using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;


/*2011年8月15日
 * BUfferedGraphicsをラッピング
 * ２つのBGを使って瞬間的に切り替えることを想定
 * 本体の方が２つのBGをうまく使うよう調整が出来ていないので
 * それを先にやることにする
 * ver1.08で作成、ver1.09で一度プロジェクトから外す
 */

namespace Marmi
{
	class Screen : IDisposable
	{
		private BufferedGraphics bg1 = null;					//ダブルバッファ本体
		private BufferedGraphics bg2 = null;					//ダブルバッファ本体
		private BufferedGraphicsContext Context1 = null;		//ダブルバッファ用コンテキスト
		//コンテキストはアプリに1つだけ
		//private BufferedGraphicsContext Context2 = null;
		private int front = 1;

		public Screen()
		{
			front = 1;
			Context1 = BufferedGraphicsManager.Current;
			backScrIndex = -1;
			backScrOrgsizeBitmap = null;
		}

		/// <summary>
		/// backScreenが保持している
		/// </summary>
		public int backScrIndex { get; set; }
		public Bitmap backScrOrgsizeBitmap { get; set; }
		public bool backScrIsDual { get; set; }
		public float backScrRatio { get; set; }

		public Graphics frontScrGraphics
		{
			get
			{
				if (bg1 == null || bg2 == null)
					return null;

				if (front == 1)
					return bg1.Graphics;
				else
					return bg2.Graphics;
			}
		}

		public Graphics backScrGraphics
		{
			get
			{
				if (bg1 == null || bg2 == null)
					return null;

				if (front == 1)
					return bg2.Graphics;
				else
					return bg1.Graphics;
			}
		}

		public void ChangeScreen()
		{
			if (front == 1)
				front = 2;
			else
				front = 1;
		}

		public void initScreen(Graphics g, Rectangle rect)
		{
			backScrIndex = -1;

			if (bg1 != null)
				bg1.Dispose();
			if (bg2 != null)
				bg2.Dispose();

			bg1 = Context1.Allocate(g, rect);
			bg2 = Context1.Allocate(g, rect);
			bg1.Graphics.Clear(Form1.g_Config.BackColor);
			bg2.Graphics.Clear(Form1.g_Config.BackColor);
		}

		public void CopyFrontToBack()
		{
			bg1.Render(bg2.Graphics);
		}


		public void RenderFrontScreen(Graphics g)
		{
			if (front == 1)
			{
				bg1.Render(g);
				//Debug.WriteLine("RenderFrontScreen():1");
			}
			else
			{
				bg2.Render(g);
				//Debug.WriteLine("RenderFrontScreen():2");
			}
		}


		public void Dispose()
		{
			if (bg1 != null)
				bg1.Dispose();
			bg1 = null;

			if (bg2 != null)
				bg2.Dispose();
			bg2 = null;
		}

	}
}

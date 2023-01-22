using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO;

namespace Marmi
{
	public  class NaviBar : UserControl
	{
		private Point m_mouseDragPoint;
		private ListBox m_ListBox;
		private PackageInfo m_packageInfo;	//g_piそのものを挿す

		private Color m_transparentBackColor = Color.FromArgb(64, 32, 32, 32);
		private Color m_NormalBackColor = Color.FromArgb(128, 32, 32, 32);
		//private Color m_NormalBackColor = Color.FromKnownColor(KnownColor.Control);

		private int GRIP_WIDTH = 10;
		private int GRIP_HEIGHT = 32;
		private int THUMBSIZE = 120;
		private int PADDING = 2;
		private int NUM_WIDTH = 20;

		public NaviBar()
		{
			this.BackColor = Color.SteelBlue;
			this.MinimumSize = new Size(GRIP_WIDTH, 1);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this.MouseMove += new MouseEventHandler(NaviBar_MouseMove);
			this.MouseDown += new MouseEventHandler(NaviBar_MouseDown);
			this.MouseLeave += new EventHandler(NaviBar_MouseLeave);
			this.MouseHover += new EventHandler(NaviBar_MouseHover);
			this.Cursor = Cursors.VSplit;
			this.Resize += new EventHandler(NaviBar_Resize);
			this.Paint += new PaintEventHandler(NaviBar_Paint);
			this.LostFocus += new EventHandler(NaviBar_LostFocus);

			m_mouseDragPoint = Point.Empty;

			m_ListBox = new ListBox();
			m_ListBox.BackColor = Color.Black;
			m_ListBox.BorderStyle = BorderStyle.None;
			m_ListBox.Left = 0;
			m_ListBox.Top = 0;
			m_ListBox.DrawMode = DrawMode.OwnerDrawVariable;
			m_ListBox.MeasureItem += new MeasureItemEventHandler(m_ListBox_MeasureItem);
			m_ListBox.DrawItem += new DrawItemEventHandler(m_ListBox_DrawItem);
			m_ListBox.SelectedIndexChanged += new EventHandler(m_ListBox_SelectedIndexChanged);
			m_ListBox.Cursor = Cursors.Hand;
			this.Controls.Add(m_ListBox);
			m_ListBox.Show();

			m_ListBox.MouseWheel += new MouseEventHandler(m_ListBox_MouseWheel);

		}

		void m_ListBox_MouseWheel(object sender, MouseEventArgs e)
		{
			//Debug.WriteLine("MouseWheel", "m_ListBox");
			if (this.Width == GRIP_WIDTH || this.Visible == false)
			{
				((Form1)Parent).Form1_MouseWheel(sender, e);
			}
		}

		public bool isMinimize
		{
			get { return (this.Width == GRIP_WIDTH); }
		}


		void NaviBar_MouseHover(object sender, EventArgs e)
		{
			if (this.Width == GRIP_WIDTH)
				SetDefaultWidth();
		}

		void NaviBar_LostFocus(object sender, EventArgs e)
		{
			SetMinimizeSize();
		}

		public void Init(PackageInfo pi)
		{
			m_packageInfo = pi;

			//画像情報の設定
			m_ListBox.Items.Clear();
			for (int i = 0; i < m_packageInfo.Items.Count; i++)
			{
				m_ListBox.Items.Add(m_packageInfo.Items[i]);
			}
		}

		public void SetDefaultWidth()
		{
			if (m_packageInfo != null)
			{
				m_ListBox.SetSelected(m_packageInfo.ViewPage, true);
				m_ListBox.TopIndex = m_packageInfo.ViewPage;
				m_ListBox.Select();	//フォーカスを当てる
			}

			const int vScrollbarWidth = 24;
			this.BackColor = m_NormalBackColor;
			this.Width = GRIP_WIDTH + THUMBSIZE + NUM_WIDTH + PADDING + vScrollbarWidth;
		}

		public void SetMinimizeSize()
		{
			this.Width = GRIP_WIDTH;
			//this.BackColor = m_transparentBackColor;
			Parent.Select();
		}

		public void SetSizeAndDock(Rectangle rect)
		{
			this.Top = rect.Top;
			this.Left = rect.Left;
			this.Height = rect.Height;
		}


		void NaviBar_MouseLeave(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.Default;
			//this.Width = GRIP_WIDTH;
		}

		void NaviBar_Paint(object sender, PaintEventArgs e)
		{
			if(this.Width == GRIP_WIDTH)
				this.BackColor = m_transparentBackColor;
			else
				this.BackColor = m_NormalBackColor;

			//グリップ枠を描写する
			Rectangle r = new Rectangle(this.Width - GRIP_WIDTH, 0, GRIP_WIDTH, this.Height);
			SolidBrush ctlColorBrush = new SolidBrush(BackColor);
			r.Inflate(-1, 0);
			e.Graphics.FillRectangle(ctlColorBrush, r);
			e.Graphics.DrawRectangle(Pens.Gray, r);

			int sx = this.Width - GRIP_WIDTH + 3;
			int sy = (this.Height - GRIP_HEIGHT) / 2;
			e.Graphics.DrawLine(Pens.Gray, sx, sy, sx, sy + GRIP_HEIGHT);
			sx += 3;
			e.Graphics.DrawLine(Pens.Gray, sx, sy, sx, sy + GRIP_HEIGHT);
		}

		void NaviBar_Resize(object sender, EventArgs e)
		{
			m_ListBox.Width = this.Width - GRIP_WIDTH;
			m_ListBox.Height = this.Height;
			//this.Invalidate();
		}

		void NaviBar_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_mouseDragPoint = this.PointToClient(MousePosition);
				Debug.WriteLine(m_mouseDragPoint, "DragStart");
			}
		}

		void NaviBar_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Point pt =this.PointToClient(MousePosition);

				int dx = pt.X - m_mouseDragPoint.X;
				Debug.WriteLine(dx, "dx");
					
				this.Width += dx;
				m_mouseDragPoint = pt;

			}
		}



		void m_ListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			int ix = m_ListBox.SelectedIndex;

			//選択色を更新するためInvalidate()
			m_ListBox.Invalidate();

			((Form1)Parent).SetViewImage(ix);
			SetMinimizeSize();
		}

		void m_ListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			//インデックスが範囲内かチェック
			if (e.Index < 0 || e.Index >= m_packageInfo.Items.Count)
				return;

			Graphics g = e.Graphics;
			//背景色の描写。選択時の色も対応してくれる
			//e.DrawBackground();

			//背景色を自前で描写
			//表示中のモノは薄い水色で表示
			//選択中のアイテムは青で
			//それ以外は白で
			SolidBrush brush = new SolidBrush(Color.FromArgb(48, 32, 32));
			if (m_ListBox.SelectedIndex == e.Index)
				g.FillRectangle(Brushes.IndianRed, e.Bounds);
			else
				g.FillRectangle(Brushes.Black, e.Bounds);



			Font fontL = new Font("ＭＳ Ｐ ゴシック", 10.5F);
			Font fontS = new Font("ＭＳ Ｐ ゴシック", 9F);

			//文字の描写:番号
			int x = e.Bounds.X + 2;
			int y = e.Bounds.Y + 20;
			string sz = string.Format("{0}", e.Index + 1);
			SizeF size = g.MeasureString(sz, fontL);
			int HeightL = (int)size.Height;
			size = g.MeasureString(sz, fontS);
			int HeightS = (int)size.Height;
			g.DrawString(sz, fontS, Brushes.DarkGray, x, y);

			//今回描写対象のアイテム
			ImageInfo ImgInfo = m_packageInfo.Items[e.Index];
			//ImageInfo ImgInfo = (ImageInfo)m_ListBox.Items[e.Index];

			//画像の描写
			//g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			x = e.Bounds.X + PADDING + NUM_WIDTH;
			y = e.Bounds.Y + PADDING;
			if (ImgInfo.ThumbImage != null)
			{
				int ThumbWidth = ImgInfo.ThumbImage.Width;
				int ThumbHeight = ImgInfo.ThumbImage.Height;

				g.DrawImage(
					ImgInfo.ThumbImage,
					x + (THUMBSIZE - ThumbWidth)/2,     // X位置（左上の）
					y + (THUMBSIZE - ThumbHeight)/2,      // Y位置（左上の）
					ThumbWidth,
					ThumbHeight
					);

				//g.DrawRectangle(
				//    Pens.LightGray,
				//    x + (THUMBSIZE - ThumbWidth)/2,     // X位置（左上の）
				//    y + (THUMBSIZE - ThumbHeight)/2,      // Y位置（左上の）
				//    ThumbWidth,
				//    ThumbHeight);
			}

			//文字の描写:ファイル名
			x += PADDING + NUM_WIDTH + THUMBSIZE;
			sz = string.Format("{0}", Path.GetFileName(ImgInfo.filename));
			g.DrawString(sz, fontL, Brushes.White, x, y);
			y += HeightL + PADDING;


			//文字の描写:サイズ, 日付
			x += 10;
			sz = string.Format(
				"{0:N0}bytes,   {1}",
				ImgInfo.length,
				ImgInfo.CreateDate
				);
			g.DrawString(sz, fontS, Brushes.DarkGray, x, y);
			size = g.MeasureString(sz, fontS, e.Bounds.Width - x);
			//x += (int)size.Width + PADDING;
			y += HeightS + PADDING;

			//文字の描写:ピクセル数
			sz = string.Format(
				"{0:N0}x{1:N0}pixels",
				ImgInfo.originalWidth,
				ImgInfo.originalHeight
				);
			g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
			y += HeightS + PADDING;

			//枠線の描写
			g.DrawLine(Pens.DimGray, 7, e.Bounds.Bottom - 1, e.Bounds.Width - 7, e.Bounds.Bottom - 1);
			//g.DrawRectangle(Pens.LightGray, e.Bounds);

			//フォーカスがあるときに枠を描写
			//e.DrawFocusRectangle();
		}

		void m_ListBox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = THUMBSIZE + PADDING * 2;
		}


	}
}

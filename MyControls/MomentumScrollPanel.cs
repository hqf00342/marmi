using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;


namespace Marmi
{
	public class MomentumScrollPanel : Control
	{
		#region private変数
		// 縦スクロールバー
		private VScrollBar m_vBar = new VScrollBar();
		
		// 横スクロールバー
		private HScrollBar m_hBar = new HScrollBar();

		// 慣性スクロール機能用のタイマー
		private Timer m_scrollTimer = new Timer();


		//MomentumScrollPanelコントロールとして持っているスクロール値
		//スクロールバーの実際の値と異なる
		//スクロールバーの値＝慣性スクロール後の目的値
		//現在指し示している値であり、慣性スクロール中は変わっていく。
		private int m_vScrollValue;
		private int m_hScrollValue;

		//タイマーの間隔[msec]
		private const int SCROLLTIMER_TICK = 30;

		//慣性力. 0.0～1.0までの間の数値。大きいほど速く収束する。
		private const double MOMENTUM_FORCE = 0.3;
		private const int MINIMUM_SCROLL = 5;

		//スクロールバーの矢印ボタンやホイールでの最低スクロール量
		private const int SCROLL_SMALLCHANGE = 20;
		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MomentumScrollPanel()
		{
			//デフォルト設定
			UseAnimation = true;

			//コントロール自身の初期化
			this.DoubleBuffered = true;
			this.ResizeRedraw = true;
			//this.MouseWheel += new MouseEventHandler(OnMouseWheel);

			//縦スクロールバーの初期化
			m_vBar.Dock = DockStyle.Right;
			m_vBar.Minimum = 0;
			m_vBar.Value = 0;
			m_vBar.Visible = false;
			m_vBar.ValueChanged += new EventHandler(m_vBar_ValueChanged);
			Controls.Add(m_vBar);
			m_vScrollValue = 0;

			//横スクロールバーの初期化
			m_hBar.Dock = DockStyle.Bottom;
			m_hBar.Minimum = 0;
			m_hBar.Value = 0;
			m_hBar.Visible = false;
			m_hBar.ValueChanged += new EventHandler(m_hBar_ValueChanged);
			m_vScrollValue = 0;

			//スクロール用タイマーの初期化
			m_scrollTimer.Interval = SCROLLTIMER_TICK;
			m_scrollTimer.Tick += new EventHandler(m_scrollTimer_Tick);
			Controls.Add(m_hBar);

			//スクロールバーの表示要否確認
			CheckScrollbarDraw();
		}

		
		///////////////////////////////////////////////////////////////
		// プロパティ
		///////////////////////////////////////////////////////////////
		#region Properties
		/// <summary>
		/// クライアント描写領域を返すプロパティ
		/// スクロールバーがある場合はその分を控除した値を返す
		/// </summary>
		public new Rectangle ClientRectangle
		{
			get
			{
				Rectangle r = base.ClientRectangle;

				if (m_vBar.Visible)
					r.Width -= m_vBar.Width;

				if (m_hBar.Visible)
					r.Height -= m_hBar.Height;

				return r;
			}
		}


		/// <summary>
		/// クライアント描写領域を返す
		/// スクロールバーがあっても控除せずスクロールバー領域を含んだ
		/// 値を返す。あまり使われると考えてはいない。
		/// </summary>
		public Rectangle ClientRectangleWithScrollbar
		{
			get { return base.ClientRectangle; }
		}

		/// <summary>
		/// 慣性スクロールをするかどうかのフラグ
		/// </summary>
		public bool UseAnimation { get; set; }

		/// <summary>
		/// スクロールバーを表示するかどうか
		/// </summary>
		public bool AutoScroll { get; set; }

		/// <summary>
		/// スクロールバーの可動範囲をset/getする
		/// スクロール領域の実際のピクセル数をセットする
		/// </summary>
		public Size AutoScrollMinSize
		{
			get
			{
				return new Size(m_hBar.Maximum, m_vBar.Maximum);
			}
			set
			{
				m_vBar.Minimum = 0;
				m_hBar.Minimum = 0;
				m_vBar.Maximum = value.Height;
				m_hBar.Maximum = value.Width;
				m_vBar.LargeChange = this.Height;
				m_hBar.LargeChange = this.Width;
				CheckScrollbarDraw();
			}
		}

		/// <summary>
		/// スクロールバーの値
		/// 実際に指し示している値のため慣性スクロール中は徐々に変化している。
		/// setした値はターゲット値。
		/// 慣性スクロールに従って徐々に値が近づいていく。
		/// </summary>
		public Point AutoScrollPosition
		{
			get
			{
				//return new Point(-m_hBar.Value, -m_vBar.Value);
				return new Point(-m_hScrollValue, -m_vScrollValue);
			}
			set 
			{
				//validation X
				if (m_hBar.Visible)
				{
					if (value.X < 0)
						m_hBar.Value = 0;
					else if (m_hBar.Maximum <= m_hBar.LargeChange)
						m_hBar.Value = 0;
					else if (value.X > m_hBar.Maximum - m_hBar.LargeChange)
						m_hBar.Value = m_hBar.Maximum - m_hBar.LargeChange;
					else
						m_hBar.Value = value.X;
				}
				//validation Y
				if (m_vBar.Visible)
				{
					if (value.Y < 0)
						m_vBar.Value = 0;
					else if (m_vBar.Maximum <= m_vBar.LargeChange)
						m_vBar.Value = 0;
					else if (value.Y > m_vBar.Maximum - m_vBar.LargeChange)
						m_vBar.Value = m_vBar.Maximum - m_vBar.LargeChange;
					else
						m_vBar.Value = value.Y;
				}
			}
		}
		#endregion


		private void CheckScrollbarDraw()
		{
			//LargeChangeを設定
			m_vBar.LargeChange = this.Height;
			m_hBar.LargeChange = this.Width;
			//if (m_vBar.LargeChange > m_vBar.Maximum)
			//    m_vBar.LargeChange = m_vBar.Maximum;
			//if (m_hBar.LargeChange > m_hBar.Maximum)
			//    m_hBar.LargeChange = m_hBar.Maximum;

			//SmallChangeを設定
			m_vBar.SmallChange = m_vBar.LargeChange / 10;
			m_hBar.SmallChange = m_hBar.LargeChange / 10;
			if (m_vBar.SmallChange < SCROLL_SMALLCHANGE)
				m_vBar.SmallChange = SCROLL_SMALLCHANGE;
			if (m_hBar.SmallChange < SCROLL_SMALLCHANGE)
				m_hBar.SmallChange = SCROLL_SMALLCHANGE;

			//表示するかどうかを確認
			m_vBar.Visible = m_vBar.Maximum > this.Height;
			m_hBar.Visible = m_hBar.Maximum > this.Width;
		}

		///////////////////////////////////////////////////////////////
		// イベント処理
		///////////////////////////////////////////////////////////////

		/// <summary>
		/// マウスホイールイベント
		/// ホイールによるスクロールも慣性スクロール対象とする
		/// </summary>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (AutoScrollMinSize.Height > this.Height)
			{
				int delta = m_vBar.SmallChange * 3;
				delta = (delta < SCROLL_SMALLCHANGE) ? SCROLL_SMALLCHANGE : delta;
				if (e.Delta > 0)
					delta *= -1;


				//スクロール位置調整
				Point p = AutoScrollPosition;
				p.Y = -p.Y + delta;
				AutoScrollPosition = p;
			}
		}


		/// <summary>
		///スクロールバー値変更イベント
		///MomentumScrollPanelコントロールとしての値も変更する
		/// </summary>
		void m_vBar_ValueChanged(object sender, EventArgs e)
		{
			//VScrollValue = m_vBar.Value;
			if (UseAnimation)
			{
				if (!m_scrollTimer.Enabled)
					m_scrollTimer.Start();
			}
			else
			{
				m_vScrollValue = m_vBar.Value;
				this.Refresh();
			}
		}

		void m_hBar_ValueChanged(object sender, EventArgs e)
		{
			//HScrollValue = m_hBar.Value;
			if (UseAnimation)
			{
				if (!m_scrollTimer.Enabled)
					m_scrollTimer.Start();
			}
			else
			{
				m_hScrollValue = m_hBar.Value;
				this.Refresh();
			}
		}

		/// <summary>
		/// 慣性スクロール用タイマーイベント
		/// 今回のメインルーチン
		/// ターゲット値(m_vBar.Value)へ慣性しながら近づけていく
		/// </summary>
		void m_scrollTimer_Tick(object sender, EventArgs e)
		{
			//縦方向
			//diff: 前回の描写位置との差分
			//	m_vBar.Value : ターゲット値
			//  m_ScrollValue: 前回の描写値、描写される値
			int diff = m_vBar.Value - m_vScrollValue;

			//if (Math.Abs(diff) <= 1)
			if (Math.Abs(diff) <= MINIMUM_SCROLL)
			{
				//1ドット以下のスクロールとなったらタイマー終了
				m_vScrollValue = m_vBar.Value;
			}
			else
			{
				//慣性スクロール実装部：簡易実装
				int add = (int)(diff * MOMENTUM_FORCE);
				//if (add == 0)
				//    add = Math.Sign(diff);	//1，-1符号だけ
				if (Math.Abs(add) < MINIMUM_SCROLL)
					add = Math.Sign(add)*MINIMUM_SCROLL;

				m_vScrollValue += add;
			}


			//横方向
			//diff: 前回の描写位置との差分
			//	m_vBar.Value : ターゲット値
			//  m_ScrollValue: 前回の描写値、描写される値
			diff = m_hBar.Value - m_hScrollValue;

			if (Math.Abs(diff) <= 1)
			{
				//1ドット以下のスクロールとなったらタイマー終了
				m_hScrollValue = m_hBar.Value;
			}
			else
			{
				//慣性スクロール実装部：簡易実装
				int add = (int)(diff * MOMENTUM_FORCE);
				if (add == 0)
					add = Math.Sign(diff);	//1，-1符号だけ
				m_hScrollValue += add;
			}

			//終了確認
			if (m_vScrollValue == m_vBar.Value &&
				m_hScrollValue == m_hBar.Value)
			{
				m_scrollTimer.Stop();
				m_scrollTimer.Enabled = false;
			}

			this.Refresh();
		}

		/// <summary>
		/// サイズ変更イベント処理
		/// サイズ変更があったときはスクロールバーのLargeChangeを変更
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			m_vBar.LargeChange = this.Height;
			m_hBar.LargeChange = this.Width;
			CheckScrollbarDraw();
		}
	}
}

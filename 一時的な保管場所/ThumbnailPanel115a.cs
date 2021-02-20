using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					//Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//UserControl
using System.Drawing.Imaging;			//PixelFormat, ColorMatrix
using System.Drawing.Drawing2D;			//GraphicsPath
using System.IO;						//Directory, File
using System.Threading;					//ThreadPool, WaitCallback


namespace Marmi
{

	enum ThumnailMode
	{
		Square,		// 正方形
		Height		// 高さだけ合わせる
	}

	/// <summary>
	/// サムネイル専用イベントの定義
	///   サムネイル中にマウスホバーが起きたときのためのイベント
	///   このイベントはThumbnailPanel::ThumbnailPanel_MouseMove()で発生している
	///   受ける側はこのEventArgsを使って受けるとアイテムが分かる。
	/// </summary>
	public class ThumbnailEventArgs : EventArgs
	{
		public int HoverItemNumber;		//Hover中のアイテム番号
		public string HoverItemName;	//Hover中のアイテム名
	}

	
	public class ThumbnailPanel : UserControl
	{
		//共通変数の定義
		private Bitmap m_offScreen;					//Bitmap. newして確保される
		private VScrollBar m_vScrollBar;			//スクロールバーコントロール
		private List<ImageInfo> m_thumbnailSet;		//ImageInfoのリスト
		private FormSaveThumbnail m_saveForm;		//サムネイル保存用ダイアログ
		private Size m_virtualScreenSize;			//仮想サムネイルのサイズ
		
		//ver 0.994 使わないことにする
		//private int m_nItemsX;						//offScreenに並ぶアイテムの数: SetScrollBar()で計算
		//private int m_nItemsY;						//offScreenに並ぶアイテムの数: SetScrollBar()で計算

		enum ThreadStatus
		{
			STOP,
			RUNNING,
			REQUEST_STOP
		}
		static ThreadStatus tStatus;				//スレッドの状況を見る
		private bool m_needHQDraw;					//ハイクオリティ描写を実施済みか
		private int m_mouseHoverItem = -1;			//現在マウスがホバーしているアイテム
		private Font m_font;						//何度も生成するのはもったいないので
		private Color m_fontColor;					//フォントの色
		private ToolTip m_tooltip;					//ツールチップ。画像情報を表示する
		private System.Windows.Forms.Timer m_timer;	//ツールチップ表示用タイマー

		//ver0.994 サムネイルモード
		private ThumnailMode m_thumbnailMode;

		private NamedBuffer<int, Bitmap> m_HQcache;		//大きなサムネイル用キャッシュ

		//プロパティの設定
		public List<ImageInfo> thumbnailImageSet
		{
			set { m_thumbnailSet = value; }
		}

		//const int PADDING = 10;
		const int PADDING = 3;	//2011年7月25日変更。ちょっと間隔開けすぎ
		//const int DEFAULT_THUMBNAIL_SIZE = 160;

		private int THUMBNAIL_SIZE;	//サムネイルの大きさ。幅と高さは同一値
		private int BOX_WIDTH;		//ボックスの幅。PADDING + THUMBNAIL_SIZE + PADDING
		private int BOX_HEIGHT;		//ボックスの高さ。PADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING
		private int FONT_HEIGHT;	//FONTの高さ。

		//専用イベントの定義
		public delegate void ThumbnailEventHandler(object obj, ThumbnailEventArgs e);
		public event ThumbnailEventHandler OnHoverItemChanged;	//マウスHoverでアイテムが替わったことを知らせる。
		public event ThumbnailEventHandler SavedItemChanged;	//


		//*** コンストラクタ ********************************************************************

		public ThumbnailPanel()
		{
			//初期化
			this.BackColor = Color.White;	//Color.FromArgb(100, 64, 64, 64);

			//m_offScreen = null;
			tStatus = ThreadStatus.STOP;
			m_thumbnailMode = ThumnailMode.Square;

			//ツールチップの初期化
			m_tooltip = new ToolTip();		//ToolTipを生成
			m_tooltip.InitialDelay = 500;	//ToolTipが表示されるまでの時間
			m_tooltip.ReshowDelay = 500;	//ToolTipが表示されている時に、別のToolTipを表示するまでの時間
			m_tooltip.AutoPopDelay = 1000;	//ToolTipを表示する時間
			m_tooltip.ShowAlways = false;	//フォームがアクティブでない時でもToolTipを表示する

			//ツールチップタイマーの初期化
			m_timer = new System.Windows.Forms.Timer();
			m_timer.Interval = 1000;
			m_timer.Tick += new EventHandler(m_timer_Tick);

			//イベント初期設定
			//this.Paint += new PaintEventHandler(OnPaint);
			//this.Resize += new EventHandler(OnResize);
			//this.MouseMove += new MouseEventHandler(ThumbnailPanel_MouseMove);
			//this.MouseLeave += new EventHandler(ThumbnailPanel_MouseLeave);
			//this.MouseWheel += new MouseEventHandler(ThumbnailPanel_MouseWheel);
			//this.MouseHover += new EventHandler(ThumbnailPanel_MouseHover);


			//スクロールバーの初期化
			m_vScrollBar = new VScrollBar();
			this.Controls.Add(m_vScrollBar);
			m_vScrollBar.Dock = DockStyle.Right;
			m_vScrollBar.Visible = false;
			m_vScrollBar.Enabled = false;
			//m_vScrollBar.Scroll += new ScrollEventHandler(vScrollBar1_Scroll);	//ver0.9832
			m_vScrollBar.ValueChanged += new EventHandler(m_vScrollBar_ValueChanged);	//ver0.9832

			//ダブルバッファ
			this.SetStyle(
				ControlStyles.AllPaintingInWmPaint
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.UserPaint,
				true);

			//フォント生成
			SetFont(new Font("ＭＳ ゴシック", 9), Color.Black);

			//サムネイルサイズからBOXの値を決定する。
			SetThumbnailSize(Form1.DEFAULT_THUMBNAIL_SIZE);

			//大きなサムネイル用キャッシュ
			m_HQcache = new NamedBuffer<int, Bitmap>();
		}


		~ThumbnailPanel()
		{
			//this.Paint -= new PaintEventHandler(OnPaint);
			//this.Resize -= new EventHandler(OnResize);
			//this.MouseMove -= new MouseEventHandler(ThumbnailPanel_MouseMove);
			//this.MouseLeave -= new EventHandler(ThumbnailPanel_MouseLeave);
			//this.MouseWheel -= new MouseEventHandler(ThumbnailPanel_MouseWheel);

			//m_vScrollBar.Scroll -= new ScrollEventHandler(vScrollBar1_Scroll);
			m_vScrollBar.ValueChanged -= new EventHandler(m_vScrollBar_ValueChanged);	//ver0.9832
			m_vScrollBar.Dispose();
			m_font.Dispose();
			m_tooltip.Dispose();

			m_timer.Tick -= new EventHandler(m_timer_Tick);
			m_timer.Dispose();

			m_HQcache.Clear();
		}


		// override
		//***************************************************************
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			//リサイズが行われたらバックスクリーンを作り直す。

			//ウィンドウサイズが０になることを想定。
			//TODO:ウィンドウサイズの最小値を決める必要有り。
			if (this.Width == 0 || this.Height == 0)
				return;

			//オフスクリーンを再設定
			if (m_offScreen == null)
				m_offScreen = new Bitmap(this.Width, this.Height);
			else
			{
				lock (m_offScreen)
				{
					m_offScreen.Dispose();
					m_offScreen = new Bitmap(this.Width, this.Height);
				}
			}

			//リサイズに伴い画面に表示アイテム数が変わるので再計算
			setScrollBar();

			MakeThumbnailScreen();
			this.Invalidate();
			return;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);
			if (m_offScreen == null)
			{
				//表示したいけど作られてないみたい・・・
				m_offScreen = new Bitmap(this.Width, this.Height);
				setScrollBar();
				MakeThumbnailScreen();
				//return;
			}
			lock (m_offScreen)
			{
				e.Graphics.DrawImageUnscaled(m_offScreen, 0, 0);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			//アイテムが1つもないときは何もしない
			if (m_thumbnailSet == null)
				return;

			//マウス位置をクライアント座標で取得
			Point pos = this.PointToClient(Cursor.Position);
			int ItemIndex = GetHoverItem(pos);	//ホバー中のアイテム番号
			if (ItemIndex == m_mouseHoverItem)
			{
				//マウスがホバーしているアイテムが変わらないときは何もしない。
				return;
			}

			//ホバーアイテムが変わっているのでツールチップ用タイマーを止める
			if (m_timer.Enabled)
				m_timer.Stop();

			//ToolTipを消す
			m_tooltip.Hide(this);

			using (Graphics g = this.CreateGraphics())
			{
				//まず今までの線を消す
				if (m_mouseHoverItem != -1)
				{

					Rectangle vanishRect = GetThumbboxRectanble(m_mouseHoverItem);
					//this.Invalidate(vanishRect);		//対象矩形自体を描写し直す
					lock (m_offScreen)
					{
						g.DrawImage(m_offScreen, vanishRect, vanishRect, GraphicsUnit.Pixel);
					}
				}

				//指定ポイントにアイテムがあるか
				//if (nx > m_nItemsX - 1 || ItemIndex > m_thumbnailSet.Count - 1 || ItemIndex < 0)
				if (ItemIndex < 0)
				{
					m_mouseHoverItem = -1;
					return;
				}


				//フォーカス枠を書く
				// 画像サイズに合わせて描写
				Rectangle r = GetThumbImageRectangle(ItemIndex);
				//r.Inflate(2, 2);	//2ピクセル拡大
				g.DrawRectangle(new Pen(Color.IndianRed, 2.5F), r);

			}

			//ホバーアイテムが替わったことを伝える
			m_mouseHoverItem = ItemIndex;

			//Hoverしているアイテムが替わったことを示すイベントを発生させる
			//このイベントはメインFormで受け取りStatusBarの表示を変える。
			ThumbnailEventArgs he = new ThumbnailEventArgs();
			he.HoverItemNumber = m_mouseHoverItem;
			he.HoverItemName = m_thumbnailSet[m_mouseHoverItem].filename;
			this.OnHoverItemChanged(this, he);

			//ToolTipを表示する
			string sz = String.Format(
				"{0}\n 日付: {1:yyyy年M月d日 H:m:s}\n 大きさ: {2:N0}bytes\n サイズ: {3:N0}x{4:N0}ピクセル",
				m_thumbnailSet[ItemIndex].filename,
				m_thumbnailSet[ItemIndex].CreateDate,
				m_thumbnailSet[ItemIndex].length,
				m_thumbnailSet[ItemIndex].originalWidth,
				m_thumbnailSet[ItemIndex].originalHeight
			);
			//m_tooltip.Show(sz, this, e.Location, 3000);
			m_tooltip.Tag = sz;
			m_timer.Start();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			//アイテムが1つもないときは何もしない
			if (m_thumbnailSet == null)
				return;

			if (m_mouseHoverItem != -1)
			{
				this.Invalidate();		//対象矩形自体を描写し直す
				m_mouseHoverItem = -1;
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			//base.OnMouseWheel(e);
			if (m_vScrollBar.Enabled)
			{
				//現在の値を取っておく
				int preValue = m_vScrollBar.Value;

				//移動する方向だけ取得。e.Deltaは下スクロールで-120、上で120を出すので反転
				int delta = 1;
				if (e.Delta > 0) delta = -1;

				//新しい値を計算。 移動値はSmallChangeの２～３倍程度がよさそう
				//問題ない値となるように検証する
				int newValue = preValue + delta * m_vScrollBar.SmallChange * 2;
				if (newValue < m_vScrollBar.Minimum)
					newValue = m_vScrollBar.Minimum;
				if (newValue > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
					newValue = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;

				//スクロールする必要があればスクロールイベントを直接呼ぶ
				if (preValue != newValue)
				{
					m_vScrollBar.Value = newValue;
					//vScrollBar1_Scroll(null, null);
					//m_vScrollBar_ValueChanged(null, null);	//ver0.9832
				}
			}
		}

		public void OnMouseWheel(object sender, MouseEventArgs e)
		{
			OnMouseWheel(e);
		}

		//*** イベント設定 **********************************************************************


		//void OnResize(object sender, EventArgs e)
		//{
		//    //リサイズが行われたらバックスクリーンを作り直す。

		//    //ウィンドウサイズが０になることを想定。
		//    //TODO:ウィンドウサイズの最小値を決める必要有り。
		//    if (this.Width == 0 || this.Height == 0)
		//        return;

		//    //オフスクリーンを再設定
		//    if (m_offScreen == null)
		//        m_offScreen = new Bitmap(this.Width, this.Height);
		//    else
		//    {
		//        lock (m_offScreen)
		//        {
		//            m_offScreen.Dispose();
		//            m_offScreen = new Bitmap(this.Width, this.Height);
		//        }
		//    }

		//    //リサイズに伴い画面に表示アイテム数が変わるので再計算
		//    setScrollBar();

		//    MakeThumbnailScreen();
		//    this.Invalidate();
		//    return;
		//}

		//void OnPaint(object sender, PaintEventArgs e)
		//{

		//    if (m_offScreen == null)
		//    {
		//        //表示したいけど作られてないみたい・・・
		//        m_offScreen = new Bitmap(this.Width, this.Height);
		//        setScrollBar();
		//        MakeThumbnailScreen();
		//        //return;
		//    }
		//    lock (m_offScreen)
		//    {
		//        e.Graphics.DrawImageUnscaled(m_offScreen, 0, 0);
		//    }
		//}

		//void ThumbnailPanel_MouseLeave(object sender, EventArgs e)
		//{
		//    //アイテムが1つもないときは何もしない
		//    if (m_thumbnailSet == null)
		//        return;

		//    if (m_mouseHoverItem != -1)
		//    {
		//        this.Invalidate();		//対象矩形自体を描写し直す
		//        m_mouseHoverItem = -1;
		//    }
		//}

		//void ThumbnailPanel_MouseMove(object sender, MouseEventArgs e)
		//{
		//    //アイテムが1つもないときは何もしない
		//    if (m_thumbnailSet == null)
		//        return;

		//    //マウス位置をクライアント座標で取得
		//    Point pos = this.PointToClient(Cursor.Position);
		//    int ItemIndex = GetHoverItem(pos);	//ホバー中のアイテム番号
		//    if (ItemIndex == m_mouseHoverItem)
		//    {
		//        //マウスがホバーしているアイテムが変わらないときは何もしない。
		//        return;
		//    }

		//    //ホバーアイテムが変わっているのでツールチップ用タイマーを止める
		//    if (m_timer.Enabled)
		//        m_timer.Stop();

		//    //ToolTipを消す
		//    m_tooltip.Hide(this);

		//    using (Graphics g = this.CreateGraphics())
		//    {
		//        //まず今までの線を消す
		//        if (m_mouseHoverItem != -1)
		//        {

		//            Rectangle vanishRect = GetThumbboxRectanble(m_mouseHoverItem);
		//            //this.Invalidate(vanishRect);		//対象矩形自体を描写し直す
		//            lock (m_offScreen)
		//            {
		//                g.DrawImage(m_offScreen, vanishRect, vanishRect, GraphicsUnit.Pixel);
		//            }
		//        }

		//        //指定ポイントにアイテムがあるか
		//        //if (nx > m_nItemsX - 1 || ItemIndex > m_thumbnailSet.Count - 1 || ItemIndex < 0)
		//        if (ItemIndex < 0)
		//        {
		//            m_mouseHoverItem = -1;
		//            return;
		//        }


		//        //フォーカス枠を書く
		//        // 画像サイズに合わせて描写
		//        Rectangle r = GetThumbImageRectangle(ItemIndex);
		//        //r.Inflate(2, 2);	//2ピクセル拡大
		//        g.DrawRectangle(new Pen(Color.IndianRed, 2.5F), r);

		//    }

		//    //ホバーアイテムが替わったことを伝える
		//    m_mouseHoverItem = ItemIndex;

		//    //Hoverしているアイテムが替わったことを示すイベントを発生させる
		//    //このイベントはメインFormで受け取りStatusBarの表示を変える。
		//    ThumbnailEventArgs he = new ThumbnailEventArgs();
		//    he.HoverItemNumber = m_mouseHoverItem;
		//    he.HoverItemName = m_thumbnailSet[m_mouseHoverItem].filename;
		//    this.OnHoverItemChanged(this, he);

		//    //ToolTipを表示する
		//    string sz = String.Format(
		//        "{0}\n 日付: {1:yyyy年M月d日 H:m:s}\n 大きさ: {2:N0}bytes\n サイズ: {3:N0}x{4:N0}ピクセル",
		//        m_thumbnailSet[ItemIndex].filename,
		//        m_thumbnailSet[ItemIndex].CreateDate,
		//        m_thumbnailSet[ItemIndex].length,
		//        m_thumbnailSet[ItemIndex].originalWidth,
		//        m_thumbnailSet[ItemIndex].originalHeight
		//    );
		//    //m_tooltip.Show(sz, this, e.Location, 3000);
		//    m_tooltip.Tag = sz;
		//    m_timer.Start();
		//}

		//public void ThumbnailPanel_MouseWheel(object sender, MouseEventArgs e)
		//{
		//    if (m_vScrollBar.Enabled)
		//    {
		//        //現在の値を取っておく
		//        int preValue = m_vScrollBar.Value;

		//        //移動する方向だけ取得。e.Deltaは下スクロールで-120、上で120を出すので反転
		//        int delta = 1;
		//        if (e.Delta > 0) delta = -1;

		//        //新しい値を計算。 移動値はSmallChangeの２～３倍程度がよさそう
		//        //問題ない値となるように検証する
		//        int newValue = preValue + delta * m_vScrollBar.SmallChange * 2;
		//        if (newValue < m_vScrollBar.Minimum)
		//            newValue = m_vScrollBar.Minimum;
		//        if (newValue > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
		//            newValue = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;

		//        //スクロールする必要があればスクロールイベントを直接呼ぶ
		//        if (preValue != newValue)
		//        {
		//            m_vScrollBar.Value = newValue;
		//            //vScrollBar1_Scroll(null, null);
		//            m_vScrollBar_ValueChanged(null, null);	//ver0.9832
		//        }
		//    }
		//}

		/// <summary>
		/// アイドル時にFormから呼び出されるルーチン
		/// 高品質描写をゆっくりやる。
		/// </summary>
		public void Application_Idle()
		{
			//サムネイル表示の準備が出来ているか
			//if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
			if (m_offScreen == null)
			{
				//準備が出来ていない場合は特に何もせず終了
				Debug.WriteLine("準備できてないよ", " Application_Idle()");
				return;
			}

			if (THUMBNAIL_SIZE > Form1.DEFAULT_THUMBNAIL_SIZE
				&& m_needHQDraw == true)
			{
				WaitCallback callback = new WaitCallback(callbackHQThumbnailThreadProc);
				ThreadPool.QueueUserWorkItem(callback);
				//ThreadProc(null);	//スレッド化せずにそのまま呼び出す。
				m_needHQDraw = false;	//高品質描写は始まったのでフラグを消す
			}
		}


		//*** 初期化 ****************************************************************************

		public void Init()
		{
			//ファイルが再読み込みされたときなどに呼び出される
			m_vScrollBar.Value = 0;
			m_vScrollBar.Visible = false;
			m_vScrollBar.Enabled = false;
			m_needHQDraw = false;

			//m_nItemsX = 0;
			//m_nItemsY = 0;

			m_HQcache.Clear();			//ver0.974
			//m_thumbnailSet.Clear();		//ver0.974 ポインタを貰っているだけなのでここでやらない
		}

		/// <summary>
		/// サムネイル画像１つのサイズを変更する
		/// option Formで変更されたあと再設定されることを想定
		/// </summary>
		/// <param name="ThumbnailSize">新しいサムネイルサイズ</param>
		public void SetThumbnailSize(int ThumbnailSize)
		{
			//ver0.982 HQcacheがすぐクリアされるので変更
			//サムネイルサイズが変わっていたら変更する
			if (THUMBNAIL_SIZE != ThumbnailSize)
			{
				THUMBNAIL_SIZE = ThumbnailSize;

				//高解像度キャッシュをクリア
				if (m_HQcache != null)
					m_HQcache.Clear();
			}

			//BOXサイズを確定
			BOX_WIDTH = THUMBNAIL_SIZE + PADDING * 2;
			//BOX_HEIGHT = THUMBNAIL_SIZE + PADDING * 3 + TEXT_HEIGHT;
			BOX_HEIGHT = THUMBNAIL_SIZE + PADDING * 2;

			//ver0.982ファイル名などの文字列表示を切り替えられるようにする
			#region ver0.982
			if (Form1.g_Config.isShowTPFileName)
				BOX_HEIGHT += PADDING + FONT_HEIGHT;

			if(Form1.g_Config.isShowTPFileSize)
				BOX_HEIGHT += PADDING + FONT_HEIGHT;

			if(Form1.g_Config.isShowTPPicSize)
				BOX_HEIGHT += PADDING + FONT_HEIGHT;
			#endregion


			//サムネイルサイズが変わると画面に表示できる
			//アイテム数が変わるので再計算
			setScrollBar();
		}

		public void SetFont(Font f, Color fc)
		{
			m_font = f;
			m_fontColor = fc;

			//TEXT_HEIGHTの決定
			using (Bitmap bmp = new Bitmap(100, 100))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					SizeF sf = g.MeasureString("テスト文字列", m_font);
					FONT_HEIGHT = (int)sf.Height;
				}
			}

			//フォントが変わるとサムネイルサイズが変わるので計算
			SetThumbnailSize(THUMBNAIL_SIZE);
		}

		public int GetHoverItem(Point pos)
		{
			//縦スクロールバーが表示されているときは換算
			if (m_vScrollBar.Enabled)
				pos.Y += m_vScrollBar.Value;

			int nx = pos.X / BOX_WIDTH;		//マウス位置のBOX座標換算：X
			int ny = pos.Y / BOX_HEIGHT;	//マウス位置のBOX座標換算：Y

			//横に並べられる数。最低１
			int numX;						//横方向のアイテム数。最小値＝１
			if (m_vScrollBar.Enabled)
				numX = (this.ClientRectangle.Width - m_vScrollBar.Width) / BOX_WIDTH;
			else
				numX = this.ClientRectangle.Width / BOX_WIDTH;
			if (numX <= 0)
				numX = 1;

			int num = ny * numX + nx;		//ホバー中のアイテム番号

			//指定ポイントにアイテムがあるか
			if (nx > numX - 1 || num > m_thumbnailSet.Count - 1)
				return -1;
			else
				return num;
		}


		//*** スクロールバー ********************************************************************

		/// <summary>
		/// スクロールバーの基本設定
		/// スクロールバーを表示するかどうかを判別し、必要に応じて表示、設定する。
		/// 必要がない場合はValueを０に設定しておく。
		/// 主にリサイズイベントが発生したときに呼び出される
		/// </summary>
		private void setScrollBar()
		{
			//スクロールバーのvalueのとる値は
			// Minimum ～ (value) ～ (Maximum-LargeChange)
			//
			//つまりMaximumには本当の最大値を設定する。（例１００）
			//LargeChangeは表示可能数を設定する。（例：１０）
			//するとValueは０～９１を示すようになる。

			//初期化済みか確認
			if (m_thumbnailSet == null)
				return;

			//アイテム数を確認
			int ItemCount = m_thumbnailSet.Count;

			//描写に必要なサイズを確認する。
			//描写領域の大きさ。まずは自分のクライアント領域を得る
			m_virtualScreenSize = calcScreenSize();

			//offScreenの方が大きい場合はスクロールバーが必要。
			if (m_virtualScreenSize.Height > this.Height)
			{
				//スクロールバーのプロパティを設定
				m_vScrollBar.Minimum = 0;						//最小値
				m_vScrollBar.Maximum = m_virtualScreenSize.Height;	//最大値
				m_vScrollBar.LargeChange = this.Height;			//空白部分を押したとき
				m_vScrollBar.SmallChange = this.Height / 10;	//矢印を押したとき
				if (m_vScrollBar.Value > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
					m_vScrollBar.Value = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;

				//有効・可視化
				m_vScrollBar.Enabled = true;
				m_vScrollBar.Visible = true;
			}
			else
			{
				//スクロールバー不要。Value=0にしておく
				m_vScrollBar.Visible = false;
				m_vScrollBar.Enabled = false;
				m_vScrollBar.Value = 0;
			}

			//Debug.WriteLine(string.Format("setScrollBar(value,min,max,)=({0},{1},{2})", m_vScrollBar.Value, m_vScrollBar.Minimum, m_vScrollBar.Maximum));
		}


		//ver0.994 オリジナルを保存
		// サムネイル画像表示の最適化をする
		//
		///// <summary>
		///// スクロールバーの基本設定
		///// スクロールバーを表示するかどうかを判別し、必要に応じて表示、設定する。
		///// 必要がない場合はValueを０に設定しておく。
		///// 主にリサイズイベントが発生したときに呼び出される
		///// </summary>
		//private void setScrollBar()
		//{
		//    //スクロールバーのvalueのとる値は
		//    // Minimum ～ (value) ～ (Maximum-LargeChange)
		//    //
		//    //つまりMaximumには本当の最大値を設定する。（例１００）
		//    //LargeChangeは表示可能数を設定する。（例：１０）
		//    //するとValueは０～９１を示すようになる。

		//    //初期化済みか確認
		//    if (m_thumbnailSet == null)
		//        return;

		//    //アイテム数を確認
		//    int ItemCount = m_thumbnailSet.Count;

		//    //描写に必要なサイズを確認する。
		//    //描写領域の大きさ。まずは自分のクライアント領域を得る
		//    int screenWidth = this.Width;
		//    int screenHeight = this.Height;
		//    if (screenWidth < 1) screenWidth = 1;
		//    if (screenHeight < 1) screenHeight = 1;

		//    //横に並べられる数。最低１
		//    m_nItemsX = screenWidth / BOX_WIDTH;	//横に並ぶアイテム数
		//    if (m_nItemsX == 0) m_nItemsX = 1;		//最低でも１にする

		//    //縦に必要な数。繰り上げる
		//    m_nItemsY = ItemCount / m_nItemsX;	//縦に並ぶアイテム数はサムネイルの数による
		//    if (ItemCount % m_nItemsX > 0)
		//        m_nItemsY++;						//割り切れなかった場合は1行追加

		//    //offScreenの方が小さい場合はスクロールバーが必要。再計算
		//    if (screenHeight < m_nItemsY * BOX_HEIGHT)
		//    {
		//        //スクロールバーが必要なので再計算
		//        m_nItemsX = (screenWidth - m_vScrollBar.Width) / BOX_WIDTH;	//再計算
		//        if (m_nItemsX == 0) m_nItemsX = 1;		//最低１
		//        m_nItemsY = (ItemCount + m_nItemsX - 1) / m_nItemsX;	//(numX-1)をあらかじめ足しておくことで繰り上げ

		//        //スクロールバーのプロパティを設定
		//        m_vScrollBar.Minimum = 0;						//最小値
		//        m_vScrollBar.Maximum = m_nItemsY * BOX_HEIGHT;	//最大値
		//        m_vScrollBar.LargeChange = screenHeight;			//空白部分を押したとき
		//        m_vScrollBar.SmallChange = screenHeight / 10;	//矢印を押したとき
		//        if (m_vScrollBar.Value > m_vScrollBar.Maximum - m_vScrollBar.LargeChange)
		//            m_vScrollBar.Value = m_vScrollBar.Maximum - m_vScrollBar.LargeChange;
		//        m_vScrollBar.Enabled = true;
		//        m_vScrollBar.Visible = true;
		//    }
		//    else
		//    {
		//        //スクロールバー不要。Value=0にしておく
		//        m_vScrollBar.Visible = false;
		//        m_vScrollBar.Enabled = false;
		//        m_vScrollBar.Value = 0;
		//    }

		//    Debug.WriteLine(string.Format("setScrollBar(value,min,max,)=({0},{1},{2})", m_vScrollBar.Value, m_vScrollBar.Minimum, m_vScrollBar.Maximum));
		//}



		/// <summary>
		/// スクリーンサイズを計算する
		/// 縦方向が大きければスクロールバーが必要ということ
		/// スクロールバーは最初からサイズとして考慮
		/// TODO スクロールバー分は必要に応じて考慮する
		/// </summary>
		private Size calcScreenSize()
		{
			//アイテム数を確認
			int ItemCount = m_thumbnailSet.Count;

			//描写に必要なサイズを確認する。
			//描写領域の大きさ。まずは自分のクライアント領域を得る
			int screenWidth = this.Width;
			int screenHeight = this.Height;
			if (screenWidth < 1) screenWidth = 1;
			if (screenHeight < 1) screenHeight = 1;

			//各アイテムの位置を決定する
			int tempx = 0;
			int tempy = 0;

			//TODO:スクリーンサイズは160以上あることが前提
			
			for (int i = 0; i < ItemCount; i++)
			{
				if ((tempx+THUMBNAIL_SIZE+PADDING) > (screenWidth - m_vScrollBar.Width))
				{
					tempx = 0;
					tempy += BOX_HEIGHT;
				}
				m_thumbnailSet[i].posX = tempx;
				m_thumbnailSet[i].posY = tempy;
				//Debug.WriteLine("ItemPos =" + tempx.ToString() +","+ tempy.ToString());
				tempx += THUMBNAIL_SIZE + PADDING;
			}

			//画像の高さ分を追加
			screenHeight = tempy + BOX_HEIGHT;
			return new Size(screenWidth, screenHeight);
		}

		void m_vScrollBar_ValueChanged(object sender, EventArgs e)
		{
			//Debug.WriteLine(vScrollBar1.Value, "Value");
			MakeThumbnailScreen();	//高速描写
			this.Refresh();
		}


		//*** 描写ルーチン **********************************************************************

		/// <summary>
		/// 全画面書き直しルーチン
		/// OnResize, onPaint,スクロール時に呼び出される
		/// ver0.97コード確認済み
		/// </summary>
		private void MakeThumbnailScreen()
		{
			//描写対象があるかチェックする。無ければ背景色にして戻る
			if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
			{
				lock (m_offScreen)
				{
					using (Graphics g = Graphics.FromImage(m_offScreen))
					{
						g.Clear(this.BackColor);
					}
				}
				return;
			}

			//スレッドを止める
			CancelHQThumbnailThread();

			//アイテム数をカウント
			int ItemCount = m_thumbnailSet.Count;

			calcScreenSize();

			//m_offScreenへの描写
			//塗りつぶした後に必要なアイテムを描写していく
			lock (m_offScreen)
			{
				using (Graphics g = Graphics.FromImage(m_offScreen))
				{
					//背景色で塗りつぶす
					g.Clear(this.BackColor);

					//描写先頭アイテムと最終アイテムを計算
					// X:描写すべき左上のアイテム番号
					// Z:描写すべき右下のアイテム番号＋１
					//int vScValue = m_vScrollBar.Value;				//スクロールバーの値。よく使うので変数に
					//int X = (vScValue / BOX_HEIGHT) * m_nItemsX;
					//int Z = (vScValue + Height) / BOX_HEIGHT * m_nItemsX + m_nItemsX;
					//if (Z > ItemCount)
					//    Z = ItemCount;
					////関係しそうなアイテムだけ描写
					//for (int Item = X; Item < Z; Item++)
					//{
					//    DrawItem3(g, Item);
					//}

					//描写すべきアイテムだけ描写する
					for (int item = 0; item < m_thumbnailSet.Count; item++)
					{
						//int tempY = m_thumbnailSet[item].posY;
						//int vScValue = m_vScrollBar.Value;				//スクロールバーの値。よく使うので変数に

						//if (tempY > vScValue - BOX_HEIGHT
						//    && tempY < vScValue + this.Height)
						//{
						//    DrawItem3(g, item);
						//}
						if(CheckNecessaryToDrawItem(item) == true)
							DrawItem3(g,item);

					}

				} //using(Graphics)
			}//lock

			//止めたスレッドはApplication_Idle()で再開される。
			//ここでは何もせず終了する。
		}

		//高速描写対応DrawItem.外部ルーチン化
		private void DrawItem3(Graphics g, int Item)
		{
			//準備が出来ているか
			//if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
			if (m_offScreen == null)
			{
				Debug.WriteLine("準備できてないよ", " DrawItem3()");
				return;
			}

			//描写品質
			if (THUMBNAIL_SIZE > Form1.DEFAULT_THUMBNAIL_SIZE)
			{
				//描き直すので最低品質で描写する
				//g.InterpolationMode = InterpolationMode.NearestNeighbor;	
				g.InterpolationMode = InterpolationMode.Bilinear;			//これぐらいの品質でもOKか？
				m_needHQDraw = true;	//描き直しフラグ
			}
			else
				//最標準サムネイルサイズ以下は高品質で描写
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;

			//描写位置の決定
			Rectangle boxRect = GetThumbboxRectanble(Item);
		
			//文字列を描く初期位置
			Rectangle tRect = new Rectangle(
				boxRect.X + PADDING,
				boxRect.Y + PADDING + THUMBNAIL_SIZE + PADDING,
				THUMBNAIL_SIZE,
				FONT_HEIGHT);
			
			//対象矩形を背景色で塗りつぶす.
			//そうしないと前に描いたアイコンが残ってしまう可能性有り
			g.FillRectangle(new SolidBrush(BackColor), boxRect);


			//キャッシュがあればそっちを使う ver0.971
			if (m_needHQDraw && m_HQcache.ContainsKey(Item))
			{
				//キャッシュされた高品質画像を描写
				g.DrawImageUnscaled(m_HQcache[Item], GetThumbboxRectanble(Item));

				//画像情報を描写
				//DrawTextInfo(g, Item, tRect); 
				DrawTextInfo(g, Item, boxRect);
				return;
			}

			Image DrawBitmap = m_thumbnailSet[Item].ThumbImage;
			bool drawFrame = true;
			if (DrawBitmap == null)
			{
				//まだサムネイルは準備できていないので画像マークを呼んでおく
				DrawBitmap = Properties.Resources.rc_tif32;
				drawFrame = false;
			}
			Rectangle imageRect = GetThumbImageRectangle(Item);


			//影を描写する.アイコン時（＝drawFrame==false）で描写しない
			if (Form1.g_Config.isDrawThumbnailShadow && drawFrame)
			{
				Rectangle frameRect = imageRect;
				BitmapUty.drawDropShadow(g, frameRect);
			}

			//外枠を書く
			if (Form1.g_Config.isDrawThumbnailFrame  && drawFrame)
			{
				Rectangle frameRect = imageRect;
				//枠がおかしいので拡大しない
				//frameRect.Inflate(2, 2);
				g.FillRectangle(Brushes.White, frameRect);
				g.DrawRectangle(Pens.LightGray, frameRect);
			}



			//画像を書く
			g.DrawImage(DrawBitmap, imageRect);

			//画像情報文字列を描く
			//DrawTextInfo(g, Item, tRect);
			DrawTextInfo(g, Item, boxRect);

		}


		//高品質専用描写DrawItem. 
		//ダミーBMPに描写するため描写位置を固定とする。
		private void DrawItemHQ2(Graphics g, int Item)
		{
			//準備が出来ているか
			//if (m_nItemsX == 0 || m_nItemsY == 0 )
			if (m_offScreen == null)
			{
				Debug.WriteLine("準備できてないよ", " DrawItemHQ2()");
				return;
			}

			//対象矩形を背景色で塗りつぶす.
			//そうしないと前に描いたアイコンが残ってしまう可能性有り
			g.FillRectangle(
				new SolidBrush(BackColor),
				//Brushes.LightYellow,
				0, 0, BOX_WIDTH, BOX_HEIGHT);

			//描写品質を最高に
			//元ファイルから取ってくる. Bitmapはnewして持ってくる
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			//ver0.993 これだとSevenZipSharpでエラーが出る
			//Image DrawBitmap = new Bitmap(((Form1)Parent).GetBitmapWithoutCache(Item));

			//ver0.993 nullReferの原因追及
			//Ver0.993 2011年7月31日いろいろお試し中
			//まずなんでocacheじゃないとダメだったのか分からない
			//エラーが出る原因はやっぱり別スレッド中からの呼び出しみたい
			Bitmap DrawBitmap = null;

			if (Parent == null)
				//親ウィンドウがなくなっているので何もしない
				return;

			if (InvokeRequired)
			{
				this.Invoke(new MethodInvoker(delegate
				{
					DrawBitmap = (Bitmap)(((Form1)Parent).GetBitmap(Item)).Clone();
				}));
			}
			else
			{
				DrawBitmap = (Bitmap)(((Form1)Parent).GetBitmap(Item)).Clone();
			}


			//フラグ設定
			bool drawFrame = true;			//枠線を描写するか
			bool isResize = true;			//リサイズが必要か（可能か）どうかのフラグ
			int w;							//描写画像の幅
			int h;							//描写画像の高さ

			if (DrawBitmap == null)
			{
				//まだサムネイルは準備できていないので画像マークを呼んでおく
				Debug.WriteLine(Item, "Image is not Ready");
				DrawBitmap = Properties.Resources.rc_tif32;
				drawFrame = false;
				isResize = false;
				w = DrawBitmap.Width;	//描写画像の幅
				h = DrawBitmap.Height;	//描写画像の高さ
			}
			else
			{
				w = DrawBitmap.Width;	//描写画像の幅
				h = DrawBitmap.Height;	//描写画像の高さ

				//リサイズすべきかどうか確認する。
				if (w <= THUMBNAIL_SIZE && h <= THUMBNAIL_SIZE)
					isResize = false;
			}


			//原寸表示させるモノは出来るだけ原寸とする
			//if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
			if (isResize)
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

			int sx = (BOX_WIDTH - w) / 2;			//画像描写X位置
			int sy = THUMBNAIL_SIZE + PADDING - h;	//画像描写Y位置：下揃え

			//写真風に外枠を書く
			if (drawFrame)
			{
				Rectangle r = new Rectangle(sx, sy, w, h);
				//r.Inflate(2, 2);
				//g.FillRectangle(Brushes.White, r);
				//g.DrawRectangle(Pens.LightGray, r);
				BitmapUty.drawDropShadow(g, r);
			}

			//画像を書く
			g.DrawImage(DrawBitmap, sx, sy, w, h);


			////画像情報を文字描写する
			//RectangleF tRect = new RectangleF(PADDING, PADDING + THUMBNAIL_SIZE + PADDING, THUMBNAIL_SIZE, TEXT_HEIGHT);
			//DrawTextInfo(g, Item, tRect);


			//Bitmapの破棄。GetBitmapWithoutCache()で取ってきたため
			if (DrawBitmap != null && (string)(DrawBitmap.Tag) != Properties.Resources.TAG_PICTURECACHE)
				DrawBitmap.Dispose();
		}


		//*** スレッド処理 ********************************************************************

		/// <summary>
		/// Application_Idle()からスレッドプール登録されて
		/// 呼び出されるcallback関数
		/// 画面描写範囲のサムネイルをHQ描写する。
		/// HQ画像がある場合は当初より描写されているはずだが
		/// 無い場合はここで作成され、描写される。
		/// </summary>
		/// <param name="dummy">使われない</param>
		private void callbackHQThumbnailThreadProc(object dummy)
		{
			////効き目不明
			if (tStatus == ThreadStatus.RUNNING)
			{
				Debug.WriteLine("実行中なのに呼ばれた", "ThreadProc()");
				return;
			}

			//親のサムネイル作成が動いていたら親側をPAUSE
			//((Form1)Parent).PauseThreadPool();
			Form1.PauseThumbnailMakerThread();	//ver1.10 2011/08/19 static化


			//初期化
			int ItemCount = m_thumbnailSet.Count;
			tStatus = ThreadStatus.RUNNING;

			//関係しそうなアイテムを計算
			int Width = this.Width;
			int Height = this.Height;
			int numItemX = Width / BOX_WIDTH;	//横方向に並ぶアイテム数
			if (numItemX < 1) numItemX = 1;		//最低１
			int vScValue = m_vScrollBar.Value;	//スクロールバーの値。よく使うのでint化

			//描写先頭アイテムと最終アイテムを計算
			// X:描写すべき左上のアイテム番号
			// Z:描写すべき右下のアイテム番号＋１
			int X = (vScValue / BOX_HEIGHT) * numItemX;
			int Z = (vScValue + Height) / BOX_HEIGHT * numItemX + numItemX;
			if (Z > ItemCount)
				Z = ItemCount;

			for (int item = 0; item < ItemCount; item++)
			{
				//スレッド中止を確認
				if (tStatus == ThreadStatus.REQUEST_STOP)
					break;

				//描写対象かどうか確認
				if (CheckNecessaryToDrawItem(item))
				{
					//描写対象アイテムなので描写する
					//高品質サムネイルを持っていればそれで描写
					if (m_HQcache.ContainsKey(item))
					{
						Bitmap b = m_HQcache[item];
						if (b != null)
						{
							Rectangle rect = GetThumbboxRectanble(item);
							lock (m_offScreen)
							{
								using (Graphics g = Graphics.FromImage(m_offScreen))
								{
									g.DrawImageUnscaled(b, rect);
									DrawTextInfo(g, item, rect);
								}
							}
							this.Invalidate();
							continue;
						}
					}
					else
					{
						//高品質キャッシュがないので生成した上で描写する。
						//ぎりぎりのサイズにする
						using (Bitmap dummyBmp = new Bitmap(BOX_WIDTH, THUMBNAIL_SIZE + PADDING * 2))
						{
							using (Graphics g = Graphics.FromImage(dummyBmp))
							{
								//高品質画像を生成、テキスト情報は描写されていない
								DrawItemHQ2(g, item);
							}
							m_HQcache.Add(item, (Bitmap)dummyBmp.Clone());


							Rectangle rect = GetThumbboxRectanble(item);
							lock (m_offScreen)
							{
								using (Graphics g = Graphics.FromImage(m_offScreen))
								{
									//生成、保存した高品質画像を描写
									g.DrawImageUnscaled(dummyBmp, rect);

									//画像情報を描写
									DrawTextInfo(g, item, rect);
								}
							}
							this.Invalidate();
						}
					}//if
				}//if
			}//for

			////関係しそうなアイテムだけ描写
			//for (int Item = X; Item < Z; Item++)
			//{
			//    if (tStatus == ThreadStatus.REQUEST_STOP)
			//        break;

			//    ////ver0.95でコメントアウト
			//    ////高速化を目指す
			//    ////
			//    //lock (m_offScreen)
			//    //{
			//    //    DrawItemHQ(Graphics.FromImage(m_offScreen), Item);	//高品質描写
			//    //    this.Invalidate();
			//    //}

			//    //ver0.95
			//    //m_offScreenのlock()を短くするためにダミーに描写
			//    //ダミーをコピーするときだけlock()する。
			//    if (m_HQcache.ContainsKey(Item))
			//    {
			//        Bitmap b = m_HQcache[Item];
			//        if (b != null)
			//        {
			//            Rectangle rect = GetThumbboxRectanble(Item);
			//            lock (m_offScreen)
			//            {
			//                using (Graphics g = Graphics.FromImage(m_offScreen))
			//                {
			//                    //保存済みの高品質画像を描写
			//                    g.DrawImageUnscaled(b, rect);

			//                    //テキストを描写
			//                    //Rectangle tRect = rect;	//TODO: もしかしたらコピーしないと駄目
			//                    //tRect.X += PADDING;
			//                    //tRect.Y += PADDING + THUMBNAIL_SIZE + PADDING;
			//                    //tRect.Width = THUMBNAIL_SIZE;
			//                    //tRect.Height = TEXT_HEIGHT;
			//                    //DrawTextInfo(g, Item, tRect);
			//                    DrawTextInfo(g, Item, rect);

			//                }

			//            }
			//            this.Invalidate();
			//            //Debug.WriteLine(Item, "HQDraw() キャッシュで描写");
			//            continue;
			//        }
			//    }

			//    //保存済のキャッシュがないので生成した上で描写する。
			//    //using (Bitmap dummyBmp = new Bitmap(BOX_WIDTH, BOX_HEIGHT)
			//    //ぎりぎりのサイズにする
			//    using (Bitmap dummyBmp = new Bitmap(BOX_WIDTH, THUMBNAIL_SIZE+PADDING*2))
			//    {
			//        using (Graphics g = Graphics.FromImage(dummyBmp))
			//        {
			//            //高品質画像を生成、テキスト情報は描写されていない
			//            DrawItemHQ2(g, Item);
			//        }
			//        m_HQcache.Add(Item, (Bitmap)dummyBmp.Clone());


			//        //ちょっと時間のかかる処理をしたので確認
			//        if (tStatus == ThreadStatus.REQUEST_STOP)
			//            break;

			//        Rectangle rect = GetThumbboxRectanble(Item);
			//        lock (m_offScreen)
			//        {
			//            using (Graphics g = Graphics.FromImage(m_offScreen))
			//            {
			//                //生成、保存した高品質画像を描写
			//                g.DrawImageUnscaled(dummyBmp, rect);

			//                //画像情報を描写
			//                DrawTextInfo(g, Item, rect);
			//            }
			//        }
			//        this.Invalidate();
			//    }
			//    //Debug.WriteLine(Item, "HQDraw");

			//}
			tStatus = ThreadStatus.STOP;

			//親のサムネイル作成をPAUSEしていたら再開
			//((Form1)Parent).ContinueThreadPool();	//NullReferが発生
			Form1.ResumeThumbnailMakerThread();				//Staticに変更したのでそのまま利用

			Debug.WriteLine("Thumbnail ThreadProc() end");

		}

		/// <summary>
		/// バックグラウンドで動いているスレッドにSTOP命令を出す
		/// STOPするまで待ってからリターン
		/// </summary>
		public void CancelHQThumbnailThread()
		{
			if (tStatus == ThreadStatus.STOP)
				return;

			tStatus = ThreadStatus.REQUEST_STOP;
			while (tStatus != ThreadStatus.STOP)
				Application.DoEvents();
		}


		//*** 描写支援ルーチン ****************************************************************

		/// <summary>
		/// 再描写関数
		/// 描写される部分をすべて再描写する。
		/// 他のクラス、フォームから呼び出される。そのためpublic
		/// 主にメニューでソートされたりしたときに呼び出される
		/// </summary>
		public void ReDraw()
		{
			setScrollBar();
			MakeThumbnailScreen();
			this.Invalidate();
		}




		/// <summary>
		/// 1アイテムの画面内での枠を返す。
		/// Thumbbox = 画像＋文字の大きな枠
		/// スクロールバーその他についても織り込み済
		/// m_offScreenや実画面に対して使われることを想定
		/// </summary>
		private Rectangle GetThumbboxRectanble(int ItemIndex)
		{
			Rectangle r = new Rectangle(
				//(ItemIndex % m_nItemsX) * BOX_WIDTH,
				//(ItemIndex / m_nItemsX) * BOX_HEIGHT - m_vScrollBar.Value,
				m_thumbnailSet[ItemIndex].posX,
				m_thumbnailSet[ItemIndex].posY - m_vScrollBar.Value,
				BOX_WIDTH,
				BOX_HEIGHT);
			return r;
		}


		/// <summary>
		/// THUMBNAILイメージの画面内での枠を返す。
		/// ThumbImage = 画像部分のみ
		/// スクロールバーその他についても織り込み済
		/// m_offScreenや実画面に対して使われることを想定
		/// </summary>
		private Rectangle GetThumbImageRectangle(int ItemIndex)
		{
			Image DrawBitmap = m_thumbnailSet[ItemIndex].ThumbImage;
			bool canExpand = true;	//拡大できるかどうかのフラグ

			int w;	//描写画像の幅
			int h;	//描写画像の高さ

			if (DrawBitmap == null)
			{
				//まだサムネイルは準備できていないので画像マークを呼んでおく
				DrawBitmap = Properties.Resources.rc_tif32;
				canExpand = false;
				w = DrawBitmap.Width;	//描写画像の幅
				h = DrawBitmap.Height;	//描写画像の高さ
			}
			else
			{
				//サムネイルはある
				w = DrawBitmap.Width;	//描写画像の幅
				h = DrawBitmap.Height;	//描写画像の高さ

				//リサイズすべきかどうか確認する。
				if (m_thumbnailSet[ItemIndex].originalWidth <= THUMBNAIL_SIZE
					&& m_thumbnailSet[ItemIndex].originalHeight <= THUMBNAIL_SIZE)
				{
					//オリジナルがサムネイルより小さいのでリサイズしない。
					w = m_thumbnailSet[ItemIndex].originalWidth;
					h = m_thumbnailSet[ItemIndex].originalHeight;
					canExpand = false;
				}
			}


			//原寸表示させるモノは出来るだけ原寸とする
			if (THUMBNAIL_SIZE != Form1.DEFAULT_THUMBNAIL_SIZE)
			{
				//拡大縮小を行う
				float ratio = 1;
				if (w > h)
					ratio = (float)THUMBNAIL_SIZE / (float)w;
				else
					ratio = (float)THUMBNAIL_SIZE / (float)h;

				if (ratio > 1 && !canExpand)
				{
					//拡大処理はしない
				}
				else
				{
					w = (int)(w * ratio);
					h = (int)(h * ratio);
				}
				////オリジナルサイズより大きい場合はオリジナルサイズにする
				//if (w > m_thumbnailSet[ItemIndex].originalWidth || h > m_thumbnailSet[ItemIndex].originalHeight)
				//{
				//    w = m_thumbnailSet[ItemIndex].originalWidth;
				//    h = m_thumbnailSet[ItemIndex].originalHeight;
				//}
			}

			Rectangle rect = GetThumbboxRectanble(ItemIndex);
			rect.X += (BOX_WIDTH - w) / 2;	//画像描写X位置
			rect.Y += THUMBNAIL_SIZE + PADDING - h; 	//画像描写X位置：下揃え
			//rect.Y -= m_vScrollBar.Value;
			rect.Width = w;
			rect.Height = h;

			return rect;
		}


		/// <summary>
		/// ファイル名、ファイルサイズ、画像サイズをテキスト描写する
		/// </summary>
		/// <param name="g">描写先のGraphics</param>
		/// <param name="Item">描写アイテム</param>
		/// <param name="thumbnailRect">描写する先のサムネイルBOX矩形。テキスト位置ではない</param>
		private void DrawTextInfo(Graphics g, int Item, Rectangle thumbnailBoxRect)
		{
			//テキスト描写位置を補正
			Rectangle textRect = thumbnailBoxRect;
			textRect.X += PADDING;								//左に余白を追加
			textRect.Y += PADDING + THUMBNAIL_SIZE + PADDING;	//上下に余白を追加
			textRect.Width = THUMBNAIL_SIZE;					//横幅はサムネイルサイズと同じ
			textRect.Height = FONT_HEIGHT;

			//テキスト描写用の初期フォーマット
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Center;			//中央揃え
			sf.Trimming = StringTrimming.EllipsisPath;		//中間の省略

			//ファイル名を書く
			if (Form1.g_Config.isShowTPFileName)
			{
				string drawString = Path.GetFileName(m_thumbnailSet[Item].filename);
				g.DrawString(drawString, m_font, new SolidBrush(m_fontColor), textRect, sf);
				textRect.Y += FONT_HEIGHT;
			}

			//ファイルサイズを書く
			if (Form1.g_Config.isShowTPFileSize)
			{
				string s = String.Format("{0:#,0} bytes", m_thumbnailSet[Item].length);
				g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
				textRect.Y += FONT_HEIGHT;
			}

			//画像サイズを書く
			if (Form1.g_Config.isShowTPPicSize)
			{
				string s = String.Format(
					"{0:#,0}x{1:#,0} px",
					m_thumbnailSet[Item].originalWidth,
					m_thumbnailSet[Item].originalHeight);
				g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
				textRect.Y += FONT_HEIGHT;
			}
		}



		/// <summary>
		/// Form1でサムネイル作成が更新されたときに呼び出される
		/// アップデートする必要があるかどうかをチェックし
		/// 必要であれば描写する
		/// Form1から呼び出されるルーチン
		/// </summary>
		/// <param name="Item"></param>
		public void CheckUpdateAndDraw(int Item)
		{
			//動的に更新する必要があるかどうか
			if (m_offScreen == null)
				return;

			//まだ表示されていない
			//if (m_nItemsX == 0)
			//    return;

			//高品質描写の場合は何もしない
			//サムネイルを書いても高品質で書き直す必要が出てくるため
			if (THUMBNAIL_SIZE > Form1.DEFAULT_THUMBNAIL_SIZE)
				return;

			//描写した画像がスクリーン描写対象か確認
			Rectangle rect = GetThumbboxRectanble(Item);

			//if ((sy + BOX_HEIGHT) > m_vScrollBar.Value && sy < (m_vScrollBar.Value + this.Height))
			if (rect.Bottom > 0 && rect.Top < this.Height)
			{
				lock (m_offScreen)
				{
					using (Graphics g = Graphics.FromImage(m_offScreen))
					{
						DrawItem3(g, Item);	//通常サムネイル描写
					} //using(Graphics)

					//ver0.97 これで終わり。
					using (Graphics g = this.CreateGraphics())
					{
						g.DrawImage(m_offScreen, rect, rect, GraphicsUnit.Pixel);
					}
				}
			}
			else
				Debug.WriteLine(Item, "CheckUpdateAndDraw() 描写しませんでした");

		}

		/// <summary>
		/// 指定したアイテムは描写対象かどうかをチェックする
		/// 判定にはitem内のposX、posYを利用している
		/// </summary>
		/// <param name="item">チェックするアイテム</param>
		/// <returns>描写対象であればtrue</returns>
		private bool CheckNecessaryToDrawItem(int item)
		{
			int tempY = m_thumbnailSet[item].posY;
			int vScValue = m_vScrollBar.Value;				//スクロールバーの値。よく使うので変数に

			if (tempY > vScValue - BOX_HEIGHT
				&& tempY < vScValue + this.Height)
			{
				return true;
			}
			else
				return false;
		}

		//*** タイマー **********************************************************

		
		/// <summary>
		/// ツールチップを表示するタイマー
		/// きたらタイマーを止めてツールチップを表示する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void m_timer_Tick(object sender, EventArgs e)
		{
			//タイマーを止める
			m_timer.Stop();

			//表示位置を特定
			Point pt = PointToClient(MousePosition);
			pt.Offset(8, 8);

			//表示
			m_tooltip.Show((string)m_tooltip.Tag, this, pt, 2000);
		}


		//*** サムネイル保存ルーチン **********************************************************

		
		/// <summary>
		/// サムネイル画像を保存する。
		/// ここでは保存用ダイアログを表示するだけ。
		/// ダイアログからSaveThumbnailImage()が呼び出される。
		/// </summary>
		/// <param name="FilenameCandidate">保存ファイル名の候補</param>
		public void SaveThumbnail(string FilenameCandidate)
		{
			if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
				return;

			//いったん保存
			int tmpThumbnailSize = THUMBNAIL_SIZE;
			int tmpScrollbarValue = m_vScrollBar.Value;

			m_saveForm = new FormSaveThumbnail(this, m_thumbnailSet, FilenameCandidate);
			m_saveForm.ShowDialog(this);
			m_saveForm.Dispose();

			//元に戻す
			SetThumbnailSize(tmpThumbnailSize);
			m_vScrollBar.Value = tmpScrollbarValue;

		}

		/// <summary>
		/// サムネイル画像一覧を作成、保存する。
		/// この関数の中で保存Bitmapを生成し、それをpng形式で保存する
		/// </summary>
		/// <param name="thumbSize">サムネイル画像のサイズ</param>
		/// <param name="numX">サムネイルの横方向の画像数</param>
		/// <param name="FilenameCandidate">保存するファイル名</param>
		/// <returns>原則true、保存しなかった場合はfalse</returns>
		public bool SaveThumbnailImage(int thumbSize, int numX, string FilenameCandidate)
		{
			//初期化済みか確認
			if (m_thumbnailSet == null)
				return false;

			//アイテム数を確認
			int ItemCount = m_thumbnailSet.Count;
			if (ItemCount <= 0)
				return false;

			//サムネイルがあるか確認
			//foreach (ImageInfo ti in m_thumbnailSet)
			//    if (ti.ThumbImage == null)
			//        return false;

			////サムネイル作成を待つ
			//while (tStatus != ThreadStatus.STOP)
			//    Application.DoEvents();

			//スクロールバーの位置を補正
			//重要：GetThumbboxRectanble(Item)で計算に利用している。
			m_vScrollBar.Value = 0;


			//サムネイルサイズを設定.再計算
			SetThumbnailSize(thumbSize);

			//アイテム数を設定
			//m_nItemsX = numX;
			//m_nItemsY = ItemCount / m_nItemsX;	//縦に並ぶアイテム数はサムネイルの数による
			//if (ItemCount % m_nItemsX > 0)
			//    m_nItemsY++;						//割り切れなかった場合は1行追加

			Size offscreenSize = calcScreenSize();

			//Bitmapを生成
			Bitmap saveBmp = new Bitmap(offscreenSize.Width, offscreenSize.Height);
			Bitmap dummyBmp = new Bitmap(BOX_WIDTH, BOX_HEIGHT);

			using (Graphics g = Graphics.FromImage(saveBmp))
			{
				//対象矩形を背景色で塗りつぶす.
				g.Clear(BackColor);


				for (int item = 0; item < m_thumbnailSet.Count; item++)
				{
					using (Graphics dummyg = Graphics.FromImage(dummyBmp))
					{
						//高品質画像を描写
						DrawItemHQ2(dummyg, item);

						//ダミーに描写した画像を描写する。
						Rectangle r = GetThumbboxRectanble(item);
						g.DrawImageUnscaled(dummyBmp, r);

						//画像情報を文字描写する
						DrawTextInfo(g, item, r);
					}

					ThumbnailEventArgs ev = new ThumbnailEventArgs();
					ev.HoverItemNumber = item;
					ev.HoverItemName = m_thumbnailSet[item].filename;
					this.SavedItemChanged(null, ev);
					Application.DoEvents();

					//キャンセル処理
					if (m_saveForm.isCancel)
						return false;
				}

				//for (int iy = 0; iy < m_nItemsY; iy++)
				//{
				//    for (int ix = 0; ix < m_nItemsX; ix++)
				//    {
				//        int Item = iy * m_nItemsX + ix;
				//        if (Item >= ItemCount)
				//            break;	//ixしか抜けられないが大丈夫なはず

				//        //if (THUMBNAIL_SIZE > DEFAULT_THUMBNAIL_SIZE)
				//        using (Graphics dummyg = Graphics.FromImage(dummyBmp))
				//        {
				//            //高品質画像を描写
				//            DrawItemHQ2(dummyg, Item);

				//            //ダミーに描写した画像を描写する。
				//            Rectangle r = GetThumbboxRectanble(Item);
				//            g.DrawImageUnscaled(dummyBmp, r);

				//            //画像情報を文字描写する
				//            DrawTextInfo(g, Item, r);
							
				//        }
				//        //else
				//        //    DrawItem3(g, Item);

				//        ThumbnailEventArgs ev = new ThumbnailEventArgs();
				//        ev.HoverItemNumber = Item;
				//        ev.HoverItemName = m_thumbnailSet[Item].filename;
				//        this.SavedItemChanged(null, ev);
				//        Application.DoEvents();

				//        //キャンセル処理
				//        if(m_saveForm.isCancel)
				//            return false;
				//    }
				//}
			}

			saveBmp.Save(FilenameCandidate);
			saveBmp.Dispose();
			return true;
		}
	}

	/// <summary>
	/// 追加専用のキャッシュ。
	/// Dictionary<>を使った連想配列でデータに名前をつけて保存できる。
	/// 基本は追加と参照のみ。消すときは全部消す
	/// 
	/// サムネイルで高品質サムネイルを一時保持するために利用
	/// </summary>
	public class NamedBuffer<TKey, TValue>
	{
		// キャッシュを保存するDictionary
		static Dictionary<TKey, TValue> _cache;

		//コンストラクタ
		public NamedBuffer()
		{
			_cache = new Dictionary<TKey, TValue>();
		}

		public void Add(TKey key, TValue obj)
		{
			//キーの重複を避ける
			if (_cache.ContainsKey(key))
				_cache.Remove(key);

			_cache.Add(key, obj);
		}

		public void Delete(TKey key)
		{
			_cache.Remove(key);
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
					TValue d = (TValue)_cache[key];
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
			return _cache.ContainsKey(key);
		}

		public void Clear()
		{
			_cache.Clear();
		}
	}
}


///// <summary>
///// 半透明のフォーカス（選択）枠を描写する。
///// グラフィックカードによっては遅いので使わない
///// </summary>
///// <param name="ItemIndex">描写対象のアイテム番号</param>
///// <param name="g">描写すべきGraphic</param>
//private void DrawSemiTransparentBox(int ItemIndex, Graphics g)
//{
//    using (GraphicsPath gp = new GraphicsPath())
//    {
//        float arc = 5.0f;
//        Rectangle rect = GetThumbboxRectanble(ItemIndex);
//        rect.Inflate(-1, -1);
//        gp.StartFigure();
//        gp.AddArc(rect.Right - arc, rect.Bottom - arc, arc, arc, 0.0f, 90.0f);  // 右下
//        gp.AddArc(rect.Left, rect.Bottom - arc, arc, arc, 90.0f, 90.0f);      // 左下
//        gp.AddArc(rect.Left, rect.Top, arc, arc, 180.0f, 90.0f);            // 左上
//        gp.AddArc(rect.Right - arc, rect.Top, arc, arc, 270.0f, 90.0f);       // 右上
//        gp.CloseFigure();

//        //新しい線を書く
//        //g.DrawRectangle(Pens.LightBlue, nx * BOX_WIDTH, ny * BOX_HEIGHT - vScrollBar1.Value, BOX_WIDTH, BOX_HEIGHT);

//        using (SolidBrush brs = new SolidBrush(Color.FromArgb(32, Color.RoyalBlue)))
//        {
//            g.FillPath(brs, gp);				//塗りつぶす
//            g.DrawPath(Pens.RoyalBlue, gp);		//枠線を書く
//        }//using brs
//    }//using gp
//}
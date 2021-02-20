using System;
using System.Collections.Generic;
using System.Drawing;					//Size, Bitmap, Font , Point, Graphics
using System.Drawing.Drawing2D;			//GraphicsPath
using System.IO;						//Directory, File
using System.Windows.Forms;				//UserControl

namespace Marmi
{
    /// <summary>
    /// サムネイル専用イベントの定義
    ///   サムネイル中にマウスホバーが起きたときのためのイベント
    ///   このイベントはThumbnailPanel::ThumbnailPanel_MouseMove()で発生している
    ///   受ける側はこのEventArgsを使って受けるとアイテムが分かる。
    /// </summary>
    public class ThumbnailEventArgs : EventArgs
    {
        public int HoverItemNumber;     //Hover中のアイテム番号
        public string HoverItemName;    //Hover中のアイテム名
    }

    //public sealed class ThumbnailPanel : MomentumScrollPanel
    public sealed class ThumbnailPanel : UserControl
    {
        //共通変数の定義
        private List<ImageInfo> m_thumbnailSet;     //ImageInfoのリスト, = g_pi.Items

        private FormSaveThumbnail m_saveForm;       //サムネイル保存用ダイアログ
                                                    //private Bitmap m_offScreen;					//Bitmap. newして確保される
                                                    //private VScrollBar m_vScrollBar;			//スクロールバーコントロール
                                                    //private Size m_virtualScreenSize;			//仮想サムネイルのサイズ
                                                    //ver 0.994 使わないことにする
                                                    //private int m_nItemsX;						//offScreenに並ぶアイテムの数: SetScrollBar()で計算
                                                    //private int m_nItemsY;						//offScreenに並ぶアイテムの数: SetScrollBar()で計算
                                                    //static ThreadStatus tStatus;				//スレッドの状況を見る
                                                    //private bool m_needHQDraw;					//ハイクオリティ描写を実施済みか

        private int m_mouseHoverItem = -1;          //現在マウスがホバーしているアイテム
        private Font m_font;                        //何度も生成するのはもったいないので
        private Color m_fontColor;                  //フォントの色
                                                    //private ToolTip m_tooltip;					//ツールチップ。画像情報を表示する
                                                    //private System.Windows.Forms.Timer m_tooltipTimer
                                                    //    = new System.Windows.Forms.Timer();		//ツールチップ表示用タイマー

        //ver0.994 サムネイルモード
        //private ThumnailMode m_thumbnailMode;

        //大きなサムネイル用キャッシュ
        private NamedBuffer<int, Bitmap> m_HQcache
            = new NamedBuffer<int, Bitmap>();

        private const long ANIMATE_DURATION = 1000; //フェードインアニメーション時間
        private const int PADDING = 10;     //2014年3月23日変更。間隔狭すぎた
        private int THUMBNAIL_SIZE;         //サムネイルの大きさ。幅と高さは同一値
        private int BOX_WIDTH;              //ボックスの幅。PADDING + THUMBNAIL_SIZE + PADDING
        private int BOX_HEIGHT;             //ボックスの高さ。PADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING
        private int FONT_HEIGHT;            //FONTの高さ。

        //専用イベントの定義
        public delegate void ThumbnailEventHandler(object obj, ThumbnailEventArgs e);

        //public event ThumbnailEventHandler OnHoverItemChanged;	//マウスHoverでアイテムが替わったことを知らせる。
        public event ThumbnailEventHandler SavedItemChanged;    //

        //コンテキストメニュー
        private ContextMenuStrip m_ContextMenu = new ContextMenuStrip();

        private bool fastDraw = false;

        //スクロールタイマー
        private System.Windows.Forms.Timer m_scrollTimer = null;

        private int m_targetScrollposY = 0;

        //***************************************************************************************

        #region コンストラクタ

        //***************************************************************************************
        public ThumbnailPanel()
        {
            //初期化
            this.BackColor = Color.White;   //Color.FromArgb(100, 64, 64, 64);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            //ダブルバッファ追加。昔の方法も書いておく
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            //tStatus = ThreadStatus.STOP;

            //ツールチップの初期化
            //m_tooltip = new ToolTip();
            ////フォームがアクティブでない時でもToolTipを表示する
            //m_tooltip.ShowAlways = false;

            ////ツールチップタイマーの初期化
            //m_tooltipTimer.Interval = 700;
            //m_tooltipTimer.Tick += new EventHandler((o,e)=>
            //    {
            //        Point pt = PointToClient(MousePosition);
            //        pt.Offset(8, 8);
            //        m_tooltip.Show(m_tooltip.Tag as string, this, pt, 3000);
            //        m_tooltipTimer.Stop();
            //    });

            //スクロールバーの初期化
            this.AutoScroll = true;

            //フォント生成
            SetFont(new Font("ＭＳ ゴシック", 9), Color.Black);

            //サムネイルサイズからBOXの値を決定する。
            SetThumbnailSize(App.DEFAULT_THUMBNAIL_SIZE);

            //コンテキストメニューの初期化
            InitContextMenu();

            //スクロールタイマー
            m_scrollTimer = new System.Windows.Forms.Timer();
            m_scrollTimer.Interval = 50;
            m_scrollTimer.Tick += m_scrollTimer_Tick;
        }

        ~ThumbnailPanel()
        {
            m_font.Dispose();
            //m_tooltip.Dispose();
            m_HQcache.Clear();
            //m_timer.Tick -= new EventHandler(m_timer_Tick);
            //m_timer.Dispose();
        }

        #endregion コンストラクタ

        //***************************************************************************************

        #region publicメソッド

        public void Init()
        {
            //m_needHQDraw = false;
            m_HQcache.Clear();          //ver0.974
                                        //m_thumbnailSet.Clear();		//ver0.974 ポインタを貰っているだけなのでここでやらない

            //スクロール位置の初期化
            AutoScrollPosition = Point.Empty;
        }

        /// <summary>
        /// サムネイル画像１つのサイズを変更する
        /// option Formで変更されたあと再設定されることを想定
        /// 幅：サムネイルの両脇にPADDING分が追加
        /// 高：サムネイルの上下にPADDING分が追加
        /// 下につく予定の文字列は入っていない
        /// </summary>
        /// <param name="thumbnailSize">新しいサムネイルサイズ</param>
        public void SetThumbnailSize(int thumbnailSize)
        {
            //ver0.982 HQcacheがすぐクリアされるので変更
            //サムネイルサイズが変わっていたら変更する
            if (THUMBNAIL_SIZE != thumbnailSize)
            {
                THUMBNAIL_SIZE = thumbnailSize;

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

            if (App.Config.isShowTPFileName)
                BOX_HEIGHT += PADDING + FONT_HEIGHT;

            if (App.Config.isShowTPFileSize)
                BOX_HEIGHT += PADDING + FONT_HEIGHT;

            if (App.Config.isShowTPPicSize)
                BOX_HEIGHT += PADDING + FONT_HEIGHT;

            #endregion ver0.982

            //サムネイルサイズが変わると画面に表示できる
            //アイテム数が変わるので再計算
            SetScrollBar();
        }

        public void SetFont(Font f, Color fc)
        {
            m_font = f;
            m_fontColor = fc;

            //TEXT_HEIGHTの決定
            using (Bitmap bmp = new Bitmap(100, 100))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SizeF sf = g.MeasureString("テスト文字列", m_font);
                FONT_HEIGHT = (int)sf.Height;
            }

            //フォントが変わるとサムネイルサイズが変わるので計算
            SetThumbnailSize(THUMBNAIL_SIZE);
        }

        /// <summary>
        /// 次の大きさのサイズを探して設定する
        /// </summary>
        public void ThumbSizeZoomIn()
        {
            Array TSizes = Enum.GetValues(typeof(DefaultThumbSize));
            foreach (DefaultThumbSize d in TSizes)
            {
                int size = (int)d;
                if (size > THUMBNAIL_SIZE)
                {
                    SetThumbnailSize(size);
                    App.Config.ThumbnailSize = size;
                    fastDraw = false;
                    this.Invalidate();
                    return;
                }
            }
            //見つからなければなにもしない
        }

        /// <summary>
        /// 1つ前のサムネイルサイズを見つけて設定する
        /// </summary>
        public void ThumbSizeZoomOut()
        {
            Array TSizes = Enum.GetValues(typeof(DefaultThumbSize));
            Array.Sort(TSizes);
            Array.Reverse(TSizes);
            foreach (DefaultThumbSize d in TSizes)
            {
                if ((int)d < THUMBNAIL_SIZE)
                {
                    SetThumbnailSize((int)d);
                    App.Config.ThumbnailSize = (int)d;
                    fastDraw = false;
                    this.Invalidate();
                    return;
                }
            }
            //見つからなければなにもしない
        }

        #endregion publicメソッド

        //***************************************************************************************

        #region コンテキストメニュー

        private void InitContextMenu()
        {
            //コンテキストメニューの準備
            this.ContextMenuStrip = m_ContextMenu;
            m_ContextMenu.ShowImageMargin = false;
            m_ContextMenu.ShowCheckMargin = true;
            ToolStripSeparator separator = new ToolStripSeparator();
            //ToolStripMenuItem filename = new ToolStripMenuItem("");
            ToolStripMenuItem addBookmark = new ToolStripMenuItem("しおりをはさむ");
            ToolStripMenuItem Bookmarks = new ToolStripMenuItem("しおり一覧");
            ToolStripMenuItem thumbnailLabel = new ToolStripMenuItem("サムネイルサイズ") { Enabled = false };

            ToolStripMenuItem thumbSizeBig = new ToolStripMenuItem("最大");
            ToolStripMenuItem thumbSizeLarge = new ToolStripMenuItem("大");
            ToolStripMenuItem thumbSizeNormal = new ToolStripMenuItem("中");
            ToolStripMenuItem thumbSizeSmall = new ToolStripMenuItem("小");
            ToolStripMenuItem thumbSizeTiny = new ToolStripMenuItem("最小");

            ToolStripMenuItem thumbShadow = new ToolStripMenuItem("影をつける");
            ToolStripMenuItem thumbFrame = new ToolStripMenuItem("枠線");

            m_ContextMenu.Items.Add("キャンセル");
            m_ContextMenu.Items.Add(separator);

            m_ContextMenu.Items.Add(addBookmark);
            m_ContextMenu.Items.Add(Bookmarks);
            m_ContextMenu.Items.Add(separator);

            m_ContextMenu.Items.Add(thumbShadow);
            m_ContextMenu.Items.Add(thumbFrame);
            m_ContextMenu.Items.Add("-");

            //m_ContextMenu.Items.Add(thumbSizeDropDown);
            m_ContextMenu.Items.Add(thumbnailLabel);
            m_ContextMenu.Items.Add(thumbSizeBig);
            m_ContextMenu.Items.Add(thumbSizeLarge);
            m_ContextMenu.Items.Add(thumbSizeNormal);
            m_ContextMenu.Items.Add(thumbSizeSmall);
            m_ContextMenu.Items.Add(thumbSizeTiny);
            //m_ContextMenu.Items.Add(Info);

            //Openしたときの初期化
            m_ContextMenu.Opening += new System.ComponentModel.CancelEventHandler((s, e) =>
            {
                ////ツールチップがあったら消す
                //if (m_tooltip.Active)
                //    m_tooltip.Hide(this);

                ////ツールチップタイマーを解除
                //if (m_tooltipTimer.Enabled)
                //    m_tooltipTimer.Stop();

                int index = GetHoverItem(PointToClient(MousePosition));
                m_ContextMenu.Tag = index;
                if (index >= 0)
                {
                    //filename.Text = Path.GetFileName(m_thumbnailSet[index].filename);
                    //filename.Enabled = true;
                    addBookmark.Checked = m_thumbnailSet[index].isBookMark;
                    addBookmark.Enabled = true;
                }
                else
                {
                    //filename.Enabled = false;
                    addBookmark.Enabled = false;
                }
                //サムネイルサイズにチェックを
                thumbSizeTiny.Checked = false;
                thumbSizeSmall.Checked = false;
                thumbSizeNormal.Checked = false;
                thumbSizeLarge.Checked = false;
                thumbSizeBig.Checked = false;
                switch (THUMBNAIL_SIZE)
                {
                    case (int)DefaultThumbSize.minimum:
                        thumbSizeTiny.Checked = true;
                        break;

                    case (int)DefaultThumbSize.small:
                        thumbSizeSmall.Checked = true;
                        break;

                    case (int)DefaultThumbSize.normal:
                        thumbSizeNormal.Checked = true;
                        break;

                    case (int)DefaultThumbSize.large:
                        thumbSizeLarge.Checked = true;
                        break;

                    case (int)DefaultThumbSize.big:
                        thumbSizeBig.Checked = true;
                        break;
                }

                //影・枠にチェック
                thumbFrame.Checked = App.Config.isDrawThumbnailFrame;
                thumbShadow.Checked = App.Config.isDrawThumbnailShadow;

                //m_tooltip.Disposed += new EventHandler((se, ee) => { m_tooltip.Active = true; });

                //しおり一覧
                Bookmarks.DropDownItems.Clear();
                foreach (ImageInfo i in m_thumbnailSet)
                    if (i.isBookMark)
                        Bookmarks.DropDownItems.Add(i.filename);
            });
            m_ContextMenu.ItemClicked += new ToolStripItemClickedEventHandler(m_ContextMenu_ItemClicked);
        }

        private void m_ContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "最小":
                    SetThumbnailSize((int)DefaultThumbSize.minimum);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.minimum;
                    break;

                case "小":
                    SetThumbnailSize((int)DefaultThumbSize.small);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.small;
                    break;

                case "中":
                    SetThumbnailSize((int)DefaultThumbSize.normal);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.normal;
                    break;

                case "大":
                    SetThumbnailSize((int)DefaultThumbSize.large);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.large;
                    break;

                case "最大":
                    SetThumbnailSize((int)DefaultThumbSize.big);
                    App.Config.ThumbnailSize = (int)DefaultThumbSize.big;
                    break;

                case "影をつける":
                    App.Config.isDrawThumbnailShadow = !App.Config.isDrawThumbnailShadow;
                    //Invalidate();
                    break;

                case "枠線":
                    App.Config.isDrawThumbnailFrame = !App.Config.isDrawThumbnailFrame;
                    //Invalidate();
                    break;

                case "しおりをはさむ":
                    int index = (int)m_ContextMenu.Tag;
                    m_thumbnailSet[index].isBookMark = !m_thumbnailSet[index].isBookMark;
                    //this.Invalidate();
                    break;

                case "しおり一覧":
                    break;

                default:
                    break;
            }
            //画面を書き直す
            fastDraw = false;
            this.Invalidate();
        }

        #endregion コンテキストメニュー

        //***************************************************************************************

        #region override関数

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            //TODO いつかm_thumbnailSetをなくす。
            //ver1.41 m_thumbnailSetはここでセットする.
            if (Visible)
                m_thumbnailSet = Form1.g_pi.Items;
        }

        protected override void OnResize(EventArgs e)
        {
            //スクロールバーの表示を更新
            SetScrollBar();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Uty.WriteLine("ThumbnailPanel::OnPaint() ClipRect={0}", e.ClipRectangle);
            //背景色で塗りつぶす
            e.Graphics.Clear(this.BackColor);

            //描写対象があるかチェックする。無ければ終了
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //描写品質の決定
            e.Graphics.InterpolationMode =
                fastDraw ?
                InterpolationMode.NearestNeighbor :
                InterpolationMode.HighQualityBicubic;

            //描写すべきアイテムだけ描写する
            //for (int item = 0; item < m_thumbnailSet.Count; item++)
            //{
            //    if (CheckNecessaryToDrawItem(item, e.ClipRectangle))
            //    {
            //        //count++;
            //        DrawItem3(e.Graphics, item);
            //    }
            //}

            //ver1.41 高速描写
            ////左上のアイテム番号
            //int horizonItems = this.ClientRectangle.Width / BOX_WIDTH;
            //if (horizonItems < 1) horizonItems = 1;
            //int startitem = (-AutoScrollPosition.Y / BOX_HEIGHT) * horizonItems;
            ////右下のアイテム番号＝(スクロール量＋画面縦）÷BOX縦 の切り上げ×横アイテム数
            //int enditem = (int)Math.Ceiling((double)(-AutoScrollPosition.Y + ClientRectangle.Height) / (double)BOX_HEIGHT) * horizonItems;
            //if (enditem >= m_thumbnailSet.Count)
            //    enditem = m_thumbnailSet.Count - 1;

            //デバッグ用：クリップ領域を表示
            //e.Graphics.DrawRectangle(Pens.Red, e.ClipRectangle);

            //ver1.41a さらにClipRectangleをつかって絞り込む
            int horizonItems = this.ClientRectangle.Width / BOX_WIDTH;
            if (horizonItems < 1) horizonItems = 1;
            int startitem = ((-AutoScrollPosition.Y + e.ClipRectangle.Y) / BOX_HEIGHT) * horizonItems;
            //右下のアイテム番号＝(スクロール量＋画面縦）÷BOX縦 の切り上げ×横アイテム数
            int enditem = (int)Math.Ceiling((double)(-AutoScrollPosition.Y + e.ClipRectangle.Bottom) / (double)BOX_HEIGHT) * horizonItems;
            if (enditem >= m_thumbnailSet.Count)
                enditem = m_thumbnailSet.Count - 1;
            //Uty.WriteLine("OnPaint Item = {0} to {1}", startitem, enditem);
            //必要なものを描写
            for (int item = startitem; item <= enditem; item++)
            {
                DrawItem3(e.Graphics, item);
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
            int itemIndex = GetHoverItem(pos);  //ホバー中のアイテム番号
            if (itemIndex == m_mouseHoverItem)
            {
                //マウスがホバーしているアイテムが変わらないときは何もしない。
                return;
            }

            //ホバーアイテムが替わっているので再描写
            fastDraw = false;
            int temp = m_mouseHoverItem;
            m_mouseHoverItem = itemIndex;
            if (temp >= 0)
                this.Invalidate(GetThumbboxRectanble(temp));
            if (itemIndex >= 0)
                this.Invalidate(GetThumbboxRectanble(itemIndex));
            this.Update();
            //this.Invalidate();
            //this.Refresh();

            //ホバーアイテムが替わったことを伝える
            //m_mouseHoverItem = itemIndex;

            //ver1.20 2011年10月9日 再描写が終わったら帰っていい
            if (itemIndex < 0)
                return;

            //Hoverしているアイテムが替わったことを示すイベントを発生させる
            //このイベントはメインFormで受け取りStatusBarの表示を変える。
            //ThumbnailEventArgs he = new ThumbnailEventArgs();
            //he.HoverItemNumber = m_mouseHoverItem;
            //he.HoverItemName = m_thumbnailSet[m_mouseHoverItem].filename;
            //this.OnHoverItemChanged(this, he);

            //ver1.20 イベント通知をやめる
            //ステータスバーを変更
            string s = string.Format(
                "[{0}]{1}",
                itemIndex + 1,
                m_thumbnailSet[m_mouseHoverItem].filename);
            Form1._instance.setStatusbarInfo(s);

            //ToolTipを表示する
            //string sz = String.Format(
            //    "{0}\n {1}\n 日付: {2:yyyy年M月d日 H:m:s}\n ファイルサイズ: {3:N0}bytes\n 画像サイズ: {4:N0}x{5:N0}ピクセル",
            //    Path.GetFileName(m_thumbnailSet[itemIndex].filename),
            //    Path.GetDirectoryName(m_thumbnailSet[itemIndex].filename),
            //    m_thumbnailSet[itemIndex].CreateDate,
            //    m_thumbnailSet[itemIndex].length,
            //    m_thumbnailSet[itemIndex].originalWidth,
            //    m_thumbnailSet[itemIndex].originalHeight
            //);
            //m_tooltip.Show(sz, this, e.Location, 3000);

            //Timerで表示
            //m_tooltip.Tag = sz;
            ////ToolTipにはTextがないのでTagに保存する。Timer内で利用
            //if (m_tooltipTimer.Enabled)
            //    m_tooltipTimer.Stop();
            //m_tooltipTimer.Start();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                //クリック位置の画像を取得
                int index = GetHoverItem(PointToClient(Cursor.Position));       //m_thumbnailSet内の番号
                if (index < 0)
                    return;
                else
                {
                    (Form1._instance).SetViewPage(index);
                    //表示をやめ終了
                    Form1._instance.SetThumbnailView(false);
                    return;
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.Focus();
            base.OnMouseEnter(e);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            //fastDraw = true;
            base.OnScroll(se);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);

            var y = -this.AutoScrollPosition.Y;
            if (e.Delta > 0)
            {
                y = y - 250;
                if (y < 0)
                    y = 0;
            }
            else
            {
                y = y + 250;
                Size virtualScreenSize = CalcScreenSize();
                int availablerange = virtualScreenSize.Height - this.ClientRectangle.Height;
                if (y > availablerange)
                    y = availablerange;
            }
            if (App.Config.ThumbnailPanelSmoothScroll)
            {
                //アニメーションをする。
                //スクロールタイマーの起動
                m_targetScrollposY = y;
                if (m_scrollTimer.Enabled == false)
                    m_scrollTimer.Start();
            }
            else
            {
                //すぐにスクロール
                AutoScrollPosition = new Point(0, y);
                return;
            }
        }

        private void m_scrollTimer_Tick(object sender, EventArgs e)
        {
            //現在のスクロールバーの位置。＋に直す。
            var y = -this.AutoScrollPosition.Y;

            //スクロール量の計算
            var delta = (m_targetScrollposY - y) / 2;
            if (delta > 100) delta = 70;
            if (delta < -100) delta = -70;
            if (delta == 0) delta = (m_targetScrollposY - y);
            var newY = y + delta;

            //スクロール
            AutoScrollPosition = new Point(0, newY);
            if (newY == y)
                m_scrollTimer.Stop();
            //throw new NotImplementedException();
        }

        #endregion override関数

        public void Application_Idle()
        {
            if (fastDraw)
            {
                fastDraw = false;
                Invalidate();
            }
        }

        //*** スクロールバー ********************************************************************

        /// <summary>
        /// スクロールバーの基本設定
        /// スクロールバーを表示するかどうかを判別し、必要に応じて表示、設定する。
        /// 必要がない場合はValueを０に設定しておく。
        /// 主にリサイズイベントが発生したときに呼び出される
        /// </summary>
        private void SetScrollBar()
        {
            //初期化済みか確認
            if (m_thumbnailSet == null)
            {
                this.AutoScrollMinSize = this.ClientRectangle.Size;
                return;
            }

            //描写に必要なサイズを確認する。
            //描写領域の大きさ。まずは自分のクライアント領域を得る
            Size virtualScreenSize = CalcScreenSize();
            this.AutoScrollMinSize = new Size(1, virtualScreenSize.Height);
            Uty.WriteLine("virtualScreenSize" + virtualScreenSize.ToString());
            Uty.WriteLine("AutoScrollMinSize=" + this.AutoScrollMinSize.ToString());
            Uty.WriteLine("this.clientrect=" + this.ClientRectangle.ToString());
        }

        /// <summary>
        /// スクリーンサイズを計算する
        /// 縦方向が大きければスクロールバーが必要ということ
        /// スクロールバーは最初からサイズとして考慮
        /// </summary>
        private Size CalcScreenSize()
        {
            //アイテム数を確認
            int itemCount = m_thumbnailSet.Count;

            //ver1.20ClientRectangleを使うことでスクロールバー考慮
            //const int scrollControllWidth = 20;

            //描写に必要なサイズを確認する。
            //描写領域の大きさ。まずは自分のクライアント領域を得る
            int screenWidth = this.ClientRectangle.Width;
            if (screenWidth < 1) screenWidth = 1;
            int screenHeight = this.ClientRectangle.Height;
            if (screenHeight < 1) screenHeight = 1;

            //各アイテムの位置を決定する
            int tempx = 0;
            int tempy = 0;

            //TODO:スクリーンサイズは160以上あることが前提
            //Debug.Assert(screenWidth > 160);

            for (int i = 0; i < itemCount; i++)
            {
                //if ((tempx + THUMBNAIL_SIZE + PADDING*2) > (screenWidth - scrollControllWidth))
                if ((tempx + THUMBNAIL_SIZE + PADDING * 2) > screenWidth)
                {
                    //キャリッジリターン
                    tempx = 0;
                    tempy += BOX_HEIGHT;
                }

                //アイテムの位置を保存しておく
                tempx += PADDING;
                //m_thumbnailSet[i].posX = tempx;
                //m_thumbnailSet[i].posY = tempy;

                //Xを次の位置に移動させる
                tempx += THUMBNAIL_SIZE + PADDING;
            }//for

            //最後の列に画像の高さ分を追加
            screenHeight = tempy + BOX_HEIGHT;
            return new Size(screenWidth, screenHeight);
        }

        //***************************************************************************************

        #region アイテム描写

        private void DrawItem3(Graphics g, int item)
        {
            //Uty.WriteLine("DrawItem3({0}", item);

            //描写位置の決定
            Rectangle thumbnailBoxRect = GetThumbboxRectanble(item);

            //対象矩形を背景色で塗りつぶす.
            //そうしないと前に描いたアイコンが残ってしまう可能性有り
            //using (SolidBrush s = new SolidBrush(BackColor))
            //{
            //    g.FillRectangle(s, thumbnailBoxRect);
            //}

            //描写するビットマップを準備
            //bool isDrawFrame = true;
            Bitmap drawBitmap = m_thumbnailSet[item].thumbnail as Bitmap;
            Rectangle imageRect = GetThumbImageRectangle(item);

            if (drawBitmap == null)
            {
                //画像がないときは非同期で取ってくる
                //スタック型の非同期GetBitmapに変更
                Form1._instance.AsyncGetBitmap(item, (MethodInvoker)(() =>
                {
                    //ver1.75 サムネイルがないので作る
                    Form1.g_pi.AsyncThumnailMaker(item);

                    if (this.Visible)
                    {
                        if (App.Config.isThumbFadein)
                        {
                            //フェードインアニメーションで表示
                            m_thumbnailSet[item].animateStartTime = DateTime.Now.Ticks;
                            var timer = new System.Windows.Forms.Timer();
                            timer.Interval = 50;
                            timer.Tick += (s, e) =>
                            {
                                this.Invalidate(GetThumbboxRectanble(item));
                                //this.Update();
                                TimeSpan tp = new TimeSpan(DateTime.Now.Ticks - m_thumbnailSet[item].animateStartTime);
                                if (tp.TotalMilliseconds > ANIMATE_DURATION)
                                {
                                    timer.Stop();
                                    timer.Dispose();
                                }
                            };
                            timer.Start();
                        }
                        else
                        {
                            //すぐに描写
                            this.Invalidate(GetThumbboxRectanble(item));
                        }
                    }
                }));

                thumbnailBoxRect.Inflate(-PADDING, -PADDING);
                thumbnailBoxRect.Height = THUMBNAIL_SIZE;
                g.FillRectangle(Brushes.White, thumbnailBoxRect);
                thumbnailBoxRect.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightGray, thumbnailBoxRect);
                return;
            }

            //画像を描写
            TimeSpan diff = new TimeSpan(DateTime.Now.Ticks - m_thumbnailSet[item].animateStartTime);
            if (diff.TotalMilliseconds < 0 || diff.TotalMilliseconds > ANIMATE_DURATION)
            {
                //通常描写
                m_thumbnailSet[item].animateStartTime = 0;

                //影の描写
                Rectangle frameRect = imageRect;
                if (App.Config.isDrawThumbnailShadow) // && isDrawFrame)
                {
                    BitmapUty.drawDropShadow(g, frameRect);
                }
                g.FillRectangle(Brushes.White, imageRect);

                //画像を描写
                g.DrawImage(drawBitmap, imageRect);

                //外枠を書く。
                if (App.Config.isDrawThumbnailFrame) // && isDrawFrame)
                {
                    g.DrawRectangle(Pens.LightGray, frameRect);
                }

                //Bookmarkを示すマークを描く
                if (m_thumbnailSet[item].isBookMark)
                {
                    using (Pen p = new Pen(Color.DarkRed, 2f))
                        g.DrawRectangle(p, frameRect);
                    g.FillEllipse(Brushes.Red, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                    g.DrawEllipse(Pens.White, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                }
            }
            else
            {
                //経過時刻に従って半透明描写
                float a = (float)diff.TotalMilliseconds / ANIMATE_DURATION;
                if (a > 1)
                    a = 1.0f;
                BitmapUty.alphaDrawImage(g, drawBitmap, imageRect, a);
            }

            //フォーカス枠
            if (item == m_mouseHoverItem)
            {
                using (Pen p = new Pen(Color.DodgerBlue, 3f))
                    g.DrawRectangle(p, imageRect);
            }

            //画像情報文字列を描く
            DrawTextInfo(g, item, thumbnailBoxRect);
        }

        #endregion アイテム描写

        //高品質専用描写DrawItem.
        //ダミーBMPに描写するため描写位置を固定とする。
        private void DrawItemHQ2(Graphics g, int item)
        {
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
            if (Parent == null)
                //親ウィンドウがなくなっているので何もしない
                return;

            Bitmap drawBitmap = GetBitmap(item);

            //フラグ設定
            bool drawFrame = true;          //枠線を描写するか
            bool isResize = true;           //リサイズが必要か（可能か）どうかのフラグ
            int w;                          //描写画像の幅
            int h;                          //描写画像の高さ

            if (drawBitmap == null)
            {
                //サムネイルは準備できていない
                drawBitmap = getDummyBitmap();
                drawFrame = false;
                isResize = false;
                w = drawBitmap.Width;   //描写画像の幅
                h = drawBitmap.Height;  //描写画像の高さ
            }
            else
            {
                w = drawBitmap.Width;   //描写画像の幅
                h = drawBitmap.Height;  //描写画像の高さ

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

            int sx = (BOX_WIDTH - w) / 2;           //画像描写X位置
            int sy = THUMBNAIL_SIZE + PADDING - h;  //画像描写Y位置：下揃え

            Rectangle imageRect = new Rectangle(sx, sy, w, h);

            //影を描写する.アイコン時（＝drawFrame==false）で描写しない
            if (App.Config.isDrawThumbnailShadow && drawFrame)
            {
                Rectangle frameRect = imageRect;
                BitmapUty.drawDropShadow(g, frameRect);
            }

            //画像を書く
            //g.DrawImage(drawBitmap, sx, sy, w, h);
            //フォーカスのない画像を描写
            g.FillRectangle(Brushes.White, imageRect);
            g.DrawImage(drawBitmap, imageRect);

            //写真風に外枠を書く
            if (App.Config.isDrawThumbnailFrame && drawFrame)
            {
                Rectangle frameRect = imageRect;
                //枠がおかしいので拡大しない
                //frameRect.Inflate(2, 2);
                //g.FillRectangle(Brushes.White, frameRect);//ver1.15 コメントアウト、なんだっけ？
                g.DrawRectangle(Pens.LightGray, frameRect);
            }

            //フォーカス枠を書く
            // 画像サイズに合わせて描写
            //if (item == m_mouseHoverItem)
            //{
            //    g.DrawRectangle(
            //        new Pen(Color.IndianRed, 2.5F),
            //        GetThumbImageRectangle(item));
            //}

            ////画像情報を文字描写する
            //RectangleF tRect = new RectangleF(PADDING, PADDING + THUMBNAIL_SIZE + PADDING, THUMBNAIL_SIZE, TEXT_HEIGHT);
            //DrawTextInfo(g, Item, tRect);

            //Bitmapの破棄。GetBitmapWithoutCache()で取ってきたため
            if (drawBitmap != null
                && (string)(drawBitmap.Tag) != Properties.Resources.TAG_PICTURECACHE)
                drawBitmap.Dispose();
        }

        private Bitmap getDummyBitmap()
        {
            return Properties.Resources.rc_tif32;
            //drawBitmap = new Bitmap(THUMBNAIL_SIZE, THUMBNAIL_SIZE);
            //using (Graphics g2 = Graphics.FromImage(drawBitmap))
            //{
            //    g2.Clear(Color.LightGray);
            //    g2.DrawRectangle(Pens.DarkGray, thumbnailBoxRect);
            //    var temprect = thumbnailBoxRect;
            //    temprect.Inflate(-1, -1);
            //    g2.DrawRectangle(Pens.White, temprect);
            //}
        }

        //*** 描写支援ルーチン ****************************************************************

        private Bitmap GetBitmap(int item)
        {
            //Form1::GetBitmap()を使うので親ウィンドウチェック
            if (Parent == null)
                return null;

            //画像読み込み
            Bitmap orgBitmap = null;
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    //orgBitmap = ((Form1)Parent).GetBitmap(item);
                    //orgBitmap = Form1.g_pi.GetBitmap(item);
                    //ver1.50
                    orgBitmap = ((Form1)Parent).SyncGetBitmap(item);
                }));
            }
            else
            {
                //orgBitmap = ((Form1)Parent).GetBitmap(item);
                //orgBitmap = Form1.g_pi.GetBitmap(item);
                //ver1.50
                orgBitmap = ((Form1)Parent).SyncGetBitmap(item);
            }

            return orgBitmap;
        }

        /// <summary>
        /// 再描写関数
        /// 描写される部分をすべて再描写する。
        /// 他のクラス、フォームから呼び出される。そのためpublic
        /// 主にメニューでソートされたりしたときに呼び出される
        /// </summary>
        public void ReDraw()
        {
            //MakeThumbnailScreen();
            SetScrollBar(); //スクロールバーの設定とサムネイルへの場所登録
            fastDraw = false;
            this.Invalidate();
        }

        /// <summary>
        /// 指定サムネイルの画面内での枠を返す。
        /// Thumbbox = 画像＋文字の大きな枠
        /// スクロールバーについても織り込み済
        /// m_offScreenや実画面に対して使われることを想定
        /// </summary>
        private Rectangle GetThumbboxRectanble(int itemIndex)
        {
            // ver1.20 横方向のアイテム数 ClientRectangleでスクロールバー考慮
            //int horizonItems = this.Width/BOX_WIDTH;
            int horizonItems = this.ClientRectangle.Width / BOX_WIDTH;
            if (horizonItems <= 0) horizonItems = 1;

            //アイテムの位置（アイテム個数による仮想座標）
            int vx = itemIndex % horizonItems;
            int vy = itemIndex / horizonItems;

            return new Rectangle(
                vx * BOX_WIDTH,
                vy * BOX_HEIGHT + AutoScrollPosition.Y,
                BOX_WIDTH,
                BOX_HEIGHT
                );
        }

        /// <summary>
        /// THUMBNAILイメージの画面内での枠を返す。
        /// ThumbImage = 画像部分のみ。イメージぴったりのサイズ
        /// スクロールバー位置も織り込み済
        /// m_offScreenや実画面に対して使われることを想定
        /// </summary>
        private Rectangle GetThumbImageRectangle(int itemIndex)
        {
            //bool canExpand = true;	//拡大できるかどうかのフラグ
            int w;                      //描写画像の幅
            int h;                      //描写画像の高さ

            Image drawBitmap = m_thumbnailSet[itemIndex].thumbnail;
            if (drawBitmap == null)
            {
                //まだサムネイルは準備できていないので画像マークを呼んでおく
                drawBitmap = getDummyBitmap();
                //canExpand = false;
                w = drawBitmap.Width;
                h = drawBitmap.Height;
            }
            else if (m_thumbnailSet[itemIndex].width <= THUMBNAIL_SIZE
                     && m_thumbnailSet[itemIndex].height <= THUMBNAIL_SIZE)
            {
                //オリジナルが小さいのでリサイズしない。
                //canExpand = false;
                w = m_thumbnailSet[itemIndex].width;
                h = m_thumbnailSet[itemIndex].height;
            }
            else
            {
                //サムネイルはある.大きいので縮小
                //canExpand = true;
                float fw = drawBitmap.Width;    //描写画像の幅
                float fh = drawBitmap.Height;   //描写画像の高さ

                //拡大縮小を行う
                float ratio = (fw > fh) ? (float)THUMBNAIL_SIZE / fw : (float)THUMBNAIL_SIZE / fh;
                w = (int)(fw * ratio);
                h = (int)(fh * ratio);
            }

            Rectangle rect = GetThumbboxRectanble(itemIndex);
            rect.X += (BOX_WIDTH - w) / 2;  //画像描写X位置
            rect.Y += THUMBNAIL_SIZE + PADDING - h;     //画像描写X位置：下揃え
                                                        //rect.Y -= m_vScrollBar.Value;
            rect.Width = w;
            rect.Height = h;
            return rect;
        }

        /// <summary>
        /// ファイル名、ファイルサイズ、画像サイズをテキスト描写する
        /// </summary>
        /// <param name="g">描写先のGraphics</param>
        /// <param name="item">描写アイテム</param>
        /// <param name="thumbnailBoxRect">描写する先のサムネイルBOX矩形。テキスト位置ではない</param>
        private void DrawTextInfo(Graphics g, int item, Rectangle thumbnailBoxRect)
        {
            //テキスト描写位置を補正
            Rectangle textRect = thumbnailBoxRect;
            //textRect.Inflate(-PADDING, 0);     //左右の余白を削除
            //textRect.Y += BOX_HEIGHT;	        //画像高を追加
            //textRect.Height = FONT_HEIGHT;      //フォントの高さに合わせる
            textRect.X += PADDING;                              //左に余白を追加
            textRect.Y += PADDING + THUMBNAIL_SIZE + PADDING;   //上下に余白を追加
            textRect.Width = THUMBNAIL_SIZE;                    //横幅はサムネイルサイズと同じ
            textRect.Height = FONT_HEIGHT;

            //テキスト描写用の初期フォーマット
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;          //中央揃え
            sf.Trimming = StringTrimming.EllipsisPath;      //中間の省略

            //ファイル名を書く
            if (App.Config.isShowTPFileName)
            {
                string filename = Path.GetFileName(m_thumbnailSet[item].filename);
                if (filename != null)
                {
                    g.DrawString(filename, m_font, new SolidBrush(m_fontColor), textRect, sf);
                    textRect.Y += FONT_HEIGHT;
                }
            }

            //ファイルサイズを書く
            if (App.Config.isShowTPFileSize)
            {
                string s = String.Format("{0:#,0} bytes", m_thumbnailSet[item].length);
                g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }

            //画像サイズを書く
            if (App.Config.isShowTPPicSize)
            {
                string s = String.Format(
                    "{0:#,0}x{1:#,0} px",
                    m_thumbnailSet[item].width,
                    m_thumbnailSet[item].height);
                g.DrawString(s, m_font, new SolidBrush(m_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }
        }

        /// <summary>
        /// サムネイルアイテムが描写対象かどうかチェックする
        /// OnPaint()で使われることも考慮して
        /// 描写領域を指定できるようにする
        /// </summary>
        /// <param name="item"></param>
        /// <param name="screenRect"></param>
        /// <returns></returns>
        private bool CheckNecessaryToDrawItem(int item, Rectangle screenRect)
        {
            Rectangle itemRect = GetThumbboxRectanble(item);
            return screenRect.IntersectsWith(itemRect);
        }

        /// <summary>
        /// 指定した位置にあるアイテム番号を返す
        /// MouseHoverでの利用を想定。
        /// スクロールバーの位置も利用して補正した値を返す
        /// </summary>
        /// <param name="pos">調べたい位置</param>
        /// <returns>その場所にあるアイテム番号。ない場合は-1</returns>
        private int GetHoverItem(Point pos)
        {
            //縦スクロールバーが表示されているときは換算
            //if (m_vScrollBar.Enabled)
            //    pos.Y += m_vScrollBar.Value;
            pos.Y -= AutoScrollPosition.Y;

            int itemPointX = pos.X / BOX_WIDTH;     //マウス位置のBOX座標換算：X
            int itemPointY = pos.Y / BOX_HEIGHT;    //マウス位置のBOX座標換算：Y

            //横に並べられる数。最低１
            int horizonItems = (this.ClientRectangle.Width) / BOX_WIDTH;
            if (horizonItems <= 0) horizonItems = 1;

            //ホバー中のアイテム番号
            int index = itemPointY * horizonItems + itemPointX;

            //指定ポイントにアイテムがあるか
            if (itemPointX > horizonItems - 1
                || index > m_thumbnailSet.Count - 1)
                return -1;
            else
                return index;
        }

        //***************************************************************************************

        #region サムネイルのファイル保存

        /// <summary>
        /// サムネイル画像を保存する。
        /// ここでは保存用ダイアログを表示するだけ。
        /// ダイアログからSaveThumbnailImage()が呼び出される。
        /// </summary>
        /// <param name="filenameCandidate">保存ファイル名の候補</param>
        public void SaveThumbnail(string filenameCandidate)
        {
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //いったん保存
            int tmpThumbnailSize = THUMBNAIL_SIZE;
            //int tmpScrollbarValue = m_vScrollBar.Value;

            m_saveForm = new FormSaveThumbnail(this, m_thumbnailSet, filenameCandidate);
            m_saveForm.ShowDialog(this);
            m_saveForm.Dispose();

            //元に戻す
            SetThumbnailSize(tmpThumbnailSize);
            //m_vScrollBar.Value = tmpScrollbarValue;
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

            //サムネイルサイズを設定.再計算
            SetThumbnailSize(thumbSize);

            //アイテム数を設定
            //m_nItemsX = numX;
            //m_nItemsY = ItemCount / m_nItemsX;	//縦に並ぶアイテム数はサムネイルの数による
            //if (ItemCount % m_nItemsX > 0)
            //    m_nItemsY++;						//割り切れなかった場合は1行追加

            Size offscreenSize = CalcScreenSize();

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

                    //ver1.31 nullチェック
                    if (SavedItemChanged != null)
                        this.SavedItemChanged(null, ev);
                    Application.DoEvents();

                    //キャンセル処理
                    if (m_saveForm.isCancel)
                        return false;
                }
            }

            saveBmp.Save(FilenameCandidate);
            saveBmp.Dispose();
            return true;
        }

        #endregion サムネイルのファイル保存
    }
}
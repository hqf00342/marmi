using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
サムネイルパネル
*/

namespace Marmi
{
    public sealed class ThumbnailPanel : UserControl
    {
        private readonly List<ImageInfo> m_thumbnailSet; //ImageInfoのリスト, = g_pi.Items
        private FormSaveThumbnail m_saveForm;   //サムネイル保存用ダイアログ
        private int m_mouseHoverItem = -1;      //現在マウスがホバーしているアイテム

        private const int PADDING = 10;         //サムネイルの余白。2014年3月23日変更。間隔狭すぎた
        private int _thumbnailSize;             //サムネイルの大きさ。幅＝高さ
        private int _thumbBoxWidth;             //サムネイルBOXのサイズ：幅 = PADDING + THUMBNAIL_SIZE + PADDING
        private int _thumbBoxHeight;            //サムネイルBOXのサイズ：高さ = PADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING

        //フォント
        private Font _font;

        private Color _fontColor;
        private const string FONTNAME = "ＭＳ ゴシック";
        private const int FONTSIZE = 9;
        private int FONT_HEIGHT; //SetFont()内で設定される。

        //サムネイル保存ダイアログに知らせるイベントハンドラー
        public event EventHandler<ThumbnailEventArgs> SavedItemChanged;

        //コンテキストメニュー
        private readonly ContextMenuStrip m_ContextMenu = new ContextMenuStrip();

        //スクロールタイマー
        private readonly Timer m_scrollTimer = null;

        private int m_targetScrollposY = 0;

        private Bitmap DummyImage => Properties.Resources.rc_tif32;

        public ThumbnailPanel()
        {
            //初期化
            this.BackColor = Color.White;   //Color.FromArgb(100, 64, 64, 64);
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            //画像一覧
            m_thumbnailSet = App.g_pi.Items;

            //ダブルバッファ追加。昔の方法も書いておく
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            //スクロールバーの初期化
            this.AutoScroll = true;

            //フォント生成
            SetFont(new Font(FONTNAME, FONTSIZE), Color.Black);

            //サムネイルサイズからBOXの値を決定する。
            CalcThumbboxSize(App.DEFAULT_THUMBNAIL_SIZE);

            //コンテキストメニューの初期化
            InitContextMenu();

            //スクロールタイマー
            m_scrollTimer = new Timer
            {
                Interval = 50
            };
            m_scrollTimer.Tick += ScrollTimer_Tick;
        }

        public void Init()
        {
            //スクロール位置の初期化
            AutoScrollPosition = Point.Empty;
        }

        /// <summary>
        /// サムネイル画像サイズから
        /// BOX_HEIGHT, BOX_WIDTH, パネルサイズを再設定する。
        /// option Formで変更されたあと再設定されることを想定
        /// 幅：サムネイルの両脇にPADDING分が追加
        /// 高：サムネイルの上下にPADDING分が追加
        /// 下につく予定の文字列は入っていない
        /// </summary>
        /// <param name="thumbnailSize">新しいサムネイルサイズ</param>
        public void CalcThumbboxSize(int thumbnailSize)
        {
            //ver0.982 HQcacheがすぐクリアされるので変更
            //サムネイルサイズが変わっていたら変更する
            if (_thumbnailSize != thumbnailSize)
            {
                _thumbnailSize = thumbnailSize;
            }

            //BOXサイズを確定
            _thumbBoxWidth = _thumbnailSize + (PADDING * 2);
            _thumbBoxHeight = _thumbnailSize + (PADDING * 2);

            //ver0.982ファイル名などの文字列表示を切り替えられるようにする

            if (App.Config.IsShowTPFileName)
                _thumbBoxHeight += PADDING + FONT_HEIGHT;

            if (App.Config.IsShowTPFileSize)
                _thumbBoxHeight += PADDING + FONT_HEIGHT;

            if (App.Config.IsShowTPPicSize)
                _thumbBoxHeight += PADDING + FONT_HEIGHT;

            //サムネイルサイズが変わると画面に表示できる
            //アイテム数が変わるので再計算
            SetScrollBar();
        }

        public void SetFont(Font font, Color color)
        {
            _font = font;
            _fontColor = color;

            //TEXT_HEIGHTの計算
            using (var bmp = new Bitmap(100, 100))
            using (var g = Graphics.FromImage(bmp))
            {
                SizeF sf = g.MeasureString("テスト文字列", _font);
                FONT_HEIGHT = (int)sf.Height;
            }

            //フォントが変わるとサムネイルサイズが変わるので計算
            CalcThumbboxSize(_thumbnailSize);
        }

        /// <summary>
        /// 次の大きさのサイズを探して設定する
        /// </summary>
        public void ThumbSizeZoomIn()
        {
            foreach (ThumbnailSize d in Enum.GetValues(typeof(ThumbnailSize)))
            {
                int size = (int)d;
                if (size > _thumbnailSize)
                {
                    CalcThumbboxSize(size);
                    App.Config.ThumbnailSize = size;
                    //_fastDraw = false;
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
            Array TSizes = Enum.GetValues(typeof(ThumbnailSize));
            Array.Sort(TSizes);
            Array.Reverse(TSizes);
            foreach (ThumbnailSize d in TSizes)
            {
                if ((int)d < _thumbnailSize)
                {
                    CalcThumbboxSize((int)d);
                    App.Config.ThumbnailSize = (int)d;
                    this.Invalidate();
                    return;
                }
            }
            //見つからなければなにもしない
        }

        #region コンテキストメニュー

        private void InitContextMenu()
        {
            //コンテキストメニューの準備
            this.ContextMenuStrip = m_ContextMenu;
            m_ContextMenu.ShowImageMargin = false;
            m_ContextMenu.ShowCheckMargin = true;

            var separator = new ToolStripSeparator();
            var addBookmark = new ToolStripMenuItem("しおりをはさむ");
            var Bookmarks = new ToolStripMenuItem("しおり一覧");
            var thumbnailLabel = new ToolStripMenuItem("サムネイルサイズ") { Enabled = false };
            var thumbSizeBig = new ToolStripMenuItem("最大");
            var thumbSizeLarge = new ToolStripMenuItem("大");
            var thumbSizeNormal = new ToolStripMenuItem("中");
            var thumbSizeSmall = new ToolStripMenuItem("小");
            var thumbSizeTiny = new ToolStripMenuItem("最小");
            var thumbShadow = new ToolStripMenuItem("影をつける");
            var thumbFrame = new ToolStripMenuItem("枠線");

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
                    addBookmark.Checked = m_thumbnailSet[index].IsBookMark;
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
                switch (_thumbnailSize)
                {
                    case (int)ThumbnailSize.Minimum:
                        thumbSizeTiny.Checked = true;
                        break;

                    case (int)ThumbnailSize.Small:
                        thumbSizeSmall.Checked = true;
                        break;

                    case (int)ThumbnailSize.Normal:
                        thumbSizeNormal.Checked = true;
                        break;

                    case (int)ThumbnailSize.Large:
                        thumbSizeLarge.Checked = true;
                        break;

                    case (int)ThumbnailSize.XLarge:
                        thumbSizeBig.Checked = true;
                        break;
                }

                //影・枠にチェック
                thumbFrame.Checked = App.Config.IsDrawThumbnailFrame;
                thumbShadow.Checked = App.Config.IsDrawThumbnailShadow;

                //m_tooltip.Disposed += new EventHandler((se, ee) => { m_tooltip.Active = true; });

                //しおり一覧
                Bookmarks.DropDownItems.Clear();
                foreach (ImageInfo i in m_thumbnailSet)
                {
                    if (i.IsBookMark)
                    {
                        Bookmarks.DropDownItems.Add(i.Filename);
                    }
                }
            });
            m_ContextMenu.ItemClicked += ContextMenu_ItemClicked;
        }

        private void ContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "最小":
                    CalcThumbboxSize((int)ThumbnailSize.Minimum);
                    App.Config.ThumbnailSize = (int)ThumbnailSize.Minimum;
                    break;

                case "小":
                    CalcThumbboxSize((int)ThumbnailSize.Small);
                    App.Config.ThumbnailSize = (int)ThumbnailSize.Small;
                    break;

                case "中":
                    CalcThumbboxSize((int)ThumbnailSize.Normal);
                    App.Config.ThumbnailSize = (int)ThumbnailSize.Normal;
                    break;

                case "大":
                    CalcThumbboxSize((int)ThumbnailSize.Large);
                    App.Config.ThumbnailSize = (int)ThumbnailSize.Large;
                    break;

                case "最大":
                    CalcThumbboxSize((int)ThumbnailSize.XLarge);
                    App.Config.ThumbnailSize = (int)ThumbnailSize.XLarge;
                    break;

                case "影をつける":
                    App.Config.IsDrawThumbnailShadow = !App.Config.IsDrawThumbnailShadow;
                    //Invalidate();
                    break;

                case "枠線":
                    App.Config.IsDrawThumbnailFrame = !App.Config.IsDrawThumbnailFrame;
                    //Invalidate();
                    break;

                case "しおりをはさむ":
                    int index = (int)m_ContextMenu.Tag;
                    m_thumbnailSet[index].IsBookMark = !m_thumbnailSet[index].IsBookMark;
                    //this.Invalidate();
                    break;

                case "しおり一覧":
                    break;

                default:
                    break;
            }
            //画面を書き直す
            this.Invalidate();
        }

        #endregion コンテキストメニュー

        #region override関数

        //protected override void OnVisibleChanged(EventArgs e)
        //{
        //    base.OnVisibleChanged(e);

        //    //TODO いつかm_thumbnailSetをなくす。
        //    //ver1.41 m_thumbnailSetはここでセットする.
        //    //if (Visible)
        //    //    m_thumbnailSet = App.g_pi.Items;
        //}

        protected override void OnResize(EventArgs e)
        {
            //スクロールバーの表示を更新
            SetScrollBar();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Debug.WriteLine($"ThumbnailPanel::OnPaint() ClipRect={e.ClipRectangle}");

            //背景色塗り
            e.Graphics.Clear(this.BackColor);

            //描写対象チェック。無ければ終了
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //描写品質の決定
            //e.Graphics.InterpolationMode = _fastDraw ?
            //    InterpolationMode.NearestNeighbor :
            //    InterpolationMode.HighQualityBicubic;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //描写範囲を表示（デバッグ用：クリップ領域）
            //e.Graphics.DrawRectangle(Pens.Red, e.ClipRectangle);

            //クリップ領域内のアイテム番号を算出
            int xItemsCount = this.ClientRectangle.Width / _thumbBoxWidth;
            if (xItemsCount < 1) xItemsCount = 1;
            int startItem = (-AutoScrollPosition.Y + e.ClipRectangle.Y) / _thumbBoxHeight * xItemsCount;

            //右下のアイテム番号＝(スクロール量＋画面縦）÷BOX縦 の切り上げ×横アイテム数
            int endItem = (int)Math.Ceiling((-AutoScrollPosition.Y + e.ClipRectangle.Bottom) / (double)_thumbBoxHeight) * xItemsCount;
            if (endItem >= m_thumbnailSet.Count)
                endItem = m_thumbnailSet.Count - 1;

            //必要なものを描写
            for (int item = startItem; item <= endItem; item++)
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

            //ホバー中のアイテム番号
            int itemIndex = GetHoverItem(pos);
            if (itemIndex == m_mouseHoverItem)
            {
                //マウスがホバーしているアイテムが変わらないときは何もしない。
                return;
            }

            //ホバーアイテムが替わっているので再描写
            int prevIndex = m_mouseHoverItem;
            m_mouseHoverItem = itemIndex;
            if (prevIndex >= 0)
                this.Invalidate(GetThumbboxRectanble(prevIndex));
            if (itemIndex >= 0)
                this.Invalidate(GetThumbboxRectanble(itemIndex));
            this.Update();

            //ver1.20 2011年10月9日 再描写が終わったら帰っていい
            if (itemIndex < 0)
                return;

            //ステータスバー変更
            string s = $"[{itemIndex + 1}]{m_thumbnailSet[m_mouseHoverItem].Filename}";
            Form1._instance.SetStatusbarInfo(s);
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                //クリック位置の画像を取得
                int index = GetHoverItem(PointToClient(Cursor.Position));       //m_thumbnailSet内の番号
                if (index < 0)
                {
                    return;
                }
                else
                {
                    await (Form1._instance).SetViewPage(index);
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

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);

            var y = -this.AutoScrollPosition.Y;
            if (e.Delta > 0)
            {
                y -= 250;
                if (y < 0)
                    y = 0;
            }
            else
            {
                y += 250;
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
                if (!m_scrollTimer.Enabled)
                    m_scrollTimer.Start();
            }
            else
            {
                //すぐにスクロール
                AutoScrollPosition = new Point(0, y);
                return;
            }
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
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

        //public void Application_Idle()
        //{
        //    if (_fastDraw)
        //    {
        //        _fastDraw = false;
        //        Invalidate();
        //    }
        //}

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
            Debug.WriteLine("virtualScreenSize" + virtualScreenSize.ToString());
            Debug.WriteLine("AutoScrollMinSize=" + this.AutoScrollMinSize.ToString());
            Debug.WriteLine("this.clientrect=" + this.ClientRectangle.ToString());
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
                if ((tempx + _thumbnailSize + (PADDING * 2)) > screenWidth)
                {
                    //キャリッジリターン
                    tempx = 0;
                    tempy += _thumbBoxHeight;
                }

                //アイテムの位置を保存しておく
                tempx += PADDING;
                //m_thumbnailSet[i].posX = tempx;
                //m_thumbnailSet[i].posY = tempy;

                //Xを次の位置に移動させる
                tempx += _thumbnailSize + PADDING;
            }//for

            //最後の列に画像の高さ分を追加
            screenHeight = tempy + _thumbBoxHeight;
            return new Size(screenWidth, screenHeight);
        }

        //***************************************************************************************

        #region アイテム描写

        /// <summary>
        /// 指定インデックスのアイテムをコントロールに描写する。
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="item">アイテム番号</param>
        private void DrawItem3(Graphics g, int item)
        {
            //描写位置の決定
            Rectangle thumbnailBoxRect = GetThumbboxRectanble(item);

            //対象矩形を背景色で塗りつぶす.
            //そうしないと前に描いたアイコンが残ってしまう可能性有り
            //using (SolidBrush s = new SolidBrush(BackColor))
            //{
            //    g.FillRectangle(s, thumbnailBoxRect);
            //}

            //描写するビットマップを準備
            Bitmap drawBitmap = m_thumbnailSet[item].Thumbnail;
            Rectangle imageRect = GetThumbImageRectangle(item);

            if (drawBitmap == null)
            {
                AsyncIO.AddJob(item, () =>
                {
                    //読み込んだらすぐに描写
                    if (this.Visible)
                    {
                        this.Invalidate(GetThumbboxRectanble(item));
                    }
                });

                //まだ読み込まれていないので枠だけ描写
                thumbnailBoxRect.Inflate(-PADDING, -PADDING);
                thumbnailBoxRect.Height = _thumbnailSize;
                g.FillRectangle(Brushes.White, thumbnailBoxRect);
                thumbnailBoxRect.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightGray, thumbnailBoxRect);
                return;
            }
            else
            {
                //通常描写
                //影の描写
                Rectangle frameRect = imageRect;
                if (App.Config.IsDrawThumbnailShadow)
                {
                    BitmapUty.DrawDropShadow(g, frameRect);
                }
                g.FillRectangle(Brushes.White, imageRect);

                //画像を描写
                g.DrawImage(drawBitmap, imageRect);

                //外枠を書く。
                if (App.Config.IsDrawThumbnailFrame)
                {
                    g.DrawRectangle(Pens.LightGray, frameRect);
                }

                //Bookmarkマークを描く
                if (m_thumbnailSet[item].IsBookMark)
                {
                    using (Pen p = new Pen(Color.DarkRed, 2f))
                        g.DrawRectangle(p, frameRect);
                    g.FillEllipse(Brushes.Red, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                    g.DrawEllipse(Pens.White, new Rectangle(frameRect.Right - 15, frameRect.Y + 5, 12, 12));
                }
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

        /// <summary>
        /// 高品質専用描写DrawItem.
        /// サムネイル一覧保存用に利用。
        /// ダミーBMPに描写するため描写位置を固定とする。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="item"></param>
        private async Task DrawItemHQ2Async(Graphics g, int item)
        {
            //対象矩形を背景色で塗りつぶす.
            g.FillRectangle(
                new SolidBrush(BackColor),
                0, 0, _thumbBoxWidth, _thumbBoxHeight);

            //描写品質を最高に
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //ver0.993 nullReferの原因追及
            //Ver0.993 2011年7月31日いろいろお試し中
            //まずなんでocacheじゃないとダメだったのか分からない
            //エラーが出る原因はやっぱり別スレッド中からの呼び出しみたい
            if (Parent == null)
            {
                //親ウィンドウがなくなっているので何もしない
                return;
            }

            //Bitmap drawBitmap = GetBitmap(item);
            var drawBitmap = await Bmp.GetBitmapAsync(item);

            //フラグ設定
            bool drawFrame = true;          //枠線を描写するか
            bool isResize = true;           //リサイズが必要か（可能か）どうかのフラグ
            int w;                          //描写画像の幅
            int h;                          //描写画像の高さ

            if (drawBitmap == null)
            {
                //サムネイルは準備できていない
                drawBitmap = DummyImage;
                drawFrame = false;
                isResize = false;
                w = drawBitmap.Width;
                h = drawBitmap.Height;
            }
            else
            {
                w = drawBitmap.Width;
                h = drawBitmap.Height;

                //リサイズすべきかどうか確認する。
                if (w <= _thumbnailSize && h <= _thumbnailSize)
                    isResize = false;
            }

            //原寸表示させるモノは出来るだけ原寸とする
            //if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
            if (isResize)
            {
                float ratio = (w > h) ?
                    (float)_thumbnailSize / (float)w :
                    (float)_thumbnailSize / (float)h;
                //if (ratio > 1)			//これをコメント化すると
                //    ratio = 1.0F;		//拡大描写も行う
                w = (int)(w * ratio);
                h = (int)(h * ratio);
            }

            int sx = (_thumbBoxWidth - w) / 2;           //画像描写X位置
            int sy = _thumbnailSize + PADDING - h;  //画像描写Y位置：下揃え

            Rectangle imageRect = new Rectangle(sx, sy, w, h);

            //影を描写する.アイコン時（＝drawFrame==false）で描写しない
            if (App.Config.IsDrawThumbnailShadow && drawFrame)
            {
                Rectangle frameRect = imageRect;
                BitmapUty.DrawDropShadow(g, frameRect);
            }

            //画像を書く
            //g.DrawImage(drawBitmap, sx, sy, w, h);
            //フォーカスのない画像を描写
            g.FillRectangle(Brushes.White, imageRect);
            g.DrawImage(drawBitmap, imageRect);

            //写真風に外枠を書く
            if (App.Config.IsDrawThumbnailFrame && drawFrame)
            {
                Rectangle frameRect = imageRect;
                //枠がおかしいので拡大しない
                //frameRect.Inflate(2, 2);
                //g.FillRectangle(Brushes.White, frameRect);//ver1.15 コメントアウト、なんだっけ？
                g.DrawRectangle(Pens.LightGray, frameRect);
            }

            ////画像情報を文字描写する
            //RectangleF tRect = new RectangleF(PADDING, PADDING + THUMBNAIL_SIZE + PADDING, THUMBNAIL_SIZE, TEXT_HEIGHT);
            //DrawTextInfo(g, Item, tRect);

            //Bitmapの破棄。GetBitmapWithoutCache()で取ってきたため
            if (drawBitmap != null && (string)(drawBitmap.Tag) != App.TAG_PICTURECACHE)
            {
                drawBitmap.Dispose();
            }
        }

        //*** 描写支援ルーチン ****************************************************************

        //private Bitmap GetBitmap(int item)
        //{
        //    //Form1::GetBitmap()を使うので親ウィンドウチェック
        //    if (Parent == null)
        //        return null;

        //    //画像読み込み
        //    Bitmap orgBitmap = null;
        //    if (InvokeRequired)
        //    {
        //        this.Invoke((Action)(() => orgBitmap = Bmp.SyncGetBitmap(item)));
        //    }
        //    else
        //    {
        //        orgBitmap = Bmp.SyncGetBitmap(item);
        //    }

        //    return orgBitmap;
        //}

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
            int horizonItems = this.ClientRectangle.Width / _thumbBoxWidth;
            if (horizonItems <= 0) horizonItems = 1;

            //アイテムの位置（アイテム個数による仮想座標）
            int vx = itemIndex % horizonItems;
            int vy = itemIndex / horizonItems;

            return new Rectangle(
                vx * _thumbBoxWidth,
                (vy * _thumbBoxHeight) + AutoScrollPosition.Y,
                _thumbBoxWidth,
                _thumbBoxHeight
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

            Image drawBitmap = m_thumbnailSet[itemIndex].Thumbnail;
            if (drawBitmap == null)
            {
                //まだサムネイルは準備できていないので画像マークを呼んでおく
                drawBitmap = DummyImage;
                //canExpand = false;
                w = drawBitmap.Width;
                h = drawBitmap.Height;
            }
            else if (m_thumbnailSet[itemIndex].Width <= _thumbnailSize
                     && m_thumbnailSet[itemIndex].Height <= _thumbnailSize)
            {
                //オリジナルが小さいのでリサイズしない。
                //canExpand = false;
                w = m_thumbnailSet[itemIndex].Width;
                h = m_thumbnailSet[itemIndex].Height;
            }
            else
            {
                //サムネイルはある.大きいので縮小
                //canExpand = true;
                float fw = drawBitmap.Width;    //描写画像の幅
                float fh = drawBitmap.Height;   //描写画像の高さ

                //拡大縮小を行う
                float ratio = (fw > fh) ? (float)_thumbnailSize / fw : (float)_thumbnailSize / fh;
                w = (int)(fw * ratio);
                h = (int)(fh * ratio);
            }

            Rectangle rect = GetThumbboxRectanble(itemIndex);
            rect.X += (_thumbBoxWidth - w) / 2;  //画像描写X位置
            rect.Y += _thumbnailSize + PADDING - h;     //画像描写X位置：下揃え
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
            textRect.Y += PADDING + _thumbnailSize + PADDING;   //上下に余白を追加
            textRect.Width = _thumbnailSize;                    //横幅はサムネイルサイズと同じ
            textRect.Height = FONT_HEIGHT;

            //テキスト描写用の初期フォーマット
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,          //中央揃え
                Trimming = StringTrimming.EllipsisPath      //中間の省略
            };

            //ファイル名を書く
            if (App.Config.IsShowTPFileName)
            {
                string filename = Path.GetFileName(m_thumbnailSet[item].Filename);
                if (filename != null)
                {
                    g.DrawString(filename, _font, new SolidBrush(_fontColor), textRect, sf);
                    textRect.Y += FONT_HEIGHT;
                }
            }

            //ファイルサイズを書く
            if (App.Config.IsShowTPFileSize)
            {
                string s = String.Format("{0:#,0} bytes", m_thumbnailSet[item].FileLength);
                g.DrawString(s, _font, new SolidBrush(_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }

            //画像サイズを書く
            if (App.Config.IsShowTPPicSize)
            {
                string s = String.Format(
                    "{0:#,0}x{1:#,0} px",
                    m_thumbnailSet[item].Width,
                    m_thumbnailSet[item].Height);
                g.DrawString(s, _font, new SolidBrush(_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }
        }

        ///// <summary>
        ///// サムネイルアイテムが描写対象かどうかチェックする
        ///// OnPaint()で使われることも考慮して
        ///// 描写領域を指定できるようにする
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="screenRect"></param>
        ///// <returns></returns>
        //private bool CheckNecessaryToDrawItem(int item, Rectangle screenRect)
        //{
        //    Rectangle itemRect = GetThumbboxRectanble(item);
        //    return screenRect.IntersectsWith(itemRect);
        //}

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
            pos.Y -= AutoScrollPosition.Y;

            int itemPointX = pos.X / _thumbBoxWidth;     //マウス位置のBOX座標換算：X
            int itemPointY = pos.Y / _thumbBoxHeight;    //マウス位置のBOX座標換算：Y

            //横に並べられる数。最低１
            int horizonItems = (this.ClientRectangle.Width) / _thumbBoxWidth;
            if (horizonItems <= 0) horizonItems = 1;

            //ホバー中のアイテム番号
            int index = (itemPointY * horizonItems) + itemPointX;

            //指定ポイントにアイテムがあるか
            return itemPointX > horizonItems - 1 || index > m_thumbnailSet.Count - 1 ? -1 : index;
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
            int tmpThumbnailSize = _thumbnailSize;
            //int tmpScrollbarValue = m_vScrollBar.Value;

            m_saveForm = new FormSaveThumbnail(this, m_thumbnailSet, filenameCandidate);
            m_saveForm.ShowDialog(this);
            m_saveForm.Dispose();

            //元に戻す
            CalcThumbboxSize(tmpThumbnailSize);
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
        public async Task<bool> SaveThumbnailImageAsync(int thumbSize, int numX, string FilenameCandidate)
        {
            //初期化済みか確認
            if (m_thumbnailSet == null)
                return false;

            //アイテム数を確認
            int ItemCount = m_thumbnailSet.Count;
            if (ItemCount <= 0)
                return false;

            //サムネイルサイズを設定.再計算
            CalcThumbboxSize(thumbSize);

            //アイテム数を設定
            //m_nItemsX = numX;
            //m_nItemsY = ItemCount / m_nItemsX;	//縦に並ぶアイテム数はサムネイルの数による
            //if (ItemCount % m_nItemsX > 0)
            //    m_nItemsY++;						//割り切れなかった場合は1行追加

            Size offscreenSize = CalcScreenSize();

            //Bitmapを生成
            Bitmap saveBmp = new Bitmap(offscreenSize.Width, offscreenSize.Height);
            Bitmap dummyBmp = new Bitmap(_thumbBoxWidth, _thumbBoxHeight);

            using (Graphics g = Graphics.FromImage(saveBmp))
            {
                //対象矩形を背景色で塗りつぶす.
                g.Clear(BackColor);

                for (int item = 0; item < m_thumbnailSet.Count; item++)
                {
                    using (Graphics dummyg = Graphics.FromImage(dummyBmp))
                    {
                        //高品質画像を描写
                        await DrawItemHQ2Async(dummyg, item);

                        //ダミーに描写した画像を描写する。
                        Rectangle r = GetThumbboxRectanble(item);
                        g.DrawImageUnscaled(dummyBmp, r);

                        //画像情報を文字描写する
                        DrawTextInfo(g, item, r);
                    }

                    ThumbnailEventArgs ev = new ThumbnailEventArgs
                    {
                        HoverItemNumber = item,
                        HoverItemName = m_thumbnailSet[item].Filename
                    };

                    //ver1.31 nullチェック
                    if (SavedItemChanged != null)
                        this.SavedItemChanged(null, ev);
                    Application.DoEvents();

                    //キャンセル処理
                    if (m_saveForm.IsCancel)
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
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
キャッシュのBitmapなどはつくらずに直接サムネイルを描写している。

TBOX: サムネイルの最大サイズ四角形。
  幅 ＝ thumbnailSize + 左右PADDING
  高 ＝ thumbnailSize + 上下PADDING +説明文字列

*/

namespace Marmi
{
    public sealed class ThumbnailPanel : UserControl
    {
        private List<ImageInfo> m_ImgSet => App.g_pi.Items; //ImageInfoのリスト, = g_pi.Items
        //private FormSaveThumbnail m_saveForm;   //サムネイル保存用ダイアログ
        private int m_mouseHoverItem = -1;      //現在マウスがホバーしているアイテム

        private const int PADDING = 10;         //サムネイルの余白。2014年3月23日変更。間隔狭すぎた
        private int _thumbnailSize;             //サムネイルの大きさ。幅＝高さ
        private int _tboxWidth;                 //サムネイルBOXのサイズ：幅 = PADDING + THUMBNAIL_SIZE + PADDING
        private int _tboxHeight;                //サムネイルBOXのサイズ：高さ = PADDING + THUMBNAIL_SIZE + PADDING + TEXT_HEIGHT + PADDING

        //フォント
        private Font _font;

        private Color _fontColor;
        private const string FONTNAME = "ＭＳ ゴシック";
        private const int FONTSIZE = 9;
        private static int FONT_HEIGHT; //SetFont()内で設定される。

        //サムネイル保存ダイアログに知らせるイベントハンドラー
        //public event EventHandler<ThumbnailEventArgs> SavedItemChanged;

        //コンテキストメニュー
        private readonly ContextMenuStrip m_ContextMenu = new ContextMenuStrip();

        //スクロールタイマー
        private readonly Timer m_scrollTimer = new Timer();

        private int m_targetScrollposY = 0;

        private Bitmap DummyImage => Properties.Resources.rc_tif32;

        public ThumbnailPanel()
        {
            //初期化
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            //ダブルバッファ。昔の方法も書いておく
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            //スクロールバーの初期化
            this.AutoScroll = true;

            //フォント生成
            SetFont(new Font(FONTNAME, FONTSIZE), Color.Black);

            //サムネイルサイズからBOXの値を決定する。
            SetThumbnailSize(App.DEFAULT_THUMBNAIL_SIZE);

            //コンテキストメニューの初期化
            InitContextMenu();

            //スクロールタイマー
            m_scrollTimer.Interval = 50;
            m_scrollTimer.Tick += ScrollTimer_Tick;
        }

        public void Init()
        {
            //スクロール位置の初期化
            AutoScrollPosition = Point.Empty;
        }

        /// <summary>
        /// サムネイルサイズを設定する。
        /// 設定と同時にスクロールバーなどのサイズも調整する。
        /// </summary>
        /// <param name="thumbnailSize">TBOXサイズ</param>
        public void SetThumbnailSize(int thumbnailSize)
        {
            _thumbnailSize = thumbnailSize;
            var size = CalcTboxSize(thumbnailSize);
            _tboxWidth = size.Width;
            _tboxHeight = size.Height;

            //サムネイルサイズが変わったので再計算
            SetScrollBar();
        }

        /// <summary>
        /// TBOXサイズを計算する。
        /// 単純にPADDING分と文字列分を足したもの。
        /// </summary>
        public static Size CalcTboxSize(int thumbnailSize)
        {
            //TBOXサイズを確定
            var w = thumbnailSize + (PADDING * 2);
            var h = thumbnailSize + (PADDING * 2);

            //文字列部追加
            if (App.Config.Thumbnail.DrawFilename)
                h += PADDING + FONT_HEIGHT;

            if (App.Config.Thumbnail.DrawFilesize)
                h += PADDING + FONT_HEIGHT;

            if (App.Config.Thumbnail.DrawPicsize)
                h += PADDING + FONT_HEIGHT;

            return new Size(w, h);
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
            SetThumbnailSize(_thumbnailSize);
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
                    SetThumbnailSize(size);
                    App.Config.Thumbnail.ThumbnailSize = size;
                    this.Invalidate();
                    return;
                }
            }
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
                    SetThumbnailSize((int)d);
                    App.Config.Thumbnail.ThumbnailSize = (int)d;
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
                    addBookmark.Checked = m_ImgSet[index].IsBookMark;
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
                thumbFrame.Checked = App.Config.Thumbnail.DrawFrame;
                thumbShadow.Checked = App.Config.Thumbnail.DrawShadowdrop;

                //m_tooltip.Disposed += new EventHandler((se, ee) => { m_tooltip.Active = true; });

                //しおり一覧
                Bookmarks.DropDownItems.Clear();
                foreach (ImageInfo i in m_ImgSet)
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
                    SetThumbnailSize((int)ThumbnailSize.Minimum);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Minimum;
                    break;

                case "小":
                    SetThumbnailSize((int)ThumbnailSize.Small);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Small;
                    break;

                case "中":
                    SetThumbnailSize((int)ThumbnailSize.Normal);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Normal;
                    break;

                case "大":
                    SetThumbnailSize((int)ThumbnailSize.Large);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.Large;
                    break;

                case "最大":
                    SetThumbnailSize((int)ThumbnailSize.XLarge);
                    App.Config.Thumbnail.ThumbnailSize = (int)ThumbnailSize.XLarge;
                    break;

                case "影をつける":
                    App.Config.Thumbnail.DrawShadowdrop = !App.Config.Thumbnail.DrawShadowdrop;
                    //Invalidate();
                    break;

                case "枠線":
                    App.Config.Thumbnail.DrawFrame = !App.Config.Thumbnail.DrawFrame;
                    //Invalidate();
                    break;

                case "しおりをはさむ":
                    int index = (int)m_ContextMenu.Tag;
                    m_ImgSet[index].IsBookMark = !m_ImgSet[index].IsBookMark;
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
            if (m_ImgSet == null || m_ImgSet.Count == 0)
                return;

            //描写品質の決定
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //クリップ領域内のアイテム番号を算出
            int xItemsCount = this.ClientRectangle.Width / _tboxWidth;
            if (xItemsCount < 1) xItemsCount = 1;
            int startItem = (-AutoScrollPosition.Y + e.ClipRectangle.Y) / _tboxHeight * xItemsCount;

            //右下のアイテム番号＝(スクロール量＋画面縦）÷BOX縦 の切り上げ×横アイテム数
            int endItem = (int)Math.Ceiling((-AutoScrollPosition.Y + e.ClipRectangle.Bottom) / (double)_tboxHeight) * xItemsCount;
            if (endItem >= m_ImgSet.Count)
                endItem = m_ImgSet.Count - 1;

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
            if (m_ImgSet == null)
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
                this.Invalidate(GetTboxRectanble(prevIndex));
            if (itemIndex >= 0)
                this.Invalidate(GetTboxRectanble(itemIndex));
            this.Update();

            //ver1.20 2011年10月9日 再描写が終わったら帰っていい
            if (itemIndex < 0)
                return;

            //ステータスバー変更
            string s = $"[{itemIndex + 1}]{m_ImgSet[m_mouseHoverItem].Filename}";
            Form1._instance.SetStatusbarInfo(s);
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                //クリック位置の画像を取得
                int index = GetHoverItem(PointToClient(Cursor.Position));
                if (index < 0)
                {
                    return;
                }
                else
                {
                    await (Form1._instance).SetViewPageAsync(index);
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
            if (App.Config.Thumbnail.SmoothScroll)
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
            if (m_ImgSet == null)
            {
                this.AutoScrollMinSize = this.ClientRectangle.Size;
                return;
            }

            //全アイテム描写に必要なサイズを確認。
            Size vScreenSize = CalcScreenSize();

            //AutoScrollMinSizeを設定する
            //このサイズを下回るとスクロールバーが現れる。
            //横スクロールバーはいらないのでX=1固定
            //縦スクロールバーは仮想画面の高さ
            this.AutoScrollMinSize = new Size(1, vScreenSize.Height);
        }

        /// <summary>
        /// 全アイテム描写に必要なスクリーンサイズを計算する
        /// スクロールバーは最初からサイズとして考慮
        /// </summary>
        private Size CalcScreenSize()
        {
            //描写領域幅=クライアント領域を得る
            //ClientRectangleを使うことでスクロールバー幅も考慮されている。
            int screenWidth = this.ClientRectangle.Width;
            if (screenWidth < 1) screenWidth = 1;

            //アイテムのおける個数をXYで求める。
            var numX = screenWidth / _tboxWidth;
            if (numX == 0) numX = 1;
            var numY = ((m_ImgSet.Count - 1) / numX) + 1;

            return new Size(this.ClientRectangle.Width, _tboxHeight * numY);
        }

        /// <summary>
        /// アイテム描写位置を計算する
        /// dot座標ではなく、アイテム座標
        /// </summary>
        /// <param name="index">インデックス番号</param>
        /// <returns>描写位置</returns>
        private (int x, int y) CalcItemPosition(int index)
        {
            var numX = this.ClientRectangle.Width / _tboxWidth;
            if (numX == 0) numX = 1;
            return (index % numX, index / numX);
        }

        /// <summary>
        /// 指定アイテムを描写する必要があるかチェック
        /// DrawItem3()で使うつもりだったがもっとClipRectでチェックしているので
        /// 不要になった
        /// </summary>
        /// <param name="index"></param>
        /// <returns>可視領域内ならtrue</returns>
        private bool NeedToDraw(int index)
        {
            //対象アイテムのRect
            //これが画面内位置に変換済のためClientRectと直接比較できる。
            var itemRect = GetTboxRectanble(index);

            //交差するかチェック
            return this.ClientRectangle.IntersectsWith(itemRect);

            //デバッグ
            //スクリーンのRect。スクロールバー考慮済
            //var screenTop = -this.AutoScrollPosition.Y; //負数なので補正
            //var screenRect = new Rectangle(0, screenTop, ClientRectangle.Width, ClientRectangle.Height);
            //this.AutoScrollPosition トップで{X=0,Y=0}、下で{X=0,Y=-9498}
            //this.AutoScrollMargin    常に{Width=0, Height=0}
            //this.AutoScrollOffset    常に{X=0,Y=0}
            //this.AutoScrollMinSize   {Width=1, Height=13455}
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
            Rectangle tboxRect = GetTboxRectanble(item);

            //クリップ範囲内かチェック
            if (!g.ClipBounds.IntersectsWith(tboxRect))
            {
                Debug.WriteLine($"クリップ領域外: {item}");
                return;
            }

            //描写するビットマップを準備
            Bitmap drawBitmap = m_ImgSet[item].Thumbnail;
            Rectangle imageRect = GetThumbImageRectangle(item);

            if (drawBitmap == null)
            {
                //枠だけ描写
                tboxRect.Inflate(-PADDING, -PADDING);
                tboxRect.Height = _thumbnailSize;
                g.FillRectangle(Brushes.White, tboxRect);
                tboxRect.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightGray, tboxRect);

                //サムネイルを作成
                Bmp.LoadBitmapAsync(item, true)
                    .ContinueWith(_ =>
                    {
                        //コントロール表示中、且つ、描写範囲内かチェック
                        //描写範囲内なら描写させる
                        if (this.Visible && NeedToDraw(item))
                        {
                            this.Invalidate(GetTboxRectanble(item));
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                return;
            }
            else
            {
                //通常描写
                //影の描写
                Rectangle frameRect = imageRect;
                if (App.Config.Thumbnail.DrawShadowdrop)
                {
                    BitmapUty.DrawDropShadow(g, frameRect);
                }
                g.FillRectangle(Brushes.White, imageRect);

                //画像を描写
                g.DrawImage(drawBitmap, imageRect);

                //外枠を書く。
                if (App.Config.Thumbnail.DrawFrame)
                {
                    g.DrawRectangle(Pens.LightGray, frameRect);
                }

                //Bookmarkマークを描く
                if (m_ImgSet[item].IsBookMark)
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
            DrawTextInfo(g, item, tboxRect);
        }

        #endregion アイテム描写

        //*** 描写支援ルーチン ****************************************************************

        /// <summary>
        /// 再描写関数
        /// 描写される部分をすべて再描写する。
        /// 他のクラス、フォームから呼び出される。そのためpublic
        /// 主にメニューでソートされたりしたときに呼び出される
        /// </summary>
        public void ReDraw()
        {
            if (this.Visible)
            {
                //MakeThumbnailScreen();
                SetScrollBar(); //スクロールバーの設定とサムネイルへの場所登録
                this.Invalidate();
            }
        }

        /// <summary>
        /// 指定TBOXの「画面内での枠位置」を返す。
        /// Tbox = 画像＋文字の大きな枠
        /// スクロールバーによる位置調整も入っている。
        /// </summary>
        private Rectangle GetTboxRectanble(int index)
        {
            // アイテム座標
            (int itemx, int itemy) = CalcItemPosition(index);

            return new Rectangle(
                itemx * _tboxWidth,
                (itemy * _tboxHeight) + AutoScrollPosition.Y,
                _tboxWidth,
                _tboxHeight);

            //AutoScrollPosition.Y はスクロールすると負の値になる。
            //それを足しこむことで画面内の位置に変換している。
        }

        /// <summary>
        /// THUMBNAILイメージの画面内での枠を返す。
        /// ThumbImage = 画像部分のみ。イメージぴったりのサイズ
        /// スクロールバー位置も織り込み済
        /// m_offScreenや実画面に対して使われることを想定
        /// </summary>
        private Rectangle GetThumbImageRectangle(int itemIndex)
        {
            int w;                      //描写画像の幅
            int h;                      //描写画像の高さ

            Image drawBitmap = m_ImgSet[itemIndex].Thumbnail;
            if (drawBitmap == null)
            {
                //まだサムネイルは準備できていないので画像マークを呼んでおく
                drawBitmap = DummyImage;
                //canExpand = false;
                w = drawBitmap.Width;
                h = drawBitmap.Height;
            }
            else if (m_ImgSet[itemIndex].Width <= _thumbnailSize
                     && m_ImgSet[itemIndex].Height <= _thumbnailSize)
            {
                //オリジナルが小さいのでリサイズしない。
                //canExpand = false;
                w = m_ImgSet[itemIndex].Width;
                h = m_ImgSet[itemIndex].Height;
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

            Rectangle rect = GetTboxRectanble(itemIndex);
            rect.X += (_tboxWidth - w) / 2;  //画像描写X位置
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
            if (App.Config.Thumbnail.DrawFilename)
            {
                string filename = Path.GetFileName(m_ImgSet[item].Filename);
                if (filename != null)
                {
                    g.DrawString(filename, _font, new SolidBrush(_fontColor), textRect, sf);
                    textRect.Y += FONT_HEIGHT;
                }
            }

            //ファイルサイズを書く
            if (App.Config.Thumbnail.DrawFilesize)
            {
                string s = $"{m_ImgSet[item].FileLength:#,0} bytes";
                g.DrawString(s, _font, new SolidBrush(_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }

            //画像サイズを書く
            if (App.Config.Thumbnail.DrawPicsize)
            {
                string s = $"{m_ImgSet[item].Width:#,0}x{m_ImgSet[item].Height:#,0} px";
                g.DrawString(s, _font, new SolidBrush(_fontColor), textRect, sf);
                textRect.Y += FONT_HEIGHT;
            }
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
            pos.Y -= AutoScrollPosition.Y;

            int itemPointX = pos.X / _tboxWidth;     //マウス位置のBOX座標換算：X
            int itemPointY = pos.Y / _tboxHeight;    //マウス位置のBOX座標換算：Y

            //横に並べられる数。最低１
            int horizonItems = (this.ClientRectangle.Width) / _tboxWidth;
            if (horizonItems <= 0) horizonItems = 1;

            //ホバー中のアイテム番号
            int index = (itemPointY * horizonItems) + itemPointX;

            //指定ポイントにアイテムがあるか
            return itemPointX > horizonItems - 1 || index > m_ImgSet.Count - 1 ? -1 : index;
        }
    }
}
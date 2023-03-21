using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
SideBarクラス

半透明の画像一覧を左端に表示するバー/パネル。
画像自体はPackageInfoそのもののポインタをもらって描写している。
タイマー遅延によって閉じるタイミングを指定可能。
*/

namespace Marmi
{
    public class SideBar : UserControl
    {
        private const int GRIP_WIDTH = 8;        //グリップ全体の幅
        private const int GRIP_HEIGHT = 32;      //グリップ描写部分の高さ
        private const int THUMBSIZE = 120;       //サムネイルサイズ
        private const int PADDING = 2;           //各種の余白
        private const int NUM_WIDTH = 32;        //番号部分の幅
        private readonly int BOX_HEIGHT;            //BOXサイズ：コンストラクタで計算
        private int m_hoverItem;                    //選択されているアイテム
        private Point m_mouseDragPoint;             //グリップをドラッグされたときのPoint
        private PackageInfo m_packageInfo;          //g_piそのものを挿す
        private int m_drawScrollValue;              //スクロール位置。≒m_vsBar.Valueだが慣性を考慮。Draw()参照
        private readonly Timer m_scrollTimer;       //スクロールに慣性をつけるためのタイマー
        private readonly Color m_NormalBackColor = Color.Black;
        private readonly Brush m_brNormalBack = Brushes.Black;
        private readonly SolidBrush m_brSelectBack = new SolidBrush(Color.FromArgb(224, Color.RoyalBlue));
        private readonly SolidBrush m_brHoverBack = new SolidBrush(Color.FromArgb(128, Color.RoyalBlue));
        private readonly VScrollBar m_vsBar = new VScrollBar();          //スクロールバー
        private readonly ToolTip m_tooltip = null;   //ツールチップ

        private readonly Font FONTL = App.Font10;
        private readonly Font FONTS = App.Font9;
        private readonly int FONTL_HEIGHT = App.Font10_Height;
        private readonly int FONTS_HEIGHT = App.Font9_Height;

        //高速描写/HQ描写判定用フラグ
        private bool fastDraw = false;

        // サイズ変更通知イベント用delegate
        public event EventHandler SidebarSizeChanged;

        // 初期化 ***********************************************************************/

        public SideBar()
        {
            //クラスメンバー変数の設定
            BOX_HEIGHT = THUMBSIZE + (PADDING * 2);
            m_hoverItem = -1;
            m_mouseDragPoint = Point.Empty;

            //このコントロールの設定
            this.BackColor = Color.Transparent;         //背景色は透明に
            this.MinimumSize = new Size(GRIP_WIDTH, 1); //最小幅を設定
            this.Width = 200;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer,
                true);

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);

            //縦スクロールバーの設定
            //m_vsBar = new VScrollBar();
            m_vsBar.Visible = false;
            m_vsBar.ValueChanged += new EventHandler(VsBar_ValueChanged);
            m_vsBar.Value = 0;
            m_drawScrollValue = 0;
            this.Controls.Add(m_vsBar);

            //スクロールタイマーの設定
            m_scrollTimer = new Timer { Interval = 10 };
            m_scrollTimer.Tick += new EventHandler(ScrollTimer_Tick);

            //ツールチップの設定
            m_tooltip = new ToolTip();

            //幅を見ているのでDPIスケーリングは無効にする
            //ただし、機能していない模様なのでコメントアウトしている
            //this.AutoScaleMode = AutoScaleMode.None;
            //this.AutoScaleDimensions = new SizeF(0.0F, 0.0F);
        }

        /// <summary>PackageInfoを登録する。
        /// </summary>
        /// <param name="pi">登録するPackageInfo</param>
        public void Init(PackageInfo pi)
        {
            m_packageInfo = pi;
            //m_packageInfo = null;
            m_vsBar.Visible = false;
            m_vsBar.Value = 0;

            SetScrollbar();
        }

        /// <summary>
        /// 指定したアイテムを中心位置にする
        /// </summary>
        /// <param name="item">中心にしたいアイテム番号</param>
        public void SetItemToCenter(int item)
        {
            if (!this.Visible)
                return;
            if (m_packageInfo == null)
                return;

            //ver1.30 2012年2月19日 itemの範囲チェック
            if (item < 0 || item > m_packageInfo.Items.Count - 1)
                return;

            if (m_packageInfo != null)
            {
                //スクロールバーを表示アイテムが見える位置にする
                //出来れば真ん中当たりにする
                int val = (item * BOX_HEIGHT) - (this.Height - BOX_HEIGHT) / 2;
                if (val < 0)
                    m_vsBar.Value = 0;
                else if (val > m_vsBar.Maximum - m_vsBar.LargeChange)
                    m_vsBar.Value = m_vsBar.Maximum - m_vsBar.LargeChange;
                else
                    m_vsBar.Value = val;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(m_NormalBackColor);

            if (m_packageInfo != null)
                //表示位置はm_vs.Valueではなく慣性力をつけるため
                //m_drawScrollValueにする。
                DrawPanel(e.Graphics, m_drawScrollValue);

            //ver1.30 2012/02/19 グリップは最後に描写
            DrawGrip(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!m_mouseDragPoint.IsEmpty)
            {
                fastDraw = true;
                //ドラッグ中なのでなにもしない
                //m_vsBar.Visible = false;
            }
            else
            {
                SetScrollbar();
                fastDraw = false;
            }
            this.Refresh();
        }

        protected override async void OnClick(EventArgs e)
        {
            base.OnClick(e);

            //ノーマルサイズではクリック位置へ移動
            int item = MousePointToItemNumber();
            if (m_packageInfo == null || item < 0 || item >= m_packageInfo.Items.Count)
                return;

            //((Form1)Parent).SetViewPage(item);
            await ((Form1)Form1._instance).SetViewPageAsync(item);

            //アイテムを中央に持ってくる
            SetItemToCenter(item);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (this.Visible && m_vsBar.Visible)
            {
                int delta = BOX_HEIGHT;
                if (e.Delta < 0)
                    m_vsBar.Value = (m_vsBar.Value + delta <= m_vsBar.Maximum - m_vsBar.LargeChange)
                        ? m_vsBar.Value + delta
                        : m_vsBar.Maximum - m_vsBar.LargeChange;
                else
                    m_vsBar.Value = (m_vsBar.Value - delta > 0) ? m_vsBar.Value - delta : 0;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left
                && e.X >= this.Width - GRIP_WIDTH)
            {
                m_mouseDragPoint = this.PointToClient(MousePosition);
                m_vsBar.Visible = false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            //ドラッグ操作かどうかチェック
            if (e.Button == MouseButtons.Left
                && !m_mouseDragPoint.IsEmpty)
            {
                m_mouseDragPoint = Point.Empty;
                m_vsBar.Visible = true;
                SetScrollbar();
                fastDraw = false;
                this.Refresh();

                //ver1.31 nullcheck
                if (SidebarSizeChanged != null)
                    this.SidebarSizeChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //カーソルの設定
            if (e.X > this.Width - GRIP_WIDTH)
                Cursor.Current = Cursors.VSplit;
            else
                Cursor.Current = Cursors.Default;

            //ドラッグ操作かどうかチェック
            if (e.Button == MouseButtons.Left
                && !m_mouseDragPoint.IsEmpty)
            {
                Point pt = this.PointToClient(MousePosition);

                int dx = pt.X - m_mouseDragPoint.X;
                this.Width += dx;
                App.Config.SidebarWidth = this.Width;

                m_mouseDragPoint = pt;

                //PicPanelとの位置関係を調整
                ((Form1)Form1._instance).AjustSidebarArrangement();
            }

            //アイテムがなければ何もしない
            if (m_packageInfo == null)
                return;

            //マウスホバーが変わっていれば再描写する。
            int item = MousePointToItemNumber();
            if (item < 0)
                return;
            else if (m_hoverItem != item)
            {
                m_hoverItem = item;
                this.Invalidate();

                //ToolTipを表示する
                //ToolTip用の文字を設定
                string sz = $"{m_packageInfo.Items[item].Filename}\n 日付: {m_packageInfo.Items[item].CreateDate:yyyy年M月d日 H:m:s}\n 大きさ: {m_packageInfo.Items[item].FileLength:N0}bytes\n サイズ: {m_packageInfo.Items[item].Width:N0}x{m_packageInfo.Items[item].Height:N0}ピクセル";

                //ToolTipの位置を設定 ver0.9833
                int dispY = item * BOX_HEIGHT - m_vsBar.Value;
                if (dispY < 0)
                    dispY = 0;

                //ToolTip表示 ver0.9833
                //m_tooltip.Show(sz, this, this.Width, e.Y, 3000);
                m_tooltip.Show(sz, this, this.Width, dispY, 3000);
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            //フォーカスを当てる
            this.Focus();
            this.Select();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            //ver1.30 2012年2月24日
            //マウスが外に出たらホバーを解除
            base.OnMouseLeave(e);
            m_hoverItem = -1;
            this.Invalidate();
        }

        private int MousePointToItemNumber()
        {
            Point pt = PointToClient(MousePosition);

            //サイズ変更グリップ上ではクリックイベントは無視
            if (pt.X > this.Width - GRIP_WIDTH)
                return -1;

            //アイテムがないときは無視 2010/06/04
            if (m_packageInfo == null)
                return -1;

            //クリックしたアイテムを表示
            int y = pt.Y;
            if (m_vsBar.Visible)    //スクロールバー分を補正
                y += m_vsBar.Value;

            int index = y / BOX_HEIGHT;
            if (index < 0 || index >= m_packageInfo.Items.Count)
                return -1;
            else
                return index;
        }

        // スクロールバー関連 ***********************************************************/

        private void VsBar_ValueChanged(object sender, EventArgs e)
        {
            //ver 0.985 2010/06/03 描写タイマーを起動
            if (!m_scrollTimer.Enabled)
                m_scrollTimer.Start();
        }

        private void SetScrollbar()
        {
            if (m_packageInfo != null)
            {
                if (m_packageInfo.Items.Count * BOX_HEIGHT > this.Height)
                {
                    m_vsBar.Top = 0;
                    m_vsBar.Left = this.Width - GRIP_WIDTH - m_vsBar.Width;
                    m_vsBar.Height = this.Height;
                    //m_vsBar.Value = 0;
                    m_vsBar.Minimum = 0;
                    m_vsBar.Maximum = m_packageInfo.Items.Count * BOX_HEIGHT;
                    m_vsBar.LargeChange = this.Height;
                    m_vsBar.SmallChange = this.Height / 10;
                    m_vsBar.Show();
                }
                else
                    m_vsBar.Visible = false;
            }
            else
            {
                m_vsBar.Visible = false;
            }
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            int algorithm = 1;  //描写アルゴリズム。下のswitch分で利用
            int mag = 10;       //慣性力。algorithm = 1で利用
            int diff = m_vsBar.Value - m_drawScrollValue;   //最終描写位置との差分

            //ver1.70 スムーススクロールをOffにする
            //0: アニメーション無しで表示。1:スムーススクロール
            if (!App.Config.General.SmoothScrollOnSidebar)
                algorithm = 0;

            //残りが1以下なら移動させてタイマーストップ
            if (Math.Abs(diff) <= 1)
            {
                m_scrollTimer.Stop();
                m_drawScrollValue = m_vsBar.Value;
                fastDraw = false;
                this.Refresh();
                return;
            }

            //スムースに動かす
            fastDraw = true;
            switch (algorithm)
            {
                case 0:
                    //標準アルゴリズム。何もしない
                    m_scrollTimer.Stop();
                    m_drawScrollValue = m_vsBar.Value;
                    this.Refresh();
                    break;

                case 1:
                    //割り算で対応。簡単な慣性力を実装
                    int adddiff = diff / mag;
                    if (adddiff == 0)
                        adddiff = Math.Sign(diff);  //1，-1符号だけ

                    //スクロール速度を一定にする
                    //if (Math.Abs(adddiff) > m_vsBar.SmallChange*2)
                    //    adddiff = Math.Sign(diff) * m_vsBar.SmallChange*2;

                    //描写
                    m_drawScrollValue += adddiff;
                    //Draw(this.CreateGraphics(), m_drawScrollValue);
                    this.Refresh();
                    break;
            }
        }

        // オーナードロー ***************************************************************/

        /// <summary>
        /// オーナードロー本体
        /// 表示対象のアイテムを全て描写する。
        /// 半透明には未対応のため呼び出し側NaviBar_Paint()での対応が必須。
        /// </summary>
        /// <param name="g">書き出し先Graphics</param>
        /// <param name="top">描写するもっとも上の位置。≒m_vsBar.Value</param>
        private void DrawPanel(Graphics g, int top)
        {
            //グリップしか表示されていない場合やアイテムが無い場合は
            //描写しない。
            if (this.Width <= GRIP_WIDTH            //グリップしか表示されていない
                || m_packageInfo == null            //アイテム生成されていない
                || m_packageInfo.Items.Count < 1    //アイテムが1つも登録されていない
                )
            {
                //グリップ枠を描写する
                //DrawGrip(g);
                return;
            }

            //アイテム描写のための変数定義
            int scbarWidth = (m_vsBar.Visible) ? m_vsBar.Width : 0; //縦スクロールバーの幅
            int ItemCount = m_packageInfo.Items.Count;              //総アイテム数
            int startItem = top / BOX_HEIGHT;                       //一番上のアイテムインデックス
            if (startItem < 0)
                startItem = 0;
            int endItem = (top + this.Height) / BOX_HEIGHT + 1;     //一番下のアイテムインデックス
            if (endItem > ItemCount)
                endItem = ItemCount;

            //画像の描写　ver1.37DrawItemから移動
            if (fastDraw)
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            else
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //アイテムを表示数分廻すルーチン
            for (int index = startItem; index <= endItem; index++)
            {
                var rc = new Rectangle(
                    0,                                      // 描写開始位置x = 0;
                    index * BOX_HEIGHT - top,               // 描写開始位置y = ボックスの絶対高さ-スクロール値
                    this.Width - GRIP_WIDTH - scbarWidth,   // 幅はグリップとスクロールバーの幅を除く
                    (THUMBSIZE + PADDING * 2)               // 高さはTHUMBSIZEに
                    );

#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                //並列に描写されることを想定し、awaitしない
                DrawItemAsync(index, g, rc);
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
            }

            //背景が透明なのでアイテムが少ないと透明のままになる
            //その部分を背景色で塗る
            int endY = (endItem) * BOX_HEIGHT - top;
            if (endY < this.Height)
            {
                g.FillRectangle(m_brNormalBack, 0, endY, this.Width - GRIP_WIDTH, this.Height - endY);
            }
        }

        /// <summary>
        /// 対象とするアイテムを指定位置に描写する
        /// </summary>
        /// <param name="index">対象アイテム番号</param>
        /// <param name="g">書き出し先Graphics</param>
        /// <param name="rect">描写する位置</param>
        private async Task DrawItemAsync(int index, Graphics g, Rectangle rect)
        {
            if (m_packageInfo == null)
                return;

            //インデックス範囲チェック
            if (index < 0 || index >= m_packageInfo.Items.Count)
                return;

            //背景色を自前で描写.普通は塗らなくてもいい
            if (index == m_hoverItem)
            {
                g.FillRectangle(m_brHoverBack, rect);
            }
            else if (index == m_packageInfo.NowViewPage)
            {
                g.FillRectangle(m_brSelectBack, rect);
            }

            //文字の描写:画像通し番号
            int x = rect.X + 2;
            int y = rect.Y + 20;
            string sz = $"{index + 1}";

            g.DrawString(sz, FONTS, Brushes.DarkGray, x, y);

            //今回描写対象のアイテム
            var imgInfo = m_packageInfo.Items[index];

            x = rect.X + PADDING + NUM_WIDTH;
            y = rect.Y + PADDING;
            if (imgInfo.Thumbnail != null)
            {
                int tWidth = imgInfo.Thumbnail.Width;
                int tHeight = imgInfo.Thumbnail.Height;

                //拡大縮小処理
                var ratio = 1.0F;
                if (tWidth > tHeight && tWidth > THUMBSIZE)
                {
                    ratio = (float)THUMBSIZE / tWidth;
                }
                else if (tHeight > THUMBSIZE)
                {
                    ratio = (float)THUMBSIZE / tHeight;
                }

                var drawImageRect = new RectangleF(
                    x + (THUMBSIZE - tWidth * ratio) / 2,   // 始点X
                    y + (THUMBSIZE - tHeight * ratio) / 2,  // 始点Y位置
                    tWidth * ratio,                         // 画像幅
                    tHeight * ratio                         // 画像高
                    );

                //サムネイル画像の描写
                g.DrawImage(
                    imgInfo.Thumbnail,
                    Rectangle.Round(drawImageRect));

                //枠の描写
                g.DrawRectangle(
                    Pens.LightGray,
                    Rectangle.Round(drawImageRect));
            }
            else
            {
                //画像を持っていないので枠だけ描写
                var drawImageRect = new RectangleF(x, y, THUMBSIZE, THUMBSIZE);
                g.DrawRectangle(
                    Pens.LightGray,
                    Rectangle.Round(drawImageRect));

                //ver1.81 画像を取りに行く
                //その後サムネイル登録.タイマーが止まってから実行
                if (m_scrollTimer == null || !m_scrollTimer.Enabled)
                {
                    await Bmp.LoadBitmapToCacheAsync(index, false);
                    if (this.Visible)
                        this.Invalidate();
                }
            }

            //文字の描写:ファイル名
            Rectangle strRect = rect;
            strRect.X = x + PADDING + NUM_WIDTH + THUMBSIZE;
            strRect.Width = rect.Width - strRect.Left;
            strRect.Y = y;
            strRect.Height = FONTL_HEIGHT;
            sz = $"{Path.GetFileName(imgInfo.Filename)}";
            g.DrawString(sz, FONTL, Brushes.White, strRect);
            strRect.Y += FONTL_HEIGHT + PADDING;

            //文字の描写:サイズ, 日付
            //x += 10;
            strRect.X += PADDING;
            strRect.Width = rect.Width - strRect.Left;
            strRect.Height = FONTS_HEIGHT;
            sz = $"{imgInfo.FileLength:N0}bytes,   {imgInfo.CreateDate}";
            g.DrawString(sz, FONTS, Brushes.LightGray, strRect);
            strRect.Y += FONTS_HEIGHT + PADDING;

            //文字の描写:ピクセル数
            sz = $"{imgInfo.Width:N0}x{imgInfo.Height:N0}pixels";
            g.DrawString(sz, FONTS, Brushes.RoyalBlue, strRect);
        }

        /// <summary>
        /// グリップ部分の描写
        /// 描写位置は自身の右端にGRIP_WIDTHで描写する
        /// </summary>
        /// <param name="g">書き出し先Graphics</param>
        private void DrawGrip(Graphics g)
        {
            Rectangle r = new Rectangle(this.Width - GRIP_WIDTH, 0, GRIP_WIDTH, this.Height);

            g.FillRectangle(SystemBrushes.Control, r);
            g.DrawLine(SystemPens.ControlDark, r.Left, r.Top, r.Left, r.Bottom);
            g.DrawLine(SystemPens.ControlDark, r.Right, r.Top, r.Right, r.Bottom);

            //持ち手を描写
            int sx = this.Width - GRIP_WIDTH + 2;
            int sy = (this.Height - GRIP_HEIGHT) / 2;
            g.DrawLine(SystemPens.ControlLightLight, sx, sy, sx, sy + GRIP_HEIGHT);
            sx++;
            g.DrawLine(SystemPens.ControlDark, sx, sy, sx, sy + GRIP_HEIGHT);
            sx += 2;
            g.DrawLine(SystemPens.ControlLightLight, sx, sy, sx, sy + GRIP_HEIGHT);
            sx++;
            g.DrawLine(SystemPens.ControlDark, sx, sy, sx, sy + GRIP_HEIGHT);
        }
    }
}
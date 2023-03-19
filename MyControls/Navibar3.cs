/********************************************************************************
NaviBar3

トラックバーと連動してサムネイル表示するパネル。
命名を変えたほうがいいかもしれない
タイマーを使ってアニメーションさせながら描写している。
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormTimer = System.Windows.Forms.Timer;

namespace Marmi
{
    public class NaviBar3 : UserControl
    {
        private const int THUMBSIZE = 200;  //サムネイルサイズ
        private const int PADDING = 2;      //各種余白
        private readonly int BOX_HEIGHT;    //BOXサイズ：コンストラクタで計算
        public int _selectedItem;           //選択されているアイテム
        private int _offset;                //現在の描写位置.ピクセル数

        private readonly PackageInfo _packageInfo;        //g_piそのものを挿す

        //背景色
        private readonly SolidBrush _BackBrush = new SolidBrush(Color.FromArgb(192, 48, 48, 48));

        //フォント,フォーマット
        private readonly Font fontL = new Font("Century Gothic", 16F);

        private readonly StringFormat sfCenterDown = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        private List<ItemPos> _posList = null; //サムネイルの位置

        //スクロール用アニメーションタイマー:
        private readonly FormTimer _timer = null;

        //暗めの画像用のImageAttribute.DrawItem()で利用
        private ImageAttributes _darkAttribute = new ImageAttributes();

        //選択画像を強調するRectのペン
        private readonly Pen _borderPen = new Pen(Color.Pink, 2);

        // 初期化 ***********************************************************************/

        public NaviBar3(PackageInfo pi)
        {
            _packageInfo = pi;
            _selectedItem = -1;

            //背景色
            this.BackColor = Color.Transparent;

            //ダブルバッファを有効に
            this.DoubleBuffered = true;

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);

            //DPIスケーリングは無効にする
            this.AutoScaleMode = AutoScaleMode.None;

            //高さを算出
            BOX_HEIGHT = PADDING + THUMBSIZE;

            //newされたあとに高さを必要とされるので高さだけ入れておく。
            this.Height = BOX_HEIGHT + PADDING;

            //Loadingと表示するイメージ
            //_dummyImage = BitmapUty.LoadingImage(THUMBSIZE * 2 / 3, THUMBSIZE);

            //タイマーの初期設定
            _timer = new FormTimer
            {
                Interval = 20
            };
            _timer.Tick += new EventHandler(Timer_Tick);

            //オフセットを設定
            _offset = 0;

            //明るさ半分のImageAttributeを初期化
            var cm = new ColorMatrix
            {
                Matrix00 = 0.5f,    // R
                Matrix11 = 0.5f,    // G
                Matrix22 = 0.5f,    // B
                Matrix33 = 1f,  // Alpha
                Matrix44 = 1f
            };
            _darkAttribute.SetColorMatrix(cm);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //アニメーションさせながら近づける。
            int diff = (GetOffset(_selectedItem) - _offset);
            diff = diff * 2 / 7;
            _offset += diff;

            if (diff == 0)
            {
                _timer.Stop();
                CalcAllItemPos();
            }
            //描写
            this.Refresh();
        }

        // publicメソッド/プロパティ ****************************************************/

        public void OpenPanel(Rectangle rect, int index)
        {
            this.Top = rect.Top;
            this.Left = rect.Left;
            this.Width = rect.Width;
            this.Height = BOX_HEIGHT + PADDING;

            _selectedItem = index;

            //アイテム位置を計算
            CalcAllItemPos();

            //初期位置を決定
            _offset = GetOffset(index);

            this.Visible = true;
            this.Refresh();

            //timerを止める
            _timer?.Stop();
        }

        public void ClosePanel()
        {
            //サムネイル作成中なら一度止める
            //Form1.PauseThumbnailMakerThread();

            this.Visible = false;

            //timerを止める
            _timer?.Stop();

            //BitmapCacheを削除
            //App.BmpCache.Clear(CacheTag.NaviPanel);
        }

        public void SetCenterItem(int index)
        {
            _selectedItem = index;

            //ver1.37バグ対処:非表示ではなにもしない
            if (!Visible)
                return;

            if (_timer != null)
            {
                //タイマーで描写
                _timer.Enabled = true;
            }
            else
            {
                //タイマーを使わずに再描写
                _offset = GetOffset(index);
                this.Refresh();
            }
        }

        // オーナードロー ***************************************************************/

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            DrawItemAll(g);
        }

        /// <summary>
        /// ver1.36
        /// 新バージョンのアイテム描写ルーチン
        /// </summary>
        /// <param name="g"></param>
        private void DrawItemAll(Graphics g)
        {
            //背景色として黒で塗りつぶす
            g.FillRectangle(_BackBrush, this.DisplayRectangle);

            //表示すべきアイテムがない場合は背景だけ
            if (_packageInfo == null || _packageInfo.Items.Count < 1)
                return;

            //すべてのアイテムの位置を更新
            //CalcAllItemPos();

            //オフセットを計算
            //タイマーが動いていないときはGetOffset()ですぐその場所へ
            //タイマー動作中は_offset
            int offset = (_timer == null) ? GetOffset(_selectedItem) : _offset;

            //全アイテム描写
            for (int item = 0; item < _packageInfo.Items.Count; item++)
            {
                //わざと並列で描写する
#pragma warning disable CS4014
                DrawItemAsync(g, item, offset);
#pragma warning restore CS4014
            }
            return;
        }

        /// <summary>
        /// サムネイルの表示位置をすべて更新
        /// </summary>
        private void CalcAllItemPos()
        {
            if (_posList == null)
            {
                //新規に位置リストを作成
                _posList = new List<ItemPos>();
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    ItemPos item = new ItemPos();
                    item.pos.X = X;
                    item.pos.Y = PADDING;
                    if (_packageInfo.Items[i].Thumbnail != null)
                    {
                        item.size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                        item.Fixed = true;
                    }
                    else
                    {
                        item.size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    }

                    _posList.Add(item);
                    X += item.size.Width + PADDING;
                }
            }
            else
            {
                //すでにあるものを更新
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    _posList[i].pos.X = X;
                    _posList[i].pos.Y = PADDING;

                    if (_posList[i].Fixed == false)
                    {
                        if (_packageInfo.Items[i].Thumbnail != null)
                        {
                            _posList[i].size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                            _posList[i].Fixed = true;
                        }
                        else
                            _posList[i].size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    }
                    X += _posList[i].size.Width + PADDING;
                }
            }
        }

        /// <summary>
        /// 中央のアイテム番号からオフセットすべきピクセル数を計算
        /// </summary>
        /// <param name="centerItem">中央のアイテム番号</param>
        /// <returns></returns>
        private int GetOffset(int centerItem)
        {
            int X = _posList[centerItem].pos.X;
            int center = X + _posList[centerItem].size.Width / 2;
            int offset = center - this.Width / 2;
            return offset;
        }

        private bool IsDrawItem(int index, int offset)
        {
            if (_posList == null)
                return false;

            var item = _posList[index];
            int x = item.pos.X - offset;
            return (x > this.Width || x + item.size.Width < 0);
        }

        /// <summary>
        /// 1アイテムを描写する
        /// </summary>
        /// <param name="g">描写先のGraphics</param>
        /// <param name="index">描写アイテム番号</param>
        private async Task DrawItemAsync(Graphics g, int index, int offset)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            App.g_pi.ThrowIfOutOfRange(index);

            //未計算だったら先に全アイテム位置を計算
            //呼び出し元で必ず実行しているのでコメントアウト
            //if (_thumbnailPosList == null)
            //    CalcAllItemPos();

            //対象データ
            var item = _posList[index];

            //描写X位置
            int x = item.X - offset;

            //描写対象外をはじく
            if (x > this.Width || x + item.Width < 0)
                return;

            var cRect = new Rectangle(
                item.X - offset,
                item.Y + BOX_HEIGHT - item.Height - PADDING,      //下揃え
                item.Width,
                item.Height);

            //画像取得
            var img = _packageInfo.Items[index].Thumbnail;

            if (img == null)
            {
                //読み込む
                //_ = await Bmp.LoadBitmapAsync(index, false);
                //CalcAllItemPos();

                ////ver1.81 読み込みルーチンをPushLow()に変更
                //if (_timer == null || !_timer.Enabled)
                //{
                //    await Bmp.LoadBitmapAsync(index, false);
                //    CalcAllItemPos();
                //    if (this.Visible)
                //        this.Invalidate();
                //    return;
                //}
            }
            else
            {
                //描写
                if (index == _selectedItem)
                {
                    //中央のアイテム
                    g.DrawImage(img, cRect);
                }
                else
                {
                    //中央以外の画像：暗めに描写
                    g.DrawImage(img, cRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, _darkAttribute);
                }
            }

            //枠描写
            var penColor = (index == _selectedItem) ? _borderPen : Pens.Gray;
            g.DrawRectangle(penColor, cRect);

            //画像番号を画像上に表示
            g.DrawString($"{index + 1}", fontL, Brushes.LightGray, cRect, sfCenterDown);
        }
    }

    /// <summary>
    /// 描写位置を保存するための構造体（クラス）
    /// </summary>
    public class ItemPos
    {
        public Point pos;
        public Size size;
        public bool Fixed = false;
        public int X => pos.X;
        public int Y => pos.Y;
        public int Width => size.Width;
        public int Height => size.Height;
    }
}
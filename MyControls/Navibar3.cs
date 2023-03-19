/********************************************************************************
NaviBar3

トラックバーと連動してサムネイル表示するパネル。
命名を変えたほうがいいかもしれない
タイマーを使ってアニメーションさせながら描写している。
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormTimer = System.Windows.Forms.Timer;

namespace Marmi
{
    public class NaviBar3 : UserControl
    {
        private const int THUMBSIZE = 200;  //サムネイルサイズ
        private const int PADDING = 2;      //各種余白
        private const int DARKPERCENT = 50; //左右の画像の暗さ％
        private readonly int BOX_HEIGHT;    //BOXサイズ：コンストラクタで計算
        public int _selectedItem;           //選択されているアイテム
        private float _alpha = 1.0F;        //描写時の透明度。OnPaint()で利用
        private int _offset;                //現在の描写位置.ピクセル数

        private readonly PackageInfo _packageInfo;        //g_piそのものを挿す
        private readonly SolidBrush _BackBrush = new SolidBrush(Color.FromArgb(192, 48, 48, 48));        //背景色

        //フォント,フォーマット
        private readonly Font fontL = new Font("Century Gothic", 16F);

        private readonly StringFormat sfCenterDown = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        private List<ItemPos> _thumbnailPosList = null; //サムネイルの位置
        private readonly Bitmap _dummyImage = null;     // Loadingイメージ
        private readonly FormTimer _timer = null;       //アニメーションタイマー

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
            //透明度は1.0
            _alpha = 1.0F;

            //高さを算出
            BOX_HEIGHT = PADDING + THUMBSIZE;
            //newされたあとに高さを必要とされるので高さだけ入れておく。
            this.Height = BOX_HEIGHT        //画像部分
                + PADDING;

            //Loadingと表示するイメージ
            _dummyImage = BitmapUty.LoadingImage(THUMBSIZE * 2 / 3, THUMBSIZE);

            //タイマーの初期設定
            _timer = new FormTimer
            {
                Interval = 20
            };
            _timer.Tick += new EventHandler(Timer_Tick);

            //オフセットを設定
            _offset = 0;
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

            #region 半透明描写しながらスライド

            //alpha = 0.0F;
            //this.Visible = true;
            //for (int i = 1; i <= 5; i++)
            //{
            //    alpha = i * 0.2F;					//透明度を設定
            //    //this.Top = rect.Top + i - 10;		//スライドインさせる

            //    this.Refresh();
            //    Application.DoEvents();
            //}

            #endregion 半透明描写しながらスライド

            //アイテム位置を計算
            CalcAllItemPos();

            //初期位置を決定
            _offset = GetOffset(index);

            this.Visible = true;
            _alpha = 1.0F;
            this.Refresh();

            //timerを止める
            _timer?.Stop();
        }

        public void ClosePanel()
        {
            //サムネイル作成中なら一度止める
            //Form1.PauseThumbnailMakerThread();

            #region 半透明描写しながらfede out

            //for (int i = 1; i <= 5; i++)
            //{
            //    alpha = 1 - i * 0.2F;		//透明度を設定
            //    //this.Top--;				//スライドアウトさせる

            //    this.Refresh();
            //    Application.DoEvents();		//これがないとフェードアウトしない
            //}

            #endregion 半透明描写しながらfede out

            this.Visible = false;
            _alpha = 1.0F;

            //timerを止める
            _timer?.Stop();

            //BitmapCacheを削除
            App.BmpCache.Clear(CacheTag.NaviPanel);
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
            //g.Clear(m_NormalBackColor);

            if (_alpha >= 1.0F)
            {
                //DrawItems(g);
                DrawItemAll(g);
            }
            else
            {
                //透明度がある場合は一度別のBitmapに描写して描写する。
                using (var bmp = new Bitmap(this.Width, this.Height))
                {
                    Graphics.FromImage(bmp).Clear(Color.Transparent);
                    DrawItemAll(Graphics.FromImage(bmp));
                    BitmapUty.AlphaDrawImage(g, bmp, _alpha);
                }
            }
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
            int offset;
            if (_timer == null)
            {
                //タイマーが動いていないときはすぐその場所へ
                offset = GetOffset(_selectedItem);
            }
            else
            {
                //タイマーが動いているときはoffsetはTimerが更新
                offset = _offset;
            }

            //全アイテム描写
            for (int item = 0; item < _packageInfo.Items.Count; item++)
            {
                //わざと並列で描写する
                // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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
            if (_thumbnailPosList == null)
            {
                //新規に位置リストを作成
                _thumbnailPosList = new List<ItemPos>();
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    ItemPos item = new ItemPos();
                    item.pos.X = X;
                    item.pos.Y = PADDING;
                    if (_packageInfo.Items[i].Thumbnail != null)
                    {
                        item.size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                    }
                    else
                        item.size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    _thumbnailPosList.Add(item);
                    X += item.size.Width + PADDING;
                }
            }
            else
            {
                //すでにあるものを更新
                int X = 0;
                for (int i = 0; i < _packageInfo.Items.Count; i++)
                {
                    //ItemPos item = thumbnailPos[i];
                    _thumbnailPosList[i].pos.X = X;
                    _thumbnailPosList[i].pos.Y = PADDING;
                    if (_packageInfo.Items[i].Thumbnail != null)
                        _thumbnailPosList[i].size = BitmapUty.CalcHeightFixImageSize(_packageInfo.Items[i].Thumbnail.Size, THUMBSIZE);
                    else
                        _thumbnailPosList[i].size = new Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    X += _thumbnailPosList[i].size.Width + PADDING;
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
            int X = _thumbnailPosList[centerItem].pos.X;
            int center = X + _thumbnailPosList[centerItem].size.Width / 2;
            int offset = center - this.Width / 2;
            return offset;
        }

        /// <summary>
        /// 1アイテムを描写する
        /// </summary>
        /// <param name="g">描写先のGraphics</param>
        /// <param name="index">描写アイテム番号</param>
        private async Task DrawItemAsync(Graphics g, int index, int offset)
        {
            if (g == null)
                throw new ArgumentNullException(nameof(g));
            App.g_pi.ThrowIfOutOfRange(index);

            //未計算だったら計算
            if (_thumbnailPosList == null)
                CalcAllItemPos();

            //描写位置を決定
            int x = _thumbnailPosList[index].pos.X - offset;

            //描写対象外をはじく
            if (x > this.Width)
                return;
            if (x + _thumbnailPosList[index].size.Width < 0)
                return;

            //画像取得
            Bitmap img = App.BmpCache.GetBitmap(index, CacheTag.NaviPanel);
            if (img == null)
            {
                img = BitmapUty.MakeHeightFixThumbnailImage(
                    _packageInfo.Items[index].Thumbnail,
                    THUMBSIZE);
                App.BmpCache.Add(index, CacheTag.NaviPanel, img);
            }

            if (img == null)
            {
                img = _dummyImage;

                //ver1.81 読み込みルーチンをPushLow()に変更
                if (_timer == null || !_timer.Enabled)
                {
                    await Bmp.LoadBitmapAsync(index, false);
                    CalcAllItemPos();
                    if (this.Visible)
                        this.Invalidate();
                }
            }

            Rectangle cRect = new Rectangle(
                _thumbnailPosList[index].pos.X - offset,
                _thumbnailPosList[index].pos.Y + BOX_HEIGHT - img.Height - PADDING,      //下揃え
                _thumbnailPosList[index].size.Width,
                _thumbnailPosList[index].size.Height);

            //描写
            if (index == _selectedItem)
            {
                //中央のアイテム
                //ver1.17追加 フォーカス枠
                BitmapUty.DrawBlurEffect(g, cRect, Color.LightBlue);
                //中央を描写
                g.DrawImage(img, cRect);
            }
            else
            {
                //中央以外の画像を描写
                g.DrawImage(
                    BitmapUty.BitmapToDark(img, DARKPERCENT),
                    cRect);
            }

            //画像番号を画像上に表示
            g.DrawString(
                $"{index + 1}",
                fontL,
                Brushes.LightGray,
                cRect,
                sfCenterDown);
        }
    }

    /// <summary>
    /// 描写位置を保存するための構造体（クラス）
    /// </summary>
    public class ItemPos
    {
        public Point pos;
        public Size size;
        public float brightness;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Marmi
{
    public partial class PicturePanel : UserControl
    {
        private bool _mouseDowmFlag = false;
        private Point _mouseDownPos = Point.Empty;      // MouseDownされた位置（ドラッグ中も不変）
        private Point _mousePreDragPos = Point.Empty;   // 前回ドラッグされた位置（ドラッグ中に変化）
        private readonly Matrix _amat = new Matrix();   // アフィン変換用行列
        private readonly ColorMatrix _cmat = new ColorMatrix(); //半透明対応 ColorMatrix
        private Bitmap _bmp = null;
        private bool _isAutoFit = true;
        private readonly Font _font = new Font("メイリオ", 20, FontStyle.Bold);

        #region --- properties ---

        /// <summary>このパネルの表示状態</summary>
        public DrawStatus State { get; set; }

        /// <summary>表示する画像</summary>
        public Bitmap Bmp
        {
            get { return _bmp; }
            set
            {
                _bmp = value;
                if (!App.Config.KeepMagnification)
                    _amat.Reset();
                //alpha = 1.0f;

                //コメントアウト。これがあると一瞬スクロールバーが見える
                //AjustScrollMinSize();
            }
        }

        /// <summary>透明度 0f～1.0f</summary>
        public float Opacity
        {
            get { return _cmat.Matrix33; }
            set
            {
                if (value > 1.0f) _cmat.Matrix33 = 1.0f;
                else if (value < 0f) _cmat.Matrix33 = 0.0f;
                else _cmat.Matrix33 = value;
            }
        }

        /// <summary>拡大率</summary>
        public float ZoomRatio
        {
            get { return _amat.Elements[0]; }
            set { Zoom(value); }
        }

        /// <summary>自動縮尺モードか</summary>
        public bool IsAutoFit
        {
            get { return _isAutoFit; }
            set
            {
                _isAutoFit = value;
                if (_isAutoFit)
                    ZoomRatio = GetScreenFitRatio();
                AjustScrollMinSize();
                AjustViewLocation();
            }
        }

        /// <summary>画面ぴったりの拡大比率</summary>
        public float FittingRatio => GetScreenFitRatio();

        /// <summary>高速描写するかどうか</summary>
        public bool FastDraw { get; set; }

        /// <summary>最後の描写モード</summary>
        public InterpolationMode LastDrawMode { get; private set; }

        //表示するメッセージ
        public string Message { get; set; }

        //描写命令番号
        public long DrawOrderTime { get; set; }

        #endregion --- properties ---

        public PicturePanel()
        {
            InitializeComponent();
            FastDraw = false;

            this.Name = "PicPanel"; //Debugのために名前をつける

            //ver1.30 OnPaintBackground
            this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.SteelBlue;
            this.DoubleBuffered = true;

            //スクロールバーを出すか
            AutoScroll = true;
            SetScrollState(ScrollStateFullDrag, false);

            //スクロールバー
            //this.Controls.Add(hscrollbar);
            //hscrollbar.Dock = DockStyle.Top;
            //this.Controls.Add(vscrollbar);
            //vscrollbar.Dock = DockStyle.Left;

            //DPIスケーリングは無効にする
            this.AutoScaleMode = AutoScaleMode.None;

            //拡大縮小回転用行列
            _amat.Reset();

            //半透明対応 ColorMatrix
            _cmat.Matrix00 = 1f;
            _cmat.Matrix11 = 1f;
            _cmat.Matrix22 = 1f;
            _cmat.Matrix33 = 1f;
            _cmat.Matrix44 = 1f;
            Opacity = 1.0f;

            //プロパティの初期化
            State = DrawStatus.idle;
        }

        /// <summary>
        /// 拡大率、透明度をクリア
        /// </summary>
        public void ResetView()
        {
            //ver1.78 倍率固定に対応
            //mat.Reset();
            if (!App.Config.KeepMagnification)
                _amat.Reset();
            Opacity = 1.0f;
        }

        public void ResizeEnd()
        {
            FastDraw = false;
            AjustViewAndShow();
        }

        public void AjustViewAndShow()
        {
            //画面描写を停止
            BeginUpdate();

            //スクロールバーサイズを算出、設定
            AjustScrollMinSize();

            //描写位置を決定、matに登録
            AjustViewLocation();

            //matからスクロールバー位置を算出
            AjustScrollPosition();

            //描写抑制を解除
            EndUpdate();

            //描写
            this.Refresh();
        }

        //画面の右半分であればtrue
        public bool CheckMousePosRight()
        {
            var pos = this.PointToClient(Cursor.Position);
            return pos.X > this.ClientSize.Width / 2;
        }

        /// <summary>
        /// 画面表示位置を変更
        /// </summary>
        /// <param name="dx">X方向に動かす量</param>
        /// <param name="dy">Y方向に動かす量</param>
        public void AddOffset(int dx, int dy)
        {
            _amat.Translate((float)dx, (float)dy, MatrixOrder.Append);
        }

        #region override

        protected override bool IsInputKey(Keys keyData)
        {
            //ver1.80Shift, Ctrlが入るとコードが変わるので取り除く
            var key = keyData & Keys.KeyCode;

            //switch (keyData)
            switch (key)
            {
                case Keys.Down:
                case Keys.Right:
                case Keys.Up:
                case Keys.Left:
                case Keys.Tab:
                    break;

                default:
                    return base.IsInputKey(keyData);
            }
            return true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);

            e.Graphics.Clear(this.BackColor);
            if (Bmp == null)
                return;

            //ver1.63 後ろに移動
            if (!string.IsNullOrEmpty(Message))
            {
                Debug.WriteLine($"PicPanel Message({Message})");
                DrawTextBottomRight(e.Graphics, Message, _font);
                return;
            }

            ////スクロールバーの調整
            //AjustScrollMinSize();
            //AjustViewLocation();

            //画質の選択
            //ドラッグスクロール ||高速描写フラグ || 整数倍拡大 なら簡易品質
            if (_mouseDowmFlag
                || FastDraw
                || (ZoomRatio > 1.0f && ZoomRatio % 1.0f <= 0.01f && App.Config.View.IsDotByDotZoom)
                )
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            }
            else
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            }
            LastDrawMode = e.Graphics.InterpolationMode;

            //表示
            if (Opacity == 1.0f)
            {
                //アンシャープを適用するかどうか
                if (App.Config.View.UseUnsharpMask
                    && LastDrawMode == InterpolationMode.HighQualityBicubic
                    && ZoomRatio != 100.0f
                    )
                {
                    //アンシャープフィルタをかける
                    Bitmap orgBmp = new Bitmap(Width, this.Height);
                    using (var g = Graphics.FromImage(orgBmp))
                    {
                        g.Clear(this.BackColor);
                        g.Transform = _amat;
                        g.DrawImage(Bmp, 0, 0, Bmp.Width, Bmp.Height);
                    }
                    var unsharpBmp = BitmapUty.Unsharpness_unsafe(orgBmp, App.Config.View.UnsharpDepth);
                    e.Graphics.DrawImage(unsharpBmp, 0, 0, unsharpBmp.Width, unsharpBmp.Height);
                }
                else
                {
                    //通常表示（半透明なし）
                    e.Graphics.Transform = _amat;
                    e.Graphics.DrawImage(Bmp, 0, 0, Bmp.Width, Bmp.Height);
                }
            }
            else
            {
                //半透明描写
                using (var ia = new ImageAttributes())
                {
                    //アルファブレンドしながら描写
                    ia.SetColorMatrix(_cmat);
                    var r = new Rectangle(0, 0, Bmp.Width, Bmp.Height);
                    e.Graphics.DrawImage(
                        Bmp, r, 0, 0, Bmp.Width, Bmp.Height,
                        GraphicsUnit.Pixel, ia);
                }
            }

            SetStatusbarRatio();
        }

        protected override void OnResize(EventArgs e)
        {
            if (Bmp == null)
                return;

            //表示倍率の調整
            if (IsAutoFit)
            {
                float r = GetScreenFitRatio();
                if (r > 1.0f && App.Config.View.NoEnlargeOver100p)
                    r = 1.0f;
                ZoomRatio = r;
            }

            FastDraw = true;

            //中央になるようにする。
            AjustViewLocation();
            this.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            //ドラッグ中ならクリックとみなさない
            if (_mouseDowmFlag && _mouseDownPos != e.Location)
                return;

            base.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);    //ルーペなどの処理

            if (Bmp == null)
                return;

            if (e.Button == MouseButtons.Left)
            {
                //左ボタンが押されている
                _mouseDowmFlag = true;
                _mouseDownPos = _mousePreDragPos = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _mouseDowmFlag = false;
            _mouseDownPos = _mousePreDragPos = Point.Empty;

            //ドラッグスクロールによる低品質描写をした可能性があるので再描写
            //if (fastDraw)
            if (LastDrawMode == InterpolationMode.NearestNeighbor)
            {
                FastDraw = false;
                this.Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            //ver1.80 全画面をダブルクリックで対応するオプション導入
            if (App.Config.DoubleClickToFullscreen)
            {
                base.OnMouseDoubleClick(e);
            }
            else
            {
                //ダブルクリックを1クリックとみなす
                OnMouseClick(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseDowmFlag)
            {
                //ドラッグ中ならアフィン変換で移動させる
                float dx = e.X - _mousePreDragPos.X;
                float dy = e.Y - _mousePreDragPos.Y;

                _amat.Translate(dx, dy, MatrixOrder.Append);

                //上下左右にスクロールしすぎないように補正
                AjustViewLocation();

                //マウス位置保存
                _mousePreDragPos.X = e.X;
                _mousePreDragPos.Y = e.Y;

                //スクロールバー更新
                AjustScrollPosition();
                this.Invalidate();

                if (Cursor.Current != App.Cursors.OpenHand)
                    Cursor.Current = App.Cursors.OpenHand;

                //デバッグ中ならステータスバーも更新
#if DEBUG
                SetStatusbarRatio();
#endif
            }
            else
            {
                //MouseMove()を処理する
                //どこかでイベントハンドルしていればこっちで処理
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            //フォーカスを当てる
            this.Focus();
            this.Select();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseWheel()");
            if (Bmp == null)
                return;

            //ver1.30 Ctrlキーを押しているときは強制的にズーム
            if (App.Config.Mouse.MouseConfigWheel == "拡大縮小"
                || (Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                    ZoomIn();
                else
                    ZoomOut();
            }
            else
            {
                //ズーム以外はイベントを続行
                base.OnMouseWheel(e);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            float dx = 0;
            float dy = 0;

            //移動方法１：差分だけ再設定
            if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll)
                dx = se.NewValue - se.OldValue;
            else
                dy = se.NewValue - se.OldValue;
            _amat.Translate(-dx, -dy, MatrixOrder.Append);

            //移動方法２：軸をゼロクリアして再設定する方法
            //float ratio = mat.Elements[0];
            //mat.Reset();
            //mat.Scale(ratio, ratio, MatrixOrder.Append);
            //mat.Translate(AutoScrollPosition.X, AutoScrollPosition.Y, MatrixOrder.Append);

            Debug.WriteLine($"OnScroll: ScrollPos={AutoScrollPosition}, ScrollSize={dy}, matOffset=({_amat.OffsetX},{_amat.OffsetY})");

            base.OnScroll(se);
            //Invalidate不要
            //this.Invalidate();
        }

        #endregion override

        //-----------------------------------------------------------------
        // privateメソッド
        //

        #region 表示関連の調整

        private void SetStatusbarRatio()
        {
            if (_bmp == null)
                return;

            string text = $"{_bmp.Width:N0} x {_bmp.Height:N0} , 拡大率={ZoomRatio:P1}";
            Form1._instance.SetStatubarRatio(text);
        }

        /// <summary>
        /// 画像を初期位置に設定。中央に表示するようにする。
        /// ・スクロールバーがないときに中央に
        /// ・左右方向にスクロールさせすぎないように
        /// ・上下方向にスクロールさせすぎないように
        /// するルーチン
        /// </summary>
        private void AjustViewLocation()
        {
            if (Bmp == null)
                return;

            //補正値:平行移動させる値
            float dx = 0;
            float dy = 0;

            int bmpwidth = (int)(Bmp.Width * ZoomRatio);
            int bmpheight = (int)(Bmp.Height * ZoomRatio);

            // X方向
            if (ClientRectangle.Width > bmpwidth)
            {
                //スクロールバーなし。画像のほうが小さいので中央に
                float newPointX = (ClientRectangle.Width - bmpwidth) / 2;
                dx = -(_amat.OffsetX - newPointX);
            }
            else if (_amat.OffsetX > 0.0f)//左
            {
                dx = -_amat.OffsetX;  //オフセットを0にする
            }
            else if (_amat.OffsetX < -(bmpwidth - ClientRectangle.Width))//左
            {
                dx = -(bmpwidth - ClientRectangle.Width) - _amat.OffsetX;//右
            }

            // Y方向
            if (ClientRectangle.Height > bmpheight)
            {
                //スクロールバーなし。画像のほうが小さいので中央に
                float newPointY = (ClientRectangle.Height - bmpheight) / 2;
                dy = -(_amat.OffsetY - newPointY);
            }
            else if (_amat.OffsetY > 0.0f)//上
            {
                dy = -_amat.OffsetY;  //オフセットを0にする
            }
            else if (_amat.OffsetY < -(bmpheight - ClientRectangle.Height))
            {
                dy = -(bmpheight - ClientRectangle.Height) - _amat.OffsetY;
            }

            //set
            if (dx == 0 && dy == 0)
                return;
            _amat.Translate(dx, dy, MatrixOrder.Append);
        }

        /// <summary>
        /// スクロールバーをズーム比率に応じて設定
        /// </summary>
        private void AjustScrollMinSize()
        {
            if (Bmp != null)
            {
                this.AutoScrollMinSize = new Size(
                    (int)(Bmp.Width * ZoomRatio),
                    (int)(Bmp.Height * ZoomRatio));
            }
        }

        /// <summary>
        /// スクロールバーの位置がずれていたら直す
        /// </summary>
        private void AjustScrollPosition()
        {
            int matx = (int)_amat.OffsetX;
            int maty = (int)_amat.OffsetY;

            if (AutoScrollPosition.X != matx || AutoScrollPosition.Y != maty)
            {
                this.AutoScrollPosition = new Point(-matx, -maty);
            }
        }

        private float GetScreenFitRatio()
        {
            //ClientRectangleはスクロールバー分が考慮されているので
            //スクロールバーがないBoundsを使う。
            if (Bmp == null)
                return 1.0f;
            else
            {
                try
                {
                    float rx = (float)this.Bounds.Width / (float)Bmp.Width;
                    float ry = (float)this.Bounds.Height / (float)Bmp.Height;

                    //小さいほうを使う
                    return (float)(rx < ry ? rx : ry);
                }
                catch
                {
                    //bmpの中身が無い場合の対処
                    return 1.0f;
                }
            }
        }

        #endregion 表示関連の調整

        #region 拡大縮小関連

        public void ZoomOut()
        {
            //2011/11/22 ちらつきの事前回避
            //効果不明
            BeginUpdate();
            AjustScrollPosition();
            EndUpdate();

            var ratios = GetZoomRatioList();

            // 次の縮小値を探す
            for (int i = ratios.Count - 1; i >= 0; i--)
            {
                if (ratios[i] < ZoomRatio - 0.001f)
                {
                    //Zoom(ratios[i]);
                    AnimateZoom(ratios[i]);

                    _isAutoFit = (ratios[i] == FittingRatio);

                    // スクロールバーを再計算
                    AjustScrollMinSize();

                    //中央表示、左右位置を調整
                    //Zoom()内部でやるのでコメントアウト
                    //AjustViewLocation();

                    //スクロールバー位置を調整
                    //これがあるとZoomがかくつく
                    //this.AutoScrollPosition = new Point(-(int)mat.OffsetX, -(int)mat.OffsetY);

                    BeginUpdate();
                    AjustScrollPosition();
                    EndUpdate();

                    //描写する
                    //this.Invalidate();	//中途半端な描写をなくすため全画面再描写
                    this.Refresh();

                    //ステータスバー
                    SetStatusbarRatio();
                    return;
                }
            }
            //見つからない場合はすでに最小値なのでなにもしない
        }

        public void ZoomIn()
        {
            //2011/11/22 ちらつきの事前回避
            //効果不明
            //TODO いつか整理したい
            BeginUpdate();
            AjustScrollPosition();
            EndUpdate();
            //this.Refresh();	//ここで再描写する必要なし

            List<float> ratios = GetZoomRatioList();

            //次の拡大値を探す
            for (int i = 0; i < ratios.Count; i++)
            {
                if (ratios[i] > ZoomRatio + 0.001f) //誤差を追加
                {
                    //Zoom(ratios[i]);
                    AnimateZoom(ratios[i]);

                    _isAutoFit = (ratios[i] == FittingRatio);

                    // スクロールバーサイズを再計算
                    AjustScrollMinSize();

                    //中央表示、左右位置を調整
                    //Zoom()内部でやるのでコメントアウト
                    //AjustViewLocation();

                    //スクロールバー位置を調整
                    //これがあるとZoomがかくつく
                    //this.AutoScrollPosition = new Point(-(int)mat.OffsetX, -(int)mat.OffsetY);
                    //this.Invalidate();	//中途半端な描写をなくすため全画面再描写
                    BeginUpdate();
                    AjustScrollPosition();
                    EndUpdate();

                    //描写する
                    //this.Invalidate();
                    this.Refresh();

                    //ステータスバー
                    SetStatusbarRatio();
                    return;
                }
            }
            //見つからない場合はすでに最大値なのでなにもしない
        }

        private void AnimateZoom(float newZoomValue)
        {
            int step = 8;
            float diff = (newZoomValue - ZoomRatio) / step;
            float ratio = ZoomRatio;
            FastDraw = true;
            for (int j = 0; j < step; j++)
            {
                ratio += diff;
                Zoom(ratio);

                //中央表示、左右位置を調整
                AjustViewLocation();

                //2011年11月22日
                //スクロールバーの位置調整
                //ここでやっておくといいみたい！
                BeginUpdate();
                AjustScrollPosition();
                EndUpdate();

                this.Refresh();
                //System.Threading.Thread.Sleep(0);
            }
            FastDraw = false;
        }

        /// <summary>
        /// 段階的なズーム比率一覧を算出。
        /// 固定値に加えて、画面サイズフィット倍率も加える。
        /// </summary>
        /// <returns>ズーム比率一覧</returns>
        private List<float> GetZoomRatioList()
        {
            List<float> zoomRatioList = new List<float>{
                        0.1f, 0.2f, 0.25f, 0.5f, 0.75f,
                        1.0f,
                        1.5f, 2.0f, 3.0f, 4.0f, 5.0f, 8.0f, 10.0f, 20.0f };

            if (!zoomRatioList.Contains(FittingRatio))
            {
                zoomRatioList.Add(FittingRatio);
                zoomRatioList.Sort();
            }
            return zoomRatioList;
        }

        /// <summary>
        /// 指定倍率を設定
        /// </summary>
        /// <param name="ratio">指定倍率（絶対値）</param>
        /// <param name="centerX">中心位置</param>
        /// <param name="centerY">中心位置</param>
        private void Zoom2(float ratio, int centerX, int centerY)
        {
            //正確な値でズームする.ただし遅い・・・

            //Zoom=1.0に換算したときの画像中心値を得る
            float cx = -((_amat.OffsetX - centerX) / ZoomRatio);
            float cy = -((_amat.OffsetY - centerY) / ZoomRatio);
            int x = (int)cx;
            int y = (int)cy;

            //リセット
            _amat.Reset();
            //中心に戻して
            _amat.Translate(-x, -y, MatrixOrder.Append);
            //ズーム
            _amat.Scale(ratio, ratio, MatrixOrder.Append);
            // 原点→ポインタの位置へ移動(元の位置へ戻す)
            _amat.Translate(centerX, centerY, MatrixOrder.Append);
        }

        private void Zoom(float zoomRatio)
        {
            Point pt = PointToClient(MousePosition);
            if (ClientRectangle.Contains(pt))
            {
                Zoom2(zoomRatio, pt.X, pt.Y);
            }
            else
            {
                Zoom2(zoomRatio, ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            }
        }

        #endregion 拡大縮小関連

        /// <summary>
        /// 画像を時計回りに回転する。
        /// </summary>
        public void Rotate()
        {
            if (Bmp != null)
            {
                Bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                ZoomRatio = FittingRatio;
                AjustScrollMinSize();
                AjustViewLocation();
                Refresh();
            }
            return;
        }

        //描写支援
        private void DrawTextBottomRight(Graphics g, string s, Font font)
        {
            //const string FontName = "MS PGothic"; //フォント名
            //const string FontName = "メイリオ"; //フォント名
            //const int FontPoint = 24;
            const int MARGIN = 5;

            //using (Graphics g = Graphics.FromHwnd(this.Handle))
            //using (Font font = new Font(FontName, FontPoint))
            {
                //サイズを測る
                SizeF size = g.MeasureString(s, font);
                Rectangle rect = ClientRectangle;

                float x = rect.Right - size.Width - MARGIN;
                float y = rect.Bottom - size.Height - MARGIN;
                x = x > 0 ? x : 0;
                y = y > 0 ? y : 0;
                g.DrawString(s, font, Brushes.White, x, y);
            }//using
        }

        #region P/Invoke

        /// <summary>
        /// EndUpdate メソッドが呼ばれるまで、コントロールを再描画しないようにします。
        /// </summary>
        public void BeginUpdate()
        {
            Win32.SendMessage(this.Handle, Win32.WM_SETREDRAW, Win32.WIN32_FALSE, 0);
        }

        /// <summary>
        /// BeginUpdate メソッドにより中断されていた描画を再開します。
        /// </summary>
        public void EndUpdate()
        {
            Win32.SendMessage(this.Handle, Win32.WM_SETREDRAW, Win32.WIN32_TRUE, 0);
        }

        #endregion P/Invoke

        /// <summary>
        /// 指定した画像をスライドインする
        /// </summary>
        /// <param name="slideBmp">スライドインする画像</param>
        /// <param name="direction">スライド方向、マイナスなら左へ</param>
        public void AnimateSlideIn(Bitmap slideBmp, int direction)
        {
            //スライド方向の決定
            int slideValueX = (direction >= 0) ? 2 : -2;

            if (!App.g_pi.PageDirectionIsLeft)
                slideValueX = -slideValueX;

            //新しい画像
            Bmp = BitmapUty.MakeFittedBitmap(slideBmp, this.ClientSize);

            if (slideValueX > 0)
                AddOffset(-20, 0);
            if (slideValueX < 0)
                AddOffset(+20, 0);

            //描写抑制を解除
            //EndUpdate();

            //alpha = 0.0f;
            Opacity = 1.0F;
            //PicPanel.AutoScroll = false;	//スクロールバーを消したい
            FastDraw = true;
            for (int x = 0; x < 10; x++)
            {
                AddOffset(slideValueX, 0);
                //alpha += 0.1f;
                Refresh();
            }
            FastDraw = false;
        }
    }//class
}
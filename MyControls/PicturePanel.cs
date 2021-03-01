using System;
using System.Collections.Generic;
using System.Diagnostics;		//Debug
using System.Drawing;
using System.Drawing.Drawing2D;	//Matrix
using System.Drawing.Imaging;	//ColorMatrix
using System.Runtime.InteropServices;	//DllImport
using System.Windows.Forms;

namespace Marmi
{
    public partial class PicturePanel : UserControl
    {
        public enum DrawStatus
        {
            idle,
            animating,
            drawing,
        };

        #region --- private member ---

        private bool mouseDowmFlag = false;
        private Point mouseDownPoint = Point.Empty;     //MouseDownされた位置（ドラッグ中も不変）
        private Point mousePredragPos = Point.Empty;    //前回ドラッグされた位置（ドラッグ中に変化）
        private Matrix mat = new Matrix();              //アフィン変換用行列
        private ColorMatrix colmat = new ColorMatrix(); //半透明対応 ColorMatrix
        private Bitmap _bmp = null;
        private bool _isAutoFit = true;

        //private HScrollBar hscrollbar = new HScrollBar();
        //private VScrollBar vscrollbar = new VScrollBar();
        private Font MessageFont = new Font("メイリオ", 20, FontStyle.Bold);

        #endregion --- private member ---

        #region --- properties ---

        /// <summary>
        /// このパネルの表示状態
        /// </summary>
        public DrawStatus State { get; set; }

        /// <summary>
        /// 表示する画像
        /// </summary>
        public Bitmap bmp
        {
            get { return _bmp; }
            set
            {
                _bmp = value;
                if (!App.Config.keepMagnification)
                    mat.Reset();
                //alpha = 1.0f;

                //コメントアウト。これがあると一瞬スクロールバーが見える
                //AjustScrollMinSize();
            }
        }

        /// <summary>
        /// 透明度 0f～1.0f
        /// </summary>
        public float alpha
        {
            get { return colmat.Matrix33; }
            set
            {
                if (value > 1.0f)
                    colmat.Matrix33 = 1.0f;
                else if (value < 0f)
                    colmat.Matrix33 = 0.0f;
                else
                    colmat.Matrix33 = value;
            }
        }

        /// <summary>
        /// 拡大率
        /// </summary>
        public float ZoomRatio
        {
            get { return mat.Elements[0]; }
            set
            {
                Zoom(value);
                //AjustScrollMinSize();
                //AjustViewLocation();
            }
        }

        /// <summary>
        /// 自動縮尺モードか
        /// </summary>
        public bool isAutoFit
        {
            get { return _isAutoFit; }
            set
            {
                //変更がなければなにもしない
                //if (value == _isAutoFit)
                //    return;

                //変更する
                _isAutoFit = value;
                //AutoScroll = !value;
                if (_isAutoFit)
                    ZoomRatio = GetScreenFitRatio();
                AjustScrollMinSize();
                AjustViewLocation();
            }
        }

        /// <summary>
        /// 画面ぴったりの拡大比率
        /// </summary>
        public float FittingRatio { get { return GetScreenFitRatio(); } }

        /// <summary>
        /// 高速描写するかどうか
        /// </summary>
        public bool fastDraw { get; set; }

        /// <summary>
        /// 最後の描写モード
        /// </summary>
        public InterpolationMode LastDrawMode { get; private set; }

        //表示するメッセージ
        public string Message { get; set; }

        //描写命令番号
        public long drawOrderTime { get; set; }

        #endregion --- properties ---

        public PicturePanel()
        {
            InitializeComponent();
            fastDraw = false;

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
            mat.Reset();

            //半透明対応 ColorMatrix
            colmat.Matrix00 = 1f;
            colmat.Matrix11 = 1f;
            colmat.Matrix22 = 1f;
            colmat.Matrix33 = 1f;
            colmat.Matrix44 = 1f;
            alpha = 1.0f;

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
            if (!App.Config.keepMagnification)
                mat.Reset();
            alpha = 1.0f;
        }

        public void ResizeEnd()
        {
            //リサイズ中はスクロールバーを隠している
            //UserControlにはOnResizeEndが無いので
            //Form.OnresizeEnd()から呼び出してもらう。
            fastDraw = false;
            //AutoScroll = true;

            //ver1.30 最大化復帰時のスクロールバー表示対策
            //this.Refresh();
            AjustViewAndShow();
        }

        /// <summary>
        /// 準備中と表示
        /// </summary>
        //public void ShowPreparation(string filename)
        //{
        //    ResetView();
        //    using (Graphics g = this.CreateGraphics())
        //    {
        //        g.Clear(App.Config.BackColor);
        //        DrawTextBottomRight(g, "準備中・・・" + filename, MessageFont);
        //    }
        //}

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
        public bool checkMousePosRight()
        {
            Point pos = this.PointToClient(Cursor.Position);
            if (pos.X > this.ClientSize.Width / 2)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 画面表示位置を変更
        /// </summary>
        /// <param name="dx">X方向に動かす量</param>
        /// <param name="dy">Y方向に動かす量</param>
        public void AddOffset(int dx, int dy)
        {
            mat.Translate((float)dx, (float)dy, MatrixOrder.Append);
        }

        #region override

        //-----------------------------------------------------------------
        // override
        //
        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
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
            if (bmp == null)
                return;

            //ver1.63 後ろに移動
            if (!string.IsNullOrEmpty(Message))
            {
                Debug.WriteLine($"PicPanel Message({Message})");
                DrawTextBottomRight(e.Graphics, Message, MessageFont);
                return;
            }

            ////スクロールバーの調整
            //AjustScrollMinSize();
            //AjustViewLocation();

            if (mouseDowmFlag                   //ドラッグスクロール
                || fastDraw                     //高速描写の必要性があるとき
                || (ZoomRatio > 1.0f && ZoomRatio % 1.0f <= 0.01f && App.Config.isDotByDotZoom) //整数倍拡大
                                                                                                    //|| (ZoomRatio > 1.0f && App.Config.isDotByDotZoom)	//整数倍拡大
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
            //e.Graphics.Transform = mat;
            if (alpha == 1.0f)
            {
                if (App.Config.useUnsharpMask   //アンシャープが有効
                    && LastDrawMode == InterpolationMode.HighQualityBicubic //高画質描写を要求
                    && ZoomRatio != 100.0f  //100%描写ではない
                    )
                {
                    //アンシャープフィルタをかける
                    //e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    Bitmap orgBmp = new Bitmap(this.Width, this.Height);
                    using (var g = Graphics.FromImage(orgBmp))
                    {
                        g.Clear(this.BackColor);
                        g.Transform = mat;
                        g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                    }
                    var unsharpBmp = BitmapUty.Unsharpness_unsafe(orgBmp, App.Config.unsharpDepth);
                    e.Graphics.DrawImage(unsharpBmp, 0, 0, unsharpBmp.Width, unsharpBmp.Height);
                }
                else
                {
                    //通常表示（半透明なし）
                    e.Graphics.Transform = mat;
                    e.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                }
            }
            else
            {
                //半透明描写
                //colmat.Matrix33 = alpha;
                ImageAttributes ia = new ImageAttributes();
                ia.SetColorMatrix(colmat);

                //アルファブレンドしながら描写
                Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);
                e.Graphics.DrawImage(
                    bmp, r, 0, 0, bmp.Width, bmp.Height,
                    GraphicsUnit.Pixel, ia);
                ia.Dispose();
            }

            ////ver1.63 メッセージを表示
            //if (!string.IsNullOrEmpty(Message))
            //{
            //    Uty.WriteLine("PicPanel Message({0})", Message);
            //    DrawTextBottomRight(e.Graphics, Message, MessageFont);
            //    return;
            //}

            setStatusbarRatio();
        }

        protected override void OnResize(EventArgs e)
        {
            //Debug.WriteLine("OnResize()");
            //base.OnResize(e);

            if (bmp == null)
                return;

            //表示倍率の調整
            if (isAutoFit)
            {
                float r = GetScreenFitRatio();
                if (r > 1.0f && App.Config.noEnlargeOver100p)
                    r = 1.0f;
                ZoomRatio = r;
            }

            fastDraw = true;

            //中央になるようにする。
            AjustViewLocation();
            this.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseClick()");
            //ドラッグチェック
            if (mouseDowmFlag == true
                && mouseDownPoint != e.Location)
                return; //クリックとみなさない

            //----------------------------------------------
            //ここからがクリックイベント本体
            base.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseDown()");
            base.OnMouseDown(e);    //ルーペなどの処理

            if (bmp == null)
                return;

            if (e.Button == MouseButtons.Left   //左ボタンが押されている
                                                // ver1.50 コメントアウト
                                                // bmpが中身を持っていないことがある（消されている時とか。
                                                //&& (bmp.Width*ZoomRatio > ClientRectangle.Width	// 横スクロール無しチェック
                                                //|| bmp.Height * ZoomRatio > ClientRectangle.Height)// 縦スクロール無しチェック
                )
            {
                mouseDowmFlag = true;
                mouseDownPoint = mousePredragPos = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseUp()");
            base.OnMouseUp(e);
            mouseDowmFlag = false;
            mouseDownPoint = mousePredragPos = Point.Empty;

            //ドラッグスクロールによる低品質描写をした可能性があるので再描写
            //if (fastDraw)
            if (LastDrawMode
                    == System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor)
            {
                fastDraw = false;
                this.Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseDoubleUp()");
            //base.OnMouseDoubleClick(e);

            //ver1.80 全画面をダブルクリックで対応するオプション導入
            if (App.Config.DoubleClickToFullscreen)
                base.OnMouseDoubleClick(e);
            //ToggleFullScreen();
            else
                OnMouseClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDowmFlag)
            {
                //Debug.WriteLine("OnMouseMove() Drag");
                float dx = e.X - mousePredragPos.X;
                float dy = e.Y - mousePredragPos.Y;

                mat.Translate(
                    dx,
                    dy,
                    MatrixOrder.Append);

                //上下左右にスクロールしすぎないように補正
                AjustViewLocation();

                //マウス位置保存
                mousePredragPos.X = e.X;
                mousePredragPos.Y = e.Y;

                //スクロールバー更新
                // ToDo ちらつき解除したい
                //this.AutoScrollPosition = new Point(-(int)mat.OffsetX,-(int)mat.OffsetY);
                AjustScrollPosition();
                this.Invalidate();

                if (Cursor.Current != App.Cursors.OpenHand)
                    Cursor.Current = App.Cursors.OpenHand;

                //デバッグ中ならステータスバーも更新
#if DEBUG
                setStatusbarRatio();
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
            if (bmp == null)
                return;

            //ver1.30 Ctrlキーを押しているときは強制的にズーム
            if (App.Config.mouseConfigWheel == "拡大縮小"
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
            mat.Translate(-dx, -dy, MatrixOrder.Append);

            //移動方法２：軸をゼロクリアして再設定する方法
            //float ratio = mat.Elements[0];
            //mat.Reset();
            //mat.Scale(ratio, ratio, MatrixOrder.Append);
            //mat.Translate(AutoScrollPosition.X, AutoScrollPosition.Y, MatrixOrder.Append);

            printScrollInfo("OnScroll()");

            base.OnScroll(se);
            //Invalidate不要
            //this.Invalidate();
        }

        #endregion override

        //-----------------------------------------------------------------
        // privateメソッド
        //

        #region 表示関連の調整

        private void setStatusbarRatio()
        {
            //Debug.WriteLine("setStatusbarText()");
            if (_bmp == null)
                return;

            string text = string.Format(
                    "{0:N0} x {1:N0} , 拡大率={2:P1}",
                    _bmp.Width,
                    _bmp.Height,
                    ZoomRatio);

#if DEBUG
            //デバグ中のみスクロール情報を表示
            text += " ScrollOffset=" + AutoScrollOffset;
            text += " ScrollPosition=" + AutoScrollPosition;
#endif

            ((Form1)(Form1._instance)).setStatubarRatio(text);
        }

        /// <summary>
        /// 画像を初期位置に設定する
        /// 中央に表示するようにする
        /// </summary>
        private void AjustViewLocation()
        {
            //スクロールバーがないときに中央に
            //左右方向にスクロールさせすぎないように
            //上下方向にスクロールさせすぎないように
            //するルーチン

            if (bmp == null)
                return;

            //補正値:平行移動させる値
            float dx = 0;
            float dy = 0;

            int bmpwidth = (int)(bmp.Width * ZoomRatio);
            int bmpheight = (int)(bmp.Height * ZoomRatio);

            #region X方向

            if (ClientRectangle.Width > bmpwidth)
            {
                //スクロールバーなし。画像のほうが小さいので中央に
                float newPointX = (ClientRectangle.Width - bmpwidth) / 2;
                dx = -(mat.OffsetX - newPointX);
            }
            else if (mat.OffsetX > 0.0f)//左
            {
                dx = -mat.OffsetX;  //オフセットを0にする
            }
            else if (mat.OffsetX < -(bmpwidth - ClientRectangle.Width))//左
            {
                dx = -(bmpwidth - ClientRectangle.Width) - mat.OffsetX;//右
            }

            #endregion X方向

            #region Y方向

            //------------------------------------------------------
            if (ClientRectangle.Height > bmpheight)
            {
                //スクロールバーなし。画像のほうが小さいので中央に
                float newPointY = (ClientRectangle.Height - bmpheight) / 2;
                dy = -(mat.OffsetY - newPointY);
            }
            else if (mat.OffsetY > 0.0f)//上
            {
                dy = -mat.OffsetY;  //オフセットを0にする
            }
            else if (mat.OffsetY < -(bmpheight - ClientRectangle.Height))
            {
                dy = -(bmpheight - ClientRectangle.Height) - mat.OffsetY;
            }

            #endregion Y方向

            //set
            if (dx == 0 && dy == 0)
                return;
            mat.Translate(dx, dy, MatrixOrder.Append);

            //これをコメントアウトするとちらつきが減る
            ////スクロールバーを調整
            //this.AutoScrollPosition = new Point(
            //    -(int)mat.OffsetX, -(int)mat.OffsetY);
            //Debug.WriteLine(string.Format(
            //    "AjustViewLocation(): dx={0},dy={1}",
            //    dx,dy
            //    ));
        }

        /// <summary>
        /// スクロールバーをズーム比率に応じて設定
        /// </summary>
        private void AjustScrollMinSize()
        {
            if (bmp != null)
            {
                this.AutoScrollMinSize = new Size(
                    (int)((float)bmp.Width * ZoomRatio),
                    (int)((float)bmp.Height * ZoomRatio));

                //これはいらない
                //スクロールバー位置を調整
                //this.AutoScrollPosition = new Point(
                //    -(int)mat.OffsetX, -(int)mat.OffsetY);
                //Debug.WriteLine(AutoScrollMinSize, "AjustScrollMinSize()");
            }
        }

        /// <summary>
        /// スクロールバーの位置がずれていたら直す
        /// </summary>
        private void AjustScrollPosition()
        {
            int matx = (int)mat.OffsetX;
            int maty = (int)mat.OffsetY;

            //printScrollInfo("AjustScrollPosition()");

            if (AutoScrollPosition.X != matx
                || AutoScrollPosition.Y != maty)
            {
                this.AutoScrollPosition = new Point(-matx, -maty);
            }
        }

        private float GetScreenFitRatio()
        {
            //ClientRectangleはスクロールバー分が考慮されているので
            //スクロールバーがないBoundsを使う。
            if (bmp == null)
                return 1.0f;
            else
            {
                try
                {
                    float rx = (float)this.Bounds.Width / (float)bmp.Width;
                    float ry = (float)this.Bounds.Height / (float)bmp.Height;

                    //小さいほうを使う
                    float r = rx < ry ? rx : ry;
                    return r;
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

        //-----------------------------------------------------------------
        // 拡大縮小関連

        public void ZoomOut()
        {
            //2011/11/22 ちらつきの事前回避
            //効果不明
            BeginUpdate();
            AjustScrollPosition();
            EndUpdate();
            //this.Refresh();	//ここで再描写する必要なし

            List<float> ratios = GetZoomRatioArray();

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
                    setStatusbarRatio();
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

            List<float> ratios = GetZoomRatioArray();

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
                    setStatusbarRatio();
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
            fastDraw = true;
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
            fastDraw = false;
        }

        private List<float> GetZoomRatioArray()
        {
            List<float> ratios = new List<float>{
                        0.1f, 0.2f, 0.25f, 0.5f, 0.75f,
                        1.0f,
                        1.5f, 2.0f, 3.0f, 4.0f, 5.0f, 8.0f, 10.0f, 20.0f };

            if (!ratios.Contains(FittingRatio))
            {
                ratios.Add(FittingRatio);
                ratios.Sort();
            }
            return ratios;
        }

        /// <summary>
        /// 現在の倍率に新しい倍率を掛け合わせる
        /// </summary>
        /// <param name="zoom">掛け合わせる倍率</param>
        /// <param name="centerX">中心位置</param>
        /// <param name="centerY">中心位置</param>
        //public void MultipleZoom(float zoom, int centerX, int centerY)
        //{
        //    isAutoFit = false;

        //    //拡大縮小は原点を中心に行われるので
        //    //拡縮中心地（現在のマウスポイント）を原点(0,0)に

        //    // ポインタの位置→原点へ移動
        //    mat.Translate(
        //        -centerX,
        //        -centerY,
        //        MatrixOrder.Append);

        //    // 拡大
        //    mat.Scale(
        //            zoom,
        //            zoom,
        //            MatrixOrder.Append);

        //    // 原点→ポインタの位置へ移動(元の位置へ戻す)
        //    mat.Translate(
        //        centerX,
        //        centerY,
        //        MatrixOrder.Append);

        //    //AjustScrollMinSize();
        //}

        /// <summary>
        /// 指定倍率を設定
        /// </summary>
        /// <param name="ratio">指定倍率（絶対値）</param>
        /// <param name="centerX">中心位置</param>
        /// <param name="centerY">中心位置</param>
        private void Zoom(float ratio, int centerX, int centerY)
        {
            //拡大縮小は原点を中心に行われるので
            //拡縮中心地（現在のマウスポイント）を原点(0,0)に

            // ポインタの位置→原点へ移動
            mat.Translate(
                -centerX,
                -centerY,
                MatrixOrder.Append);

            // 拡大
            float zoomratio = ratio / mat.Elements[0];
            mat.Scale(
                    zoomratio,
                    zoomratio,
                    MatrixOrder.Append);

            // 原点→ポインタの位置へ移動(元の位置へ戻す)
            mat.Translate(
                centerX,
                centerY,
                MatrixOrder.Append);

            //スクロールバー更新
            //これがあるとZoomがかくつく
            //AjustScrollMinSize();
            //this.AutoScrollPosition = new Point(-(int)mat.OffsetX, -(int)mat.OffsetY);
        }

        private void Zoom2(float ratio, int centerX, int centerY)
        {
            //正確な値でズームする.ただし遅い・・・

            //Zoom=1.0に換算したときの画像中心値を得る
            float cx = -((mat.OffsetX - centerX) / ZoomRatio);
            float cy = -((mat.OffsetY - centerY) / ZoomRatio);
            int x = (int)cx;
            int y = (int)cy;

            //リセット
            mat.Reset();
            //中心に戻して
            mat.Translate(-x, -y, MatrixOrder.Append);
            //ズーム
            mat.Scale(ratio, ratio, MatrixOrder.Append);
            // 原点→ポインタの位置へ移動(元の位置へ戻す)
            mat.Translate(centerX, centerY, MatrixOrder.Append);
        }

        private void Zoom(float zoomRatio)
        {
            //拡大縮小は画面中心
            //Zoom(zoomRatio,
            //    ClientRectangle.Width / 2,
            //    ClientRectangle.Height / 2);

            Point pt = PointToClient(MousePosition);
            if (ClientRectangle.Contains(pt))
            {
                //Zoom(zoomRatio, pt.X, pt.Y);
                Zoom2(zoomRatio, pt.X, pt.Y);
            }
            else
            {
                //Zoom(zoomRatio, ClientRectangle.Width / 2, ClientRectangle.Height / 2);
                Zoom2(zoomRatio, ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            }
        }

        #endregion 拡大縮小関連

        #region 回転

        public void Rotate()
        {
            if (bmp != null)
            {
                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                ZoomRatio = FittingRatio;
                AjustScrollMinSize();
                AjustViewLocation();
                Refresh();
            }
            return;
        }

        #endregion 回転

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

        #region BeginUpdate/EndUpdate

        //-----------------------------------------------------------------
        //描写制御
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public const int WM_SETREDRAW = 0x000B;
        public const int Win32False = 0;
        public const int Win32True = 1;

        /// <summary>
        /// EndUpdate メソッドが呼ばれるまで、コントロールを再描画しないようにします。
        /// </summary>
        public void BeginUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, Win32False, 0);
        }

        /// <summary>
        /// BeginUpdate メソッドにより中断されていた描画を再開します。
        /// </summary>
        public void EndUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, Win32True, 0);
            //this.Invalidate();
        }

        #endregion BeginUpdate/EndUpdate

        #region Animate

        /// <summary>
        /// 現在表示している画像をスライドアウトする
        /// </summary>
        /// <param name="direction">0以上なら右へ、マイナスなら左へ</param>
        public void AnimateSlideOut(int direction)
        {
            //スクロールバーを消す
            BeginUpdate();      //描写を一時ストップ
            AutoScroll = false; //スクロールバーを消す
            EndUpdate();        //描写再開
            Refresh();  //これがないとスクロールバーなしで再描写しない。
                        //System.Threading.Thread.Sleep(1000);
                        //return;

            Bitmap tmp = BitmapUty.CaptureWindow(this);

            this.bmp = tmp;
            AjustViewAndShow();
            //Stopwatch sw = Stopwatch.StartNew();
            //スライド方向の決定
            int slideValueX = 2;    //右へ順送り
            int slideValueY = 0;
            slideValueX = (direction >= 0) ? 2 : -2;

            if (!App.g_pi.PageDirectionIsLeft)
                slideValueX = -slideValueX;

            fastDraw = true;
            for (int x = 0; x < 10; x++)
            {
                AddOffset(slideValueX, slideValueY);
                alpha -= 0.1f;
                Refresh();
            }
            fastDraw = false;
            //this.AutoScroll = true;
            //sw.Stop();
            //if(sw.ElapsedMilliseconds>0)
            //    Uty.WriteLine("Animate fps={0}", 10 * 1000 / sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// 指定した画像をスライドインする
        /// </summary>
        /// <param name="slideBmp">スライドインする画像</param>
        /// <param name="direction">スライド方向、マイナスなら左へ</param>
        public void AnimateSlideIn(Bitmap slideBmp, int direction)
        {
            //スライド方向の決定
            int slideValueX = (direction >= 0) ? 2 : -2;
            int slideValueY = 0;

            if (!App.g_pi.PageDirectionIsLeft)
                slideValueX = -slideValueX;

            //新しい画像
            bmp = BitmapUty.MakeFittedBitmap(slideBmp, this.ClientSize);

            if (slideValueX > 0)
                AddOffset(-20, 0);
            if (slideValueX < 0)
                AddOffset(+20, 0);

            //描写抑制を解除
            //EndUpdate();

            //alpha = 0.0f;
            alpha = 1.0F;
            //PicPanel.AutoScroll = false;	//スクロールバーを消したい
            fastDraw = true;
            for (int x = 0; x < 10; x++)
            {
                AddOffset(slideValueX, slideValueY);
                //alpha += 0.1f;
                Refresh();
            }
            fastDraw = false;
        }

        public void AnimateFadeOut()
        {
            //ver1.24今の画面をキャプチャしアニメーション効果にする。
            Bitmap tempbmp = BitmapUty.CaptureWindow(this);
            bmp = tempbmp;
            ResetView();

            //だんだん透明度を上げる
            if (alpha > 0f)
            {
                alpha -= 0.1f;
                if (alpha < 0f)
                    alpha = 0f;
                Refresh();
            }
        }

        public void AnimateFadeIn(Bitmap bmp)
        {
            //新しい画像
            bmp = BitmapUty.MakeFittedBitmap(bmp, this.ClientSize);

            //描写抑制を解除
            //EndUpdate();

            alpha = 0.0f;
            //PicPanel.AutoScroll = false;	//スクロールバーを消したい
            fastDraw = true;
            for (int x = 0; x < 10; x++)
            {
                alpha += 0.1f;
                Refresh();
            }
            fastDraw = false;
        }

        #endregion Animate

        //-----------------------------------------------------------------
        //Debug用
        private void printScrollInfo(string s)
        {
            Debug.WriteLine(string.Format(
                "{4}: ScrollPos={0}, ScrollSize={1}, matOffset=({2},{3})",
                AutoScrollPosition,
                AutoScrollMinSize,
                mat.OffsetX, mat.OffsetY,
                s
                ));
        }
    }//class
}
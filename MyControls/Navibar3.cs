using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public class NaviBar3 : UserControl
    {
        //サムネイルサイズ
        private const int THUMBSIZE = 200; //320;

                                           //各種余白
                                           //private const int PADDING = 6;
        private const int PADDING = 2;

        //左右の画像の暗さ％
        private const int DARKPERCENT = 50;

        //BOXサイズ：コンストラクタで計算
        private int BOX_HEIGHT;

        //選択されているアイテム
        public int m_selectedItem;

        //g_piそのものを挿す
        private PackageInfo m_packageInfo;

        //背景色
        private SolidBrush m_BackBrush = new SolidBrush(Color.FromArgb(192, 48, 48, 48));

        //テキスト描写フォント
        private Font fontS = new Font("ＭＳ Ｐ ゴシック", 9F);

        private Font fontL = new Font("Century Gothic", 16F);

        //テキスト描写フォーマット
        private StringFormat sfCenterUp = new StringFormat() { Alignment = StringAlignment.Center };

        private StringFormat sfCenterDown = new StringFormat()
        { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        //テキストの高さ
        private int FONT_HEIGHT;

        //透明度
        private float alpha = 1.0F;     //描写時の透明度。OnPaint()に効いてくる

                                        //サムネイルの位置
        private List<ItemPos> thumbnailPos = null;

        // Loadingイメージ
        private Bitmap dummyImage = null;

        //アニメーションタイマー
        private System.Windows.Forms.Timer timer = null;

        //現在の描写位置
        private int nowOffset;

        // 初期化 ***********************************************************************/

        public NaviBar3(PackageInfo pi)
        {
            m_packageInfo = pi;
            m_selectedItem = -1;

            //背景色
            this.BackColor = Color.Transparent;
            //ダブルバッファを有効に
            this.DoubleBuffered = true;

            //ver1.19 フォーカスを当てないようにする
            this.SetStyle(ControlStyles.Selectable, false);
            //DPIスケーリングは無効にする
            this.AutoScaleMode = AutoScaleMode.None;
            //透明度は1.0
            alpha = 1.0F;

            //高さを算出
            BOX_HEIGHT = PADDING + THUMBSIZE;
            //newされたあとに高さを必要とされるので高さだけ入れておく。
            this.Height = BOX_HEIGHT        //画像部分
                                            //+ PADDING
                + PADDING;

            //fontの高さを測る
            using (Bitmap bmp = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    SizeF sf = g.MeasureString("テスト文字列", fontS);
                    FONT_HEIGHT = (int)sf.Height;
                }
            }

            //Loadingと表示するイメージ
            dummyImage = BitmapUty.LoadingImage(THUMBSIZE * 2 / 3, THUMBSIZE);

            //タイマーの初期設定
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;
            timer.Tick += new EventHandler(timer_Tick);

            //オフセットを設定
            nowOffset = 0;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            int diff = (GetOffset(m_selectedItem) - nowOffset);
            //THUMBSIZE以上離れていたらすぐにTHUMBSIZEに近づける
            //if (diff > THUMBSIZE*2)
            //    diff = diff - THUMBSIZE*1;
            //else
            diff = diff * 2 / 7;
            nowOffset += diff;

            if (diff == 0)
            {
                timer.Stop();
                CalcAllItemPos();
                Debug.WriteLine("Timer Stop diff 0");
            }
            //描写
            this.Refresh();
        }

        // publicメソッド/プロパティ ****************************************************/

        public void OpenPanel(Rectangle rect, int index)
        {
            //サムネイル作成中なら一度止める
            //Form1.PauseThumbnailMakerThread();

            this.Top = rect.Top;
            this.Left = rect.Left;
            this.Width = rect.Width;
            //this.Height = BOX_HEIGHT        //画像部分
            //    + PADDING + FONT_HEIGHT      //画像番号部
            //    + PADDING + FONT_HEIGHT      //ファイル名
            //    + PADDING;
            this.Height = BOX_HEIGHT        //画像部分
                                            //+PADDING
                + PADDING;

            m_selectedItem = index;

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
            nowOffset = GetOffset(index);

            this.Visible = true;
            alpha = 1.0F;
            this.Refresh();

            //timerを止める
            if (timer != null)
                timer.Stop();
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
            alpha = 1.0F;

            //timerを止める
            if (timer != null)
                timer.Stop();
        }

        public void SetCenterItem(int index)
        {
            m_selectedItem = index;

            //ver1.37バグ対処:非表示ではなにもしない
            if (!Visible)
                return;

            if (timer != null)
            {
                //タイマーで描写
                timer.Enabled = true;
                Debug.WriteLine("TimerStart at SetCenterItem");
            }
            else
            {
                //タイマーを使わずに再描写
                nowOffset = GetOffset(index);
                this.Refresh();
            }
        }

        // オーナードロー ***************************************************************/

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            //g.Clear(m_NormalBackColor);

            if (alpha >= 1.0F)
            {
                //DrawItems(g);
                DrawItemAll(g);
            }
            else
            {
                using (Bitmap bmp = new Bitmap(this.Width, this.Height))
                {
                    Graphics.FromImage(bmp).Clear(Color.Transparent);
                    //DrawItems(Graphics.FromImage(bmp));
                    DrawItemAll(Graphics.FromImage(bmp));
                    BitmapUty.alphaDrawImage(g, bmp, alpha);
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
            g.FillRectangle(m_BackBrush, this.DisplayRectangle);

            //表示すべきアイテムがない場合は背景だけ
            if (m_packageInfo == null || m_packageInfo.Items.Count < 1)
                return;

            //すべてのアイテムの位置を更新
            //CalcAllItemPos();

            //オフセットを計算
            int offset;
            if (timer == null)
                //タイマーが動いていないときはすぐその場所へ
                offset = GetOffset(m_selectedItem);
            else
                //タイマーが動いているときはoffsetはTimerが更新
                offset = nowOffset;

            //全アイテム描写
            for (int item = 0; item < m_packageInfo.Items.Count; item++)
                DrawItem(g, item, offset);
            return;
        }

        /// <summary>
        /// サムネイルの表示位置をすべて更新
        /// </summary>
        private void CalcAllItemPos()
        {
            if (thumbnailPos == null)
            {
                //新規に位置リストを作成
                thumbnailPos = new List<ItemPos>();
                int X = 0;
                for (int i = 0; i < m_packageInfo.Items.Count; i++)
                {
                    ItemPos item = new ItemPos();
                    item.pos.X = X;
                    item.pos.Y = PADDING;
                    if (m_packageInfo.Items[i].thumbnail != null)
                    {
                        item.size = BitmapUty.calcHeightFixImageSize(m_packageInfo.Items[i].thumbnail.Size, THUMBSIZE);
                    }
                    else
                        item.size = new System.Drawing.Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    thumbnailPos.Add(item);
                    X += item.size.Width + PADDING;
                }
            }
            else
            {
                //すでにあるものを更新
                int X = 0;
                for (int i = 0; i < m_packageInfo.Items.Count; i++)
                {
                    //ItemPos item = thumbnailPos[i];
                    thumbnailPos[i].pos.X = X;
                    thumbnailPos[i].pos.Y = PADDING;
                    if (m_packageInfo.Items[i].thumbnail != null)
                        thumbnailPos[i].size = BitmapUty.calcHeightFixImageSize(m_packageInfo.Items[i].thumbnail.Size, THUMBSIZE);
                    else
                        thumbnailPos[i].size = new System.Drawing.Size(THUMBSIZE * 2 / 3, THUMBSIZE);
                    X += thumbnailPos[i].size.Width + PADDING;
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
            int X = thumbnailPos[centerItem].pos.X;
            int center = X + thumbnailPos[centerItem].size.Width / 2;
            int offset = center - this.Width / 2;
            return offset;
        }

        /// <summary>
        /// 指定した1アイテムを描写する
        /// </summary>
        /// <param name="g">描写先のGraphics</param>
        /// <param name="index">描写アイテム番号</param>
        private void DrawItem(Graphics g, int index, int offset)
        {
            //Indexチェック
            //if (index < 0 || index >= m_packageInfo.Items.Count)
            //    return;
            //未計算だったら計算
            if (thumbnailPos == null)
                CalcAllItemPos();

            //描写位置を決定
            int x = thumbnailPos[index].pos.X - offset;

            //描写対象外をはじく
            if (x > this.Width)
                return;
            if (x + thumbnailPos[index].size.Width < 0)
                return;

            //サイズ調整
            Bitmap img = BitmapUty.MakeHeightFixThumbnailImage(
                m_packageInfo.Items[index].thumbnail as Bitmap,
                THUMBSIZE);

            if (img == null)
            {
                img = dummyImage;
                //非同期で画像を取得してくる
                //タイマーが止まっていたら非同期で画像取得
                //if (timer == null || !timer.Enabled)
                //{
                //	Form1._instance.AsyncGetBitmap(index, (MethodInvoker)(() =>
                //		{
                //			Debug.WriteLine("Navibar3::DrawItem() GoGO");
                //			//再計算
                //			//CalcAllItemPos();

                //			//GUIスレッドで再描写
                //			//非表示では何もしない
                //			if (!this.Visible)
                //				return;

                //			//タイマー動作中も何もしない
                //			//if (timer != null && timer.Enabled)
                //			//    return;

                //			//再描写
                //			CalcAllItemPos();
                //			nowOffset = GetOffset(m_selectedItem);
                //			if(this.Visible)
                //				this.Invalidate();
                //		}));
                //}

                //ver1.81 読み込みルーチンをPushLow()に変更
                if (timer == null || !timer.Enabled)
                {
                    //Form1.PushLow(index, (Action)(() =>
                    //{
                    //    var bmp = Form1.SyncGetBitmap(index);
                    //    App.g_pi.ThumnailMaker(index, bmp);
                    //    CalcAllItemPos();
                    //    if (this.Visible)
                    //        this.Invalidate();
                    //}));
                    AsyncIO.AddJobLow(index, () =>
                    {
                        var bmp = Form1.SyncGetBitmap(index);
                        App.g_pi.ThumnailMaker(index, bmp);
                        CalcAllItemPos();
                        if (this.Visible)
                            this.Invalidate();
                    });
                }
            }

            Rectangle cRect = new Rectangle(
                thumbnailPos[index].pos.X - offset,
                thumbnailPos[index].pos.Y + BOX_HEIGHT - img.Height - PADDING,      //下揃え
                thumbnailPos[index].size.Width,
                thumbnailPos[index].size.Height);

            //描写
            if (index == m_selectedItem)
            {
                //中央のアイテム
                //ver1.17追加 フォーカス枠
                BitmapUty.drawBlurEffect(g, cRect, Color.LightBlue);
                //中央を描写
                g.DrawImage(img, cRect);

                //ver1.62 コメントアウト
                ////画像番号を描写
                //Rectangle stringRect = new Rectangle(
                //    0,
                //    BOX_HEIGHT + PADDING,
                //    this.Width,
                //    FONT_HEIGHT);

                //g.DrawString(
                //    string.Format("{0}", m_selectedItem + 1),
                //    fontS,
                //    Brushes.White,
                //    stringRect,
                //    sfCenterUp);

                ////中央の文字列を描写
                //stringRect.X = 0;
                //stringRect.Y = BOX_HEIGHT + PADDING + FONT_HEIGHT + PADDING;
                //stringRect.Width = this.Width;
                //stringRect.Height = FONT_HEIGHT;

                //g.DrawString(
                //    Path.GetFileName(m_packageInfo.Items[m_selectedItem].filename),
                //    fontS,
                //    Brushes.White,
                //    stringRect,
                //    sfCenterUp);
            }
            else
            {
                //中央以外の画像を描写
                g.DrawImage(
                    BitmapUty.BitmapToDark(img, DARKPERCENT),
                    cRect);

                ////画像番号を描写
                //cRect.Y = BOX_HEIGHT + PADDING;
                //g.DrawString(
                //    string.Format("{0}", index + 1),
                //    fontS,
                //    Brushes.LightGray,
                //    cRect,
                //    sfCenter);
            }

            //画像番号を画像上に表示
            g.DrawString(
                string.Format("{0}", index + 1),
                fontL,
                Brushes.LightGray,
                cRect,
                sfCenterDown);
        }

        /// <summary>
        /// 非同期でサムネイルを作成、描写まで行う
        /// これをThreadPoolに入れる
        /// </summary>
        /// <param name="index"></param>
        //private void AsyncGetBitmapAndDraw(int index)
        //{
        //    //2重に非同期取得が発行されることがあるのでチェック
        //    if (m_packageInfo.Items[index].ThumbImage != null)
        //        return;

        //    //Bitmapを取得＝サムネイル作成
        //    //Form1._instance.GetBitmap(index);
        //    m_packageInfo.GetBitmap(index);	//ver1.39

        //    //再計算
        //    //CalcAllItemPos();

        //    //GUIスレッドで再描写
        //    BeginInvoke((MethodInvoker)(() =>
        //    {
        //        //非表示では何もしない
        //        if (!this.Visible)
        //            return;

        //        //タイマー動作中も何もしない
        //        //if (timer != null && timer.Enabled)
        //        //    return;

        //        //再描写の必要有
        //        CalcAllItemPos();
        //        nowOffset = GetOffset(m_selectedItem);
        //        this.Invalidate();
        //    }));
        //}
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
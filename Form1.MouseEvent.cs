using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.Windows.Forms;

/*
 * マウスとキーイベント
 *
 * ver1.09で切り出し
 * 2011年8月15日
 *
 */

namespace Marmi
{
    public partial class Form1 : Form
    {
        private void PicPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (App.Config.mouseConfigWheel == "拡大縮小")
            {
                //PicPanel内部で処理しているのでなにもしない
            }
            else
            {
                //サイドバーが生きていたら何もしない
                //if (!g_Sidebar.isMinimize)
                //    return;

                if (e.Delta > 0)
                    NavigateToBack();
                else
                    NavigateToForword();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            Uty.WriteLine("OnMouseClick: {0}", e.Button);

            //スライドショー中だったら中断させる
            if (IsSlideShow)
            {
                StopSlideShow();
                return;
            }

            //右クリック対応はForm1_MouseUp()でやっているので無視
            if (e.Button == MouseButtons.Right)
                return;

            //ドラッグによるスクロール中であればクリックイベントは無視
            if (!g_LastClickPoint.IsEmpty)
            {
                Debug.WriteLine("OnMouseClick()どうしてここに来るのか不明");
                g_LastClickPoint = Point.Empty;
                return;
            }

            //ファイルが1つもなければメッセージを表示
            if (App.g_pi.Items.Count == 0)
            {
                //ファイルをD&Dしてくださいと表示
                g_ClearPanel.ShowAndClose(
                    Properties.Resources.MSG_LETDORPFILES, 1000);
                return;
            }

            //ver1.80キー設定に基づく動作
            MethodInvoker func = null;
            if (e.Button == MouseButtons.Middle)
                if (KeyDefines.TryGetValue(Keys.MButton, out func))
                    if (func != null)
                    {
                        func();
                        return;
                    }
            if (e.Button == MouseButtons.XButton1)
                if (KeyDefines.TryGetValue(Keys.XButton1, out func))
                    if (func != null)
                    {
                        func();
                        return;
                    }
            if (e.Button == MouseButtons.XButton2)
                if (KeyDefines.TryGetValue(Keys.XButton2, out func))
                    if (func != null)
                    {
                        func();
                        return;
                    }

            //ページナビゲートをする。
            bool isForword = PicPanel.checkMousePosRight();
            //コンフィグ確認
            if (!App.Config.RightScrClickIsNextPic)
                isForword = !isForword;
            //左開きなら入れ替え
            if (!App.g_pi.PageDirectionIsLeft)
                isForword = !isForword;
            ////入れ替えがあるならさらに入れ替え ver1.65 commentout
            //if (App.Config.isReplaceArrowButton)
            //    isForword = !isForword;
            //ナビゲート
            if (isForword)
            {
                NavigateToForword();
                //Uty.WriteLine("OnMouseClick end ({0}) {1}", g_pi.NowViewPage, tick);
            }
            else
                NavigateToBack();
        }

        /// <summary>
        /// ダブルクリック処理
        /// シングルクリックの2回目と見なして処理する
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            //ver1.80 全画面をダブルクリックで対応するオプション導入
            if (App.Config.DoubleClickToFullscreen)
                ToggleFullScreen();

            if (e.Button == MouseButtons.Left)
                OnMouseClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //カーソルの変更
            SetCursorShape(e.Location);

            //クライアント領域を静的に
            Rectangle cRect = GetClientRectangle();

            //右ボタンが押されていないのにルーペがある。
            if (e.Button != MouseButtons.Right && loupe != null)
            {
                CloseLoupe();
            }

            //右ボタン押下でルーペがなければ作る。
            if (e.Button == MouseButtons.Right && loupe == null)
            {
                //ルーペを作成
                //OpenLoope(cRect);
                OpenLoope(PicPanel.Bounds);
                PicPanel.Visible = false;
            }
            else if (e.Button == MouseButtons.Right && loupe != null)
            {
                //ルーペは動作中
                if (loupe.Visible)
                {
                    //マウスが動いたので表示内容を更新
                    UpdateLoopeView(e.X, e.Y, cRect);
                }
                else
                {
                    //右ドラッグ中に左クリックされたたため非表示状態
                    //ここで削除してしまうと右クリックされているため
                    //またすぐにルーペが生成されてしまうため
                    //いったんコメントアウト
                    //CloseLoupe();

                    //非表示状態であることを確実にするため親を再描写する
                    this.Refresh();
                }
            }

            //全画面モードの時にツールバーを表示するか
            if (App.Config.isFullScreen)
            {
                //全画面表示中。表示非表示を切り替え
                if (e.Y < 10 && toolStrip1.Visible == false)
                {
                    //画面最上部にマウスがあれば表示
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > this.Height - 10 && toolStrip1.Visible == false)
                {
                    //画面下でも動作するようにする
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > toolStrip1.Height && toolStrip1.Visible == true && !App.Config.isThumbnailView)
                {
                    //スクロールバーが出る可能性があるので消しておく
                    //PicPanel.AutoScroll = true;

                    toolStrip1.Visible = false;
                    statusbar.Visible = false;
                }
            }
        }

        /// <summary>
        /// カーソル位置とルーペの状況からカーソル形状を決定する
        /// </summary>
        /// <param name="cursorPos"></param>
        private void SetCursorShape(Point cursorPos)
        {
            if (loupe != null && loupe.Visible)
            {
                //ルーペ中
                ChangeCursor(App.Cursors.Loupe);
            }
            else if (!g_LastClickPoint.IsEmpty)
            {
                //ドラッグスクロール中
                ChangeCursor(App.Cursors.OpenHand);
            }
            else if (App.g_pi.Items.Count <= 1 | !GetClientRectangle().Contains(cursorPos))
            {
                Cursor.Current = Cursors.Default;
            }
            else if (PicPanel.checkMousePosRight())
            {
                //画面の右側
                ChangeCursor(App.Cursors.Right);
            }
            else
            {
                //画面の左側
                ChangeCursor(App.Cursors.Left);

                //半透明の前後ページを表示しようとしてみた
                //if(g_pi.NowViewPage>=1)
                //overlay.bmp = g_pi.Items[g_pi.NowViewPage-1].cache.bmp;
                //if (!PicPanel.Controls.Contains(overlay))
                //{
                //    PicPanel.Controls.Add(overlay);
                //    overlay.BringToFront();
                //    overlay.BackColor = Color.Black;
                //    overlay.SetBounds(10, 10, 200, 200);
                //    overlay.alpha = 0.5f;
                //    overlay.isAutoFit = true;
                //    overlay.Show();
                //    overlay.Visible = true;
                //}
            }
        }

        private void ChangeCursor(Cursor newCursor)
        {
            if (newCursor != Cursor.Current)
            {
                Cursor.Current = newCursor;
            }
        }

        /// <summary>
        /// ルーペコントロール内をアップデート表示する
        /// マウスの位置によってルーペの内容が変わるので位置を引数として渡す
        /// </summary>
        /// <param name="mouseX">マウス位置</param>
        /// <param name="mouseY">マウス位置</param>
        /// <param name="cRect">クライアントサイズを示す</param>
        private void UpdateLoopeView(int mouseX, int mouseY, Rectangle cRect)
        {
            //ver1.27 マウス位置を補正
            float ratio = PicPanel.ZoomRatio;   //拡縮率をローカルに取る

            if (App.Config.isOriginalSizeLoupe    //原寸ルーペ設定が有効
                                                //&& viewPages == 1				//1ページ表示モード.コメントアウト2011年7月22日
                && ratio < 0.99F                //100%未満表示である.99%にしておく
                )
            {
                ////ver0.982
                ////オリジナルサイズ（原寸）でのルーペ表示
                //int _bmpHeight = g_originalSizeBitmap.Height;
                //int _bmpWidth = g_originalSizeBitmap.Width;
                ////TODO 簡易復旧
                //int x0 = (cRect.Width - (int)(_bmpWidth * ratio)) / 2 + cRect.Left;	//ver0.986サイドバー補正
                //int y0 = (cRect.Height - (int)(_bmpHeight * ratio)) / 2 + cRect.Top;
                ////縮小画像内でのマウス位置を計算
                //int x1 = mouseX - x0;
                //int y1 = mouseY - y0;
                ////TODO 簡易復旧
                //x1 = (int)(x1 / ratio);
                //y1 = (int)(y1 / ratio);
                ////原寸描写
                //loupe.DrawOriginalSizeLoupe(x1, y1, g_originalSizeBitmap);
                //loupe.Refresh();

                //ver1.27 左上座標に補正
                //元画像サイズをローカルに取る
                Bitmap screenImage = PicPanel.bmp;
                int _bmpHeight = screenImage.Height;
                int _bmpWidth = screenImage.Width;
                //縮尺画像の原点の表示位置を確認
                int x0 = (PicPanel.Width - (int)(_bmpWidth * ratio)) / 2;
                int y0 = (PicPanel.Height - (int)(_bmpHeight * ratio)) / 2;
                ////縮小画像内でのマウス位置を計算
                int x1 = mouseX - x0;
                int y1 = mouseY - y0;
                //始点を算出
                double sx = (double)x1 / ratio - mouseX;
                double sy = (double)y1 / ratio - mouseY;
                //左上始点指定版のルーペ
                loupe.DrawOriginalSizeLoupe2((int)sx, (int)sy, screenImage);
                loupe.Refresh();
            }
            else
            {
                //unsafe版高速n倍ルーペ
                //画面キャプチャに対するルーペを実施。
                //ツールバー分を補正
                //loupe.DrawLoupeFast2(mouseX - cRect.Left, mouseY - cRect.Top);	//ver0.986サイドバー補正

                //ver1.27 左上座標に補正
                double mag = App.Config.loupeMagnifcant;
                double x9 = ((mag - 1.0d) / mag) * (double)mouseX;
                double y9 = ((mag - 1.0d) / mag) * (double)mouseY;
                loupe.DrawLoupeFast3((int)x9, (int)y9);
                loupe.Refresh();
            }
        }

        private void OpenLoope(Rectangle cRect)
        {
            //サムネイル作成を停止
            //PauseThumbnailMakerThread();

            //ルーペのサイズを決める。
            //HACK: 全画面ルーペ test code
            //ver0.990 2011/07/21全画面ルーペ
            int dx = cRect.Width;
            int dy = cRect.Height;
            int mag = App.Config.loupeMagnifcant;

            loupe = new Loupe(this, dx, dy, mag);
            this.Controls.Add(loupe);

            //ルーペの位置を決める。縦方向は上
            //loupe.Top = GetClientRectangle().Top;
            //loupe.Left = GetClientRectangle().Left;	// =0と想定
            loupe.Top = cRect.Top;
            loupe.Left = cRect.Left;

            //表示させる
            if (loupe.Visible == false)
                loupe.Visible = true;

            //ステータスバー表示
            if (App.Config.isOriginalSizeLoupe    //原寸ルーペ設定が有効
                && PicPanel.ZoomRatio < 1.0F)           //表示倍率が100%以下
            {
                setStatubarRatio("ルーペ（100%表示）");
            }
            else
            {
                //%表示
                setStatubarRatio(
                    string.Format("ルーペ:{0}倍（{1,0:p1}表示）",
                        App.Config.loupeMagnifcant,
                        (double)(PicPanel.ZoomRatio * App.Config.loupeMagnifcant)
                        ));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            //ルーペが存在し、且つ右クリックだったか
            if (loupe != null && e.Button == MouseButtons.Right)
            {
                //ルーペを終了
                CloseLoupe();
                Cursor.Current = Cursors.Default;
                PicPanel.Visible = true;
            }
            else if (e.Button == MouseButtons.Right && !App.Config.isThumbnailView)
            {
                //右クリックで且つサムネイル中ではないので
                //コンテキストメニューを表示する
                contextMenuStrip1.Show(this.PointToScreen(e.Location));
            }

            //2011年9月11日 左クリックの場合
            //ドラッグスクロールの中止
            // OnClockでやっているので不要
            if (e.Button == MouseButtons.Left)
            {
                g_LastClickPoint = Point.Empty;
            }
        }

        private void CloseLoupe()
        {
            loupe.Close();
            this.Controls.Remove(loupe);
            loupe.Dispose();
            loupe = null;

            //サムネイル作成を再開する
            //ResumeThumbnailMakerThread();
        }

        /// <summary>
        /// 他のクラス（sidebar)からキー入力を転送するために使う
        /// いらないかも
        /// </summary>
        /// <param name="e"></param>
        //public void TransferKeyDown(KeyEventArgs e)
        //{
        //    OnKeyDown(e);
        //}

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    base.OnKeyDown(e);

        //    //Debug.WriteLine(e.KeyCode, "KeyCode");

        //    //スライドショー中だったら中断させる
        //    if (isSlideShow)
        //    {
        //        StopSlideShow();
        //        return;
        //    }

        //    //Altキーは特別な動作
        //    if (e.KeyCode == Keys.Menu && !menuStrip1.Visible)
        //    {
        //        menuStrip1.Visible = true;
        //        AjustSidebarArrangement();
        //        return;
        //    }

        //    //キー毎のメソッドを実行
        //    MethodInvoker func = null;
        //    if (KeyMethods.TryGetValue(e.KeyCode, out func))
        //        func();

        //}

        /// <summary>
        /// サムネイル画面時のマウス移動
        /// 全画面時のツールバーの表示を制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void g_ThumbPanel_MouseMove(object sender, MouseEventArgs e)
        {
            //全画面モードの時にツールバーを表示するか
            if (App.Config.isFullScreen)
            {
                //全画面表示中。表示非表示を切り替え
                if (e.Y < 1
                    && toolStrip1.Visible == false)
                {
                    //Cursor.Current = Cursors.Default;
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > this.Height - 10 && toolStrip1.Visible == false)
                {
                    //画面下でも動作するようにする
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > toolStrip1.Height
                    && toolStrip1.Visible == true)
                {
                    toolStrip1.Visible = false;
                    statusbar.Visible = false;
                }
            }
        }
    }
}
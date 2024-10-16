﻿using Marmi.Models;
using System.Drawing;
using System.Threading.Tasks;
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
        #region D&D

        protected override async void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            Uty.DebugPrint("Start");

            //ドロップされた物がファイルかどうかチェック
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Formをアクティブ
                this.Activate();
                string[] files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];

                //2022年9月17日 非同期IOを中止
                await AsyncIO.ClearJobAndWaitAsync();

                //await StartAsync(files);
                StartAsync(files).FireAndForget();
            }
            Uty.DebugPrint("End");
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                drgevent.Effect = DragDropEffects.All;
            else
                drgevent.Effect = DragDropEffects.None;
        }

        #endregion D&D

        private async void PicPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (App.Config.Mouse.MouseConfigWheel == "拡大縮小")
            {
                //PicPanel内部で処理しているのでなにもしない
            }
            else
            {
                //サイドバーが生きていたら何もしない
                //if (!g_Sidebar.isMinimize)
                //    return;

                if (e.Delta > 0)
                    await NavigateToBackAsync();
                else
                    await NavigateToForwordAsync();
            }
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            //スライドショー中だったら中断させる
            if (IsSlideShow)
            {
                StopSlideShow();
                return;
            }

            //右クリック
            //Form1_MouseUp()でやっているので無視
            if (e.Button == MouseButtons.Right)
                return;

            //X1ボタン
            if (e.Button == MouseButtons.XButton1)
            {
                await ExecuteXButtonCommandAsync(App.Config.Mouse.X1Behavior);
                return;
            }

            //X2ボタン
            if (e.Button == MouseButtons.XButton2)
            {
                await ExecuteXButtonCommandAsync(App.Config.Mouse.X2Behavior);
                return;
            }

            //ドラッグによるスクロール中であればクリックイベントは無視
            if (!g_LastClickPoint.IsEmpty)
            {
                Uty.DebugPrint("OnMouseClick()どうしてここに来るのか不明");
                g_LastClickPoint = Point.Empty;
                return;
            }

            //ファイルが1つもなければメッセージを表示
            if (App.g_pi.Items.Count == 0)
            {
                //ファイルをD&Dしてくださいと表示
                _clearPanel.ShowAndClose("ファイルをドロップしてください", 1000);
                return;
            }

            //ver1.80キー設定に基づく動作
            if (e.Button == MouseButtons.Middle
                && KeyDefines.TryGetValue(Keys.MButton, out MethodInvoker func))
            {
                func?.Invoke();
                return;
            }
            if (e.Button == MouseButtons.XButton1
                && KeyDefines.TryGetValue(Keys.XButton1, out func))
            {
                func?.Invoke();
                return;
            }
            if (e.Button == MouseButtons.XButton2
                && KeyDefines.TryGetValue(Keys.XButton2, out func))
            {
                func?.Invoke();
                return;
            }

            //ページナビゲートをする。
            bool isForword = PicPanel.CheckMousePosRight();
            //コンフィグ確認
            if (!App.Config.Mouse.ClickRightToNextPic)
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
                await NavigateToForwordAsync();
            }
            else
            {
                await NavigateToBackAsync();
            }
        }

        /// <summary>
        /// ダブルクリック処理
        /// シングルクリックの2回目と見なして処理する
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            //ver1.80 全画面をダブルクリックで対応するオプション導入
            if (App.Config.Mouse.DoubleClickToFullscreen)
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
            if (ViewState.FullScreen)
            {
                //全画面表示中。表示非表示を切り替え
                if (e.Y < 10 && !toolStrip1.Visible)
                {
                    //画面最上部にマウスがあれば表示
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > this.Height - 10 && !toolStrip1.Visible)
                {
                    //画面下でも動作するようにする
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > toolStrip1.Height && toolStrip1.Visible && !ViewState.ThumbnailView)
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
            if (loupe?.Visible == true)
            {
                //ルーペ中
                ChangeCursor(App.Cursors.Loupe);
            }
            else if (!g_LastClickPoint.IsEmpty)
            {
                //ドラッグスクロール中
                ChangeCursor(App.Cursors.OpenHand);
            }
            else if (App.g_pi.Items.Count <= 1 || !GetClientRectangle().Contains(cursorPos))
            {
                Cursor.Current = Cursors.Default;
            }
            else if (PicPanel.CheckMousePosRight())
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
            else if (e.Button == MouseButtons.Right && !ViewState.ThumbnailView)
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

        /// <summary>
        /// サムネイル画面時のマウス移動
        /// 全画面時のツールバーの表示を制御
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThumbPanel_MouseMove(object sender, MouseEventArgs e)
        {
            //全画面モードの時にツールバーを表示するか
            if (ViewState.FullScreen)
            {
                //全画面表示中。表示非表示を切り替え
                if (e.Y < 1 && !toolStrip1.Visible)
                {
                    //Cursor.Current = Cursors.Default;
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > this.Height - 10 && !toolStrip1.Visible)
                {
                    //画面下でも動作するようにする
                    toolStrip1.Visible = true;
                    statusbar.Visible = true;
                }
                else if (e.Y > toolStrip1.Height && toolStrip1.Visible)
                {
                    toolStrip1.Visible = false;
                    statusbar.Visible = false;
                }
            }
        }

        private async Task ExecuteXButtonCommandAsync(string behavior)
        {
            //サムネイルモードなら何もしない
            if (ViewState.ThumbnailView)
                return;

            if (string.IsNullOrEmpty(behavior))
                return;

            switch (behavior)
            {
                case "次のページ":
                    await NavigateToForwordAsync();
                    break;

                case "前のページ":
                    await NavigateToBackAsync();
                    break;

                case "複数ページ進む":
                    await NavigateToForwordMultiPageAsync();
                    break;

                case "複数ページ戻る":
                    await NavigateToBackwordMultiPageAsync();
                    break;

                case "2ページモード切替":
                    await SetDualViewModeAsync(!ViewState.DualView);
                    break;

                case "回転":
                    ToolStripButton_Rotate_Click(null, null);
                    break;

                case "フルスクリーン":
                    ToggleFullScreen();
                    break;

                case "最小化":
                    ToggleFormSizeMinNormal();
                    break;

                case "しおりOn/Off":
                    ToggleBookmark();
                    break;

                case "サムネイルモード":
                    Menu_ViewThumbnail_Click(null, null);
                    break;
            }
        }
    }
}
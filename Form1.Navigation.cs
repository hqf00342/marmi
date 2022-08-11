using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
画面遷移関連のメソッド
*/

namespace Marmi
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// 指定したインデックスの画像を表示する。
        /// publicになっている理由はサイドバーやサムネイル画面からの
        /// 呼び出しに対応するため。
        /// 前のページに戻らないようにdrawOrderTickを導入
        /// </summary>
        /// <param name="index">インデックス番号</param>
        /// <param name="drawOrderTick">描写順序を示すオーダー時間 = DateTime.Now.Ticks</param>
        public async Task SetViewPageAsync(int index, long drawOrderTick = 0)
        {
            if (drawOrderTick == 0)
                drawOrderTick = DateTime.Now.Ticks;

            //ver1.09 オプションダイアログを閉じると必ずここに来ることに対するチェック
            if (App.g_pi.Items == null || App.g_pi.Items.Count == 0)
                return;

            //ver1.36 Index範囲チェック
            Debug.Assert(index >= 0 && index < App.g_pi.Items.Count);

            // ページ進行方向 進む方向なら正、戻る方向なら負
            // アニメーションで利用する
            int pageDirection = index - App.g_pi.NowViewPage;

            //ページ番号を更新
            App.g_pi.NowViewPage = index;
            _trackbar.Value = index;

            //ver1.35 スクリーンキャッシュチェック
            if (ScreenCache.Dic.TryGetValue(index, out Bitmap screenImage))
            {
                //スクリーンキャッシュあったのですぐに描写
                SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
                Debug.WriteLine(index, "Use ScreenCache");
            }
            else
            {
                //ver1.50
                //Keyだけある{key,null}キャッシュだったら消す。稀に発生するため
                if (ScreenCache.Dic.ContainsKey(index))
                    ScreenCache.Remove(index);

                //ver1.50 読み込み中と表示
                SetStatusbarInfo("Now Loading ... " + (index + 1).ToString());

                //画像作成をスレッドプールに登録
                var img = await Bmp.MakeOriginalSizeImageAsync(index);
                if (img == null)
                {
                    PicPanel.Message = "読込みに時間がかかってます.リロードしてください";
                }
                else
                {
                    SetViewPage2(index, pageDirection, img, drawOrderTick);
                }

                //カーソルをWaitに
                //this.Cursor = Cursors.WaitCursor;
            }

            //回転情報を適用
            var rotate = App.g_pi.Items[App.g_pi.NowViewPage].Rotate;
            if (rotate != 0)
            {
                PicPanel.Rotate(rotate);
            }
        }

        private void SetViewPage2(int index, int pageDirection, Bitmap screenImage, long orderTime)
        {
            //ver1.55 drawOrderTickのチェック.
            // スレッドプールに入るため稀に順序が前後する。
            // 最新の描写でなければスキップ
            if (PicPanel.DrawOrderTime > orderTime)
            {
                Debug.WriteLine($"Skip SetViewPage2({index}) too old order={orderTime} < now={PicPanel.DrawOrderTime}");
                return;
            }

            //描写開始
            PicPanel.State = DrawStatus.drawing;
            PicPanel.DrawOrderTime = orderTime;

            if (screenImage == null)
            {
                Debug.WriteLine($"bmpがnull(index={index})");
                PicPanel.State = DrawStatus.idle;
                PicPanel.Message = "表示エラー 再度表示してみてください" + index.ToString();
                PicPanel.Refresh();
                return;
            }

            //ver1.50 表示
            PicPanel.State = DrawStatus.drawing;
            PicPanel.Message = string.Empty;
            if (App.Config.View.PictureSwitchMode != AnimateMode.none  //アニメーションモードである
                && !App.Config.KeepMagnification                  //倍率固定モードではアニメーションしない
                && pageDirection != 0)
            {
                //スライドインアニメーション
                PicPanel.AnimateSlideIn(screenImage, pageDirection);
            }

            PicPanel.Bmp = screenImage;
            PicPanel.ResetView();
            PicPanel.FastDraw = false;

            //ver1.78 倍率をオプション指定できるように変更
            if (!App.Config.KeepMagnification     //倍率維持モードではない
                || IsFitToScreen)             //画面にフィットしている
            {
                //画面切り替わり時はフィットモードで起動
                float r = PicPanel.FittingRatio;
                if (r > 1.0f && App.Config.View.NoEnlargeOver100p)
                    r = 1.0f;
                PicPanel.ZoomRatio = r;
            }

            //ページを描写
            PicPanel.AjustViewAndShow();

            //1ページ表示か2ページ表示か
            g_viewPages = (int)screenImage.Tag;
            PicPanel.State = DrawStatus.idle;

            //カーソルを元に戻す
            this.Cursor = Cursors.Default;

            //UI更新
            UpdateStatusbar();
            UpdateToolbar();

            //サイドバーでアイテムを中心に
            if (_sidebar.Visible)
                _sidebar.SetItemToCenter(App.g_pi.NowViewPage);

            needMakeScreenCache = true;

            //PicPanel.Message = string.Empty;
            PicPanel.State = DrawStatus.idle;
            //2021年2月26日 GCをやめる
            //ToDo:ここだけはあったほうがいいかもしれないがLOHの扱いも同時にすべき
            //Uty.ForceGC();
        }

        #region Navigation

        private async Task NavigateToBackAsync()
        {
            //前に戻る
            long drawOrderTick = DateTime.Now.Ticks;
            int prev = await GetPrevPageIndex(App.g_pi.NowViewPage);
            if (prev >= 0)
            {
                await SetViewPageAsync(prev, drawOrderTick);
            }
            else
                _clearPanel.ShowAndClose("先頭のページです", 1000);
        }

        private async Task NavigateToForwordAsync()
        {
            //ver1.35 ループ機能を実装
            long drawOrderTick = DateTime.Now.Ticks;
            int now = App.g_pi.NowViewPage;
            int next = await GetNextPageIndexAsync(App.g_pi.NowViewPage);
            Debug.WriteLine($"NavigateToForword() {now} -> {next}");
            if (next >= 0)
            {
                await SetViewPageAsync(next, drawOrderTick);
            }
            else if (App.Config.View.LastPage_toTop)
            {
                //先頭ページへループ
                await SetViewPageAsync(0, drawOrderTick);
                _clearPanel.ShowAndClose("先頭ページに戻りました", 1000);
            }
            else
            {
                _clearPanel.ShowAndClose("最後のページです", 1000);
            }
        }

        /// <summary>
        /// 最終ページを見ているかどうか確認。２ページ表示に対応
        /// 先頭ページはそのまま０かどうかチェックするだけなので作成しない。
        /// </summary>
        /// <returns>最終ページであればtrue</returns>
        private bool IsLastPageViewing()
        {
            if (string.IsNullOrEmpty(App.g_pi.PackageName))
                return false;
            if (App.g_pi.Items.Count <= 1)
                return false;
            return App.g_pi.NowViewPage + g_viewPages >= App.g_pi.Items.Count;
        }

        //ver1.35 前のページ番号。すでに先頭ページなら-1
        internal static async Task<int> GetPrevPageIndex(int index)
        {
            if (index > 0)
            {
                int declimentPages = -1;
                //2ページ減らすことが出来るか
                if (await CanDualViewAsync(App.g_pi.NowViewPage - 2))
                    declimentPages = -2;

                int ret = index + declimentPages;
                return ret >= 0 ? ret : 0;
            }
            else
            {
                //すでに先頭ページなので-1を返す
                return -1;
            }
        }

        //ver1.36次のページ番号。すでに最終ページなら-1
        internal static async Task<int> GetNextPageIndexAsync(int index)
        {
            int pages = await CanDualViewAsync(index) ? 2 : 1;

            if (index + pages <= App.g_pi.Items.Count - 1)
            {
                return index + pages;
            }
            else
            {
                //最終ページ
                return -1;
            }
        }

        #endregion Navigation

    }
}

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
        //高速キー入力に対応するため、最後のオーダーを保存する
        private long _lastDrawOrderTick = 0;

        /// <summary>
        /// 指定したインデックスの画像を表示する。
        /// publicになっている理由はサイドバーやサムネイル画面からの
        /// 呼び出しに対応するため。
        /// 前のページに戻らないようにdrawOrderTickを導入
        /// </summary>
        /// <param name="index">インデックス番号</param>
        public async Task SetViewPageAsync(int index)
        {
            var drawOrderTick = DateTime.Now.Ticks;
            _lastDrawOrderTick = drawOrderTick;

            //ver1.09 オプションダイアログを閉じると必ずここに来ることに対するチェック
            if (App.g_pi.Items == null || App.g_pi.Items.Count == 0)
                return;

            //ver1.36 Index範囲チェック
            Debug.Assert(index >= 0 && index < App.g_pi.Items.Count);

            if (index < 0)
                index = 0;

            if (index > App.g_pi.Items.Count - 1)
                index = App.g_pi.Items.Count - 1;

            // ページ進行方向 進む方向なら正、戻る方向なら負
            // アニメーションで利用する
            int pageDirection = index - App.g_pi.NowViewPage;

            //ページ番号を更新
            App.g_pi.NowViewPage = index;
            _trackbar.Value = index;

            //ver1.35 スクリーンキャッシュチェック
            if (App.Config.UseScreenCache && ScreenCache.Dic.TryGetValue(index, out Bitmap screenImage))
            {
                //スクリーンキャッシュあったのですぐに描写
                SetViewPage2(index, pageDirection, screenImage, drawOrderTick);
            }
            else
            {
                //ver1.50 読み込み中と表示
                SetStatusbarInfo("Now Loading ... " + (index + 1).ToString());

                //画像作成
                var img = await Bmp.MakeOriginalSizeImageAsync(index);
                if (img == null)
                {
                    PicPanel.Message = "読込みに時間がかかってます.リロードしてください";
                }
                else
                {
                    SetViewPage2(index, pageDirection, img, drawOrderTick);
                }
            }

            //回転情報を適用
            var rotate = App.g_pi.Items[App.g_pi.NowViewPage].Rotate;
            if (rotate != 0)
            {
                PicPanel.Rotate(rotate);
            }
        }

        /// <summary>
        /// 実際のPicPanelへの描写を実施する
        /// </summary>
        /// <param name="index">ページ番号</param>
        /// <param name="pageDirection">ページ方向。アニメーションに使う</param>
        /// <param name="screenImage">表示するBitmap</param>
        /// <param name="orderTick">オーダー時間。古い描写指示を落とすために利用</param>
        private void SetViewPage2(int index, int pageDirection, Bitmap screenImage, long orderTick)
        {
            //ver1.55 最新描写でなければスキップ
            if (_lastDrawOrderTick > orderTick)
            {
                Uty.DebugPrint($"Skip draw(too old). index={index}]");
                return;
            }

            if (screenImage == null)
            {
                Uty.DebugPrint($"bmpがnull(index={index})");
                PicPanel.Message = $"表示エラー 再度表示してみてください (index={index})";
                PicPanel.Refresh();
                return;
            }

            //ver1.50 表示
            PicPanel.Message = string.Empty;
            if (App.Config.View.PageTransitionEffect == "アニメーション"
                && !App.Config.View.KeepMagnification
                && pageDirection != 0)
            {
                //スライドインアニメーション
                PicPanel.AnimateSlideIn(screenImage, pageDirection);
            }

            PicPanel.Bmp = screenImage;
            PicPanel.ResetZoomAndAlpha();
            PicPanel.FastDraw = false;

            //ver1.78 倍率をオプション指定できるように変更
            if (!App.Config.View.KeepMagnification     //倍率維持モードではない
                || PicPanel.IsFitToScreen)        //画面にフィットしている
            {
                //画面切り替わり時はフィットモードで起動
                float r = PicPanel.JustFitRatio;
                if (r > 1.0f && App.Config.View.ProhigitExpansionOver100p)
                    r = 1.0f;
                PicPanel.ZoomRatio = r;
            }

            //ページを描写
            PicPanel.AjustViewAndShow();

            //1ページ表示か2ページ表示か
            g_viewPages = (int)screenImage.Tag;

            //カーソルを元に戻す
            this.Cursor = Cursors.Default;

            //UI更新
            UpdateStatusbar();
            UpdateToolbar();

            //サイドバーでアイテムを中心に
            if (_sidebar.Visible)
                _sidebar.SetItemToCenter(App.g_pi.NowViewPage);

            needMakeScreenCache = true;
        }

        #region Navigation

        private async Task NavigateToBackAsync()
        {
            //前に戻る
            int prev = await GetPrevPageIndex(App.g_pi.NowViewPage);
            if (prev >= 0)
            {
                await SetViewPageAsync(prev);
            }
            else
                _clearPanel.ShowAndClose("先頭のページです", 1000);
        }

        private async Task NavigateToForwordAsync()
        {
            //ver1.35 ループ機能を実装
            int now = App.g_pi.NowViewPage;
            int next = await GetNextPageIndexAsync(App.g_pi.NowViewPage);
            Uty.DebugPrint($"NavigateToForword() {now} -> {next}");
            if (next >= 0)
            {
                await SetViewPageAsync(next);
            }
            else if (App.Config.View.MoveToTopAtLastPage)
            {
                //先頭ページへループ
                await SetViewPageAsync(0);
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
                if (await Bmp.CanDualViewAsync(App.g_pi.NowViewPage - 2))
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
            int pages = await Bmp.CanDualViewAsync(index) ? 2 : 1;

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

        /// <summary>
        /// ページを複数ページ進める
        /// </summary>
        /// <returns></returns>
        private async Task NavigateToForwordMultiPageAsync()
        {
            var nextPage = App.g_pi.NowViewPage + App.Config.General.MultiPageNavigationCount;
            if (nextPage > App.g_pi.Items.Count - 1)
            {
                nextPage = App.g_pi.Items.Count - 1;
            }
            await SetViewPageAsync(nextPage);
        }

        /// <summary>
        /// ページを複数ページ戻る
        /// </summary>
        /// <returns></returns>
        private async Task NavigateToBackwordMultiPageAsync()
        {
            var nextPage = App.g_pi.NowViewPage - App.Config.General.MultiPageNavigationCount;
            if (nextPage < 0)
            {
                nextPage = 0;
            }
            await SetViewPageAsync(nextPage);
        }

        #endregion Navigation
    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public static class Bmp
    {
        /// <summary>
        /// 非同期でBitmapを読み込む。
        /// 実質にはAsyncIOスレッド内で実行される。
        /// 読み込むとキャッシュに保存される。
        /// 読み込み終わった後に実行するuiActionを設定可能。
        /// </summary>
        /// <param name="index">対象画像ののインデックス番号</param>
        /// <param name="uiAction">読み込み完了後に実行するAction</param>
        public static void AsyncGetBitmap(int index, Action uiAction)
        {
            //キャッシュを持っていれば非同期しない
            if (App.g_pi.HasCacheImage(index))
            {
                uiAction?.Invoke();
            }

            //ver1.54 HighQueueとして登録されているかどうか確認する。
            //ToDo:2021年2月22日 この方法ではuiActionが実行されない。
            var array = AsyncIO.GetAllJob();
            foreach (var elem in array)
            {
                if (elem.Key == index)
                {
                    Debug.WriteLine($"AsyncGetBitmap() : Skip. {index} is already queued.");
                    return;
                }
            }

            //非同期するためにPush
            AsyncIO.AddJob(index, uiAction);
        }

        /// <summary>
        /// Bitmapを取得する
        /// </summary>
        /// <param name="index">取得したいBitmapのIndex</param>
        /// <returns>Bitmap。失敗した場合はnull</returns>
        public static Bitmap SyncGetBitmap(int index)
        {
            var bmp = App.g_pi.GetBitmapFromCache(index);

            if (bmp != null)
            {
                return bmp;
            }
            else
            {
                bool asyncFinished = false;
                Stopwatch sw = Stopwatch.StartNew();
                AsyncGetBitmap(index, () => asyncFinished = true);

                while (!asyncFinished && sw.ElapsedMilliseconds < App.ASYNC_TIMEOUT)
                    Application.DoEvents();
                sw.Stop();

                if (sw.ElapsedMilliseconds < App.ASYNC_TIMEOUT)
                    return App.g_pi.GetBitmapFromCache(index);
                else
                {
                    Debug.WriteLine($"SyncGetBitmap({index}) timeOut");
                    return null;
                }
            }
        }

        /// <summary>
        /// Bitmapサイズを取得する
        /// Bitmap化しないだけ速いはず。
        /// </summary>
        /// <param name="index">取得したいBitmapのIndex</param>
        /// <returns>サイズ</returns>
        internal static Size SyncGetBitmapSize(int index)
        {
            if (App.g_pi.Items[index].HasInfo)
                return App.g_pi.Items[index].ImgSize;
            else
            {
                //非同期のGetBitmap()を読み終わるまで待つ
                bool asyncFinished = false;
                var sw = Stopwatch.StartNew();
                AsyncGetBitmap(index, () => asyncFinished = true);
                while (!asyncFinished && sw.ElapsedMilliseconds < App.ASYNC_TIMEOUT)
                    Application.DoEvents();
                sw.Stop();

                if (App.g_pi.Items[index].HasInfo)
                    return App.g_pi.Items[index].ImgSize;
                else
                    return Size.Empty;
            }
        }

        internal static Bitmap MakeOriginalSizeImage(int index)
        {
            Debug.WriteLine($"MakeOriginalSizeImage({index})");

            //とりあえず1枚読め！
            Bitmap bmp1 = SyncGetBitmap(index);
            if (bmp1 == null)
            {
                //if (g_pi.isSolid && App.Config.isExtractIfSolidArchive)
                //    PicPanel.Message = "画像ファイルを展開中です";
                //else
                //    PicPanel.Message = "読込みに時間がかかってます.リロードしてください";
                return null;
            }

            //ver1.81 サムネイル登録：2021年2月25日コメントアウト
            //App.g_pi.AsyncThumnailMaker(index, bmp1.Clone() as Bitmap);

            if (App.Config.dualView && Form1.CanDualView(index))
            {
                //2枚表示
                Bitmap bmp2 = SyncGetBitmap(index + 1);
                if (bmp2 == null)
                {
                    //2枚目の読み込みがエラーなので1枚表示にする
                    bmp1.Tag = 1;
                    return bmp1;
                }

                //ver1.81 サムネイル登録：2021年2月25日コメントアウト
                //App.g_pi.AsyncThumnailMaker(index + 1, bmp2.Clone() as Bitmap);

                //合成ページを作る
                int width1 = bmp1.Width;
                int width2 = bmp2.Width;
                int height1 = bmp1.Height;
                int height2 = bmp2.Height;
                Bitmap returnBmp = new Bitmap(
                    width1 + width2,
                    (height1 > height2) ? height1 : height2);

                using (Graphics g = Graphics.FromImage(returnBmp))
                {
                    g.Clear(App.Config.BackColor);
                    if (App.g_pi.PageDirectionIsLeft)
                    {
                        //左から右へ
                        //2枚目(左）を描写
                        g.DrawImage(bmp2, 0, 0, width2, height2);
                        //1枚目（右）を描写
                        g.DrawImage(bmp1, width2, 0, width1, height1);
                    }
                    else
                    {
                        //右から左へ
                        //2枚目(左）を描写
                        g.DrawImage(bmp1, 0, 0, width1, height1);
                        //1枚目（右）を描写
                        g.DrawImage(bmp2, width1, 0, width2, height2);
                    }
                }
                bmp1.Dispose();
                bmp2.Dispose();
                returnBmp.Tag = 2;
                return returnBmp;
            }
            else
            {
                //1枚表示
                bmp1.Tag = 1;
                return bmp1;
            }
        }
    }
}
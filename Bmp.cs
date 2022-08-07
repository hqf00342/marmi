using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Marmi
{
    public static class Bmp
    {
        /// <summary>
        /// Bitmapを取得
        /// </summary>
        /// <param name="ix">画像番号</param>
        /// <returns>Bitmap</returns>
        public static async Task<Bitmap> GetBitmapAsync(int ix, bool highPriority)
        {
            await LoadBitmapAsync(ix, highPriority);
            return App.g_pi.Items[ix].CacheImage.ToBitmap();
        }

        /// <summary>
        /// Bitmapを非同期で読み込み、キャッシュに保存
        /// RawCacheとして持ったままとし、Bitmap化はしない。
        /// 画像サイズが欲しいときに利用する。
        /// </summary>
        /// <param name="ix"></param>
        /// <returns>なし</returns>
        public static async Task LoadBitmapAsync(int ix, bool highPriority)
        {
            if (App.g_pi.Items[ix].CacheImage.HasImage)
                return;

            bool complete = false;

            if(highPriority)
                AsyncIO.AddJob(ix, () => { complete = true; });
            else
                AsyncIO.AddJobLow(ix, () => { complete = true; });

            await Task.Yield();
            while (!complete)
            {
                await Task.Delay(10);
            }
        }

        /// <summary>
        /// 原寸サイズの画像を生成
        /// 2枚表示する場合は2画像を結合する。
        /// </summary>
        /// <param name="index">画像番号</param>
        /// <returns></returns>
        internal static async Task<Bitmap> MakeOriginalSizeImageAsync(int index)
        {
            Debug.WriteLine($"MakeOriginalSizeImage({index})");

            //とりあえず1枚読め！
            var bmp1 = await GetBitmapAsync(index, true);
            if (bmp1 == null)
            {
                return null;
            }

            if (App.Config.DualView && await Form1.CanDualViewAsync(index))
            {
                //2枚表示
                Bitmap bmp2 = await GetBitmapAsync(index + 1, true);
                if (bmp2 == null)
                {
                    //2枚目の読み込みがエラーなので1枚表示にする
                    bmp1.Tag = 1;
                    return bmp1;
                }

                //2枚合成ページを作る
                var returnBmp = new Bitmap(
                    bmp1.Width + bmp2.Width,
                    (bmp1.Height > bmp2.Height) ? bmp1.Height : bmp2.Height);

                using (var g = Graphics.FromImage(returnBmp))
                {
                    g.Clear(App.Config.General.BackColor);
                    if (App.g_pi.PageDirectionIsLeft)
                    {
                        //左から右へ
                        //2枚目(左）を描写
                        g.DrawImage(bmp2, 0, 0, bmp2.Width, bmp2.Height);
                        //1枚目（右）を描写
                        g.DrawImage(bmp1, bmp2.Width, 0, bmp1.Width, bmp1.Height);
                    }
                    else
                    {
                        //右から左へ
                        //2枚目(左）を描写
                        g.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);
                        //1枚目（右）を描写
                        g.DrawImage(bmp2, bmp1.Width, 0, bmp2.Width, bmp2.Height);
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
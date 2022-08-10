﻿using System;
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
            var bmp = App.g_pi.Items[ix].CacheImage.ToBitmap();
            if (bmp == null)
                throw new Exception("画像取得に失敗");
            return bmp;
        }

        /// <summary>
        /// Bitmapを非同期で読み込み、キャッシュに保存
        /// RawCacheとして持ったままとし、Bitmap化はしない。
        /// 画像サイズが欲しいときに利用する。
        /// </summary>
        /// <param name="ix"></param>
        /// <returns>戻り値は使わない。.NET Frameworkにはジェネリック版しかない</returns>
        public static Task<bool> LoadBitmapAsync(int ix, bool highPriority)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (App.g_pi.Items[ix].CacheImage.HasImage)
            {
                tcs.SetResult(true);
            }
            else
            {
                if (highPriority)
                    AsyncIO.AddJob(ix, () => { tcs.SetResult(true); });
                else
                    AsyncIO.AddJobLow(ix, () => { tcs.SetResult(true); });
            }
            return tcs.Task;
        }

        /// <summary>
        /// 非同期でBitmapを読み込む。
        /// await不要でタスクを積むだけでよい場合はこれを使う。
        /// サムネイルを作る、画像サイズを取得するだけならこれでいい。
        /// Taskオブジェクトを作らない分、軽いはず。
        /// </summary>
        /// <param name="ix"></param>
        public static void LoadBitmapAndForget(int ix)
        {
            if (!App.g_pi.Items[ix].CacheImage.HasImage)
            {
                AsyncIO.AddJobLow(ix, null);
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
                Debug.WriteLine("GetBitmapAsync()からnullがきた。");
                throw new InvalidOperationException("nullはおかしい");
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
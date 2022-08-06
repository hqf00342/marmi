using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Marmi
{
    internal static class ScreenCache
    {
        //ver1.35 スクリーンキャッシュ
        private static Dictionary<int, Bitmap> _screenCache = new Dictionary<int, Bitmap>();

        public static IReadOnlyDictionary<int, Bitmap> Dic => _screenCache;

        /// <summary>
        /// 前後ページの画面キャッシュを作成する
        /// 現在見ているページを中心とする
        /// </summary>
        internal static void MakeCacheForPreAndNextPages()
        {
            //ver1.37 スレッドで使うことを前提にロック
            lock ((_screenCache as ICollection).SyncRoot)
            {
                //前のページ
                int ix = Form1.GetPrevPageIndex(App.g_pi.NowViewPage);
                if (ix >= 0 && !_screenCache.ContainsKey(ix))
                {
                    Debug.WriteLine(ix, "getScreenCache() Add Prev");
                    var bmp = Bmp.MakeOriginalSizeImage(ix);
                    if (bmp != null)
                        _screenCache.Add(ix, bmp);
                }

                //前のページ
                ix = Form1.GetNextPageIndex(App.g_pi.NowViewPage);
                if (ix >= 0 && !_screenCache.ContainsKey(ix))
                {
                    Debug.WriteLine(ix, "getScreenCache() Add Next");
                    var bmp = Bmp.MakeOriginalSizeImage(ix);
                    if (bmp != null)
                        _screenCache.Add(ix, bmp);
                }
            }
        }

        /// <summary>
        /// 不要なスクリーンキャッシュを削除する
        /// </summary>
        internal static void Purge()
        {
            //削除候補をリストアップ
            int now = App.g_pi.NowViewPage;
            const int DISTANCE = 2;
            List<int> deleteCandidate = new List<int>();

            foreach (var ix in _screenCache.Keys)
            {
                if (ix > now + DISTANCE || ix < now - DISTANCE)
                {
                    deleteCandidate.Add(ix);
                }
            }

            //削除候補を削除する
            if (deleteCandidate.Count > 0)
            {
                foreach (int ix in deleteCandidate)
                {
                    //先に消してはだめ！
                    //ディクショナリから削除した後BitmapをDispose()
                    //Bitmap tempBmp = ScreenCache[key];
                    //ScreenCache.Remove(key);
                    //tempBmp.Dispose();
                    if (_screenCache.TryGetValue(ix, out Bitmap tempBmp))
                    {
                        _screenCache.Remove(ix);
                        Debug.WriteLine($"PurgeScreenCache({ix})");
                    }
                    else
                    {
                        Debug.WriteLine($"PurgeScreenCache({ix})失敗");
                    }
                }
            }
            //ver1.37GC
            //Uty.ForceGC();
        }

        internal static void Clear() => _screenCache.Clear();

        internal static void Remove(int index) => _screenCache.Remove(index);
    }
}

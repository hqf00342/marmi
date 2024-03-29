﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Marmi
{
    internal static class ScreenCache
    {
        //ver1.35 スクリーンキャッシュ
        private static readonly Dictionary<int, Bitmap> _screenCache = new Dictionary<int, Bitmap>();

        public static IReadOnlyDictionary<int, Bitmap> Dic => _screenCache;

        /// <summary>
        /// 前後ページの画面キャッシュを作成する
        /// 現在見ているページを中心とする
        /// </summary>
        internal static async Task MakeCacheForPreAndNextPagesAsync()
        {
            //ver1.37 スレッドで使うことを前提にロック
            //前のページ
            int ix = await Form1.GetPrevPageIndex(App.g_pi.NowViewPage);
            if (ix >= 0 && !_screenCache.ContainsKey(ix))
            {
                Debug.WriteLine(ix, "getScreenCache() Add Prev");
                var bmp = await Bmp.MakeOriginalSizeImageAsync(ix);
                AddImage(ix, bmp);
            }

            //前のページ
            ix = await Form1.GetNextPageIndexAsync(App.g_pi.NowViewPage);
            if (ix >= 0 && !_screenCache.ContainsKey(ix))
            {
                Debug.WriteLine(ix, "getScreenCache() Add Next");
                var bmp = await Bmp.MakeOriginalSizeImageAsync(ix);
                AddImage(ix, bmp);
            }
        }

        private static void AddImage(int ix, Bitmap bmp)
        {
            if (bmp == null) return;
            lock ((_screenCache as ICollection).SyncRoot)
            {
                try
                {
                    _screenCache.Add(ix, bmp);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                    //高速画面遷移されるとどうしても同じタイミングで
                    //キャッシュ追加されることがあるので
                    //Key重複例外は無視
                }
            }
        }

        /// <summary>
        /// 不要なスクリーンキャッシュを削除する
        /// </summary>
        internal static void Purge()
        {
            int now = App.g_pi.NowViewPage;
            const int DISTANCE = 2;
            List<int> delList = new List<int>();

            //削除候補をリストアップ
            foreach (var ix in _screenCache.Keys)
            {
                if (ix > now + DISTANCE 
                    || ix < now - DISTANCE
                    || _screenCache[ix] == null)
                {
                    delList.Add(ix);
                }
            }

            //削除候補を削除する
            if (delList.Count > 0)
            {
                foreach (int ix in delList)
                {
                    //先に消してはだめ！
                    if (_screenCache.ContainsKey(ix))
                    {
                        _screenCache.Remove(ix);
                        Debug.WriteLine($"PurgeScreenCache({ix})");
                    }
                }
            }
        }

        internal static void Clear() => _screenCache.Clear();

        internal static void Remove(int index) => _screenCache.Remove(index);
    }
}

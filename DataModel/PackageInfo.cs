using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

/*
閲覧対象の画像一覧クラス
シリアライズ対応
*/

namespace Marmi
{
    [Serializable]
    public class PackageInfo // : IDisposable
    {
        /// <summary>Zipファイル名、もしくはディレクトリ名</summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>現在見ているアイテム番号</summary>
        public int NowViewPage { get; set; } = 0;

        /// <summary>Zipファイルサイズ</summary>
        public long PackageSize { get; set; }

        /// <summary>Zipファイル作成日</summary>
        public DateTime CreateDate { get; set; }

        /// <summary>サムネイル画像集</summary>
        public List<ImageInfo> Items { get; } = new List<ImageInfo>();

        /// <summary>ver1.31 パッケージのタイプ</summary>
        public PackageType PackType { get; set; } = PackageType.None;

        /// <summary>ver1.30 ページ送り方向</summary>
        public bool PageDirectionIsLeft { get; set; } = true;

        /// <summary>ver1.09 書庫のモード</summary>
        [NonSerialized]
        public bool isSolid = false;

        /// <summary>書庫一時展開先ディレクトリ</summary>
        [NonSerialized]
        public string tempDirname = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PackageInfo()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化ルーチン
        /// </summary>
        public void Initialize()
        {
            PackageName = string.Empty;
            PackType = PackageType.None;
            isSolid = false;
            NowViewPage = 0;
            PackageSize = 0;
            CreateDate = DateTime.MinValue;
            tempDirname = string.Empty;

            PageDirectionIsLeft = true;

            //ファイルキャッシュをクリア
            foreach (var item in Items)
                item.CacheImage.Clear();

            Items.Clear();
        }

        /// <summary>
        /// ファイル名からインデックスを取得する
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>インデックス番号。無い場合は-1</returns>
        public int GetIndexFromFilename(string filename)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Filename == filename)
                    return i;
            return -1;
        }

        /// <summary>
        /// キャッシュにあるBitmapを返す
        /// 持っていなければnullを返す
        /// </summary>
        /// <param name="index">対象のインデックス</param>
        /// <returns>Bitmap 無ければcacheがなければnull</returns>
        public Bitmap GetBitmapFromCache(int index)
        {
            return Items[index].CacheImage.ToBitmap();
        }

        public void ClearCache(int index)
        {
            Items[index].CacheImage.Clear();
        }

        /// <summary>
        /// サムネイルの作成・登録
        /// ここ1か所で行う。(2021年2月25日)
        /// 現在はAsyncIOで行っているが本来はCache登録処理内で行いたい。
        /// </summary>
        /// <param name="index"></param>
        public void ThumnailMaker(int index)
        {
            if (index < 0 || index >= Items.Count) return;

            try
            {
                if (Items[index].CacheImage.HasImage)
                {
                    //ver1.10 サムネイル登録も行う
                    Bitmap _bmp = Items[index].CacheImage.ToBitmap();
                    if (_bmp != null)
                    {
                        Items[index].ResisterThumbnailImage(_bmp);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// FileCacheをパージする
        /// オーバーしていたらMaxの半分までクリアする。
        /// ・アイドル状態時
        /// ・AsyncLoadImageInfo()
        /// から呼び出されている。
        /// </summary>
        /// <param name="MaxCacheSize">Maxメモリ量[MB]</param>
        public void FileCacheCleanUp2(int MaxCacheSize)
        {
            MaxCacheSize *= 1_000_000;    //MB変換

            //すべてのキャッシュをクリアする
            //foreach (var i in Items)
            //    i.CacheImage.Clear();

            //現在のサイズを計算
            int nowBufferSize = Items.Sum(i => i.CacheImage.Length);

            if (nowBufferSize <= MaxCacheSize)
                return;

            //サイズオーバーしたのでMacCacheSizeの半分になるまで開放

            //現在の位置から上下にさかのぼっていく
            int now1 = NowViewPage;     //上にたどるポインタ
            int now2 = NowViewPage + 1; //下にたどるポインタ

            int sumBytes = 0;
            int halfSize = MaxCacheSize / 2;//半分のサイズまで解放

            while (now1 >= 0 || now2 < Items.Count)
            {
                //前方走査
                if (now1 >= 0)
                {
                    sumBytes += Items[now1].CacheImage.Length;
                    if (sumBytes > halfSize)
                    {
                        Items[now1].CacheImage.Clear();
                    }
                    now1--;
                }

                //後方走査
                if (now2 < Items.Count)
                {
                    sumBytes += Items[now2].CacheImage.Length;
                    if (sumBytes > halfSize)
                    {
                        Items[now2].CacheImage.Clear();
                    }
                    now2++;
                }
            }

            //2021年2月26日 GCをやめる
            //Uty.ForceGC();
        }

        /// <summary>
        /// ブックマーク一覧CSVを作成する
        /// </summary>
        /// <returns>CSV化されたブックマーク。ページ番号の羅列</returns>
        public string CreateBookmarkString()
        {
            var bookmarks = Items.Where(c => c.IsBookMark).Select(c => c.OrgIndex);
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

        /// <summary>
        /// ブックマークCSV文字列を読み込みブックマークとする
        /// </summary>
        /// <param name="csv">ブックマーク文字列。CreateBookmarkString()で生成されたもの</param>
        public void LoadBookmarkString(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return;

            //csvをInt配列に変換
            var bm = csv.Split(',').Select(c => { int.TryParse(c, out int r); return r; });

            //g_piに適用
            for (int i = 0; i < Items.Count; i++)
            {
                if (bm.Contains(Items[i].OrgIndex))
                    Items[i].IsBookMark = true;
            }
        }

        public void ThrowIfOutOfRange(int index)
        {
            if (index < 0 || index >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
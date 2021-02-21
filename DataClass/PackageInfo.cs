using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;	//Bookmarkの集計のために追加
using System.Threading;
/*
閲覧対象の画像一覧クラス
シリアライズ対応
*/
namespace Marmi
{
    [Serializable]
    public class PackageInfo // : IDisposable
    {
        //Zipファイル名、もしくはディレクトリ名
        public string PackageName { get; set; }

        //現在見ているアイテム番号
        public int NowViewPage { get; set; }

        //Zipファイルサイズ
        public long PackageSize { get; set; }

        //Zipファイル作成日
        public DateTime CreateDate { get; set; }

        //サムネイル画像集
        public List<ImageInfo> Items { get; } = new List<ImageInfo>();

        //ver1.31 パッケージのタイプ
        public PackageType PackType { get; set; } = PackageType.None;

        //ver1.30 ページ送り方向
        public bool PageDirectionIsLeft { get; set; }

        //ver1.09 書庫のモード
        [NonSerialized]
        public bool isSolid;

        //書庫一時展開先
        [NonSerialized]
        public string tempDirname;

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

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].Dispose(); //サムネイル画像をクリア
                Items.Clear();
            }

            //ファイルキャッシュをクリア
            foreach (var item in Items)
                item.cacheImage.Clear();
        }

        /// <summary>
        /// ファイル名からインデックスを取得する
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>インデックス番号。無い場合は-1</returns>
        public int GetIndexFromFilename(string filename)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].filename == filename)
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
            return hasCacheImage(index) ? Items[index].cacheImage.ToBitmap() : null;
        }

        public bool hasCacheImage(int index)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            return Items[index].cacheImage.hasImage;
        }

        /// <summary>
        /// キャッシュにファイルを読み込む
        /// </summary>
        /// <param name="index"></param>
        /// <param name="_7z"></param>
        /// <returns></returns>
        public bool LoadCache(int index, SevenZipWrapper _7z)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            string filename = Items[index].filename;

            if (PackType != PackageType.Archive)
            {
                //通常ファイルからの読み込み
                Items[index].cacheImage.Load(filename);
            }
            else if (isSolid && App.Config.isExtractIfSolidArchive)
            {
                //ver1.10 ソリッド書庫 一時フォルダから読み取りを試みる
                string tempname = Path.Combine(tempDirname, filename);
                Items[index].cacheImage.Load(tempname);
            }
            else
            {
                //ver1.05 Solid書庫ではない書庫ファイルモード
                try
                {
                    if (_7z == null)
                        _7z = new SevenZipWrapper();
                    if (_7z.Open(PackageName))
                    {
                        Items[index].cacheImage.Load(_7z.GetStream(filename));
                    }
                    else
                        return false;
                }
                catch (IOException e)
                {
                    //7zTemp展開中にアクセスされたケースと想定
                    //ファイルがなかったものとしてnullを返す
                    Debug.WriteLine(e.Message, "!Exception! " + e.TargetSite);
                    return false;
                }
            }
            //画像サイズを設定
            Items[index].bmpsize = Items[index].cacheImage.GetImageSize();

            //サムネイル登録はThreadPoolで
            //AsyncThumnailMaker(index);

            return true;
        }

        public void AsyncThumnailMaker(int index)
        {
            //ver1.73 index check
            if (index > Items.Count) return;

            if (Items[index].cacheImage.hasImage)
            {
                ThreadPool.QueueUserWorkItem(dummy =>
                {
                    try
                    {
                        //ver1.10 サムネイル登録も行う
                        Bitmap _bmp = Items[index].cacheImage.ToBitmap();
                        if (_bmp != null)
                        {
                            Items[index].resisterThumbnailImage(_bmp);

                            //ver1.81コメントアウト。悪さしそう
                            //_bmp.Dispose();
                        }
                    }
                    catch { }
                });
            }
        }

        /// <summary>
        /// サムネイル登録
        /// すでに画像を持っている場合はこれを使う
        /// </summary>
        /// <param name="index">画像のインデックス</param>
        /// <param name="orgBitmap">元画像</param>
        public void AsyncThumnailMaker(int index, Bitmap orgBitmap)
        {
            //ver1.73 index check
            if (index > Items.Count) return;
            if (orgBitmap == null) return;

            ThreadPool.QueueUserWorkItem(dummy =>
            {
                Items[index].resisterThumbnailImage(orgBitmap);
                //TODO:画像をDispose()すべき
                orgBitmap.Dispose();
            });
        }

        public void ThumnailMaker(int index, Bitmap bmp)
        {
            //ver1.73 index check
            if (index > Items.Count) return;

            if (bmp != null)
                Items[index].resisterThumbnailImage(bmp);
        }

        /// <summary>
        /// FileCacheをパージする
        /// </summary>
        /// <param name="MaxCacheSize">残しておくメモリ量[MB]</param>
        public void FileCacheCleanUp2(int MaxCacheSize)
        {
            MaxCacheSize *= 1000000;    //MB変換
            switch (App.Config.memModel)
            {
                case MemoryModel.Small:
                    //すべてのキャッシュをクリアする
                    foreach (var i in Items)
                        i.cacheImage.Clear();
                    break;

                case MemoryModel.Large:
                    //可能な限り残しておく
                    break;

                case MemoryModel.UserDefined:
                    //現在のサイズを計算
                    int nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.cacheImage.Length;
                    if (nowBufferSize <= MaxCacheSize)
                        break;

                    //大きいので消す
                    //現在の位置から上下にさかのぼっていく
                    int nowup = NowViewPage;
                    int nowdown = NowViewPage + 1;
                    int sumBytes = 0;
                    int thresholdSize = MaxCacheSize / 2;//半分のサイズまで解放

                    Uty.WriteLine("FileCacheCleanUp() start: {0:N0}bytes", nowBufferSize);
                    while (nowup >= 0 || nowdown < Items.Count)
                    {
                        if (nowup >= 0)
                        {
                            sumBytes += Items[nowup].cacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowup].cacheImage.Length > 0)
                                {
                                    Uty.WriteLine("FileCacheCleanUp():target={0}", nowup);
                                    Items[nowup].cacheImage.Clear();
                                }
                            }
                            nowup--;
                        }
                        if (nowdown < Items.Count)
                        {
                            sumBytes += Items[nowdown].cacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowdown].cacheImage.Length > 0)
                                {
                                    Uty.WriteLine("FileCacheCleanUp():target={0}", nowdown);
                                    Items[nowdown].cacheImage.Clear();
                                }
                            }
                            nowdown++;
                        }
                    }//while
                    nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.cacheImage.Length;
                    Uty.WriteLine("FileCacheCleanUp() end: {0:N0}bytes", nowBufferSize);
                    Uty.ForceGC();
                    break;

                default:
                    break;
            }
        }

        public string GetCsvFromBookmark()
        {
            //bookmarkされたorgIndexを拾ってくる。
            var bookmarks = Items.Where(c => c.isBookMark).Select(c => c.nOrgIndex);

            //Int配列をcsvに変換
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

        public void SetBookmarksFromCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return;

            //csvをInt配列に変換
            var bm = csv.Split(',').Select(c => { int.TryParse(c, out int r); return r; });

            //g_piに適用
            for (int i = 0; i < Items.Count; i++)
            {
                if (bm.Contains(Items[i].nOrgIndex))
                    Items[i].isBookMark = true;
            }
        }
    }
}
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
        /// <summary>Zipファイル名、もしくはディレクトリ名</summary>
        public string PackageName { get; set; }

        /// <summary>現在見ているアイテム番号</summary>
        public int NowViewPage { get; set; }

        /// <summary>Zipファイルサイズ</summary>
        public long PackageSize { get; set; }

        /// <summary>Zipファイル作成日</summary>
        public DateTime CreateDate { get; set; }

        /// <summary>サムネイル画像集</summary>
        public List<ImageInfo> Items { get; } = new List<ImageInfo>();

        /// <summary>ver1.31 パッケージのタイプ</summary>
        public PackageType PackType { get; set; } = PackageType.None;

        /// <summary>ver1.30 ページ送り方向</summary>
        public bool PageDirectionIsLeft { get; set; }

        /// <summary>ver1.09 書庫のモード</summary>
        [NonSerialized]
        public bool isSolid;

        /// <summary>書庫一時展開先ディレクトリ</summary>
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

            //if (Items.Count > 0)
            //{
            //    for (int i = 0; i < Items.Count; i++)
            //        Items[i].Dispose(); //サムネイル画像をクリア
            //}
            Items.Clear();

            //ファイルキャッシュをクリア
            foreach (var item in Items)
                item.CacheImage.Clear();
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
            return hasCacheImage(index) ? Items[index].CacheImage.ToBitmap() : null;
        }

        public bool hasCacheImage(int index)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            return Items[index].CacheImage.HasImage;
        }

        /// <summary>
        /// キャッシュにファイルを読み込む
        /// AsyncIOから呼ばれている画像読み込み本体。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="_7z"></param>
        /// <returns></returns>
        public bool LoadImageToCache(int index, SevenZipWrapper _7z)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            string filename = Items[index].Filename;

            if (PackType != PackageType.Archive)
            {
                //通常ファイルからの読み込み
                Items[index].CacheImage.Load(filename);
            }
            else if (isSolid && App.Config.isExtractIfSolidArchive)
            {
                //ver1.10 ソリッド書庫
                //一時フォルダの画像ファイルから読取り
                string tempname = Path.Combine(tempDirname, filename);
                Items[index].CacheImage.Load(tempname);
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
                        Items[index].CacheImage.Load(_7z.GetStream(filename));
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
            Items[index].ImgSize = Items[index].CacheImage.GetImageSize();

            //サムネイル登録はThreadPoolで
            //AsyncThumnailMaker(index);

            return true;
        }

        public void AsyncThumnailMaker(int index)
        {
            //ver1.73 index check
            if (index > Items.Count) return;

            if (Items[index].CacheImage.HasImage)
            {
                ThreadPool.QueueUserWorkItem(dummy =>
                {
                    try
                    {
                        //ver1.10 サムネイル登録も行う
                        Bitmap _bmp = Items[index].CacheImage.ToBitmap();
                        if (_bmp != null)
                        {
                            Items[index].ResisterThumbnailImage(_bmp);

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
                Items[index].ResisterThumbnailImage(orgBitmap);
                //TODO:画像をDispose()すべき
                orgBitmap.Dispose();
            });
        }

        /// <summary>
        /// サムネイルの作成・登録
        /// ここ1か所で行う。
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
                        i.CacheImage.Clear();
                    break;

                case MemoryModel.Large:
                    //可能な限り残しておく
                    break;

                case MemoryModel.UserDefined:
                    //現在のサイズを計算
                    int nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.CacheImage.Length;
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
                            sumBytes += Items[nowup].CacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowup].CacheImage.Length > 0)
                                {
                                    Uty.WriteLine("FileCacheCleanUp():target={0}", nowup);
                                    Items[nowup].CacheImage.Clear();
                                }
                            }
                            nowup--;
                        }
                        if (nowdown < Items.Count)
                        {
                            sumBytes += Items[nowdown].CacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowdown].CacheImage.Length > 0)
                                {
                                    Uty.WriteLine("FileCacheCleanUp():target={0}", nowdown);
                                    Items[nowdown].CacheImage.Clear();
                                }
                            }
                            nowdown++;
                        }
                    }//while
                    nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.CacheImage.Length;
                    Uty.WriteLine("FileCacheCleanUp() end: {0:N0}bytes", nowBufferSize);
                    Uty.ForceGC();
                    break;

                default:
                    break;
            }
        }

        public string CreateBookmarkString()
        {
            //bookmarkされたorgIndexを拾ってくる。
            var bookmarks = Items.Where(c => c.IsBookMark).Select(c => c.OrgIndex);

            //Int配列をcsvに変換
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

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
    }
}
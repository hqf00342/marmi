using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;	//Bookmarkの集計のために追加
using System.Threading;

namespace Marmi
{
    /********************************************************************************/
    //シリアライズ用データクラス：見ている画像セット
    /********************************************************************************/

    [Serializable]
    public class PackageInfo // : IDisposable
    {
        //Zipファイル名、もしくはディレクトリ名
        public string PackageName;

        //zipファイルかどうか
        //public bool isZip;

        //現在見ているアイテム番号
        public int NowViewPage;

        //Zipファイルサイズ
        public long size;

        //Zipファイル作成日
        public DateTime date;

        //サムネイル画像集
        public List<ImageInfo> Items = new List<ImageInfo>();

        //ver1.31 パッケージのタイプ
        public PackageType packType = PackageType.None;

        //ver1.30 ページ送り方向
        public bool LeftBook;

        //ver1.09 書庫のモード
        [NonSerialized]
        public bool isSolid;

        //書庫一時展開先
        [NonSerialized]
        public string tempDirname;

        //SevenZip書庫インスタンス
        //private SevenZipWrapper m_szw = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PackageInfo()
        {
            Uty.WriteLine("コンストラクタ");
            Initialize();
        }

        /// <summary>
        /// 初期化ルーチン
        /// </summary>
        public void Initialize()
        {
            Clear();
            //m_szw = new SevenZipWrapper();
            //Uty.WriteLine("make m_szw in Initialize");
        }

        public void Clear()
        {
            PackageName = string.Empty;
            packType = PackageType.None;
            isSolid = false;
            NowViewPage = 0;
            size = 0;
            date = DateTime.MinValue;
            tempDirname = string.Empty;
            //PackageMode = Mode.None;
            //isZip = false;

            LeftBook = true;

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].Dispose(); //サムネイル画像をクリア
                Items.Clear();
            }

            //ファイルキャッシュをクリア
            //g_FileCache.Clear();
            foreach (var item in Items)
                item.cacheImage.Clear();

            //7zをクリア
            //if (m_szw != null)
            //{
            //    m_szw.Close();
            //    m_szw = null;
            //    Uty.WriteLine("m_szw clear");
            //}
        }

        //loadThumbnailDBFile()のためだけに生成
        //public void InitCache()
        //{
        //    if (g_FileCache == null)
        //        g_FileCache = new BitmapCache();
        //}

        //public void Dispose()
        //{
        //    if (Items.Count > 0)
        //    {
        //        for (int i = 0; i < Items.Count; i++)
        //            Items[i].Dispose();	//サムネイル画像をクリア
        //        Items.Clear();
        //    }
        //    //Items.Clear();

        //    g_FileCache.Clear();

        //    if (m_szw != null)
        //    {
        //        m_szw.Close();
        //    }
        //}

        /// <summary>
        /// 指定インデックスが正しいかどうかチェックする
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private void CheckIndex(int index)
        {
            Debug.Assert(index >= 0 && index < Items.Count);
            //return (index >= 0 && index < Items.Count);
        }

        /// <summary>
        /// ファイル名からインデックスを取得する
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <returns>インデックス番号。無い場合は-1</returns>
        public int GetIndexFromFilename(string name)
        {
            int z = Items.Count;
            for (int i = 0; i < z; i++)
                if (Items[i].filename == name)
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
            //ver1.70 Indexチェック
            if (index < 0 || index >= Items.Count)
                return null;

            if (Items[index].cacheImage.hasImage)
                return Items[index].cacheImage.bitmap;
            else
                return null;
        }

        public bool hasCacheImage(int index)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            if (Items[index].cacheImage.hasImage)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 指定インデックスのBitmapを得る。
        /// 得られない場合はnullを返す
        /// </summary>
        /// <param name="index">取得するItem番号</param>
        /// <returns>得られたBitmap</returns>
        //public Bitmap GetBitmap(int index)
        //{
        //    //if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
        //    CheckIndex(index);

        //    //キャッシュにファイルがあるかどうかチェック
        //    string filename = Items[index].filename;
        //    //if (Items[index].cacheImage.bitmap != null)
        //    if (Items[index].cacheImage.hasImage)
        //    {
        //        //キャッシュにファイルがあるのでそれを使う
        //        Debug.WriteLine(filename, "GetBitmap() from CACHE");
        //        return Items[index].cacheImage.bitmap;
        //    }
        //    else
        //    {
        //        //キャッシュにファイルがないので取ってくる
        //        try
        //        {
        //            //取ったついでにキャッシュに追加
        //            if (m_szw == null)
        //            {
        //                m_szw = new SevenZipWrapper();
        //                Uty.WriteLine("make m_szw in GetBitmap(int index)");
        //            }
        //            Bitmap bmp = GetBitmapWithoutCache(index, m_szw);
        //            if (bmp == null)
        //            {
        //                Debug.WriteLine(filename, "GetBitmap() CANNOT load");
        //                return null;
        //            }

        //            Debug.WriteLine(filename, "GetBitmap() Load");
        //            return bmp;
        //        }
        //        catch (ArgumentException e)
        //        {
        //            //キャッシュ重複でnull返しはモッタイナイ
        //            Debug.WriteLine(e.Message, "キャッシュ重複エラー");
        //            return Items[index].cacheImage.bitmap;
        //            //return g_FileCache[filename];
        //        }
        //    }
        //}

        /// <summary>
        /// キャッシュなしでの読み込みルーチン
        /// 読み込めなかったときは潔くnullを返す仕様
        /// ver1.10 一時フォルダがある場合はそこから読み込む
        /// 　　　　そこから読み込めない場合はnullを返す
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>得られたBitmapオブジェクト。無い場合はnull</returns>
        //[Obsolete]
        //public Bitmap GetBitmapWithoutCache(int index, SevenZipWrapper _7z)
        //{
        //    LoadCache(index, _7z);

        //    //ver1.10 サムネイル登録も行う
        //    Bitmap _bmp = Items[index].cacheImage.bitmap;
        //    //if (_bmp != null)
        //    //    Items[index].resisterThumbnailImage(_bmp);

        //    return _bmp;
        //}

        /// <summary>
        /// キャッシュにファイルを読み込む
        /// </summary>
        /// <param name="index"></param>
        /// <param name="_7z"></param>
        /// <returns></returns>
        public bool LoadCache(int index, SevenZipWrapper _7z)
        {
            CheckIndex(index);

            string filename = Items[index].filename;

            if (packType != PackageType.Archive)
            {
                //通常ファイルからの読み込み
                Items[index].cacheImage.Add(filename);
            }
            else if (isSolid && Form1.g_Config.isExtractIfSolidArchive)
            {
                //ver1.10 ソリッド書庫 一時フォルダから読み取りを試みる
                string tempname = Path.Combine(tempDirname, filename);
                Items[index].cacheImage.Add(tempname);
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
                        Items[index].cacheImage.Add(_7z.GetStream(filename));
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
                        Bitmap _bmp = Items[index].cacheImage.bitmap;
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
            switch (Form1.g_Config.memModel)
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

        public string getBookmarks()
        {
            //bookmarkされたorgIndexを拾ってくる。
            var bookmarks = Items.Where(c => c.isBookMark).Select(c => c.nOrgIndex);

            //csvに変換
            //string s = string.Empty;
            //foreach(var b in bookmarks)
            //{
            //	s = s + b.ToString() + ",";
            //}
            //s = s.Trim(',');

            //Int配列をcsvに変換
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

        public void setBookmarks(string csv)
        {
            //空だったらなにもしない。NullReference対策
            if (string.IsNullOrEmpty(csv))
                return;

            //csvをInt配列に変換
            var bm = csv.Split(',').Select(c => { int r; Int32.TryParse(c, out r); return r; });

            //g_piに適用
            for (int i = 0; i < Items.Count; i++)
            {
                if (bm.Contains(Items[i].nOrgIndex))
                    Items[i].isBookMark = true;
            }
        }
    }
}
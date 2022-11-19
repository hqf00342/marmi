#define SEVENZIP	//SevenZipSharpを使うときはこれを定義する。

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi
{
    public static class AsyncIO
    {
        //非同期IO用スレッド
        private static Thread _thread = null;

        //Worker内部のSzがOpenしているかどうか
        //ClearJobAndWaitAsync()でszがオープンしているか判定するのに利用する。
        private static volatile bool _szOpen = false;

        //スレッドが生きているかどうか。終了したいときにfalseにすると無限ループを終了する。
        private static volatile bool _threadAlive = true;

        //非同期Jobリスト
        internal static PrioritySafeQueue<KeyValuePair<int, Action>> _queue = new PrioritySafeQueue<KeyValuePair<int, Action>>();

        /// <summary>
        /// 非同期IOを処理するための無限ループスレッドを生成し開始する
        /// </summary>
        public static void StartThread()
        {
            _thread = new Thread(Worker);
            _thread.Start();
        }

        /// <summary>スレッドを停止する。Marmiが終了するときに呼ばれる。</summary>
        public static void StopThread()
        {
            //_thread?.Abort();
            _threadAlive = false;
            _thread?.Join(3000);//最悪3秒待つ
        }

        /// <summary>
        /// 無限ループ。
        /// 新しいスレッド内でタスクを待ち受けている
        /// タスクを発見したらそのタスクを処理。
        /// </summary>
        private static void Worker()
        {
            //スレッド専用SevenZip
            SevenZipWrapper AsyncSZ = new SevenZipWrapper();


            while (_threadAlive)
            {
                if (_queue.Count > 0)
                {
                    var kv = _queue.Pop();
                    int index = kv.Key;
                    Delegate action = kv.Value;

                    Debug.WriteLine($"AsyncIO.Worker() : ix={index}, queue={_queue.Count} : {Path.GetFileName(App.g_pi.PackageName)}");

                    //終了信号受信
                    if (index < 0)
                    {
                        Debug.WriteLine("AsyncIO : 7z解放信号受信");
                        if (AsyncSZ.IsOpen)
                        {
                            AsyncSZ.Close();
                            AsyncSZ = new SevenZipWrapper();
                            Debug.WriteLine($"AsyncIO : sz Close()");
                        }
                        //szOpen = false;
                        continue;
                    }

                    try
                    {
                        App.g_pi.ThrowIfOutOfRange(index);

                        if (!App.g_pi.Items[index].CacheImage.HasImage)
                        {
                            //画像読み込み
                            LoadImage(AsyncSZ, index);

                            //画像サイズ設定
                            App.g_pi.Items[index].ImgSize = App.g_pi.Items[index].CacheImage.GetImageSize();

                            //サムネイル作成。ここ1か所に集約(2021年2月25日)
                            App.g_pi.Items[index].CreateThumbnail();
                        }

                        //Invoke(action)を実行
                        if (Form1._instance.IsHandleCreated && action != null)
                        {
                            Form1._instance.Invoke(action);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"AsyncIO : EXCEPTION: {e.GetType().Name}");
                    }
                }
                else
                {
                    //少し休憩
                    _szOpen = AsyncSZ.IsOpen;
                    Thread.Sleep(50);
                }
            }

            //スレッド終了処理
            if (AsyncSZ.IsOpen)
            {
                AsyncSZ?.Close();
                Debug.WriteLine($"AsyncIO : sz Close()");
            }
            Debug.WriteLine("AsyncIO.Worker()スレッドは終了しました");
        }

        /// <summary>
        /// PackageTypeごとの画像読み込み。
        /// AsyncIO.Work()内で都度呼び出される。
        /// </summary>
        /// <param name="sz"></param>
        /// <param name="index"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void LoadImage(SevenZipWrapper sz, int index)
        {
            if (sz == null)
                throw new ArgumentNullException(nameof(sz));

            App.g_pi.ThrowIfOutOfRange(index);

            var filename = App.g_pi.Items[index].Filename;

            switch (App.g_pi.PackType)
            {
                case PackageType.Archive:
                    if (!sz.IsOpen)
                    {
                        //書庫をOpenする
                        sz.Open(App.g_pi.PackageName);
                        Debug.WriteLine($"AsyncIO.LoadImage() : Open 7z: {App.g_pi.PackageName}");
                    }

                    if (App.g_pi.isSolid && App.Config.General.ExtractArchiveIfSolid)
                    {
                        //ソリッド書庫 又は オプション指定あり
                        //一時フォルダの画像を利用
                        string tempname = Path.Combine(App.g_pi.tempDirname, filename);
                        App.g_pi.Items[index].CacheImage.Load(tempname);
                    }
                    else
                    {
                        //通常書庫
                        //書庫から直接読みだす
                        App.g_pi.Items[index].CacheImage.Load(sz.GetStream(filename));
                    }
                    break;

                case PackageType.Pictures:
                case PackageType.Directory:
                    //生ファイルを読み込む
                    App.g_pi.Items[index].CacheImage.Load(filename);
                    break;

                case PackageType.Pdf:
                    //pdfファイルの読み込み
                    byte[] b = App.susie.GetFile(App.g_pi.PackageName, index, (int)App.g_pi.Items[index].FileLength);
                    App.g_pi.Items[index].CacheImage.Load(b);
                    App.g_pi.Items[index].ImgSize = App.g_pi.Items[index].CacheImage.GetImageSize();
                    break;

                case PackageType.None:
                default:
                    throw new NotImplementedException("PackageTypeが定義できていない");
            }
        }

        /// <summary>Low Queue Jobを追加。サムネイル作成など</summary>
        public static void AddJobLow(int index, Action uiAction) => _queue.PushLow(new KeyValuePair<int, Action>(index, uiAction));

        /// <summary>High Queue Jobを追加</summary>
        public static void AddJobHigh(int index, Action uiAction) => _queue.PushHigh(new KeyValuePair<int, Action>(index, uiAction));

        /// <summary>Jobをクリアする</summary>
        public static void ClearJob() => _queue.Clear();

        /// <summary>Job一覧を取得</summary>
        public static KeyValuePair<int, Action>[] GetAllJob() => _queue.ToArrayHigh();

        /// <summary>
        /// ジョブをオールクリアする。
        /// クリアし、szも閉じるまで待つ。
        /// </summary>
        /// <returns></returns>
        public static async Task ClearJobAndWaitAsync()
        {
            _queue.Clear();
            AddJobHigh(-1, null);

            if (_threadAlive)
            {
                while (_szOpen)
                {
                    await Task.Delay(50);
                }
                Debug.WriteLine("AsyncIO.ClearJobAndWaitAsync() 完了");
            }
        }
    }
}
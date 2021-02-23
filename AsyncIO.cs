#define SEVENZIP	//SevenZipSharpを使うときはこれを定義する。

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Marmi
{
    public static class AsyncIO
    {
        //非同期IO用スレッド
        private static Thread _thread = null;

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
            _thread?.Abort();
            _thread?.Join();
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

            while (true)
            {
                if (_queue.Count > 0)
                {
                    var kv = _queue.Pop();
                    int index = kv.Key;
                    Delegate action = kv.Value;

                    //終了信号受信
                    if (index < 0 && action == null)
                    {
                        Debug.WriteLine("AsyncIO : 7z解放信号受信");
                        if (AsyncSZ.IsOpen)
                        {
                            Uty.WriteLine("AsyncIO : {0}解放", AsyncSZ.Filename);
                            AsyncSZ.Close();
                        }
                        continue;
                    }

                    //画像読み込み
                    try //不意のファイルドロップによりindexがOutOfRangeになるため。効果なさそう
                    {
                        if (!App.g_pi.Items[index].CacheImage.HasImage)
                        {
                            Debug.WriteLine($"AsyncIO : index={index}, remain={_queue.Count}");
                            //7zをOpenしていなければOpen
                            if (App.g_pi.PackType == PackageType.Archive && !AsyncSZ.IsOpen)
                            {
                                AsyncSZ.Open(App.g_pi.PackageName);
                                Debug.WriteLine("AsyncIO : 7zOpen");
                            }

                            if (App.g_pi.PackType == PackageType.Pdf)
                            {
                                //pdfファイルの読み込み
                                byte[] b = App.susie.GetFile(App.g_pi.PackageName, index, (int)App.g_pi.Items[index].Length);
                                App.g_pi.Items[index].CacheImage.Load(b);
                                App.g_pi.Items[index].bmpsize = App.g_pi.Items[index].CacheImage.GetImageSize();
                                App.g_pi.AsyncThumnailMaker(index);
                            }
                            else
                            {
                                //pdf以外の読み込み
                                App.g_pi.LoadCache(index, AsyncSZ);
                                //ver1.75 サムネイル登録
                                //ver1.81コメントアウト
                                //サムネイル作成はあとでやる。
                                //g_pi.ThumnailMaker(index, g_pi.Items[index].cacheImage.bitmap);
                            }
                        }

                        //Invoke(action)を実行
                        if (Form1._instance.IsHandleCreated && action != null)
                        {
                            Form1._instance.Invoke(action);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine($"AsyncIO : {e.GetType().Name}");
                    }
                }
                else
                {
                    //タスクがなかったので少し休憩
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>Low Queue Jobを追加。サムネイル作成など</summary>
        public static void AddJobLow(int index, Action uiAction) => _queue.PushLow(new KeyValuePair<int, Action>(index, uiAction));

        /// <summary>High Queue Jobを追加</summary>
        public static void AddJob(int index, Action uiAction) => _queue.PushHigh(new KeyValuePair<int, Action>(index, uiAction));

        /// <summary>Jobをクリアする</summary>
        public static void ClearJob() => _queue.Clear();

        /// <summary>Job一覧を取得</summary>
        public static KeyValuePair<int, Action>[] GetAllJob() => _queue.ToArrayHigh();

    }
}
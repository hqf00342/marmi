using System;
using System.Collections.Generic;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;
using System.IO;						//Directory, File
using System.Threading;					//ThreadPool, WaitCallback
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // ThreadPool関数 ***************************************************************/

        /// <summary>
        /// ver1.17
        /// 2011年9月25日
        /// サムネイル作成スレッド
        /// キャッシュ活用バージョン
        /// </summary>
        /// <param name="state"></param>
        [Obsolete]
        private void ThumbNailMakerCallback3(Object state)
        {
            //ver0.993 2011/07/31すでに実行中なら何もしない
            if (tsThumbnail == ThreadStatus.RUNNING)
            {
                Debug.WriteLine("MakeThumbNailThread()2重起動");
                return;
            }

            //ThreadPoolから呼び出される
            tsThumbnail = ThreadStatus.RUNNING;

            using (SevenZipWrapper szw = new SevenZipWrapper())
            {
                for (int item = 0; item < g_pi.Items.Count; item++)
                {
                    //スレッドの一時中断確認
                    while (tsThumbnail == ThreadStatus.PAUSE)
                    {
                        Thread.Sleep(100);
                    }
                    //スレッドを中止させるか確認
                    if (tsThumbnail == ThreadStatus.REQUEST_STOP)
                    {
                        tsThumbnail = ThreadStatus.STOP;
                        return;
                    }

                    //作成済みであれば作成しない
                    if (g_pi.Items[item].ThumbImage != null)
                        continue;

                    //サムネイルの作成
                    //ver1.07 書き換え
                    //アーカイブに対し無駄にオブジェクトを作らないようにするため
                    //自分でロードしている。
                    string filename = g_pi.Items[item].filename;
                    Bitmap _bmp = null;

                    if (g_FileCache.ContainsKey(filename))
                    {
                        //キャッシュがあればそのまま使う
                        _bmp = g_FileCache[filename];
                    }
                    else if (g_pi.packType != PackageType.Archive)  //(!g_pi.isZip)
                    {
                        //通常ファイルモード
                        g_FileCache.Add(filename);
                        _bmp = g_FileCache[filename];
                    }
                    else if (g_pi.isSolid && g_Config.isExtractIfSolidArchive)
                    {
                        //ver1.10
                        //ソリッド書庫からの読み込み
                        //一時展開フォルダから読み取りを試みる
                        string tempname = Path.Combine(g_pi.tempDirname, filename);
                        g_FileCache.Add(tempname, filename);
                        _bmp = g_FileCache[filename];
                    }
                    else
                    {
                        //Solid書庫ではない書庫ファイルモード
                        if (!szw.isOpen)
                        {
                            szw.Open(g_pi.PackageName);
                        }

                        //ver1.17コメントアウト・キャッシュに取る
                        g_FileCache.Add(filename, szw.GetStream(filename));
                        _bmp = g_FileCache[filename];
                    }
                    //ver1.10 サムネイルを登録
                    g_pi.Items[item].resisterThumbnailImage(_bmp);

                    //ver1.09 どうしてこれが消えたのか・・・？？
                    //サムネイルパネル中なら更新を通知する
                    if (g_Config.isThumbnailView)
                    {
                        g_ThumbPanel.CheckUpdateAndDraw(item);
                    }

                    //ver1.30 2012年2月25日
                    //表示中のアイテムに近ければ再描写信号を出す
                    if (item == g_pi.NowViewPage + 1
                        && g_Config.dualView)
                    {
                        //次のページ情報が更新された
                        //2ページ表示可能かもしれないのでチェック
                        if (CanDualView(g_pi.NowViewPage))
                        {
                            //this.BeginInvoke((MethodInvoker)(() => SetViewPage(g_pi.NowViewPage)));
                            this.Invoke((MethodInvoker)(() => SetViewPage(g_pi.NowViewPage)));
                        }
                    }

                    //サイドバー更新
                    if (g_Sidebar.Visible)
                    {
                        this.BeginInvoke(
                            (Action<int>)((a) => g_Sidebar.UpdateViewItem(a)),
                            new object[] { item }
                            );
                    }

                    //ステータスバーの更新
                    string s = string.Format(
                        "サムネイル作成中:[{0}/{1}] {2}",
                        item + 1,   //0始まりなので+1
                        g_pi.Items.Count,
                        Path.GetFileName(filename));
                    this.BeginInvoke(
                        (Action<string>)((sz) => setStatusbarInfo(sz)),
                        new object[] { s });
                    //this.Invoke((MethodInvoker)(() => setStatusbarInfo(s)));

                    //ver1.26 Bitmap破棄
                    if (_bmp != null)
                        _bmp.Dispose();
                }//for

                //ver1.05ここは同期でInvoke()のままでいく。
                //this.Invoke(new MethodInvoker(delegate { Statusbar_InfoLabel.Text = "サムネイル作成完了"; }));
                //ver1.09 デッドロックする可能性があるので非同期に
                //this.BeginInvoke(delegateStatusbarRenew, new object[] { "サムネイル作成完了" });
                this.BeginInvoke(
                    (Action<string>)((sz) => setStatusbarInfo(sz)),
                    new object[] { "サムネイル作成完了" });

                tsThumbnail = ThreadStatus.STOP;
                //g_prepareThumbnail = true;
            }//using
        }

        [Obsolete]
        private void StartThumnailMakerThread()
        {
            //ver1.35 メモリモデルを導入
            if (g_Config.memModel == MemoryModel.Small)
                return;

            //作成するサムネイル数が多すぎる場合の事前確認
            if (g_pi.Items.Count >= MAX_THUMBNAIL_NUMBER)
            {
                string s = string.Format("ファイルが{0}個あります。サムネイルを作りますか？", g_pi.Items.Count);
                if (MessageBox.Show(s, "メモリ不足となる可能性があります", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    //サムネイルは作らないことを明示。ツールバーなどへ反映
                    g_makeThumbnail = false;
                    return;
                }
            }

            //サムネイル作成
            if (MAKETHUMBNAIL_BACKGROUND)
            {
                g_makeThumbnail = true;     //サムネイル作成フラグ、ツールバーなどへ反映。

                //ThreadPool版サムネイル作成
                //WaitCallback callback = new WaitCallback(ThumbNailMakerCallback3);
                //ThreadPool.QueueUserWorkItem(callback);

                //Thread版サムネイル作成
                //Threadが終了しているようにする
                //StopThumbnailMakerThread();

                if (ThumbnailMakerThread == null)
                {
                    //Threadオブジェクトを作成
                    ThumbnailMakerThread = new Thread(() => ThumbNailMakerCallback3(null));
                    ThumbnailMakerThread.Name = "Thumbnail Maker Thread";
                    ThumbnailMakerThread.IsBackground = true;
                }
                //Thread開始
                ThumbnailMakerThread.Start();
            }
        }

        [Obsolete]
        private void StopThumbnailMakerThread()
        {
            //ver1.35 メモリモデルを導入
            if (g_Config.memModel == MemoryModel.Small)
                return;

            //ThreadPool版
            //if (tsThumbnail != ThreadStatus.STOP)
            //{
            //    tsThumbnail = ThreadStatus.REQUEST_STOP;
            //    while (tsThumbnail != ThreadStatus.STOP)
            //    {
            //        //PumpMessage
            //        Application.DoEvents();
            //        Thread.Sleep(10);
            //    }
            //}

            //Thread版
            if (ThumbnailMakerThread == null)
                return;
            //if (!ThumbnailMakerThread.IsAlive)
            //    return;
            ThumbnailMakerThread.Abort();
            int joinCount = 0;
            while (!ThumbnailMakerThread.Join(1000))
            {
                setStatusbarInfo("サムネイル作成をキャンセルしてます" + joinCount.ToString());
            }
            ThumbnailMakerThread = null;
        }

        /// <summary>スレッド中断
        /// スレッドを中断する
        /// ThumnailPanel内から呼び出されるためにpublic指定。
        /// 他のForm（ThumbnailPanel）から呼び出されるためStatic化 2011/08/19 ver1.10
        /// </summary>
        [Obsolete]
        public static void PauseThumbnailMakerThread()
        {
            //ver1.35 メモリモデルを導入
            if (g_Config.memModel == MemoryModel.Small)
                return;

            Debug.WriteLine("PauseThreadPool()");
            //ver1.31 Pauseさせない
            return;

            if (tsThumbnail == ThreadStatus.RUNNING)
            {
                tsThumbnail = ThreadStatus.PAUSE;
            }
        }

        /// <summary> スレッドを再開する。
        /// ThumnailPanel内から呼び出されるためにpublic指定。
        /// 他のFormから呼び出されるためStaticに変更 2010/03/21 ver0.985
        /// </summary>
        [Obsolete]
        public static void ResumeThumbnailMakerThread()
        {
            //ver1.35 メモリモデルを導入
            if (g_Config.memModel == MemoryModel.Small)
                return;

            Debug.WriteLine("ContinueThreadPool()");
            if (tsThumbnail == ThreadStatus.PAUSE)
            {
                tsThumbnail = ThreadStatus.RUNNING;
            }
        }

        // 7zファイルを全展開させるスレッド *********************************************/
        //private void start7zExtractallThread()
        //{
        //    if (g_pi.isSolid)
        //    {
        //        ThreadStart tsAction = () =>
        //            {
        //                using (SevenZipWrapper szw = new SevenZipWrapper())
        //                {
        //                    szw.Open(g_pi.PackageName);
        //                    string tempdir = makeTempDirName(true);
        //                    g_pi.tempDirname = tempdir;
        //                    szw.ExtractAll(tempdir);
        //                }
        //            };

        //        //Thread _th = new Thread(tsAction);
        //        if (_th != null)
        //            _th.Abort();
        //        //Thread _th = new Thread(tsAction);
        //        _th = new Thread(tsAction);
        //        _th.Name = "7zExtractor Thread";
        //        _th.IsBackground = true;
        //        _th.Start();
        //        _th.Join();

        //        //フォルダに注意喚起のテキストを入れておく
        //        string attentionFilename = Path.Combine(g_pi.tempDirname, "このフォルダは消しても安全です.txt");
        //        string[] texts = {
        //            "このファイルはMarmi.exeによって作成された一時フォルダです",
        //            "Marmi.exeを起動していない場合、安全に削除できます"};
        //        try
        //        {
        //            File.WriteAllLines(
        //                attentionFilename,
        //                texts,
        //                System.Text.Encoding.UTF8);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    }
        //}
    }
}
#define SEVENZIP	//SevenZipSharpを使うときはこれを定義する。

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        //ver1.54 再作成
        private void AsyncIOThreadStart()
        {
            //Thread動作
            App.AsyncIOThread = new Thread(Worker);
            App.AsyncIOThread.Start();
        }

        /// <summary>
        /// 無限ループ。
        /// 新しいスレッド内でタスクを待ち受けている
        /// タスクを発見したらそのタスクを処理。
        /// </summary>
        private void Worker()
        {
            //スレッド専用SevenZip
            SevenZipWrapper AsyncSZ = new SevenZipWrapper();

            while (true)
            {
                if (App.stack.Count > 0)
                {
                    var kv = App.stack.Pop();
                    int index = kv.Key;
                    Delegate action = kv.Value;

                    //終了信号受信
                    if (index < 0 && action == null)
                    {
                        Uty.WriteLine("AsyncIOThread() 7z解放信号受信");
                        if (AsyncSZ.isOpen)
                        {
                            Uty.WriteLine("AsyncIOThread() {0}解放", AsyncSZ.Filename);
                            AsyncSZ.Close();
                        }
                        continue;
                    }

                    //画像読み込み
                    try //不意のファイルドロップによりindexがOutOfRangeになるため。効果なさそう
                    {
                        if (!g_pi.Items[index].cacheImage.hasImage)
                        {
                            Uty.WriteLine("AsyncIOThread() index={0}, remain={1}", index, App.stack.Count);
                            //7zをOpenしていなければOpen
                            if (g_pi.PackType == PackageType.Archive && !AsyncSZ.isOpen)
                            {
                                AsyncSZ.Open(g_pi.PackageName);
                                Uty.WriteLine("非同期IO 7zOpen");
                            }

                            if (g_pi.PackType == PackageType.Pdf)
                            {
                                //pdfファイルの読み込み
                                byte[] b = App.susie.GetFile(g_pi.PackageName, index, (int)g_pi.Items[index].length);
                                //ImageConverter ic = new ImageConverter();
                                //Bitmap _b = ic.ConvertFrom(b) as Bitmap;
                                //g_pi.Items[index].cacheImage.Load(_b);
                                g_pi.Items[index].cacheImage.Load(b);
                                g_pi.Items[index].bmpsize = g_pi.Items[index].cacheImage.GetImageSize();
                                g_pi.AsyncThumnailMaker(index);
                            }
                            else
                            {
                                //pdf以外の読み込み
                                g_pi.LoadCache(index, AsyncSZ);
                                //ver1.75 サムネイル登録
                                //ver1.81コメントアウト
                                //サムネイル作成はあとでやる。
                                //g_pi.ThumnailMaker(index, g_pi.Items[index].cacheImage.bitmap);
                            }
                        }

                        //Invoke(action)を実行
                        if (this.IsHandleCreated)
                        {
                            if (action != null)
                                this.Invoke(action);
                        }
                    }
                    catch
                    {
                        //System.ArgumentOutOfRangeException
                        //System.Threading.ThreadAbortException
                        Uty.WriteLine("catch出来たみたい。AsyncIOThreadStart()");
                    }
                }
                else
                {
                    //タスクがなかったので少し休憩
                    Thread.Sleep(50);
                }
            }
        }
    }
}
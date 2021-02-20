#define X86ONLY		//x86

using System;
using System.Diagnostics;				//Debug, Stopwatch
using System.IO;						//Directory, File
using System.Threading;

//Sharp Zip
using System.Windows.Forms;				//DialogResult
using SevenZip;
using ArchivedFiles = System.Collections.ObjectModel.ReadOnlyCollection<SevenZip.ArchiveFileInfo>;

namespace Marmi
{
    //インターフェース
    //  IArchiver: ZipWrapperと同じインターフェースを提供する ver1.31で対象外
    //  IDisposavle: usingで使うために必要
    //class SevenZipWrapper : IArchiver,IDisposable
    public class SevenZipWrapper : IDisposable
    {
        private string m_filename;              //OpenしているZipファイル名
        private bool m_isOpen = false;          //パスワード認証含めオープンしているか
        private SevenZipExtractor m_7z;
        private Thread m_ExtractAllThread;      //全解凍用スレッド

        //public string m_TempDir;				//一時展開先の書庫
        //volatile private bool isAsyncExtractionFinished;	//非同期展開が終わったことを示すフラグ
        //private object locker = new object();	//スレッドセーフにするため読込ロックをするためのobject

        //パスワードの保存
        //このクラスは１つだけ作られるわけではないのでstaticで持っておく
        //成功したときの未設定。string.emptyのときはパスワード無し
        private static string m_password;

        //イベントハンドラー
        //AyncExtractAllでのファイル展開用
        public event Action<string> ExtractEventHandler;        //１ファイル完了ごとのイベント

        public event EventHandler ExtractAllEndEventHandler;    //完全展開完了後のイベント

        public bool isCancelExtraction { get; set; }

        public bool isOpen { get { return m_isOpen; } }

        /// <summary>
        ///書庫にあるアイテム数を返す
        ///ディレクトリも1ファイルとして返すので注意
        /// </summary>
        public int itemCount
        {
            get { return m_7z.ArchiveFileData.Count; }
        }

        /// <summary>
        /// 書庫がソリッドかどうか返す
        /// </summary>
        public bool isSolid
        {
            get { return m_7z.IsSolid; }
        }

        public ArchivedFiles Items
        {
            get { return m_7z.ArchiveFileData; }
        }

        public string Filename
        {
            get { return m_filename; }
        }

        public SevenZipWrapper()
        {
            m_filename = null;
            m_7z = null;
            m_isOpen = false;

            //強制的に32bitライブラリに
            SevenZipExtractor.SetLibraryPath(
                Path.Combine(Application.StartupPath, "7z.dll"));
            //setLibrary32or64();

            //ver1.10 キャンセル処理のための初期化
            isCancelExtraction = false;
        }

        public SevenZipWrapper(string filename) : this()
        {
            if (!Open(filename))
                throw new FileNotFoundException();
        }

        public SevenZipWrapper(string filename, string password) : this()
        {
            if (string.IsNullOrEmpty(password))
                m_password = string.Empty;
            else
                m_password = password;

            if (!Open(filename))
                throw new FileNotFoundException();
        }

        ~SevenZipWrapper()
        {
            //m_password = string.Empty;
            //SevenZipWrapper.m_password = string.Empty;

            //イベントハンドラーの解除
            if (m_7z != null)
            {
                m_7z.Extracting -= new EventHandler<ProgressEventArgs>(evtExtracting);
                m_7z.ExtractionFinished -= new EventHandler<EventArgs>(evtExtractionFinished);
                m_7z.FileExtractionFinished -= new EventHandler<FileInfoEventArgs>(evtFileExtractionFinished);
                //m_7z.FileExists -= new EventHandler<FileOverwriteEventArgs>(evtFileExists);
                //m_7z.FileExtractionStarted -= new EventHandler<FileInfoEventArgs>(evtFileExtractionStarted);
            }
        }

        /// <summary>
        /// パスワードをクリアする。
        /// 新しく書庫を設定するとき（＝Start()）のときのみ
        /// 呼び出されることを想定。
        /// </summary>
        public static void ClearPassword()
        {
            m_password = string.Empty;
        }

        private void setLibrary32or64()
        {
            //ライブラリパスの設定
            if (IntPtr.Size == 4)
            {
                //32bit process
                SevenZipExtractor.SetLibraryPath(
                    Path.Combine(Application.StartupPath, "7z.dll"));
            }
            else if (IntPtr.Size == 8)
            {
                //64bit process
                SevenZipExtractor.SetLibraryPath(
                    Path.Combine(Application.StartupPath, "7z64.dll"));
            }
        }

        public bool Open(string ArchiveName)
        {
            //同じファイルをOpen済みならなにもしない
            //Uty.WriteLine("7z.Open() isOpen={0}, arc={1}", m_isOpen, ArchiveName);
            if (m_isOpen && ArchiveName == m_filename)
                return true;

            Uty.WriteLine("7z.Open({0})", ArchiveName);

            //ファイルの存在を確認
            if (!File.Exists(ArchiveName))
                return false;

            //オープンしていたら閉じておく
            if (m_isOpen)
            {
                Close();
            }

            m_isOpen = false;
            m_filename = ArchiveName;

            if (m_7z != null)
            {
                m_7z.Dispose();
                m_7z = null;
            }

            //ver1.05 7zipファイルオープン static password対応版
            try
            {
                //if (SevenZipWrapper.m_password != string.Empty)
                if (!string.IsNullOrEmpty(SevenZipWrapper.m_password))
                {
                    m_7z = new SevenZipExtractor(ArchiveName, m_password);

                    //書庫チェック。壊れていないか
                    //if (!m_sevenzipExtractor.Check())
                    if (m_7z.FilesCount == 0)
                    {
                        m_7z.Dispose();
                        m_7z = null;
                        m_isOpen = false;
                        return false;
                    }

                    if (TryPassword())
                        return true;
                    //password checkに失敗するとダイアログで対応
                }
                else
                {
                    //通常通りのOpenをする
                    m_7z = new SevenZipExtractor(ArchiveName);

                    //書庫チェック。壊れていないか
                    //if (!m_sevenzipExtractor.Check()
                    //	|| m_sevenzipExtractor.FilesCount == 0)

                    //ver1.31書庫チェックのつもりだったけど中止
                    //7zパスワード付きがここで例外
                    //if (m_7z.FilesCount == 0)
                    //{
                    //    m_7z.Dispose();
                    //    m_7z = null;
                    //    m_isOpen = false;
                    //    return false;
                    //}
                }

                //パスワードチェック
                //ver1.31 ディレクトリでないアイテムを探す
                int testitem = getFirstNonDirItem();
                if (testitem == -1)
                    return false;
                //パスワードチェック
                if (!m_7z.ArchiveFileData[testitem].Encrypted)
                {
                    //password不要なのでOpen完了
                    m_isOpen = true;
                    return true;
                }
                else
                {
                    //パスワード認証フォームを出し3回チャレンジする
                    return TryPasswordCheck(ArchiveName);
                }//if(Encrypted)
            }
            catch (SevenZipArchiveException)
            {
                //おそらく7zのパスワードに引っかかった
                return TryPasswordCheck(ArchiveName);
            }
            finally
            {
                //イベントハンドラーの登録
                add7zEvent();
            }
        }

        private bool TryPasswordCheck(string ArchiveName)
        {
            //パスワード認証フォームを出し3回チャレンジする
            int passwordRetryRemain = 3;
            using (FormPassword pf = new FormPassword())
            {
                //リトライ回数分をループ
                while (passwordRetryRemain > 0)
                {
                    if (pf.ShowDialog() == DialogResult.OK)
                    {
                        //作ったStreamを解除し、パスワード付きStreamを再生成
                        m_7z.Dispose();
                        m_7z = new SevenZipExtractor(ArchiveName, pf.PasswordText);
                    }
                    else
                    {
                        //Cancelされたので戻る
                        passwordRetryRemain = 0;
                        break;
                    }

                    //パスワードチェック
                    if (TryPassword())
                    {
                        m_isOpen = true;    //パスワード認証済み
                        m_password = pf.PasswordText;
                        //SevenZipWrapper.m_password = pf.PasswordText;
                        //m_password = m_sevenzipExtractor.Password;
                        return true;
                    }
                    else
                    {
                        passwordRetryRemain--;
                        pf.PasswordText = "";   //password dialogのTextBoxをクリア
                    };
                }

                //パスワード認証3回失敗
                m_isOpen = false;   //パスワード認証失敗によるクローズ状態
                return false;
            }//using
        }

        private void add7zEvent()
        {
            if (m_7z != null)
            {
                m_7z.Extracting += new EventHandler<ProgressEventArgs>(evtExtracting);
                m_7z.ExtractionFinished += new EventHandler<EventArgs>(evtExtractionFinished);
                m_7z.FileExtractionFinished += new EventHandler<FileInfoEventArgs>(evtFileExtractionFinished);
                //m_7z.FileExists += new EventHandler<FileOverwriteEventArgs>(evtFileExists);
                //m_7z.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(evtFileExtractionStarted);
            }
        }

        /// <summary>
        /// パスワードが必要かどうかチェックする
        /// </summary>
        /// <returns>必要な場合はfalse</returns>
        private bool TryPassword()
        {
            //試しに展開させ、パスワード認証失敗なら例外を発生させる
            try
            {
                int testitem = getFirstNonDirItem();
                if (testitem == -1) return false;

                using (MemoryStream ms = new MemoryStream())
                {
                    //m_7z.ExtractFile(0, ms);
                    m_7z.ExtractFile(testitem, ms);
                }
                //ここに来たときは成功
                return true;
            }
            catch //(Exception e)
            {
                return false;
            }
        }

        private int getFirstNonDirItem()
        {
            //書庫内のディレクトリではないアイテムを探す
            //パスワードチェック用に利用
            //見つからない場合は-1を返す
            int testitem = 0;
            while (m_7z.ArchiveFileData[testitem].IsDirectory)
            {
                testitem++;
                if (testitem >= m_7z.ArchiveFileData.Count)
                    return -1;
            }
            return testitem;
        }

        public void Close()
        {
            if (m_7z != null)
                m_7z.Dispose();

            m_7z = null;
            m_filename = null;
            m_isOpen = false;
            //m_password = string.Empty;
        }

        public Stream GetStream(string filename)
        {
            try
            {
                if (m_7z != null)
                {
                    //UNDONE:2011年7月30日 サムネイル中のnullreferの原因が
                    //UNDONE:スレッドセーフでないと当たりをつけて実施。
                    //lock (locker)
                    //{
                    MemoryStream st = new MemoryStream();
                    m_7z.ExtractFile(filename, st);
                    st.Seek(0, SeekOrigin.Begin);
                    return st;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch (ExtractionFailedException e)
            {
                MessageBox.Show(
                    "書庫が壊れています\n" + e.Message,
                    "ファイル展開エラー");
                return null;
            }
        }

        //ver1.31 プロパティで代用
        //public ArchiveItem Item(int index)
        //{
        //    return new ArchiveItem(
        //        m_7z.ArchiveFileData[index].FileName,
        //        m_7z.ArchiveFileData[index].CreationTime,
        //        m_7z.ArchiveFileData[index].Size,
        //        m_7z.ArchiveFileData[index].IsDirectory
        //        );
        //}

        public void ExtractAll(string ExtractDir)
        {
            if (m_7z == null)
                return;

            ////イベントハンドラーの登録
            //add7zEvent();

            //ディレクトリが無ければ生成
            if (!Directory.Exists(ExtractDir))
                Directory.CreateDirectory(ExtractDir);

            //非同期展開開始。
            Debug.WriteLine(ExtractDir, "7z展開開始");
            //isAsyncExtractionFinished = false;
            //m_sevenzipExtractor.BeginExtractArchive(m_TempDir);
            try
            {
                m_7z.ExtractArchive(ExtractDir);
            }
            catch (SevenZipException e)
            {
                Debug.Write("7zError::");
                Debug.WriteLine(e.Message, e.StackTrace);
                //throw e;
            }
            catch (PathTooLongException e)
            {
                //どうしようもないので無視
                Debug.Write("7zError::");
                Debug.WriteLine(e.Message, e.StackTrace);
                //throw e;
            }
            catch (IOException e)
            {
                Debug.Write("7zError::");
                Debug.WriteLine(e.Message, e.StackTrace);
            }
        }

        public void AsyncExtractAll(string zippedFilename, string extractFolder)
        {
            //スレッドが動いていないことを確認する
            if (m_ExtractAllThread != null)
            {
                if (m_ExtractAllThread.IsAlive)
                    return;
                m_ExtractAllThread.Abort();
            }

            //extractorはスレッド外で生成できないので破棄
            if (m_7z != null)
            {
                m_7z.Dispose();
                m_7z = null;
            }

            ThreadStart tsAction = () =>
            {
                Open(zippedFilename);
                ExtractAll(extractFolder);
                //ExtractAll(extractFolder, true);
            };

            m_ExtractAllThread = new Thread(tsAction);
            m_ExtractAllThread.Name = "7zExtractor Thread";
            m_ExtractAllThread.IsBackground = true;
            m_ExtractAllThread.Start();
            //m_ExtractAllThread.Join();

            //フォルダに注意喚起のテキストを入れておく
            try
            {
                string attentionFilename = Path.Combine(
                    extractFolder,
                    "このフォルダは消しても安全です.txt");
                string[] texts = {
                    "このファイルはMarmi.exeによって作成された一時フォルダです",
                    "Marmi.exeを起動していない場合、安全に削除できます"};

                File.WriteAllLines(
                    attentionFilename,
                    texts,
                    System.Text.Encoding.UTF8);
            }
            catch
            {
                //別に作成できなくてもいいので例外はすべて放置
                //throw;
            }
        }

        //AsyncExtractAllをキャンセルする
        public void CancelAsyncExtractAll()
        {
            Debug.WriteLine("Extract is Calceling...");
            if (m_7z != null)
                isCancelExtraction = true;
        }

        public bool ExtractFile(string extractFilename, string path)
        {
            try
            {
                string outfile = Path.Combine(path, extractFilename);
                using (FileStream fs = File.Open(outfile, FileMode.Create))
                {
                    m_7z.ExtractFile(extractFilename, fs);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        //
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // イベントハンドラー
        //

        //void evtFileExists(object sender, FileOverwriteEventArgs e)
        //{
        //    Debug.WriteLine(e.FileName, "SevenZipWrapper - ファイルがすでに存在しています");
        //    //e.Cancel = true;
        //}

        private void evtExtractionFinished(object sender, EventArgs e)
        {
            //単品解凍でも表示されるのでコメントアウト
            //Debug.WriteLine("SevenZipWrapper - 展開完了");

            //イベント登録チェック
            if (ExtractAllEndEventHandler != null)
            {
                Debug.WriteLine("SevenZipWrapper - 展開完了");
                ExtractAllEndEventHandler(this, EventArgs.Empty);
            }
        }

        private void evtFileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            //ver1.10 キャンセル処理の追加
            if (isCancelExtraction)
            {
                e.Cancel = true;
                Debug.WriteLine("7z展開中に中断処理が入りました");
            }

            //1ファイル展開毎にイベント生成
            if (ExtractEventHandler != null)
                ExtractEventHandler(e.FileInfo.FileName);
        }

        private void evtExtracting(object sender, ProgressEventArgs e)
        {
            //ver1.10 キャンセル処理の追加
            //ただしここでのキャンセルは効かない模様。
            //効くのはFileExtractionFinished()
            if (isCancelExtraction)
            {
                e.Cancel = true;
                Debug.WriteLine("7z展開中に中断処理が入りました", "m_sevenzipExtractor_Extracting()");
            }
        }

        #region IDisposable メンバ

        void IDisposable.Dispose()
        {
            //throw new Exception("The method or operation is not implemented.");
            if (m_7z != null)
                m_7z.Dispose();
        }

        #endregion IDisposable メンバ
    }
}
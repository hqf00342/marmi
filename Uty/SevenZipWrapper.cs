#define X86ONLY		//x86

using SevenZip;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ArchivedFiles = System.Collections.ObjectModel.ReadOnlyCollection<SevenZip.ArchiveFileInfo>;

namespace Marmi
{
    public class SevenZipWrapper : I7zipWrapper //: IDisposable
    {
        private string m_filename;
        private bool m_isOpen = false;          //パスワード認証含めオープンしているか
        private SevenZipExtractor m_7z;

        //パスワード
        //このクラスは１つだけ作られるわけではないのでstaticで持っておく
        //成功したときの未設定。string.emptyのときはパスワード無し
        private static string m_password;

        //全展開時のイベント：１ファイル完了
        public event Action<string> ExtractEventHandler;

        //全展開時のイベント：全ファイル完了
        public event EventHandler ExtractAllEndEventHandler;

        public bool IsCancelExtraction { get; set; }

        public bool IsOpen => m_isOpen;

        /// <summary>
        ///書庫にあるアイテム数を返す
        ///ディレクトリも1ファイルとして返すので注意
        /// </summary>
        public int ItemCount => m_7z.ArchiveFileData.Count;

        /// <summary>
        /// 書庫がソリッドかどうか返す
        /// </summary>
        public bool IsSolid => m_7z.IsSolid;

        public ArchivedFiles Items => m_7z.ArchiveFileData;

        public string Filename => m_filename;

        public SevenZipWrapper()
        {
            m_filename = null;
            m_7z = null;
            m_isOpen = false;

            //強制的に32bitライブラリに
            //SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7z.dll"));
            //SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7z64.dll"));
            SetLibraryPath();

            //ver1.10 キャンセル処理のための初期化
            IsCancelExtraction = false;
        }

        ~SevenZipWrapper()
        {
            Close();
        }

        private void SetLibraryPath()
        {
            var libname = Environment.Is64BitOperatingSystem ? "7z64.dll" : "7z.dll";
            SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, libname));
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

        //private void setLibrary32or64()
        //{
        //    //ライブラリパスの設定
        //    if (IntPtr.Size == 4)
        //    {
        //        //32bit process
        //        SevenZipExtractor.SetLibraryPath(
        //            Path.Combine(Application.StartupPath, "7z.dll"));
        //    }
        //    else if (IntPtr.Size == 8)
        //    {
        //        //64bit process
        //        SevenZipExtractor.SetLibraryPath(
        //            Path.Combine(Application.StartupPath, "7z64.dll"));
        //    }
        //}

        public bool Open(string ArchiveName)
        {
            //同じファイルをOpen済みならなにもしない
            if (m_isOpen && ArchiveName == m_filename)
                return true;

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
            m_7z?.Dispose();

            //ver1.05 7zipファイルオープン static password対応版
            try
            {
                if (!string.IsNullOrEmpty(m_password))
                {
                    m_7z = new SevenZipExtractor(ArchiveName, m_password);

                    //書庫チェック。壊れていないか
                    if (m_7z.FilesCount == 0)
                    {
                        m_7z.Dispose();
                        m_7z = null;
                        m_isOpen = false;
                        return false;
                    }

                    if (TryExtract())
                        return true;
                    //password checkに失敗するとダイアログで対応
                }
                else
                {
                    //通常通りのOpenをする
                    m_7z = new SevenZipExtractor(ArchiveName);
                }

                //パスワードチェック
                //ver1.31 ディレクトリでないアイテムを探す
                int testitem = GetFirstNonDirIndex();
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
                if (m_7z != null)
                {
                    m_7z.ExtractionFinished += ExtractionFinished;
                    m_7z.FileExtractionFinished += FileExtractionFinished;
                }
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
                    if (TryExtract())
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
                    }
                }

                //パスワード認証3回失敗
                m_isOpen = false;   //パスワード認証失敗によるクローズ状態
                return false;
            }//using
        }

        /// <summary>
        /// パスワードが必要かどうかチェックするために1ファイル展開する
        /// </summary>
        /// <returns>展開失敗した場合＝パスワード無効時はfalse</returns>
        private bool TryExtract()
        {
            //試しに展開させ、パスワード認証失敗なら例外を発生させる
            try
            {
                int testitem = GetFirstNonDirIndex();
                if (testitem == -1)
                    return false;

                using (var ms = new MemoryStream())
                {
                    m_7z.ExtractFile(testitem, ms);
                }
                //ここに来たときは成功
                return true;
            }
            catch
            {
                //SevenZipArchiveException  : 7z書庫のパスワードが正しくなかった
                //ExtractionFailedException : zip書庫のパスワードが正しくなかった
                return false;
            }
        }

        /// <summary>
        /// 書庫内のディレクトリではないアイテムを探す.パスワードチェック用に利用
        /// </summary>
        /// <returns>書庫内のインデックス。見つからない場合は-1</returns>
        private int GetFirstNonDirIndex()
        {
            int index = 0;
            while (m_7z.ArchiveFileData[index].IsDirectory)
            {
                index++;
                if (index >= m_7z.ArchiveFileData.Count)
                    return -1;
            }
            return index;
        }

        /// <summary>
        /// 書庫を閉じる。イベントの解除も行う
        /// </summary>
        public void Close()
        {
            if (m_7z != null)
            {
                m_7z.ExtractionFinished -= ExtractionFinished;
                m_7z.FileExtractionFinished -= FileExtractionFinished;
                m_7z.Dispose();
                m_7z = null;
            }

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
            try
            {
                m_7z.ExtractArchive(ExtractDir);
            }
            catch (SevenZipException e)
            {
                Debug.WriteLine($"7z Error : {e.GetType().Name} {e.Message}");
            }
            catch (PathTooLongException e)
            {
                //どうしようもないので無視
                Debug.WriteLine($"7z Error : {e.GetType().Name} {e.Message}");
            }
            catch (IOException e)
            {
                Debug.WriteLine($"7z Error : {e.GetType().Name} {e.Message}");
            }
        }

        //AsyncExtractAllをキャンセルする
        public void CancelExtractAll()
        {
            if (m_7z != null)
                IsCancelExtraction = true;
        }

        /// <summary>イベント処理</summary>
        private void ExtractionFinished(object sender, EventArgs e)
        {
            //全ファイル完了イベントを発火
            ExtractAllEndEventHandler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>イベント処理</summary>
        private void FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            //ver1.10 キャンセル処理の追加
            if (IsCancelExtraction)
            {
                e.Cancel = true;
                Debug.WriteLine("7z展開中に中断処理が入りました");
            }

            //1ファイル展開イベントを発火
            ExtractEventHandler?.Invoke(e.FileInfo.FileName);
        }
    }
}
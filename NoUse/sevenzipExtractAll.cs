using System.Diagnostics;				//Debug, Stopwatch
using System.IO;						//Directory, File
using System.Threading;					//ThreadPool, WaitCallback

//
// ver1.10
// 非同期に7zを展開するクラス
// SevenZipWrapperに内包したため必要なくなった
//

namespace Marmi
{
    public class sevenzipExtractAll
    {
        Thread _th;
        SevenZipWrapper szw;

        public sevenzipExtractAll(string zippedFilename, string extractFolder)
        {
            //ちょっとテスト：スレッドの外で生成して問題ないか
            szw = new SevenZipWrapper();
            //szw.Open(zippedFilename);

            ThreadStart tsAction = () =>
            {
                //szw = new SevenZipWrapper();
                szw.Open(zippedFilename);
                szw.ExtractAll(extractFolder);
            };

            if (_th != null)
                _th.Abort();

            _th = new Thread(tsAction);
            _th.Name = "7zExtractor Thread";
            _th.IsBackground = true;
            _th.Start();
            //_th.Join();

            //フォルダに注意喚起のテキストを入れておく
            string attentionFilename = Path.Combine(extractFolder, "このフォルダは消しても安全です.txt");
            string[] texts = {
                    "このファイルはMarmi.exeによって作成された一時フォルダです",
                    "Marmi.exeを起動していない場合、安全に削除できます"};
            try
            {
                File.WriteAllLines(
                    attentionFilename,
                    texts,
                    System.Text.Encoding.UTF8);
            }
            catch
            {
                throw;
            }
        }

        public void cancel()
        {
            Debug.WriteLine("calceling...");
            if (szw != null)
                szw.isCancelExtraction = true;
        }
    }
}
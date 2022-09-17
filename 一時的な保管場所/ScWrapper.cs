/****************************************************************************
ScWrapper.cs

SharpCompress のラッパークラス
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Readers;

namespace Marmi
{
    internal class ScWrapper : I7zipWrapper
    {
        private readonly Encoding _sjis = Encoding.GetEncoding(932);
        private IArchive _archive;
        private string _password;
        private bool cancelExtraction = false;

        //全ファイル展開時のイベントハンドラー
        public event Action<string> ExtractEventHandler;
        public event EventHandler ExtractAllEndEventHandler;


        public bool IsOpen => _archive != null;

        public int ItemCount => _archive?.Entries?.Count() ?? 0;

        public bool IsSolid => _archive?.IsSolid ?? true;

        public string Filename { get; private set; }

        public bool Open(string filename)
        {
            Filename = filename;
            var opt = CreateArchiveOptions();
            _archive = ArchiveFactory.Open(filename, opt);

            //パスワードチェックをする
            if (CheckAccess())
            {
                return true;
            }
            else
            {
                //アクセスできない＝パスワードが必要。
                return ShowPasswordDialog(filename);
            }
        }

        public void Close()
        {
            _archive?.Dispose();
            _archive = null;
            _password = string.Empty;
            Filename = string.Empty;
        }

        /// <summary>
        /// 書庫にアクセスできるかチェック
        /// パスワードが不足していると失敗する。
        /// </summary>
        /// <returns>成功するとtrue</returns>
        private bool CheckAccess()
        {
            if (_archive == null) return false;
            try
            {
                //zipのアクセスができるかチェック
                var st = _archive.Entries.First(a => !a.IsDirectory).OpenEntryStream();
                st.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CancelExtractAll()
        {
                cancelExtraction = true;
        }


        public void ExtractAll(string extractDir)
        {
            if (_archive == null)
                return;

            //ディレクトリが無ければ生成
            if (!Directory.Exists(extractDir))
                Directory.CreateDirectory(extractDir);

            //非同期展開開始。
            cancelExtraction = false;
            foreach (var item in _archive.Entries)
            {
                if (cancelExtraction)
                    break;

                try
                {
                    ExtractEventHandler?.Invoke(item.Key);
                    item.WriteToDirectory(extractDir);
                }
                catch
                {
                    break;
                }
            }
            ExtractAllEndEventHandler?.Invoke(null, EventArgs.Empty);
        }

        public Stream GetStream(string filename)
        {
            if(_archive== null) return null;

            try
            {
                var entry = _archive.Entries.FirstOrDefault(a => a.Key == filename);
                    return entry?.OpenEntryStream();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 書庫オープン時に使うオプションを生成する
        /// ・SJISファイル名へ対応
        /// ・パスワードがあるときは付与
        /// </summary>
        /// <param name="passwd">パスワード</param>
        /// <returns>Open()に使うReaderOptions</returns>
        private ReaderOptions CreateArchiveOptions(string passwd = null)
        {
            var opts = new SharpCompress.Readers.ReaderOptions();
            opts.ArchiveEncoding = new()
            {
                CustomDecoder = (data, _, _) => _sjis.GetString(data)
            };
            if (!string.IsNullOrEmpty(passwd))
            {
                opts.Password = passwd;
                _password = passwd;
            }
            return opts;
        }

        /// <summary>
        /// パスワード認証フォームを出し3回チャレンジする
        /// </summary>
        /// <param name="ArchiveName"></param>
        /// <returns></returns>
        private bool ShowPasswordDialog(string filename)
        {
            var passwordRetryRemain = 3;
            using var pf = new FormPassword();

            //リトライループ
            while (passwordRetryRemain-- > 0)
            {
                Close();
                if (pf.ShowDialog() == DialogResult.OK)
                {
                    var opt = CreateArchiveOptions(pf.PasswordText);
                    _archive = ArchiveFactory.Open(filename, opt);
                    if (CheckAccess())
                    {
                        _password = pf.PasswordText;
                        return true;
                    }
                }
                else
                {
                    //Cancelされた
                    passwordRetryRemain = 0;
                    break;
                }
            }
            return false;
        }
    }
}

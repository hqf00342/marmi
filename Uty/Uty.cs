using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Marmi
{
    public static class Uty
    {
        /// <summary>
        /// 対応書庫かどうかをチェックする
        /// </summary>
        /// <param name="archiveName">チェック対象の書庫ファイル名</param>
        /// <returns>対応書庫ならtrue</returns>
        public static bool IsSupportArchiveFile(string archiveName)
        {
            var ext = Path.GetExtension(archiveName).ToLower();
            switch (ext)
            {
                case ".zip":
                case ".7z":
                case ".rar":
                case ".tar":
                case ".lzh":
                case ".gz":
                case ".tgz":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 対応しているファイル形式かどうかをチェック
        /// 書庫、画像、pdfとしてチェック
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsAvailableFile(string filename)
        {
            if (IsSupportArchiveFile(filename))
                return true;
            else if (IsPictureFilename(filename))
                return true;
            return filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 画像ファイルかどうか確認する。拡張子を見て確認
        /// </summary>
        /// <param name="sz">ファイル名</param>
        /// <returns>画像ファイルであればtrue</returns>
        public static bool IsPictureFilename(string sz)
        {
            return Regex.Match(sz, @"\.(jpeg|jpg|jpe|png|gif|bmp|ico|tif|tiff)$", RegexOptions.IgnoreCase).Success;
        }

        /// <summary>
        /// ガベージコレクションをする。
        /// 全ジェネレーション実施
        /// </summary>
        public static void ForceGC()
        {
            //2021年2月26日
            // GCしたいのならば
            //85KB以上のObjectはLargeObjectHeapにあるのでLOHを圧縮する設定をどこかでしたほうがいい
            //System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;

            var before = GC.GetTotalMemory(false);
            GC.Collect();                   // 全ジェネレーションのGC
            GC.WaitForPendingFinalizers();  // ファイナライズ終了まで待つ
            GC.Collect();                   // ファイナライズされたObjをGC

            Debug.WriteLine($"ForceGC {before:N0}byte to {GC.GetTotalMemory(false):N0}byte");
        }

        /// <summary>
        /// ファイル名（アーカイブ名）をベースにユニークな展開フォルダ名を探す。
        /// 書庫と同じ場所で探す
        /// </summary>
        /// <param name="archiveName">書庫名</param>
        /// <returns></returns>
        public static string GetUniqueDirname(string archiveName)
        {
            //string ext = Path.GetExtension(archiveName);
            int trynum = 0;

            string tryBaseName = Path.Combine(
                Path.GetDirectoryName(archiveName),
                Path.GetFileNameWithoutExtension(archiveName));

            //そのままの名前で使えるか
            if (!Directory.Exists(tryBaseName))
                return tryBaseName;

            //ダメな場合はユニークな名前を探し回る
            while (true)
            {
                string tryName = string.Format("{0}{1}", tryBaseName, trynum);
                if (Directory.Exists(tryName))
                    trynum++;
                else
                    return tryName;
            }
        }

        /// <summary>
        /// 一時ディレクトリに「このフォルダは消しても安全です.txt」を作る。
        /// </summary>
        /// <param name="tempDirName">対象のディレクトリ</param>
        public static void CreateAnnotationFile(string tempDirName)
        {
            try
            {
                File.WriteAllText(
                    Path.Combine(tempDirName, "このフォルダは消しても安全です.txt"),
                    "このファイルはMarmi.exeによって作成された一時フォルダです" + Environment.NewLine +
                    "Marmi.exeを起動していない場合、安全に削除できます",
                    System.Text.Encoding.UTF8);
            }
            catch
            {
                //別に作成できなくてもいいので例外はすべて放置
                //throw;
            }
        }

        /// <summary>
        /// 一時ディレクトリを削除する
        /// </summary>
        /// <param name="tempDirName">一時ディレクトリ</param>
        public static void DeleteTempDir(string tempDirName)
        {
            if (Directory.Exists(tempDirName))
            {
                try
                {
                    //再帰的に消す
                    Directory.Delete(tempDirName, true);
                }
                catch (Exception)
                {
                    MessageBox.Show($"{tempDirName}の削除が出来ませんでした", "フォルダ削除エラー");
                }
            }
        }

        /// <summary>
        /// ver1.35 ゴミ箱へ送る
        /// </summary>
        /// <param name="filename"></param>
        public static void RecycleBin(string filename)
        {
            FileSystem.DeleteFile(
               filename,
               UIOption.OnlyErrorDialogs,
               RecycleOption.SendToRecycleBin);
        }

        public static string GetUsedMemory()
        {
            long used = GC.GetTotalMemory(false);
            if (used < 1000)
                return string.Format("Used:{0}bytes", used);
            else if (used < 1000000)
                return string.Format("Used:{0}Kbytes", used / 1000);
            else if (used < 1000000000)
                return string.Format("Used:{0}Mbytes", used / 1000000);
            else
                return string.Format("Used:{0}Gbytes", used / 1000000000);
        }
    }
}
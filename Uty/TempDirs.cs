using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    internal static class TempDirs
    {
        //削除候補ディレクトリ
        private static readonly List<string> dirList = new List<string>();

        /// <summary>
        /// 全ての一時ディレクトリを削除する
        /// </summary>
        internal static void DeleteAll()
        {
            foreach (string dir in dirList)
            {
                DeleteTempDir(dir);
            }
            dirList.Clear();
        }

        /// <summary>
        /// 一時ディレクトリリストに追加する
        /// </summary>
        /// <param name="dir">ディレクトリ名</param>
        internal static void AddDir(string dir)
        {
            dirList.Add(dir);
        }

        /// <summary>
        /// 一時ディレクトリを削除する
        /// </summary>
        /// <param name="tempDirName">一時ディレクトリ</param>
        private static void DeleteTempDir(string tempDirName)
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
    }
}
/*
MRU
最近使ったファイル
シリアライズ用
*/

using System;

namespace Marmi
{
    [Serializable]
    public class MRU : IComparable
    {
        /// <summary>ファイル名</summary>
        public string Name;

        /// <summary>利用日時</summary>
        public DateTime Date;

        /// <summary>ver1.37 : 最後に見たページ</summary>
        public int LastViewPage;

        /// <summary>ブックマーク一覧。csvテキスト(ver1.77)</summary>
        public string Bookmarks { get; set; }

        /// <summary>シリアライズのために必要</summary>
        public MRU()
        {
        }

        public MRU(string filename, DateTime date, int lastViewPage, string bookmarks)
        {
            Name = filename;
            Date = date;
            LastViewPage = lastViewPage;
            Bookmarks = bookmarks;
        }

        /// <summary>日付でソート(古い順)</summary>
        int IComparable.CompareTo(object obj) => DateTime.Compare(Date, (obj as MRU).Date);
    }
}
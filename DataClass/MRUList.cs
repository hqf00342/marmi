using System;

/*
最近使ったファイル
シリアライズ用
*/

namespace Marmi
{
    [Serializable]
    public class MRUList : IComparable
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
        public MRUList()
        {
        }

        public MRUList(string filename, DateTime date, int lastViewPage, string bookmarks)
        {
            Name = filename;
            Date = date;
            LastViewPage = lastViewPage;
            Bookmarks = bookmarks;
        }

        /// <summary>日付でソート(古い順)</summary>
        int IComparable.CompareTo(object obj) => DateTime.Compare(Date, (obj as MRUList).Date);
    }
}
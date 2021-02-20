using System;

namespace Marmi
{
    /********************************************************************************/
    //シリアライズ用データクラス：MRU
    /********************************************************************************/

    [Serializable]
    public class MRUList : IComparable
    {
        //ファイル名
        public string Name;

        //利用日時
        public DateTime Date;

        //ver1.37
        //最後に見たページ
        public int LastViewPage;

        //パッケージタイプ
        //public PackageType packageType;
        //ページ数
        //public int Pages;
        //表紙にするページ番号
        //public int coverPage;
        //Zipファイルサイズ
        //public long size;
        //MD5
        //public string MD5;

        //ver1.77 ブックマークを示す文字列。
        //ページ番号をcsv形式で文字列化
        public string Bookmarks { get; set; }

        /// <summary>
        /// 引数なしコンストラクタはシリアライズのために必要
        /// </summary>
        public MRUList()
        {
        }

        public MRUList(string s, DateTime d, int lastViewPage, string bookmarks)
        {
            //ファイル名
            Name = s;
            //最後に見た日時
            Date = d;
            //最後に見たページ
            LastViewPage = lastViewPage;
            Bookmarks = bookmarks;
        }

        int IComparable.CompareTo(object obj)
        {
            //古い順に並べる
            //return DateTime.Compare(Date, ((MRUList)obj).Date);
            return DateTime.Compare(Date, (obj as MRUList).Date);
        }
    }
}
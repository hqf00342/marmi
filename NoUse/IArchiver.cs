using System;
using System.IO;

namespace Marmi
{
    public interface IArchiver
    {
        //プロパティ
        bool isOpen { get; }                    //Archiveが開いているかどうか
        int itemCount { get; }          //アイテム数を返す
                                        //ArchiveItem this[int i] { get;}

        bool Open(string Filename);
        void Close();
        ArchiveItem Item(int num);
        Stream GetStream(string filename);

        //public DateTime GetDateTime(string filename)
    }
}
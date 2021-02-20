using System;
using System.IO;

namespace Marmi
{
    public interface IArchiver
    {
        //�v���p�e�B
        bool isOpen { get; }                    //Archive���J���Ă��邩�ǂ���
        int itemCount { get; }          //�A�C�e������Ԃ�
                                        //ArchiveItem this[int i] { get;}

        bool Open(string Filename);
        void Close();
        ArchiveItem Item(int num);
        Stream GetStream(string filename);

        //public DateTime GetDateTime(string filename)
    }
}
using System;

/*
�ŋߎg�����t�@�C��
�V���A���C�Y�p
*/

namespace Marmi
{
    [Serializable]
    public class MRUList : IComparable
    {
        /// <summary>�t�@�C����</summary>
        public string Name;

        /// <summary>���p����</summary>
        public DateTime Date;

        /// <summary>ver1.37 : �Ō�Ɍ����y�[�W</summary>
        public int LastViewPage;

        /// <summary>�u�b�N�}�[�N�ꗗ�Bcsv�e�L�X�g(ver1.77)</summary>
        public string Bookmarks { get; set; }

        /// <summary>�V���A���C�Y�̂��߂ɕK�v</summary>
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

        /// <summary>���t�Ń\�[�g(�Â���)</summary>
        int IComparable.CompareTo(object obj) => DateTime.Compare(Date, (obj as MRUList).Date);
    }
}
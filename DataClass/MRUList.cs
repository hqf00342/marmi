using System;

namespace Marmi
{
    /********************************************************************************/
    //�V���A���C�Y�p�f�[�^�N���X�FMRU
    /********************************************************************************/

    [Serializable]
    public class MRUList : IComparable
    {
        //�t�@�C����
        public string Name;

        //���p����
        public DateTime Date;

        //ver1.37
        //�Ō�Ɍ����y�[�W
        public int LastViewPage;

        //�p�b�P�[�W�^�C�v
        //public PackageType packageType;
        //�y�[�W��
        //public int Pages;
        //�\���ɂ���y�[�W�ԍ�
        //public int coverPage;
        //Zip�t�@�C���T�C�Y
        //public long size;
        //MD5
        //public string MD5;

        //ver1.77 �u�b�N�}�[�N������������B
        //�y�[�W�ԍ���csv�`���ŕ�����
        public string Bookmarks { get; set; }

        /// <summary>
        /// �����Ȃ��R���X�g���N�^�̓V���A���C�Y�̂��߂ɕK�v
        /// </summary>
        public MRUList()
        {
        }

        public MRUList(string s, DateTime d, int lastViewPage, string bookmarks)
        {
            //�t�@�C����
            Name = s;
            //�Ō�Ɍ�������
            Date = d;
            //�Ō�Ɍ����y�[�W
            LastViewPage = lastViewPage;
            Bookmarks = bookmarks;
        }

        int IComparable.CompareTo(object obj)
        {
            //�Â����ɕ��ׂ�
            //return DateTime.Compare(Date, ((MRUList)obj).Date);
            return DateTime.Compare(Date, (obj as MRUList).Date);
        }
    }
}
/*
PackageInfo.cs
�{���Ώۂ̉摜�ꗗ�N���X
�V���A���C�Y�Ή�

��ȃv���p�e�B
Items       �y�[�W�ꗗ
NowViewPage ���݂̃y�[�W
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Marmi
{
    [Serializable]
    public class PackageInfo // : IDisposable
    {
        /// <summary>Zip�t�@�C�����A�������̓f�B���N�g����</summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>���݌��Ă���A�C�e���ԍ�</summary>
        public int NowViewPage { get; set; } = 0;

        /// <summary>�摜�Z�b�g</summary>
        public List<ImageInfo> Items { get; } = new List<ImageInfo>();

        /// <summary>ver1.31 �p�b�P�[�W�̃^�C�v</summary>
        public PackageType PackType { get; set; } = PackageType.None;

        /// <summary>ver1.30 �y�[�W�������</summary>
        public bool PageDirectionIsLeft { get; set; } = true;

        /// <summary>ver1.09 ���ɂ̃��[�h</summary>
        [NonSerialized]
        public bool isSolid = false;

        /// <summary>���Ɉꎞ�W�J��f�B���N�g��</summary>
        [NonSerialized]
        public string tempDirname = string.Empty;

        /// <summary>
        /// �t�@�C��������C���f�b�N�X���擾����
        /// </summary>
        /// <param name="filename">�t�@�C����</param>
        /// <returns>�C���f�b�N�X�ԍ��B�����ꍇ��-1</returns>
        public int GetIndexFromFilename(string filename)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Filename == filename)
                    return i;
            return -1;
        }

        /// <summary>
        /// �L���b�V���ɂ���Bitmap��Ԃ�
        /// �����Ă��Ȃ����null��Ԃ�
        /// </summary>
        /// <param name="index">�Ώۂ̃C���f�b�N�X</param>
        /// <returns>Bitmap �������cache���Ȃ����null</returns>
        public Bitmap GetBitmapFromCache(int index)
        {
            return Items[index].CacheImage.ToBitmap();
        }

        /// <summary>
        /// FileCache���p�[�W����
        /// �I�[�o�[���Ă�����Max�̔����܂ŃN���A����B
        /// �E�A�C�h����Ԏ�
        /// �EAsyncLoadImageInfo()
        /// ����Ăяo����Ă���B
        /// </summary>
        /// <param name="MaxCacheSize">Max��������[MB]</param>
        public void FileCacheCleanUp2(int MaxCacheSize)
        {
            MaxCacheSize *= 1_000_000;    //MB�ϊ�

            //���ׂẴL���b�V�����N���A����
            //foreach (var i in Items)
            //    i.CacheImage.Clear();

            //���݂̃T�C�Y���v�Z
            int nowBufferSize = Items.Sum(i => i.CacheImage.Length);

            if (nowBufferSize <= MaxCacheSize)
                return;

            //�T�C�Y�I�[�o�[�����̂�MacCacheSize�̔����ɂȂ�܂ŊJ��

            //���݂̈ʒu����㉺�ɂ����̂ڂ��Ă���
            int now1 = NowViewPage;     //��ɂ��ǂ�|�C���^
            int now2 = NowViewPage + 1; //���ɂ��ǂ�|�C���^

            int sumBytes = 0;
            int halfSize = MaxCacheSize / 2;//�����̃T�C�Y�܂ŉ��

            while (now1 >= 0 || now2 < Items.Count)
            {
                //�O������
                if (now1 >= 0)
                {
                    sumBytes += Items[now1].CacheImage.Length;
                    if (sumBytes > halfSize)
                    {
                        Items[now1].CacheImage.Clear();
                    }
                    now1--;
                }

                //�������
                if (now2 < Items.Count)
                {
                    sumBytes += Items[now2].CacheImage.Length;
                    if (sumBytes > halfSize)
                    {
                        Items[now2].CacheImage.Clear();
                    }
                    now2++;
                }
            }

            //2021�N2��26�� GC����߂�
            //Uty.ForceGC();
        }

        /// <summary>
        /// �u�b�N�}�[�N�ꗗCSV���쐬����
        /// </summary>
        /// <returns>CSV�����ꂽ�u�b�N�}�[�N�B�y�[�W�ԍ��̗���</returns>
        public string CreateBookmarkString()
        {
            var bookmarks = Items.Where(c => c.IsBookMark).Select(c => c.OrgIndex);
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

        /// <summary>
        /// �u�b�N�}�[�NCSV�������ǂݍ��݃u�b�N�}�[�N�Ƃ���
        /// </summary>
        /// <param name="csv">�u�b�N�}�[�N������BCreateBookmarkString()�Ő������ꂽ����</param>
        public void LoadBookmarkString(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return;

            //csv��Int�z��ɕϊ�
            var bm = csv.Split(',').Select(c => { int.TryParse(c, out int r); return r; });

            //g_pi�ɓK�p
            for (int i = 0; i < Items.Count; i++)
            {
                if (bm.Contains(Items[i].OrgIndex))
                    Items[i].IsBookMark = true;
            }
        }

        public void ThrowIfOutOfRange(int index)
        {
            if (index < 0 || index >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
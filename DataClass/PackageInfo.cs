using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
/*
�{���Ώۂ̉摜�ꗗ�N���X
�V���A���C�Y�Ή�
*/
namespace Marmi
{
    [Serializable]
    public class PackageInfo // : IDisposable
    {
        /// <summary>Zip�t�@�C�����A�������̓f�B���N�g����</summary>
        public string PackageName { get; set; }

        /// <summary>���݌��Ă���A�C�e���ԍ�</summary>
        public int NowViewPage { get; set; }

        /// <summary>Zip�t�@�C���T�C�Y</summary>
        public long PackageSize { get; set; }

        /// <summary>Zip�t�@�C���쐬��</summary>
        public DateTime CreateDate { get; set; }

        /// <summary>�T���l�C���摜�W</summary>
        public List<ImageInfo> Items { get; } = new List<ImageInfo>();

        /// <summary>ver1.31 �p�b�P�[�W�̃^�C�v</summary>
        public PackageType PackType { get; set; } = PackageType.None;

        /// <summary>ver1.30 �y�[�W�������</summary>
        public bool PageDirectionIsLeft { get; set; }

        /// <summary>ver1.09 ���ɂ̃��[�h</summary>
        [NonSerialized]
        public bool isSolid;

        /// <summary>���Ɉꎞ�W�J��f�B���N�g��</summary>
        [NonSerialized]
        public string tempDirname;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PackageInfo()
        {
            Initialize();
        }

        /// <summary>
        /// ���������[�`��
        /// </summary>
        public void Initialize()
        {
            PackageName = string.Empty;
            PackType = PackageType.None;
            isSolid = false;
            NowViewPage = 0;
            PackageSize = 0;
            CreateDate = DateTime.MinValue;
            tempDirname = string.Empty;

            PageDirectionIsLeft = true;

            //�t�@�C���L���b�V�����N���A
            foreach (var item in Items)
                item.CacheImage.Clear();

            Items.Clear();
        }

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
            return HasCacheImage(index) ? Items[index].CacheImage.ToBitmap() : null;
        }

        public bool HasCacheImage(int index)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            return Items[index].CacheImage.HasImage;
        }

        public void ClearCache(int index)
        {
            Items[index].CacheImage.Clear();
        }

        /// <summary>
        /// �T���l�C���̍쐬�E�o�^
        /// ����1�����ōs���B(2021�N2��25��)
        /// ���݂�AsyncIO�ōs���Ă��邪�{����Cache�o�^�������ōs�������B
        /// </summary>
        /// <param name="index"></param>
        public void ThumnailMaker(int index)
        {
            if (index < 0 || index >= Items.Count) return;

            try
            {
                if (Items[index].CacheImage.HasImage)
                {
                    //ver1.10 �T���l�C���o�^���s��
                    Bitmap _bmp = Items[index].CacheImage.ToBitmap();
                    if (_bmp != null)
                    {
                        Items[index].ResisterThumbnailImage(_bmp);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// FileCache���p�[�W����
        /// </summary>
        /// <param name="MaxCacheSize">�c���Ă�����������[MB]</param>
        public void FileCacheCleanUp2(int MaxCacheSize)
        {
            MaxCacheSize *= 1_000_000;    //MB�ϊ�

            switch (App.Config.memModel)
            {
                case MemoryModel.Small:
                    //���ׂẴL���b�V�����N���A����
                    foreach (var i in Items)
                        i.CacheImage.Clear();
                    break;

                case MemoryModel.Large:
                    //�\�Ȍ���c���Ă���
                    break;

                case MemoryModel.UserDefined:
                    //���݂̃T�C�Y���v�Z
                    int nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.CacheImage.Length;
                    if (nowBufferSize <= MaxCacheSize)
                        break;

                    //�傫���̂ŏ���
                    //���݂̈ʒu����㉺�ɂ����̂ڂ��Ă���
                    int nowup = NowViewPage;
                    int nowdown = NowViewPage + 1;
                    int sumBytes = 0;
                    int thresholdSize = MaxCacheSize / 2;//�����̃T�C�Y�܂ŉ��

                    Debug.WriteLine($"FileCacheCleanUp() start: {nowBufferSize:N0}bytes");
                    while (nowup >= 0 || nowdown < Items.Count)
                    {
                        if (nowup >= 0)
                        {
                            sumBytes += Items[nowup].CacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowup].CacheImage.Length > 0)
                                {
                                    Debug.WriteLine($"FileCacheCleanUp():target={nowup}");
                                    Items[nowup].CacheImage.Clear();
                                }
                            }
                            nowup--;
                        }
                        if (nowdown < Items.Count)
                        {
                            sumBytes += Items[nowdown].CacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowdown].CacheImage.Length > 0)
                                {
                                    Debug.WriteLine($"FileCacheCleanUp():target={nowdown}");
                                    Items[nowdown].CacheImage.Clear();
                                }
                            }
                            nowdown++;
                        }
                    }//while
                    nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.CacheImage.Length;
                    Debug.WriteLine($"FileCacheCleanUp() end: {nowBufferSize:N0}bytes");
                    //2021�N2��26�� GC����߂�
                    //Uty.ForceGC();
                    break;

                default:
                    break;
            }
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
    }
}
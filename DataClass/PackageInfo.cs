using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;	//Bookmark�̏W�v�̂��߂ɒǉ�
using System.Threading;
/*
�{���Ώۂ̉摜�ꗗ�N���X
�V���A���C�Y�Ή�
*/
namespace Marmi
{
    [Serializable]
    public class PackageInfo // : IDisposable
    {
        //Zip�t�@�C�����A�������̓f�B���N�g����
        public string PackageName { get; set; }

        //���݌��Ă���A�C�e���ԍ�
        public int NowViewPage { get; set; }

        //Zip�t�@�C���T�C�Y
        public long PackageSize { get; set; }

        //Zip�t�@�C���쐬��
        public DateTime CreateDate { get; set; }

        //�T���l�C���摜�W
        public List<ImageInfo> Items { get; } = new List<ImageInfo>();

        //ver1.31 �p�b�P�[�W�̃^�C�v
        public PackageType PackType { get; set; } = PackageType.None;

        //ver1.30 �y�[�W�������
        public bool PageDirectionIsLeft { get; set; }

        //ver1.09 ���ɂ̃��[�h
        [NonSerialized]
        public bool isSolid;

        //���Ɉꎞ�W�J��
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

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].Dispose(); //�T���l�C���摜���N���A
                Items.Clear();
            }

            //�t�@�C���L���b�V�����N���A
            foreach (var item in Items)
                item.cacheImage.Clear();
        }

        /// <summary>
        /// �t�@�C��������C���f�b�N�X���擾����
        /// </summary>
        /// <param name="filename">�t�@�C����</param>
        /// <returns>�C���f�b�N�X�ԍ��B�����ꍇ��-1</returns>
        public int GetIndexFromFilename(string filename)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].filename == filename)
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
            return hasCacheImage(index) ? Items[index].cacheImage.ToBitmap() : null;
        }

        public bool hasCacheImage(int index)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            return Items[index].cacheImage.hasImage;
        }

        /// <summary>
        /// �L���b�V���Ƀt�@�C����ǂݍ���
        /// </summary>
        /// <param name="index"></param>
        /// <param name="_7z"></param>
        /// <returns></returns>
        public bool LoadCache(int index, SevenZipWrapper _7z)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            string filename = Items[index].filename;

            if (PackType != PackageType.Archive)
            {
                //�ʏ�t�@�C������̓ǂݍ���
                Items[index].cacheImage.Load(filename);
            }
            else if (isSolid && App.Config.isExtractIfSolidArchive)
            {
                //ver1.10 �\���b�h���� �ꎞ�t�H���_����ǂݎ������݂�
                string tempname = Path.Combine(tempDirname, filename);
                Items[index].cacheImage.Load(tempname);
            }
            else
            {
                //ver1.05 Solid���ɂł͂Ȃ����Ƀt�@�C�����[�h
                try
                {
                    if (_7z == null)
                        _7z = new SevenZipWrapper();
                    if (_7z.Open(PackageName))
                    {
                        Items[index].cacheImage.Load(_7z.GetStream(filename));
                    }
                    else
                        return false;
                }
                catch (IOException e)
                {
                    //7zTemp�W�J���ɃA�N�Z�X���ꂽ�P�[�X�Ƒz��
                    //�t�@�C�����Ȃ��������̂Ƃ���null��Ԃ�
                    Debug.WriteLine(e.Message, "!Exception! " + e.TargetSite);
                    return false;
                }
            }
            //�摜�T�C�Y��ݒ�
            Items[index].bmpsize = Items[index].cacheImage.GetImageSize();

            //�T���l�C���o�^��ThreadPool��
            //AsyncThumnailMaker(index);

            return true;
        }

        public void AsyncThumnailMaker(int index)
        {
            //ver1.73 index check
            if (index > Items.Count) return;

            if (Items[index].cacheImage.hasImage)
            {
                ThreadPool.QueueUserWorkItem(dummy =>
                {
                    try
                    {
                        //ver1.10 �T���l�C���o�^���s��
                        Bitmap _bmp = Items[index].cacheImage.ToBitmap();
                        if (_bmp != null)
                        {
                            Items[index].resisterThumbnailImage(_bmp);

                            //ver1.81�R�����g�A�E�g�B����������
                            //_bmp.Dispose();
                        }
                    }
                    catch { }
                });
            }
        }

        /// <summary>
        /// �T���l�C���o�^
        /// ���łɉ摜�������Ă���ꍇ�͂�����g��
        /// </summary>
        /// <param name="index">�摜�̃C���f�b�N�X</param>
        /// <param name="orgBitmap">���摜</param>
        public void AsyncThumnailMaker(int index, Bitmap orgBitmap)
        {
            //ver1.73 index check
            if (index > Items.Count) return;
            if (orgBitmap == null) return;

            ThreadPool.QueueUserWorkItem(dummy =>
            {
                Items[index].resisterThumbnailImage(orgBitmap);
                //TODO:�摜��Dispose()���ׂ�
                orgBitmap.Dispose();
            });
        }

        public void ThumnailMaker(int index, Bitmap bmp)
        {
            //ver1.73 index check
            if (index > Items.Count) return;

            if (bmp != null)
                Items[index].resisterThumbnailImage(bmp);
        }

        /// <summary>
        /// FileCache���p�[�W����
        /// </summary>
        /// <param name="MaxCacheSize">�c���Ă�����������[MB]</param>
        public void FileCacheCleanUp2(int MaxCacheSize)
        {
            MaxCacheSize *= 1000000;    //MB�ϊ�
            switch (App.Config.memModel)
            {
                case MemoryModel.Small:
                    //���ׂẴL���b�V�����N���A����
                    foreach (var i in Items)
                        i.cacheImage.Clear();
                    break;

                case MemoryModel.Large:
                    //�\�Ȍ���c���Ă���
                    break;

                case MemoryModel.UserDefined:
                    //���݂̃T�C�Y���v�Z
                    int nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.cacheImage.Length;
                    if (nowBufferSize <= MaxCacheSize)
                        break;

                    //�傫���̂ŏ���
                    //���݂̈ʒu����㉺�ɂ����̂ڂ��Ă���
                    int nowup = NowViewPage;
                    int nowdown = NowViewPage + 1;
                    int sumBytes = 0;
                    int thresholdSize = MaxCacheSize / 2;//�����̃T�C�Y�܂ŉ��

                    Uty.WriteLine("FileCacheCleanUp() start: {0:N0}bytes", nowBufferSize);
                    while (nowup >= 0 || nowdown < Items.Count)
                    {
                        if (nowup >= 0)
                        {
                            sumBytes += Items[nowup].cacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowup].cacheImage.Length > 0)
                                {
                                    Uty.WriteLine("FileCacheCleanUp():target={0}", nowup);
                                    Items[nowup].cacheImage.Clear();
                                }
                            }
                            nowup--;
                        }
                        if (nowdown < Items.Count)
                        {
                            sumBytes += Items[nowdown].cacheImage.Length;
                            if (sumBytes > thresholdSize)
                            {
                                if (Items[nowdown].cacheImage.Length > 0)
                                {
                                    Uty.WriteLine("FileCacheCleanUp():target={0}", nowdown);
                                    Items[nowdown].cacheImage.Clear();
                                }
                            }
                            nowdown++;
                        }
                    }//while
                    nowBufferSize = 0;
                    foreach (var i in Items)
                        nowBufferSize += i.cacheImage.Length;
                    Uty.WriteLine("FileCacheCleanUp() end: {0:N0}bytes", nowBufferSize);
                    Uty.ForceGC();
                    break;

                default:
                    break;
            }
        }

        public string GetCsvFromBookmark()
        {
            //bookmark���ꂽorgIndex���E���Ă���B
            var bookmarks = Items.Where(c => c.isBookMark).Select(c => c.nOrgIndex);

            //Int�z���csv�ɕϊ�
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

        public void SetBookmarksFromCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return;

            //csv��Int�z��ɕϊ�
            var bm = csv.Split(',').Select(c => { int.TryParse(c, out int r); return r; });

            //g_pi�ɓK�p
            for (int i = 0; i < Items.Count; i++)
            {
                if (bm.Contains(Items[i].nOrgIndex))
                    Items[i].isBookMark = true;
            }
        }
    }
}
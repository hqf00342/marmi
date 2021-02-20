using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;	//Bookmark�̏W�v�̂��߂ɒǉ�
using System.Threading;

namespace Marmi
{
    /********************************************************************************/
    //�V���A���C�Y�p�f�[�^�N���X�F���Ă���摜�Z�b�g
    /********************************************************************************/

    [Serializable]
    public class PackageInfo // : IDisposable
    {
        //Zip�t�@�C�����A�������̓f�B���N�g����
        public string PackageName;

        //zip�t�@�C�����ǂ���
        //public bool isZip;

        //���݌��Ă���A�C�e���ԍ�
        public int NowViewPage;

        //Zip�t�@�C���T�C�Y
        public long size;

        //Zip�t�@�C���쐬��
        public DateTime date;

        //�T���l�C���摜�W
        public List<ImageInfo> Items = new List<ImageInfo>();

        //ver1.31 �p�b�P�[�W�̃^�C�v
        public PackageType packType = PackageType.None;

        //ver1.30 �y�[�W�������
        public bool LeftBook;

        //ver1.09 ���ɂ̃��[�h
        [NonSerialized]
        public bool isSolid;

        //���Ɉꎞ�W�J��
        [NonSerialized]
        public string tempDirname;

        //SevenZip���ɃC���X�^���X
        //private SevenZipWrapper m_szw = null;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PackageInfo()
        {
            Uty.WriteLine("�R���X�g���N�^");
            Initialize();
        }

        /// <summary>
        /// ���������[�`��
        /// </summary>
        public void Initialize()
        {
            Clear();
            //m_szw = new SevenZipWrapper();
            //Uty.WriteLine("make m_szw in Initialize");
        }

        public void Clear()
        {
            PackageName = string.Empty;
            packType = PackageType.None;
            isSolid = false;
            NowViewPage = 0;
            size = 0;
            date = DateTime.MinValue;
            tempDirname = string.Empty;
            //PackageMode = Mode.None;
            //isZip = false;

            LeftBook = true;

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].Dispose(); //�T���l�C���摜���N���A
                Items.Clear();
            }

            //�t�@�C���L���b�V�����N���A
            //g_FileCache.Clear();
            foreach (var item in Items)
                item.cacheImage.Clear();

            //7z���N���A
            //if (m_szw != null)
            //{
            //    m_szw.Close();
            //    m_szw = null;
            //    Uty.WriteLine("m_szw clear");
            //}
        }

        //loadThumbnailDBFile()�̂��߂����ɐ���
        //public void InitCache()
        //{
        //    if (g_FileCache == null)
        //        g_FileCache = new BitmapCache();
        //}

        //public void Dispose()
        //{
        //    if (Items.Count > 0)
        //    {
        //        for (int i = 0; i < Items.Count; i++)
        //            Items[i].Dispose();	//�T���l�C���摜���N���A
        //        Items.Clear();
        //    }
        //    //Items.Clear();

        //    g_FileCache.Clear();

        //    if (m_szw != null)
        //    {
        //        m_szw.Close();
        //    }
        //}

        /// <summary>
        /// �w��C���f�b�N�X�����������ǂ����`�F�b�N����
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private void CheckIndex(int index)
        {
            Debug.Assert(index >= 0 && index < Items.Count);
            //return (index >= 0 && index < Items.Count);
        }

        /// <summary>
        /// �t�@�C��������C���f�b�N�X���擾����
        /// </summary>
        /// <param name="name">�t�@�C����</param>
        /// <returns>�C���f�b�N�X�ԍ��B�����ꍇ��-1</returns>
        public int GetIndexFromFilename(string name)
        {
            int z = Items.Count;
            for (int i = 0; i < z; i++)
                if (Items[i].filename == name)
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
            //ver1.70 Index�`�F�b�N
            if (index < 0 || index >= Items.Count)
                return null;

            if (Items[index].cacheImage.hasImage)
                return Items[index].cacheImage.bitmap;
            else
                return null;
        }

        public bool hasCacheImage(int index)
        {
            if (index < 0 || index >= Items.Count)
                return false;

            if (Items[index].cacheImage.hasImage)
                return true;
            else
                return false;
        }

        /// <summary>
        /// �w��C���f�b�N�X��Bitmap�𓾂�B
        /// �����Ȃ��ꍇ��null��Ԃ�
        /// </summary>
        /// <param name="index">�擾����Item�ԍ�</param>
        /// <returns>����ꂽBitmap</returns>
        //public Bitmap GetBitmap(int index)
        //{
        //    //if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
        //    CheckIndex(index);

        //    //�L���b�V���Ƀt�@�C�������邩�ǂ����`�F�b�N
        //    string filename = Items[index].filename;
        //    //if (Items[index].cacheImage.bitmap != null)
        //    if (Items[index].cacheImage.hasImage)
        //    {
        //        //�L���b�V���Ƀt�@�C��������̂ł�����g��
        //        Debug.WriteLine(filename, "GetBitmap() from CACHE");
        //        return Items[index].cacheImage.bitmap;
        //    }
        //    else
        //    {
        //        //�L���b�V���Ƀt�@�C�����Ȃ��̂Ŏ���Ă���
        //        try
        //        {
        //            //��������łɃL���b�V���ɒǉ�
        //            if (m_szw == null)
        //            {
        //                m_szw = new SevenZipWrapper();
        //                Uty.WriteLine("make m_szw in GetBitmap(int index)");
        //            }
        //            Bitmap bmp = GetBitmapWithoutCache(index, m_szw);
        //            if (bmp == null)
        //            {
        //                Debug.WriteLine(filename, "GetBitmap() CANNOT load");
        //                return null;
        //            }

        //            Debug.WriteLine(filename, "GetBitmap() Load");
        //            return bmp;
        //        }
        //        catch (ArgumentException e)
        //        {
        //            //�L���b�V���d����null�Ԃ��̓��b�^�C�i�C
        //            Debug.WriteLine(e.Message, "�L���b�V���d���G���[");
        //            return Items[index].cacheImage.bitmap;
        //            //return g_FileCache[filename];
        //        }
        //    }
        //}

        /// <summary>
        /// �L���b�V���Ȃ��ł̓ǂݍ��݃��[�`��
        /// �ǂݍ��߂Ȃ������Ƃ��͌���null��Ԃ��d�l
        /// ver1.10 �ꎞ�t�H���_������ꍇ�͂�������ǂݍ���
        /// �@�@�@�@��������ǂݍ��߂Ȃ��ꍇ��null��Ԃ�
        /// </summary>
        /// <param name="index">�C���f�b�N�X</param>
        /// <returns>����ꂽBitmap�I�u�W�F�N�g�B�����ꍇ��null</returns>
        //[Obsolete]
        //public Bitmap GetBitmapWithoutCache(int index, SevenZipWrapper _7z)
        //{
        //    LoadCache(index, _7z);

        //    //ver1.10 �T���l�C���o�^���s��
        //    Bitmap _bmp = Items[index].cacheImage.bitmap;
        //    //if (_bmp != null)
        //    //    Items[index].resisterThumbnailImage(_bmp);

        //    return _bmp;
        //}

        /// <summary>
        /// �L���b�V���Ƀt�@�C����ǂݍ���
        /// </summary>
        /// <param name="index"></param>
        /// <param name="_7z"></param>
        /// <returns></returns>
        public bool LoadCache(int index, SevenZipWrapper _7z)
        {
            CheckIndex(index);

            string filename = Items[index].filename;

            if (packType != PackageType.Archive)
            {
                //�ʏ�t�@�C������̓ǂݍ���
                Items[index].cacheImage.Add(filename);
            }
            else if (isSolid && Form1.g_Config.isExtractIfSolidArchive)
            {
                //ver1.10 �\���b�h���� �ꎞ�t�H���_����ǂݎ������݂�
                string tempname = Path.Combine(tempDirname, filename);
                Items[index].cacheImage.Add(tempname);
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
                        Items[index].cacheImage.Add(_7z.GetStream(filename));
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
                        Bitmap _bmp = Items[index].cacheImage.bitmap;
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
            switch (Form1.g_Config.memModel)
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

        public string getBookmarks()
        {
            //bookmark���ꂽorgIndex���E���Ă���B
            var bookmarks = Items.Where(c => c.isBookMark).Select(c => c.nOrgIndex);

            //csv�ɕϊ�
            //string s = string.Empty;
            //foreach(var b in bookmarks)
            //{
            //	s = s + b.ToString() + ",";
            //}
            //s = s.Trim(',');

            //Int�z���csv�ɕϊ�
            return string.Join(",", bookmarks.Select(c => c.ToString()).ToArray());
        }

        public void setBookmarks(string csv)
        {
            //�󂾂�����Ȃɂ����Ȃ��BNullReference�΍�
            if (string.IsNullOrEmpty(csv))
                return;

            //csv��Int�z��ɕϊ�
            var bm = csv.Split(',').Select(c => { int r; Int32.TryParse(c, out r); return r; });

            //g_pi�ɓK�p
            for (int i = 0; i < Items.Count; i++)
            {
                if (bm.Contains(Items[i].nOrgIndex))
                    Items[i].isBookMark = true;
            }
        }
    }
}
/*
ImageInfo
�摜���ƃL���b�V����ۑ����邽�߂̃N���X
*/

using Marmi.DataModel;
using System;
using System.Drawing;
using System.IO;

namespace Marmi
{
    [Serializable()]
    public class ImageInfo //: IDisposable
    {
        /// <summary>�t�@�C����</summary>
        public string Filename { get; }

        /// <summary>�I���W�i���摜�T�C�Y</summary>
        public Size ImgSize { get; set; } = Size.Empty;

        public int Width => ImgSize.Width;
        public int Height => ImgSize.Height;

        /// <summary>�t�@�C���쐬���B���\���ƃ\�[�g�Ɏg��</summary>
        public DateTime CreateDate { get; }

        /// <summary>�o�C�g��</summary>
        public long FileLength { get; }

        /// <summary>���ɓ��̏���</summary>
        public int OrgIndex { get; }

        /// <summary>�摜�L���b�V���B�L���b�V����������Ɏ��� 2013/01/13</summary>
        [NonSerialized]
        public RawImage CacheImage = new RawImage();

        /// <summary>�T���l�C���摜</summary>
        [field: NonSerialized]
        public Bitmap Thumbnail { get; set; } = null;

        public Exif Exif { get; set; } = new Exif();

        /// <summary>������ 2011�N10��2��</summary>
        public bool IsBookMark { get; set; } = false;

        //��]��� 2011�N12��24��
        private int _rotate = 0;

        public int Rotate
        {
            get => _rotate;
            set => _rotate = value % 360;
        }

        //var1.54 �������ǂ���
        public bool IsFat => Width > Height;

        public ImageInfo(int index, string filename, DateTime creationDate, long bytes)
        {
            OrgIndex = index;
            Filename = filename;
            CreateDate = creationDate;
            FileLength = bytes;
        }

        public ImageInfo(int index, string filename)
        {
            var fi = new FileInfo(filename);
            OrgIndex = index;
            Filename = filename;
            CreateDate = fi.CreationTime;
            FileLength = fi.Length;
        }

        /// <summary>
        /// ver1.10 �T���l�C���摜���I���W�i���摜����o�^����B
        /// Bitmap�����̉�������̂�Exif�������Ɏ擾
        /// </summary>

        public void CreateThumbnail()
        {
            if (Thumbnail != null)
                return;
            var bmp = CacheImage.ToBitmap();
            this.Thumbnail = BitmapUty.MakeHeightFixThumbnailImage(bmp, App.DEFAULT_THUMBNAIL_SIZE);

            //��������BMP��������̂�Exif���o�^����
            Exif.GetExifInfo(bmp);
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
/*
�T���l�C���摜��ۑ����邽�߂̃N���X
Exif����摜�f�[�^�����W����ȂǍ����ɃT���l�C�����擾����
*/
namespace Marmi
{
    [Serializable()]
    public class ImageInfo //: IDisposable
    {
        //�T���l�C���摜�̃T�C�Y�B�ő�l
        //private readonly int THUMBNAIL_WIDTH = App.DEFAULT_THUMBNAIL_SIZE;

        private readonly int THUMBNAIL_HEIGHT = App.DEFAULT_THUMBNAIL_SIZE;

        /// <summary>�t�@�C����</summary>
        public string Filename { get; }

        /// <summary>�I���W�i���摜�T�C�Y</summary>
        public Size ImgSize { get; set; } = Size.Empty;

        public int Width => ImgSize.Width;
        public int Height => ImgSize.Height;

        /// <summary>�t�@�C���쐬��</summary>
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

        ///// <summary>�A�j���[�V�����^�C�}�[ DateTime.Now.Ticks</summary>
        //[field: NonSerialized]
        //public long AnimateStartTime { get; set; } = 0;

        /// <summary>EXIF: ISO�l</summary>
        public int ExifISO { get; private set; }

        /// <summary>EXIF: �B�e��</summary>
        public string ExifDate { get; private set; }

        /// <summary>EXIF: ���[�J�[</summary>
        public string ExifMake { get; private set; }

        /// <summary>EXIF: ���f��</summary>
        public string ExifModel { get; private set; }

        /// <summary>������ 2011�N10��2��</summary>
        public bool IsBookMark { get; set; } = false;

        //��]��� 2011�N12��24��
        //private int _rotate = 0;
        //public int Rotate
        //{
        //    get { return _rotate; }
        //    set { _rotate = value % 360; }
        //}

        //ver1.36 �\�������邩�ǂ���
        public bool IsVisible { get; set; } = true;

        //ver1.51 �摜���������Ă��邩
        public bool HasInfo => Width != 0;

        //ver1.54 �c�����ǂ���
        public bool IsTall => Height > Width;

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
        /// ���LoadImage()���g��Ȃ����邽�߂ɍ쐬
        /// </summary>
        /// <param name="orgImage">���摜</param>
        public void ResisterThumbnailImage(Bitmap orgImage)
        {
            //�o�^�ς݂Ȃ牽�����Ȃ�
            if (Thumbnail != null)
                return;

            if (orgImage == null)
            {
                Thumbnail = null;
            }
            else
            {
                //ver1.26 �����Œ�̃T���l�C�������
                this.Thumbnail = BitmapUty.MakeHeightFixThumbnailImage(orgImage, THUMBNAIL_HEIGHT);
                GetExifInfo(orgImage);
            }
        }

        /// <summary>
        /// Exif���̎擾�B
        /// http://cachu.xrea.jp/perl/ExifTAG.html
        /// http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
        /// http://exif.org/specifications.html
        /// </summary>
        /// <param name="img">�Ώۂ̉摜</param>
        private void GetExifInfo(Image img)
        {
            foreach (PropertyItem pi in img.PropertyItems)
            {
                switch (pi.Id)
                {
                    case 0x8827: //ISO
                        ExifISO = BitConverter.ToUInt16(pi.Value, 0);
                        break;

                    case 0x9003: //�B�e�����uYYYY:MM:DD HH:MM:SS�v�`��19����
                        ExifDate = Encoding.ASCII.GetString(pi.Value, 0, 19);
                        break;
                    case 0x010f: //Make
                        ExifMake = Encoding.ASCII.GetString(pi.Value);
                        ExifMake = ExifMake.Trim(new char[] { '\0' });
                        break;

                    case 0x0110: //Model
                        ExifModel = Encoding.ASCII.GetString(pi.Value).Trim(new char[] { '\0' });
                        break;
                }
            }
        }
    }
}
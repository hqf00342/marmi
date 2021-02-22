using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
/*
�T���l�C���摜��ۑ����邽�߂̃N���X
Exif����摜�f�[�^�����W����ȂǍ����ɃT���l�C�����擾����
*/
namespace Marmi
{
    [Serializable()]
    public class ImageInfo : IDisposable
    {
        //�T���l�C���摜�̃T�C�Y�B�ő�l
        private int THUMBNAIL_WIDTH = App.DEFAULT_THUMBNAIL_SIZE;

        private int THUMBNAIL_HEIGHT = App.DEFAULT_THUMBNAIL_SIZE;

        //�t�@�C����
        public string filename;

        //�I���W�i���摜�T�C�Y
        public Size bmpsize = Size.Empty;

        public int Width { get { return bmpsize.Width; } }
        public int Height { get { return bmpsize.Height; } }

        //�쐬��
        public DateTime createDate;

        //�o�C�g��
        public long length;

        //�\�[�g�O�̏���
        public int nOrgIndex;

        //�L���b�V����������Ɏ��� 2013/01/13
        [NonSerialized]
        public RawImage cacheImage = new RawImage();

        //ver1.26 Jpeg��Serialize���邽�߂ɕύX
        private readonly JpegSerializedImage _thumbImage = new JpegSerializedImage();

        public Bitmap Thumbnail
        {
            get { return _thumbImage.Bitmap as Bitmap; }
            set { _thumbImage.Bitmap = value; }
        }

        //�A�j���[�V�����^�C�}�[ DateTime.Now.Ticks
        [NonSerialized]
        public long animateStartTime = 0;

        //EXIF
        public int ExifISO;

        public string ExifDate;
        public string ExifMake;
        public string ExifModel;

        //������ 2011�N10��2��
        public bool isBookMark = false;

        //��]��� 2011�N12��24��
        private int _rotate = 0;

        public int Rotate
        {
            get { return _rotate; }
            set { _rotate = value % 360; }
        }

        //ver1.36 �\�������邩�ǂ���
        public bool isVisible;

        //ver1.51 �摜���������Ă��邩
        public bool HasInfo { get { return Width != 0; } }

        //ver1.54 �c�����ǂ���
        public bool IsTall { get { return Height > Width; } }

        //var1.54 �������ǂ���
        public bool IsFat { get { return Width > Height; } }

        public ImageInfo(int index, string name, DateTime date, long bytes)
        {
            //������
            Thumbnail = null;
            isVisible = true;
            bmpsize = Size.Empty;
            animateStartTime = 0;

            nOrgIndex = index;
            filename = name;
            createDate = date;
            length = bytes;
        }

        ~ImageInfo()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Thumbnail != null)
                Thumbnail.Dispose();
            Thumbnail = null;
            if (cacheImage != null)
                cacheImage.Clear();
        }

        //-------------------------------------------
        // ���\�b�h

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
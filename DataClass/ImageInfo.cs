using System;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Drawing.Imaging;			// fadePictureBox ColorMatrix
using System.Text;

//using System.Xml.Serialization;		// XmlSerializer

namespace Marmi
{
    /********************************************************************************/
    //�T���l�C���摜��ۑ����邽�߂̃N���X
    /********************************************************************************/

    /// <summary>
    /// �T���l�C���摜��ۑ����邽�߂̃N���X�B
    /// Exif����摜�f�[�^�����W����ȂǍ����ɃT���l�C�����擾����
    /// </summary>
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

        public int width { get { return bmpsize.Width; } }
        public int height { get { return bmpsize.Height; } }
        //public int width = 0;
        //public int height = 0;

        //�쐬��
        public DateTime createDate;

        //�o�C�g��
        public long length;

        //�\�[�g�O�̏���
        public int nOrgIndex;

        //�L���b�V����������Ɏ��� 2013/01/13
        [NonSerialized]
        public RawImage cacheImage = new RawImage();

        //public Bitmap image {
        //    get
        //    {
        //        if (cacheImage.Length == 0)
        //            return null;
        //        else
        //            return cacheImage.bitmap;
        //    }
        //}

        //ver1.26 Jpeg��Serialize���邽�߂ɕύX
        private JpegSerializedImage _thumbImage = new JpegSerializedImage();

        public Bitmap thumbnail
        {
            get { return _thumbImage.bitmap as Bitmap; }
            set { _thumbImage.bitmap = value; }
        }

        //ver1.56 SmallBitmap��thumbnail
        //private SmallBitmap _thumb = new SmallBitmap();
        //public Bitmap thumbnail
        //{
        //    get { return _thumb.bitmap; }
        //    set { _thumb.Add(value); }
        //}

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

        public int rotate
        {
            get { return _rotate; }
            set { _rotate = value % 360; }
        }

        //ver1.36 �\�������邩�ǂ���
        public bool isVisible;

        //�T���l�C���p�l�����ł̈ʒu
        //[NonSerialized()]
        //public Point ThumbnailPos;

        //ver1.51 �摜���������Ă��邩
        public bool hasInfo { get { return width != 0; } }

        //ver1.54 �c�����ǂ���
        public bool isTall { get { return height > width; } }

        //var1.54 �������ǂ���
        public bool isFat { get { return width > height; } }

        public ImageInfo(int index, string name, DateTime date, long bytes)
        {
            //������
            thumbnail = null;
            isVisible = true;
            //width = 0;
            //height = 0;
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
            if (thumbnail != null)
                thumbnail.Dispose();
            thumbnail = null;
            if (cacheImage != null)
                cacheImage.Clear();
        }

        //-------------------------------------------
        // ���\�b�h

        /// <summary>
        /// ver1.10 �T���l�C���摜���I���W�i���摜����o�^����B
        /// ���LoadImage()���g��Ȃ����邽�߂ɍ쐬
        /// </summary>
        /// <param name="orgSizeBitmap"></param>
        public void resisterThumbnailImage(Bitmap orgSizeBitmap)
        {
            //�o�^�ς݂Ȃ牽�����Ȃ�
            if (thumbnail != null)
                return;

            //�O�̂��߁B�o�^�摜���Ȃ���΃N���A
            if (orgSizeBitmap == null)
            {
                thumbnail = null;
                //width = 0;
                //height = 0;
                return;
            }

            //ver1.26 �����Œ�̃T���l�C�������
            this.thumbnail = BitmapUty.MakeHeightFixThumbnailImage(orgSizeBitmap, THUMBNAIL_HEIGHT);
            //this.width = orgSizeBitmap.Width;
            //this.height = orgSizeBitmap.Height;
            GetExifInfo(orgSizeBitmap);
        }

        private void GetExifInfo(Image orgImage)
        {
            //Exif���̎擾�B
            //http://cachu.xrea.jp/perl/ExifTAG.html
            //http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
            //http://exif.org/specifications.html
            foreach (PropertyItem pi in orgImage.PropertyItems)
            {
                switch (pi.Id)
                {
                    case 0x8827: //ISO
                        ExifISO = BitConverter.ToUInt16(pi.Value, 0);
                        break;

                    case 0x9003: //�B�e����
                                 //�uYYYY:MM:DD HH:MM:SS�v�`��19����
                        ExifDate = Encoding.ASCII.GetString(pi.Value, 0, 19);
                        //date = date.Replace(':', '-').Replace(' ', '_');
                        break;
                    //DateTime dt = DateTime.ParseExact(val, "yyyy:MM:dd HH:mm:ss", null);
                    case 0x010f: //Make
                        ExifMake = Encoding.ASCII.GetString(pi.Value);
                        ExifMake = ExifMake.Trim(new char[] { '\0' });
                        break;

                    case 0x0110: //Model
                        ExifModel = Encoding.ASCII.GetString(pi.Value);
                        ExifModel = ExifModel.Trim(new char[] { '\0' });
                        break;
                }//switch
            }
        }
    }
}
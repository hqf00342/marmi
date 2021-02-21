using System;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Drawing.Imaging;			// fadePictureBox ColorMatrix
using System.Text;

//using System.Xml.Serialization;		// XmlSerializer

namespace Marmi
{
    /********************************************************************************/
    //サムネイル画像を保存するためのクラス
    /********************************************************************************/

    /// <summary>
    /// サムネイル画像を保存するためのクラス。
    /// Exifから画像データを収集するなど高速にサムネイルを取得する
    /// </summary>
    [Serializable()]
    public class ImageInfo : IDisposable
    {
        //サムネイル画像のサイズ。最大値
        private int THUMBNAIL_WIDTH = App.DEFAULT_THUMBNAIL_SIZE;

        private int THUMBNAIL_HEIGHT = App.DEFAULT_THUMBNAIL_SIZE;

        //ファイル名
        public string filename;

        //オリジナル画像サイズ
        public Size bmpsize = Size.Empty;

        public int width { get { return bmpsize.Width; } }
        public int height { get { return bmpsize.Height; } }
        //public int width = 0;
        //public int height = 0;

        //作成日
        public DateTime createDate;

        //バイト数
        public long length;

        //ソート前の順番
        public int nOrgIndex;

        //キャッシュをこちらに持つ 2013/01/13
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

        //ver1.26 JpegでSerializeするために変更
        private JpegSerializedImage _thumbImage = new JpegSerializedImage();

        public Bitmap thumbnail
        {
            get { return _thumbImage.bitmap as Bitmap; }
            set { _thumbImage.bitmap = value; }
        }

        //ver1.56 SmallBitmap版thumbnail
        //private SmallBitmap _thumb = new SmallBitmap();
        //public Bitmap thumbnail
        //{
        //    get { return _thumb.bitmap; }
        //    set { _thumb.Add(value); }
        //}

        //アニメーションタイマー DateTime.Now.Ticks
        [NonSerialized]
        public long animateStartTime = 0;

        //EXIF
        public int ExifISO;

        public string ExifDate;
        public string ExifMake;
        public string ExifModel;

        //しおり 2011年10月2日
        public bool isBookMark = false;

        //回転情報 2011年12月24日
        private int _rotate = 0;

        public int rotate
        {
            get { return _rotate; }
            set { _rotate = value % 360; }
        }

        //ver1.36 表示させるかどうか
        public bool isVisible;

        //サムネイルパネル内での位置
        //[NonSerialized()]
        //public Point ThumbnailPos;

        //ver1.51 画像情報を持っているか
        public bool hasInfo { get { return width != 0; } }

        //ver1.54 縦長かどうか
        public bool isTall { get { return height > width; } }

        //var1.54 横長かどうか
        public bool isFat { get { return width > height; } }

        public ImageInfo(int index, string name, DateTime date, long bytes)
        {
            //初期化
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
        // メソッド

        /// <summary>
        /// ver1.10 サムネイル画像をオリジナル画像から登録する。
        /// 上のLoadImage()を使わなくするために作成
        /// </summary>
        /// <param name="orgSizeBitmap"></param>
        public void resisterThumbnailImage(Bitmap orgSizeBitmap)
        {
            //登録済みなら何もしない
            if (thumbnail != null)
                return;

            //念のため。登録画像がなければクリア
            if (orgSizeBitmap == null)
            {
                thumbnail = null;
                //width = 0;
                //height = 0;
                return;
            }

            //ver1.26 高さ固定のサムネイルを作る
            this.thumbnail = BitmapUty.MakeHeightFixThumbnailImage(orgSizeBitmap, THUMBNAIL_HEIGHT);
            //this.width = orgSizeBitmap.Width;
            //this.height = orgSizeBitmap.Height;
            GetExifInfo(orgSizeBitmap);
        }

        private void GetExifInfo(Image orgImage)
        {
            //Exif情報の取得。
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

                    case 0x9003: //撮影日時
                                 //「YYYY:MM:DD HH:MM:SS」形式19文字
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
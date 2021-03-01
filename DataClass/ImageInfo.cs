using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
/*
サムネイル画像を保存するためのクラス
Exifから画像データを収集するなど高速にサムネイルを取得する
*/
namespace Marmi
{
    [Serializable()]
    public class ImageInfo //: IDisposable
    {
        //サムネイル画像のサイズ。最大値
        //private readonly int THUMBNAIL_WIDTH = App.DEFAULT_THUMBNAIL_SIZE;

        private readonly int THUMBNAIL_HEIGHT = App.DEFAULT_THUMBNAIL_SIZE;

        /// <summary>ファイル名</summary>
        public string Filename { get; }

        /// <summary>オリジナル画像サイズ</summary>
        public Size ImgSize { get; set; } = Size.Empty;

        public int Width => ImgSize.Width;
        public int Height => ImgSize.Height;

        /// <summary>ファイル作成日</summary>
        public DateTime CreateDate { get; }

        /// <summary>バイト長</summary>
        public long FileLength { get; }

        /// <summary>書庫内の順番</summary>
        public int OrgIndex { get; }

        /// <summary>画像キャッシュ。キャッシュをこちらに持つ 2013/01/13</summary>
        [NonSerialized]
        public RawImage CacheImage = new RawImage();

        /// <summary>サムネイル画像</summary>
        [field: NonSerialized]
        public Bitmap Thumbnail { get; set; } = null;

        ///// <summary>アニメーションタイマー DateTime.Now.Ticks</summary>
        //[field: NonSerialized]
        //public long AnimateStartTime { get; set; } = 0;

        /// <summary>EXIF: ISO値</summary>
        public int ExifISO { get; private set; }

        /// <summary>EXIF: 撮影日</summary>
        public string ExifDate { get; private set; }

        /// <summary>EXIF: メーカー</summary>
        public string ExifMake { get; private set; }

        /// <summary>EXIF: モデル</summary>
        public string ExifModel { get; private set; }

        /// <summary>しおり 2011年10月2日</summary>
        public bool IsBookMark { get; set; } = false;

        //回転情報 2011年12月24日
        //private int _rotate = 0;
        //public int Rotate
        //{
        //    get { return _rotate; }
        //    set { _rotate = value % 360; }
        //}

        //ver1.36 表示させるかどうか
        public bool IsVisible { get; set; } = true;

        //ver1.51 画像情報を持っているか
        public bool HasInfo => Width != 0;

        //ver1.54 縦長かどうか
        public bool IsTall => Height > Width;

        //var1.54 横長かどうか
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
        /// ver1.10 サムネイル画像をオリジナル画像から登録する。
        /// 上のLoadImage()を使わなくするために作成
        /// </summary>
        /// <param name="orgImage">元画像</param>
        public void ResisterThumbnailImage(Bitmap orgImage)
        {
            //登録済みなら何もしない
            if (Thumbnail != null)
                return;

            if (orgImage == null)
            {
                Thumbnail = null;
            }
            else
            {
                //ver1.26 高さ固定のサムネイルを作る
                this.Thumbnail = BitmapUty.MakeHeightFixThumbnailImage(orgImage, THUMBNAIL_HEIGHT);
                GetExifInfo(orgImage);
            }
        }

        /// <summary>
        /// Exif情報の取得。
        /// http://cachu.xrea.jp/perl/ExifTAG.html
        /// http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
        /// http://exif.org/specifications.html
        /// </summary>
        /// <param name="img">対象の画像</param>
        private void GetExifInfo(Image img)
        {
            foreach (PropertyItem pi in img.PropertyItems)
            {
                switch (pi.Id)
                {
                    case 0x8827: //ISO
                        ExifISO = BitConverter.ToUInt16(pi.Value, 0);
                        break;

                    case 0x9003: //撮影日時「YYYY:MM:DD HH:MM:SS」形式19文字
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
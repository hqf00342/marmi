/*
ImageInfo
画像情報とキャッシュを保存するためのクラス
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
        /// <summary>ファイル名</summary>
        public string Filename { get; }

        /// <summary>オリジナル画像サイズ</summary>
        public Size ImgSize { get; set; } = Size.Empty;

        public int Width => ImgSize.Width;
        public int Height => ImgSize.Height;

        /// <summary>ファイル作成日。情報表示とソートに使う</summary>
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

        public Exif Exif { get; set; } = new Exif();

        /// <summary>しおり 2011年10月2日</summary>
        public bool IsBookMark { get; set; } = false;

        //回転情報 2011年12月24日
        private int _rotate = 0;

        public int Rotate
        {
            get => _rotate;
            set => _rotate = value % 360;
        }

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
        /// Bitmapを実体化させるのでExifも同時に取得
        /// </summary>

        public void CreateThumbnail()
        {
            if (Thumbnail != null)
                return;
            var bmp = CacheImage.ToBitmap();
            this.Thumbnail = BitmapUty.MakeHeightFixThumbnailImage(bmp, App.DEFAULT_THUMBNAIL_SIZE);

            //せっかくBMPを作ったのでExifも登録する
            Exif.GetExifInfo(bmp);
        }
    }
}
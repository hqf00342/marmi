using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marmi.DataModel
{
    public class Exif
    {        /// <summary>EXIF: ISO値</summary>
        public int ISO { get; private set; }

        /// <summary>EXIF: 撮影日</summary>
        public string ShootingDate { get; private set; }

        /// <summary>EXIF: メーカー</summary>
        public string Maker { get; private set; }

        /// <summary>EXIF: モデル</summary>
        public string Model { get; private set; }

        /// <summary>
        /// Exif情報の取得。
        /// http://cachu.xrea.jp/perl/ExifTAG.html
        /// http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
        /// http://exif.org/specifications.html
        /// </summary>
        /// <param name="img">対象の画像</param>
        public void GetExifInfo(Image img)
        {
            if (img is null) return;

            foreach (PropertyItem pi in img.PropertyItems)
            {
                switch (pi.Id)
                {
                    case 0x8827: //ISO
                        ISO = BitConverter.ToUInt16(pi.Value, 0);
                        break;

                    case 0x9003: //撮影日時「YYYY:MM:DD HH:MM:SS」形式19文字
                        ShootingDate = Encoding.ASCII.GetString(pi.Value, 0, 19);
                        break;
                    case 0x010f: //Make
                        Maker = Encoding.ASCII.GetString(pi.Value);
                        Maker = Maker.Trim(new char[] { '\0' });
                        break;

                    case 0x0110: //Model
                        Model = Encoding.ASCII.GetString(pi.Value).Trim(new char[] { '\0' });
                        break;
                }
            }
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
/*
BitmapをJpegでシリアライズできるようにしたもの
*/
namespace Marmi
{
    [Serializable]
    internal class JpegSerializedImage : ISerializable
    {
        [NonSerialized]
        private Image _bmp;

        [NonSerialized]
        private readonly string SERIALISESTRING = "JPEG";

        public Image Bitmap
        {
            get { return _bmp; }
            set { _bmp = value; }
        }

        /// <summary>
        /// 標準のコンストラクタ
        /// </summary>
        public JpegSerializedImage()
        {
            _bmp = null;
        }

        /// <summary>
        /// ビットマップ設定可能なコンストラクタ
        /// </summary>
        /// <param name="bmp"></param>
        public JpegSerializedImage(Bitmap bmp)
        {
            _bmp = bmp;
        }

        /// <summary>
        /// シリアライズ用コンストラクタ
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected JpegSerializedImage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException(nameof(info));
            byte[] array = info.GetValue(SERIALISESTRING, typeof(byte[])) as byte[];
            //_bmp = ic.ConvertFrom(array) as Bitmap;

            //ver1.27 null対策。nullは0が1つだけ飛んでくる
            if (array.Length == 1 && array[0] == 0)
            {
                _bmp = null;
            }
            else
            {
                var ic = new ImageConverter();
                _bmp = ic.ConvertFrom(array) as Bitmap;
            }
        }

        /// <summary>
        /// ISerializeメンバー
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            //ver1.27 nullのときは出力させないとしてみる
            //if (_bmp == null)
            //    return;

            //ver1.27 null対策。nullは0が1つだけ飛んでくる
            var test = BitmapToByteArray(_bmp);
            if (test == null)
                test = new byte[] { 0 };
            info.AddValue(SERIALISESTRING, test, test.GetType());
        }

        /// <summary>
        /// Serialize用コンバーター
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private byte[] BitmapToByteArray(Image bmp)
        {
            if (_bmp == null)
                return null;

            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                ms.Close();
                return ms.ToArray();
            }
        }
    }
}
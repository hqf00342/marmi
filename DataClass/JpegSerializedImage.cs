using System;
using System.Drawing;
using System.Drawing.Imaging;	//ImageFormatter
using System.IO;
using System.Runtime.Serialization;	//ISerializable

namespace Marmi
{
    /// <summary>
    /// BitmapをJpegでシリアライズできるようにしたもの
    /// </summary>

    [Serializable]
    internal class JpegSerializedImage : ISerializable
    {
        [NonSerialized]
        private Image _bmp;

        [NonSerialized]
        private string SERIALISESTRING = "JPEG";

        //[NonSerialized]
        //ImageConverter ic = new ImageConverter();

        //public Bitmap bitmap
        public Image bitmap
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
                throw new System.ArgumentNullException("info");
            byte[] array = info.GetValue(SERIALISESTRING, typeof(byte[])) as byte[];
            //_bmp = ic.ConvertFrom(array) as Bitmap;

            //ver1.27 null対策。nullは0が1つだけ飛んでくる
            if (array.Length == 1 && array[0] == 0)
                _bmp = null;
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

        #region ISerializable メンバ

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            //ver1.27 nullのときは出力させないとしてみる
            //if (_bmp == null)
            //    return;

            byte[] test = BitmapToByteArray(_bmp);

            //ver1.27 null対策。nullは0が1つだけ飛んでくる
            if (test == null)
                test = new byte[1] { 0 };
            info.AddValue(SERIALISESTRING, test, test.GetType());
        }

        #endregion ISerializable メンバ

        /// <summary>
        /// Serialize用コンバーター
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private byte[] BitmapToByteArray(Image bmp)
        {
            if (_bmp == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                ms.Close();
                return ms.ToArray();
            }
        }
    }
}
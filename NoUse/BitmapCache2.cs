using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Marmi
{
    class BitmapCache2
    {
        private byte[] ByteImage;
        public DateTime TouchTime { get; set; }

        //---------------------------------------------------
        private void Touch()
        {
            TouchTime = DateTime.Now;
        }

        public void SetBitmap(string filename)
        {
            Touch();
            //ファイルが存在しなければ何もしない
            if (!File.Exists(filename))
                return;

            using (FileStream fs = File.OpenRead(filename))
            {
                FileInfo fi = new FileInfo(filename);
                MemoryStream ms = new MemoryStream((int)fi.Length);
                int len;
                byte[] buffer = new byte[4096];
                while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, len);

                ms.Close();
                ByteImage = ms.GetBuffer();
            }//using
        }

        public void SetBitmap(Stream st)
        {
            Touch();
            MemoryStream ms;
            if (st is MemoryStream)  //if(st.GetType() == typeof(MemoryStream))
            {
                ms = st as MemoryStream;
            }
            else
            {
                //Seekしないと末尾にあるのでコピーできない
                st.Seek(0, SeekOrigin.Begin);
                ms = new MemoryStream((int)st.Length);
                int len;
                byte[] buffer = new byte[4096];
                while ((len = st.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, len);
            }
            ms.Close();
            ByteImage = ms.GetBuffer();
        }

        public void SetBitmap(Bitmap bmp)
        {
            Touch();
            MemoryStream ms = new MemoryStream();

            //システム設定のJpegで保存
            bmp.Save(ms, ImageFormat.Jpeg);
            ms.Close();
            ByteImage = ms.GetBuffer();
        }

        public Bitmap GetBitmap()
        {
            Touch();
            //ちゃんとMemoryStreamをクローズした方がいいと思っていたが
            //using上のコードで閉じてしまうとBitmapが最後まで参照できない模様
            //そのためGetExifInfo()などでObjectDisposedException が発生している
            MemoryStream ms = new MemoryStream(ByteImage);
            try
            {
                return new Bitmap(ms);
            }
            catch (ArgumentException)
            {
                //Bitmapじゃなかった
                return null;
            }
            //ここでMemoryStreamをClose()してはだめ
        }
    }
}
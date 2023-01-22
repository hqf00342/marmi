using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

namespace Marmi
{
	public class SmallBitmap
	{
		#region --- const for GetImageSize() ---
		const byte JPG_SOF0 = 0xc0;
		const byte JPG_SOF15 = 0xcf;
		const int BMP_HEADER2 = 0x424d;
		const int JPG_HEADER2 = 0xffd8;
		const int PNG_HEADER2 = 0x8950;
		const int GIF_HEADER2 = 0x4749;
		const int TIF_HEADERI = 0x4949;
		const int TIF_HEADERM = 0x4d4d;
		#endregion

		#region --- private ---
		private byte[] internalBuffer = null;
		#endregion


		#region --- プロパティ ---
		public Bitmap bitmap
		{
			get { return ImageConv(); }
			set
			{
				if (value == null)
					Clear();
				else
					Add(value);
			}
		}
		public int Length
		{
			get {
				if (internalBuffer == null)
					return 0;
				else
					return internalBuffer.Length;
			}
		}

		public bool hasImage { get { return internalBuffer != null; } }
		#endregion





		////////////////////////////////////////////////////////////////////////////////
		// publicメソッド


		/// <summary>
		/// 指定したファイル名をメモリバッファに登録する
		/// </summary>
		/// <param name="filename">登録したい実ファイル名</param>
		public void Add(string filename)
		{
			//キーとファイル名を同じにして登録
			//ファイルが存在しなければ何もしない
			if (!File.Exists(filename))
				return;

			using (FileStream fs = File.OpenRead(filename))
			{
				FileInfo fi = new FileInfo(filename);
				MemoryStream ms = new MemoryStream((int)fi.Length);
				int len;
				byte[] buf = new byte[4096];
				while ((len = fs.Read(buf, 0, buf.Length)) > 0)
					ms.Write(buf, 0, len);

				ms.Close();
				internalBuffer = ms.GetBuffer();
				//buffer = ms.ToArray();
			}//using
		}


		/// <summary>
		/// ストリームをそのまま登録する
		/// </summary>
		/// <param name="st">登録ストリーム</param>
		public void Add(Stream st)
		{

			MemoryStream ms;
			if (st is MemoryStream)  //if(st.GetType() == typeof(MemoryStream))
			{
				//このままClose()してbyte[]にするのでSeek()不要
				//st.Seek(0, SeekOrigin.Begin);
				ms = st as MemoryStream;
			}
			else
			{
				//Seekしないと末尾にあるのでコピーできない
				st.Seek(0, SeekOrigin.Begin);

				ms = new MemoryStream((int)st.Length);
				int len;
				byte[] buf = new byte[4096];
				while ((len = st.Read(buf, 0, buf.Length)) > 0)
					ms.Write(buf, 0, len);
			}

			ms.Close();
			internalBuffer = ms.GetBuffer();
		}

		/// <summary>
		/// BitmapをBufferに登録する。
		/// </summary>
		/// <param name="bmp">登録するBitmap</param>
		public void Add(Bitmap bitmap)
		{
			//null設定された時はクリアする
			if (bitmap == null)
			{
				Clear();
				return;
			}
			//システム設定のpngで保存
			MemoryStream ms = new MemoryStream();
			//bitmap.Save(ms, ImageFormat.Png);
			bitmap.Save(ms, ImageFormat.Jpeg);
			ms.Close();

			//メモリを最小化するためにGetBuffer()ではなくToArray()を使う
			//msdn:このメソッドは、MemoryStream が閉じられているときに機能します。
			internalBuffer = ms.ToArray();
		}

		public void Clear()
		{
			if (internalBuffer == null)
				return;
			if (internalBuffer.Length > 0)
				internalBuffer = null;
		}

		#region private Method
		private Bitmap ImageConv()
		{
			if (internalBuffer == null || internalBuffer.Length == 0)
				return null;

			try
			{
				ImageConverter ic = new ImageConverter();
				return ic.ConvertFrom(internalBuffer) as Bitmap;

				//using (MemoryStream ms = new MemoryStream(internalBuffer))
				//{
				//    return Image.FromStream(ms, true, false) as Bitmap;
				//}
			}
			catch (ArgumentException)
			{
				return null;
			}

		}
		#endregion

		/// <summary>
		/// ver1.51 画像サイズをBMPを作らずに取得
		/// 画像サイズを取得する
		/// byte[]配列を直接見ることで対応
		/// ver1.51 png, jpg, bmpに対応
		/// </summary>
		/// <returns>画像のサイズ、取得できない場合はSize.Empty</returns>
		public Size GetImageSize()
		{
			int width = 0;
			int height = 0;
			byte[] bs = internalBuffer;
			if (bs == null)
				return Size.Empty;

			int header = bs[0] * 256 + bs[1];	//big endian
			switch (header)
			{
				case PNG_HEADER2:
					width = (bs[16] << 24) + (bs[17] << 16) + (bs[18] << 8) + bs[19];
					height = (bs[20] << 24) + (bs[21] << 16) + (bs[22] << 8) + bs[23];
					break;
				case BMP_HEADER2:
					width = BitConverter.ToInt32(bs, 0x12);
					height = BitConverter.ToInt32(bs, 0x16);
					break;
				case JPG_HEADER2:
					int p = 2;
					while (p < bs.Length)
					{
						if (bs[p] != 0xff)
							break;
						byte mark = bs[p + 1];
						if (mark >= JPG_SOF0 && mark <= JPG_SOF15)
						{
							height = bs[p + 5] * 256 + bs[p + 6];
							width = bs[p + 7] * 256 + bs[p + 8];
							break;
						}
						int len = bs[p + 2] * 256 + bs[p + 3];
						p = p + len + 2;	//2バイトはマーカー分
					}
					break;
				case GIF_HEADER2:
					//リトルエンディアン2バイト
					width = BitConverter.ToInt16(bs, 0x06);
					height = BitConverter.ToInt32(bs, 0x08);
					break;
				case TIF_HEADERI:
				case TIF_HEADERM:
				default:
					//TIFFフォーマットは面倒なのでBITMAP化
					using (Bitmap bmp = ImageConv())
					{
						width = bmp.Width;
						height = bmp.Height;
					}
					break;
					

			}
			return new Size(width, height);
		}
	}
}

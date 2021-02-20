using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace Marmi
{
	class BitmapCache
	{
		//cache
		private Dictionary<string, byte[]> items = new Dictionary<string, byte[]>();
		private long _usageBytes = 0;

		////////////////////////////////////////////////////////////////////////////////
		// プロパティ

		public int Count
		{
			get { return items.Count; }
		}

		public Bitmap this[string keyname]
		{
			//get { return GetBitmap(keyname);	}
			//get { return TryGetBitmap(keyname); }
			get { return ImageConv(keyname); }
		}

		public long UsageBytes
		{
			get
			{
				//int bytes = 0;
				//foreach (var item in items)
				//    bytes += item.Value.Length;
				//return bytes;
				return _usageBytes;
			}
		}

		public string[] Keys
		{
			get
			{
				List<string> s = new List<string>();
				foreach (var key in items.Keys)
					s.Add(key);
				return s.ToArray();
			}
		}


		////////////////////////////////////////////////////////////////////////////////
		// publicメソッド


		/// <summary>
		/// 指定したファイル名をメモリバッファに登録する
		/// </summary>
		/// <param name="filename">登録したい実ファイル名</param>
		public void Add(string filename)
		{
			//キーとファイル名を同じにして登録
			Add(filename, filename);
		}

		/// <summary>
		/// 指定したファイルを指定したキーで登録
		/// キーをファイル名とは別に登録したいときの関数
		/// </summary>
		/// <param name="filename">読み込む実ファイル名</param>
		/// <param name="keyname">登録キー</param>
		public void Add(string filename, string keyname)
		{
			//Debug.WriteLine(filename, "BitmapCache.Add(string)");

			//ファイルが存在しなければ何もしない
			if (!File.Exists(filename))
				return;

			lock ((items as ICollection).SyncRoot)
			{
				//登録済みなら何もしない
				if (items.ContainsKey(keyname))
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
					
					items.Add(keyname, ms.GetBuffer());
					_usageBytes += items[keyname].Length;
					//Debug.WriteLine(keyname, "BitmapCache RESISTERED");
				}//using
			}//lock
		}

		/// <summary>
		/// ストリームをそのまま登録する
		/// </summary>
		/// <param name="keyname">登録するファイル名（＝キー-）</param>
		/// <param name="st">登録ストリーム</param>
		public void Add(string keyname, Stream st)
		{
			//Debug.WriteLine(filename, "BitmapCache.Add(stream)");

			//lock (syncRoot)
			lock ((items as ICollection).SyncRoot)
			{
				//登録済みなら何もしない
				if (items.ContainsKey(keyname))
					return;

				MemoryStream ms;
				if (st is MemoryStream)  //if(st.GetType() == typeof(MemoryStream))
				{
					//このままClose()してbyte[]にするのでSeek()不要
					//st.Seek(0, SeekOrigin.Begin);
					ms = st as MemoryStream;
					//Debug.WriteLine("st as MemoryStream");
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
				items.Add(keyname, ms.GetBuffer());
				_usageBytes += items[keyname].Length;
			}//lock
		}

		/// <summary>
		/// BitmapをBufferに登録する。
		/// 登録する際にはJpeg化し、70%の品質で保存する
		/// </summary>
		/// <param name="keyname">登録するファイル名（＝キー）</param>
		/// <param name="bmp">登録するBitmap</param>
		public void Add(string keyname, Bitmap bmp)
		{
			Debug.WriteLine(keyname, "BitmapCache.Add(Bitmap)");

			//lock (syncRoot)
			lock ((items as ICollection).SyncRoot)
			{
				//登録済みなら何もしない.lock()内部でreturnしてもOKそう
				if (items.ContainsKey(keyname))
					return;

				const Int64 quality = 70L;
				MemoryStream ms = new MemoryStream();

				//システム設定のJpegで保存
				//bmp.Save(ms, ImageFormat.Jpeg);

				//品質指定で保存
				ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
				System.Drawing.Imaging.Encoder myEncoder =
						System.Drawing.Imaging.Encoder.Quality;
				EncoderParameters myEncoderParameters = new EncoderParameters(1);
				EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
				myEncoderParameters.Param[0] = myEncoderParameter;
				bmp.Save(ms, jgpEncoder, myEncoderParameters);

				//MemoryStreamをバイト配列にして保存
				//byte[] a = ms.GetBuffer();
				ms.Close();

				//メモリを最小化するためにGetBuffer()ではなくToArray()を使う
				items.Add(keyname, ms.ToArray());
				_usageBytes += items[keyname].Length;

			}//lock
		}

		public void Remove(string keyname)
		{
			if (!ContainsKey(keyname))
				return;
			lock ((items as ICollection).SyncRoot)
			{
				_usageBytes -= items[keyname].Length;
				items.Remove(keyname);
			}
		}

		/// <summary>
		/// ファイル名をキーにBitmapを返す
		/// </summary>
		/// <param name="keyname">登録キー</param>
		/// <returns>画像、キーがない場合はnull</returns>
		public Bitmap GetBitmap(string keyname)
		{
			//if (!buf.ContainsKey(filename))
			if (!ContainsKey(keyname))
				return null;

			//Bitmap bmp;
			//using (MemoryStream ms = new MemoryStream(buf[filename]))
			//{
			//    bmp = new Bitmap(ms);
			//}
			//Debug.WriteLine("BitmapCache.GetBitmap()");
			//return bmp;

			//ちゃんとMemoryStreamをクローズした方がいいと思っていたが
			//上のコードで閉じてしまうとBitmapが最後まで参照できない模様
			//そのためGetExifInfo()などでObjectDisposedException が発生している
			lock ((items as ICollection).SyncRoot)
			{
				//TODO MemoryStreamが不正でArgumentExceptionが発生することを想定すべき
				//Mac関連のPngはだめそう
				MemoryStream ms = new MemoryStream(items[keyname]);
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
		/// <summary>
		/// キャッシュをすべてクリア
		/// </summary>
		public void Clear()
		{
			//気にせずすべて解放
			//配列は参照がなくなった時点でCLR回収
			//lock (syncRoot)
			lock ((items as ICollection).SyncRoot)
			{
				if (items.Count > 0)
					items.Clear();
			}
			_usageBytes = 0;
		}
		/// <summary>
		/// キャッシュが指定した量を超える場合は
		/// 古いものをリリースする。
		/// どれをリリースするかは考え中
		/// </summary>
		/// <param name="limit">キャッシュの最大量。byte単位</param>
		public void ReleaseCache(int limit)
		{
			//TODO いつか実装
		}

		public bool ContainsKey(string str)
		{
			//return buf.ContainsKey(str);
			bool b;
			lock ((items as ICollection).SyncRoot)
			{
				b = items.ContainsKey(str);
			}
			return b;
		}

		/// <summary>
		/// GetBitmapの高速版として仮実装
		/// </summary>
		/// <param name="keyname">Key</param>
		/// <returns>あればBitmap、なければnullを返す</returns>
		private Bitmap TryGetBitmap(string keyname)
		{
			byte[] o;
			lock ((items as ICollection).SyncRoot)
			{
				if (items.TryGetValue(keyname, out o))
				{
					MemoryStream ms = new MemoryStream(o);
					try
					{
						return new Bitmap(ms);
					}
					catch (ArgumentException)
					{
						//Bitmapじゃなかった
						return null;
					}
				}
				else
				{
					return null;
				}
			}
		}

		private Bitmap ImageConv(string keyname)
		{
			byte[] o;
			ImageConverter ic = new ImageConverter();
			lock ((items as ICollection).SyncRoot)
			{
				if (items.TryGetValue(keyname, out o))
				{
					try
					{
						return ic.ConvertFrom(o) as Bitmap;
					}
					catch (ArgumentException)
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}

		}

		/// <summary>
		/// Jpegの画像品質を設定するために必要なルーチン
		/// http://msdn.microsoft.com/ja-jp/library/bb882583.aspx
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		private ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}


	}
}

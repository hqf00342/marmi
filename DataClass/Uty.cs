using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;				//Debug, Stopwatch
using System.Text.RegularExpressions;	//正規表現
using System.Threading;					//スレッドを返すため
using System.Windows.Forms;				//MessageBox;
using Microsoft.VisualBasic.FileIO;		//ゴミ箱

namespace Marmi
{
	class Uty
	{
		/// <summary>
		/// 対応書庫かどうかをチェックする
		/// </summary>
		/// <param name="archiveName">チェック対象の書庫ファイル名</param>
		/// <returns>対応書庫ならtrue</returns>
		public static bool isAvailableArchiveFile(string archiveName)
		{
			if (string.Compare(Path.GetExtension(archiveName), ".zip", true) == 0)
				return true;
			if (string.Compare(Path.GetExtension(archiveName), ".7z", true) == 0)
				return true;
			if (string.Compare(Path.GetExtension(archiveName), ".rar", true) == 0)
				return true;
			if (string.Compare(Path.GetExtension(archiveName), ".tar", true) == 0)
				return true;
			if (string.Compare(Path.GetExtension(archiveName), ".lzh", true) == 0)
				return true;
			//ver1.31
			//if (string.Compare(Path.GetExtension(archiveName), ".bz2", true) == 0)
			//    return true;
			if (string.Compare(Path.GetExtension(archiveName), ".gz", true) == 0)
				return true;
			if (string.Compare(Path.GetExtension(archiveName), ".tgz", true) == 0)
				return true;

			return false;
		}

		/// <summary>
		/// 対応しているファイル形式かどうかをチェック
		/// 書庫、画像、pdfとしてチェック
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static bool isAvailableFile(string filename)
		{
			if (isAvailableArchiveFile(filename))
				return true;
			else if(isPictureFilename(filename))
				return true;
			return filename.ToLower().EndsWith(".pdf");
		}

		/// <summary>
		/// 画像ファイルかどうか確認する。拡張子を見て確認
		/// </summary>
		/// <param name="sz">ファイル名</param>
		/// <returns>画像ファイルであればtrue</returns>
		public static bool isPictureFilename(string sz)
		{
			//System.Text.RegularExpressions.Regex
			if (Regex.Match(sz, @"\.(jpeg|jpg|jpe|png|gif|bmp|ico|tif|tiff)$", RegexOptions.IgnoreCase).Success)
				return true;
			else
				return false;
		}

		/// <summary>
		/// ガベージコレクションをする。
		/// 全ジェネレーション実施
		/// </summary>
		public static void ForceGC()
		{
			long before = GC.GetTotalMemory(false);
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			System.GC.Collect();
			Uty.WriteLine("ForceGC {0:N0} -> {1:N0}", before, GC.GetTotalMemory(false));
		}

		/// <summary>
		/// ファイルのMD5を計算する。
		/// </summary>
		/// <param name="filename">対象のファイル</param>
		/// <returns>16進数文字列</returns>
		public static string calcMd5(string filename)
		{
			//ファイルを開く
			System.IO.FileStream fs = File.OpenRead(filename);

			//ハッシュ値を計算する
			var md5 = System.Security.Cryptography.MD5.Create();
			byte[] bs = md5.ComputeHash(fs);

			//ファイルを閉じる
			fs.Close();

			return BitConverter.ToString(bs).ToLower().Replace("-","");
		}

		//public static string TryMakedir(string dirname)
		//{
		//    string trydir = dirname;
		//    int trynum = 0;

		//    while (!Directory.Exists(trydir))
		//        trydir = string.Format("{0}{1}", dirname, trynum++);

		//    Directory.CreateDirectory(trydir);
		//    return trydir;
		//}

		/// <summary>
		/// 再帰的に書庫を展開
		/// 単独で利用するとブロックするのでスレッド内で利用されることを想定
		/// </summary>
		/// <param name="archivename">書庫名</param>
		/// <param name="extractDir">展開先</param>
		public static void RecurseExtractAll(string archivename, string extractDir)
		{
			using(SevenZipWrapper sz = new SevenZipWrapper(archivename))
			{
				sz.ExtractAll(extractDir);

				string[] exfiles = Directory.GetFiles(extractDir);
				foreach(string file in exfiles)
				{
					if (isAvailableArchiveFile(file))
					{
						string extDirName = getUniqueDirname(file);
						Debug.WriteLine(file, extDirName);
						RecurseExtractAll(file, extDirName);
					}
				}

			}
		}

		/// <summary>
		/// ファイル名（アーカイブ名）をベースにユニークな
		/// 展開フォルダ名を探す。書庫と同じ場所で探す
		/// </summary>
		/// <param name="archiveName">書庫名</param>
		/// <returns></returns>
		public static string getUniqueDirname(string archiveName)
		{
			string ext = Path.GetExtension(archiveName);
			int trynum = 0;

			string tryBaseName = Path.Combine(
				Path.GetDirectoryName(archiveName),
				Path.GetFileNameWithoutExtension(archiveName));

			//そのままの名前で使えるか
			if (!Directory.Exists(tryBaseName))
				return tryBaseName;

			//ダメな場合はユニークな名前を探し回る
			while (true)
			{
				string tryName = string.Format("{0}{1}", tryBaseName, trynum);
				if (Directory.Exists(tryName))
					trynum++;
				else
					return tryName;
			}
		}


		public static Thread AsyncRecurseExtractAll(string archivename, string extractDir)
		{
			ThreadStart tsAction = () =>
			{
				RecurseExtractAll(archivename,extractDir);
			};
			Thread th = new Thread(tsAction);
			th.Name = "RecurseExtractAll";
			th.IsBackground = true;
			th.Start();

			//注意書きを入れておく
			MakeAttentionTextfile(extractDir);

			//Threadを返す
			return th;

		}

		public static void MakeAttentionTextfile(string extractFolder)
		{
			//フォルダに注意喚起のテキストを入れておく
			try
			{
				string attentionFilename = Path.Combine(
					extractFolder,
					"このフォルダは消しても安全です.txt");
				string[] texts = {
					"このファイルはMarmi.exeによって作成された一時フォルダです",
					"Marmi.exeを起動していない場合、安全に削除できます"};

				File.WriteAllLines(
					attentionFilename,
					texts,
					System.Text.Encoding.UTF8);
			}
			catch
			{
				//別に作成できなくてもいいので例外はすべて放置
				//throw;
			}
		}

		public static void DeleteTempDir(string tempDirName)
		{
			if (Directory.Exists(tempDirName))
			{
				try
				{
					//再帰的に消す
					Directory.Delete(tempDirName, true);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, @"一時フォルダの削除が出来ませんでした");
					//throw e;
				}
			}
		}

		/// <summary>
		/// ver1.35 ゴミ箱へ送る
		/// </summary>
		/// <param name="filename"></param>
		public static void RecycleBin(string filename)
		{
			FileSystem.DeleteFile(
			   filename,
			   UIOption.OnlyErrorDialogs,
			   RecycleOption.SendToRecycleBin);

		}

		[Conditional("DEBUG")]
		public static void WriteLine(string format, params object[] args)
		{
			Debug.WriteLine(string.Format(format, args), DateTime.Now.ToString());
			//for(int i=0; i<args.Length; ++i)
			//{
			//   format = format.Replace(
			//        "{" + i.ToString() + "}",
			//       args[i].ToString());
			//}
			//Debug.WriteLine(format);
		}

		[Conditional("DEBUG")]
		public static void WriteLine(string s)
		{
			Debug.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString(), s));
		}

		public static string GetUsedMemory()
		{
			long used = GC.GetTotalMemory(false);
			if (used < 1000)
				return string.Format("Used:{0}bytes", used);
			else if (used < 1000000)
				return string.Format("Used:{0}Kbytes", used / 1000);
			else if (used < 1000000000)
				return string.Format("Used:{0}Mbytes", used / 1000000);
			else
				return string.Format("Used:{0}Gbytes", used / 1000000000);
		}


		/// <summary>
		/// unsafe string.ToArray()を利用しない
		/// 動的確保をやめる
		/// 事前のディレクトリ確認をやめる
		/// 文字列の長さを事前に確認
		/// unsafe版最速
		/// </summary>
		/// <param name="s1">比較文字列１</param>
		/// <param name="s2">比較文字列２</param>
		/// <returns></returns>
		public static int Compare_unsafeFast(string s1, string s2)
		{
			//数値を一度変換し長さによらない比較をする。	
			//XP以降のソート相当に対応したはず・・・


			//
			// 1文字ずつチェックを開始する
			//
			int p1 = 0;		// s1を指すポインタ加算値
			int p2 = 0;		// s2を指すポインタ加算値
			long num1 = 0;	// s1に含まれる数値。大きな数値に対応させるためlong
			long num2 = 0;	// s2に含まれる数値。大きな数値に対応させるためlong
			char c1;	//比較文字１ c1 = s1[p1];
			char c2;	//比較文字２ c2 = s2[p2];
			int s1Len = s1.Length;
			int s2Len = s2.Length;

			unsafe
			{
				fixed (char* cp1 = s1)
				fixed (char* cp2 = s2)
				{
					do
					{
						c1 = *(cp1 + p1);
						c2 = *(cp2 + p2);

						//c1とc2の比較を開始する前に数字だったら数値モードへ
						//数値モードの場合は数値に変換して比較
						if (c1 >= '0' && c1 <= '9' && c2 >= '0' && c2 <= '9')
						{
							//s1系列の文字を数値num1に変換
							num1 = 0;
							while (c1 >= '0' && c1 <= '9')
							{
								num1 = num1 * 10 + c1 - '0';
								++p1;
								if (p1 >= s1Len)
									break;
								c1 = s1[p1];
							}

							//s2系列の文字を数値num2に変換
							num2 = 0;
							while (c2 >= '0' && c2 <= '9')
							{
								num2 = num2 * 10 + c2 - '0';
								++p2;
								if (p2 >= s2Len)
									break;
								c2 = s2[p2];
							}

							//数値として比較
							if (num1 != num2)
								return (int)(num1 - num2);
						}
						else
						{
							//単一文字として比較
							if (c1 != c2)
							{
								if (c1 == '\\' || c1 == '/')
									return 1;
								if (c2 == '\\' || c2 == '/')
									return -1;
								return (int)(c1 - c2);
							}
							++p1;
							++p2;
						}
					}
					while (p1 < s1Len && p2 < s2Len);
				}//fixed
			}
			//どちらかが終端に達した。あとは長い方が後ろ。
			return s1Len - s2Len;
		}
	}
}

using System;
using System.Collections.Generic;

namespace Marmi
{
	//class CompareClass
	//{
	//}

	/********************************************************************************/
	// ソート用比較クラス
	// 自然言語ソートするための比較クラス
	/********************************************************************************/

	public class NaturalOrderComparer2 : IComparer<ImageInfo>
	{
		public int Compare(ImageInfo x, ImageInfo y)
		{
			//return NaturalOrderCompareOriginal(x.filename, y.filename);
			return Compare_unsafeFast(x.filename, y.filename);
		}

		[Obsolete]
		public int NaturalOrderCompareOriginal(string s1, string s2)
		{
			//数値を一度変換し長さによらない比較をする。	
			//XP以降のソート相当に対応したはず・・・

			//階層をチェック
			int lev1 = 0;	//xの階層
			int lev2 = 0;	//yの階層
			for (int i = 0; i < s1.Length; i++)
				if (s1[i] == '/' || s1[i] == '\\') lev1++;
			for (int i = 0; i < s2.Length; i++)
				if (s2[i] == '/' || s2[i] == '\\') lev2++;

			if (lev1 != lev2)
				return lev1 - lev2;

			//
			// 同一階層なので1文字ずつチェックを開始する
			//
			int p1 = 0;		// s1を指すポインタ
			int p2 = 0;		// s2を指すポインタ
			long num1 = 0;	// s1に含まれる数値。大きな数値に対応させるためlong
			long num2 = 0;	// s2に含まれる数値。大きな数値に対応させるためlong

			do
			{
				char c1 = s1[p1];
				char c2 = s2[p2];

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
						if (p1 >= s1.Length)
							break;
						c1 = s1[p1];
					}

					//s2系列の文字を数値num2に変換
					num2 = 0;
					while (c2 >= '0' && c2 <= '9')
					{
						num2 = num2 * 10 + c2 - '0';
						++p2;
						if (p2 >= s2.Length)
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
						return (int)(c1 - c2);
					++p1;
					++p2;
				}
			}
			while (p1 < s1.Length && p2 < s2.Length);

			//どちらかが終端に達した。あとは長い方が後ろ。
			return s1.Length - s2.Length;
		}

		/// <summary>
		/// NaturalSort3改善。ループ内での動的変数確保をやめる
		/// 文字列の長さを事前に確認
		/// おそらくManaged最速
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		public int Compare_ManagedFast(string s1, string s2)
		{
			//数値を一度変換し長さによらない比較をする。	
			//XP以降のソート相当に対応したはず・・・


			//
			// 同一階層なので1文字ずつチェックを開始する
			//
			int p1 = 0;		// s1を指すポインタ
			int p2 = 0;		// s2を指すポインタ
			long num1 = 0;	// s1に含まれる数値。大きな数値に対応させるためlong
			long num2 = 0;	// s2に含まれる数値。大きな数値に対応させるためlong
			char c1;	//比較文字１ c1 = s1[p1];
			char c2;	//比較文字２ c2 = s2[p2];
			int s1Len = s1.Length;
			int s2Len = s2.Length;

			do
			{
				c1 = s1[p1];
				c2 = s2[p2];

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
						//if (p1 >= s1.Length)
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
						//if (p2 >= s2.Length)
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
			//while (p1 < s1.Length && p2 < s2.Length) ;

			//どちらかが終端に達した。あとは長い方が後ろ。
			return s1Len - s2Len;
			//return s1.Length - s2.Length;
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
		public int Compare_unsafeFast(string s1, string s2)
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


		/// <summary>
		/// ほんのちょっとだけ最適化
		/// インクリメントを中に
		/// whileループをdo～whileに
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		public int Compare_unsafeFast2(string s1, string s2)
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
							do
							{
								num1 = num1 * 10 + c1 - '0';
								if (++p1 >= s1Len)
									break;
								c1 = s1[p1];
							}
							while (c1 >= '0' && c1 <= '9');

							//s2系列の文字を数値num2に変換
							num2 = 0;
							do
							{
								num2 = num2 * 10 + c2 - '0';
								if (++p2 >= s2Len)
									break;
								c2 = s2[p2];
							}
							while (c2 >= '0' && c2 <= '9');

							//数値として比較
							if (num1 != num2)
								return (int)(num1 - num2);
						}

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
					while (p1 < s1Len && p2 < s2Len);
				}//fixed
			}

			//どちらかが終端に達した。あとは長い方が後ろ。
			return s1Len - s2Len;
		}
	
	}



	public class DateCompare : IComparer<ImageInfo>
	{

		public int Compare(ImageInfo x, ImageInfo y)
		{
			return DateTime.Compare(x.createDate, y.createDate);
		}
	}



	public class CustomSortCompare : IComparer<ImageInfo>
	{
		public int Compare(ImageInfo x, ImageInfo y)
		{
			return x.nOrgIndex - y.nOrgIndex;
		}
	}


}

using System;
using System.IO;

namespace Marmi
{

	public class ArchiveItem
	{
		public string filename;
		public DateTime datetime;
		public ulong filesize;
		public bool isDirectory;

		/// <summary>
		/// デフォルトコンストラクタは使わない
		/// </summary>
		//public ArchiveItem()
		//{
		//}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name">アイテムのファイル名</param>
		/// <param name="date">作成日時</param>
		/// <param name="size">ファイルサイズ</param>
		/// <param name="isDir">ディレクトリかどうか</param>
		public ArchiveItem(string name, DateTime date, ulong size, bool isDir)
		{
			filename = name;
			datetime = date;
			filesize = size;
			isDirectory = isDir;
		}
	}
}

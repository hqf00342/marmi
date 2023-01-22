using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Marmi
{
	public class Win32
	{

		#region kernel32.dll

		[DllImport("kernel32", SetLastError = true)]
		public extern static IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32", SetLastError = true)]
		public extern static bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32", SetLastError = true)]
		public extern static IntPtr GetProcAddress(
			IntPtr hModule, string lpProcName);

		[DllImport("kernel32")]
		public extern static IntPtr LocalLock(IntPtr hMem);

		[DllImport("kernel32")]
		public extern static bool LocalUnlock(IntPtr hMem);

		[DllImport("kernel32")]
		public extern static IntPtr LocalFree(IntPtr hMem);

		#endregion


		#region gdi32.dll
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BITMAPFILEHEADER
		{
			public ushort bfType;
			public uint bfSize;
			public ushort bfReserved1;
			public ushort bfReserved2;
			public uint bfOffBits;
		}
		public const ushort BM = 0x4d42;	// 'BM' ... set to BITMAPFILEHEADER.bfType

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BITMAPINFO
		{
			public BITMAPINFOHEADER bmiHeader;
			public RGBQUAD biColors;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct RGBQUAD
		{
			public byte rgbBlue;
			public byte rgbGreen;
			public byte rgbRed;
			public byte rgbReserved;
		}

		#endregion
	}
}

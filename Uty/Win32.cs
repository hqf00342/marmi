using System;
using System.Runtime.InteropServices;

namespace Marmi
{
    public static class Win32
    {
        #region USER32

        [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        //[DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        //public static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int SW_NORMAL = 1;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public extern static bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public extern static bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
        //[DllImport("user32.dll")]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        // ShowWindowAsync関数のパラメータに渡す定義値
        public const int SW_RESTORE = 9;  // 画面を元の大きさに戻す

        public const int WM_USER = 0x400;
        public const int MY_FORCE_FOREGROUND_MESSAGE = WM_USER + 1;

        #endregion USER32

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

        #endregion kernel32.dll

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

        public const ushort BM = 0x4d42;    // 'BM' ... set to BITMAPFILEHEADER.bfType

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

        #endregion gdi32.dll
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Marmi
{
    public enum SusieConfigType
    {
        About = 0,
        Config = 1,
    }

    public class SusiePlugin : IDisposable
    {
        private Win32.BITMAPFILEHEADER bf;
        private IntPtr hMod;
        private string name;
        public string Name { get { return name; } }

        // 00IN,00AM 必須
        // int _export PASCAL GetPluginInfo(int infono, LPSTR dw, int len);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int GetPluginInfoHandler(int infono, StringBuilder buf, int buflen);

        private GetPluginInfoHandler getPluginInfo;

        // for GetPluginInfo(Type)
        private const int GETINFO_TYPE = 0;

        private string type;
        public string Type { get { return type; } }
        public const string TYPE_SINGLE = "00IN";
        public const string TYPE_MULTI = "00AM";

        // for GetPluginInfo(Version)
        private const int GETINFO_VERSION = 1;

        private string version;
        public string Version { get { return version; } }

        // for GetPluginInfo(Filter)
        private const int GETINFO_FILTER = 2;

        private string filter;
        public string Filter { get { return filter; } }
        public List<string> Extentions = new List<string>();

        // 00IN,00AM 必須
        // int _export PASCAL IsSupported(LPSTR file, DWORD dw);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int IsSupportedHandler(string filename, [In] byte[] dw);

        private IsSupportedHandler isSupported;

        // 00IN,00AM 任意
        // int _export PASCAL ConfigurationDlg(HWND parent, int fnc)
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int ConfigurationDlgHandler(IntPtr parent, SusieConfigType fnc);

        private ConfigurationDlgHandler configurationDlg;

        public EventHandler GetConfigHandler(IntPtr parent, SusieConfigType fnc)
        {
            if (configurationDlg == null) return null;
            return delegate { configurationDlg(parent, fnc); };
        }

        // 00IN 必須
        // int _export PASCAL GetPicture(LPSTR strb, long len, unsigned int flag, HANDLE *pHBInfo, HANDLE *pHBm, FARPROC lpPrgressCallback, long lData);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int GetPictureHandler(
            [In] byte[] buf,
            int len,
            InputFlag flag,
            out IntPtr pHBInfo,
            out IntPtr pHBm,
            int lpProgressCallback,
            int lData);

        private GetPictureHandler getPicture;

        private enum InputFlag : int
        {
            File = 0,
            Memory = 1,
            InFileOutMem = 0x0100,
            InFileOutFile = 0x0101,
        }

        // 00AM 必須
        // int _export PASCAL GetArchiveInfo(LPSTR strb, long len, unsigned int flag, HLOCAL *lphInf)
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int GetArchiveInfoHandler(
            [In] byte[] buf,
            [In] int len,
            [In] InputFlag flag,
            out IntPtr lphInf);

        private GetArchiveInfoHandler getArchiveInfo;

        // 00AM 必須
        // int _export PASCAL GetFileInfo(LPSTR strb, long len, LPSTR file, unsigned int flag, fileInfo *lpInfo)
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int GetFileInfoHandler(
            [In] byte[] buf,
            int len,
            [In] byte[] file,
            InputFlag flag,
            out IntPtr lpInfo);

        private GetFileInfoHandler getFileInfo;

        // 00AM 必須
        // int _export PASCAL GetFile(LPSTR src, long len, LPSTR dest, unsigned int flag, FARPROC prgressCallback, long lData)
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int GetFileHandler(
            [In] byte[] src,
            int len,
            out IntPtr dest,
            InputFlag flag,
            int lpProgressCallback,
            int lData);

        private GetFileHandler getFile;

        public static SusiePlugin Load(string filename)
        {
            SusiePlugin spi = new SusiePlugin();
            spi.name = Path.GetFileName(filename);
            spi.hMod = Win32.LoadLibrary(filename);
            if (spi.hMod == IntPtr.Zero)
                return null;

            IntPtr addr;

            // 00IN,00AM 必須 GetPluginInfo()
            addr = Win32.GetProcAddress(spi.hMod, "GetPluginInfo");
            if (addr == IntPtr.Zero) return null;
            spi.getPluginInfo = (GetPluginInfoHandler)Marshal.
                GetDelegateForFunctionPointer(addr, typeof(GetPluginInfoHandler));

            StringBuilder strb = new StringBuilder(256);
            spi.getPluginInfo(GETINFO_TYPE, strb, strb.Capacity);
            spi.type = strb.ToString();
            strb.Length = 0;
            spi.getPluginInfo(GETINFO_VERSION, strb, strb.Capacity);
            spi.version = strb.ToString();
            StringBuilder filter = new StringBuilder();
            StringBuilder ext = new StringBuilder(256);
            for (int i = GETINFO_FILTER; ; i += 2)
            {
                ext.Length = 0;
                if (spi.getPluginInfo(i, ext, ext.Capacity) == 0) break;
                strb.Length = 0;
                if (spi.getPluginInfo(i + 1, strb, strb.Capacity) == 0) break;
                filter.Append(strb).Append('|').Append(ext).Append('|');
            }
            spi.filter = filter.ToString(0, filter.Length - 1);

            // 00IN,00AM 必須 IsSupported()
            addr = Win32.GetProcAddress(spi.hMod, "IsSupported");
            if (addr == IntPtr.Zero) return null;
            spi.isSupported = (IsSupportedHandler)Marshal.
                GetDelegateForFunctionPointer(addr, typeof(IsSupportedHandler));

            // 00IN,00AM 任意 ConfigurationDlg()
            addr = Win32.GetProcAddress(spi.hMod, "ConfigurationDlg");
            if (addr != IntPtr.Zero)
            {
                spi.configurationDlg = (ConfigurationDlgHandler)Marshal.
                    GetDelegateForFunctionPointer(addr, typeof(ConfigurationDlgHandler));
            }

            if (spi.type == TYPE_SINGLE)
            {
                // 00IN 必須 GetPicture()
                addr = Win32.GetProcAddress(spi.hMod, "GetPicture");
                if (addr == IntPtr.Zero) return null;
                spi.getPicture = (GetPictureHandler)Marshal.
                    GetDelegateForFunctionPointer(addr, typeof(GetPictureHandler));
            }
            else if (spi.type == TYPE_MULTI)
            {
                // 00AM 必須 GetArchiveInfo()
                addr = Win32.GetProcAddress(spi.hMod, "GetArchiveInfo");
                if (addr != IntPtr.Zero)
                {
                    spi.getArchiveInfo = (GetArchiveInfoHandler)Marshal.
                    GetDelegateForFunctionPointer(addr, typeof(GetArchiveInfoHandler));
                }
                // 00AM 必須 GetFileInfo()
                addr = Win32.GetProcAddress(spi.hMod, "GetFileInfo");
                if (addr != IntPtr.Zero)
                {
                    spi.getFileInfo = (GetFileInfoHandler)Marshal.
                    GetDelegateForFunctionPointer(addr, typeof(GetFileInfoHandler));
                }
                // 00AM 必須 GetFile()
                addr = Win32.GetProcAddress(spi.hMod, "GetFile");
                if (addr != IntPtr.Zero)
                {
                    spi.getFile = (GetFileHandler)Marshal.
                    GetDelegateForFunctionPointer(addr, typeof(GetFileHandler));
                }
            }
            else
            {
                return null;
            }

            return spi;
        }

        public Bitmap GetPicture(string file, byte[] buf)
        {
            if (type != TYPE_SINGLE) return null;
            if (isSupported(file, buf) == 0) return null;
            IntPtr hBInfo, hBm;
            if (getPicture(buf, buf.Length, InputFlag.Memory, out hBInfo, out hBm, 0, 0) != 0) return null;
            try
            {
                IntPtr pBInfo = Win32.LocalLock(hBInfo);
                IntPtr pBm = Win32.LocalLock(hBm);
                makeBitmapFileHeader(pBInfo);
                byte[] mem = new byte[bf.bfSize];
                GCHandle handle = GCHandle.Alloc(bf, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(handle.AddrOfPinnedObject(), mem, 0, Marshal.SizeOf(bf));
                }
                finally
                {
                    handle.Free();
                }
                Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), (int)bf.bfOffBits - Marshal.SizeOf(bf));
                Marshal.Copy(pBm, mem, (int)bf.bfOffBits, (int)(bf.bfSize - bf.bfOffBits));
                using (MemoryStream ms = new MemoryStream(mem))
                {
                    return new Bitmap(ms);
                }
            }
            finally
            {
                Win32.LocalUnlock(hBInfo);
                Win32.LocalFree(hBInfo);
                Win32.LocalUnlock(hBm);
                Win32.LocalFree(hBm);
            }
        }

        public List<SusieFileInfo> GetArchiveInfo(string file)
        {
            IntPtr pBuf;
            byte[] buf = Encoding.GetEncoding("Shift_JIS").GetBytes(file + "\0");

            if (getArchiveInfo(buf, buf.Length, InputFlag.File, out pBuf) == 0)
            {
                if (pBuf == IntPtr.Zero)
                    return null;

                List<SusieFileInfo> retval = new List<SusieFileInfo>();

                //成功
                IntPtr pBInfo = Win32.LocalLock(pBuf);
                unsafe
                {
                    byte* ptr = (byte*)pBInfo;
                    while (*ptr != 0)
                    {
                        fileinfo fi = (fileinfo)Marshal.PtrToStructure((IntPtr)ptr, typeof(fileinfo));
                        SusieFileInfo sfi = ComvertToSusieFileInfo(fi);
                        retval.Add(sfi);
                        ptr += sizeof(fileinfo);
                    }
                }
                Win32.LocalUnlock(pBuf);
                Win32.LocalFree(pBuf);  //add by t.naga 2013/05/18
                return retval;
            }
            else
                return null;
        }

        private unsafe SusieFileInfo ComvertToSusieFileInfo(fileinfo fi)
        {
            var ret = new SusieFileInfo()
            {
                method = Marshal.PtrToStringAnsi((IntPtr)fi.method),
                filename = Marshal.PtrToStringAnsi((IntPtr)fi.filename),
                position = fi.position,
                compsize = fi.compsize,
                filesize = fi.filesize,
                crc = fi.crc,
                timestamp = new DateTime(1970, 1, 1).AddSeconds(fi.timestamp).ToLocalTime(),
            };
            return ret;
        }

        public byte[] GetFile(string file, SusieFileInfo sfi)
        {
            byte[] srcfilename = Encoding.GetEncoding("Shift_JIS").GetBytes(file + "\0");
            int len = (int)sfi.position;    // offset
            IntPtr dest;

            int ret = getFile(srcfilename, len, out dest, InputFlag.InFileOutMem, 0, 0);
            if (ret == 0)
            {
                if (dest == IntPtr.Zero)
                    return null;
                try
                {
                    Win32.LocalLock(dest);
                    byte[] byteArray = new byte[sfi.filesize];
                    Marshal.Copy(dest, byteArray, 0, (int)sfi.filesize);
                    return byteArray;
                }
                finally
                {
                    Win32.LocalUnlock(dest);
                    Win32.LocalFree(dest);
                }
            }
            else
                return null;
        }

        public byte[] GetFile(string file, int pos, int filesize)
        {
            byte[] srcfilename = Encoding.GetEncoding("Shift_JIS").GetBytes(file + "\0");
            int len = pos;  // offset
            IntPtr dest;

            int ret = getFile(srcfilename, len, out dest, InputFlag.InFileOutMem, 0, 0);
            if (ret == 0)
            {
                if (dest == IntPtr.Zero)
                    return null;
                try
                {
                    Win32.LocalLock(dest);
                    byte[] byteArray = new byte[filesize];
                    Marshal.Copy(dest, byteArray, 0, filesize);
                    return byteArray;
                }
                finally
                {
                    Win32.LocalUnlock(dest);
                    Win32.LocalFree(dest);
                }
            }
            else
                return null;
        }

        //[DllImport("msvcrt.dll", CharSet = CharSet.Auto)]
        //public static extern long time(IntPtr tm);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private unsafe struct fileinfo
        {
            public fixed byte method[8];
            public UInt32 position;
            public UInt32 compsize;
            public UInt32 filesize;
            public UInt32 timestamp;
            public fixed byte path[200];
            public fixed byte filename[200];
            public UInt32 crc;
        }

        private void makeBitmapFileHeader(IntPtr pBInfo)
        {
            Win32.BITMAPINFOHEADER bi = (Win32.BITMAPINFOHEADER)
                Marshal.PtrToStructure(pBInfo, typeof(Win32.BITMAPINFOHEADER));
            bf.bfSize = (uint)((((bi.biWidth * bi.biBitCount + 0x1f) >> 3) & ~3) * bi.biHeight);
            bf.bfOffBits = (uint)(Marshal.SizeOf(bf) + Marshal.SizeOf(bi));
            if (bi.biBitCount <= 8)
            {
                uint palettes = bi.biClrUsed;
                if (palettes == 0)
                    palettes = 1u << bi.biBitCount;
                bf.bfOffBits += palettes << 2;
            }
            bf.bfSize += bf.bfOffBits;
            bf.bfType = Win32.BM;
            bf.bfReserved1 = 0;
            bf.bfReserved2 = 0;
        }

        ~SusiePlugin()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (hMod != IntPtr.Zero)
            {
                Win32.FreeLibrary(hMod);
                hMod = IntPtr.Zero;
            }
        }
    }
}
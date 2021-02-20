using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;   //SafeHandle
namespace Marmi
{
    class LateBoundUnmanagedDll : SafeHandle
    {
        // アンマネージDLLの遅延バインディング
        // 以下のURLを参照。
        // http://blog.recyclebin.jp/archives/1219
        //

        public override bool IsInvalid
        {
            get{ return IsClosed || this.handle == IntPtr.Zero; }
        }

        public LateBoundUnmanagedDll(string dllName) : base(IntPtr.Zero, true)
        {
            IntPtr handle = LoadLibrary(dllName);
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return FreeLibrary(this.handle);
        }

        public TDelegate GetFunction<TDelegate>(string entryPoint)
           where TDelegate : class
        {
            if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
            {
                throw new ArgumentException();
            }

            IntPtr function = GetProcAddress(this.handle, entryPoint);
            return Marshal.GetDelegateForFunctionPointer(function, typeof(TDelegate)) as TDelegate;
        }

        #region DllImport
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory([MarshalAs(UnmanagedType.LPStr)] string lpPathName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        #endregion
    }
}

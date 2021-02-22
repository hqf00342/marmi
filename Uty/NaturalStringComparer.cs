/*
Natural Compare用の関数

*/
using System.Collections.Generic;

namespace Marmi
{
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return Win32.StrCmpLogicalW(x, y);
        }

        /// <summary>static版. Array.Sort()で利用</summary>
        public static int CompareS(string a, string b)
        {
            return Win32.StrCmpLogicalW(a, b);
        }
    }
}

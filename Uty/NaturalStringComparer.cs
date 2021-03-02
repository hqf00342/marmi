using System.Collections.Generic;
/*
Natural Compare用の関数
*/
namespace Marmi
{
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y) => Win32.StrCmpLogicalW(x, y);

        /// <summary>static版. Array.Sort()で利用</summary>
        public static int CompareS(string a, string b) => Win32.StrCmpLogicalW(a, b);
    }
}

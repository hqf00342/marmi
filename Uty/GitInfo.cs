using System.IO;
using System.Linq;
using System.Reflection;

namespace Mii
{
    internal class GitInfo
    {
        public static string CommitId => GetResource("git_id.txt");
        public static string BranchName => GetResource("git_branch.txt");

        private static string GetResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(name));
            if (resName == null)
                return string.Empty;
            using (var st = asm.GetManifestResourceStream(resName))
            {
                if (st == null) return string.Empty;
                var reader = new StreamReader(st);
                return reader.ReadToEnd().Trim('\r', '\n');
            }
        }
    }
}

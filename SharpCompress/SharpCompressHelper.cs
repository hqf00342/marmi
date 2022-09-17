using System.Text;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Marmi
{
    internal class SharpCompressHelper
    {
        public static ReaderOptions SjisReader(string filename)
        {
            var sjis = Encoding.GetEncoding(932);
            return new ReaderOptions
            {
                ArchiveEncoding = new ArchiveEncoding
                {
                    CustomDecoder = (data, _, __) => sjis.GetString(data)
                }
            };
        }
    }
}

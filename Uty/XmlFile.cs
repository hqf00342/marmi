using System.IO;
using System.Xml.Serialization;

namespace Marmi
{
    internal class XmlFile
    {
        /// <summary>
        /// XML形式で保存したObjectをロードする。
        /// </summary>
        /// <returns></returns>
        public static T LoadFromXmlFile<T>(string xmlFilename) where T : class
        {
            if (!File.Exists(xmlFilename))
                return null;

            using (var fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read))
            {
                var xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(fs);
            }
        }

        /// <summary>
        /// XML形式でObjectを保存する
        /// </summary>
        /// <param name="obj"></param>
        public static void SaveToXmlFile<T>(T obj, string xmlFilename)
        {
            using (var fs = new FileStream(xmlFilename, FileMode.Create, FileAccess.Write))
            {
                var xs = new XmlSerializer(typeof(T));
                xs.Serialize(fs, obj);
            }
        }
    }
}

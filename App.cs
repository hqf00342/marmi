using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marmi
{
    public static class App
    {
        public static AppGlobalConfig Config;

        //サムネイル標準サイズ
        public static readonly int DEFAULT_THUMBNAIL_SIZE = 400;

        //現在見ているパッケージ情報
        //public static PackageInfo g_pi = null;

        //コンフィグファイル名。XmlSerializeで利用
        //private const string CONFIGNAME = "Marmi.xml";

        //アプリ名。タイトルとして利用
        internal const string APPNAME = "Marmi";

        //サムネイルキャッシュの拡張子
        internal const string CACHEEXT = ".tmp";

        //サイドバーの標準サイズ
        internal const int SIDEBAR_DEFAULT_WIDTH = 200;

        //非同期IOタイムアウト値
        internal const int ASYNC_TIMEOUT = 5000;

    }
}

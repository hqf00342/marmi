using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public static class App
    {
        //コンフィグ。Program.csで読み込みまたは生成している。
        public static AppGlobalConfig Config;

        //サムネイル標準サイズ
        public static readonly int DEFAULT_THUMBNAIL_SIZE = 400;

        //現在見ているパッケージ情報
        public static PackageInfo g_pi = null;

        //コンフィグファイル名。XmlSerializeで利用
        //private const string CONFIGNAME = "Marmi.xml";

        //アプリ名。タイトルとして利用
        internal const string APPNAME = "Marmi";

        //サイドバーの標準サイズ
        internal const int SIDEBAR_DEFAULT_WIDTH = 200;

        //非同期IOタイムアウト値
        internal const int ASYNC_TIMEOUT = 5000;

        //Susieプラグイン
        internal static Susie susie = new Susie();

        //unrar.dllプラグイン ver1.76
        internal static Unrar unrar = new Unrar();

        //ver1.35 スクリーンキャッシュ
        internal static Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();

        //Bitmap.Tagにつけるタグ:リソース文字列から移動
        //TODO:利用していない疑惑あり
        internal const string TAG_PICTURECACHE = "CACHE";

        public static class Cursors
        {
            private static readonly Icon iconLoope = Properties.Resources.loopeIcon;
            private static readonly Icon iconLeftFinger = Properties.Resources.finger_left_shadow_ico;
            private static readonly Icon iconRightFinger = Properties.Resources.finger_right_shadow_ico;
            private static readonly Icon iconHandOpen = Properties.Resources.iconHandOpen;

            internal static Cursor Left = new Cursor(iconLeftFinger.Handle);
            internal static Cursor Right = new Cursor(iconRightFinger.Handle);
            internal static Cursor Loupe = new Cursor(iconLoope.Handle);
            internal static Cursor OpenHand = new Cursor(iconHandOpen.Handle);
        }
    }
}

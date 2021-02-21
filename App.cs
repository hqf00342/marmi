using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi
{
    public static class App
    {
        public static AppGlobalConfig Config;

        //サムネイル標準サイズ
        public static readonly int DEFAULT_THUMBNAIL_SIZE = 400;

        //現在見ているパッケージ情報
        public static PackageInfo g_pi = null;

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

        //非同期IO用スレッド
        internal static Thread AsyncIOThread = null;

        //非同期取得用スタック
        internal static PrioritySafeQueue<KeyValuePair<int, Delegate>> stack = new PrioritySafeQueue<KeyValuePair<int, Delegate>>();

        //Susieプラグイン
        internal static Susie susie = new Susie();

        //unrar.dllプラグイン ver1.76
        internal static Unrar unrar = new Unrar();

        //ver1.35 スクリーンキャッシュ
        internal static Dictionary<int, Bitmap> ScreenCache = new Dictionary<int, Bitmap>();

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

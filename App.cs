using Marmi.Models;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public static class App
    {
        //コンフィグ。Program.csで読み込みまたは生成している。
        public static AppGlobalConfig Config;

        //サムネイル標準サイズ
        public static readonly int DEFAULT_THUMBNAIL_SIZE = 400;

        //サイドバーの基本の幅
        internal const int SIDEBAR_INIT_WIDTH = 200;

        //コンフィグファイル名。XmlSerializeで利用
        internal const string CONFIGNAME = "Marmi.xml";

        internal static string ConfigFilename => Path.Combine(Application.StartupPath, CONFIGNAME);

        //現在見ているパッケージ情報
        public static PackageInfo g_pi = new PackageInfo();

        //アプリ名。タイトルとして利用
        internal const string APPNAME = "Marmi";

        //サイドバーの標準サイズ
        internal const int SIDEBAR_DEFAULT_WIDTH = 200;

        //非同期IOタイムアウト値
        internal const int ASYNC_TIMEOUT = 5000;

        //Susieプラグイン
        internal static Susie susie = new Susie();

        //Bitmap.Tagにつけるタグ:リソース文字列から移動
        //TODO:利用していない疑惑あり
        internal const string TAG_PICTURECACHE = "CACHE";

        //多目的Bitmapキャッシュ
        internal readonly static BitmapCache BmpCache = new BitmapCache();

        //staticフォント
        internal static readonly Font Font9 = new Font("ＭＳ Ｐ ゴシック", 9F);
        internal static readonly Font Font10 = new Font("ＭＳ Ｐ ゴシック", 10.5F);
        internal static readonly Font Font12B = new Font("ＭＳ Ｐ ゴシック", 12F, FontStyle.Bold);

        internal static readonly int Font9_Height;
        internal static readonly int Font10_Height;
        internal static readonly int Font12B_Height;

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

        static App()
        {
            Font9_Height = (int)GetFontHeight(Font9);
            Font10_Height = (int)GetFontHeight(Font10);
            Font12B_Height = (int)GetFontHeight(Font12B);
        }

        private static float GetFontHeight(Font font)
        {
            using (var bmp = new Bitmap(100, 100))
            using (var g = Graphics.FromImage(bmp))
            {
                SizeF sf = g.MeasureString("テスト文字列", font);
                return sf.Height;
            }
        }

        /// <summary>
        /// その気になったら使おう
        /// </summary>
        /// <param name="msg"></param>
        public static void SetStatusbarInfo(string msg)
        {
            if (Form1._instance.InvokeRequired)
            {
                Form1._instance.BeginInvoke((Action)(() => { SetStatusbarInfo(msg); })); ;
            }
            else
            {
                Form1._instance.SetStatusbarInfo(msg);
            }
        }
    }
}
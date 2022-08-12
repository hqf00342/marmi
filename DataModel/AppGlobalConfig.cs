using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Marmi
{
    /********************************************************************************/
    // 設定を保存するクラス。
    /********************************************************************************/

    [Serializable]
    public class AppGlobalConfig //: INotifyPropertyChanged
    {
        //コンフィグファイル名。XmlSerializeで利用
        private const string CONFIGNAME = "Marmi.xml";

        //サイドバーの基本の幅
        private const int SIDEBAR_INIT_WIDTH = 200;

        public static string ConfigFilename => Path.Combine(Application.StartupPath, CONFIGNAME);

        public bool DualView { get; set; }          //2画面並べて表示
        public Size windowSize;                     //ウィンドウサイズ
        public Point windowLocation;                //ウィンドウ表示位置

        public List<MRU> Mru { get; set; } = new List<MRU>();     //MRUリスト用配列

        public bool VisibleMenubar { get; set; }                 //メニューバーの表示
        public bool VisibleToolBar { get; set; }                 //ツールバーの表示
        public bool VisibleStatusBar { get; set; }               //ステータスバーの表示

        public bool IsRecurseSearchDir { get; set; }             //ディレクトリの再帰検索

        public bool IsFitScreenAndImage { get; set; }            //画像とイメージをフィットさせる
        public bool IsStopPaintingAtResize { get; set; }         //リサイズ時の描写をやめる
        public bool VisibleSidebar { get; set; }                 //サイドバーの表示
        public bool IsAutoCleanOldCache { get; set; }            //古いキャッシュの自動削除
        public int SidebarWidth { get; set; }                    //サイドバーの幅

        ////ver1.35 スクリーンショー時間[ms]
        public int SlideShowTime { get; set; }

        //ver1.62 ツールバーの位置
        public bool IsToolbarTop { get; set; }

        //ver1.77 画面モード保存対象にする。
        public bool isFullScreen;

        //サムネイルモード
        [XmlIgnore]
        public bool isThumbnailView;

        //ver1.78 倍率の保持
        public bool KeepMagnification { get; set; }

        public GeneralConfig General { get; set; } = new GeneralConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public KeyConfig Keys { get; set; } = new KeyConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public AdvanceConfig Advance { get; set; } = new AdvanceConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        /*******************************************************************************/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppGlobalConfig()
        {
            Initialize();
        }

        /// <summary>
        /// 各パラメータの初期値
        /// </summary>
        private void Initialize()
        {
            VisibleMenubar = true;
            VisibleToolBar = true;
            VisibleStatusBar = true;
            VisibleSidebar = false;

            DualView = false;
            isFullScreen = false;
            isThumbnailView = false;
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            //isSaveThumbnailCache = false;
            IsRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            IsFitScreenAndImage = true;

            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;

            //サイドバー
            SidebarWidth = SIDEBAR_INIT_WIDTH;

            //ループするかどうか
            //isLoopToTopPage = false;

            //スクリーンショー時間
            SlideShowTime = 3000;

            //ツールバーの位置
            IsToolbarTop = true;

            //ver1.70 2枚表示はデフォルトで簡易チェック
            //dualview_exactCheck = false;

            //ver1.78 倍率の保持
            KeepMagnification = false;
            //ver1.79 書庫は必ず展開

            //ver1.91 コンフィグ分離 2022年8月7日
            General.Init();
            View.Init();
            Thumbnail.Init();
            Keys.Init();
            Mouse.Init();
            Loupe.Init();
            Advance.Init();
        }

        /// <summary>
        /// XML形式で保存したObjectをロードする。
        /// </summary>
        /// <returns></returns>
        public static object LoadFromXmlFile()
        {
            string path = AppGlobalConfig.ConfigFilename;

            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    //読み込んで逆シリアル化する
                    var xs = new XmlSerializer(typeof(AppGlobalConfig));
                    return xs.Deserialize(fs);
                }
            }
            return null;
        }

        /// <summary>
        /// XML形式でObjectを保存する
        /// </summary>
        /// <param name="obj"></param>
        public static void SaveToXmlFile(object obj)
        {
            string path = AppGlobalConfig.ConfigFilename;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var xs = new XmlSerializer(typeof(AppGlobalConfig));
                xs.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// 現在閲覧しているg_pi.PackageNameをMRUに追加する
        /// 以前も見たことがある場合、閲覧日付だけを更新
        /// </summary>
        public void UpdateMRUList(PackageInfo pi)
        {
            //var pi = App.g_pi;

            //なにも無ければ追加しない
            if (string.IsNullOrEmpty(pi.PackageName))
                return;

            //MRUに追加する必要があるか確認
            bool needMruAdd = true;
            for (int i = 0; i < Mru.Count; i++)
            {
                if (Mru[i] == null)
                    continue;
                if (Mru[i].Name == pi.PackageName)
                {
                    //登録済みのMRUを更新
                    //日付だけ更新
                    Mru[i].Date = DateTime.Now;
                    //最後に見たページも更新 v1.37
                    Mru[i].LastViewPage = pi.NowViewPage;
                    needMruAdd = false;

                    //ver1.77 Bookmarkも設定
                    Mru[i].Bookmarks = pi.CreateBookmarkString();
                }
            }
            if (needMruAdd)
            {
                Mru.Add(new MRU(pi.PackageName, DateTime.Now, pi.NowViewPage, pi.CreateBookmarkString()));
            }
        }
    }
}
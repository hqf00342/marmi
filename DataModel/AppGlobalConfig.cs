using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.IO;
using System.Windows.Forms;		//Application
using System.Xml.Serialization;			// XmlSerializer

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

        public bool DualView { get; set; }                       //2画面並べて表示
        public Size windowSize;                     //ウィンドウサイズ
        public Point windowLocation;                //ウィンドウ表示位置

        public List<MRU> Mru { get; set; } = new List<MRU>();     //MRUリスト用配列

        public bool VisibleMenubar { get; set; }                 //メニューバーの表示
        public bool VisibleToolBar { get; set; }                 //ツールバーの表示
        public bool VisibleStatusBar { get; set; }               //ステータスバーの表示

        public bool IsRecurseSearchDir { get; set; }             //ディレクトリの再帰検索

        public bool IsFitScreenAndImage { get; set; }            //画像とイメージをフィットさせる
        public bool IsStopPaintingAtResize { get; set; }         //リサイズ時の描写をやめる
        public bool VisibleNavibar { get; set; }                 //ナビバーの表示
        public bool IsAutoCleanOldCache { get; set; }            //古いキャッシュの自動削除

        public bool IsFastDrawAtResize { get; set; }             // 高速描写をするかどうか

        //サイドバー関連
        //public bool isFixSidebar;					//サイドバーを固定にするかどうか
        public int SidebarWidth { get; set; }                    //サイドバーの幅

        //ver1.25
        public bool NoEnlargeOver100p { get; set; }          //画面フィッティングは100%未満にする

        public bool IsDotByDotZoom { get; set; }             //Dot-by-Dot補間モードにする

        //ver1.21画像切り替え方法
        public AnimateMode PictureSwitchMode { get; set; }

        ////ver1.35 スクリーンショー時間[ms]
        public int SlideShowTime { get; set; }

        //ver1.62 ツールバーの位置
        public bool IsToolbarTop { get; set; }

        //ver1.70 2枚表示の厳格化
        //public bool dualview_exactCheck;

        //ver1.71 最終ページの動作
        public bool LastPage_stay { get; set; }

        public bool LastPage_toTop { get; set; }
        //public bool LastPage_toNextArchive { get; set; }

        //ver1.77 画面モード保存対象にする。
        public bool isFullScreen;

        //サムネイルモード
        [XmlIgnore]
        public bool isThumbnailView;

        //ver1.78 倍率の保持
        public bool KeepMagnification { get; set; }

        //ver1.79 2ページモード
        public bool DualView_Force { get; set; }

        public bool DualView_Normal { get; set; }
        public bool DualView_withSizeCheck { get; set; }

        //ver1.80 ダブルクリック
        public bool DoubleClickToFullscreen { get; set; }

        //ver1.83 アンシャープマスク
        public bool UseUnsharpMask { get; set; }

        public int UnsharpDepth { get; set; }

        public GeneralConfig General { get; set; } = new GeneralConfig();
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
            VisibleNavibar = false;

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

            //高度な設定
            IsFastDrawAtResize = true;                      //リサイズ時に高速描写をするかどうか
                                                            //書庫
                                                            //クロスフェード
                                                            //isCrossfadeTransition = false;

            // 画面切り替えモード
            PictureSwitchMode = AnimateMode.Slide;
            //zoom
            NoEnlargeOver100p = true;       //画面フィッティングは100%未満にする
            IsDotByDotZoom = false;         //Dot-by-Dot補間モードにする

            //ループするかどうか
            //isLoopToTopPage = false;

            //スクリーンショー時間
            SlideShowTime = 3000;

            //ツールバーの位置
            IsToolbarTop = true;

            //ver1.70 2枚表示はデフォルトで簡易チェック
            //dualview_exactCheck = false;

            //ver1.71 最終ページの動作
            LastPage_stay = true;
            LastPage_toTop = false;
            //LastPage_toNextArchive = false;

            //ver1.78 倍率の保持
            KeepMagnification = false;
            //ver1.79 書庫は必ず展開
            //ver1.79 2ページモードアルゴリズム
            DualView_Force = false;
            DualView_Normal = true;
            DualView_withSizeCheck = false;

            //ダブルクリック機能を開放する
            DoubleClickToFullscreen = false;

            //ver1.83 アンシャープマスク
            UseUnsharpMask = true;
            UnsharpDepth = 25;

            //ver1.91 コンフィグ分離 2022年8月7日
            General.Init();
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
    }
}
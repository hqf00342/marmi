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
        public bool IsSaveConfig { get; set; }                   //コンフィグの保存

        public bool IsRecurseSearchDir { get; set; }             //ディレクトリの再帰検索

        public bool IsReplaceArrowButton { get; set; }               //ツールバーの左右ボタンを入れ替える

        public bool IsContinueZipView { get; set; }              //zipファイルは前回の続きから
        public bool IsFitScreenAndImage { get; set; }            //画像とイメージをフィットさせる
        public bool IsStopPaintingAtResize { get; set; }         //リサイズ時の描写をやめる
        public bool VisibleNavibar { get; set; }                 //ナビバーの表示
        public bool IsAutoCleanOldCache { get; set; }            //古いキャッシュの自動削除

        public bool IsFastDrawAtResize { get; set; }             // 高速描写をするかどうか

        //サイドバー関連
        //public bool isFixSidebar;					//サイドバーを固定にするかどうか
        public int SidebarWidth { get; set; }                    //サイドバーの幅

        //ver1.09 書庫関連
        public bool IsExtractIfSolidArchive { get; set; }        //ソリッド書庫なら一時展開するか

        //ver1.25
        public bool NoEnlargeOver100p { get; set; }          //画面フィッティングは100%未満にする

        public bool IsDotByDotZoom { get; set; }             //Dot-by-Dot補間モードにする

        //ver1.21画像切り替え方法
        public AnimateMode PictureSwitchMode { get; set; }

        ////ver1.35 スクリーンショー時間[ms]
        public int SlideShowTime { get; set; }


        //ver1.49 ウィンドウの初期位置
        public bool IsWindowPosCenter { get; set; }

        //ver1.62 ツールバーの位置
        public bool IsToolbarTop { get; set; }

        //ver1.65 ツールバーアイテムの文字を消すか
        public bool EraseToolbarItemString { get; set; }

        //ver1.70 サイドバーのスムーススクロール
        public bool Sidebar_smoothScroll { get; set; }

        //ver1.70 2枚表示の厳格化
        //public bool dualview_exactCheck;

        //ver1.71 最終ページの動作
        public bool LastPage_stay { get; set; }

        public bool LastPage_toTop { get; set; }
        //public bool LastPage_toNextArchive { get; set; }

        //ver1.73 一時展開フォルダ
        public string TmpFolder { get; set; }

        //ver1.73 MRU保持数
        public int NumberOfMru;

        [XmlIgnore]
        public Color BackColor;

        [XmlElement("XmlMainBackColor")]
        public string XmlMainColor
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }



        //ver1.77 画面モード保存対象にする。
        public bool isFullScreen;

        //サムネイルモード
        [XmlIgnore]
        public bool isThumbnailView;

        //ver1.76 多重起動禁止フラグ
        public bool DisableMultipleStarts { get; set; }

        //ver1.77 画面表示位置調整を簡易にするか
        public bool SimpleCalcForWindowLocation { get; set; }

        //ver1.77 フルスクリーン状態を復元できるようにする
        public bool SaveFullScreenMode { get; set; }

        //ver1.78 倍率の保持
        public bool KeepMagnification { get; set; }

        //ver1.79 書庫を常に展開するかどうか
        public bool AlwaysExtractArchive { get; set; }

        //ver1.79 2ページモード
        public bool DualView_Force { get; set; }

        public bool DualView_Normal { get; set; }
        public bool DualView_withSizeCheck { get; set; }

        //ver1.80 ダブルクリック
        public bool DoubleClickToFullscreen { get; set; }


        //ver1.83 アンシャープマスク
        public bool UseUnsharpMask { get; set; }

        public int UnsharpDepth { get; set; }

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
            IsSaveConfig = false;
            //isSaveThumbnailCache = false;
            IsRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            BackColor = Color.LightSlateGray;
            IsReplaceArrowButton = false;
            IsFitScreenAndImage = true;

            IsContinueZipView = false;
            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;


            //サイドバー
            SidebarWidth = SIDEBAR_INIT_WIDTH;

            //高度な設定
            IsFastDrawAtResize = true;                      //リサイズ時に高速描写をするかどうか
                                                            //書庫
            IsExtractIfSolidArchive = true;
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

            //画面の初期位置
            IsWindowPosCenter = false;

            //ツールバーの位置
            IsToolbarTop = true;

            //ver1.64ツールバーアイテムの文字を消す
            EraseToolbarItemString = false;

            //ver1.70 サイドバーのスムーススクロールはOn
            Sidebar_smoothScroll = true;

            //ver1.70 2枚表示はデフォルトで簡易チェック
            //dualview_exactCheck = false;

            //ver1.71 最終ページの動作
            LastPage_stay = true;
            LastPage_toTop = false;
            //LastPage_toNextArchive = false;

            //ver1.73 一時展開フォルダ
            TmpFolder = string.Empty;
            NumberOfMru = 10;

            //ver1.76 多重起動
            DisableMultipleStarts = false;
            //ver1.77 ウィンドウ位置を簡易計算にするか
            SimpleCalcForWindowLocation = false;
            //ver1.77 フルスクリーン状態を復元できるようにする
            SaveFullScreenMode = true;
            //ver1.78 倍率の保持
            KeepMagnification = false;
            //ver1.79 書庫は必ず展開
            AlwaysExtractArchive = false;
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
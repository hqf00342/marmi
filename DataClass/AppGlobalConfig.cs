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

        //public bool isSaveThumbnailCache;			//サムネイルキャッシュの保存
        public bool IsRecurseSearchDir { get; set; }             //ディレクトリの再帰検索

        public bool IsReplaceArrowButton { get; set; }               //ツールバーの左右ボタンを入れ替える

        public bool IsContinueZipView { get; set; }              //zipファイルは前回の続きから
        public bool IsFitScreenAndImage { get; set; }            //画像とイメージをフィットさせる
        public bool IsStopPaintingAtResize { get; set; }         //リサイズ時の描写をやめる
        public int ThumbnailSize;                   //サムネイル画像の大きさ
        public bool VisibleNavibar { get; set; }                 //ナビバーの表示
        public bool IsAutoCleanOldCache { get; set; }            //古いキャッシュの自動削除

        public bool IsDrawThumbnailShadow { get; set; }          // サムネイルに影を描写するか
        public bool IsDrawThumbnailFrame { get; set; }           // サムネイルに枠を描写するか
        public bool IsShowTPFileName { get; set; }               // サムネイルにファイル名を表示するか
        public bool IsShowTPFileSize { get; set; }               // ファイル名にファイルサイズを表示するか
        public bool IsShowTPPicSize { get; set; }                // ファイル名に画像サイズを表示するか

        // ver1.35 メモリモデル
        //public MemoryModel memModel;                //メモリーモデル

        public int CacheSize { get; set; }                       //memModel == userDefinedのときのキャッシュサイズ[MB]

        //ルーペ関連
        public int loupeMagnifcant;                //ルーペ倍率

        public bool IsOriginalSizeLoupe { get; set; }            // ルーペを原寸表示とするかどうか。

        public bool IsFastDrawAtResize { get; set; }             // 高速描写をするかどうか

        //サイドバー関連
        //public bool isFixSidebar;					//サイドバーを固定にするかどうか
        public int SidebarWidth { get; set; }                    //サイドバーの幅

        //ver1.09 書庫関連
        public bool IsExtractIfSolidArchive { get; set; }        //ソリッド書庫なら一時展開するか

        //ver1.24 マウスホイール
        public string MouseConfigWheel { get; set; }

        //ver1.25
        public bool NoEnlargeOver100p { get; set; }          //画面フィッティングは100%未満にする

        public bool IsDotByDotZoom { get; set; }             //Dot-by-Dot補間モードにする

        //ver1.21画像切り替え方法
        public AnimateMode PictureSwitchMode { get; set; }

        ////ver1.35 スクリーンショー時間[ms]
        public int SlideShowTime { get; set; }

        //ver1.42 サムネイルのフェードイン
        [Obsolete]
        public bool IsThumbFadein { get; set; }

        //ver1.49 ウィンドウの初期位置
        public bool IsWindowPosCenter { get; set; }

        //ver1.62 ツールバーの位置
        public bool IsToolbarTop { get; set; }

        //ver1.64 画面ナビゲーション.右画面クリックで進む
        public bool RightScrClickIsNextPic { get; set; }

        //ver1.64 左綴じ本でクリック位置逆転
        public bool ReverseDirectionWhenLeftBook { get; set; }

        //ver1.65 ツールバーアイテムの文字を消すか
        public bool EraseToolbarItemString { get; set; }

        //ver1.70 サイドバーのスムーススクロール
        public bool Sidebar_smoothScroll { get; set; }

        //ver1.70 2枚表示の厳格化
        //public bool dualview_exactCheck;

        //ver1.71 最終ページの動作
        public bool LastPage_stay { get; set; }

        public bool LastPage_toTop { get; set; }
        public bool LastPage_toNextArchive { get; set; }

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

        #region サムネイル

        [XmlIgnore]
        public Color ThumbnailBackColor;

        [XmlElement("XmlThumbnailBackColor")]
        public string XmlTbColor
        {
            set { ThumbnailBackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailBackColor); }
        }

        [XmlIgnore]
        public Font ThumbnailFont;

        [XmlElement("XmlThumbnailFont")]
        public string XmlTbFont
        {
            set
            {
                FontConverter fc = new FontConverter();
                ThumbnailFont = (Font)fc.ConvertFromString(value);
            }
            get
            {
                FontConverter fc = new FontConverter();
                return fc.ConvertToString(ThumbnailFont);
            }
        }

        [XmlIgnore]
        public Color ThumbnailFontColor;

        [XmlElement("XmlThumbnailFontColor")]
        public string XmlFontColor
        {
            set { ThumbnailFontColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailFontColor); }
        }

        #endregion サムネイル

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

        //#region OnPropertyChanged

        //public event PropertyChangedEventHandler PropertyChanged;

        //private void OnPropertyChanged(string s)
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(s));
        //}

        //#endregion OnPropertyChanged

        //ver1.79 書庫を常に展開するかどうか
        public bool AlwaysExtractArchive { get; set; }

        //ver1.79 2ページモード
        public bool DualView_Force { get; set; }

        public bool DualView_Normal { get; set; }
        public bool DualView_withSizeCheck { get; set; }

        //ver1.80 キーコンフィグ２
        public Keys Key_Exit1 { get; set; }

        public Keys Key_Exit2 { get; set; }
        public Keys Key_Bookmark1 { get; set; }
        public Keys Key_Bookmark2 { get; set; }
        public Keys Key_Fullscreen1 { get; set; }
        public Keys Key_Fullscreen2 { get; set; }
        public Keys Key_Dualview1 { get; set; }
        public Keys Key_Dualview2 { get; set; }
        public Keys Key_ViewRatio1 { get; set; }
        public Keys Key_ViewRatio2 { get; set; }
        public Keys Key_Recycle1 { get; set; }
        public Keys Key_Recycle2 { get; set; }

        public Keys Key_Nextpage1 { get; set; }
        public Keys Key_Nextpage2 { get; set; }
        public Keys Key_Prevpage1 { get; set; }
        public Keys Key_Prevpage2 { get; set; }
        public Keys Key_Prevhalf1 { get; set; }
        public Keys Key_Prevhalf2 { get; set; }
        public Keys Key_Nexthalf1 { get; set; }
        public Keys Key_Nexthalf2 { get; set; }
        public Keys Key_Toppage1 { get; set; }
        public Keys Key_Toppage2 { get; set; }
        public Keys Key_Lastpage1 { get; set; }
        public Keys Key_Lastpage2 { get; set; }

        //ver1.80 ダブルクリック
        public bool DoubleClickToFullscreen { get; set; }

        public bool ThumbnailPanelSmoothScroll { get; set; }

        //ver1.83 アンシャープマスク
        public bool UseUnsharpMask { get; set; }

        public int UnsharpDepth { get; set; }

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

            //サムネイルタブ
            ThumbnailSize = 200;                            //サムネイルサイズ
            ThumbnailBackColor = Color.White;               //サムネイルのバックカラー
            ThumbnailFont = new Font("MS UI Gothic", 9);    //サムネイルのフォント
            ThumbnailFontColor = Color.Black;               //サムネイルのフォントカラー
                                                            //isAutoCleanOldCache = false;					//サムネイルを自動でクリーンするか
            IsDrawThumbnailShadow = true;                   //サムネイルに影を描写するか
            IsDrawThumbnailFrame = true;                    //サムネイルに枠を描写するか
            IsShowTPFileName = true;                        //画像名を表示するか
            IsShowTPFileSize = false;                       //画像のファイルサイズを表示するか
            IsShowTPPicSize = false;                        //画像のピクセルサイズを表示するか
            //IsThumbFadein = false;

            //ルーペタブ
            loupeMagnifcant = 3;
            IsOriginalSizeLoupe = true;

            //サイドバー
            //isFixSidebar = false;
            //sidebarWidth = ThumbnailSize + 50;
            SidebarWidth = SIDEBAR_INIT_WIDTH;

            //高度な設定
            IsFastDrawAtResize = true;                      //リサイズ時に高速描写をするかどうか
                                                            //書庫
            IsExtractIfSolidArchive = true;
            //クロスフェード
            //isCrossfadeTransition = false;

            //マウスコンフィグ
            MouseConfigWheel = "拡大縮小";
            // 画面切り替えモード
            PictureSwitchMode = AnimateMode.Slide;
            //zoom
            NoEnlargeOver100p = true;       //画面フィッティングは100%未満にする
            IsDotByDotZoom = false;         //Dot-by-Dot補間モードにする
            CacheSize = 100;                    //ver1.53 100MB

            //ループするかどうか
            //isLoopToTopPage = false;
            
            //スクリーンショー時間
            SlideShowTime = 3000;
            
            //画面の初期位置
            IsWindowPosCenter = false;
            
            //ツールバーの位置
            IsToolbarTop = true;

            //ver1.64 画面クリックナビゲーション
            RightScrClickIsNextPic = true;
            ReverseDirectionWhenLeftBook = true;

            //ver1.64ツールバーアイテムの文字を消す
            EraseToolbarItemString = false;

            //ver1.70 サイドバーのスムーススクロールはOn
            Sidebar_smoothScroll = true;

            //ver1.70 2枚表示はデフォルトで簡易チェック
            //dualview_exactCheck = false;

            //ver1.71 最終ページの動作
            LastPage_stay = true;
            LastPage_toTop = false;
            LastPage_toNextArchive = false;

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

            //1.80キーコンフィグ
            Key_Exit1 = Keys.Q;
            Key_Exit2 = Keys.None;
            Key_Bookmark1 = Keys.B;
            Key_Bookmark2 = Keys.None;
            Key_Fullscreen1 = Keys.Escape;
            Key_Fullscreen2 = Keys.None;
            Key_Dualview1 = Keys.D;
            Key_Dualview2 = Keys.None;
            Key_ViewRatio1 = Keys.V;
            Key_ViewRatio2 = Keys.None;
            Key_Recycle1 = Keys.Delete;
            Key_Recycle2 = Keys.None;
            //1.80キーコンフィグ ナビゲーション関連
            Key_Nextpage1 = Keys.Right;
            Key_Nextpage2 = Keys.None;
            Key_Prevpage1 = Keys.Left;
            Key_Prevpage2 = Keys.None;
            Key_Prevhalf1 = Keys.PageUp;
            Key_Prevhalf2 = Keys.None;
            Key_Nexthalf1 = Keys.PageDown;
            Key_Nexthalf2 = Keys.None;
            Key_Toppage1 = Keys.Home;
            Key_Toppage2 = Keys.None;
            Key_Lastpage1 = Keys.End;
            Key_Lastpage2 = Keys.None;

            //ダブルクリック機能を開放する
            DoubleClickToFullscreen = false;
            //ver1.81 サムネイルパネルのアニメーション
            ThumbnailPanelSmoothScroll = true;

            //ver1.83 アンシャープマスク
            UseUnsharpMask = true;
            UnsharpDepth = 25;
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
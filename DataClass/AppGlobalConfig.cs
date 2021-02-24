using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class AppGlobalConfig : INotifyPropertyChanged
    {
        //コンフィグファイル名。XmlSerializeで利用
        private const string CONFIGNAME = "Marmi.xml";

        //サイドバーの基本の幅
        private const int SIDEBAR_INIT_WIDTH = 200;

        public bool dualView;                       //2画面並べて表示
        public Size windowSize;                     //ウィンドウサイズ
        public Point windowLocation;                //ウィンドウ表示位置
        //public MRU[] mru = new MRU[50];     //MRUリスト用配列
        public List<MRU> mru = new List<MRU>();     //MRUリスト用配列
        public bool visibleMenubar;                 //メニューバーの表示
        public bool visibleToolBar;                 //ツールバーの表示
        public bool visibleStatusBar;               //ステータスバーの表示
        public bool isSaveConfig;                   //コンフィグの保存

                                                    //public bool isSaveThumbnailCache;			//サムネイルキャッシュの保存
        public bool isRecurseSearchDir;             //ディレクトリの再帰検索

        public bool isReplaceArrowButton;               //ツールバーの左右ボタンを入れ替える

        public bool isContinueZipView;              //zipファイルは前回の続きから
        public bool isFitScreenAndImage;            //画像とイメージをフィットさせる
        public bool isStopPaintingAtResize;         //リサイズ時の描写をやめる
        public int ThumbnailSize;                   //サムネイル画像の大きさ
        public bool visibleNavibar;                 //ナビバーの表示
        public bool isAutoCleanOldCache;            //古いキャッシュの自動削除

        public bool isDrawThumbnailShadow;          // サムネイルに影を描写するか
        public bool isDrawThumbnailFrame;           // サムネイルに枠を描写するか
        public bool isShowTPFileName;               // サムネイルにファイル名を表示するか
        public bool isShowTPFileSize;               // ファイル名にファイルサイズを表示するか
        public bool isShowTPPicSize;                // ファイル名に画像サイズを表示するか

                                                    //
                                                    // ver1.35 メモリモデル
        public MemoryModel memModel;                //メモリーモデル

        public int CacheSize;                       //memModel == userDefinedのときのキャッシュサイズ[MB]
                                                    // ver1.35 最終ページを先頭へループ
                                                    //public bool isLoopToTopPage { get; set; }

        //ルーペ関連
        public int loupeMagnifcant;                 //ルーペ倍率

        public bool isOriginalSizeLoupe;            // ルーペを原寸表示とするかどうか。

        public bool isFastDrawAtResize;             // 高速描写をするかどうか

        //サイドバー関連
        //public bool isFixSidebar;					//サイドバーを固定にするかどうか
        public int sidebarWidth;                    //サイドバーの幅

        //ver1.09 書庫関連
        public bool isExtractIfSolidArchive;        //ソリッド書庫なら一時展開するか

        //ver1.09 クロスフェード
        //public bool isCrossfadeTransition;			//画面遷移でクロスフェードするか

        //ver1.21 キーコンフィグ
        //1.81 コメントアウト
        //public string keyConfNextPage;
        //public string keyConfPrevPage;
        //public string keyConfNextPageHalf;
        //public string keyConfPrevPageHalf;
        //public string keyConfTopPage;
        //public string keyConfLastPage;
        //public string keyConfFullScr;
        //public string keyConfPrintMode;
        //public string keyConfBookMark;
        //public string keyConfDualMode;
        //public string keyConfRecycleBin;
        //public string keyConfExitApp;	//ver1.77

        //ver1.24 マウスホイール
        public string mouseConfigWheel;

        //ver1.25
        public bool noEnlargeOver100p;          //画面フィッティングは100%未満にする

        public bool isDotByDotZoom;             //Dot-by-Dot補間モードにする

        //ver1.21画像切り替え方法
        public AnimateMode pictureSwitchMode;

        ////ver1.35 スクリーンショー時間[ms]
        public int slideShowTime { get; set; }

        //ver1.42 サムネイルのフェードイン
        public bool isThumbFadein;

        //ver1.49 ウィンドウの初期位置
        public bool isWindowPosCenter;

        //ver1.62 ツールバーの位置
        public bool isToolbarTop;

        //ver1.64 画面ナビゲーション.右画面クリックで進む
        public bool RightScrClickIsNextPic;

        //ver1.64 左綴じ本でクリック位置逆転
        public bool ReverseDirectionWhenLeftBook;

        //ver1.65 ツールバーアイテムの文字を消すか
        public bool eraseToolbarItemString;

        //ver1.70 サイドバーのスムーススクロール
        public bool sidebar_smoothScroll;

        //ver1.70 2枚表示の厳格化
        //public bool dualview_exactCheck;

        //ver1.71 最終ページの動作
        public bool lastPage_stay;

        public bool lastPage_toTop;
        public bool lastPage_toNextArchive;

        //ver1.73 一時展開フォルダ
        public string tmpFolder;

        //ver1.73 MRU保持数
        public int numberOfMru;

        #region メイン画面背景色

        [XmlIgnore]
        public Color BackColor;

        [XmlElementAttribute("XmlMainBackColor")]
        public string xmlMainColor
        {
            set { BackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(BackColor); }
        }

        #endregion メイン画面背景色

        #region サムネイル背景色

        [XmlIgnore]
        public Color ThumbnailBackColor;

        [XmlElementAttribute("XmlThumbnailBackColor")]
        public string xmlTbColor
        {
            set { ThumbnailBackColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailBackColor); }
        }

        #endregion サムネイル背景色

        #region サムネイル用フォント

        [XmlIgnore]
        public Font ThumbnailFont;

        [XmlElementAttribute("XmlThumbnailFont")]
        public string xmlTbFont
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

        #endregion サムネイル用フォント

        #region サムネイル用フォントカラー

        [XmlIgnore]
        public Color ThumbnailFontColor;

        [XmlElementAttribute("XmlThumbnailFontColor")]
        public string xmlFontColor
        {
            set { ThumbnailFontColor = ColorTranslator.FromHtml(value); }
            get { return ColorTranslator.ToHtml(ThumbnailFontColor); }
        }

        #endregion サムネイル用フォントカラー

        //ver1.77 画面モード保存対象にする。
        public bool isFullScreen;

        #region 保存しないパラメータ

        ////画面モード
        //[XmlIgnore]
        //public bool isFullScreen;

        //サムネイルモード
        [XmlIgnore]
        public bool isThumbnailView;

        #endregion 保存しないパラメータ

        //ver1.76 多重起動禁止フラグ
        public bool disableMultipleStarts { get; set; }

        //ver1.77 画面表示位置調整を簡易にするか
        public bool simpleCalcForWindowLocation { get; set; }

        //ver1.77 フルスクリーン状態を復元できるようにする
        public bool saveFullScreenMode { get; set; }

        //ver1.78 倍率の保持
        public bool keepMagnification { get; set; }

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string s)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(s));
        }

        #endregion OnPropertyChanged

        //ver1.79 書庫を常に展開するかどうか
        public bool AlwaysExtractArchive { get; set; }

        //ver1.79 2ページモード
        public bool dualView_Force { get; set; }

        public bool dualView_Normal { get; set; }
        public bool dualView_withSizeCheck { get; set; }

        //ver1.80 キーコンフィグ２
        public Keys ka_exit1 { get; set; }

        public Keys ka_exit2 { get; set; }
        public Keys ka_bookmark1 { get; set; }
        public Keys ka_bookmark2 { get; set; }
        public Keys ka_fullscreen1 { get; set; }
        public Keys ka_fullscreen2 { get; set; }
        public Keys ka_dualview1 { get; set; }
        public Keys ka_dualview2 { get; set; }
        public Keys ka_viewratio1 { get; set; }
        public Keys ka_viewratio2 { get; set; }
        public Keys ka_recycle1 { get; set; }
        public Keys ka_recycle2 { get; set; }

        public Keys ka_nextpage1 { get; set; }
        public Keys ka_nextpage2 { get; set; }
        public Keys ka_prevpage1 { get; set; }
        public Keys ka_prevpage2 { get; set; }
        public Keys ka_prevhalf1 { get; set; }
        public Keys ka_prevhalf2 { get; set; }
        public Keys ka_nexthalf1 { get; set; }
        public Keys ka_nexthalf2 { get; set; }
        public Keys ka_toppage1 { get; set; }
        public Keys ka_toppage2 { get; set; }
        public Keys ka_lastpage1 { get; set; }
        public Keys ka_lastpage2 { get; set; }

        //ver1.80 ダブルクリック
        public bool DoubleClickToFullscreen { get; set; }

        public bool ThumbnailPanelSmoothScroll { get; set; }

        //ver1.83 アンシャープマスク
        public bool useUnsharpMask { get; set; }

        public int unsharpDepth { get; set; }

        /*******************************************************************************/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppGlobalConfig()
        {
            Initialize();
        }

        //public AppGlobalConfig Clone()
        //{
        //	return MemberwiseClone() as AppGlobalConfig;
        //}

        /// <summary>
        /// 各パラメータの初期値
        /// </summary>
        private void Initialize()
        {
            visibleMenubar = true;
            visibleToolBar = true;
            visibleStatusBar = true;
            visibleNavibar = false;

            dualView = false;
            isFullScreen = false;
            isThumbnailView = false;
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            isSaveConfig = false;
            //isSaveThumbnailCache = false;
            isRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            BackColor = Color.LightSlateGray;
            isReplaceArrowButton = false;
            isFitScreenAndImage = true;

            isContinueZipView = false;
            isFitScreenAndImage = true;
            isStopPaintingAtResize = false;

            //サムネイルタブ
            ThumbnailSize = 200;                            //サムネイルサイズ
            ThumbnailBackColor = Color.White;               //サムネイルのバックカラー
            ThumbnailFont = new Font("MS UI Gothic", 9);    //サムネイルのフォント
            ThumbnailFontColor = Color.Black;               //サムネイルのフォントカラー
                                                            //isAutoCleanOldCache = false;					//サムネイルを自動でクリーンするか
            isDrawThumbnailShadow = true;                   //サムネイルに影を描写するか
            isDrawThumbnailFrame = true;                    //サムネイルに枠を描写するか
            isShowTPFileName = true;                        //画像名を表示するか
            isShowTPFileSize = false;                       //画像のファイルサイズを表示するか
            isShowTPPicSize = false;                        //画像のピクセルサイズを表示するか
            isThumbFadein = true;

            //ルーペタブ
            loupeMagnifcant = 3;
            isOriginalSizeLoupe = true;

            //サイドバー
            //isFixSidebar = false;
            //sidebarWidth = ThumbnailSize + 50;
            sidebarWidth = SIDEBAR_INIT_WIDTH;

            //高度な設定
            isFastDrawAtResize = true;                      //リサイズ時に高速描写をするかどうか
                                                            //書庫
            isExtractIfSolidArchive = true;
            //クロスフェード
            //isCrossfadeTransition = false;
            //キーコンフィグ
            //ver1.81コメントアウト
            //keyConfNextPage ="→";
            //keyConfPrevPage = "←";
            //keyConfNextPageHalf = "PageDown";
            //keyConfPrevPageHalf = "PageUp";
            //keyConfTopPage = "Home";
            //keyConfLastPage = "End";
            //keyConfFullScr = "ESC";
            //keyConfPrintMode = "V";
            //keyConfBookMark = "B";
            //keyConfDualMode = "D";
            //keyConfRecycleBin = "Delete";
            //keyConfExitApp = "Q";
            //マウスコンフィグ
            mouseConfigWheel = "拡大縮小";
            // 画面切り替えモード
            pictureSwitchMode = AnimateMode.Slide;
            //zoom
            noEnlargeOver100p = true;       //画面フィッティングは100%未満にする
            isDotByDotZoom = false;         //Dot-by-Dot補間モードにする
                                            //メモリーモデル
                                            //memModel = MemoryModel.Small;	//最小
            memModel = MemoryModel.UserDefined; //キャッシュ活用モード
            CacheSize = 100;                    //ver1.53 100MB
                                                //ループするかどうか
                                                //isLoopToTopPage = false;
                                                //スクリーンショー時間
            slideShowTime = 3000;
            //画面の初期位置
            isWindowPosCenter = false;
            //ツールバーの位置
            isToolbarTop = true;

            //ver1.64 画面クリックナビゲーション
            RightScrClickIsNextPic = true;
            ReverseDirectionWhenLeftBook = true;

            //ver1.64ツールバーアイテムの文字を消す
            eraseToolbarItemString = false;

            //ver1.70 サイドバーのスムーススクロールはOn
            sidebar_smoothScroll = true;

            //ver1.70 2枚表示はデフォルトで簡易チェック
            //dualview_exactCheck = false;

            //ver1.71 最終ページの動作
            lastPage_stay = true;
            lastPage_toTop = false;
            lastPage_toNextArchive = false;

            //ver1.73 一時展開フォルダ
            tmpFolder = string.Empty;
            numberOfMru = 10;

            //ver1.76 多重起動
            disableMultipleStarts = false;
            //ver1.77 ウィンドウ位置を簡易計算にするか
            simpleCalcForWindowLocation = false;
            //ver1.77 フルスクリーン状態を復元できるようにする
            saveFullScreenMode = true;
            //ver1.78 倍率の保持
            keepMagnification = false;
            //ver1.79 書庫は必ず展開
            AlwaysExtractArchive = false;
            //ver1.79 2ページモードアルゴリズム
            dualView_Force = false;
            dualView_Normal = true;
            dualView_withSizeCheck = false;

            //1.80キーコンフィグ
            ka_exit1 = Keys.Q;
            ka_exit2 = Keys.None;
            ka_bookmark1 = Keys.B;
            ka_bookmark2 = Keys.None;
            ka_fullscreen1 = Keys.Escape;
            ka_fullscreen2 = Keys.None;
            ka_dualview1 = Keys.D;
            ka_dualview2 = Keys.None;
            ka_viewratio1 = Keys.V;
            ka_viewratio2 = Keys.None;
            ka_recycle1 = Keys.Delete;
            ka_recycle2 = Keys.None;
            //1.80キーコンフィグ ナビゲーション関連
            ka_nextpage1 = Keys.Right;
            ka_nextpage2 = Keys.None;
            ka_prevpage1 = Keys.Left;
            ka_prevpage2 = Keys.None;
            ka_prevhalf1 = Keys.PageUp;
            ka_prevhalf2 = Keys.None;
            ka_nexthalf1 = Keys.PageDown;
            ka_nexthalf2 = Keys.None;
            ka_toppage1 = Keys.Home;
            ka_toppage2 = Keys.None;
            ka_lastpage1 = Keys.End;
            ka_lastpage2 = Keys.None;

            //ダブルクリック機能を開放する
            DoubleClickToFullscreen = false;
            //ver1.81 サムネイルパネルのアニメーション
            ThumbnailPanelSmoothScroll = true;

            //ver1.83 アンシャープマスク
            useUnsharpMask = true;
            unsharpDepth = 25;
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string s)
        //{
        //	if (PropertyChanged != null)
        //		PropertyChanged(this, new PropertyChangedEventArgs(s));
        //}

        /// <summary>
        /// コンフィグのファイル名を返す。
        /// プロパティを作ったので不要なはず
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static string getConfigFileName()
        {
            return Path.Combine(Application.StartupPath, CONFIGNAME);
        }

        public static string configFilename
        {
            get
            {
                return Path.Combine(Application.StartupPath, CONFIGNAME);
            }
        }

        /// <summary>
        /// XML形式で保存したObjectをロードする。
        /// </summary>
        /// <returns></returns>
        public static object LoadFromXmlFile()
        {
            //string path = getConfigFileName();
            string path = AppGlobalConfig.configFilename;

            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(AppGlobalConfig));

                    //読み込んで逆シリアル化する
                    Object obj = xs.Deserialize(fs);
                    return obj;
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
            //string path = getConfigFileName();
            string path = AppGlobalConfig.configFilename;

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xs = new XmlSerializer(typeof(AppGlobalConfig));
                //シリアル化して書き込む
                xs.Serialize(fs, obj);
            }
        }
    }
}
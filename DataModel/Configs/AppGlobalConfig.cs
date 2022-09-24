using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

/********************************************************************************
 設定を保存するクラス
XmlSerializeされる設定を管理している。
********************************************************************************/

namespace Marmi
{
    [Serializable]
    public class AppGlobalConfig
    {
        public Size windowSize;                     //ウィンドウサイズ
        public Point windowLocation;                //ウィンドウ表示位置

        public List<MRU> Mru { get; set; } = new List<MRU>();     //MRUリスト用配列

        public bool IsRecurseSearchDir { get; set; }             //ディレクトリの再帰検索

        public bool IsFitScreenAndImage { get; set; }            //画像とイメージをフィットさせる
        public bool IsStopPaintingAtResize { get; set; }         //リサイズ時の描写をやめる
        public bool IsAutoCleanOldCache { get; set; }            //古いキャッシュの自動削除
        public int SidebarWidth { get; set; }                    //サイドバーの幅

        ////ver1.35 スクリーンショー時間[ms]
        public int SlideShowTime { get; set; }

        //ver1.62 ツールバーの位置
        public bool IsToolbarTop { get; set; }

        //ver1.78 倍率の保持
        public bool KeepMagnification { get; set; }

        public GeneralConfig General { get; set; } = new GeneralConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public KeyConfig Keys { get; set; } = new KeyConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public AdvanceConfig Advance { get; set; } = new AdvanceConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        #region UIなし

        public bool UseScreenCache { get; set; } = false;   //スクリーンキャッシュを使うかどうか

        #endregion UIなし

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
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            //isSaveThumbnailCache = false;
            IsRecurseSearchDir = false;
            //BackColor = Color.DarkGray;
            IsFitScreenAndImage = true;

            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;

            //サイドバー
            SidebarWidth = App.SIDEBAR_INIT_WIDTH;

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
            string path = App.ConfigFilename;

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
            string path = App.ConfigFilename;

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
            if (string.IsNullOrEmpty(pi.PackageName))
                return;

            var mru = Mru.FirstOrDefault(a => a.Name == pi.PackageName);
            if (mru == null)
            {
                //新規追加
                Mru.Add(new MRU(pi.PackageName, DateTime.Now, pi.NowViewPage, pi.CreateBookmarkString()));
            }
            else
            {
                //過去データを更新
                mru.Date = DateTime.Now;
                mru.LastViewPage = pi.NowViewPage;
                mru.Bookmarks = pi.CreateBookmarkString();
            }
            return;
        }
    }
}
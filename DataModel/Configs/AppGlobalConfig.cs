using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        /// <summary>
        /// 2画面モードかどうか。
        /// シリアライズするためだけに存在
        /// 通常はViewState.DualViewにアクセスすること。
        /// </summary>
        public bool DualView
        {
            get => ViewState.DualView;
            set => ViewState.DualView = value;
        }

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

        /// <summary>
        /// スクリーンキャッシュを使うかどうか
        /// </summary>
        public bool UseScreenCache { get; set; } = false;

        /// <summary>
        /// MRUリスト
        /// </summary>
        public List<MRU> Mru { get; set; } = new List<MRU>();

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
            IsRecurseSearchDir = false;
            IsFitScreenAndImage = true;
            IsStopPaintingAtResize = false;
            SidebarWidth = App.SIDEBAR_INIT_WIDTH;
            SlideShowTime = 3000;
            IsToolbarTop = true;
            KeepMagnification = false;

            General.Init();
            View.Init();
            Thumbnail.Init();
            Keys.Init();
            Mouse.Init();
            Loupe.Init();
            Advance.Init();
        }

        /// <summary>
        /// 現在閲覧しているg_pi.PackageNameをMRUに追加する
        /// 以前も見たことがある場合、閲覧日付だけを更新
        /// </summary>
        public void AddMRU(PackageInfo pi)
        {
            if (pi==null || string.IsNullOrEmpty(pi.PackageName))
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
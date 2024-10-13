/********************************************************************************
AppGlobalConfig
設定を保存するクラス
XmlSerializeされる設定を管理している。
********************************************************************************/

using Marmi.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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

        /// <summary>
        /// ディレクトリの再帰検索を行うか
        /// </summary>
        public bool RecurseSearchDir { get; set; }

        /// <summary>
        /// 画像とイメージをフィットさせる
        /// </summary>
        public bool FitToScreen { get; set; }

        /// <summary>
        /// サイドバー幅
        /// </summary>
        public int SidebarWidth { get; set; }

        /// <summary>
        /// スクリーンショー時間[msec]
        /// </summary>
        public int SlideshowTime { get; set; }

        /// <summary>
        /// ツールバーの位置.上部ならtrue
        /// </summary>
        public bool ToolbarIsTop { get; set; }

        /// <summary>
        /// 表示倍率を保持する場合はtrue
        /// </summary>
        public bool KeepMagnification { get; set; }

        /// <summary>
        /// OptionFormダイアログの「全般」タブ用Config
        /// </summary>
        public GeneralConfig General { get; set; } = new GeneralConfig();

        /// <summary>
        /// OptionFormダイアログの「表示」タブ用Config
        /// </summary>
        public ViewConfig View { get; set; } = new ViewConfig();

        /// <summary>
        /// OptionFormダイアログの「キーコンフィグ」タブ用Config
        /// </summary>
        public KeyConfig Keys { get; set; } = new KeyConfig();

        /// <summary>
        /// OptionFormダイアログの「マウス」タブ用Config
        /// </summary>
        public MouseConfig Mouse { get; set; } = new MouseConfig();

        /// <summary>
        /// OptionFormダイアログの「ルーペ」タブ用Config
        /// </summary>
        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        /// <summary>
        /// OptionFormダイアログの「高度な設定」タブ用Config
        /// </summary>
        public AdvanceConfig Advance { get; set; } = new AdvanceConfig();

        /// <summary>
        /// OptionFormダイアログの「サムネイル」タブ用Config
        /// </summary>
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
        /// コンストラクタ。
        /// 各パラメータを初期値にする。
        /// </summary>
        public AppGlobalConfig()
        {
            Initialize();
        }

        /// <summary>
        /// 全プロパティを初期値に設定
        /// </summary>
        private void Initialize()
        {
            windowSize = new Size(640, 480);
            windowLocation = new Point(0, 0);
            RecurseSearchDir = false;
            FitToScreen = true;
            SidebarWidth = App.SIDEBAR_INIT_WIDTH;
            SlideshowTime = 3000;
            ToolbarIsTop = true;
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
            if (pi == null || string.IsNullOrEmpty(pi.PackageName))
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

        /// <summary>
        /// このオブジェクトをディープコピーしCloneする。
        /// </summary>
        /// <returns></returns>
        public AppGlobalConfig Clone()
        {
            var xs = new XmlSerializer(typeof(AppGlobalConfig));
            using (var mem = new MemoryStream())
            {
                xs.Serialize(mem, this);
                mem.Position = 0;
                return (AppGlobalConfig)xs.Deserialize(mem);
            }
        }
    }
}
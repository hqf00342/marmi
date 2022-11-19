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
        ///ウィンドウサイズ
        public Size WindowSize { get; set; }

        ///ウィンドウ表示位置
        public Point WindowPos { get; set; }

        /// <summary>
        /// 2画面モードかどうか。
        /// XMLシリアライズするためだけに存在
        /// 通常はViewState.DualViewにアクセスすること。
        /// </summary>
        public bool DualView
        {
            get => ViewState.DualView;
            set => ViewState.DualView = value;
        }

        //ディレクトリの再帰検索
        public bool RecurseSearchDir { get; set; }

        ////ver1.35 スクリーンショー時間[ms]
        public int SlideshowTime { get; set; }

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
            Init();
        }

        /// <summary>
        /// 各パラメータの初期値
        /// </summary>
        private void Init()
        {
            WindowSize = new Size(640, 480);
            WindowPos = new Point(0, 0);
            RecurseSearchDir = false;
            SlideshowTime = 3000;

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
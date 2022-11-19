namespace Marmi
{
    public static class ViewState
    {
        /// <summary>
        /// 2画像並べて表示するモード
        /// AppGlobalConfig.DualViewのこのプロパティのコピーがある。
        /// </summary>
        public static bool DualView { get; set; } = false;

        /// <summary>
        /// メニューバーの表示
        /// </summary>
        public static bool VisibleMenubar { get; set; } = true;

        /// <summary>
        /// ツールバーの表示
        /// </summary>
        public static bool VisibleToolBar { get; set; } = true;

        /// <summary>
        /// ステータスバーの表示
        /// </summary>
        public static bool VisibleStatusBar { get; set; } = true;

        /// <summary>
        ///サイドバーの表示
        /// </summary>
        public static bool VisibleSidebar { get; set; } = false;

        /// <summary>
        /// フルスクリーンモード
        /// </summary>
        public static bool FullScreen { get; set; } = false;

        /// <summary>
        /// サムネイルモード
        /// </summary>
        public static bool ThumbnailView { get; set; } = false;

        /// <summary>
        /// 画像とイメージをフィットさせる
        /// </summary>
        public static bool FitToScreen { get; set; } = true;

        /// <summary>
        /// サイドバーの幅。pixels
        /// </summary>
        public static int SidebarWidth { get; set; } = App.SIDEBAR_INIT_WIDTH;

        /// <summary>
        /// ツールバーの位置
        /// </summary>
        public static bool ToolbarIsTop { get; set; } = true;
    }
}
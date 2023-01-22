namespace Marmi
{
    public static class ViewState
    {
        /// <summary>
        /// 2画面並べて表示
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
        /// サイドバーの表示
        /// </summary>
        public static bool VisibleSidebar { get; set; } = false;

        /// <summary>
        /// 画面モード. 保存対象にする
        /// </summary>
        public static bool FullScreen { get; set; }

        /// <summary>
        /// サムネイルモード
        /// </summary>
        public static bool ThumbnailView { get; set; }
    }
}
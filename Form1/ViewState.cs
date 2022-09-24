namespace Marmi
{
    public static class ViewState
    {
        //2画面並べて表示
        public static bool DualView { get; set; } = false;

        //メニューバーの表示
        public static bool VisibleMenubar { get; set; } = true;

        //ツールバーの表示
        public static bool VisibleToolBar { get; set; } = true;

        //ステータスバーの表示
        public static bool VisibleStatusBar { get; set; } = true;

        //サイドバーの表示
        public static bool VisibleSidebar { get; set; } = false;

        //ver1.77 画面モード保存対象にする。
        public static bool FullScreen { get; set; }

        //サムネイルモード
        public static bool ThumbnailView { get; set; }
    }
}
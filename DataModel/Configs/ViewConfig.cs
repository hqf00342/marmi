namespace Marmi.DataModel
{
    public class ViewConfig
    {
        //画像サイズ調整は100%未満にする
        public bool ProhigitExpansionOver100p { get; set; }

        //Dot-by-Dot補間モードにする(100%以上に拡大する際に補完しない)
        public bool DotByDotZoom { get; set; }

        //ver1.79 2ページモード
        public bool DualView_Force { get; set; }

        public bool DualView_Normal { get; set; }
        public bool DualView_withSizeCheck { get; set; }

        //ver1.71 最終ページの動作
        public bool StayOnLastPage { get; set; }

        public bool MoveToTopAtLastPage { get; set; }

        //ver1.21画像切り替え方法
        public string PageTransitionEffect { get; set; }

        public void Init()
        {
            ProhigitExpansionOver100p = true;
            DotByDotZoom = false;
            StayOnLastPage = true;
            MoveToTopAtLastPage = false;
            DualView_Force = false;
            DualView_Normal = true;
            DualView_withSizeCheck = false;
            PageTransitionEffect = "アニメーション";
        }
    }
}
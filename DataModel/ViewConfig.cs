namespace Marmi.DataModel
{
    public class ViewConfig
    {
        #region Draw

        public bool UseUnsharpMask { get; set; }

        public int UnsharpDepth { get; set; }

        #endregion Draw

        //画像サイズ調整は100%未満にする
        public bool NoEnlargeOver100p { get; set; }

        //Dot-by-Dot補間モードにする(100%以上に拡大する際に補完しない)
        public bool IsDotByDotZoom { get; set; }

        //ver1.79 2ページモード
        public bool DualView_Force { get; set; }

        public bool DualView_Normal { get; set; }
        public bool DualView_withSizeCheck { get; set; }

        //ver1.71 最終ページの動作
        public bool LastPage_stay { get; set; }

        public bool LastPage_toTop { get; set; }

        public void Init()
        {
            NoEnlargeOver100p = true;
            IsDotByDotZoom = false;
            LastPage_stay = true;
            LastPage_toTop = false;
            DualView_Force = false;
            DualView_Normal = true;
            DualView_withSizeCheck = false;
            UseUnsharpMask = true;
            UnsharpDepth = 25;
        }
    }
}
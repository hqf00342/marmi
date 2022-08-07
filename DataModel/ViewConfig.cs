namespace Marmi.DataModel
{
    public class ViewConfig
    {
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

        //ver1.21画像切り替え方法
        public AnimateMode PictureSwitchMode { get; set; }

        public void Init()
        {
            NoEnlargeOver100p = true;
            IsDotByDotZoom = false;
            LastPage_stay = true;
            LastPage_toTop = false;
            DualView_Force = false;
            DualView_Normal = true;
            DualView_withSizeCheck = false;
            PictureSwitchMode = AnimateMode.Slide;
        }
    }
}
namespace Marmi.DataModel
{
    public class MouseConfig
    {
        //ver1.24 マウスホイール
        public string MouseConfigWheel { get; set; }

        //ver1.64 画面ナビゲーション.右画面クリックで進む
        public bool RightScrClickIsNextPic { get; set; }

        //ver1.64 左綴じ本でクリック位置逆転
        public bool ReverseDirectionWhenLeftBook { get; set; }

        public void Init()
        {
            //マウスコンフィグ
            MouseConfigWheel = "拡大縮小";

            //ver1.64 画面クリックナビゲーション
            RightScrClickIsNextPic = true;
            ReverseDirectionWhenLeftBook = true;
        }
    }
}
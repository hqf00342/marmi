namespace Marmi.DataModel
{
    public class MouseConfig
    {
        public string MouseConfigWheel { get; set; }

        public bool RightScrClickIsNextPic { get; set; }

        public bool ReverseDirectionWhenLeftBook { get; set; }
        public bool DoubleClickToFullscreen { get; set; }

        public void Init()
        {
            MouseConfigWheel = "拡大縮小";
            RightScrClickIsNextPic = true;
            ReverseDirectionWhenLeftBook = true;
            DoubleClickToFullscreen = false;
        }
    }
}
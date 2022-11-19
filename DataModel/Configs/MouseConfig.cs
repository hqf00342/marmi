using System.Xml.Serialization;

namespace Marmi.DataModel
{
    public class MouseConfig
    {
        public string MouseConfigWheel { get; set; }

        public bool ClickRightToNextPic { get; set; }

        [XmlIgnore]
        public bool ClickLeftToNextPic
        {
            get => !ClickRightToNextPic;
            set => ClickRightToNextPic = !value;
        }

        public bool ReverseDirectionWhenLeftBook { get; set; }

        public bool DoubleClickToFullscreen { get; set; }

        public void Init()
        {
            MouseConfigWheel = "拡大縮小";
            ClickRightToNextPic = true;
            ReverseDirectionWhenLeftBook = true;
            DoubleClickToFullscreen = false;
        }
    }
}
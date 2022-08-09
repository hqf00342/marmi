namespace Marmi.DataModel
{
    public class AdvanceConfig
    {
        public bool IsFastDrawAtResize { get; set; }

        public int CacheSize { get; set; }

        public bool UseUnsharpMask { get; set; }

        public int UnsharpDepth { get; set; }

        public void Init()
        {
            IsFastDrawAtResize = true;
            CacheSize = 500;
            UseUnsharpMask = true;
            UnsharpDepth = 25;
        }
    }
}
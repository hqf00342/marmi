namespace Marmi.DataModel
{
    public class AdvanceConfig
    {
        /// <summary>リサイズ時に高速描写をするかどうか</summary>
        public bool IsFastDrawAtResize { get; set; }

        /// <summary>キャッシュサイズ。MByte</summary>
        public int CacheSize { get; set; }

        /// <summary>アンシャープマスク</summary>
        public bool UseUnsharpMask { get; set; }

        /// <summary>アンシャープ深度</summary>
        public int UnsharpDepth { get; set; }

        public void Init()
        {
            IsFastDrawAtResize = true;
            CacheSize = 500;
            UseUnsharpMask = false;
            UnsharpDepth = 25;
        }
    }
}
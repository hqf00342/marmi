namespace Marmi.DataModel
{
    public class AdvanceConfig
    {
        /// <summary>
        /// キャッシュサイズ。MByte
        /// </summary>
        public int CacheSize { get; set; }

        /// <summary>
        /// アンシャープマスク
        /// </summary>
        public bool UnsharpMask { get; set; }

        /// <summary>
        /// アンシャープ深度
        /// </summary>
        public int UnsharpDepth { get; set; }

        public void Init()
        {
            CacheSize = 500;
            UnsharpMask = false;
            UnsharpDepth = 25;
        }
    }
}
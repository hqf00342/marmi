namespace Marmi.DataModel
{
    public class LoupeConfig
    {
        /// <summary>
        /// ルーペ倍率
        /// </summary>
        public int LoupeMagnifcant { get; set; }

        /// <summary>
        /// ルーペを原寸表示とするかどうか。
        /// </summary>
        public bool OriginalSizeLoupe { get; set; }

        public void Init()
        {
            LoupeMagnifcant = 3;
            OriginalSizeLoupe = true;
        }
    }
}
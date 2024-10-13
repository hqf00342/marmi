/*
高度な設定

2024年10月13日
BindableBase派生にしINotifyPropertyChangedに対応する。
しかし、全部をINotifyPropertyChangedにするのは面倒なので
必要なとき(初期値に戻す)にOnPropertyChangedを手動呼び出しする。
*/

using Mii;

namespace Marmi.DataModel
{
    public class AdvanceConfig : BindableBase
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

            //WinFormsのデータバインド機構は
            //1つPropertyChangedを投げると全部チェックしてくれるため
            //1つだけ投げる
            OnPropertyChanged(nameof(CacheSize));
        }
    }
}
/*
ルーペのコンフィグ

2024年10月13日
BindableBase派生にしINotifyPropertyChangedに対応する。
しかし、全部をINotifyPropertyChangedにするのは面倒なので
必要なとき(初期値に戻す)にOnPropertyChangedを手動呼び出しする。

*/

using Mii;

namespace Marmi.DataModel
{
    public class LoupeConfig : BindableBase
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

            //WinFormsのデータバインド機構は
            //1つPropertyChangedを投げると全部チェックしてくれるため
            //1つだけ投げる
            OnPropertyChanged(nameof(LoupeMagnifcant));
        }
    }
}
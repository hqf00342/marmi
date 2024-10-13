/*
マウスコンフィグ

2024年10月13日
BindableBase派生にしINotifyPropertyChangedに対応する。
しかし、全部をINotifyPropertyChangedにするのは面倒なので
必要なとき(初期値に戻す)にOnPropertyChangedを手動呼び出しする。
*/

using Mii;
using System.Xml.Serialization;

namespace Marmi.DataModel
{
    public class MouseConfig : BindableBase
    {
        /// <summary>
        /// マウスホイールの動作指定。「拡大縮小」
        /// </summary>
        public string MouseConfigWheel { get; set; }

        /// <summary>
        /// 右クリックで次ページ
        /// </summary>
        public bool ClickRightToNextPic { get; set; }

        /// <summary>
        /// 左クリックで次ページ。
        /// このプロパティは削ってはダメ。バインディングで利用している。
        /// </summary>
        [XmlIgnore]
        public bool ClickLeftToNextPic
        {
            get => !ClickRightToNextPic;
            set => ClickRightToNextPic = !value;
        }

        /// <summary>
        /// 左開き本で方向を逆転する
        /// </summary>
        public bool ReverseDirectionWhenLeftBook { get; set; }

        /// <summary>
        /// ダブルクリックで全画面にする
        /// </summary>
        public bool DoubleClickToFullscreen { get; set; }

        public void Init()
        {
            MouseConfigWheel = "拡大縮小";
            ClickRightToNextPic = true;
            ReverseDirectionWhenLeftBook = true;
            DoubleClickToFullscreen = false;

            //WinFormsのデータバインド機構は
            //1つPropertyChangedを投げると全部チェックしてくれるため
            //1つだけ投げる
            OnPropertyChanged(nameof(MouseConfigWheel));
        }
    }
}
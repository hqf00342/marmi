/********************************************************************************
ViewConfig
画面描写設定

2024年10月13日
BindableBase派生にしINotifyPropertyChangedに対応する。
しかし、全部をINotifyPropertyChangedにするのは面倒なので
必要なとき(初期値に戻す)にOnPropertyChangedを手動呼び出しする。

********************************************************************************/

using Mii;

namespace Marmi.DataModel
{
    public class ViewConfig : BindableBase
    {
        /// <summary>
        /// 画像サイズ調整は100%未満にする
        /// </summary>
        public bool ProhigitExpansionOver100p { get; set; }

        /// <summary>
        /// Dot-by-Dot補間モード(1拡大時に補完しない)
        /// </summary>
        public bool DotByDotZoom { get; set; }

        /// <summary>
        /// 2ページモード：強制
        /// </summary>
        public bool DualView_Force { get; set; }

        /// <summary>
        /// 2ページモード：通常（縦長画像同士）
        /// </summary>
        public bool DualView_Normal { get; set; }

        /// <summary>
        /// 2ページモード：サイズチェック（縦長＋高さが等しい）
        /// </summary>
        public bool DualView_withSizeCheck { get; set; }

        /// <summary>
        /// 最終ページに留まる
        /// </summary>
        public bool StayOnLastPage { get; set; }

        /// <summary>
        /// 最終ページ→先頭ページに移動
        /// </summary>
        public bool MoveToTopAtLastPage
        { get => !StayOnLastPage; set { StayOnLastPage = !value; } }

        /// <summary>
        /// 画像切り替え方法「アニメーション」
        /// </summary>
        public string PageTransitionEffect { get; set; }

        /// <summary>
        /// 表示倍率を保持する場合はtrue
        /// </summary>
        public bool KeepMagnification { get; set; }

        public void Init()
        {
            ProhigitExpansionOver100p = false;
            DotByDotZoom = false;
            StayOnLastPage = false;
            MoveToTopAtLastPage = true;
            DualView_Force = false;
            DualView_Normal = true;
            DualView_withSizeCheck = false;
            PageTransitionEffect = "アニメーション";
            KeepMagnification = false;

            //WinFormsのデータバインド機構は
            //1つPropertyChangedを投げると全部チェックしてくれるため
            //1つだけ投げる
            OnPropertyChanged(nameof(DotByDotZoom));
        }
    }
}
using System.Collections.Generic;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// キーコンフィグと呼び出しメソッドを関連づけるDic
        /// </summary>
        private readonly Dictionary<Keys, MethodInvoker> KeyDefines = new Dictionary<Keys, MethodInvoker>();

        /// <summary>
        /// キーコンフィグをDicに登録するメソッド
        /// マウスの中ボタン、進む戻るはマウスイベントで実施
        /// </summary>
        private void SetKeyConfig2()
        {
            KeyDefines.Clear();

            void setkey(Keys key, MethodInvoker func)
            {
                if (key != Keys.None)
                    KeyDefines.Add(key, func);
            }

            //前後ページ移動
            setkey(App.Config.Key_Nextpage1, NavigateToForword);
            setkey(App.Config.Key_Nextpage2, NavigateToForword);
            setkey(App.Config.Key_Prevpage1, NavigateToBack);
            setkey(App.Config.Key_Prevpage2, NavigateToBack);

            //前後ページ移動（半分）
            setkey(App.Config.Key_Nexthalf1, () => SetViewPage(++App.g_pi.NowViewPage));
            setkey(App.Config.Key_Nexthalf2, () => SetViewPage(++App.g_pi.NowViewPage));
            setkey(App.Config.Key_Prevhalf1, () => SetViewPage(--App.g_pi.NowViewPage));
            setkey(App.Config.Key_Prevhalf2, () => SetViewPage(--App.g_pi.NowViewPage));

            //先頭最終ページ
            setkey(App.Config.Key_Toppage1, () => SetViewPage(0));
            setkey(App.Config.Key_Toppage2, () => SetViewPage(0));
            setkey(App.Config.Key_Lastpage1, () => SetViewPage(App.g_pi.Items.Count - 1));
            setkey(App.Config.Key_Lastpage2, () => SetViewPage(App.g_pi.Items.Count - 1));

            //ブックマーク
            setkey(App.Config.Key_Bookmark1, ToggleBookmark);
            setkey(App.Config.Key_Bookmark2, ToggleBookmark);

            //フルスクリーン
            setkey(App.Config.Key_Fullscreen1, ToggleFullScreen);
            setkey(App.Config.Key_Fullscreen2, ToggleFullScreen);
            //２画面モード切替
            setkey(App.Config.Key_Dualview1, () => SetDualViewMode(!App.Config.DualView));
            setkey(App.Config.Key_Dualview2, () => SetDualViewMode(!App.Config.DualView));
            // ゴミ箱
            setkey(App.Config.Key_Recycle1, () => RecycleBinNowPage());
            setkey(App.Config.Key_Recycle2, () => RecycleBinNowPage());
            //表示モード
            setkey(App.Config.Key_ViewRatio1, ToggleFitScreen);
            setkey(App.Config.Key_ViewRatio2, ToggleFitScreen);
            // 終了 ver1.77
            setkey(App.Config.Key_Exit1, () => Application.Exit());
            setkey(App.Config.Key_Exit2, () => Application.Exit());
        }
    }
}
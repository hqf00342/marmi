using System.Collections.Generic;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// キーコンフィグと呼び出しメソッドを関連づけるDic
        /// </summary>
        private Dictionary<Keys, MethodInvoker> KeyDefines = new Dictionary<Keys, MethodInvoker>();

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
            setkey(App.Config.ka_nextpage1, NavigateToForword);
            setkey(App.Config.ka_nextpage2, NavigateToForword);
            setkey(App.Config.ka_prevpage1, NavigateToBack);
            setkey(App.Config.ka_prevpage2, NavigateToBack);

            //前後ページ移動（半分）
            setkey(App.Config.ka_nexthalf1, () => SetViewPage(++g_pi.NowViewPage));
            setkey(App.Config.ka_nexthalf2, () => SetViewPage(++g_pi.NowViewPage));
            setkey(App.Config.ka_prevhalf1, () => SetViewPage(--g_pi.NowViewPage));
            setkey(App.Config.ka_prevhalf2, () => SetViewPage(--g_pi.NowViewPage));

            //先頭最終ページ
            setkey(App.Config.ka_toppage1, () => SetViewPage(0));
            setkey(App.Config.ka_toppage2, () => SetViewPage(0));
            setkey(App.Config.ka_lastpage1, () => SetViewPage(g_pi.Items.Count - 1));
            setkey(App.Config.ka_lastpage2, () => SetViewPage(g_pi.Items.Count - 1));

            //ブックマーク
            setkey(App.Config.ka_bookmark1, ToggleBookmark);
            setkey(App.Config.ka_bookmark2, ToggleBookmark);

            //フルスクリーン
            setkey(App.Config.ka_fullscreen1, ToggleFullScreen);
            setkey(App.Config.ka_fullscreen2, ToggleFullScreen);
            //２画面モード切替
            setkey(App.Config.ka_dualview1, () => SetDualViewMode(!App.Config.dualView));
            setkey(App.Config.ka_dualview2, () => SetDualViewMode(!App.Config.dualView));
            // ゴミ箱
            setkey(App.Config.ka_recycle1, () => RecycleBinNowPage());
            setkey(App.Config.ka_recycle2, () => RecycleBinNowPage());
            //表示モード
            setkey(App.Config.ka_viewratio1, ToggleFitScreen);
            setkey(App.Config.ka_viewratio2, ToggleFitScreen);
            // 終了 ver1.77
            setkey(App.Config.ka_exit1, () => Application.Exit());
            setkey(App.Config.ka_exit2, () => Application.Exit());
        }
    }
}
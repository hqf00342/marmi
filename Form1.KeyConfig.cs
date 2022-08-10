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
            setkey(App.Config.Keys.Key_Nextpage1, async () => { await NavigateToForwordAsync(); });
            setkey(App.Config.Keys.Key_Nextpage2, async () => { await NavigateToForwordAsync(); });
            setkey(App.Config.Keys.Key_Prevpage1, async () => { await NavigateToBackAsync(); });
            setkey(App.Config.Keys.Key_Prevpage2, async () => { await NavigateToBackAsync(); });

            //前後ページ移動（半分）
            setkey(App.Config.Keys.Key_Nexthalf1, async () => { await SetViewPageAsync(++App.g_pi.NowViewPage); });
            setkey(App.Config.Keys.Key_Prevhalf1, async () => { await SetViewPageAsync(--App.g_pi.NowViewPage); });

            //先頭最終ページ
            setkey(App.Config.Keys.Key_Toppage1, async () => { await SetViewPageAsync(0); });
            setkey(App.Config.Keys.Key_Lastpage1, async () => { await SetViewPageAsync(App.g_pi.Items.Count - 1); });

            //ブックマーク
            setkey(App.Config.Keys.Key_Bookmark1, ToggleBookmark);

            //フルスクリーン
            setkey(App.Config.Keys.Key_Fullscreen1, ToggleFullScreen);
            //２画面モード切替
            setkey(App.Config.Keys.Key_Dualview1, async () => { await SetDualViewModeAsync(!App.Config.DualView); });
            // ゴミ箱
            setkey(App.Config.Keys.Key_Recycle1, async () => { await RecycleBinNowPageAsync(); });
            //表示モード
            setkey(App.Config.Keys.Key_ViewRatio1, ToggleFitScreen);
            // 終了 ver1.77
            setkey(App.Config.Keys.Key_Exit1, () => Application.Exit());
            setkey(App.Config.Keys.Key_Exit2, () => Application.Exit());
            // 回転 ver 1.91
            setkey(App.Config.Keys.Key_Rotate1, () => PicPanel.Rotate());
            //サムネイル・サイドバー
            setkey(App.Config.Keys.Key_Thumbnail, () => Menu_ViewThumbnail_Click(null, null));
            setkey(App.Config.Keys.Key_Sidebar, () => Menu_ViewSidebar_Click(null, null));
        }
    }
}
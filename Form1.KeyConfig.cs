using System.Collections.Generic;
using System.Windows.Forms;


namespace Marmi
{
	public partial class Form1 : Form
	{
		/// <summary>
		/// キーコンフィグと呼び出しメソッドを関連づけるDic
		/// </summary>
		private Dictionary<Keys, MethodInvoker> KeyMethods = new Dictionary<Keys, MethodInvoker>();
		//#region --- キーコンフィグリスト ---
		//public static Dictionary<string, Keys> keyConfigList = new Dictionary<string, Keys>()
		//{
		//	//{"(なし)", Keys.None},	//ver1.60重複チェック対象外のためはずす
		//	{"→", Keys.Right},
		//	{"←", Keys.Left},
		//	{"↑", Keys.Up},
		//	{"↓", Keys.Down},
		//	{"ESC", Keys.Escape},
		//	{"Space", Keys.Space},
		//	{"Enter", Keys.Enter},
		//	{"Tab", Keys.Tab},
		//	{"PageUp", Keys.PageUp},
		//	{"PageDown", Keys.PageDown},
		//	{"Home", Keys.Home},
		//	{"End", Keys.End},
		//	{"Insert", Keys.Insert},
		//	{"Delete", Keys.Delete},
		//	{"BackSpace", Keys.Back},
		//	{"1", Keys.D1},
		//	{"2", Keys.D2},
		//	{"3", Keys.D3},
		//	{"4", Keys.D4},
		//	{"5", Keys.D5},
		//	{"6", Keys.D6},
		//	{"7", Keys.D7},
		//	{"8", Keys.D8},
		//	{"9", Keys.D9},
		//	{"0", Keys.D0},
		//	{"A", Keys.A},
		//	{"B", Keys.B},
		//	{"C", Keys.C},
		//	{"D", Keys.D},
		//	{"E", Keys.E},
		//	{"F", Keys.F},
		//	{"G", Keys.G},
		//	{"H", Keys.H},
		//	{"I", Keys.I},
		//	{"J", Keys.J},
		//	{"K", Keys.K},
		//	{"L", Keys.L},
		//	{"M", Keys.M},
		//	{"N", Keys.N},
		//	{"O", Keys.O},
		//	{"P", Keys.P},
		//	{"Q", Keys.Q},
		//	{"R", Keys.R},
		//	{"S", Keys.S},
		//	{"T", Keys.T},
		//	{"U", Keys.U},
		//	{"V", Keys.V},
		//	{"W", Keys.W},
		//	{"X", Keys.X},
		//	{"Y", Keys.Y},
		//	{"Z", Keys.Z}
		//};
		//#endregion

		//private void SetKeyConfig()
		//{
		//	SetKeyConfig2();
		//	return;

		//	KeyMethods.Clear();
		//	Keys keyValue = Keys.None;

		//	//次のページ
		//	if (keyConfigList.TryGetValue(g_Config.keyConfNextPage, out keyValue))
		//		KeyMethods.Add(keyValue, NavigateToForword);
		//	//次のページ（半分）
		//	if (keyConfigList.TryGetValue(g_Config.keyConfNextPageHalf, out keyValue))
		//		KeyMethods.Add(keyValue, () => SetViewPage(++g_pi.NowViewPage));
		//	//最後のページ
		//	if (keyConfigList.TryGetValue(g_Config.keyConfLastPage, out keyValue))
		//		KeyMethods.Add(keyValue, () => SetViewPage(g_pi.Items.Count - 1));
		//	//前のページ
		//	if (keyConfigList.TryGetValue(g_Config.keyConfPrevPage, out keyValue))
		//		KeyMethods.Add(keyValue, NavigateToBack);
		//	//前のページ（半分）
		//	if (keyConfigList.TryGetValue(g_Config.keyConfPrevPageHalf, out keyValue))
		//		KeyMethods.Add(keyValue, () => SetViewPage(--g_pi.NowViewPage));
		//	//ブックマーク
		//	if (keyConfigList.TryGetValue(g_Config.keyConfBookMark, out keyValue))
		//		KeyMethods.Add(keyValue, ToggleBookmark);
		//	//フルスクリーン
		//	if (keyConfigList.TryGetValue(g_Config.keyConfFullScr, out keyValue))
		//		KeyMethods.Add(keyValue, ToggleFullScreen);
		//	//表示モード
		//	if (keyConfigList.TryGetValue(g_Config.keyConfPrintMode, out keyValue))
		//		KeyMethods.Add(keyValue, ToggleFitScreen);
		//	//先頭ページ
		//	if (keyConfigList.TryGetValue(g_Config.keyConfTopPage, out keyValue))
		//		KeyMethods.Add(keyValue, () => SetViewPage(0));
		//	//２画面モード切替
		//	if (keyConfigList.TryGetValue(g_Config.keyConfDualMode, out keyValue))
		//		KeyMethods.Add(keyValue, () => SetDualViewMode(!g_Config.dualView));
		//	// ゴミ箱
		//	if (keyConfigList.TryGetValue(g_Config.keyConfRecycleBin, out keyValue))
		//		KeyMethods.Add(keyValue, () => RecycleBinNowPage());
		//	// 終了 ver1.77
		//	if (keyConfigList.TryGetValue(g_Config.keyConfExitApp, out keyValue))
		//		KeyMethods.Add(keyValue, () => Application.Exit());

		//}

		/// <summary>
		/// キーコンフィグをDicに登録するメソッド
		/// マウスの中ボタン、進む戻るはマウスイベントで実施
		/// </summary>
		private void SetKeyConfig2()
		{
			KeyMethods.Clear();

			//前後ページ移動
			if (g_Config.ka_nextpage1 != Keys.None)
				KeyMethods.Add(g_Config.ka_nextpage1, NavigateToForword);
			if (g_Config.ka_nextpage2 != Keys.None)
				KeyMethods.Add(g_Config.ka_nextpage2, NavigateToForword);
			if (g_Config.ka_prevpage1 != Keys.None)
				KeyMethods.Add(g_Config.ka_prevpage1, NavigateToBack);
			if (g_Config.ka_prevpage2 != Keys.None)
				KeyMethods.Add(g_Config.ka_prevpage2, NavigateToBack);

			//前後ページ移動（半分）
			if (g_Config.ka_nexthalf1 != Keys.None)
				KeyMethods.Add(g_Config.ka_nexthalf1, () => SetViewPage(++g_pi.NowViewPage));
			if (g_Config.ka_nexthalf2 != Keys.None)
				KeyMethods.Add(g_Config.ka_nexthalf2, () => SetViewPage(++g_pi.NowViewPage));
			if (g_Config.ka_prevhalf1 != Keys.None)
				KeyMethods.Add(g_Config.ka_prevhalf1, () => SetViewPage(--g_pi.NowViewPage));
			if (g_Config.ka_prevhalf2 != Keys.None)
				KeyMethods.Add(g_Config.ka_prevhalf2, () => SetViewPage(--g_pi.NowViewPage));

			//先頭最終ページ
			if(g_Config.ka_toppage1 != Keys.None)
				KeyMethods.Add(g_Config.ka_toppage1, () => SetViewPage(0));
			if (g_Config.ka_toppage2 != Keys.None)
				KeyMethods.Add(g_Config.ka_toppage2, () => SetViewPage(0));
			if (g_Config.ka_lastpage1 != Keys.None)
				KeyMethods.Add(g_Config.ka_lastpage1, () => SetViewPage(g_pi.Items.Count - 1));
			if (g_Config.ka_lastpage2 != Keys.None)
				KeyMethods.Add(g_Config.ka_lastpage2, () => SetViewPage(g_pi.Items.Count - 1));


			//ブックマーク
			if(g_Config.ka_bookmark1 != Keys.None)
				KeyMethods.Add(g_Config.ka_bookmark1, ToggleBookmark);
			if (g_Config.ka_bookmark2 != Keys.None)
				KeyMethods.Add(g_Config.ka_bookmark2, ToggleBookmark);

			//フルスクリーン
			if (g_Config.ka_fullscreen1 != Keys.None)
				KeyMethods.Add(g_Config.ka_fullscreen1, ToggleFullScreen);
			if (g_Config.ka_fullscreen2 != Keys.None)
				KeyMethods.Add(g_Config.ka_fullscreen2, ToggleFullScreen);
			//２画面モード切替
			if (g_Config.ka_dualview1 != Keys.None)
				KeyMethods.Add(g_Config.ka_dualview1, ()=>SetDualViewMode(!g_Config.dualView) );
			if (g_Config.ka_dualview2 != Keys.None)
				KeyMethods.Add(g_Config.ka_dualview2, () => SetDualViewMode(!g_Config.dualView));
			// ゴミ箱
			if (g_Config.ka_recycle1 != Keys.None)
				KeyMethods.Add(g_Config.ka_recycle1, () => RecycleBinNowPage());
			if (g_Config.ka_recycle2 != Keys.None)
				KeyMethods.Add(g_Config.ka_recycle2, () => RecycleBinNowPage());
			//表示モード
			if (g_Config.ka_viewratio1 != Keys.None)
				KeyMethods.Add(g_Config.ka_viewratio1, ToggleFitScreen);
			if (g_Config.ka_viewratio2 != Keys.None)
				KeyMethods.Add(g_Config.ka_viewratio2, ToggleFitScreen);
			// 終了 ver1.77
			if (g_Config.ka_exit1 != Keys.None)
				KeyMethods.Add(g_Config.ka_exit1, () => Application.Exit());
			if (g_Config.ka_exit2 != Keys.None)
				KeyMethods.Add(g_Config.ka_exit2, () => Application.Exit());

		}


	}
}
using System;
using System.ComponentModel;			//EventArgs
using System.IO;						//Directory, File
using System.Windows.Forms;

//using System.Drawing;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // メニューイベント ************************************************************************

        private void OnClickMRUMenu(object sender, EventArgs e)
        {
            ToolStripDropDownItem tsddi = (ToolStripDropDownItem)sender;
            if (File.Exists(tsddi.Text))
            {
                //OpenFileAndStart(tsddi.Text);

                //ver1.09 初期化してスタートを明確に
                //OpenFileAndStart(tsddi.Text);は何をするのかよく分からない状態なので削除
                Start(new string[] { tsddi.Text });
            }
            else
            {
                string sz = string.Format("ファイルが見つかりませんでした\n{0}", tsddi.Text);
                MessageBox.Show(sz, "ファイルオープンエラー");

                //MRUリストから削除
                for (int i = 0; i < App.Config.mru.Count; i++)
                {
                    if (App.Config.mru[i] != null && App.Config.mru[i].Name == tsddi.Text)
                    {
                        App.Config.mru[i] = null;
                        break;
                    }
                }
            }
        }

        //ファイルメニュー**************************************************************************

        private void Menu_FileOpen_Click(object sender, EventArgs e)
        {
            OpenDialog();
        }

        private void Menu_SaveThumbnail_Click(object sender, EventArgs e)
        {
            g_ThumbPanel.Location = GetClientRectangle().Location;
            g_ThumbPanel.Size = GetClientRectangle().Size;
            g_ThumbPanel.Parent = this;
            g_ThumbPanel.SaveThumbnail(App.g_pi.PackageName);
        }

        private void Menu_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void Menu_ClearMRU_Click(object sender, EventArgs e)
        {
            App.Config.mru.Clear();
            //for (int i = 0; i < App.Config.mru.Length; i++)
            //{
            //    App.Config.mru[i] = null;
            //}
        }

        //表示メニュー******************************************************************************

        private void Menu_ViewNext_Click(object sender, EventArgs e)
        {
            NavigateToForword();
        }

        private void Menu_ViewBack_Click(object sender, EventArgs e)
        {
            NavigateToBack();
        }

        private void Menu_ViewTop_Click(object sender, EventArgs e)
        {
            SetViewPage(0);
        }

        private void Menu_ViewEnd_Click(object sender, EventArgs e)
        {
            SetViewPage(App.g_pi.Items.Count - 1);
        }

        private void Menu_ViewThumbnail_Click(object sender, EventArgs e)
        {
            if (App.g_pi.Items.Count > 0)
            {
                //全画面であればメニュー系は全て非表示
                if (App.Config.isFullScreen)
                {
                    toolStrip1.Visible = false;
                    statusbar.Visible = false;
                    menuStrip1.Visible = false;
                }
                SetThumbnailView(!App.Config.isThumbnailView);
            }
        }

        private void Menu_ViewFullScreen_Click(object sender, EventArgs e)
        {
            SetFullScreen(!App.Config.isFullScreen);
        }

        private void Menu_ViewMenubar_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            App.Config.visibleMenubar = !App.Config.visibleMenubar;
            menuStrip1.Visible = App.Config.visibleMenubar;

            //ver0.972 サイドバーの位置調整
            //AjustControlArrangement();
            //再描写
            ReloadPage();
        }

        private void Menu_ViewToolbar_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            App.Config.visibleToolBar = !App.Config.visibleToolBar;
            toolStrip1.Visible = App.Config.visibleToolBar;

            //ver0.972 ナビバーがあればリサイズ
            //AjustControlArrangement();
            //再描写
            ReloadPage();
        }

        private void Menu_ViewStatusbar_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            App.Config.visibleStatusBar = !App.Config.visibleStatusBar;
            statusbar.Visible = App.Config.visibleStatusBar;

            //ver0.972 サイドバーの位置調整
            //AjustControlArrangement();
            //再描写
            ReloadPage();
        }

        private void Menu_ViewDualPage_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            SetDualViewMode(!App.Config.dualView);
        }

        private void Menu_ViewHalfPageBack_Click(object sender, EventArgs e)
        {
            if (App.Config.dualView)
            {
                if (App.g_pi.NowViewPage > 0)
                {
                    //g_pi.NowViewPage -= 1;	//半ページ戻し
                    SetViewPage(--App.g_pi.NowViewPage);    //ver0.988
                }
                else
                {
                    //TODO:いつかきれいにしましょう
                    //InformationLabel il = new InformationLabel(
                    //    this,
                    //    "先頭ページです。" + g_pi.ViewPage.ToString()
                    //    );
                }
            }
        }

        private void Menu_ViewHalfPageForword_Click(object sender, EventArgs e)
        {
            if (App.Config.dualView)
            {
                if (App.g_pi.NowViewPage < App.g_pi.Items.Count)
                {
                    App.g_pi.NowViewPage += 1;  //半ページ戻し
                    SetViewPage(App.g_pi.NowViewPage);  //ver0.988 2010年6月20日
                }
                else
                {
                    //TODO:いつかきれいにしましょう。最終ページじゃないし
                    //InformationLabel il = new InformationLabel(
                    //    this,
                    //    "最終ページです。" + g_pi.ViewPage.ToString()
                    //    );
                }
                //setStatusbarPages();
                //setStatusbarFilename();
            }
        }

        private void Menu_ViewPictureInfo_Click(object sender, EventArgs e)
        {
            //ver1.81 画像情報確認
            if (App.g_pi.NowViewPage < 0 || App.g_pi.NowViewPage >= App.g_pi.Items.Count)
                return;

            FormPictureInfo p = new FormPictureInfo();
            if (g_viewPages == 1)
                p.Show(this, App.g_pi.Items[App.g_pi.NowViewPage], null);
            else
                p.Show(this, App.g_pi.Items[App.g_pi.NowViewPage], App.g_pi.Items[App.g_pi.NowViewPage + 1]);
        }

        private void Menu_ViewPackageInfo_Click(object sender, EventArgs e)
        {
            //ver1.81 画像数確認
            if (App.g_pi.Items.Count <= 0)
                return;

            FormPackageInfo pif = new FormPackageInfo(this, App.g_pi);
            pif.setSortMode(false);
            //pif.Show(g_pi.ViewPage);
            pif.ShowDialog(App.g_pi.NowViewPage);
        }

        private void Menu_ViewFitScreenSize_Click(object sender, EventArgs e)
        {
            ToggleFitScreen();
        }

        private void ToggleFitScreen()
        {
            App.Config.isFitScreenAndImage = !App.Config.isFitScreenAndImage;
            PicPanel.isAutoFit = App.Config.isFitScreenAndImage;

            PicPanel.Refresh();
            UpdateStatusbar();
        }

        private void Menu_ViewSidebar_Click(object sender, EventArgs e)
        {
            if (g_Sidebar.Visible)
            {
                //閉じる
                g_Sidebar.Visible = false;
                App.Config.visibleNavibar = false;
            }
            else
            {
                //サイドバーオープン
                g_Sidebar.Init(App.g_pi);
                if (App.Config != null)
                    g_Sidebar.Width = App.Config.sidebarWidth;
                else
                    g_Sidebar.Width = App.SIDEBAR_DEFAULT_WIDTH;

                g_Sidebar.Visible = true;
                g_Sidebar.SetItemToCenter(App.g_pi.NowViewPage);
                App.Config.visibleNavibar = true;
            }
            AjustSidebarArrangement();
        }

        [Obsolete]
        private void Menu_ViewFixSidebar_Click(object sender, EventArgs e)
        {
            //App.Config.isFixSidebar = !App.Config.isFixSidebar;
            //MenuItem_ViewFixSidebar.Checked = App.Config.isFixSidebar;
            //MenuItem_OptionSidebarFix.Checked = App.Config.isFixSidebar;
        }

        private void Menu_View_LeftOpen_Click(object sender, EventArgs e)
        {
            //g_pi.PageDirectionisRight = !MenuItem_View_LeftOpen.Checked;
            App.g_pi.PageDirectionIsLeft = !App.g_pi.PageDirectionIsLeft;
            if (App.Config.dualView)
            {
                SetViewPage(App.g_pi.NowViewPage);
            }
        }

        private void Menu_ToolbarBottom_Click(object sender, EventArgs e)
        {
            if (toolStrip1.Dock == DockStyle.Bottom)
                toolStrip1.Dock = DockStyle.Top;
            else
                toolStrip1.Dock = DockStyle.Bottom;
        }

        private void Menu_Reload_Click(object sender, EventArgs e)
        {
            ReloadPage();
        }

        //ヘルプメニュー*****************************************************************************

        private void MenuItem_HelpVersion_Click(object sender, EventArgs e)
        {
            VersionForm vf = new VersionForm();
            vf.StartPosition = FormStartPosition.CenterParent;
            vf.ShowDialog();
        }

        //オプションメニュー*************************************************************************

        private void Menu_Option_Click(object sender, EventArgs e)
        {
            FormOption fo = new FormOption();
            fo.LoadConfig(App.Config);
            if (fo.ShowDialog() == DialogResult.OK)
            {
                fo.SaveConfig(ref App.Config);

                //ver1.21 キーコンフィグ反映
                //ver1.81 変更
                //SetKeyConfig();
                SetKeyConfig2();

                //ver1.65 ツールバーの文字はすぐ反映
                SetToolbarString();
                ResizeTrackBar();

                //ver1.79 ScreenCacheをクリアする。
                App.ScreenCache.Clear();

                //サムネイルサイズはすぐに反映
                //if (g_ThumbPanel != null)
                if (g_ThumbPanel != null && g_ThumbPanel.Visible)
                {
                    g_ThumbPanel.CalcThumbboxSize(App.Config.ThumbnailSize);
                    g_ThumbPanel.BackColor = App.Config.ThumbnailBackColor;
                    g_ThumbPanel.SetFont(App.Config.ThumbnailFont, App.Config.ThumbnailFontColor);
                }
                if (App.Config.isThumbnailView)
                {
                    g_ThumbPanel.ReDraw();
                }
                else
                {
                    //通常画面を再描写
                    SetViewPage(App.g_pi.NowViewPage);
                }
            }
        }

        private void Menu_ClearCacheFile_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(Application.StartupPath, "*" + App.CACHEEXT);
            if (files.Length > 0)
            {
                long size = 0;
                foreach (string delFile in files)
                {
                    FileInfo fi = new FileInfo(delFile);
                    size += fi.Length;
                    File.Delete(delFile);
                }
                MessageBox.Show(string.Format("{0}個のキャッシュ、{1:N0}バイトを削除しました",
                    files.Length, size));
            }
            else
                MessageBox.Show("キャッシュファイルはありませんでした");
        }

        private void Menu_RemakeThumbnail_Click(object sender, EventArgs e)
        {
            //ver1.81 画像数確認
            if (App.g_pi.Items.Count <= 0)
                return;

            //サムネイルをクリアする
            for (int i = 0; i < App.g_pi.Items.Count; i++)
            {
                if (App.g_pi.Items[i].Thumbnail != null)
                    App.g_pi.Items[i].Thumbnail.Dispose();
                App.g_pi.Items[i].Thumbnail = null;
            }
            //ver 1.55再登録
            AsyncLoadImageInfo();
        }

        private void Menu_RecurseDir_Click(object sender, EventArgs e)
        {
            App.Config.isRecurseSearchDir = !App.Config.isRecurseSearchDir;
            Menu_OptionRecurseDir.Checked = App.Config.isRecurseSearchDir;
        }

        private void Menu_keepMagnification_Click(object sender, EventArgs e)
        {
            App.Config.keepMagnification = !App.Config.keepMagnification;
        }

        private void Menu_UseBicubic_Click(object sender, EventArgs e)
        {
            App.Config.isDotByDotZoom = !App.Config.isDotByDotZoom;
        }

        private void Menu_DontEnlargeOver100percent_Click(object sender, EventArgs e)
        {
            App.Config.noEnlargeOver100p = !App.Config.noEnlargeOver100p;
            SetViewPage(App.g_pi.NowViewPage);
        }

        // コンテキストメニュー  ********************************************************************

        private void Menu_ContextBookmark_Click(object sender, EventArgs e)
        {
            ToggleBookmark();
        }

        //ソートメニュー*****************************************************************************

        private void Menu_SortByName_Click(object sender, EventArgs e)
        {
            //ファイルリストを並び替える
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
                App.g_pi.Items.Sort(comparer);

                //サムネイル表示中であれば再描写させる
                if (App.Config.isThumbnailView)
                {
                    g_ThumbPanel.ReDraw();
                }

                //ver1.38 ソート後に画面を書き直す
                App.ScreenCache.Clear();
                SetViewPage(App.g_pi.NowViewPage);
            }
        }

        private void Menu_SortByDate_Click(object sender, EventArgs e)
        {
            //ファイルリストを並び替える
            if (App.g_pi.Items.Count > 0)
            {
                //StopThumbnailMakerThread();	//ソート中にスレッドが動いていないことを担保
                //PauseThumbnailMakerThread();	//ver1.09 スレッド中断（Pause）

                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.CreateDate);
                App.g_pi.Items.Sort(comparer);

                //サムネイル表示中であれば再描写させる
                if (App.Config.isThumbnailView)
                {
                    //ThumbPanel.MakeThumbnailScreen(true);	//強制再描写
                    //ThumbPanel.Invalidate();
                    g_ThumbPanel.ReDraw();
                }
                //StartThumnailMakerThread();//ソート完了、スレッド再開
                //ResumeThumbnailMakerThread();	//ver1.09 スレッド再開

                //ver1.38 ソート後に画面を書き直す
                App.ScreenCache.Clear();
                SetViewPage(App.g_pi.NowViewPage);
            }
        }

        private void Menu_SortCustom_Click(object sender, EventArgs e)
        {
            FormPackageInfo pif = new FormPackageInfo(this, App.g_pi);
            pif.setSortMode(true);
            pif.ShowDialog(App.g_pi.NowViewPage);

            //ver1.38 ソート後に画面を書き直す
            App.ScreenCache.Clear();
            SetViewPage(App.g_pi.NowViewPage);
        }

        // メニューホバーイベント *******************************************************************

        private void Menu_MouseHover(object sender, EventArgs e)
        {
            //1クリック対応用に保持しておく
            g_hoverStripItem = sender;
        }

        private void Menu_MouseLeave(object sender, EventArgs e)
        {
            g_hoverStripItem = null;
        }

        private void menuStrip1_MenuDeactivate(object sender, EventArgs e)
        {
            //全画面モードでフォーカスを失ったときは隠す
            if (App.Config.isFullScreen)
                menuStrip1.Visible = false;
        }

        // メニューオープニングイベント *************************************************************

        private void Menu_File_DropDownOpening(object sender, EventArgs e)
        {
            //MRUを追加
            UpdateMruMenuListUI();

            ////ファイルを閲覧していない場合のナビゲーション
            //if (g_pi.Items == null || g_pi.Items.Count < 1)
            //{
            //	MenuItem_FileSaveThumbnail.Enabled = false;
            //}
            //else
            //{
            //	//サムネイルボタン
            //	MenuItem_FileSaveThumbnail.Enabled = true;
            //}

            //ver1.81サムネイルはしばらく無視
            MenuItem_FileSaveThumbnail.Enabled = false;
        }

        private void Menu_View_DropDownOpening(object sender, EventArgs e)
        {
            //バー関連のメニュー
            Menu_ViewToolbar.Enabled = !App.Config.isFullScreen;
            Menu_ViewStatusbar.Enabled = !App.Config.isFullScreen;
            Menu_ViewMenubar.Checked = menuStrip1.Visible;
            Menu_ViewToolbar.Checked = toolStrip1.Visible;
            Menu_ViewStatusbar.Checked = statusbar.Visible;
            Menu_View2Page.Checked = App.Config.dualView;
            Menu_ViewFullScreen.Checked = App.Config.isFullScreen;
            Menu_ViewFitScreenSize.Checked = App.Config.isFitScreenAndImage;
            Menu_ViewNavibar.Checked = g_Sidebar.Visible;
            //ツールバーの位置
            Menu_ToolbarBottom.Checked = (toolStrip1.Dock == DockStyle.Bottom);
            //サイドバー関連
            //MenuItem_ViewFixSidebar.Checked = App.Config.isFixSidebar;
            //しおり関連機能
            //ver1.79コメントアウト
            //AddBookmarkMenuItem(MenuItem_ViewBookmarkList);

            //ver1.81 パッケージなしのときの対処
            bool isPackageOpen = (App.g_pi.Items.Count > 0);
            Menu_ViewPictureInfo.Enabled = isPackageOpen;
            Menu_ViewPackageInfo.Enabled = isPackageOpen;
        }

        private void Menu_Option_DropDownOpening(object sender, EventArgs e)
        {
            //チェック状態
            Menu_OptionRecurseDir.Checked = App.Config.isRecurseSearchDir;
            //MenuItem_OptionSidebarFix.Checked = App.Config.isFixSidebar;
            Menu_keepMagnification.Checked = App.Config.keepMagnification;
            Menu_UseBicubic.Checked = !App.Config.isDotByDotZoom;
            Menu_DontEnlargeOver100percent.Checked = App.Config.noEnlargeOver100p;

            //ファイルを閲覧していない場合のナビゲーション
            if (App.g_pi.Items == null || App.g_pi.Items.Count <= 1)
            {
                Menu_OptionReloadThumbnail.Enabled = false;
            }
            else
            {
                Menu_OptionReloadThumbnail.Enabled = true;
            }

            //ver1.83アンシャープ
            MenuItem_Unsharp.Checked = App.Config.useUnsharpMask;
        }

        private void Menu_Help_DropDownOpening(object sender, EventArgs e)
        {
            MenuItem_CheckSusie.Checked = App.susie.isSupportedExtentions("pdf");
            MenuItem_CheckUnrar.Checked = App.unrar.dllLoaded;
        }

        private void Menu_Page_DropDownOpening(object sender, EventArgs e)
        {
            //左開きにする
            Menu_View_LeftOpen.Checked = !App.g_pi.PageDirectionIsLeft;
            //ファイルを閲覧していない場合のナビゲーション
            if (App.g_pi == null || App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //ファイルがない場合
                Menu_ViewTop.Enabled = false;
                Menu_ViewEnd.Enabled = false;
                Menu_ViewBack.Enabled = false;
                Menu_ViewNext.Enabled = false;          //次へ
                Menu_ViewThumbnail.Enabled = false;     //サムネイル
                Menu_ViewHalfPageBack.Enabled = false;  //半ページ
                Menu_ViewHalfPageForword.Enabled = false;//半ページ
                Menu_ViewFitScreenSize.Enabled = false; //フルスクリーン
                Menu_ViewPictureInfo.Enabled = false;   //画像情報
                                                        //Menu_ViewAddBookmark.Checked = false;	//しおり
                                                        //Menu_ViewAddBookmark.Enabled = false;	//しおり
                Menu_ViewZoom.Enabled = false;  //Zoom
                Menu_ViewReload.Enabled = false;
                Menu_SlideShow.Enabled = false;
                return;
            }
            else if (App.g_pi.Items.Count == 1)
            {
                //ファイルが１枚
                Menu_ViewTop.Enabled = false;
                Menu_ViewEnd.Enabled = false;
                Menu_ViewBack.Enabled = false;
                Menu_ViewNext.Enabled = false;
                Menu_ViewHalfPageBack.Enabled = false;
                Menu_ViewHalfPageForword.Enabled = false;
                Menu_ViewThumbnail.Enabled = true;
                Menu_ViewFitScreenSize.Enabled = true;
                Menu_ViewPictureInfo.Enabled = true;
                //Menu_ViewAddBookmark.Checked = false;	//しおり
                //Menu_ViewAddBookmark.Enabled = false;	//しおり
                Menu_ViewZoom.Enabled = true;   //Zoom
                Menu_ViewReload.Enabled = true;
                Menu_SlideShow.Enabled = false;
                return;
            }
            else
            {
                //複数枚表示
                Menu_ViewFitScreenSize.Enabled = true;
                Menu_ViewPictureInfo.Enabled = true;
                Menu_ViewZoom.Enabled = true;   //Zoom
                Menu_ViewReload.Enabled = true;

                //スライドショー
                Menu_SlideShow.Enabled = true;

                //サムネイルボタン
                Menu_ViewThumbnail.Checked = App.Config.isThumbnailView;

                //2ページモード:半ページ送りは2ページモード時のみ
                Menu_ViewHalfPageBack.Enabled = App.Config.dualView && (bool)(App.g_pi.NowViewPage != 0); //先頭ページチェック
                Menu_ViewHalfPageForword.Enabled = App.Config.dualView && !IsLastPageViewing();       //最終ページチェック

                //しおり機能
                //ver1.79コメントアウト
                //Menu_ViewAddBookmark.Enabled = true;	//しおり
                //Menu_ViewAddBookmark.Checked =
                //	g_pi.Items[g_pi.NowViewPage].isBookMark;

                //サムネイル表示中
                if (App.Config.isThumbnailView)
                {
                    //サムネイル中は左右はDisable
                    Menu_ViewTop.Enabled = false;
                    Menu_ViewBack.Enabled = false;
                    Menu_ViewEnd.Enabled = false;
                    Menu_ViewNext.Enabled = false;
                    Menu_View2Page.Enabled = false;
                    Menu_ViewNavibar.Enabled = false;
                }
                else
                {
                    //ナビゲーションメニューの有効無効
                    Menu_ViewTop.Enabled = (bool)(App.g_pi.NowViewPage != 0);   //先頭ページチェック
                    Menu_ViewBack.Enabled = (bool)(App.g_pi.NowViewPage != 0);  //先頭ページチェック
                    Menu_ViewEnd.Enabled = !IsLastPageViewing();        //最終ページチェック
                    Menu_ViewNext.Enabled = !IsLastPageViewing();       //最終ページチェック
                    Menu_View2Page.Enabled = true;
                    Menu_ViewNavibar.Enabled = true;
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //バー関連のメニュー
            Menu_ContextToolbar.Enabled = !App.Config.isFullScreen;
            Menu_ContextStatusbar.Enabled = !App.Config.isFullScreen;

            Menu_ContextMenubar.Checked = App.Config.visibleMenubar;
            Menu_ContextToolbar.Checked = App.Config.visibleToolBar;
            Menu_ContextStatusbar.Checked = App.Config.visibleStatusBar;

            Menu_ContextDualView.Checked = App.Config.dualView;
            Menu_ContextFullView.Checked = App.Config.isFullScreen;
            Menu_ContextFitScreenSize.Checked = App.Config.isFitScreenAndImage;

            Menu_ContextNavibar.Checked = g_Sidebar.Visible;

            //ファイルを閲覧していない場合のナビゲーション
            if (App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //ファイルを閲覧していない場合のナビゲーション
                Menu_ContextTop.Enabled = false;
                Menu_ContextBack.Enabled = false;
                Menu_ContextNext.Enabled = false;
                Menu_ContextLast.Enabled = false;
                Menu_ContextThumbnailView.Enabled = false;
                Menu_ContextHalfPageBack.Enabled = false;
                Menu_ContextHalfPageForword.Enabled = false;
                Menu_ContextFitScreenSize.Enabled = false;
                Menu_ContextPictureInfo.Enabled = false;
                Menu_ContextAddBookmark.Enabled = false;
                Menu_ContextZoom.Enabled = false;   //zoom
                Menu_ContextRedraw.Enabled = false;
                return;
            }
            else if (App.g_pi.Items.Count == 1)
            {
                //ファイルを閲覧していない場合のナビゲーション
                Menu_ContextTop.Enabled = false;
                Menu_ContextBack.Enabled = false;
                Menu_ContextNext.Enabled = false;
                Menu_ContextLast.Enabled = false;
                Menu_ContextHalfPageBack.Enabled = false;
                Menu_ContextHalfPageForword.Enabled = false;

                Menu_ContextThumbnailView.Enabled = true;
                Menu_ContextFitScreenSize.Enabled = true;
                Menu_ContextPictureInfo.Enabled = true;
                Menu_ContextAddBookmark.Enabled = false;
                Menu_ContextZoom.Enabled = true;    //zoom
                Menu_ContextRedraw.Enabled = true;
                return;
            }
            else
            {
                Menu_ContextFitScreenSize.Enabled = true;
                Menu_ContextPictureInfo.Enabled = true;
                Menu_ContextNavibar.Enabled = true;
                Menu_ContextZoom.Enabled = true;    //zoom
                Menu_ContextRedraw.Enabled = true;

                //しおり機能
                Menu_ContextAddBookmark.Enabled = true;
                if (App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark)
                    Menu_ContextAddBookmark.Checked = true;
                else
                    Menu_ContextAddBookmark.Checked = false;
                AddBookmarkMenuItem(Menu_ContextBookmarkList);

                //サムネイルボタン
                //MenuItem_ContextThumbnailView.Enabled = g_makeThumbnail;	//サムネイルを作っているかどうか
                Menu_ContextThumbnailView.Checked = App.Config.isThumbnailView;

                //2ページモード:半ページ送りは2ページモード時のみ
                Menu_ContextHalfPageBack.Enabled = App.Config.dualView && (bool)(App.g_pi.NowViewPage != 0);  //先頭ページチェック
                Menu_ContextHalfPageForword.Enabled = App.Config.dualView && !IsLastPageViewing();    //最終ページチェック

                //サムネイル表示中
                if (App.Config.isThumbnailView)
                {
                    //サムネイル中はContext関係なし
                }
                else
                {
                    //ナビゲーションメニューの有効無効
                    Menu_ContextTop.Enabled = (bool)(App.g_pi.NowViewPage != 0);        //先頭ページチェック
                    Menu_ContextBack.Enabled = (bool)(App.g_pi.NowViewPage != 0);   //先頭ページチェック
                    Menu_ContextNext.Enabled = !IsLastPageViewing();        //最終ページチェック
                    Menu_ContextLast.Enabled = !IsLastPageViewing();        //最終ページチェック
                                                                            //MenuItem_ContextLast.Enabled = (bool)(g_pi.ViewPage != g_pi.Items.Count - 1);
                }
            }
        }

        private void Menu_Bookmark_DropDownOpening(object sender, EventArgs e)
        {
            //メニューを一度全部クリアして初期メニューを作る
            Menu_Bookmark.DropDownItems.Clear();
            this.Menu_Bookmark.DropDownItems.AddRange(new ToolStripItem[] {
                this.Bookmark_Add,
                this.BookMark_Clear,
                this.toolStripSeparator11});

            //メニューを初期化
            if (App.g_pi.Items.Count == 0)
            {
                Bookmark_Add.Enabled = false;
                BookMark_Clear.Enabled = false;
                return;
            }
            else
            {
                Bookmark_Add.Enabled = true;
                BookMark_Clear.Enabled = true;
            }

            //ver1.79 イメージを追加する
            int count = 0;
            foreach (ImageInfo ii in App.g_pi.Items)
            {
                if (ii.IsBookMark)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    item.Text = Path.GetFileName(ii.Filename);
                    item.Image = BitmapUty.MakeSquareThumbnailImage(ii.Thumbnail, 40);
                    item.ImageScaling = ToolStripItemImageScaling.None;
                    int ix = count;
                    item.Tag = count;
                    //item.Click += (s, ex) => { SetViewPage((int)((s as ToolStripMenuItem).Tag)); };
                    item.Click += (s, ex) => { SetViewPage(ix); };
                    Menu_Bookmark.DropDownItems.Add(item);
                }
                count++;
            }
        }

        // ブックマーク関連 *************************************************************************

        /// <summary>
        /// メニューにBookmarkを追加する。
        /// コンテキストメニューの追加に使用されている(ver1.79)
        /// </summary>
        /// <param name="BookmarkMenu"></param>
        private void AddBookmarkMenuItem(ToolStripMenuItem BookmarkMenu)
        {
            if (App.g_pi.Items.Count == 0)
            {
                BookmarkMenu.Enabled = false;
                return;
            }

            int count = 0;
            BookmarkMenu.DropDownItems.Clear();

            foreach (ImageInfo ii in App.g_pi.Items)
            {
                if (ii.IsBookMark)
                {
                    count++;
                    BookmarkMenu.DropDownItems.Add(
                        ii.Filename,    //ファイル名
                        null,
                        new System.EventHandler(OnBookmarkList) //イベント
                        );
                }
            }
            if (count == 0)
                BookmarkMenu.Enabled = false;
            else
                BookmarkMenu.Enabled = true;
            return;
        }

        /// <summary>
        /// しおりの一覧から選択されたときに呼ばれる
        /// </summary>
        private void OnBookmarkList(object sender, EventArgs e)
        {
            var tsddi = (ToolStripDropDownItem)sender;
            int index = App.g_pi.GetIndexFromFilename(tsddi.Text);
            if (index >= 0)
                SetViewPage(index);
        }

        private void BookMark_Clear_Click(object sender, EventArgs e)
        {
            int len = App.g_pi.Items.Count;
            for (int i = 0; i < len; i++)
            {
                App.g_pi.Items[i].IsBookMark = false;
            }
        }

        // そのほか *********************************************************************************

        private void ReloadPage()
        {
            if (App.g_pi == null || App.g_pi.Items.Count == 0)
                return;

            //ver0.972 ナビバーがあればリサイズ
            AjustSidebarArrangement();

            if (App.Config.isThumbnailView)
                g_ThumbPanel.ReDraw();
            else
                SetViewPage(App.g_pi.NowViewPage);
        }

        private void Menu_Slideshow_Click(object sender, EventArgs e)
        {
            if (App.g_pi.Items.Count == 0)
                return;

            if (SlideShowTimer.Enabled)
            {
                StopSlideShow();
            }
            else
            {
                g_ClearPanel.ShowAndClose(
                    "スライドショーを開始します。\r\nマウスクリックまたはキー入力で終了します。",
                    1500);

                //タイマー設定
                int msec = 0;
                string s = (sender as ToolStripItem).Tag as string;
                if (int.TryParse(s, out msec) && msec != 0)
                    SlideShowTimer.Interval = msec;
                else
                    SlideShowTimer.Interval = App.Config.slideShowTime;

                //タイマー開始
                SlideShowTimer.Start();
            }
        }
    }
}
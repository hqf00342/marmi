using System;
using System.ComponentModel;			//EventArgs
using System.IO;                        //Directory, File
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        // メニューイベント ************************************************************************

        private async void OnClickMRUMenu(object sender, EventArgs e)
        {
            var filename = ((ToolStripDropDownItem)sender).Text;

            if (File.Exists(filename) || Directory.Exists(filename))
            {
                await StartAsync(new string[] { filename });
            }
            else
            {
                MessageBox.Show($"ファイルが見つかりませんでした\n{filename}", "ファイルオープンエラー");

                //MRUリストから削除
                var target = App.Config.Mru.Find(a => a.Name == filename);
                if (target != null)
                {
                    App.Config.Mru.Remove(target);
                }
            }
        }

        // #### FILE ###############################################################################

        #region FILE

        private async void Menu_FileOpen_Click(object sender, EventArgs e)
        {
            await OpenDialog();
        }

        private void Menu_SaveThumbnail_Click(object sender, EventArgs e)
        {
            _thumbPanel.Location = GetClientRectangle().Location;
            _thumbPanel.Size = GetClientRectangle().Size;
            _thumbPanel.Parent = this;
            var form = new SaveThumbnailForm(App.g_pi.Items, App.g_pi.PackageName);
            form.ShowDialog(this);
        }

        private void Menu_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void Menu_ClearMRU_Click(object sender, EventArgs e)
        {
            App.Config.Mru.Clear();
            //for (int i = 0; i < App.Config.mru.Length; i++)
            //{
            //    App.Config.mru[i] = null;
            //}
        }

        private void Menu_File_DropDownOpening(object sender, EventArgs e)
        {
            //MRUを追加
            UpdateMruMenuListUI();

            //ファイルを閲覧していない場合,サムネイル保存を無効にする
            MenuItem_FileSaveThumbnail.Enabled = (App.g_pi?.Items?.Count > 0);
        }

        /// <summary>
        /// MRUリストを更新する。実際にメニューの中身を更新
        /// この関数を呼び出しているのはMenu_File_DropDownOpeningのみ
        /// </summary>
        private void UpdateMruMenuListUI()
        {
            MenuItem_FileRecent.DropDownItems.Clear();

            //Array.Sort(App.Config.mru);
            App.Config.Mru = App.Config.Mru.OrderBy(a => a.Date).ToList();

            int menuCount = 0;

            //新しい順にする
            for (int i = App.Config.Mru.Count - 1; i >= 0; i--)
            {
                if (App.Config.Mru[i] == null)
                    continue;

                MenuItem_FileRecent.DropDownItems.Add(App.Config.Mru[i].Name, null, new EventHandler(OnClickMRUMenu));

                //ver1.73 MRU表示数の制限
                if (++menuCount >= App.Config.General.MaxMruNumber)
                    break;
            }
        }

        #endregion FILE

        //表示メニュー******************************************************************************

        private async void Menu_ViewNext_Click(object sender, EventArgs e)
        {
            await NavigateToForwordAsync();
        }

        private async void Menu_ViewBack_Click(object sender, EventArgs e)
        {
            await NavigateToBackAsync();
        }

        private async void Menu_ViewTop_Click(object sender, EventArgs e)
        {
            await SetViewPageAsync(0);
        }

        private async void Menu_ViewEnd_Click(object sender, EventArgs e)
        {
            await SetViewPageAsync(App.g_pi.Items.Count - 1);
        }

        private void Menu_ViewThumbnail_Click(object sender, EventArgs e)
        {
            if (App.g_pi.Items.Count > 0)
            {
                //全画面であればメニュー系は全て非表示
                if (ViewState.FullScreen)
                {
                    toolStrip1.Visible = false;
                    statusbar.Visible = false;
                    menuStrip1.Visible = false;
                }
                SetThumbnailView(!ViewState.ThumbnailView);
            }
        }

        private void Menu_ViewFullScreen_Click(object sender, EventArgs e)
        {
            SetFullScreen(!ViewState.FullScreen);
        }

        private async void Menu_ViewMenubar_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            ViewState.VisibleMenubar = !ViewState.VisibleMenubar;
            menuStrip1.Visible = ViewState.VisibleMenubar;

            //ver0.972 サイドバーの位置調整
            //AjustControlArrangement();
            //再描写
            await ReloadPageAsync();
        }

        private async void Menu_ViewToolbar_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            ViewState.VisibleToolBar = !ViewState.VisibleToolBar;
            toolStrip1.Visible = ViewState.VisibleToolBar;

            //ver0.972 ナビバーがあればリサイズ
            //AjustControlArrangement();
            //再描写
            await ReloadPageAsync();
        }

        private async void Menu_ViewStatusbar_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            ViewState.VisibleStatusBar = !ViewState.VisibleStatusBar;
            statusbar.Visible = ViewState.VisibleStatusBar;

            //ver0.972 サイドバーの位置調整
            //AjustControlArrangement();
            //再描写
            await ReloadPageAsync();
        }

        private async void Menu_ViewDualPage_Click(object sender, EventArgs e)
        {
            //トグル切り替え
            await SetDualViewModeAsync(!ViewState.DualView);
        }

        private async void Menu_ViewHalfPageBack_Click(object sender, EventArgs e)
        {
            if (ViewState.DualView)
            {
                if (App.g_pi.NowViewPage > 0)
                {
                    await SetViewPageAsync(--App.g_pi.NowViewPage);
                }
                else
                {
                    //先頭ページだったので何もしない。
                }
            }
        }

        private async void Menu_ViewHalfPageForword_Click(object sender, EventArgs e)
        {
            if (ViewState.DualView)
            {
                if (App.g_pi.NowViewPage < App.g_pi.Items.Count)
                {
                    App.g_pi.NowViewPage++;  //半ページ戻し
                    await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010年6月20日
                }
                else
                {
                    // 最終ページなので何もしない
                }
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
            if (App.g_pi.Items.Count == 0)
                return;

            FormPackageInfo pif = new FormPackageInfo(this, App.g_pi);
            pif.SetSortMode(false);
            //pif.Show(g_pi.ViewPage);
            pif.ShowDialog(App.g_pi.NowViewPage);
        }

        private void Menu_ViewFitScreenSize_Click(object sender, EventArgs e)
        {
            ToggleFitScreen();
        }

        private void ToggleFitScreen()
        {
            App.Config.FitToScreen = !App.Config.FitToScreen;
            PicPanel.IsAutoFit = App.Config.FitToScreen;

            PicPanel.Refresh();
            UpdateStatusbar();
        }

        private void Menu_ViewSidebar_Click(object sender, EventArgs e)
        {
            if (_sidebar.Visible)
            {
                //閉じる
                _sidebar.Visible = false;
                ViewState.VisibleSidebar = false;
            }
            else
            {
                //サイドバーオープン
                _sidebar.Init(App.g_pi);
                if (App.Config != null)
                    _sidebar.Width = App.Config.SidebarWidth;
                else
                    _sidebar.Width = App.SIDEBAR_DEFAULT_WIDTH;

                _sidebar.Visible = true;
                _sidebar.SetItemToCenter(App.g_pi.NowViewPage);
                ViewState.VisibleSidebar = true;
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

        private async void Menu_View_LeftOpen_Click(object sender, EventArgs e)
        {
            App.g_pi.PageDirectionIsLeft = !App.g_pi.PageDirectionIsLeft;
            if (ViewState.DualView)
            {
                await SetViewPageAsync(App.g_pi.NowViewPage);
            }
        }

        private void Menu_ToolbarBottom_Click(object sender, EventArgs e)
        {
            if (toolStrip1.Dock == DockStyle.Bottom)
                toolStrip1.Dock = DockStyle.Top;
            else
                toolStrip1.Dock = DockStyle.Bottom;
        }

        private async void Menu_Reload_Click(object sender, EventArgs e)
        {
            await ReloadPageAsync();
        }

        //ヘルプメニュー*****************************************************************************

        private void MenuItem_HelpVersion_Click(object sender, EventArgs e)
        {
            var vf = new VersionForm
            {
                StartPosition = FormStartPosition.CenterParent
            };
            vf.ShowDialog();
        }

        //オプションメニュー*************************************************************************

        private void Menu_Option_Click(object sender, EventArgs e)
        {
            var fo = new OptionForm();
            fo.LoadConfig(App.Config);
            if (fo.ShowDialog() == DialogResult.OK)
            {
                //App.Configに取り込み
                fo.SaveConfig(ref App.Config);
                //App.Configをウィンドウに反映
                ApplyConfigToWindow();
            }
        }

        private void Menu_RecurseDir_Click(object sender, EventArgs e)
        {
            App.Config.RecurseSearchDir = !App.Config.RecurseSearchDir;
            Menu_OptionRecurseDir.Checked = App.Config.RecurseSearchDir;
        }

        private void Menu_keepMagnification_Click(object sender, EventArgs e)
        {
            App.Config.KeepMagnification = !App.Config.KeepMagnification;
        }

        private void Menu_UseBicubic_Click(object sender, EventArgs e)
        {
            App.Config.View.DotByDotZoom = !App.Config.View.DotByDotZoom;
        }

        private async void Menu_DontEnlargeOver100percent_Click(object sender, EventArgs e)
        {
            App.Config.View.ProhigitExpansionOver100p = !App.Config.View.ProhigitExpansionOver100p;
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }

        // コンテキストメニュー  ********************************************************************

        private void Menu_ContextBookmark_Click(object sender, EventArgs e)
        {
            ToggleBookmark();
        }

        //ソートメニュー*****************************************************************************

        private async void Menu_SortByName_Click(object sender, EventArgs e)
        {
            //ファイルリストを並び替える
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
                App.g_pi.Items.Sort(comparer);

                //サムネイル表示中であれば再描写させる
                if (ViewState.ThumbnailView)
                {
                    _thumbPanel.ReDraw();
                }

                //ver1.38 ソート後に画面を書き直す
                ScreenCache.Clear();
                await SetViewPageAsync(App.g_pi.NowViewPage);
            }
        }

        private async void Menu_SortByDate_Click(object sender, EventArgs e)
        {
            //ファイルリストを並び替える
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.CreateDate);
                App.g_pi.Items.Sort(comparer);

                //サムネイル表示中であれば再描写させる
                if (ViewState.ThumbnailView)
                {
                    _thumbPanel.ReDraw();
                }

                //ver1.38 ソート後に画面を書き直す
                ScreenCache.Clear();
                await SetViewPageAsync(App.g_pi.NowViewPage);
            }
        }

        private async void Menu_SortCustom_Click(object sender, EventArgs e)
        {
            FormPackageInfo pif = new FormPackageInfo(this, App.g_pi);
            pif.SetSortMode(true);
            pif.ShowDialog(App.g_pi.NowViewPage);

            //ver1.38 ソート後に画面を書き直す
            ScreenCache.Clear();
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }

        // メニューホバーイベント *******************************************************************

        private void Menu_MouseHover(object sender, EventArgs e)
        {
            //1クリック対応用に保持しておく
            _hoverStripItem = sender;
        }

        private void Menu_MouseLeave(object sender, EventArgs e)
        {
            _hoverStripItem = null;
        }

        private void MenuStrip1_MenuDeactivate(object sender, EventArgs e)
        {
            //全画面モードでフォーカスを失ったときは隠す
            if (ViewState.FullScreen)
                menuStrip1.Visible = false;
        }

        // メニューオープニングイベント *************************************************************

        private void Menu_View_DropDownOpening(object sender, EventArgs e)
        {
            //バー関連のメニュー
            Menu_ViewToolbar.Enabled = !ViewState.FullScreen;
            Menu_ViewStatusbar.Enabled = !ViewState.FullScreen;
            Menu_ViewMenubar.Checked = menuStrip1.Visible;
            Menu_ViewToolbar.Checked = toolStrip1.Visible;
            Menu_ViewStatusbar.Checked = statusbar.Visible;
            Menu_View2Page.Checked = ViewState.DualView;
            Menu_ViewFullScreen.Checked = ViewState.FullScreen;
            Menu_ViewFitScreenSize.Checked = App.Config.FitToScreen;
            Menu_ViewSidebar.Checked = _sidebar.Visible;
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
            Menu_OptionRecurseDir.Checked = App.Config.RecurseSearchDir;
            //MenuItem_OptionSidebarFix.Checked = App.Config.isFixSidebar;
            Menu_keepMagnification.Checked = App.Config.KeepMagnification;
            Menu_UseBicubic.Checked = !App.Config.View.DotByDotZoom;
            Menu_DontEnlargeOver100percent.Checked = App.Config.View.ProhigitExpansionOver100p;

            //ver1.83アンシャープ
            MenuItem_Unsharp.Checked = App.Config.Advance.UnsharpMask;
        }

        private void Menu_Help_DropDownOpening(object sender, EventArgs e)
        {
            MenuItem_CheckSusie.Checked = App.susie.isSupportedExtentions("pdf");
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

                //2ページモード:半ページ送りは2ページモード時のみ
                Menu_ViewHalfPageBack.Enabled = ViewState.DualView && (bool)(App.g_pi.NowViewPage != 0); //先頭ページチェック
                Menu_ViewHalfPageForword.Enabled = ViewState.DualView && !IsLastPageViewing();       //最終ページチェック

                //しおり機能
                //ver1.79コメントアウト
                //Menu_ViewAddBookmark.Enabled = true;	//しおり
                //Menu_ViewAddBookmark.Checked =
                //	g_pi.Items[g_pi.NowViewPage].isBookMark;

                //サムネイル表示中
                if (ViewState.ThumbnailView)
                {
                    //サムネイル中は左右はDisable
                    Menu_ViewTop.Enabled = false;
                    Menu_ViewBack.Enabled = false;
                    Menu_ViewEnd.Enabled = false;
                    Menu_ViewNext.Enabled = false;
                    Menu_View2Page.Enabled = false;
                    Menu_ViewSidebar.Enabled = false;
                }
                else
                {
                    //ナビゲーションメニューの有効無効
                    Menu_ViewTop.Enabled = (bool)(App.g_pi.NowViewPage != 0);   //先頭ページチェック
                    Menu_ViewBack.Enabled = (bool)(App.g_pi.NowViewPage != 0);  //先頭ページチェック
                    Menu_ViewEnd.Enabled = !IsLastPageViewing();        //最終ページチェック
                    Menu_ViewNext.Enabled = !IsLastPageViewing();       //最終ページチェック
                    Menu_View2Page.Enabled = true;
                    Menu_ViewSidebar.Enabled = true;
                }
            }
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //バー関連のメニュー
            Menu_ContextToolbar.Enabled = !ViewState.FullScreen;
            Menu_ContextStatusbar.Enabled = !ViewState.FullScreen;

            Menu_ContextMenubar.Checked = ViewState.VisibleMenubar;
            Menu_ContextToolbar.Checked = ViewState.VisibleToolBar;
            Menu_ContextStatusbar.Checked = ViewState.VisibleStatusBar;

            Menu_ContextDualView.Checked = ViewState.DualView;
            Menu_ContextFullView.Checked = ViewState.FullScreen;
            Menu_ContextFitScreenSize.Checked = App.Config.FitToScreen;

            Menu_ContextSidebar.Checked = _sidebar.Visible;

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
                Menu_ContextSidebar.Enabled = true;
                Menu_ContextZoom.Enabled = true;    //zoom
                Menu_ContextRedraw.Enabled = true;

                //しおり機能
                Menu_ContextAddBookmark.Enabled = true;
                Menu_ContextAddBookmark.Checked = App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark;
                AddBookmarkMenuItem(Menu_ContextBookmarkList);

                //サムネイルボタン
                //MenuItem_ContextThumbnailView.Enabled = g_makeThumbnail;	//サムネイルを作っているかどうか
                Menu_ContextThumbnailView.Checked = ViewState.ThumbnailView;

                //2ページモード:半ページ送りは2ページモード時のみ
                Menu_ContextHalfPageBack.Enabled = ViewState.DualView && (bool)(App.g_pi.NowViewPage != 0);  //先頭ページチェック
                Menu_ContextHalfPageForword.Enabled = ViewState.DualView && !IsLastPageViewing();    //最終ページチェック

                //サムネイル表示中
                if (ViewState.ThumbnailView)
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
                    ToolStripMenuItem item = new ToolStripMenuItem
                    {
                        Text = Path.GetFileName(ii.Filename),
                        Image = BitmapUty.MakeSquareThumbnailImage(ii.Thumbnail, 40),
                        ImageScaling = ToolStripItemImageScaling.None
                    };
                    int ix = count;
                    item.Tag = count;
                    //item.Click += (s, ex) => { SetViewPage((int)((s as ToolStripMenuItem).Tag)); };
                    item.Click += async (s, ex) => { await SetViewPageAsync(ix); };
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
            BookmarkMenu.Enabled = count != 0;
            return;
        }

        /// <summary>
        /// しおりの一覧から選択されたときに呼ばれる
        /// </summary>
        private async void OnBookmarkList(object sender, EventArgs e)
        {
            var tsddi = (ToolStripDropDownItem)sender;
            int index = App.g_pi.GetIndexFromFilename(tsddi.Text);
            if (index >= 0)
                await SetViewPageAsync(index);
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

        private async Task ReloadPageAsync()
        {
            if (App.g_pi == null || App.g_pi.Items.Count == 0)
                return;

            //ver0.972 サイドバーがあればリサイズ
            AjustSidebarArrangement();

            if (ViewState.ThumbnailView)
                _thumbPanel.ReDraw();
            else
                await SetViewPageAsync(App.g_pi.NowViewPage);
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
                _clearPanel.ShowAndClose(
                    "スライドショーを開始します。\r\nマウスクリックまたはキー入力で終了します。",
                    1500);

                //タイマー設定
                string s = (sender as ToolStripItem).Tag as string;
                if (int.TryParse(s, out int msec) && msec != 0)
                    SlideShowTimer.Interval = msec;
                else
                    SlideShowTimer.Interval = App.Config.SlideshowTime;

                //タイマー開始
                SlideShowTimer.Start();
            }
        }

        private void Menu_Unsharp_Click(object sender, EventArgs e)
        {
            App.Config.Advance.UnsharpMask = !App.Config.Advance.UnsharpMask;
            MenuItem_Unsharp.Checked = App.Config.Advance.UnsharpMask;

            //再描写
            PicPanel.Invalidate();
        }
    }
}
using System;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
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

        private async void Menu_Reload_Click(object sender, EventArgs e)
        {
            await ReloadPageAsync();
        }
    }
}
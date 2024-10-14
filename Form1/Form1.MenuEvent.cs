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
        // ���j���[�C�x���g ************************************************************************

        // #### FILE ###############################################################################

        #region FILE

        /// <summary>
        /// MRU���X�g���X�V����B���ۂɃ��j���[�̒��g���X�V
        /// ���̊֐����Ăяo���Ă���̂�Menu_File_DropDownOpening�̂�
        /// </summary>
        private void UpdateMruMenuListUI()
        {
            MenuItem_FileRecent.DropDownItems.Clear();

            //Array.Sort(App.Config.mru);
            App.Config.Mru = App.Config.Mru.OrderBy(a => a.Date).ToList();

            int menuCount = 0;

            //�V�������ɂ���
            for (int i = App.Config.Mru.Count - 1; i >= 0; i--)
            {
                if (App.Config.Mru[i] == null)
                    continue;

                MenuItem_FileRecent.DropDownItems.Add(App.Config.Mru[i].Name, null, new EventHandler(OnClickMRUMenu));

                //ver1.73 MRU�\�����̐���
                if (++menuCount >= App.Config.General.MaxMruNumber)
                    break;
            }
        }

        #endregion FILE

        //�\�����j���[******************************************************************************

        private void ToggleFitScreen()
        {
            App.Config.FitToScreen = !App.Config.FitToScreen;
            PicPanel.IsAutoFit = App.Config.FitToScreen;

            PicPanel.Refresh();
            UpdateStatusbar();
        }

        private void Menu_ToolbarBottom_Click(object sender, EventArgs e)
        {
            if (toolStrip1.Dock == DockStyle.Bottom)
                toolStrip1.Dock = DockStyle.Top;
            else
                toolStrip1.Dock = DockStyle.Bottom;
        }

        //�w���v���j���[*****************************************************************************

        private void MenuItem_HelpVersion_Click(object sender, EventArgs e)
        {
            var vf = new VersionForm
            {
                StartPosition = FormStartPosition.CenterParent
            };
            vf.ShowDialog();
        }

        //�I�v�V�������j���[*************************************************************************

        // �R���e�L�X�g���j���[  ********************************************************************

        private void Menu_ContextBookmark_Click(object sender, EventArgs e)
        {
            ToggleBookmark();
        }

        //�\�[�g���j���[*****************************************************************************

        private async void Menu_SortByName_Click(object sender, EventArgs e)
        {
            //�t�@�C�����X�g����ёւ���
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
                App.g_pi.Items.Sort(comparer);

                //�T���l�C���\�����ł���΍ĕ`�ʂ�����
                if (ViewState.ThumbnailView)
                {
                    _thumbPanel.ReDraw();
                }

                //ver1.38 �\�[�g��ɉ�ʂ���������
                ScreenCache.Clear();
                await SetViewPageAsync(App.g_pi.NowViewPage);
            }
        }

        private async void Menu_SortByDate_Click(object sender, EventArgs e)
        {
            //�t�@�C�����X�g����ёւ���
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.CreateDate);
                App.g_pi.Items.Sort(comparer);

                //�T���l�C���\�����ł���΍ĕ`�ʂ�����
                if (ViewState.ThumbnailView)
                {
                    _thumbPanel.ReDraw();
                }

                //ver1.38 �\�[�g��ɉ�ʂ���������
                ScreenCache.Clear();
                await SetViewPageAsync(App.g_pi.NowViewPage);
            }
        }

        private async void Menu_SortCustom_Click(object sender, EventArgs e)
        {
            FormPackageInfo pif = new FormPackageInfo(this, App.g_pi);
            pif.SetSortMode(true);
            pif.ShowDialog(App.g_pi.NowViewPage);

            //ver1.38 �\�[�g��ɉ�ʂ���������
            ScreenCache.Clear();
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }

        // ���j���[�z�o�[�C�x���g *******************************************************************

        private void Menu_MouseHover(object sender, EventArgs e)
        {
            //1�N���b�N�Ή��p�ɕێ����Ă���
            _hoverStripItem = sender;
        }

        private void Menu_MouseLeave(object sender, EventArgs e)
        {
            _hoverStripItem = null;
        }

        private void MenuStrip1_MenuDeactivate(object sender, EventArgs e)
        {
            //�S��ʃ��[�h�Ńt�H�[�J�X���������Ƃ��͉B��
            if (ViewState.FullScreen)
                menuStrip1.Visible = false;
        }

        // ���j���[�I�[�v�j���O�C�x���g *************************************************************

        private void Menu_View_DropDownOpening(object sender, EventArgs e)
        {
            //�o�[�֘A�̃��j���[
            Menu_ViewToolbar.Enabled = !ViewState.FullScreen;
            Menu_ViewStatusbar.Enabled = !ViewState.FullScreen;
            Menu_ViewMenubar.Checked = menuStrip1.Visible;
            Menu_ViewToolbar.Checked = toolStrip1.Visible;
            Menu_ViewStatusbar.Checked = statusbar.Visible;
            Menu_View2Page.Checked = ViewState.DualView;
            Menu_ViewFullScreen.Checked = ViewState.FullScreen;
            Menu_ViewFitScreenSize.Checked = App.Config.FitToScreen;
            Menu_ViewSidebar.Checked = _sidebar.Visible;
            //�c�[���o�[�̈ʒu
            Menu_ToolbarBottom.Checked = (toolStrip1.Dock == DockStyle.Bottom);
            //�T�C�h�o�[�֘A
            //MenuItem_ViewFixSidebar.Checked = App.Config.isFixSidebar;
            //������֘A�@�\
            //ver1.79�R�����g�A�E�g
            //AddBookmarkMenuItem(MenuItem_ViewBookmarkList);

            //ver1.81 �p�b�P�[�W�Ȃ��̂Ƃ��̑Ώ�
            bool isPackageOpen = (App.g_pi.Items.Count > 0);
            Menu_ViewPictureInfo.Enabled = isPackageOpen;
            Menu_ViewPackageInfo.Enabled = isPackageOpen;
        }

        private void Menu_Option_DropDownOpening(object sender, EventArgs e)
        {
            //�`�F�b�N���
            Menu_OptionRecurseDir.Checked = App.Config.RecurseSearchDir;
            //MenuItem_OptionSidebarFix.Checked = App.Config.isFixSidebar;
            Menu_keepMagnification.Checked = App.Config.View.KeepMagnification;
            Menu_UseBicubic.Checked = !App.Config.View.DotByDotZoom;
            Menu_DontEnlargeOver100percent.Checked = App.Config.View.ProhigitExpansionOver100p;

            //ver1.83�A���V���[�v
            MenuItem_Unsharp.Checked = App.Config.Advance.UnsharpMask;
        }

        private void Menu_Help_DropDownOpening(object sender, EventArgs e)
        {
            MenuItem_CheckSusie.Checked = App.susie.isSupportedExtentions("pdf");
        }

        private void Menu_Page_DropDownOpening(object sender, EventArgs e)
        {
            //���J���ɂ���
            Menu_View_LeftOpen.Checked = !App.g_pi.PageDirectionIsLeft;

            //�����y�[�W�\���������X�V
            UpDateMenuTextOfMultiPageNavigation();

            //�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
            if (App.g_pi == null || App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //�t�@�C�����Ȃ��ꍇ
                Menu_ViewTop.Enabled = false;
                Menu_ViewEnd.Enabled = false;
                Menu_ViewBack.Enabled = false;
                Menu_ViewNext.Enabled = false;          //����
                Menu_ViewHalfPageBack.Enabled = false;  //���y�[�W
                Menu_ViewHalfPageForword.Enabled = false;//���y�[�W
                Menu_ViewFitScreenSize.Enabled = false; //�t���X�N���[��
                Menu_ViewPictureInfo.Enabled = false;   //�摜���

                //Menu_ViewAddBookmark.Checked = false;	//������
                //Menu_ViewAddBookmark.Enabled = false;	//������
                Menu_ViewZoom.Enabled = false;  //Zoom
                Menu_ViewReload.Enabled = false;
                Menu_SlideShow.Enabled = false;
                Menu_ForwordMultiPages.Enabled = false;
                Menu_BackwordMultiPages.Enabled = false;
                return;
            }
            else if (App.g_pi.Items.Count == 1)
            {
                //�t�@�C�����P��
                Menu_ViewTop.Enabled = false;
                Menu_ViewEnd.Enabled = false;
                Menu_ViewBack.Enabled = false;
                Menu_ViewNext.Enabled = false;
                Menu_ViewHalfPageBack.Enabled = false;
                Menu_ViewHalfPageForword.Enabled = false;
                Menu_ViewFitScreenSize.Enabled = true;
                Menu_ViewPictureInfo.Enabled = true;
                //Menu_ViewAddBookmark.Checked = false;	//������
                //Menu_ViewAddBookmark.Enabled = false;	//������
                Menu_ViewZoom.Enabled = true;   //Zoom
                Menu_ViewReload.Enabled = true;
                Menu_SlideShow.Enabled = false;
                Menu_ForwordMultiPages.Enabled = false;
                Menu_BackwordMultiPages.Enabled = false;
                return;
            }
            else
            {
                //�������\��
                Menu_ViewFitScreenSize.Enabled = true;
                Menu_ViewPictureInfo.Enabled = true;
                Menu_ViewZoom.Enabled = true;   //Zoom
                Menu_ViewReload.Enabled = true;

                //�X���C�h�V���[
                Menu_SlideShow.Enabled = true;

                //2�y�[�W���[�h:���y�[�W�����2�y�[�W���[�h���̂�
                Menu_ViewHalfPageBack.Enabled = ViewState.DualView && (bool)(App.g_pi.NowViewPage != 0); //�擪�y�[�W�`�F�b�N
                Menu_ViewHalfPageForword.Enabled = ViewState.DualView && !IsLastPageViewing();       //�ŏI�y�[�W�`�F�b�N

                //������@�\
                //ver1.79�R�����g�A�E�g
                //Menu_ViewAddBookmark.Enabled = true;	//������
                //Menu_ViewAddBookmark.Checked =
                //	g_pi.Items[g_pi.NowViewPage].isBookMark;

                //�T���l�C���\����
                if (ViewState.ThumbnailView)
                {
                    //�T���l�C�����͍��E��Disable
                    Menu_ViewTop.Enabled = false;
                    Menu_ViewBack.Enabled = false;
                    Menu_ViewEnd.Enabled = false;
                    Menu_ViewNext.Enabled = false;
                    Menu_View2Page.Enabled = false;
                    Menu_ViewSidebar.Enabled = false;
                    Menu_ForwordMultiPages.Enabled = false;
                    Menu_BackwordMultiPages.Enabled = false;
                }
                else
                {
                    //�i�r�Q�[�V�������j���[�̗L������
                    Menu_ViewTop.Enabled = (bool)(App.g_pi.NowViewPage != 0);   //�擪�y�[�W�`�F�b�N
                    Menu_ViewBack.Enabled = (bool)(App.g_pi.NowViewPage != 0);  //�擪�y�[�W�`�F�b�N
                    Menu_ViewEnd.Enabled = !IsLastPageViewing();        //�ŏI�y�[�W�`�F�b�N
                    Menu_ViewNext.Enabled = !IsLastPageViewing();       //�ŏI�y�[�W�`�F�b�N
                    Menu_View2Page.Enabled = true;
                    Menu_ViewSidebar.Enabled = true;
                    Menu_ForwordMultiPages.Enabled = true;
                    Menu_BackwordMultiPages.Enabled = true;
                }
            }
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //�o�[�֘A�̃��j���[
            Menu_ContextToolbar.Enabled = !ViewState.FullScreen;
            Menu_ContextStatusbar.Enabled = !ViewState.FullScreen;

            Menu_ContextMenubar.Checked = ViewState.VisibleMenubar;
            Menu_ContextToolbar.Checked = ViewState.VisibleToolBar;
            Menu_ContextStatusbar.Checked = ViewState.VisibleStatusBar;

            Menu_ContextDualView.Checked = ViewState.DualView;
            Menu_ContextFullView.Checked = ViewState.FullScreen;
            Menu_ContextFitScreenSize.Checked = App.Config.FitToScreen;

            Menu_ContextSidebar.Checked = _sidebar.Visible;

            //�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
            if (App.g_pi.Items == null || App.g_pi.Items.Count < 1)
            {
                //�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
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
                //�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
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

                //������@�\
                Menu_ContextAddBookmark.Enabled = true;
                Menu_ContextAddBookmark.Checked = App.g_pi.Items[App.g_pi.NowViewPage].IsBookMark;
                AddBookmarkMenuItem(Menu_ContextBookmarkList);

                //�T���l�C���{�^��
                //MenuItem_ContextThumbnailView.Enabled = g_makeThumbnail;	//�T���l�C��������Ă��邩�ǂ���
                Menu_ContextThumbnailView.Checked = ViewState.ThumbnailView;

                //2�y�[�W���[�h:���y�[�W�����2�y�[�W���[�h���̂�
                Menu_ContextHalfPageBack.Enabled = ViewState.DualView && (bool)(App.g_pi.NowViewPage != 0);  //�擪�y�[�W�`�F�b�N
                Menu_ContextHalfPageForword.Enabled = ViewState.DualView && !IsLastPageViewing();    //�ŏI�y�[�W�`�F�b�N

                //�T���l�C���\����
                if (ViewState.ThumbnailView)
                {
                    //�T���l�C������Context�֌W�Ȃ�
                }
                else
                {
                    //�i�r�Q�[�V�������j���[�̗L������
                    Menu_ContextTop.Enabled = (bool)(App.g_pi.NowViewPage != 0);        //�擪�y�[�W�`�F�b�N
                    Menu_ContextBack.Enabled = (bool)(App.g_pi.NowViewPage != 0);   //�擪�y�[�W�`�F�b�N
                    Menu_ContextNext.Enabled = !IsLastPageViewing();        //�ŏI�y�[�W�`�F�b�N
                    Menu_ContextLast.Enabled = !IsLastPageViewing();        //�ŏI�y�[�W�`�F�b�N
                                                                            //MenuItem_ContextLast.Enabled = (bool)(g_pi.ViewPage != g_pi.Items.Count - 1);
                }
            }
        }

        private void Menu_Bookmark_DropDownOpening(object sender, EventArgs e)
        {
            //���j���[����x�S���N���A���ď������j���[�����
            Menu_Bookmark.DropDownItems.Clear();
            this.Menu_Bookmark.DropDownItems.AddRange(new ToolStripItem[] {
                this.Bookmark_Add,
                this.BookMark_Clear,
                this.toolStripSeparator11});

            //���j���[��������
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

            //ver1.79 �C���[�W��ǉ�����
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

        // �u�b�N�}�[�N�֘A *************************************************************************

        /// <summary>
        /// ���j���[��Bookmark��ǉ�����B
        /// �R���e�L�X�g���j���[�̒ǉ��Ɏg�p����Ă���(ver1.79)
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
                        ii.Filename,    //�t�@�C����
                        null,
                        new System.EventHandler(OnBookmarkList) //�C�x���g
                        );
                }
            }
            BookmarkMenu.Enabled = count != 0;
            return;
        }

        /// <summary>
        /// ������̈ꗗ����I�����ꂽ�Ƃ��ɌĂ΂��
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

        // ���̂ق� *********************************************************************************

        private async Task ReloadPageAsync()
        {
            if (App.g_pi == null || App.g_pi.Items.Count == 0)
                return;

            //ver0.972 �T�C�h�o�[������΃��T�C�Y
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
                    "�X���C�h�V���[���J�n���܂��B\r\n�}�E�X�N���b�N�܂��̓L�[���͂ŏI�����܂��B",
                    1500);

                //�^�C�}�[�ݒ�
                string s = (sender as ToolStripItem).Tag as string;
                if (int.TryParse(s, out int msec) && msec != 0)
                    SlideShowTimer.Interval = msec;
                else
                    SlideShowTimer.Interval = App.Config.SlideshowTime;

                //�^�C�}�[�J�n
                SlideShowTimer.Start();
            }
        }

        private void Menu_Unsharp_Click(object sender, EventArgs e)
        {
            App.Config.Advance.UnsharpMask = !App.Config.Advance.UnsharpMask;
            MenuItem_Unsharp.Checked = App.Config.Advance.UnsharpMask;

            //�ĕ`��
            PicPanel.Invalidate();
        }

        private async void Menu_ForwordMultiPages_Click(object sender, EventArgs e)
        {
            await NavigateToForwordMultiPageAsync();
        }

        private async void Menu_BackwordMultiPages_Click(object sender, EventArgs e)
        {
            await NavigateToBackwordMultiPageAsync();
        }

        private void UpDateMenuTextOfMultiPageNavigation()
        {
            var pages = App.Config.General.MultiPageNavigationCount;
            Menu_ForwordMultiPages.Text = $"{pages} �y�[�W�i��";
            Menu_BackwordMultiPages.Text = $"{pages} �y�[�W�߂�";
        }
    }
}
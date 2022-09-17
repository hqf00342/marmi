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

        private async void OnClickMRUMenu(object sender, EventArgs e)
        {
            var filename = ((ToolStripDropDownItem)sender).Text;

            if (File.Exists(filename) || Directory.Exists(filename))
            {
                await StartAsync(new string[] { filename });
            }
            else
            {
                MessageBox.Show($"�t�@�C����������܂���ł���\n{filename}", "�t�@�C���I�[�v���G���[");

                //MRU���X�g����폜
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
            _thumbPanel.SaveThumbnail(App.g_pi.PackageName);
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
            //MRU��ǉ�
            UpdateMruMenuListUI();

            ////�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
            //if (g_pi.Items == null || g_pi.Items.Count < 1)
            //{
            //	MenuItem_FileSaveThumbnail.Enabled = false;
            //}
            //else
            //{
            //	//�T���l�C���{�^��
            //	MenuItem_FileSaveThumbnail.Enabled = true;
            //}

            //ver1.81�T���l�C���͂��΂炭����
            MenuItem_FileSaveThumbnail.Enabled = false;
        }

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
                if (++menuCount >= App.Config.General.NumberOfMru)
                    break;
            }
        }

        #endregion FILE

        //�\�����j���[******************************************************************************

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
                //�S��ʂł���΃��j���[�n�͑S�Ĕ�\��
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

        private async void Menu_ViewMenubar_Click(object sender, EventArgs e)
        {
            //�g�O���؂�ւ�
            App.Config.VisibleMenubar = !App.Config.VisibleMenubar;
            menuStrip1.Visible = App.Config.VisibleMenubar;

            //ver0.972 �T�C�h�o�[�̈ʒu����
            //AjustControlArrangement();
            //�ĕ`��
            await ReloadPageAsync();
        }

        private async void Menu_ViewToolbar_Click(object sender, EventArgs e)
        {
            //�g�O���؂�ւ�
            App.Config.VisibleToolBar = !App.Config.VisibleToolBar;
            toolStrip1.Visible = App.Config.VisibleToolBar;

            //ver0.972 �i�r�o�[������΃��T�C�Y
            //AjustControlArrangement();
            //�ĕ`��
            await ReloadPageAsync();
        }

        private async void Menu_ViewStatusbar_Click(object sender, EventArgs e)
        {
            //�g�O���؂�ւ�
            App.Config.VisibleStatusBar = !App.Config.VisibleStatusBar;
            statusbar.Visible = App.Config.VisibleStatusBar;

            //ver0.972 �T�C�h�o�[�̈ʒu����
            //AjustControlArrangement();
            //�ĕ`��
            await ReloadPageAsync();
        }

        private async void Menu_ViewDualPage_Click(object sender, EventArgs e)
        {
            //�g�O���؂�ւ�
            await SetDualViewModeAsync(!App.Config.DualView);
        }

        private async void Menu_ViewHalfPageBack_Click(object sender, EventArgs e)
        {
            if (App.Config.DualView)
            {
                if (App.g_pi.NowViewPage > 0)
                {
                    await SetViewPageAsync(--App.g_pi.NowViewPage);
                }
                else
                {
                    //�擪�y�[�W�������̂ŉ������Ȃ��B
                }
            }
        }

        private async void Menu_ViewHalfPageForword_Click(object sender, EventArgs e)
        {
            if (App.Config.DualView)
            {
                if (App.g_pi.NowViewPage < App.g_pi.Items.Count)
                {
                    App.g_pi.NowViewPage++;  //���y�[�W�߂�
                    await SetViewPageAsync(App.g_pi.NowViewPage);  //ver0.988 2010�N6��20��
                }
                else
                {
                    // �ŏI�y�[�W�Ȃ̂ŉ������Ȃ�
                }
            }
        }

        private void Menu_ViewPictureInfo_Click(object sender, EventArgs e)
        {
            //ver1.81 �摜���m�F
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
            //ver1.81 �摜���m�F
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
            App.Config.IsFitScreenAndImage = !App.Config.IsFitScreenAndImage;
            PicPanel.IsAutoFit = App.Config.IsFitScreenAndImage;

            PicPanel.Refresh();
            UpdateStatusbar();
        }

        private void Menu_ViewSidebar_Click(object sender, EventArgs e)
        {
            if (_sidebar.Visible)
            {
                //����
                _sidebar.Visible = false;
                App.Config.VisibleSidebar = false;
            }
            else
            {
                //�T�C�h�o�[�I�[�v��
                _sidebar.Init(App.g_pi);
                if (App.Config != null)
                    _sidebar.Width = App.Config.SidebarWidth;
                else
                    _sidebar.Width = App.SIDEBAR_DEFAULT_WIDTH;

                _sidebar.Visible = true;
                _sidebar.SetItemToCenter(App.g_pi.NowViewPage);
                App.Config.VisibleSidebar = true;
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
            if (App.Config.DualView)
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

        private async void Menu_Option_Click(object sender, EventArgs e)
        {
            var fo = new FormOption();
            fo.LoadConfig(App.Config);
            if (fo.ShowDialog() == DialogResult.OK)
            {
                fo.SaveConfig(ref App.Config);

                //�L�[�R���t�B�O���f
                SetKeyConfig2();

                //ver1.65 �c�[���o�[�̕����͂������f
                SetToolbarString();
                ResizeTrackBar();

                //ver1.79 ScreenCache���N���A����B
                ScreenCache.Clear();

                //�T���l�C���T�C�Y�͂����ɔ��f
                if (_thumbPanel != null && _thumbPanel.Visible)
                {
                    _thumbPanel.SetThumbnailSize(App.Config.Thumbnail.ThumbnailSize);
                    _thumbPanel.BackColor = App.Config.Thumbnail.ThumbnailBackColor;
                    _thumbPanel.SetFont(App.Config.Thumbnail.ThumbnailFont, App.Config.Thumbnail.ThumbnailFontColor);
                }
                if (App.Config.isThumbnailView)
                {
                    _thumbPanel.ReDraw();
                }
                else
                {
                    //�ʏ��ʂ��ĕ`��
                    await SetViewPageAsync(App.g_pi.NowViewPage);
                }
            }
        }

        private void Menu_RecurseDir_Click(object sender, EventArgs e)
        {
            App.Config.IsRecurseSearchDir = !App.Config.IsRecurseSearchDir;
            Menu_OptionRecurseDir.Checked = App.Config.IsRecurseSearchDir;
        }

        private void Menu_keepMagnification_Click(object sender, EventArgs e)
        {
            App.Config.KeepMagnification = !App.Config.KeepMagnification;
        }

        private void Menu_UseBicubic_Click(object sender, EventArgs e)
        {
            App.Config.View.IsDotByDotZoom = !App.Config.View.IsDotByDotZoom;
        }

        private async void Menu_DontEnlargeOver100percent_Click(object sender, EventArgs e)
        {
            App.Config.View.NoEnlargeOver100p = !App.Config.View.NoEnlargeOver100p;
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }

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
                if (App.Config.isThumbnailView)
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
                //StopThumbnailMakerThread();	//�\�[�g���ɃX���b�h�������Ă��Ȃ����Ƃ�S��
                //PauseThumbnailMakerThread();	//ver1.09 �X���b�h���f�iPause�j

                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.CreateDate);
                App.g_pi.Items.Sort(comparer);

                //�T���l�C���\�����ł���΍ĕ`�ʂ�����
                if (App.Config.isThumbnailView)
                {
                    //ThumbPanel.MakeThumbnailScreen(true);	//�����ĕ`��
                    //ThumbPanel.Invalidate();
                    _thumbPanel.ReDraw();
                }
                //StartThumnailMakerThread();//�\�[�g�����A�X���b�h�ĊJ
                //ResumeThumbnailMakerThread();	//ver1.09 �X���b�h�ĊJ

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
            if (App.Config.isFullScreen)
                menuStrip1.Visible = false;
        }

        // ���j���[�I�[�v�j���O�C�x���g *************************************************************

        private void Menu_View_DropDownOpening(object sender, EventArgs e)
        {
            //�o�[�֘A�̃��j���[
            Menu_ViewToolbar.Enabled = !App.Config.isFullScreen;
            Menu_ViewStatusbar.Enabled = !App.Config.isFullScreen;
            Menu_ViewMenubar.Checked = menuStrip1.Visible;
            Menu_ViewToolbar.Checked = toolStrip1.Visible;
            Menu_ViewStatusbar.Checked = statusbar.Visible;
            Menu_View2Page.Checked = App.Config.DualView;
            Menu_ViewFullScreen.Checked = App.Config.isFullScreen;
            Menu_ViewFitScreenSize.Checked = App.Config.IsFitScreenAndImage;
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
            Menu_OptionRecurseDir.Checked = App.Config.IsRecurseSearchDir;
            //MenuItem_OptionSidebarFix.Checked = App.Config.isFixSidebar;
            Menu_keepMagnification.Checked = App.Config.KeepMagnification;
            Menu_UseBicubic.Checked = !App.Config.View.IsDotByDotZoom;
            Menu_DontEnlargeOver100percent.Checked = App.Config.View.NoEnlargeOver100p;

            //ver1.83�A���V���[�v
            MenuItem_Unsharp.Checked = App.Config.Advance.UseUnsharpMask;
        }

        private void Menu_Help_DropDownOpening(object sender, EventArgs e)
        {
            MenuItem_CheckSusie.Checked = App.susie.isSupportedExtentions("pdf");
        }

        private void Menu_Page_DropDownOpening(object sender, EventArgs e)
        {
            //���J���ɂ���
            Menu_View_LeftOpen.Checked = !App.g_pi.PageDirectionIsLeft;
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
                Menu_ViewHalfPageBack.Enabled = App.Config.DualView && (bool)(App.g_pi.NowViewPage != 0); //�擪�y�[�W�`�F�b�N
                Menu_ViewHalfPageForword.Enabled = App.Config.DualView && !IsLastPageViewing();       //�ŏI�y�[�W�`�F�b�N

                //������@�\
                //ver1.79�R�����g�A�E�g
                //Menu_ViewAddBookmark.Enabled = true;	//������
                //Menu_ViewAddBookmark.Checked =
                //	g_pi.Items[g_pi.NowViewPage].isBookMark;

                //�T���l�C���\����
                if (App.Config.isThumbnailView)
                {
                    //�T���l�C�����͍��E��Disable
                    Menu_ViewTop.Enabled = false;
                    Menu_ViewBack.Enabled = false;
                    Menu_ViewEnd.Enabled = false;
                    Menu_ViewNext.Enabled = false;
                    Menu_View2Page.Enabled = false;
                    Menu_ViewSidebar.Enabled = false;
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
                }
            }
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //�o�[�֘A�̃��j���[
            Menu_ContextToolbar.Enabled = !App.Config.isFullScreen;
            Menu_ContextStatusbar.Enabled = !App.Config.isFullScreen;

            Menu_ContextMenubar.Checked = App.Config.VisibleMenubar;
            Menu_ContextToolbar.Checked = App.Config.VisibleToolBar;
            Menu_ContextStatusbar.Checked = App.Config.VisibleStatusBar;

            Menu_ContextDualView.Checked = App.Config.DualView;
            Menu_ContextFullView.Checked = App.Config.isFullScreen;
            Menu_ContextFitScreenSize.Checked = App.Config.IsFitScreenAndImage;

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
                Menu_ContextThumbnailView.Checked = App.Config.isThumbnailView;

                //2�y�[�W���[�h:���y�[�W�����2�y�[�W���[�h���̂�
                Menu_ContextHalfPageBack.Enabled = App.Config.DualView && (bool)(App.g_pi.NowViewPage != 0);  //�擪�y�[�W�`�F�b�N
                Menu_ContextHalfPageForword.Enabled = App.Config.DualView && !IsLastPageViewing();    //�ŏI�y�[�W�`�F�b�N

                //�T���l�C���\����
                if (App.Config.isThumbnailView)
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

            //ver0.972 �i�r�o�[������΃��T�C�Y
            AjustSidebarArrangement();

            if (App.Config.isThumbnailView)
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
                    SlideShowTimer.Interval = App.Config.SlideShowTime;

                //�^�C�}�[�J�n
                SlideShowTimer.Start();
            }
        }

        private void Menu_Unsharp_Click(object sender, EventArgs e)
        {
            App.Config.Advance.UseUnsharpMask = !App.Config.Advance.UseUnsharpMask;
            MenuItem_Unsharp.Checked = App.Config.Advance.UseUnsharpMask;

            //�ĕ`��
            PicPanel.Invalidate();
        }
    }
}
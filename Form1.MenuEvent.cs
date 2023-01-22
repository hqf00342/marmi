using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;				//Debug, Stopwatch
using System.IO;						//Directory, File
using System.ComponentModel;			//EventArgs
//using System.Drawing;


namespace Marmi
{
	public partial class Form1 : Form
	{
		// ���j���[�C�x���g *************************************************************/

		private void OnClickMRUMenu(object sender, EventArgs e)
		{
			ToolStripDropDownItem tsddi = (ToolStripDropDownItem)sender;
			if (File.Exists(tsddi.Text))
			{
				//OpenFileAndStart(tsddi.Text);

				//ver1.09 ���������ăX�^�[�g�𖾊m��
				//OpenFileAndStart(tsddi.Text);�͉�������̂��悭������Ȃ���ԂȂ̂ō폜
				Start(new string[] { tsddi.Text });
			}
			else
			{
				string sz = string.Format("�t�@�C����������܂���ł���\n{0}", tsddi.Text);
				MessageBox.Show(sz, "�t�@�C���I�[�v���G���[");

				//MRU���X�g����폜
				for (int i = 0; i < g_Config.mru.Length; i++)
				{
					if (g_Config.mru[i] != null && g_Config.mru[i].Name == tsddi.Text)
					{
						g_Config.mru[i] = null;
						break;
					}
				}
			}
		}


		//�t�@�C�����j���[******************************************************************************

		/// <summary>
		/// �t�@�C���I�[�v�����j���[
		/// </summary>
		private void Menu_FileOpen_Click(object sender, EventArgs e)
		{
			OpenDialog();
		}

		/// <summary>
		/// �T���l�C���ۑ����j���[
		/// </summary>
		private void Menu_SaveThumbnail_Click(object sender, EventArgs e)
		{
			g_ThumbPanel.Location = GetClientRectangle().Location;
			g_ThumbPanel.Size = GetClientRectangle().Size;
			g_ThumbPanel.Parent = this;
			g_ThumbPanel.SaveThumbnail(g_pi.PackageName);
		}

		/// <summary>
		/// �I�����j���[
		/// </summary>
		private void Menu_Exit_Click(object sender, EventArgs e)
		{
			this.Close();
			Application.Exit();
		}

		//�\�����j���[******************************************************************************

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
			SetViewPage(g_pi.Items.Count - 1);
		}

		private void Menu_ViewThumbnail_Click(object sender, EventArgs e)
		{
			if (g_pi.Items.Count > 0)
			{
				//�S��ʂł���΃��j���[�n�͑S�Ĕ�\��
				if (g_Config.isFullScreen)
				{
					toolStrip1.Visible = false;
					statusbar.Visible = false;
					menuStrip1.Visible = false;
				}
				SetThumbnailView(!g_Config.isThumbnailView);
			}
		}

		private void Menu_ViewFullScreen_Click(object sender, EventArgs e)
		{
			SetFullScreen(!g_Config.isFullScreen);
		}

		private void Menu_ViewMenubar_Click(object sender, EventArgs e)
		{
			//�g�O���؂�ւ�
			g_Config.visibleMenubar = !g_Config.visibleMenubar;
			menuStrip1.Visible = g_Config.visibleMenubar;

			//ver0.972 �T�C�h�o�[�̈ʒu����
			//AjustControlArrangement();
			//�ĕ`��
			Reload();
		}

		private void Menu_ViewToolbar_Click(object sender, EventArgs e)
		{
			//�g�O���؂�ւ�
			g_Config.visibleToolBar = !g_Config.visibleToolBar;
			toolStrip1.Visible = g_Config.visibleToolBar;


			//ver0.972 �i�r�o�[������΃��T�C�Y
			//AjustControlArrangement();
			//�ĕ`��
			Reload();
		}

		private void Menu_ViewStatusbar_Click(object sender, EventArgs e)
		{
			//�g�O���؂�ւ�
			g_Config.visibleStatusBar = !g_Config.visibleStatusBar;
			statusbar.Visible = g_Config.visibleStatusBar;

			//ver0.972 �T�C�h�o�[�̈ʒu����
			//AjustControlArrangement();
			//�ĕ`��
			Reload();
		}

		private void Menu_ViewDualPage_Click(object sender, EventArgs e)
		{
			//�g�O���؂�ւ�
			SetDualViewMode(!g_Config.dualView);
		}

		private void Menu_ViewHalfPageBack_Click(object sender, EventArgs e)
		{
			if (g_Config.dualView)
			{
				if (g_pi.NowViewPage > 0)
				{
					//g_pi.NowViewPage -= 1;	//���y�[�W�߂�
					SetViewPage(--g_pi.NowViewPage);	//ver0.988
				}
				else
				{
					//TODO:�������ꂢ�ɂ��܂��傤
					//InformationLabel il = new InformationLabel(
					//    this,
					//    "�擪�y�[�W�ł��B" + g_pi.ViewPage.ToString()
					//    );
				}
			}
		}

		private void Menu_ViewHalfPageForword_Click(object sender, EventArgs e)
		{
			if (g_Config.dualView)
			{
				if (g_pi.NowViewPage < g_pi.Items.Count)
				{
					g_pi.NowViewPage += 1;	//���y�[�W�߂�
					SetViewPage(g_pi.NowViewPage);	//ver0.988 2010�N6��20��
				}
				else
				{
					//TODO:�������ꂢ�ɂ��܂��傤�B�ŏI�y�[�W����Ȃ���
					//InformationLabel il = new InformationLabel(
					//    this,
					//    "�ŏI�y�[�W�ł��B" + g_pi.ViewPage.ToString()
					//    );
				}
				//setStatusbarPages();
				//setStatusbarFilename();

			}

		}

		private void Menu_ViewPictureInfo_Click(object sender, EventArgs e)
		{
			//ver1.81 �摜���m�F
			if (!CheckIndex(g_pi.NowViewPage))
				return;

			FormPictureInfo p = new FormPictureInfo();
			if (g_viewPages == 1)
				p.Show(this, g_pi.Items[g_pi.NowViewPage], null);
			else
				p.Show(this, g_pi.Items[g_pi.NowViewPage], g_pi.Items[g_pi.NowViewPage + 1]);
		}

		private void Menu_ViewPackageInfo_Click(object sender, EventArgs e)
		{
			//ver1.81 �摜���m�F
			if (g_pi.Items.Count <=0)
				return;

			FormPackageInfo pif = new FormPackageInfo(this, g_pi);
			pif.setSortMode(false);
			//pif.Show(g_pi.ViewPage);
			pif.ShowDialog(g_pi.NowViewPage);

		}

		private void Menu_ViewFitScreenSize_Click(object sender, EventArgs e)
		{
			ToggleFitScreen();
		}

		private void ToggleFitScreen()
		{
			g_Config.isFitScreenAndImage = !g_Config.isFitScreenAndImage;
			PicPanel.isAutoFit = g_Config.isFitScreenAndImage;

			PicPanel.Refresh();
			UpdateStatusbar();
		}


		private void MenuItem_ViewSidebar_Click(object sender, EventArgs e)
		{
			if (g_Sidebar.Visible)
			{
				//����
				g_Sidebar.Visible = false;
				g_Config.visibleNavibar = false;
			}
			else
			{
				//�T�C�h�o�[�I�[�v��
				g_Sidebar.Init(g_pi);
				if (Form1.g_Config != null)
					g_Sidebar.Width = Form1.g_Config.sidebarWidth;
				else
					g_Sidebar.Width = SIDEBAR_DEFAULT_WIDTH;

				g_Sidebar.Visible = true;
				g_Sidebar.SetItemToCenter(g_pi.NowViewPage);
				g_Config.visibleNavibar = true;
			}
			AjustSidebarArrangement();
		}


		[Obsolete]
		private void MenuItem_ViewFixSidebar_Click(object sender, EventArgs e)
		{
			//g_Config.isFixSidebar = !g_Config.isFixSidebar;
			//MenuItem_ViewFixSidebar.Checked = g_Config.isFixSidebar;
			//MenuItem_OptionSidebarFix.Checked = g_Config.isFixSidebar;
		}


		private void MenuItem_View_LeftOpen_Click(object sender, EventArgs e)
		{
			//g_pi.PageDirectionisRight = !MenuItem_View_LeftOpen.Checked;
			g_pi.LeftBook = !g_pi.LeftBook;
			if (g_Config.dualView)
			{
				SetViewPage(g_pi.NowViewPage);
			}
		}

		private void Menu_ToolbarBottom_Click(object sender, EventArgs e)
		{
			if (toolStrip1.Dock == DockStyle.Bottom)
				toolStrip1.Dock = DockStyle.Top;
			else
				toolStrip1.Dock = DockStyle.Bottom;
		}

	
		//�w���v���j���[******************************************************************************

		private void MenuItem_HelpVersion_Click(object sender, EventArgs e)
		{
			VersionForm vf = new VersionForm();
			vf.StartPosition = FormStartPosition.CenterParent;
			vf.ShowDialog();
		}

		//�I�v�V�������j���[******************************************************************************

		private void Menu_Option_Click(object sender, EventArgs e)
		{
			FormOption fo = new FormOption();
			fo.LoadConfig(g_Config);
			if (fo.ShowDialog() == DialogResult.OK)
			{
				fo.SaveConfig(ref g_Config);

				//ver1.21 �L�[�R���t�B�O���f
				//ver1.81 �ύX
				//SetKeyConfig();
				SetKeyConfig2();

				//ver1.65 �c�[���o�[�̕����͂������f
				SetToolbarString();
				ResizeTrackBar();

				//ver1.79 ScreenCache���N���A����B
				ScreenCache.Clear();

				//�T���l�C���T�C�Y�͂����ɔ��f
				//if (g_ThumbPanel != null)
				if (g_ThumbPanel != null && g_ThumbPanel.Visible)
				{
					g_ThumbPanel.SetThumbnailSize(g_Config.ThumbnailSize);
					g_ThumbPanel.BackColor = g_Config.ThumbnailBackColor;
					g_ThumbPanel.SetFont(g_Config.ThumbnailFont, g_Config.ThumbnailFontColor);
				}
				if (g_Config.isThumbnailView)
				{
					g_ThumbPanel.ReDraw();
				}
				else
				{
					//�ʏ��ʂ��ĕ`��
					SetViewPage(g_pi.NowViewPage);
				}

			}
		}


		private void Menu_ClearCacheFile_Click(object sender, EventArgs e)
		{
			string[] files = Directory.GetFiles(Application.StartupPath, "*" + CACHEEXT);
			if (files.Length > 0)
			{
				long size = 0;
				foreach (string delFile in files)
				{
					FileInfo fi = new FileInfo(delFile);
					size += fi.Length;
					File.Delete(delFile);
				}
				MessageBox.Show(string.Format("{0}�̃L���b�V���A{1:N0}�o�C�g���폜���܂���",
					files.Length, size));
			}
			else
				MessageBox.Show("�L���b�V���t�@�C���͂���܂���ł���");
		}


		private void Menu_RemakeThumbnail_Click(object sender, EventArgs e)
		{
			//ver1.81 �摜���m�F
			if (g_pi.Items.Count <= 0)
				return;

			//�T���l�C�����N���A����
			for (int i = 0; i < g_pi.Items.Count; i++)
			{
				if (g_pi.Items[i].thumbnail != null)
					g_pi.Items[i].thumbnail.Dispose();
				g_pi.Items[i].thumbnail = null;
			}
			//ver 1.55�ēo�^
			AsyncLoadImageInfo();
		}

		private void Menu_RecurseDir_Click(object sender, EventArgs e)
		{
			g_Config.isRecurseSearchDir = !g_Config.isRecurseSearchDir;
			Menu_OptionRecurseDir.Checked = g_Config.isRecurseSearchDir;
		}

		//�\�[�g���j���[******************************************************************************

		private void Menu_SortByName_Click(object sender, EventArgs e)
		{
			//�t�@�C�����X�g����ёւ���
			if (g_pi.Items.Count > 0)
			{
				NaturalOrderComparer2 noc = new NaturalOrderComparer2();
				g_pi.Items.Sort(noc);

				//�T���l�C���\�����ł���΍ĕ`�ʂ�����
				if (g_Config.isThumbnailView)
				{
					g_ThumbPanel.ReDraw();
				}

				//ver1.38 �\�[�g��ɉ�ʂ���������
				ScreenCache.Clear();
				SetViewPage(g_pi.NowViewPage);
			}
		}

		private void Menu_SortByDate_Click(object sender, EventArgs e)
		{
			//�t�@�C�����X�g����ёւ���
			if (g_pi.Items.Count > 0)
			{
				//StopThumbnailMakerThread();	//�\�[�g���ɃX���b�h�������Ă��Ȃ����Ƃ�S��
				//PauseThumbnailMakerThread();	//ver1.09 �X���b�h���f�iPause�j

				DateCompare dc = new DateCompare();
				g_pi.Items.Sort(dc);

				//�T���l�C���\�����ł���΍ĕ`�ʂ�����
				if (g_Config.isThumbnailView)
				{
					//ThumbPanel.MakeThumbnailScreen(true);	//�����ĕ`��
					//ThumbPanel.Invalidate();
					g_ThumbPanel.ReDraw();
				}
				//StartThumnailMakerThread();//�\�[�g�����A�X���b�h�ĊJ
				//ResumeThumbnailMakerThread();	//ver1.09 �X���b�h�ĊJ

				//ver1.38 �\�[�g��ɉ�ʂ���������
				ScreenCache.Clear();
				SetViewPage(g_pi.NowViewPage);
			}

		}

		private void Menu_SortCustom_Click(object sender, EventArgs e)
		{
			FormPackageInfo pif = new FormPackageInfo(this, g_pi);
			pif.setSortMode(true);
			pif.ShowDialog(g_pi.NowViewPage);

			//ver1.38 �\�[�g��ɉ�ʂ���������
			ScreenCache.Clear();
			SetViewPage(g_pi.NowViewPage);
		}

		// ���j���[�z�o�[�C�x���g *******************************************************/

		private void Menu_MouseHover(object sender, EventArgs e)
		{
			//1�N���b�N�Ή��p�ɕێ����Ă���
			g_hoverStripItem = sender;
		}

		private void Menu_MouseLeave(object sender, EventArgs e)
		{
			g_hoverStripItem = null;
		}

		private void menuStrip1_MenuDeactivate(object sender, EventArgs e)
		{
			//�S��ʃ��[�h�Ńt�H�[�J�X���������Ƃ��͉B��
			if (g_Config.isFullScreen)
				menuStrip1.Visible = false;

		}


		// ���j���[�I�[�v�j���O�C�x���g **************************************************

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

		private void Menu_View_DropDownOpening(object sender, EventArgs e)
		{
			//�o�[�֘A�̃��j���[
			Menu_ViewToolbar.Enabled = !g_Config.isFullScreen;
			Menu_ViewStatusbar.Enabled = !g_Config.isFullScreen;
			Menu_ViewMenubar.Checked = menuStrip1.Visible;
			Menu_ViewToolbar.Checked = toolStrip1.Visible;
			Menu_ViewStatusbar.Checked = statusbar.Visible;
			Menu_View2Page.Checked = g_Config.dualView;
			Menu_ViewFullScreen.Checked = g_Config.isFullScreen;
			Menu_ViewFitScreenSize.Checked = g_Config.isFitScreenAndImage;
			Menu_ViewNavibar.Checked = g_Sidebar.Visible;
			//�c�[���o�[�̈ʒu
			Menu_ToolbarBottom.Checked = (toolStrip1.Dock == DockStyle.Bottom);
			//�T�C�h�o�[�֘A
			//MenuItem_ViewFixSidebar.Checked = g_Config.isFixSidebar;
			//������֘A�@�\
			//ver1.79�R�����g�A�E�g
			//AddBookmarkMenuItem(MenuItem_ViewBookmarkList);

			//ver1.81 �p�b�P�[�W�Ȃ��̂Ƃ��̑Ώ�
			bool isPackageOpen = (g_pi.Items.Count > 0);
			Menu_ViewPictureInfo.Enabled = isPackageOpen;
			Menu_ViewPackageInfo.Enabled = isPackageOpen;
		}

		private void Menu_Option_DropDownOpening(object sender, EventArgs e)
		{
			//�`�F�b�N���
			Menu_OptionRecurseDir.Checked = g_Config.isRecurseSearchDir;
			//MenuItem_OptionSidebarFix.Checked = g_Config.isFixSidebar;
			Menu_keepMagnification.Checked = g_Config.keepMagnification;
			Menu_UseBicubic.Checked = !g_Config.isDotByDotZoom;
			Menu_DontEnlargeOver100percent.Checked = g_Config.noEnlargeOver100p;

			//�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
			if (g_pi.Items == null || g_pi.Items.Count <= 1)
			{
				Menu_OptionReloadThumbnail.Enabled = false;
			}
			else
			{
				Menu_OptionReloadThumbnail.Enabled = true;
			}

		}


		private void Menu_Help_DropDownOpening(object sender, EventArgs e)
		{
			MenuItem_CheckSusie.Checked = susie.isSupportedExtentions("pdf");
			MenuItem_CheckUnrar.Checked = unrar.dllLoaded;
		}

		private void Menu_Page_DropDownOpening(object sender, EventArgs e)
		{
			//���J���ɂ���
			Menu_View_LeftOpen.Checked = !g_pi.LeftBook;
			//�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
			if (g_pi == null || g_pi.Items == null || g_pi.Items.Count < 1)
			{
				//�t�@�C�����Ȃ��ꍇ
				Menu_ViewTop.Enabled = false;
				Menu_ViewEnd.Enabled = false;
				Menu_ViewBack.Enabled = false;
				Menu_ViewNext.Enabled = false;			//����
				Menu_ViewThumbnail.Enabled = false;		//�T���l�C��
				Menu_ViewHalfPageBack.Enabled = false;	//���y�[�W
				Menu_ViewHalfPageForword.Enabled = false;//���y�[�W
				Menu_ViewFitScreenSize.Enabled = false;	//�t���X�N���[��
				Menu_ViewPictureInfo.Enabled = false;	//�摜���
				//Menu_ViewAddBookmark.Checked = false;	//������
				//Menu_ViewAddBookmark.Enabled = false;	//������
				Menu_ViewZoom.Enabled = false;	//Zoom
				Menu_ViewReload.Enabled = false;
				Menu_SlideShow.Enabled = false;
				return;
			}
			else if (g_pi.Items.Count == 1)
			{
				//�t�@�C�����P��
				Menu_ViewTop.Enabled = false;
				Menu_ViewEnd.Enabled = false;
				Menu_ViewBack.Enabled = false;
				Menu_ViewNext.Enabled = false;
				Menu_ViewHalfPageBack.Enabled = false;
				Menu_ViewHalfPageForword.Enabled = false;
				Menu_ViewThumbnail.Enabled = true;
				Menu_ViewFitScreenSize.Enabled = true;
				Menu_ViewPictureInfo.Enabled = true;
				//Menu_ViewAddBookmark.Checked = false;	//������
				//Menu_ViewAddBookmark.Enabled = false;	//������
				Menu_ViewZoom.Enabled = true;	//Zoom
				Menu_ViewReload.Enabled = true;
				Menu_SlideShow.Enabled = false;
				return;
			}
			else
			{
				//�������\��
				Menu_ViewFitScreenSize.Enabled = true;
				Menu_ViewPictureInfo.Enabled = true;
				Menu_ViewZoom.Enabled = true;	//Zoom
				Menu_ViewReload.Enabled = true;

				//�X���C�h�V���[
				Menu_SlideShow.Enabled = true;

				//�T���l�C���{�^��
				Menu_ViewThumbnail.Checked = g_Config.isThumbnailView;

				//2�y�[�W���[�h:���y�[�W�����2�y�[�W���[�h���̂�
				Menu_ViewHalfPageBack.Enabled = g_Config.dualView && (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
				Menu_ViewHalfPageForword.Enabled = g_Config.dualView && !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N

				//������@�\
				//ver1.79�R�����g�A�E�g
				//Menu_ViewAddBookmark.Enabled = true;	//������
				//Menu_ViewAddBookmark.Checked =
				//	g_pi.Items[g_pi.NowViewPage].isBookMark;

				//�T���l�C���\����
				if (g_Config.isThumbnailView)
				{
					//�T���l�C�����͍��E��Disable
					Menu_ViewTop.Enabled = false;
					Menu_ViewBack.Enabled = false;
					Menu_ViewEnd.Enabled = false;
					Menu_ViewNext.Enabled = false;
					Menu_View2Page.Enabled = false;
					Menu_ViewNavibar.Enabled = false;
				}
				else
				{
					//�i�r�Q�[�V�������j���[�̗L������
					Menu_ViewTop.Enabled = (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
					Menu_ViewBack.Enabled = (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
					Menu_ViewEnd.Enabled = !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N
					Menu_ViewNext.Enabled = !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N
					Menu_View2Page.Enabled = true;
					Menu_ViewNavibar.Enabled = true;
				}
			}
		}

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			//�o�[�֘A�̃��j���[
			Menu_ContextToolbar.Enabled = !g_Config.isFullScreen;
			Menu_ContextStatusbar.Enabled = !g_Config.isFullScreen;

			Menu_ContextMenubar.Checked = g_Config.visibleMenubar;
			Menu_ContextToolbar.Checked = g_Config.visibleToolBar;
			Menu_ContextStatusbar.Checked = g_Config.visibleStatusBar;

			Menu_ContextDualView.Checked = g_Config.dualView;
			Menu_ContextFullView.Checked = g_Config.isFullScreen;
			Menu_ContextFitScreenSize.Checked = g_Config.isFitScreenAndImage;

			Menu_ContextNavibar.Checked = g_Sidebar.Visible;

			//�t�@�C�����{�����Ă��Ȃ��ꍇ�̃i�r�Q�[�V����
			if (g_pi.Items == null || g_pi.Items.Count < 1)
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
				Menu_ContextZoom.Enabled = false;	//zoom
				Menu_ContextRedraw.Enabled = false;
				return;
			}
			else if (g_pi.Items.Count == 1)
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
				Menu_ContextZoom.Enabled = true;	//zoom
				Menu_ContextRedraw.Enabled = true;
				return;
			}
			else
			{
				Menu_ContextFitScreenSize.Enabled = true;
				Menu_ContextPictureInfo.Enabled = true;
				Menu_ContextNavibar.Enabled = true;
				Menu_ContextZoom.Enabled = true;	//zoom
				Menu_ContextRedraw.Enabled = true;

				//������@�\
				Menu_ContextAddBookmark.Enabled = true;
				if (g_pi.Items[g_pi.NowViewPage].isBookMark)
					Menu_ContextAddBookmark.Checked = true;
				else
					Menu_ContextAddBookmark.Checked = false;
				AddBookmarkMenuItem(Menu_ContextBookmarkList);

				//�T���l�C���{�^��
				//MenuItem_ContextThumbnailView.Enabled = g_makeThumbnail;	//�T���l�C��������Ă��邩�ǂ���
				Menu_ContextThumbnailView.Checked = g_Config.isThumbnailView;

				//2�y�[�W���[�h:���y�[�W�����2�y�[�W���[�h���̂�
				Menu_ContextHalfPageBack.Enabled = g_Config.dualView && (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
				Menu_ContextHalfPageForword.Enabled = g_Config.dualView && !IsLastPageViewing();	//�ŏI�y�[�W�`�F�b�N

				//�T���l�C���\����
				if (g_Config.isThumbnailView)
				{
					//�T���l�C������Context�֌W�Ȃ�
				}
				else
				{
					//�i�r�Q�[�V�������j���[�̗L������
					Menu_ContextTop.Enabled = (bool)(g_pi.NowViewPage != 0);		//�擪�y�[�W�`�F�b�N
					Menu_ContextBack.Enabled = (bool)(g_pi.NowViewPage != 0);	//�擪�y�[�W�`�F�b�N
					Menu_ContextNext.Enabled = !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N
					Menu_ContextLast.Enabled = !IsLastPageViewing();		//�ŏI�y�[�W�`�F�b�N
					//MenuItem_ContextLast.Enabled = (bool)(g_pi.ViewPage != g_pi.Items.Count - 1);
				}
			}
		}

		// ���̂ق� **************************************************
		/// <summary>
		/// ���j���[��Bookmark��ǉ�����B
		/// �R���e�L�X�g���j���[�̒ǉ��Ɏg�p����Ă���(ver1.79)
		/// </summary>
		/// <param name="BookmarkMenu"></param>
		private void AddBookmarkMenuItem(ToolStripMenuItem BookmarkMenu)
		{
			if (g_pi.Items.Count == 0)
			{
				BookmarkMenu.Enabled = false;
				return;
			}

			int count = 0;
			BookmarkMenu.DropDownItems.Clear();

			foreach (ImageInfo ii in g_pi.Items)
			{
				if (ii.isBookMark)
				{
					count++;
					BookmarkMenu.DropDownItems.Add(
						ii.filename,	//�t�@�C����
						null,
						new System.EventHandler(OnBookmarkList)	//�C�x���g
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
		/// ������̈ꗗ����I�����ꂽ�Ƃ��ɌĂ΂��
		/// </summary>
		private void OnBookmarkList(object sender, EventArgs e)
		{
			ToolStripDropDownItem tsddi = (ToolStripDropDownItem)sender;
			int index = g_pi.GetIndexFromFilename(tsddi.Text);
			//int index = (int)tsddi.Tag;
			if (index < 0)
				return;
			SetViewPage(index);
		}


		private void Menu_ContextBookmark_Click(object sender, EventArgs e)
		{
			ToggleBookmark();
		}

		private void Menu_Reload_Click(object sender, EventArgs e)
		{
			Reload();
		}

		private void Reload()
		{
			if (g_pi == null || g_pi.Items.Count == 0)
				return;

			//ver0.972 �i�r�o�[������΃��T�C�Y
			AjustSidebarArrangement();

			if (g_Config.isThumbnailView)
				g_ThumbPanel.ReDraw();
			else
				SetViewPage(g_pi.NowViewPage);
		}

		private void Menu_Slideshow_Click(object sender, EventArgs e)
		{
			if (g_pi.Items.Count == 0)
				return;

			if (SlideShowTimer.Enabled)
			{
				StopSlideShow();
			}
			else
			{
				g_ClearPanel.ShowAndClose(
					"�X���C�h�V���[���J�n���܂��B\r\n�}�E�X�N���b�N�܂��̓L�[���͂ŏI�����܂��B",
					1500);

				//�^�C�}�[�ݒ�
				int msec = 0;
				string s = (sender as ToolStripItem).Tag as string;
				if (int.TryParse(s, out msec) && msec != 0)
					SlideShowTimer.Interval = msec;
				else
					SlideShowTimer.Interval = g_Config.slideShowTime;

				//�^�C�}�[�J�n
				SlideShowTimer.Start();
			}
		}

		private void Menu_ClearMRU_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < g_Config.mru.Length; i++)
			{
				g_Config.mru[i] = null;
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
			if (g_pi.Items.Count == 0)
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
			foreach (ImageInfo ii in g_pi.Items)
			{
				if (ii.isBookMark)
				{
					ToolStripMenuItem item = new ToolStripMenuItem();
					item.Text = Path.GetFileName(ii.filename);
					item.Image = BitmapUty.MakeSquareThumbnailImage(ii.thumbnail, 40);
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

		private void BookMark_Clear_Click(object sender, EventArgs e)
		{
			int len = g_pi.Items.Count;
			for (int i = 0; i < len; i++)
			{
				g_pi.Items[i].isBookMark = false;
			}
		}

		private void Menu_keepMagnification_Click(object sender, EventArgs e)
		{
			g_Config.keepMagnification = !g_Config.keepMagnification;
		}


		private void Menu_UseBicubic_Click(object sender, EventArgs e)
		{
			g_Config.isDotByDotZoom = !g_Config.isDotByDotZoom;
		}

		private void Menu_DontEnlargeOver100percent_Click(object sender, EventArgs e)
		{
			g_Config.noEnlargeOver100p = !g_Config.noEnlargeOver100p;
			SetViewPage(g_pi.NowViewPage);
		}

		private void Menu_Bookshelf_Click(object sender, EventArgs e)
		{
			if (BookShelf == null)
			{
				BookShelf = new FlowLayoutPanel();
				//BookShelf.BackColor = Color.AntiqueWhite;
				this.Controls.Add(BookShelf);
				BookShelf.Parent = this;
				BookShelf.Dock = DockStyle.Fill;
				//PicPanel���\����
				PicPanel.Visible = false;
				PicPanel.Dock = DockStyle.None;
			}
			else
			{
				BookShelf.Hide();
				this.Controls.Remove(BookShelf);
				BookShelf.Dispose();
				BookShelf = null;
				//PicPanel���\����
				PicPanel.Visible = true;
				PicPanel.Dock = DockStyle.Fill;
			}
		}
	}
}

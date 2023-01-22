using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;				//Debug, Stopwatch
using System.IO;						//Directory, File
using System.Xml.Serialization;			//XmlSerializer
using System.Runtime.Serialization.Formatters.Binary;	//BinaryFormatter
using System.IO.Compression;			//DeflateStream
using System.Runtime.Serialization;		//IFormatter
using System.Linq;

namespace Marmi
{
	public partial class Form1 : Form
	{
		// ���[�e�B���e�B�n�FConfig�t�@�C�� *********************************************/

		//
		//AppGlobalConfig.cs�ֈړ��B���������������\�b�h�ɂ����B
		//

		//private string getConfigFileName()
		//{
		//	return Path.Combine(Application.StartupPath, CONFIGNAME);
		//}

		//public object LoadFromXmlFile()
		//{
		//	string path = getConfigFileName();

		//	if (File.Exists(path))
		//	{
		//		using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
		//		{
		//			XmlSerializer xs = new XmlSerializer(typeof(AppGlobalConfig));

		//			//�ǂݍ���ŋt�V���A��������
		//			Object obj = xs.Deserialize(fs);
		//			return obj;
		//		}
		//	}
		//	return null;
		//}

		//public void SaveToXmlFile(object obj)
		//{
		//	string path = getConfigFileName();

		//	using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
		//	{
		//		XmlSerializer xs = new XmlSerializer(typeof(AppGlobalConfig));
		//		//�V���A�������ď�������
		//		xs.Serialize(fs, obj);
		//	}
		//}

		private void applySettingToApplication()
		{
			//�o�[�֘A
			menuStrip1.Visible = g_Config.visibleMenubar;
			toolStrip1.Visible = g_Config.visibleToolBar;
			statusbar.Visible = g_Config.visibleStatusBar;

			//�i�r�o�[
			//g_Sidebar.SetSizeAndDock(GetClientRectangle());
			g_Sidebar.Visible = g_Config.visibleNavibar;

			//ver1.77 ��ʈʒu����F�f���A���f�B�X�v���C�Ή�
			if(g_Config.simpleCalcForWindowLocation)
			{
				//�ȈՁFas is
				this.Size = g_Config.windowSize;
				this.Location = g_Config.windowLocation;
			}
			else
				SetFormPosLocation();

			//ver1.77�S��ʃ��[�h�Ή�
			if (g_Config.saveFullScreenMode && g_Config.isFullScreen)
				SetFullScreen(true);

			//2���\��
			toolButtonDualMode.Checked = g_Config.dualView;

			//MRU���f
			//�I�[�v������Ƃ��Ɏ��{����̂ŃR�����g�A�E�g
			//UpdateMruMenuListUI();

			//�ċA����
			Menu_OptionRecurseDir.Checked = g_Config.isRecurseSearchDir;

			//���E�������Ή�
			if (g_Config.isReplaceArrowButton)
			{
				toolButtonLeft.Tag = "���̃y�[�W�Ɉړ����܂�";
				toolButtonLeft.Text = "����";
				toolButtonRight.Tag = "�O�̃y�[�W�Ɉړ����܂�";
				toolButtonRight.Text = "�O��";
			}
			else
			{
				toolButtonLeft.Tag = "�O�̃y�[�W�Ɉړ����܂�";
				toolButtonLeft.Text = "�O��";
				toolButtonRight.Tag = "���̃y�[�W�Ɉړ����܂�";
				toolButtonRight.Text = "����";
			}

			//�T���l�C���֘A
			if (g_ThumbPanel != null)
			{
				g_ThumbPanel.BackColor = g_Config.ThumbnailBackColor;
				g_ThumbPanel.SetThumbnailSize(g_Config.ThumbnailSize);
				g_ThumbPanel.SetFont(g_Config.ThumbnailFont, g_Config.ThumbnailFontColor);
			}

		}

		private void SetFormPosLocation()
		{
			//�f���A���f�B�X�v���C�Ή�
			//���オ��ʓ��ɂ���X�N���[����T��
			foreach (var scr in Screen.AllScreens)
			{
				if (scr.WorkingArea.Contains(g_Config.windowLocation))
				{
					setFormPosLocation2(scr);
					return;
				}
			}
			//�����ɗ������͂ǂ̃f�B�X�v���C�ɂ������Ȃ������Ƃ�

			//�ǂ̉�ʂɂ������Ȃ��̂Ńv���C�}���ɍs���Ă��炤
			//setFormPosLocation2(Screen.PrimaryScreen);
			//return;
			//�ǂ̉�ʂɂ������Ȃ��̂ň�ԋ߂��f�B�X�v���C��T��
			var pos = g_Config.windowLocation;
			double distance = double.MaxValue;
			int target = 0;
			for (int i = 0; i < Screen.AllScreens.Length;i++ )
			{
				var scr = Screen.AllScreens[i];
				//�ȈՌv�Z
				var d = Math.Abs(pos.X - scr.Bounds.X) + Math.Abs(pos.Y - scr.Bounds.Y);
				if (d < distance)
				{
					distance = d;
					target = i;
				}
			}
			setFormPosLocation2(Screen.AllScreens[target]);
			return;
			

		}

		/// <summary>
		/// g_Config�̓��e����\���ʒu�����肷��
		/// �f���A���f�B�X�v���C�ɑΉ�
		/// ��ʊO�ɕ\�������Ȃ��B
		/// </summary>
		/// <param name="scr"></param>
		private void setFormPosLocation2(Screen scr)
		{
			//���̃X�N���[���̃��[�L���O�G���A���`�F�b�N����
			var disp = scr.WorkingArea;

			//ver1.77 �E�B���h�E�T�C�Y�̒���(����������Ƃ��j
			if (g_Config.windowSize.Width < this.MinimumSize.Width)
				g_Config.windowSize.Width = this.MinimumSize.Width;
			if (g_Config.windowSize.Height < this.MinimumSize.Height)
				g_Config.windowSize.Height = this.MinimumSize.Height;

			//�E�B���h�E�T�C�Y�̒���(�傫������Ƃ��j
			if (disp.Width < g_Config.windowSize.Width)
			{
				g_Config.windowLocation.X = 0;
				g_Config.windowSize.Width = disp.Width;
			}
			if (disp.Height < g_Config.windowSize.Height)
			{
				g_Config.windowLocation.Y = 0;
				g_Config.windowSize.Height = disp.Height;
			}

			//�E�B���h�E�ʒu�̒����i��ʊO:�}�C�i�X�����j
			if (g_Config.windowLocation.X < disp.X)
				g_Config.windowLocation.X = disp.X;
			if (g_Config.windowLocation.Y < disp.Y)
				g_Config.windowLocation.Y = disp.Y;

			//�E������ʊO�ɕ\�������Ȃ�
			var right = g_Config.windowLocation.X + g_Config.windowSize.Width;
			var bottom = g_Config.windowLocation.Y + g_Config.windowSize.Height;
			if (right > disp.X + disp.Width)
				g_Config.windowLocation.X = disp.X + disp.Width - g_Config.windowSize.Width;
			if (bottom > disp.Y + disp.Height)
				g_Config.windowLocation.Y = disp.Y + disp.Height - g_Config.windowSize.Height;

			//�����\���������ǂ���
			if (g_Config.isWindowPosCenter)
			{
				g_Config.windowLocation.X = disp.X + (disp.Width - g_Config.windowSize.Width) / 2;
				g_Config.windowLocation.Y = disp.Y + (disp.Height - g_Config.windowSize.Height) / 2;
			}
			//�T�C�Y�̓K�p
			this.Size = g_Config.windowSize;
			//���������\��
			this.Location = g_Config.windowLocation;
		}

		private void SetWindowPosSize_old()
		{
			//�f�B�X�v���C�̃T�C�Y���擾
			System.Drawing.Size displaySize = Screen.PrimaryScreen.Bounds.Size;

			//ver1.72 �\���T�C�Y�͊o���Ă���

			//ver1.77 �E�B���h�E�T�C�Y�̒���(����������Ƃ��j
			// �ŏ��T�C�Y�ȉ��ɂ����Ȃ��B
			if (g_Config.windowSize.Width < this.MinimumSize.Width)
				g_Config.windowSize.Width = this.MinimumSize.Width;
			if (g_Config.windowSize.Height < this.MinimumSize.Height)
				g_Config.windowSize.Height = this.MinimumSize.Height;


			//�E�B���h�E�T�C�Y�̒���(�傫������Ƃ��j
			if (displaySize.Width < g_Config.windowSize.Width)
			{
				//�n�_��0�A������ʕ���
				g_Config.windowLocation.X = 0;
				g_Config.windowSize.Width = displaySize.Width;
			}
			if (displaySize.Height < g_Config.windowSize.Height)
			{
				//�n�_��0�A��������ʍ���
				g_Config.windowLocation.Y = 0;
				g_Config.windowSize.Height = displaySize.Height;
			}

			//�T�C�Y�̓K�p
			this.Size = g_Config.windowSize;

			//�E�B���h�E�ʒu�̒����i��ʊO:�}�C�i�X�����j
			if (g_Config.windowLocation.X < 0) g_Config.windowLocation.X = 0;
			if (g_Config.windowLocation.Y < 0) g_Config.windowLocation.Y = 0;

			//�E�B���h�E�ʒu�̒����i��ʊO:�v���X�����j
			//int maxWidth = displaySize.Width;
			//int maxHeight = displaySize.Height;
			//�f�B�X�v���C�͂����Ɖ��ɕ��ׂĂ���I
			var maxWidth = Screen.AllScreens.Sum(c => c.Bounds.Width);
			//�c�ɂ͂Ȃ�ׂȂ�
			var maxHeight = Screen.AllScreens.Max(c => c.Bounds.Height);
			//if (System.Windows.Forms.Screen.AllScreens.Length > 1)
			//{
			//	//�f���A���f�B�X�v���C�l��
			//	//�f�B�X�v���C�͂����Ɖ��ɕ��ׂĂ���I
			//	maxWidth = Screen.AllScreens.Sum(c => c.Bounds.Width);
			//	//�c�ɂ͂Ȃ�ׂȂ�
			//	maxHeight = Screen.AllScreens.Max(c => c.Bounds.Height);
			//}

			//��ʊO�ɕ\�������Ȃ�
			if (g_Config.windowLocation.X > maxWidth)
				g_Config.windowLocation.X = maxWidth - g_Config.windowSize.Width;
			if (g_Config.windowLocation.Y > maxHeight)
				g_Config.windowLocation.Y = maxHeight - g_Config.windowSize.Height;


			//�����\���������ǂ���
			if (g_Config.isWindowPosCenter)
			{
				int x = (displaySize.Width - g_Config.windowSize.Width) / 2;
				int y = (displaySize.Height - g_Config.windowSize.Height) / 2;
				//���������\��
				this.Location = new System.Drawing.Point(x, y);
			}
			else
				//{
				//	//�\���ʒu�̒���
				//	if (g_Config.windowLocation.X >= 0)
				//		g_Config.windowLocation.X = 0;
				//	if (g_Config.windowLocation.Y >= 0)
				//		g_Config.windowLocation.Y = 0;


				//	//�\���ʒu�ݒ�
				//	if (g_Config.windowLocation.X >= 0
				//		&& g_Config.windowLocation.Y >= 0
				//		)
				//	{
				//		this.Location = g_Config.windowLocation;
				//	}
				//	else
				//		//�����ɕ\�����悤
				//		this.Location = new System.Drawing.Point(
				//				(displaySize.Width - g_Config.windowSize.Width) / 2,
				//				(displaySize.Height - g_Config.windowSize.Height) / 2);
				//}
				this.Location = g_Config.windowLocation;
		}

		private void applySettingToConfig()
		{
			g_Config.windowLocation = this.Location;
			g_Config.windowSize = this.Size;

			//Debug.Assert(this.Location.X >= 0);
			//Debug.Assert(this.Location.Y >= 0);

			//g_mySetting.formStartPos = this.StartPosition;
			//mySetting.fullScreen = toolButtonFullScreen.Checked;
			//g_mySetting.dualView = toolButtonDualMode.Checked;
		}


		// ���[�e�B���e�B�n�F�o�C�i���V���A���C�Y ***************************************/

		private object LoadBinary(string path)
		{
			FileStream fs = new FileStream(
				path,
				FileMode.Open,
				FileAccess.Read);
			BinaryFormatter f = new BinaryFormatter();
			//�ǂݍ���ŋt�V���A��������
			object obj = f.Deserialize(fs);
			fs.Close();

			return obj;
		}

		private void SaveBinary(object obj, string path)
		{
			FileStream fs = new FileStream(path,
				FileMode.Create,
				FileAccess.Write);
			BinaryFormatter bf = new BinaryFormatter();
			//�V���A�������ď�������
			bf.Serialize(fs, obj);
			fs.Close();
		}

		private void SaveCompressedBinay(string filePath, object obj)
		{
			using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				using (DeflateStream ds = new DeflateStream(stream, CompressionMode.Compress, true))
				{
					IFormatter formatter = new BinaryFormatter();
					formatter.Serialize(ds, obj);
				}
			}
		}

		private void SaveCompressedBinay2(string filePath, object obj)
		{
			byte[] buffer;
			using (MemoryStream ms = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				buffer = ms.ToArray();
			}
			using (MemoryStream ms = new MemoryStream())
			{
				using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress, true))
				{
					ds.Write(buffer, 0, buffer.Length);
				}
				buffer = ms.ToArray();
			}
			using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				stream.Write(buffer, 0, buffer.Length);
			}

		}

		private object LoadCompressedBinary(string filePath)
		{
			object obj;
			using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				using (DeflateStream ds = new DeflateStream(stream, CompressionMode.Decompress))
				{
					IFormatter formatter = new BinaryFormatter();
					obj = formatter.Deserialize(ds);
				}
			}
			return obj;
		}

		private void saveThumbnailDBFile(PackageInfo savedata)
		{
			//�X���b�h���쒆�͕ۑ������Ȃ�
			if (tsThumbnail == ThreadStatus.RUNNING)
				return;

			//zip�ȊO�͑ΏۊO
			//if (!savedata.isZip)
			if (savedata.packType != PackageType.Archive)
				return;

			//�p�b�P�[�W�����`�F�b�N
			if (string.IsNullOrEmpty(savedata.PackageName))
				return;

			//if (string.Compare(Path.GetExtension(g_pi.PackageName), ".zip", true) != 0)
			if (!Uty.isAvailableArchiveFile(savedata.PackageName))
				return;

			//�ۑ��Ώۂ����邱�Ƃ��`�F�b�N
			if (savedata.Items.Count <= 0)
				return;

			//�ۑ�����
			string filename = getThumbnailDBFilename(savedata.PackageName);
			//SaveCompressedBinay(filename, m_thumbnailSet);	//���k�ۑ�
			//SaveCompressedBinay2(filename, g_pi.Items);	//���k�ۑ�
			SaveBinary(savedata, filename);			//�ʏ�ۑ�
		}

		private void loadThumbnailDBFile()
		{
			string filename = getThumbnailDBFilename(g_pi.PackageName);
			if (File.Exists(filename))
			{
				//List<ThumbnailImage> tmp = (List<ThumbnailImage>)LoadCompressedBinary(filename);
				//List<ImageInfo> tmp = (List<ImageInfo>)LoadBinary(filename);
				PackageInfo tmp = (PackageInfo)LoadBinary(filename);

				//ver1.41 �L���b�V���p�N���X�𐶐�
				//tmp.InitCache();

				//m_thumbnailSet�ƃt�@�C����r
				if (g_pi.Items.Count != tmp.Items.Count)
					return;

				//�t�@�C���T�C�Y���r
				FileInfo fi = new FileInfo(g_pi.PackageName);
				if (g_pi.size != fi.Length)
					return;

				for (int i = 0; i < g_pi.Items.Count; i++)
				{
					if (searchThumbnailInFile(g_pi.Items[i].filename, tmp.Items) == false)
						return;	//�ǂݍ��񂾕��͈Ⴄ�t�@�C���݂���
				}

				//���S�Ɉ�v�����ƍl����
				g_pi.Items.Clear();
				g_pi = tmp;
			}
		}

		private string getThumbnailDBFilename(string packagename)
		{
			string dbName = Path.GetFileName(packagename) + CACHEEXT;
			return Path.Combine(Application.StartupPath, dbName);
		}

		private bool searchThumbnailInFile(string searchName, List<ImageInfo> list)
		{
			foreach (ImageInfo ti in list)
			{
				if (ti.filename == searchName)
					return true;
			}
			return false;
		}

	}
}

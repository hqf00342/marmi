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
		// ユーティリティ系：Configファイル *********************************************/

		//
		//AppGlobalConfig.csへ移動。ｓｔａｔｉｃメソッドにした。
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

		//			//読み込んで逆シリアル化する
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
		//		//シリアル化して書き込む
		//		xs.Serialize(fs, obj);
		//	}
		//}

		private void applySettingToApplication()
		{
			//バー関連
			menuStrip1.Visible = g_Config.visibleMenubar;
			toolStrip1.Visible = g_Config.visibleToolBar;
			statusbar.Visible = g_Config.visibleStatusBar;

			//ナビバー
			//g_Sidebar.SetSizeAndDock(GetClientRectangle());
			g_Sidebar.Visible = g_Config.visibleNavibar;

			//ver1.77 画面位置決定：デュアルディスプレイ対応
			if(g_Config.simpleCalcForWindowLocation)
			{
				//簡易：as is
				this.Size = g_Config.windowSize;
				this.Location = g_Config.windowLocation;
			}
			else
				SetFormPosLocation();

			//ver1.77全画面モード対応
			if (g_Config.saveFullScreenMode && g_Config.isFullScreen)
				SetFullScreen(true);

			//2枚表示
			toolButtonDualMode.Checked = g_Config.dualView;

			//MRU反映
			//オープンするときに実施するのでコメントアウト
			//UpdateMruMenuListUI();

			//再帰検索
			Menu_OptionRecurseDir.Checked = g_Config.isRecurseSearchDir;

			//左右矢印交換対応
			if (g_Config.isReplaceArrowButton)
			{
				toolButtonLeft.Tag = "次のページに移動します";
				toolButtonLeft.Text = "次へ";
				toolButtonRight.Tag = "前のページに移動します";
				toolButtonRight.Text = "前へ";
			}
			else
			{
				toolButtonLeft.Tag = "前のページに移動します";
				toolButtonLeft.Text = "前へ";
				toolButtonRight.Tag = "次のページに移動します";
				toolButtonRight.Text = "次へ";
			}

			//サムネイル関連
			if (g_ThumbPanel != null)
			{
				g_ThumbPanel.BackColor = g_Config.ThumbnailBackColor;
				g_ThumbPanel.SetThumbnailSize(g_Config.ThumbnailSize);
				g_ThumbPanel.SetFont(g_Config.ThumbnailFont, g_Config.ThumbnailFontColor);
			}

		}

		private void SetFormPosLocation()
		{
			//デュアルディスプレイ対応
			//左上が画面内にいるスクリーンを探す
			foreach (var scr in Screen.AllScreens)
			{
				if (scr.WorkingArea.Contains(g_Config.windowLocation))
				{
					setFormPosLocation2(scr);
					return;
				}
			}
			//ここに来た時はどのディスプレイにも属さなかったとき

			//どの画面にも属さないのでプライマリに行ってもらう
			//setFormPosLocation2(Screen.PrimaryScreen);
			//return;
			//どの画面にも属さないので一番近いディスプレイを探す
			var pos = g_Config.windowLocation;
			double distance = double.MaxValue;
			int target = 0;
			for (int i = 0; i < Screen.AllScreens.Length;i++ )
			{
				var scr = Screen.AllScreens[i];
				//簡易計算
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
		/// g_Configの内容から表示位置を決定する
		/// デュアルディスプレイに対応
		/// 画面外に表示させない。
		/// </summary>
		/// <param name="scr"></param>
		private void setFormPosLocation2(Screen scr)
		{
			//このスクリーンのワーキングエリアをチェックする
			var disp = scr.WorkingArea;

			//ver1.77 ウィンドウサイズの調整(小さすぎるとき）
			if (g_Config.windowSize.Width < this.MinimumSize.Width)
				g_Config.windowSize.Width = this.MinimumSize.Width;
			if (g_Config.windowSize.Height < this.MinimumSize.Height)
				g_Config.windowSize.Height = this.MinimumSize.Height;

			//ウィンドウサイズの調整(大きすぎるとき）
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

			//ウィンドウ位置の調整（画面外:マイナス方向）
			if (g_Config.windowLocation.X < disp.X)
				g_Config.windowLocation.X = disp.X;
			if (g_Config.windowLocation.Y < disp.Y)
				g_Config.windowLocation.Y = disp.Y;

			//右下も画面外に表示させない
			var right = g_Config.windowLocation.X + g_Config.windowSize.Width;
			var bottom = g_Config.windowLocation.Y + g_Config.windowSize.Height;
			if (right > disp.X + disp.Width)
				g_Config.windowLocation.X = disp.X + disp.Width - g_Config.windowSize.Width;
			if (bottom > disp.Y + disp.Height)
				g_Config.windowLocation.Y = disp.Y + disp.Height - g_Config.windowSize.Height;

			//中央表示強制かどうか
			if (g_Config.isWindowPosCenter)
			{
				g_Config.windowLocation.X = disp.X + (disp.Width - g_Config.windowSize.Width) / 2;
				g_Config.windowLocation.Y = disp.Y + (disp.Height - g_Config.windowSize.Height) / 2;
			}
			//サイズの適用
			this.Size = g_Config.windowSize;
			//強制中央表示
			this.Location = g_Config.windowLocation;
		}

		private void SetWindowPosSize_old()
		{
			//ディスプレイのサイズを取得
			System.Drawing.Size displaySize = Screen.PrimaryScreen.Bounds.Size;

			//ver1.72 表示サイズは覚えておく

			//ver1.77 ウィンドウサイズの調整(小さすぎるとき）
			// 最小サイズ以下にさせない。
			if (g_Config.windowSize.Width < this.MinimumSize.Width)
				g_Config.windowSize.Width = this.MinimumSize.Width;
			if (g_Config.windowSize.Height < this.MinimumSize.Height)
				g_Config.windowSize.Height = this.MinimumSize.Height;


			//ウィンドウサイズの調整(大きすぎるとき）
			if (displaySize.Width < g_Config.windowSize.Width)
			{
				//始点を0、幅を画面幅に
				g_Config.windowLocation.X = 0;
				g_Config.windowSize.Width = displaySize.Width;
			}
			if (displaySize.Height < g_Config.windowSize.Height)
			{
				//始点を0、高さを画面高に
				g_Config.windowLocation.Y = 0;
				g_Config.windowSize.Height = displaySize.Height;
			}

			//サイズの適用
			this.Size = g_Config.windowSize;

			//ウィンドウ位置の調整（画面外:マイナス方向）
			if (g_Config.windowLocation.X < 0) g_Config.windowLocation.X = 0;
			if (g_Config.windowLocation.Y < 0) g_Config.windowLocation.Y = 0;

			//ウィンドウ位置の調整（画面外:プラス方向）
			//int maxWidth = displaySize.Width;
			//int maxHeight = displaySize.Height;
			//ディスプレイはきっと横に並べている！
			var maxWidth = Screen.AllScreens.Sum(c => c.Bounds.Width);
			//縦にはならべない
			var maxHeight = Screen.AllScreens.Max(c => c.Bounds.Height);
			//if (System.Windows.Forms.Screen.AllScreens.Length > 1)
			//{
			//	//デュアルディスプレイ考慮
			//	//ディスプレイはきっと横に並べている！
			//	maxWidth = Screen.AllScreens.Sum(c => c.Bounds.Width);
			//	//縦にはならべない
			//	maxHeight = Screen.AllScreens.Max(c => c.Bounds.Height);
			//}

			//画面外に表示させない
			if (g_Config.windowLocation.X > maxWidth)
				g_Config.windowLocation.X = maxWidth - g_Config.windowSize.Width;
			if (g_Config.windowLocation.Y > maxHeight)
				g_Config.windowLocation.Y = maxHeight - g_Config.windowSize.Height;


			//中央表示強制かどうか
			if (g_Config.isWindowPosCenter)
			{
				int x = (displaySize.Width - g_Config.windowSize.Width) / 2;
				int y = (displaySize.Height - g_Config.windowSize.Height) / 2;
				//強制中央表示
				this.Location = new System.Drawing.Point(x, y);
			}
			else
				//{
				//	//表示位置の調整
				//	if (g_Config.windowLocation.X >= 0)
				//		g_Config.windowLocation.X = 0;
				//	if (g_Config.windowLocation.Y >= 0)
				//		g_Config.windowLocation.Y = 0;


				//	//表示位置設定
				//	if (g_Config.windowLocation.X >= 0
				//		&& g_Config.windowLocation.Y >= 0
				//		)
				//	{
				//		this.Location = g_Config.windowLocation;
				//	}
				//	else
				//		//中央に表示しよう
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


		// ユーティリティ系：バイナリシリアライズ ***************************************/

		private object LoadBinary(string path)
		{
			FileStream fs = new FileStream(
				path,
				FileMode.Open,
				FileAccess.Read);
			BinaryFormatter f = new BinaryFormatter();
			//読み込んで逆シリアル化する
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
			//シリアル化して書き込む
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
			//スレッド動作中は保存させない
			if (tsThumbnail == ThreadStatus.RUNNING)
				return;

			//zip以外は対象外
			//if (!savedata.isZip)
			if (savedata.packType != PackageType.Archive)
				return;

			//パッケージ名をチェック
			if (string.IsNullOrEmpty(savedata.PackageName))
				return;

			//if (string.Compare(Path.GetExtension(g_pi.PackageName), ".zip", true) != 0)
			if (!Uty.isAvailableArchiveFile(savedata.PackageName))
				return;

			//保存対象があることをチェック
			if (savedata.Items.Count <= 0)
				return;

			//保存する
			string filename = getThumbnailDBFilename(savedata.PackageName);
			//SaveCompressedBinay(filename, m_thumbnailSet);	//圧縮保存
			//SaveCompressedBinay2(filename, g_pi.Items);	//圧縮保存
			SaveBinary(savedata, filename);			//通常保存
		}

		private void loadThumbnailDBFile()
		{
			string filename = getThumbnailDBFilename(g_pi.PackageName);
			if (File.Exists(filename))
			{
				//List<ThumbnailImage> tmp = (List<ThumbnailImage>)LoadCompressedBinary(filename);
				//List<ImageInfo> tmp = (List<ImageInfo>)LoadBinary(filename);
				PackageInfo tmp = (PackageInfo)LoadBinary(filename);

				//ver1.41 キャッシュ用クラスを生成
				//tmp.InitCache();

				//m_thumbnailSetとファイル比較
				if (g_pi.Items.Count != tmp.Items.Count)
					return;

				//ファイルサイズを比較
				FileInfo fi = new FileInfo(g_pi.PackageName);
				if (g_pi.size != fi.Length)
					return;

				for (int i = 0; i < g_pi.Items.Count; i++)
				{
					if (searchThumbnailInFile(g_pi.Items[i].filename, tmp.Items) == false)
						return;	//読み込んだ物は違うファイルみたい
				}

				//完全に一致したと考える
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

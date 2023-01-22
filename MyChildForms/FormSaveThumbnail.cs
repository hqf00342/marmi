using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace Marmi
{
	public partial class FormSaveThumbnail : Form
	{
		//const int DEFAULT_THUMBNAIL_SIZE = 120; //Form1.DEFAULT_THUMBNAIL_SIZEに統合;
		const int DEFAULT_VERTICAL_WIDTH = 640;

		private bool saveConf_isDrawFilename;		//グローバルコンフィグの一時待避領域
		private bool saveConf_isDrawFileSize;		//グローバルコンフィグの一時待避領域
		private bool saveConf_isDrawPicSize;		//グローバルコンフィグの一時待避領域

		string savename;							//保存ファイル名
		private List<ImageInfo> m_thumbnailSet;		//リストへのポインタ
		private ThumbnailPanel m_tPanel = null;		//親のパネルを表す
		private bool m_Saving = false;				//保存処理中かどうかを表すフラグ

		public bool isCancel
		{
			get { return !m_Saving; }
		}


		public FormSaveThumbnail(ThumbnailPanel tp, List<ImageInfo> lii, string Filename)
		{
			InitializeComponent();
			m_thumbnailSet = lii;
			m_tPanel = tp;
			m_tPanel.SavedItemChanged += new ThumbnailPanel.ThumbnailEventHandler(tPanel_SavedItemChanged);

			//保存ファイル名
			savename = suggestFilename(Filename);

			//グローバルコンフィグを一時保存
			saveConf_isDrawFilename = Form1.g_Config.isShowTPFileName;
			saveConf_isDrawFileSize = Form1.g_Config.isShowTPFileSize;
			saveConf_isDrawPicSize = Form1.g_Config.isShowTPPicSize;
		}


		~FormSaveThumbnail()
		{
			m_tPanel.SavedItemChanged -= new ThumbnailPanel.ThumbnailEventHandler(tPanel_SavedItemChanged);
		}

		private void FormSaveThumbnail_Load(object sender, EventArgs e)
		{
			//テキストボックスの初期化：画像サイズ、画像個数
			tbPixels.Text = Form1.DEFAULT_THUMBNAIL_SIZE.ToString();
			int vertical = DEFAULT_VERTICAL_WIDTH / Form1.DEFAULT_THUMBNAIL_SIZE;
			tbnItemX.Text = vertical.ToString();


			//プログレスバーの初期化
			tsProgressBar1.Visible = false;

			//チェックボックスの初期化
			isDrawFileName.Checked = true;
			isDrawFileSize.Checked = false;
			isDrawPicSize.Checked = false;
		}

		private void FormSaveThumbnail_FormClosed(object sender, FormClosedEventArgs e)
		{
			//グローバルコンフィグを元に戻す
			Form1.g_Config.isShowTPFileName = saveConf_isDrawFilename;
			Form1.g_Config.isShowTPFileSize = saveConf_isDrawFileSize;
			Form1.g_Config.isShowTPPicSize = saveConf_isDrawPicSize;

		}

		private void btCancel_Click(object sender, EventArgs e)
		{
			if (m_Saving)
			{
				m_Saving = false;
				btExcute.Enabled = true;
				tbPixels.Enabled = true;
				tbnItemX.Enabled = true;
			}
			else
			{
				this.Close();
			}
		}


		private void btExcute_Click(object sender, EventArgs e)
		{
			//サムネイルサイズの設定
			int ThumbnailSize = 0;
			if (!Int32.TryParse(tbPixels.Text, out ThumbnailSize))
				ThumbnailSize = Form1.DEFAULT_THUMBNAIL_SIZE;
			tbPixels.Text = ThumbnailSize.ToString();

			//横に並ぶ個数の設定
			int nItemX = 0;
			if(!Int32.TryParse(tbnItemX.Text, out nItemX))
				nItemX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
			tbnItemX.Text = nItemX.ToString();

			//ファイル名の確認
			SaveFileDialog sf = new SaveFileDialog();
			sf.AddExtension = true;
			sf.DefaultExt = "png";
			sf.FileName = savename;
			sf.InitialDirectory = Path.GetDirectoryName(savename);
			sf.Filter = "pngファイル|*.png|全てのファイル|*.*";
			sf.FilterIndex = 1;
			sf.OverwritePrompt = true;
			if (sf.ShowDialog() == DialogResult.OK)
				savename = sf.FileName;
			else
				return;	//キャンセル相当


			tbInfo.Text += "保存先 : " + savename + "\r\n"
						+"アイテム数 : " + m_thumbnailSet.Count + "\r\n";

			tsProgressBar1.Minimum = 0;
			tsProgressBar1.Maximum = m_thumbnailSet.Count -1;	//0始まり
			tsProgressBar1.Value = 0;
			tsProgressBar1.Visible = true;

			//グローバルコンフィグを一時的に変更
			//FormClosed()で元に戻す
			Form1.g_Config.isShowTPFileName = isDrawFileName.Checked;
			Form1.g_Config.isShowTPFileSize = isDrawFileSize.Checked;
			Form1.g_Config.isShowTPPicSize = isDrawPicSize.Checked;

			//サムネイルを保存する
			btExcute.Enabled = false;
			tbPixels.Enabled = false;
			tbnItemX.Enabled = false;
			m_Saving = true;
			m_tPanel.SaveThumbnailImage(ThumbnailSize, nItemX, savename);
			this.Close();
		}


		private string suggestFilename(string orgName)
		{
			//拡張子pngを提案する。

			//何もないときはデスクトップ/thumbnaul.pngを提案
			if (string.IsNullOrEmpty(orgName))
			{
				string sz = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				sz += @"\Thumbnail.png";
				return sz;
			}

			//拡張子を切り出し
			string suggest = Path.Combine(
				Path.GetDirectoryName(orgName), Path.GetFileNameWithoutExtension(orgName));
			suggest += ".png";
			return suggest;
		}


		void tPanel_SavedItemChanged(object obj, ThumbnailEventArgs e)
		{
			int num = e.HoverItemNumber;
			toolStripStatusLabel1.Text = string.Format("完了数 : {0} / {1}", num+1, m_thumbnailSet.Count);
			if(tsProgressBar1.Visible)
				tsProgressBar1.Value = num;

			if (num+1 >= m_thumbnailSet.Count)
			{
				//btExcute.Enabled = true;
				//tbPixels.Enabled = true;
				//tbVnum.Enabled = true;
				toolStripStatusLabel1.Text = "完了しました";
			}
		}


		private void textbox_TextChanged(object sender, EventArgs e)
		{
			//サムネイルサイズの設定
			int ThumbnailSize = 0;
			if (!Int32.TryParse(tbPixels.Text, out ThumbnailSize))
				ThumbnailSize = Form1.DEFAULT_THUMBNAIL_SIZE;
			tbPixels.Text = ThumbnailSize.ToString();

			//横に並ぶ個数の設定
			int nItemsX = 0;
			if (!Int32.TryParse(tbnItemX.Text, out nItemsX))
				nItemsX = DEFAULT_VERTICAL_WIDTH / ThumbnailSize;
			tbnItemX.Text = nItemsX.ToString();

			//Bitmapの想定サイズを計算
			int ItemCount = m_thumbnailSet.Count;
			int nItemsY = ItemCount / nItemsX;	//縦に並ぶアイテム数はサムネイルの数による
			if (ItemCount % nItemsX > 0)		//割り切れなかった場合は1行追加
				nItemsY++;

			tbInfo.Text = string.Format("出力画像サイズ : {0:N0} x {1:N0} [pixels]\r\n",
				nItemsX * ThumbnailSize, nItemsY * ThumbnailSize);

		}


	}
}
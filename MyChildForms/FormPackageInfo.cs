using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormPackageInfo : Form
    {
        private readonly Form1 m_parent;             //親フォーム
        private readonly PackageInfo m_packageInfo;  //g_piそのものを挿す

        //定数
        //リストボックスの高さはTHUMBSIZE + PADDING * 2;
        private const int PADDING = 2;        //定数：画像の上下パディング

        private const int NUM_WIDTH = 30;     //定数：画像番号表示幅
        private const int THUMBSIZE = 60;     //定数：サムネイルサイズ

        //描写用オブジェクト
        private Font fontL = null;

        private Font fontS = null;

        public FormPackageInfo(Form1 Parent, PackageInfo packageInfo)
        {
            //リサイズ用のグリップを表示
            this.SizeGripStyle = SizeGripStyle.Show;

            m_parent = Parent;
            m_packageInfo = packageInfo;
            InitializeComponent();

            LoadPackageInfo();
        }

        private void PackageInfoForm_Load(object sender, EventArgs e)
        {
            fontL = new Font("ＭＳ Ｐ ゴシック", 10.5F);
            fontS = new Font("ＭＳ Ｐ ゴシック", 9F);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //fontL.Dispose();
            //fontS.Dispose();
        }

        private void LoadPackageInfo()
        {
            //パッケージ情報の設定
            if (m_packageInfo.PackType == PackageType.Archive)//(m_packageInfo.isZip)
            {
                //Zipファイル
                pictureBox1.Image = Properties.Resources.zippedFile;
                textBox1.Lines = new string[]{
                    string.Format("Zipファイル名 \t: {0}", Path.GetFileName(m_packageInfo.PackageName)),
                    string.Format("ファイルパス  \t: {0}", m_packageInfo.PackageName),
                    string.Format("ファイルサイズ\t: {0:N0}", m_packageInfo.PackageSize),
                    string.Format("画像ファイル数\t: {0}", m_packageInfo.Items.Count),
                };
            }
            else if (m_packageInfo.PackageName != null)
            {
                pictureBox1.Image = Properties.Resources.Folder_Open;
                textBox1.Lines = new string[]{
                    string.Format("フォルダ名    \t: {0}", Path.GetFileName(m_packageInfo.PackageName)),
                    string.Format("ファイルパス  \t: {0}", m_packageInfo.PackageName),
                    string.Format("画像ファイル数\t: {0}", m_packageInfo.Items.Count),
                };
            }
            else
            {
                pictureBox1.Image = Properties.Resources.Image_File;
                textBox1.Lines = new string[]{
                    "画像ファイル",
                    string.Format("画像ファイル数\t: {0}", m_packageInfo.Items.Count),
                };
            }

            //画像情報の設定
            for (int i = 0; i < m_packageInfo.Items.Count; i++)
            {
                listBox1.Items.Add(m_packageInfo.Items[i]);
                //m_packageInfo.Items[i].OrgIndex = i; //2021年2月24日コメントアウト。不要なはず      //元の順序を保存しておく
            }
        }

        //ver1.11　使われていないのでコメントアウト
        //public void Show(int page)
        //{
        //    this.Show(m_parent);
        //    if (page >= 0 && page < listBox1.Items.Count)
        //    {
        //        listBox1.SelectedIndex = page;
        //        listBox1.TopIndex = page;
        //    }
        //}

        /// <summary>
        /// ダイアログ表示
        /// パッケージ情報として、もしくはソート用として表示
        /// </summary>
        /// <param name="page">選択状態にしておくページ番号</param>
        public void ShowDialog(int page)
        {
            if (page >= 0 && page < listBox1.Items.Count)
            {
                listBox1.SelectedIndex = page;
                listBox1.TopIndex = page;
            }
            this.ShowDialog(m_parent);
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ix = listBox1.SelectedIndex;
            //pictureBox2.Image = m_packageInfo.Items[ix].ThumbImage;

            if (checkBoxChangeMainWindow.Checked)
            {
                m_parent.SetViewPageAsync(ix);
                //m_parent.setViewImage(((ImageInfo)(listBox1.Items[ix])).nOrgIndex);
            }

            //選択色を更新するためInvalidate()
            //背景色を自前描写しているために必要
            //listBox1.Invalidate();
            listBox1.Refresh();
        }

        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //インデックスが範囲内かチェック
            if (e.Index < 0 || e.Index >= m_packageInfo.Items.Count)
                return;

            Graphics g = e.Graphics;

            //背景色の描写。選択時の色も対応してくれる
            //e.DrawBackground();

            //背景色を自前で描写
            //表示中のモノは薄い水色で表示
            //選択中のアイテムは青で
            //それ以外は白で
            if (listBox1.SelectedIndex == e.Index)
                g.FillRectangle(Brushes.Lavender, e.Bounds);    //Brushes.LightBlue
            else if (m_packageInfo.NowViewPage == e.Index)
                g.FillRectangle(Brushes.AliceBlue, e.Bounds);   //AliceBlue
            else if (m_parent.g_viewPages == 2 && m_packageInfo.NowViewPage + 1 == e.Index)
                g.FillRectangle(Brushes.AliceBlue, e.Bounds);
            else
                g.FillRectangle(Brushes.White, e.Bounds);

            //Font fontL = new Font("ＭＳ Ｐ ゴシック", 10.5F);
            //Font fontS = new Font("ＭＳ Ｐ ゴシック", 9F);
            //SolidBrush orangeBrush = new SolidBrush(Color.Orange);

            //通し番号の描写
            int x = e.Bounds.X + 2;
            int y = e.Bounds.Y + 20;
            string sz = string.Format("{0}", e.Index + 1);
            SizeF size = g.MeasureString(sz, fontL);
            int HeightL = (int)size.Height;
            size = g.MeasureString(sz, fontS);
            int HeightS = (int)size.Height;
            g.DrawString(sz, fontS, Brushes.DarkGray, x, y);

            //今回描写対象のアイテム
            ImageInfo ImgInfo = m_packageInfo.Items[e.Index];
            //ImageInfo ImgInfo = (ImageInfo)listBox1.Items[e.Index];

            //画像の描写
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            x = e.Bounds.X + PADDING + NUM_WIDTH;
            y = e.Bounds.Y + PADDING;
            if (ImgInfo.Thumbnail != null)
            {
                float mag = BitmapUty.GetMagnificationWithFixAspectRatio(ImgInfo.Thumbnail.Size, THUMBSIZE);
                int ThumbWidth = (int)(ImgInfo.Thumbnail.Width * mag);
                int ThumbHeight = (int)(ImgInfo.Thumbnail.Height * mag);

                //サムネイル画像の描写
                g.DrawImage(
                    ImgInfo.Thumbnail,
                    x + (THUMBSIZE - ThumbWidth) / 2,     // X位置（左上の）
                    y + (THUMBSIZE - ThumbHeight) / 2,      // Y位置（左上の）
                    ThumbWidth,
                    ThumbHeight
                    );

                //画像枠の描写
                g.DrawRectangle(
                    Pens.LightGray,
                    x + (THUMBSIZE - ThumbWidth) / 2,     // X位置（左上の）
                    y + (THUMBSIZE - ThumbHeight) / 2,      // Y位置（左上の）
                    ThumbWidth,
                    ThumbHeight);
            }

            //文字の描写:ファイル名
            x += PADDING + NUM_WIDTH + THUMBSIZE;
            sz = string.Format("{0}", Path.GetFileName(ImgInfo.Filename));
            g.DrawString(sz, fontL, Brushes.Black, x, y);
            y += HeightL + PADDING;

            //文字の描写:パス
            x += 10;
            sz = Path.GetDirectoryName(ImgInfo.Filename);
            if (!string.IsNullOrEmpty(sz))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisPath  //表示しきれないときは・・・表記
                };

                //描写サイズを確認
                size = g.MeasureString(sz, fontS, e.Bounds.Width - x, sf);
                Rectangle rc = new Rectangle(x, y, e.Bounds.Width - x, HeightS);
                g.DrawString(sz, fontS, Brushes.DarkGray, rc, sf);
                //y += (int)size.Height + PADDING;
                y += HeightS + PADDING;
            }

            //文字の描写:サイズ, 日付
            sz = string.Format(
                "{0:N0}bytes,   {1}",
                ImgInfo.FileLength,
                ImgInfo.CreateDate
                );
            g.DrawString(sz, fontS, Brushes.DarkGray, x, y);
            size = g.MeasureString(sz, fontS, e.Bounds.Width - x);
            x += (int)size.Width + PADDING;
            //y += HeightS + PADDING;

            //文字の描写:ピクセル数
            sz = string.Format(
                "{0:N0}x{1:N0}pixels",
                ImgInfo.Width,
                ImgInfo.Height
                );
            g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
            //y += HeightS + PADDING; //最後なので不要

            ////文字の描写:Exifその他
            //sz = string.Format(
            //    "{0} {1}",
            //    ImgInfo.ExifMake,
            //    ImgInfo.ExifModel);
            //if (ImgInfo.ExifISO != 0)
            //    sz = string.Format("ISO={0} {1}", ImgInfo.ExifISO, sz);
            //g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);

            g.DrawRectangle(Pens.LightGray, e.Bounds);
            e.DrawFocusRectangle();     //フォーカスがあるときに枠を描写
        }

        /// <summary>
        /// リストボックスの高さを返す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = THUMBSIZE + PADDING * 2;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonUp_Click(object sender, EventArgs e)
        {
            int ix = listBox1.SelectedIndex;

            if (ix > 0)
            {
                //Object temp = listBox1.Items[ix];
                //listBox1.Items[ix] = listBox1.Items[ix - 1];
                //listBox1.Items[ix - 1] = temp;

                ImageInfo t = m_packageInfo.Items[ix];
                m_packageInfo.Items[ix] = m_packageInfo.Items[ix - 1];
                m_packageInfo.Items[ix - 1] = t;
                listBox1.SelectedIndex = ix - 1;
                //listBox1.Invalidate();
            }
        }

        private void ButtonDown_Click(object sender, EventArgs e)
        {
            int ix = listBox1.SelectedIndex;

            if (ix < listBox1.Items.Count - 1)
            {
                //Object temp = listBox1.Items[ix];
                //listBox1.Items[ix] = listBox1.Items[ix + 1];
                //listBox1.Items[ix + 1] = temp;

                ImageInfo t = m_packageInfo.Items[ix];
                m_packageInfo.Items[ix] = m_packageInfo.Items[ix + 1];
                m_packageInfo.Items[ix + 1] = t;
                listBox1.SelectedIndex = ix + 1;
                //listBox1.Invalidate();
            }
        }

        private void ButtonSortByName_Click(object sender, EventArgs e)
        {
            var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
            m_packageInfo.Items.Sort(comparer);

            //ListBoxを更新
            listBox1.TabIndex = listBox1.SelectedIndex;
            listBox1.Refresh();

            //メイン画面を更新
            ListBox1_SelectedIndexChanged(null, null);
        }

        private void ButtonSortByDate_Click(object sender, EventArgs e)
        {
            var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
            m_packageInfo.Items.Sort(comparer);

            //ListBoxを更新
            listBox1.TabIndex = listBox1.SelectedIndex;
            listBox1.Refresh();

            //メイン画面を更新
            ListBox1_SelectedIndexChanged(null, null);
        }

        private void ButtonSortOrg_Click(object sender, EventArgs e)
        {
            var comparer = new ImageInfoComparer(ImageInfoComparer.Target.OriginalIndex);
            m_packageInfo.Items.Sort(comparer);

            //ListBoxを更新
            listBox1.TabIndex = listBox1.SelectedIndex;
            listBox1.Refresh();

            //メイン画面を更新
            ListBox1_SelectedIndexChanged(null, null);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            //元の順序にソート
            if (checkBoxSort.Enabled)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.OriginalIndex);
                m_packageInfo.Items.Sort(comparer);
            }
            this.Close();
        }

        private void CheckBoxSort_CheckedChanged(object sender, EventArgs e)
        {
            bool b = checkBoxSort.Checked;

            buttonUp.Enabled = b;
            buttonDown.Enabled = b;
            buttonSortOrg.Enabled = b;
            buttonSortByDate.Enabled = b;
            buttonSortByName.Enabled = b;
        }

        public void SetSortMode(bool canSort)
        {
            if (canSort)
            {
                checkBoxSort.Checked = true;
                checkBoxSort.Enabled = true;
            }
            else
            {
                checkBoxSort.Checked = false;
                checkBoxSort.Enabled = false;
            }
            CheckBoxSort_CheckedChanged(null, null);
        }
    }
}
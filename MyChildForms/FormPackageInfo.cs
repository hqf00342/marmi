using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormPackageInfo : Form
    {
        private readonly Form1 m_parent;             //�e�t�H�[��
        private readonly PackageInfo m_packageInfo;  //g_pi���̂��̂�}��

        //�萔
        //���X�g�{�b�N�X�̍�����THUMBSIZE + PADDING * 2;
        private const int PADDING = 2;        //�萔�F�摜�̏㉺�p�f�B���O

        private const int NUM_WIDTH = 30;     //�萔�F�摜�ԍ��\����
        private const int THUMBSIZE = 60;     //�萔�F�T���l�C���T�C�Y

        //�`�ʗp�I�u�W�F�N�g
        private Font fontL = null;

        private Font fontS = null;

        public FormPackageInfo(Form1 Parent, PackageInfo packageInfo)
        {
            //���T�C�Y�p�̃O���b�v��\��
            this.SizeGripStyle = SizeGripStyle.Show;

            m_parent = Parent;
            m_packageInfo = packageInfo;
            InitializeComponent();

            LoadPackageInfo();
        }

        private void PackageInfoForm_Load(object sender, EventArgs e)
        {
            fontL = new Font("�l�r �o �S�V�b�N", 10.5F);
            fontS = new Font("�l�r �o �S�V�b�N", 9F);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //fontL.Dispose();
            //fontS.Dispose();
        }

        private void LoadPackageInfo()
        {
            //�p�b�P�[�W���̐ݒ�
            if (m_packageInfo.PackType == PackageType.Archive)//(m_packageInfo.isZip)
            {
                //Zip�t�@�C��
                pictureBox1.Image = Properties.Resources.zippedFile;
                textBox1.Lines = new string[]{
                    string.Format("Zip�t�@�C���� \t: {0}", Path.GetFileName(m_packageInfo.PackageName)),
                    string.Format("�t�@�C���p�X  \t: {0}", m_packageInfo.PackageName),
                    string.Format("�t�@�C���T�C�Y\t: {0:N0}", m_packageInfo.PackageSize),
                    string.Format("�摜�t�@�C����\t: {0}", m_packageInfo.Items.Count),
                };
            }
            else if (m_packageInfo.PackageName != null)
            {
                pictureBox1.Image = Properties.Resources.Folder_Open;
                textBox1.Lines = new string[]{
                    string.Format("�t�H���_��    \t: {0}", Path.GetFileName(m_packageInfo.PackageName)),
                    string.Format("�t�@�C���p�X  \t: {0}", m_packageInfo.PackageName),
                    string.Format("�摜�t�@�C����\t: {0}", m_packageInfo.Items.Count),
                };
            }
            else
            {
                pictureBox1.Image = Properties.Resources.Image_File;
                textBox1.Lines = new string[]{
                    "�摜�t�@�C��",
                    string.Format("�摜�t�@�C����\t: {0}", m_packageInfo.Items.Count),
                };
            }

            //�摜���̐ݒ�
            for (int i = 0; i < m_packageInfo.Items.Count; i++)
            {
                listBox1.Items.Add(m_packageInfo.Items[i]);
                //m_packageInfo.Items[i].OrgIndex = i; //2021�N2��24���R�����g�A�E�g�B�s�v�Ȃ͂�      //���̏�����ۑ����Ă���
            }
        }

        //ver1.11�@�g���Ă��Ȃ��̂ŃR�����g�A�E�g
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
        /// �_�C�A���O�\��
        /// �p�b�P�[�W���Ƃ��āA�������̓\�[�g�p�Ƃ��ĕ\��
        /// </summary>
        /// <param name="page">�I����Ԃɂ��Ă����y�[�W�ԍ�</param>
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

            //�I��F���X�V���邽��Invalidate()
            //�w�i�F�����O�`�ʂ��Ă��邽�߂ɕK�v
            //listBox1.Invalidate();
            listBox1.Refresh();
        }

        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //�C���f�b�N�X���͈͓����`�F�b�N
            if (e.Index < 0 || e.Index >= m_packageInfo.Items.Count)
                return;

            Graphics g = e.Graphics;

            //�w�i�F�̕`�ʁB�I�����̐F���Ή����Ă����
            //e.DrawBackground();

            //�w�i�F�����O�ŕ`��
            //�\�����̃��m�͔������F�ŕ\��
            //�I�𒆂̃A�C�e���͐�
            //����ȊO�͔���
            if (listBox1.SelectedIndex == e.Index)
                g.FillRectangle(Brushes.Lavender, e.Bounds);    //Brushes.LightBlue
            else if (m_packageInfo.NowViewPage == e.Index)
                g.FillRectangle(Brushes.AliceBlue, e.Bounds);   //AliceBlue
            else if (m_parent.g_viewPages == 2 && m_packageInfo.NowViewPage + 1 == e.Index)
                g.FillRectangle(Brushes.AliceBlue, e.Bounds);
            else
                g.FillRectangle(Brushes.White, e.Bounds);

            //Font fontL = new Font("�l�r �o �S�V�b�N", 10.5F);
            //Font fontS = new Font("�l�r �o �S�V�b�N", 9F);
            //SolidBrush orangeBrush = new SolidBrush(Color.Orange);

            //�ʂ��ԍ��̕`��
            int x = e.Bounds.X + 2;
            int y = e.Bounds.Y + 20;
            string sz = string.Format("{0}", e.Index + 1);
            SizeF size = g.MeasureString(sz, fontL);
            int HeightL = (int)size.Height;
            size = g.MeasureString(sz, fontS);
            int HeightS = (int)size.Height;
            g.DrawString(sz, fontS, Brushes.DarkGray, x, y);

            //����`�ʑΏۂ̃A�C�e��
            ImageInfo ImgInfo = m_packageInfo.Items[e.Index];
            //ImageInfo ImgInfo = (ImageInfo)listBox1.Items[e.Index];

            //�摜�̕`��
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            x = e.Bounds.X + PADDING + NUM_WIDTH;
            y = e.Bounds.Y + PADDING;
            if (ImgInfo.Thumbnail != null)
            {
                float mag = BitmapUty.GetMagnificationWithFixAspectRatio(ImgInfo.Thumbnail.Size, THUMBSIZE);
                int ThumbWidth = (int)(ImgInfo.Thumbnail.Width * mag);
                int ThumbHeight = (int)(ImgInfo.Thumbnail.Height * mag);

                //�T���l�C���摜�̕`��
                g.DrawImage(
                    ImgInfo.Thumbnail,
                    x + (THUMBSIZE - ThumbWidth) / 2,     // X�ʒu�i����́j
                    y + (THUMBSIZE - ThumbHeight) / 2,      // Y�ʒu�i����́j
                    ThumbWidth,
                    ThumbHeight
                    );

                //�摜�g�̕`��
                g.DrawRectangle(
                    Pens.LightGray,
                    x + (THUMBSIZE - ThumbWidth) / 2,     // X�ʒu�i����́j
                    y + (THUMBSIZE - ThumbHeight) / 2,      // Y�ʒu�i����́j
                    ThumbWidth,
                    ThumbHeight);
            }

            //�����̕`��:�t�@�C����
            x += PADDING + NUM_WIDTH + THUMBSIZE;
            sz = string.Format("{0}", Path.GetFileName(ImgInfo.Filename));
            g.DrawString(sz, fontL, Brushes.Black, x, y);
            y += HeightL + PADDING;

            //�����̕`��:�p�X
            x += 10;
            sz = Path.GetDirectoryName(ImgInfo.Filename);
            if (!string.IsNullOrEmpty(sz))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisPath  //�\��������Ȃ��Ƃ��́E�E�E�\�L
                };

                //�`�ʃT�C�Y���m�F
                size = g.MeasureString(sz, fontS, e.Bounds.Width - x, sf);
                Rectangle rc = new Rectangle(x, y, e.Bounds.Width - x, HeightS);
                g.DrawString(sz, fontS, Brushes.DarkGray, rc, sf);
                //y += (int)size.Height + PADDING;
                y += HeightS + PADDING;
            }

            //�����̕`��:�T�C�Y, ���t
            sz = string.Format(
                "{0:N0}bytes,   {1}",
                ImgInfo.FileLength,
                ImgInfo.CreateDate
                );
            g.DrawString(sz, fontS, Brushes.DarkGray, x, y);
            size = g.MeasureString(sz, fontS, e.Bounds.Width - x);
            x += (int)size.Width + PADDING;
            //y += HeightS + PADDING;

            //�����̕`��:�s�N�Z����
            sz = string.Format(
                "{0:N0}x{1:N0}pixels",
                ImgInfo.Width,
                ImgInfo.Height
                );
            g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);
            //y += HeightS + PADDING; //�Ō�Ȃ̂ŕs�v

            ////�����̕`��:Exif���̑�
            //sz = string.Format(
            //    "{0} {1}",
            //    ImgInfo.ExifMake,
            //    ImgInfo.ExifModel);
            //if (ImgInfo.ExifISO != 0)
            //    sz = string.Format("ISO={0} {1}", ImgInfo.ExifISO, sz);
            //g.DrawString(sz, fontS, Brushes.SteelBlue, x, y);

            g.DrawRectangle(Pens.LightGray, e.Bounds);
            e.DrawFocusRectangle();     //�t�H�[�J�X������Ƃ��ɘg��`��
        }

        /// <summary>
        /// ���X�g�{�b�N�X�̍�����Ԃ�
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

            //ListBox���X�V
            listBox1.TabIndex = listBox1.SelectedIndex;
            listBox1.Refresh();

            //���C����ʂ��X�V
            ListBox1_SelectedIndexChanged(null, null);
        }

        private void ButtonSortByDate_Click(object sender, EventArgs e)
        {
            var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
            m_packageInfo.Items.Sort(comparer);

            //ListBox���X�V
            listBox1.TabIndex = listBox1.SelectedIndex;
            listBox1.Refresh();

            //���C����ʂ��X�V
            ListBox1_SelectedIndexChanged(null, null);
        }

        private void ButtonSortOrg_Click(object sender, EventArgs e)
        {
            var comparer = new ImageInfoComparer(ImageInfoComparer.Target.OriginalIndex);
            m_packageInfo.Items.Sort(comparer);

            //ListBox���X�V
            listBox1.TabIndex = listBox1.SelectedIndex;
            listBox1.Refresh();

            //���C����ʂ��X�V
            ListBox1_SelectedIndexChanged(null, null);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            //���̏����Ƀ\�[�g
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
namespace Marmi
{
	partial class FormPackageInfo
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPackageInfo));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxChangeMainWindow = new System.Windows.Forms.CheckBox();
			this.buttonUp = new System.Windows.Forms.Button();
			this.buttonDown = new System.Windows.Forms.Button();
			this.buttonSortByName = new System.Windows.Forms.Button();
			this.buttonSortByDate = new System.Windows.Forms.Button();
			this.buttonSortOrg = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.checkBoxSort = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(6, 18);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 48);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Location = new System.Drawing.Point(60, 18);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(402, 60);
			this.textBox1.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 1;
			this.label2.Text = "画像情報";
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 12;
			this.listBox1.Location = new System.Drawing.Point(12, 143);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(387, 306);
			this.listBox1.TabIndex = 3;
			this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox1_DrawItem);
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.ListBox1_SelectedIndexChanged);
			this.listBox1.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.ListBox1_MeasureItem);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(324, 458);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 5;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.AutoSize = true;
			this.groupBox1.Controls.Add(this.textBox1);
			this.groupBox1.Controls.Add(this.pictureBox1);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(468, 96);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "パッケージ情報";
			// 
			// checkBoxChangeMainWindow
			// 
			this.checkBoxChangeMainWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxChangeMainWindow.AutoSize = true;
			this.checkBoxChangeMainWindow.Location = new System.Drawing.Point(12, 465);
			this.checkBoxChangeMainWindow.Name = "checkBoxChangeMainWindow";
			this.checkBoxChangeMainWindow.Size = new System.Drawing.Size(248, 16);
			this.checkBoxChangeMainWindow.TabIndex = 7;
			this.checkBoxChangeMainWindow.Text = "画像選択時にメインウィンドウの画像も変更する";
			this.checkBoxChangeMainWindow.UseVisualStyleBackColor = true;
			// 
			// buttonUp
			// 
			this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUp.Location = new System.Drawing.Point(405, 165);
			this.buttonUp.Name = "buttonUp";
			this.buttonUp.Size = new System.Drawing.Size(75, 23);
			this.buttonUp.TabIndex = 8;
			this.buttonUp.Text = "１つ上に";
			this.buttonUp.UseVisualStyleBackColor = true;
			this.buttonUp.Click += new System.EventHandler(this.ButtonUp_Click);
			// 
			// buttonDown
			// 
			this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDown.Location = new System.Drawing.Point(405, 194);
			this.buttonDown.Name = "buttonDown";
			this.buttonDown.Size = new System.Drawing.Size(75, 23);
			this.buttonDown.TabIndex = 8;
			this.buttonDown.Text = "１つ下に";
			this.buttonDown.UseVisualStyleBackColor = true;
			this.buttonDown.Click += new System.EventHandler(this.ButtonDown_Click);
			// 
			// buttonSortByName
			// 
			this.buttonSortByName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSortByName.Location = new System.Drawing.Point(405, 223);
			this.buttonSortByName.Name = "buttonSortByName";
			this.buttonSortByName.Size = new System.Drawing.Size(75, 23);
			this.buttonSortByName.TabIndex = 9;
			this.buttonSortByName.Text = "名前順";
			this.buttonSortByName.UseVisualStyleBackColor = true;
			this.buttonSortByName.Click += new System.EventHandler(this.ButtonSortByName_Click);
			// 
			// buttonSortByDate
			// 
			this.buttonSortByDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSortByDate.Location = new System.Drawing.Point(405, 252);
			this.buttonSortByDate.Name = "buttonSortByDate";
			this.buttonSortByDate.Size = new System.Drawing.Size(75, 23);
			this.buttonSortByDate.TabIndex = 9;
			this.buttonSortByDate.Text = "日付順";
			this.buttonSortByDate.UseVisualStyleBackColor = true;
			this.buttonSortByDate.Click += new System.EventHandler(this.ButtonSortByDate_Click);
			// 
			// buttonSortOrg
			// 
			this.buttonSortOrg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSortOrg.Location = new System.Drawing.Point(405, 311);
			this.buttonSortOrg.Name = "buttonSortOrg";
			this.buttonSortOrg.Size = new System.Drawing.Size(75, 23);
			this.buttonSortOrg.TabIndex = 9;
			this.buttonSortOrg.Text = "元に戻す";
			this.buttonSortOrg.UseVisualStyleBackColor = true;
			this.buttonSortOrg.Click += new System.EventHandler(this.ButtonSortOrg_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(405, 458);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 10;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// checkBoxSort
			// 
			this.checkBoxSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxSort.Location = new System.Drawing.Point(405, 143);
			this.checkBoxSort.Name = "checkBoxSort";
			this.checkBoxSort.Size = new System.Drawing.Size(75, 16);
			this.checkBoxSort.TabIndex = 12;
			this.checkBoxSort.Text = "並び替え";
			this.checkBoxSort.UseVisualStyleBackColor = true;
			this.checkBoxSort.CheckedChanged += new System.EventHandler(this.CheckBoxSort_CheckedChanged);
			// 
			// PackageInfoForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(492, 493);
			this.Controls.Add(this.checkBoxSort);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonSortOrg);
			this.Controls.Add(this.buttonSortByDate);
			this.Controls.Add(this.buttonSortByName);
			this.Controls.Add(this.buttonDown);
			this.Controls.Add(this.buttonUp);
			this.Controls.Add(this.checkBoxChangeMainWindow);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.label2);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 400);
			this.Name = "PackageInfoForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "画像情報";
			this.Load += new System.EventHandler(this.PackageInfoForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBoxChangeMainWindow;
		private System.Windows.Forms.Button buttonUp;
		private System.Windows.Forms.Button buttonDown;
		private System.Windows.Forms.Button buttonSortByName;
		private System.Windows.Forms.Button buttonSortByDate;
		private System.Windows.Forms.Button buttonSortOrg;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.CheckBox checkBoxSort;
	}
}
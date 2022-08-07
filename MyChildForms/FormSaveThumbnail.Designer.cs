namespace Marmi
{
	partial class FormSaveThumbnail
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSaveThumbnail));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tbPixels = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbnItemX = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.btExcute = new System.Windows.Forms.Button();
			this.btCancel = new System.Windows.Forms.Button();
			this.tbInfo = new System.Windows.Forms.TextBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.isDrawFileName = new System.Windows.Forms.CheckBox();
			this.isDrawFileSize = new System.Windows.Forms.CheckBox();
			this.isDrawPicSize = new System.Windows.Forms.CheckBox();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(50, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(139, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "サムネイル一覧を保存します";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 66);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(110, 12);
			this.label2.TabIndex = 0;
			this.label2.Text = "サムネイル１つの大きさ";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 93);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(112, 12);
			this.label3.TabIndex = 0;
			this.label3.Text = "横方向のサムネイル数";
			// 
			// tbPixels
			// 
			this.tbPixels.Location = new System.Drawing.Point(130, 63);
			this.tbPixels.Name = "tbPixels";
			this.tbPixels.Size = new System.Drawing.Size(60, 19);
			this.tbPixels.TabIndex = 1;
			this.tbPixels.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbPixels.TextChanged += new System.EventHandler(this.Textbox_TextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(196, 66);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(29, 12);
			this.label4.TabIndex = 0;
			this.label4.Text = "pixel";
			// 
			// tbnItemX
			// 
			this.tbnItemX.Location = new System.Drawing.Point(130, 88);
			this.tbnItemX.Name = "tbnItemX";
			this.tbnItemX.Size = new System.Drawing.Size(60, 19);
			this.tbnItemX.TabIndex = 2;
			this.tbnItemX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbnItemX.TextChanged += new System.EventHandler(this.Textbox_TextChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(196, 93);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(17, 12);
			this.label5.TabIndex = 0;
			this.label5.Text = "個";
			// 
			// btExcute
			// 
			this.btExcute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btExcute.Location = new System.Drawing.Point(188, 286);
			this.btExcute.Name = "btExcute";
			this.btExcute.Size = new System.Drawing.Size(74, 23);
			this.btExcute.TabIndex = 3;
			this.btExcute.Text = "実行";
			this.btExcute.UseVisualStyleBackColor = true;
			this.btExcute.Click += new System.EventHandler(this.BtnExcute_Click);
			// 
			// btCancel
			// 
			this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btCancel.Location = new System.Drawing.Point(268, 286);
			this.btCancel.Name = "btCancel";
			this.btCancel.Size = new System.Drawing.Size(74, 23);
			this.btCancel.TabIndex = 4;
			this.btCancel.Text = "キャンセル";
			this.btCancel.UseVisualStyleBackColor = true;
			this.btCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// tbInfo
			// 
			this.tbInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbInfo.Location = new System.Drawing.Point(12, 187);
			this.tbInfo.Multiline = true;
			this.tbInfo.Name = "tbInfo";
			this.tbInfo.ReadOnly = true;
			this.tbInfo.Size = new System.Drawing.Size(332, 93);
			this.tbInfo.TabIndex = 0;
			this.tbInfo.TabStop = false;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsProgressBar1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 322);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(354, 22);
			this.statusStrip1.TabIndex = 5;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(237, 17);
			this.toolStripStatusLabel1.Spring = true;
			this.toolStripStatusLabel1.Text = "実行を押すとサムネイル作成します";
			this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tsProgressBar1
			// 
			this.tsProgressBar1.Name = "tsProgressBar1";
			this.tsProgressBar1.Size = new System.Drawing.Size(100, 16);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(12, 13);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 32);
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			// 
			// isDrawFileName
			// 
			this.isDrawFileName.AutoSize = true;
			this.isDrawFileName.Location = new System.Drawing.Point(14, 121);
			this.isDrawFileName.Name = "isDrawFileName";
			this.isDrawFileName.Size = new System.Drawing.Size(122, 16);
			this.isDrawFileName.TabIndex = 7;
			this.isDrawFileName.Text = "ファイル名を描写する";
			this.isDrawFileName.UseVisualStyleBackColor = true;
			// 
			// isDrawFileSize
			// 
			this.isDrawFileSize.AutoSize = true;
			this.isDrawFileSize.Location = new System.Drawing.Point(14, 143);
			this.isDrawFileSize.Name = "isDrawFileSize";
			this.isDrawFileSize.Size = new System.Drawing.Size(139, 16);
			this.isDrawFileSize.TabIndex = 7;
			this.isDrawFileSize.Text = "ファイルサイズを描写する";
			this.isDrawFileSize.UseVisualStyleBackColor = true;
			// 
			// isDrawPicSize
			// 
			this.isDrawPicSize.AutoSize = true;
			this.isDrawPicSize.Location = new System.Drawing.Point(14, 165);
			this.isDrawPicSize.Name = "isDrawPicSize";
			this.isDrawPicSize.Size = new System.Drawing.Size(129, 16);
			this.isDrawPicSize.TabIndex = 7;
			this.isDrawPicSize.Text = "画像サイズを描写する";
			this.isDrawPicSize.UseVisualStyleBackColor = true;
			// 
			// FormSaveThumbnail
			// 
			this.AcceptButton = this.btExcute;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btCancel;
			this.ClientSize = new System.Drawing.Size(354, 344);
			this.Controls.Add(this.isDrawPicSize);
			this.Controls.Add(this.isDrawFileSize);
			this.Controls.Add(this.isDrawFileName);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.tbInfo);
			this.Controls.Add(this.btCancel);
			this.Controls.Add(this.btExcute);
			this.Controls.Add(this.tbnItemX);
			this.Controls.Add(this.tbPixels);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FormSaveThumbnail";
			this.Text = "サムネイル一覧の保存";
			this.Load += new System.EventHandler(this.FormSaveThumbnail_Load);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormSaveThumbnail_FormClosed);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbPixels;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbnItemX;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btExcute;
		private System.Windows.Forms.Button btCancel;
		private System.Windows.Forms.TextBox tbInfo;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripProgressBar tsProgressBar1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.CheckBox isDrawFileName;
		private System.Windows.Forms.CheckBox isDrawFileSize;
		private System.Windows.Forms.CheckBox isDrawPicSize;
	}
}
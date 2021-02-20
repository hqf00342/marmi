namespace Marmi
{
	partial class FormPictureInfo
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
			this.SuspendLayout();
			// 
			// PictureInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "PictureInfo";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "PictureInfo";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureInfo_Paint);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureInfo_MouseUp);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PictureInfo_FormClosing);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureInfo_MouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureInfo_MouseDown);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
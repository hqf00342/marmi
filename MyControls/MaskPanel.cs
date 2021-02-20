using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Marmi
{
	class MaskPanel : UserControl
	{

		//�摜�`�ʊ֘A
		//private Rectangle clientRect;	// �N���C�A���g�̈�
		//private SolidBrush m_brush;			// �u���V
		//private float alpha;				// �`�ʎ��̓����x�BOnPaint()�ŗ��p���Ă���
		private Bitmap _bmp;				// �^�����ɓh��ꂽBitmap
		
		//�\������e�L�X�g
		private List<string> texts = new List<string>();

		//private System.Threading.Timer timer;

		//�t�F�[�h�z��Q
		float[] ToBlackAlphas = { 0.0f, 0.4f, 0.7f, 0.9f, 1.0f };
		float[] ToClearAlphas = { 1.0f, 0.6f, 0.3f, 0.2f, 0.1f};

		public float alpha { get; set; }

		// ������ ***********************************************************************/


		//TODO: �T�C�h�o�[�Œ�̎��Ƀz�C�[���̃t�H�[�J�X����ɃT�C�h�o�[�ɂȂ��Ă��܂��B



		public MaskPanel()
		{
			//�����x�͕s������
			alpha = 1.0F;

			//texts��������
			texts.Clear();

			//�_�u���o�b�t�@��L����
			this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);


			//�w�i�F�͓�����
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
			//this.BackColor = Color.FromArgb(128, Color.SlateBlue);

			//�摜������
			_bmp = null;
			//_bmp = new Bitmap(clientRect.Width, clientRect.Height);
			//using (Graphics g = Graphics.FromImage(_bmp))
			//{
			//    g.Clear(this.BackColor);
			//}

			//Timer
			//timer = new System.Threading.Timer(callbackTimer);
		}

		//�ʒu�w��̃R���X�g���N�^
		public MaskPanel(Rectangle cRect)
			: this()
		{
			//��Ɉ����Ȃ��̃R���X�g���N�^�����s�����
			SetPosition(cRect);
		}

	
		~MaskPanel()
		{
			_bmp.Dispose();
			//m_brush.Dispose();
		}



		// public���\�b�h/�v���p�e�B ****************************************************/

		public void SetPosition(Rectangle cRect)
		{
			this.Left   = cRect.X;
			this.Top    = cRect.Y;
			this.Width  = cRect.Width;
			this.Height = cRect.Height;

			//Bitmap������
			if (_bmp != null)
			{
				_bmp.Dispose();
			}
			_bmp = new Bitmap(cRect.Width, cRect.Height);
			using (Graphics g = Graphics.FromImage(_bmp))
			{
				g.Clear(this.BackColor);
			}
		}


		/// <summary>
		/// ��������alpha=1�ɂ���B
		/// </summary>
		public void FadeIn()
		{
			alpha = 0.0F;
			this.Refresh();
			this.Visible = true;

			foreach(float a in ToBlackAlphas)
			{
				alpha = a;		//�����x��ݒ�
				this.Refresh();
				//Thread.Sleep(30);
			}
			alpha = 1.0F;
		}

		/// <summary>
		/// �����ɂ���B
		/// </summary>
		public void FadeOut()
		{
			foreach (float a in ToClearAlphas)
			{
				alpha = a;		//�����x��ݒ�
				this.Refresh();
				//Thread.Sleep(100);
				//Debug.WriteLine(a, "fadeout()");
			}
			this.Visible = false;
			alpha = 0.0F;
		}

		// �����\���ł���悤�ɂ���
		// �\���ʒu�͉E��
		public void SetInformationText(string s)
		{
			string FontName = "MS PGothic";			//�t�H���g��
			int FontPoint = 11;
			int MARGIN = 3;

			using (Graphics g = Graphics.FromImage(_bmp))
			using (SolidBrush brush = new SolidBrush(Color.White))
			using (Font font = new Font(FontName, FontPoint))
			{
				g.Clear(this.BackColor);

				//�T�C�Y�𑪂�
				SizeF size = g.MeasureString(s, font);

				float x = _bmp.Width - size.Width - MARGIN;
				float y = _bmp.Height - size.Height - MARGIN;
				x = x > 0 ? x : 0;
				y = y > 0 ? y : 0;
				g.DrawString(s, font, brush, x, y);
			}//using	
		}

		//�e�L�X�g��ǉ�����
		public void addText(string s)
		{
			texts.Add(s);
			MakeBitmap();
		}

		/// <summary>
		/// �e�L�X�g��Bitmap�ɕ`�ʂ���
		/// </summary>
		private void MakeBitmap()
		{
			string FontName = "MS PGothic";			//�t�H���g��
			int FontPoint = 12;
			int MARGIN = 5;

			using (Graphics g = Graphics.FromImage(_bmp))
			using (SolidBrush brush = new SolidBrush(Color.White))
			using (Font font = new Font(FontName, FontPoint))
			{
				g.Clear(this.BackColor);

				//�t���ɏ�������
				texts.Reverse();

				float x = _bmp.Width - MARGIN;
				float y = _bmp.Height - MARGIN;
				foreach (string s in texts)
				{
					//�T�C�Y�𑪂�
					SizeF size = g.MeasureString(s, font);
					x = _bmp.Width - size.Width - MARGIN;
					y = y - size.Height - MARGIN;
					x = x > 0 ? x : 0;
					y = y > 0 ? y : 0;

					g.DrawString(s, font, brush, x, y);
				}

				//���̏��Ԃɖ߂�
				texts.Reverse();
			}//using	

		}



		// �I�[�i�[�h���[ ***************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);

			//e.Graphics.Clear(Color.Transparent);
			if (alpha >= 1.0F)
			{
				//�����`�ʂ��Ȃ�
				e.Graphics.DrawImageUnscaled(_bmp, 0, 0);
			}
			else
			{
				//�������`��
				BitmapUty.alphaDrawImage(e.Graphics, _bmp, alpha);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//base.OnPaintBackground(e);
			// �w�ʂ̃R���g���[����`�悵�Ȃ� Or �w�i�F���s�����Ȃ̂Ŕw�ʂ̃R���g���[����`�悷��K�v�Ȃ�
			if (this.BackColor.A == 255) 
			{
				base.OnPaintBackground(e);
				return;
			}

			// �w�ʂ̃R���g���[����`��
			this.DrawParentWithBackControl(e);


			// �w�i��`��
			this.DrawBackground(e);
		}




		/// <summary>
		/// �R���g���[���̔w�i��`�悵�܂��B
		/// </summary>
		/// <param name="pevent">�`���̃R���g���[���Ɋ֘A������</param>
		private void DrawBackground(System.Windows.Forms.PaintEventArgs pevent)
		{
			// �w�i�F
			using (SolidBrush sb = new SolidBrush(this.BackColor)) {
				pevent.Graphics.FillRectangle(sb, this.ClientRectangle);
			}

			//// �w�i�摜
			//if (this.BackgroundImage != null) {
			//    this.DrawBackgroundImage(pevent.Graphics, this.BackgroundImage, this.BackgroundImageLayout);
			//}
		}

		/// <summary>
		/// �R���g���[���̔w�i�摜��`�悵�܂��B
		/// </summary>
		/// <param name="g">�`��Ɏg�p����O���t�B�b�N�X �I�u�W�F�N�g</param>
		/// <param name="img">�`�悷��摜</param>
		/// <param name="layout">�摜�̃��C�A�E�g</param>
		//private void DrawBackgroundImage(Graphics g, Image img, ImageLayout layout) 
		//{
		//    Size imgSize = img.Size;

		//    switch (layout) {
		//        case ImageLayout.None:
		//            g.DrawImage(img, 0, 0, imgSize.Width, imgSize.Height);

		//            break;
		//        case ImageLayout.Tile:
		//            int xCount = Convert.ToInt32(Math.Ceiling((double)this.ClientRectangle.Width / (double)imgSize.Width));
		//            int yCount = Convert.ToInt32(Math.Ceiling((double)this.ClientRectangle.Height / (double)imgSize.Height));
		//            for (int x = 0; x <= xCount - 1; x++) {
		//                for (int y = 0; y <= yCount - 1; y++) {
		//                    g.DrawImage(img, imgSize.Width * x, imgSize.Height * y, imgSize.Width, imgSize.Height);
		//                }
		//            }

		//            break;
		//        case ImageLayout.Center: {
		//                int x = 0;
		//                if (this.ClientRectangle.Width > imgSize.Width) {
		//                    x = (int)Math.Floor((double)(this.ClientRectangle.Width - imgSize.Width) / 2.0);
		//                }
		//                int y = 0;
		//                if (this.ClientRectangle.Height > imgSize.Height) {
		//                    y = (int)Math.Floor((double)(this.ClientRectangle.Height - imgSize.Height) / 2.0);
		//                }
		//                g.DrawImage(img, x, y, imgSize.Width, imgSize.Height);

		//                break;
		//            }
		//        case ImageLayout.Stretch:
		//            g.DrawImage(img, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);

		//            break;
		//        case ImageLayout.Zoom: {
		//                double xRatio = (double)this.ClientRectangle.Width / (double)imgSize.Width;
		//                double yRatio = (double)this.ClientRectangle.Height / (double)imgSize.Height;
		//                double minRatio = Math.Min(xRatio, yRatio);
						
		//                Size zoomSize = new Size(Convert.ToInt32(Math.Ceiling(imgSize.Width * minRatio)), Convert.ToInt32(Math.Ceiling(imgSize.Height * minRatio)));

		//                int x = 0;
		//                if (this.ClientRectangle.Width > zoomSize.Width) {
		//                    x = (int)Math.Floor((double)(this.ClientRectangle.Width - zoomSize.Width) / 2.0);
		//                }
		//                int y = 0;
		//                if (this.ClientRectangle.Height > zoomSize.Height) {
		//                    y = (int)Math.Floor((double)(this.ClientRectangle.Height - zoomSize.Height) / 2.0);
		//                }
		//                g.DrawImage(img, x, y, zoomSize.Width, zoomSize.Height);

		//                break;
		//            }
		//    }
		//}

		/// <summary>
		/// �e�R���g���[���Ɣw�ʂɂ���R���g���[����`�悵�܂��B
		/// </summary>
		/// <param name="pevent">�`���̃R���g���[���Ɋ֘A������</param>
		private void DrawParentWithBackControl(System.Windows.Forms.PaintEventArgs pevent) {
			// �e�R���g���[����`��
			this.DrawParentControl(this.Parent, pevent);

			// �e�R���g���[���Ƃ̊Ԃ̃R���g���[����e������`��
			for (int i = this.Parent.Controls.Count - 1; i >= 0; i--)
			{
				Control c = this.Parent.Controls[i];
				//if (c.Name != "PicPanel")
				//    continue;
				if (c == this)
				{
					break;
				}
				if (this.Bounds.IntersectsWith(c.Bounds) == false)
				{
					continue;
				}

				Debug.WriteLine(c.Name, "ParentControl");
				this.DrawBackControl(c, pevent);
			}
		}

		/// <summary>
		/// �e�R���g���[����`�悵�܂��B
		/// </summary>
		/// <param name="c">�e�R���g���[��</param>
		/// <param name="pevent">�`���̃R���g���[���Ɋ֘A������</param>
		private void DrawParentControl(Control c, System.Windows.Forms.PaintEventArgs pevent)
		{
			using (Bitmap bmp = new Bitmap(c.Width, c.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			using (Graphics g = Graphics.FromImage(bmp))
			using (PaintEventArgs p = new PaintEventArgs(g, c.ClientRectangle)) 
			{
				this.InvokePaintBackground(c, p);
				this.InvokePaint(c, p);

				int offsetX = this.Left + (int)Math.Floor((double)(this.Bounds.Width - this.ClientRectangle.Width) / 2.0);
				int offsetY = this.Top + (int)Math.Floor((double)(this.Bounds.Height - this.ClientRectangle.Height) / 2.0);
				pevent.Graphics.DrawImage(
					bmp,
					this.ClientRectangle,
					new Rectangle(offsetX, offsetY, this.ClientRectangle.Width, this.ClientRectangle.Height),
					GraphicsUnit.Pixel);
			}
		}

		/// <summary>
		/// �w�ʂ̃R���g���[����`�悵�܂��B
		/// </summary>
		/// <param name="c">�w�ʂ̃R���g���[��</param>
		/// <param name="pevent">�`���̃R���g���[���Ɋ֘A������</param>
		private void DrawBackControl(Control c, System.Windows.Forms.PaintEventArgs pevent)
		{
			using (Bitmap bmp = new Bitmap(c.Width, c.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			{
				c.DrawToBitmap(bmp, new Rectangle(0, 0, c.Width, c.Height));

				int offsetX = (c.Left - this.Left) - (int)Math.Floor((double)(this.Bounds.Width - this.ClientRectangle.Width) / 2.0);
				int offsetY = (c.Top - this.Top) - (int)Math.Floor((double)(this.Bounds.Height - this.ClientRectangle.Height) / 2.0);
				pevent.Graphics.DrawImage(bmp, offsetX, offsetY, c.Width, c.Height);
			}
		}
	}
}

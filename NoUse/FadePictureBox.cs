using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				//PictureBox, ScrollBar
using System.Drawing.Imaging;			//fadePictureBox ColorMatrix
using System.Drawing.Drawing2D;			//GraphicsPath


namespace Marmi
{

	/********************************************************************************/
	//���Ԃ�����Ǝ����I�ɏ�����PictureBox
	/********************************************************************************/
	/// <summary>
	/// fadePictureBox
	/// �w�莞�Ԃŏ�����PictureBox�B
	/// �g�����F������ɕ\���ʒu�A�傫������щ摜���w�肵��Show()�B
	/// </summary>
	class FadePictureBox : UserControl // : PictureBox
	{
		private Bitmap m_bgImage = null;			//�w�i�ۑ��p�B�������ɃL���v�`�����Ă���B
		private Bitmap m_fgImage = null;			//�\������摜�B
		private Bitmap m_ShowImage = null;			//��������Bmp�B�����\������B
		private System.Windows.Forms.Timer m_timer = null;		//Holdtime���������邽�߂̃^�C�}�[
		//private BitmapPanel bp = new BitmapPanel();

		#region alphaF �t�F�[�h�C���E�A�E�g�p�̓��ߒl�B
		float[] alphaF = { 
			0.1F,
			0.11F,
			0.13F,
			0.26F,
			0.3F,
			0.35F,
			0.4F,
			0.5F,
			0.6F,
			0.7F,
			0.75F,
			0.8F,
			0.85F,
			0.9F,
			0.95F,
			//0.85F,
			//0.9F,
			//0.95F,
			//1.0F
		};
		#endregion


		public FadePictureBox(Control parent)
		{
			this.Visible = false;
			this.BackColor = Color.Transparent;	//����A�d�v���ˁB
			this.Parent = parent;
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			parent.Controls.Add(this);

			//�^�C�}�[�ݒ�
			m_timer = new System.Windows.Forms.Timer();
			m_timer.Tick += new EventHandler(timer_Tick);

			//�C�x���g�ݒ�
			this.Paint += new PaintEventHandler(fadePictureBox_Paint);

		}

		~FadePictureBox()
		{
			if (this.Visible)
				fadeOut();

			this.Parent.Controls.Remove(this);
			m_timer.Tick -= new EventHandler(timer_Tick);
			this.Paint -= new PaintEventHandler(fadePictureBox_Paint);
		}

		void fadePictureBox_Paint(object sender, PaintEventArgs e)
		{
			if (m_ShowImage != null)
				e.Graphics.DrawImage(m_ShowImage, this.ClientRectangle);
			else
				e.Graphics.Clear(this.BackColor);
		}


		private void timer_Tick(object sender, EventArgs e)
		{
			m_timer.Stop();
			m_timer.Dispose();

			fadeOut();

			//���\�[�X���J������
			m_bgImage.Dispose();
			m_bgImage = null;
			//m_fgImage.Dispose();	//����̓N���X�O���\�[�X��������Ȃ��̂�Dispose()���Ȃ�
			m_fgImage = null;
			m_ShowImage.Dispose();
			m_ShowImage = null;
		}

		public void ShowAuto(string sz, int holdtime)
		{
			BitmapPanel bp = new BitmapPanel();
			Image img = bp.TextBoard(sz, true);
			ShowAuto(img, holdtime);
		}

		public void ShowAuto(Image img, int holdtime)
		{
			//�܂��t�F�[�h�C��������
			ShowIn(img);

			//timer�̏���
			m_timer.Interval = holdtime;
			m_timer.Start();
		}

		public void ShowIn(string sz)
		{
			BitmapPanel bp = new BitmapPanel();
			Image img = bp.TextBoard(sz, true);
			ShowIn(img);
		}

		public void ShowIn(Image img)
		{
			//�^�C�}�[���쒆�Ȃ牽�����Ȃ�
			if (m_timer.Enabled)
				return;

			//�摜��ݒ�
			m_fgImage = (Bitmap)img;
			this.Width = img.Width;
			this.Height = img.Height;

			//�\���ʒu��ݒ�
			int cx = this.Parent.Width;
			int cy = this.Parent.Height;
			this.Left = (cx - this.Width) / 2;
			this.Top = (cy - this.Height) / 2;

			//�\���ʒu�̔w�i���擾
			captureBackGroundImage();
			using (Graphics g = this.CreateGraphics())
			{
				g.DrawImage(m_bgImage, 0, 0);
			}
			this.Visible = true;
			fadeIn();
		}

		public void ShowOut()
		{
			fadeOut();
		}


		/// <summary>
		/// �摜���t�F�[�h�C��������B�ŏI�I�ɂ�Image���\�������
		/// </summary>
		private void fadeIn()
		{
			//�t�F�[�h�C��
			for (int i = 0; i < alphaF.Length; i++)
			{
				DrawBrendImage(m_fgImage, alphaF[i]);
				System.Threading.Thread.Sleep(10);
			}
		}


		/// <summary>
		/// �t�F�[�h�A�E�g����B�Ō�͔�\���ƂȂ�B
		/// �\������摜��this.Image�i���o�b�N�A�b�v����ImageBackup�j
		/// </summary>
		private void fadeOut()
		{
			//�t�F�[�h�A�E�g
			for (int i = alphaF.Length - 1; i >= 0; i--)
			{
				DrawBrendImage(m_fgImage, alphaF[i]);
				System.Threading.Thread.Sleep(10);
			}
			this.Visible = false;
		}


		/// <summary>
		/// �A���t�@�u�����h�����摜��`�ʂ���
		/// </summary>
		/// <param name="img">�u�����h����摜�B�w�i��bgImage</param>
		/// <param name="alpha">���ߒl�B�O�`�P�̊�</param>
		private void DrawBrendImage(Image img, float alpha)
		{
			//�u�����h���ĕ\������w�i������
			m_ShowImage = (Bitmap)m_bgImage.Clone();
			Graphics brendG = Graphics.FromImage(m_ShowImage);

			//System.Drawing.Imaging.ColorMatrix�I�u�W�F�N�g�̍쐬
			//ColorMatrix�̍s��̒l��ύX���āA�A���t�@�l��alpha�ɕύX�����悤�ɂ���
			ColorMatrix cm = new ColorMatrix();
			cm.Matrix00 = 1;
			cm.Matrix11 = 1;
			cm.Matrix22 = 1;
			cm.Matrix33 = alpha;
			cm.Matrix44 = 1;

			//System.Drawing.Imaging.ImageAttributes�I�u�W�F�N�g�̍쐬
			//ColorMatrix��ݒ�AImageAttributes���g�p���Ĕw�i�ɕ`��
			ImageAttributes ia = new ImageAttributes();
			ia.SetColorMatrix(cm);
			//ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			//�A���t�@�u�����h���Ȃ���`��
			brendG.DrawImage(
				img,
				new Rectangle(0, 0, img.Width, img.Height),
				0, 0,
				img.Width, img.Height,
				GraphicsUnit.Pixel, ia);

			//�������ꂽ�摜��\��
			using (Graphics g = this.CreateGraphics())
			{
				g.DrawImage(m_ShowImage, 0, 0);
			}
			//���\�[�X���J������
			brendG.Dispose();
			//brendBmp.Dispose();
			//this.Image = brendBmp;	//ver0.83�ŃR�����g�A�E�g�B�Ȃɂ��Ă���񂾂����H
		}



		/// <summary>
		/// �w�i�ƂȂ�摜���擾����B
		/// �擾�����摜��bgImage�ɕۑ��B
		/// </summary>
		private void captureBackGroundImage()
		{
			//�\���ʒu�̔w�i���擾
			Rectangle rc;
			//rc = this.RectangleToScreen(this.Bounds);
			//this(PictureBox)�ɑ΂���RectangleToScreen�i�j����̂Ńp�����[�^��this.Bounds�ł͑ʖ�
			rc = this.RectangleToScreen(new Rectangle(0, 0, this.Width, this.Height));

			m_bgImage = new Bitmap(
			  rc.Width,
			  rc.Height,
			  PixelFormat.Format32bppArgb
			  );
			using (Graphics gBgImage = Graphics.FromImage(m_bgImage))
			{
				gBgImage.CopyFromScreen(
					rc.X, rc.Y,
					0, 0,
					rc.Size,
					CopyPixelOperation.SourceCopy);
			}

		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Marmi
{
    class NoUse
    {
        /// <summary>
        /// �X�e�[�^�X�o�[�̕\���B
        /// �g���邩�Ǝv�������S��ʃ��[�h�̓��ꐫ�ɑΉ��ł���
        /// �O���[�o���R���t�B�O�����������Ă��܂��̂ł��ƂŖ߂��Ȃ�
        /// </summary>
        /// <param name="isVisible">�\�����邩�ǂ���</param>
        private void setStatusbarVisible(bool isVisible)
        {
            statusStrip1.Visible = isVisible;
            g_Config.visibleStatusBar = isVisible;
            MenuItem_ViewStatusbar.Checked = isVisible;
            MenuItem_ContextStatusbar.Checked = isVisible;
        }

        /// <summary>
        /// �c�[���o�[�̕\���B�S��ʃ��[�h�ɑΉ��ł���
        /// �O���[�o���R���t�B�O�����������Ă��܂��̂ł��ƂŖ߂��Ȃ�
        /// </summary>
        /// <param name="isVisible">�\�����邩�ǂ���</param>
        private void setToolbarVisible(bool isVisible)
        {
            toolStrip1.Visible = isVisible;
            g_Config.visibleToolBar = isVisible;
            MenuItem_ViewToolbar.Checked = isVisible;
            MenuItem_ContextToolbar.Checked = isVisible;
        }

        /// <summary>
        /// ���j���[�o�[�̕\���B�S��ʃ��[�h�ɑΉ��ł���
        /// �O���[�o���R���t�B�O�����������Ă��܂��̂ł��ƂŖ߂��Ȃ�
        /// </summary>
        /// <param name="isVisible">�\�����邩�ǂ���</param>
        private void setMenubarVisible(bool isVisible)
        {
            menuStrip1.Visible = isVisible;
            g_Config.visibleMenubar = isVisible;
            MenuItem_ViewMenubar.Checked = isVisible;
            MenuItem_ContextMenubar.Checked = isVisible;
        }

        /// <summary>
        /// TEMP�t�H���_������Ă���ꍇ�͍폜����
        /// </summary>
        private void DeleteTempFolder()
        {
            if (Directory.Exists(TEMP_FOLDER))
            {
                try
                {
                    Directory.Delete(TEMP_FOLDER, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        //ver0.62 ��ʃT�C�Y��g_bmp�ւ̕`��.2���`�ʁA�t�����Ή�.���ݎg���Ă��Ȃ�
        private void DrawImageToScreen3(int nIndex, int direction)
        {
            Cursor cc = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //�����̐��K��
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;
            direction = (direction >= 0) ? 1 : -1;

            //�Ƃ肠����1���ǂ߁I
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                //TODO:�t�@�C���擪�����ȍ~�̎Q�ƁB�G���[��������B
                viewPages = 0;
                DrawImageToGBMP3(null, null);
                return;
            }

            if (!g_Config.dualView
                || bmp1.Width > bmp1.Height)
            {
                //1��ʃ��[�h�m��
                viewPages = 1;      //1���\�����[�h
                DrawImageToGBMP3(bmp1, null);
                this.Refresh();
            }
            else
            {
                //2��ʃ��[�h�̋^���L��
                int next = nIndex + direction;
                Bitmap bmp2 = GetBitmap(nIndex + direction);
                if (bmp2 == null || bmp2.Width > bmp2.Height)
                {
                    //1��ʃ��[�h�m��
                    viewPages = 1;      //1���\�����[�h
                    DrawImageToGBMP3(bmp1, null);
                    this.Refresh();
                }
                else
                {
                    //2��ʃ��[�h�m��
                    viewPages = 2;      //2���\�����[�h
                                        //g_bmp = new Bitmap(
                                        //    bmp1.Width + bmp2.Width,
                                        //    bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height);
                    using (Graphics g = Graphics.FromImage(g_bmp))
                    {
                        if (direction > 0)
                        {
                            //������2���\��
                            DrawImageToGBMP3(bmp1, bmp2);
                        }
                        else
                        {
                            //�t����2���\��
                            DrawImageToGBMP3(bmp2, bmp1);
                            g_pi.ViewPage--;    //1�O�ɂ��Ă���
                        }
                    }
                    //bmp1.Dispose();	//�j��������ʖځIcache�����������Ȃ�
                    //bmp2.Dispose();	//�j��������ʖځIcache��������Ă��܂�
                    this.Refresh();
                }
            }

            Cursor.Current = cc;
            return;
        }

        //ver0.62 DrawImageToScreen3()����Ăяo�����B
        private void DrawImageToGBMP3(Bitmap bmp1, Bitmap bmp2)
        {
            if (g_bmp == null)
                g_bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height, PixelFormat.Format24bppRgb);

            Graphics g = Graphics.FromImage(g_bmp);
            Rectangle cr = ClientRectangle; // this.Bounds; //this.DisplayRectangle;

            //�����␳�B�c�[���o�[�̍�����ClientRectanble/Bounds����␳
            int toolbarHeight = 0;
            //if (toolStrip1.Visible && !toolButtonFullScreen.Checked)	//�S��ʃ��[�h�ł͂Ȃ��Atoolbar���\������Ă���Ƃ�
            if (toolStrip1.Visible && !g_Config.isFullScreen)   //�S��ʃ��[�h�ł͂Ȃ��Atoolbar���\������Ă���Ƃ�
                toolbarHeight = toolStrip1.Height;
            cr.Height -= toolbarHeight;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(g_Config.BackColor);    //����͕K�v���H
            if (bmp1 == null)
                return;

            float ratio = 1.0f;
            if (bmp2 == null)
            {
                //1���`�ʃ��[�h
                //g_bmp���c����P�F�P�ŕ\������B100%�����͏k���\��
                //ratio�͏������ق��ɂ��킹��
                viewPages = 1;      //1���\�����[�h
                float ratioX = (float)cr.Width / (float)bmp1.Width;
                float ratioY = (float)cr.Height / (float)bmp1.Height;
                ratio = (ratioX > ratioY) ? ratioY : ratioX;
                if (ratio >= 1) ratio = 1.0F;
                if (ratio == 0) ratio = 1.0F;

                int width = (int)(bmp1.Width * ratio);
                int height = (int)(bmp1.Height * ratio);

                g.DrawImage(
                    bmp1,                                       //�`�ʃC���[�W
                    (cr.Width - width) / 2,                     //�n�_X
                    (cr.Height - height) / 2 + toolbarHeight,   //�n�_Y
                    width,                                      //��
                    height                                      //����
                );
            }
            else
            {
                //2���`�ʃ��[�h
                viewPages = 2;      //2���\�����[�h
                int width = bmp1.Width + bmp2.Width;
                int height = bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height;
                float ratioX = (float)cr.Width / (float)width;
                float ratioY = (float)cr.Height / (float)height;
                ratio = (ratioX > ratioY) ? ratioY : ratioX;
                if (ratio >= 1) ratio = 1.0F;
                if (ratio == 0) ratio = 1.0F;

                //�^�񒆂̂��߂�offset�v�Z
                float offsetX = ((float)cr.Width - width * ratio) / 2;
                float offsetY = ((float)cr.Height - height * ratio) / 2 + toolbarHeight;

                //bmp2�͍��ɕ`��
                g.DrawImage(
                    bmp2,
                    0 + offsetX,
                    0 + offsetY,
                    bmp2.Width * ratio,
                    bmp2.Height * ratio
                    );

                //bmp1�͉E�ɕ`��
                g.DrawImage(
                    bmp1,
                    bmp2.Width * ratio + offsetX,
                    0 + offsetY,
                    bmp1.Width * ratio,
                    bmp1.Height * ratio
                    );
            }

            //�X�e�[�^�X�o�[�ɔ{���\��
            setStatubarRatio(ratio);
        }

        //ver0.81 �����ƊȌ���
        /// <summary>
        /// ��ʂ҂�����T�C�Y��g_bmp�𐶐�����
        /// </summary>
        /// <param name="nIndex"></param>
        private void DrawImageToGBMP4(int nIndex)
        {
            //�����̐��K��
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;

            //�J�[�\���̐ݒ�
            Cursor.Current = Cursors.WaitCursor;

            //1���͐�ǂ݂��Ă���
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                viewPages = 0;
                DrawGBMP4(null, null);
                Cursor.Current = Cursors.Default;
                return;
            }

            if (g_Config.dualView && CanDualView(nIndex))
            {
                //2���\��
                Bitmap bmp2 = GetBitmap(nIndex + 1);
                DrawGBMP4(bmp1, bmp2);
                this.Refresh();
            }
            else
            {
                //1���\��
                DrawGBMP4(bmp1, null);
                this.Refresh();
            }

            Cursor.Current = Cursors.Default;
            return;
        }

        //ver0.81 �����ƊȌ���
        /// <summary>
        /// DrawImageToGBMP4()����Ăяo������郋�[�`��
        /// </summary>
        /// <param name="bmp1"></param>
        /// <param name="bmp2"></param>
        private void DrawGBMP4(Bitmap bmp1, Bitmap bmp2)
        {
            Rectangle cr = GetClientRectangle();
            if (g_bmp == null || g_bmp.Size != cr.Size)
                g_bmp = new Bitmap(cr.Width, cr.Height, PixelFormat.Format24bppRgb);

            if (bmp1 == null)
                return;

            using (Graphics g = Graphics.FromImage(g_bmp))
            {
                g.Clear(g_Config.BackColor);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                float ratio = 1.0f;
                if (bmp2 == null)
                {
                    //1���`�ʃ��[�h
                    //g_bmp���c����P�F�P�ŕ\������B100%�����͏k���\��
                    //ratio�͏������ق��ɂ��킹��
                    viewPages = 1;      //1���\�����[�h
                    float ratioX = (float)cr.Width / (float)bmp1.Width;
                    float ratioY = (float)cr.Height / (float)bmp1.Height;
                    ratio = (ratioX > ratioY) ? ratioY : ratioX;
                    if (ratio >= 1) ratio = 1.0F;
                    if (ratio == 0) ratio = 1.0F;

                    int width = (int)(bmp1.Width * ratio);
                    int height = (int)(bmp1.Height * ratio);

                    g.DrawImage(
                        bmp1,                       //�`�ʃC���[�W
                        (cr.Width - width) / 2,     //�n�_X
                        (cr.Height - height) / 2,   //�n�_Y
                        width,                      //��
                        height                      //����
                    );
                }
                else
                {
                    //2���`�ʃ��[�h
                    viewPages = 2;      //2���\�����[�h
                    int width = bmp1.Width + bmp2.Width;
                    int height = bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height;
                    float ratioX = (float)cr.Width / (float)width;
                    float ratioY = (float)cr.Height / (float)height;
                    ratio = (ratioX > ratioY) ? ratioY : ratioX;
                    if (ratio >= 1) ratio = 1.0F;
                    if (ratio == 0) ratio = 1.0F;

                    //�^�񒆂̂��߂�offset�v�Z
                    float offsetX = ((float)cr.Width - width * ratio) / 2;
                    float offsetY = ((float)cr.Height - height * ratio) / 2;

                    //bmp2�͍��ɕ`��
                    g.DrawImage(
                        bmp2,
                        0 + offsetX,
                        0 + offsetY,
                        bmp2.Width * ratio,
                        bmp2.Height * ratio
                        );

                    //bmp1�͉E�ɕ`��
                    g.DrawImage(
                        bmp1,
                        bmp2.Width * ratio + offsetX,
                        0 + offsetY,
                        bmp1.Width * ratio,
                        bmp1.Height * ratio
                        );
                }
                //�X�e�[�^�X�o�[�ɔ{���\��
                setStatubarRatio(ratio);
            }//using
        }

        /// <summary>
        /// Form1_Paint����Ă΂�Ă��郂�W���[���B
        /// PaintGBMP()�ŃX�N���[���o�[�Ή��������ߎ���đ��ꂽ�B
        /// </summary>
        /// <param name="g"></param>
        private void RenderGBMP_FittingSize(Graphics g)
        {
            //Rectangle rect = this.ClientRectangle; // this.Bounds;

            ////�����␳�B�c�[���o�[�̍�����ClientRectanble/Bounds����␳
            //int toolbarHeight = 0;
            //if (toolStrip1.Visible && !toolButtonFullScreen.Checked)	//�S��ʃ��[�h�ł͂Ȃ��Atoolbar���\������Ă���Ƃ�
            //    toolbarHeight = toolStrip1.Height;
            //rect.Height -= toolbarHeight;

            Rectangle rect = GetClientRectangle();

            //g_bmp���c����P�F�P�ŕ\������B100%�����͏k���\��
            //ratio�͏������ق��ɂ��킹��
            float ratioX = (float)rect.Width / (float)g_bmp.Width;
            float ratioY = (float)rect.Height / (float)g_bmp.Height;
            float ratio = (ratioX > ratioY) ? ratioY : ratioX;
            if (ratio >= 1 || ratio <= 0) ratio = 1.0F;

            int width = (int)(g_bmp.Width * ratio);
            int height = (int)(g_bmp.Height * ratio);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(g_Config.BackColor);
            g.DrawImage(
                g_bmp,                                      //�`�ʃC���[�W
                (rect.Width - width) / 2,                   //�n�_X
                (rect.Height - height) / 2 + rect.Top,      //�n�_Y
                width,                                      //��
                height                                      //����
            );

            //�X�e�[�^�X�o�[�ɔ{���\��
            setStatubarRatio(ratio);
        }

        //g_bmp�ւ̕`��.2���`�ʁA�t�����Ή�
        private void DrawImageToGBMP(int nIndex, int direction)
        {
            //�����̐��K��
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;

            Cursor cc = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            direction = (direction >= 0) ? 1 : -1;

            //�Ƃ肠����1���ǂ߁I
            //g_bmp = GetBitmap(nIndex);
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                //TODO:�t�@�C���擪�����ȍ~�̎Q�ƁB�G���[��������B
                viewPages = 0;
                return;
            }

            //�\�����郂�m�͂���̂�g_bmp���N���A����B
            if (g_bmp != null)
                g_bmp.Dispose();

            if (!g_Config.dualView
                || bmp1.Width > bmp1.Height)
            {
                //1��ʃ��[�h�m��
                viewPages = 1;      //1���\�����[�h
                g_bmp = (Bitmap)bmp1.Clone();
                this.Refresh();
            }
            else
            {
                //2��ʃ��[�h�̋^���L��
                int next = nIndex + direction;
                Bitmap bmp2 = GetBitmap(nIndex + direction);
                if (bmp2 == null || bmp2.Width > bmp2.Height)
                {
                    //1��ʃ��[�h�m��
                    viewPages = 1;      //1���\�����[�h
                    g_bmp = (Bitmap)bmp1.Clone();
                    this.Refresh();
                }
                else
                {
                    //2��ʃ��[�h�m��
                    viewPages = 2;      //2���\�����[�h
                    g_bmp = new Bitmap(
                        bmp1.Width + bmp2.Width,
                        bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height);
                    using (Graphics g = Graphics.FromImage(g_bmp))
                    {
                        if (direction > 0)
                        {
                            //������2���\��
                            g.DrawImage(bmp2, 0, 0, bmp2.Width, bmp2.Height);
                            g.DrawImage(bmp1, bmp2.Width, 0, bmp1.Width, bmp1.Height);
                        }
                        else
                        {
                            //�t����2���\��
                            g.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);
                            g.DrawImage(bmp2, bmp1.Width, 0, bmp2.Width, bmp2.Height);
                            g_pi.ViewPage--;    //1�O�ɂ��Ă���
                        }
                    }
                    //bmp1.Dispose();	//�j��������ʖځIcache�����������Ȃ�
                    //bmp2.Dispose();	//�j��������ʖځIcache��������Ă��܂�
                    this.Refresh();
                }
            }

            Cursor.Current = cc;
            return;
        }

        //���̃o�[�W�����ł�SharpZip����̃X�g���[���ɑΉ��ł��Ȃ�
        private static Bitmap LoadIcon(Stream fs)
        {
            try
            {
                using (Icon ico = new Icon(fs))
                {
                    return ico.ToBitmap();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("�A�C�R���������s�B���g���C");
                fs.Seek(0, 0);  //'System.NotSupportedException' �̏����O�� ICSharpCode.SharpZipLib.dll �Ŕ������܂����B
                Bitmap b = (Bitmap)Bitmap.FromStream(fs, false, false);
                if (b == null)
                    throw e;
                else
                {
                    Debug.WriteLine("�A�C�R���Đ�������");
                    return b;
                }
            }
        }

        //SharpZip����̃X�g���[���ɑΉ�
        private static Bitmap LoadIcon2(Stream fs)
        {
            //SharpZip�̃X�g���[����Seek()�ȂǂɑΉ��ł��Ȃ��B
            //�Ȃ̂�MemoryStream�Ɉ�x��荞��ŗ��p����
            using (MemoryStream ms = new MemoryStream())
            {
                //��荞��
                int len;
                byte[] buffer = new byte[16384];
                while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, len);

                //�擪��Rewind
                ms.Seek(0, SeekOrigin.Begin);   //�d�v�I
                                                //�A�C�R���ǂݎ��J�n
                try
                {
                    using (Icon ico = new Icon(ms))
                    {
                        return ico.ToBitmap();
                    }
                }
                catch (Exception e)
                {
                    ms.Seek(0, 0);  //�d�v�I
                    Debug.WriteLine("�A�C�R���������s�B���g���C");
                    Bitmap b = new Bitmap(Bitmap.FromStream(ms, false, false));
                    if (b == null)
                        throw e;
                    else
                    {
                        Debug.WriteLine("�A�C�R���Đ�������");
                        return b;
                    }
                }
            }
        }

        //�A�C�R���t�@�C����͔�
        private static Bitmap LoadIcon3(Stream fs)
        {
            // �A�C�R���t�@�C������͂�GDI+�őΉ��ł��Ă��Ȃ�
            // �傫�ȃT�C�Y�̃A�C�R���AVista��PNG�^�A�C�R���ɑΉ�����
            //
            // �A�C�R���t�@�C���̍\��
            //   ICONDIR�\����(6byte)
            //   ICONDIRENTRY�\����(16byte)�~�A�C�R����
            //   ICONIMAGE�\���́~�A�C�R����
            //

            //SharpZip�̃X�g���[����Seek()�ɑΉ��ł��Ȃ��B
            //�Ȃ̂�MemoryStream�Ɉ�x��荞��ŗ��p����
            using (MemoryStream ms = new MemoryStream())
            {
                //MemoryStream�Ɏ�荞��
                int len;
                byte[] buffer = new byte[16384];
                while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, len);

                //�擪��Rewind
                ms.Seek(0, 0);  //�d�v�I

                //�A�C�R���ǂݎ��J�n
                //ICONDIR�\���̂�ǂݎ��
                Byte[] ICONDIR = new byte[6];
                ms.Read(ICONDIR, 0, 6);

                //�A�C�R���t�@�C���`�F�b�N
                if (ICONDIR[0] != 0
                    || ICONDIR[1] != 0
                    || ICONDIR[2] != 1
                    || ICONDIR[3] != 0)
                    return null;

                //������A�C�R���̐����擾
                //int idCount = ICONDIR[4] + ICONDIR[5] * 256;
                int idCount = (int)BitConverter.ToInt16(ICONDIR, 4);

                //ICONDIRENTRY�\���̂̓ǂݎ��
                //��ԑ傫���A�F�[�x�̍����A�C�R�����擾����
                Byte[] ICONDIRENTRY = new byte[16];
                int bWidth = 0;                 //�A�C�R���̕�
                int bHeight = 0;                //�A�C�R���̍���
                int Item;                       //�Ώۂ̃A�C�e���ԍ��i�Ӗ��Ȃ��j
                int wBitCount = 0;              //�F�[�x
                UInt32 dwBytesInRes = 0;        //�ΏۃC���[�W�̃o�C�g��
                UInt32 dwImageOffset = 0;       //�ΏۃC���[�W�̃I�t�Z�b�g

                for (int i = 0; i < idCount; i++)
                {
                    //��ԑ傫��,�F�[�x�̍����A�C�R����T��
                    ms.Read(ICONDIRENTRY, 0, 16);
                    int width = (int)ICONDIRENTRY[0];
                    if (width == 0)
                        width = 256;    //0��256���Ӗ�����B�قڊm����PNG
                    int height = (int)ICONDIRENTRY[1];
                    if (height == 0)
                        height = 256;
                    width = width >= height ? width : height;   //�傫���������
                    int colorDepth = BitConverter.ToUInt16(ICONDIRENTRY, 6);
                    if (width >= bWidth && colorDepth >= wBitCount)
                    {
                        Item = i;
                        bWidth = width;
                        wBitCount = colorDepth;
                        dwBytesInRes = BitConverter.ToUInt32(ICONDIRENTRY, 8);
                        dwImageOffset = BitConverter.ToUInt32(ICONDIRENTRY, 12);
                        Debug.WriteLine(string.Format(
                            "Item={0}, bWidth={1}, dwimageOffset={2}, dwBytesInRes={3}",
                            Item,
                            bWidth,
                            dwImageOffset,
                            dwBytesInRes),
                            "ICONDIRENTRY");
                    }
                }

                //BITMAPINFOHEADER�\����
                Byte[] BITMAPINFOHEADER = new byte[40];
                ms.Seek(dwImageOffset, SeekOrigin.Begin);
                ms.Read(BITMAPINFOHEADER, 0, 40);
                if (BITMAPINFOHEADER[1] == (byte)'P'
                    && BITMAPINFOHEADER[2] == (byte)'N'
                    && BITMAPINFOHEADER[3] == (byte)'G')
                {
                    //PNG�f�[�^�ł����B
                    ms.Seek(dwImageOffset, SeekOrigin.Begin);
                    using (MemoryStream PngStream = new MemoryStream(ms.GetBuffer(), (int)dwImageOffset, (int)dwBytesInRes))
                    {
                        PngStream.Seek(0, SeekOrigin.Begin);
                        Bitmap png = new Bitmap(PngStream);
                        return png;
                    }
                }
                else
                {
                    UInt16 biBitCount = BitConverter.ToUInt16(BITMAPINFOHEADER, 14);
                    UInt32 biCompression = BitConverter.ToUInt32(BITMAPINFOHEADER, 16);
                    Debug.WriteLine(string.Format(
                        "biBitCount={0}, biCompression={1}",
                        biBitCount,
                        biCompression),
                        "BITMAPINFOHEADER");

                    //�F������p���b�g�����v�Z
                    int PALLET = 0;
                    if (biBitCount > 0 && biBitCount <= 8)
                        PALLET = (int)Math.Pow(2, biBitCount);

                    //BITMAPFILEHEADER(14)�����ABitmap�N���X���ǂݎ���悤��
                    //Bitmap�f�[�^�����B
                    //�\����
                    // BIMAPFILEHEADER(14)	:�蓮�ō쐬
                    // BITMAPINFOHEADER(40)	:���̂܂ܗ��p
                    // RGBQUAD(PALLET*4)	:���̂܂ܗ��p
                    // IMAGEDATA + MASK		:���̂܂ܗ��p
                    //
                    byte[] BMP = new byte[14 + dwBytesInRes];
                    Array.Clear(BMP, 0, 14);    //�擪14�o�C�g�͊m���ɂO��
                    BMP[0] = (byte)'B';
                    BMP[1] = (byte)'M';
                    UInt32 dwSize = 14 + dwBytesInRes;
                    byte[] tmp1 = BitConverter.GetBytes(dwSize);
                    BMP[2] = tmp1[0];
                    BMP[3] = tmp1[1];
                    BMP[4] = tmp1[2];
                    BMP[5] = tmp1[3];
                    int bfOffBits = 14 + 40 + PALLET * 4;//BITMAPFILEHEADER(14) + BitmapInfoHeader(40) + PALLET*4
                    byte[] tmp = BitConverter.GetBytes(bfOffBits);
                    BMP[10] = tmp[0];
                    BMP[11] = tmp[1];
                    BMP[12] = tmp[2];
                    BMP[13] = tmp[3];
                    ms.Seek(dwImageOffset, SeekOrigin.Begin);
                    ms.Read(BMP, 14, (int)dwBytesInRes);

                    //������������������,�}�X�N�Ŕ{�ɂȂ��Ă���̂Ŕ�����
                    int bmpWidth = BitConverter.ToInt32(BMP, 14 + 4);
                    int bmpHeight = BitConverter.ToInt32(BMP, 14 + 8);
                    bmpHeight /= 2;
                    byte[] hArray = BitConverter.GetBytes(bmpHeight);
                    BMP[14 + 8] = hArray[0];
                    BMP[14 + 9] = hArray[1];
                    BMP[14 + 10] = hArray[2];
                    BMP[14 + 11] = hArray[3];
                    //BMP[14 + 9] = BMP[14 + 9 - 4];
                    //BMP[14 + 10] = BMP[14 + 10 - 4];
                    //BMP[14 + 11] = BMP[14 + 11 - 4];

                    //��ԑ傫���A�C�R�����擾����
                    //ms.Seek(dwImageOffset, SeekOrigin.Begin);
                    MemoryStream ImageStream = new MemoryStream(BMP);
                    ImageStream.Seek(0, SeekOrigin.Begin);
                    //Bitmap newbmp = new Bitmap(ImageStream);
                    Bitmap newbmp;
                    if (biBitCount == 32 && biCompression == 0)
                    {
                        //32bit�Ȃ̂ŃA���t�@�`���l����ǂݍ���

                        //UnSafe��
                        newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                        ImageStream.Seek(14 + 40 + PALLET * 4, SeekOrigin.Begin);
                        Rectangle lockRect = new Rectangle(0, 0, bmpWidth, bmpHeight);
                        BitmapData bmpData = newbmp.LockBits(lockRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        unsafe
                        {
                            byte* offset = (byte*)bmpData.Scan0;
                            int writePos;
                            for (int y = bmpHeight - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < bmpWidth; x++)
                                {
                                    //4byte������������
                                    writePos = x * 4 + bmpData.Stride * y;
                                    offset[writePos + 0] = (byte)ImageStream.ReadByte(); // B;
                                    offset[writePos + 1] = (byte)ImageStream.ReadByte(); // G;
                                    offset[writePos + 2] = (byte)ImageStream.ReadByte(); // R;
                                    offset[writePos + 3] = (byte)ImageStream.ReadByte(); // A;
                                }//for x
                            }//for y
                        }//unsafe
                        newbmp.UnlockBits(bmpData);

                        ////Manage(Safe)��
                        ////32bit�Ȃ̂ŃA���t�@�`���l����ǂݍ���
                        //newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                        //ImageStream.Seek(14 + 40 + PALLET * 4, SeekOrigin.Begin);
                        //for (int y = bmpHeight - 1; y >= 0; y--)
                        //{
                        //    for (int x = 0; x < bmpWidth; x++)
                        //    {
                        //        //8bit������������
                        //        int B = ImageStream.ReadByte();
                        //        int G = ImageStream.ReadByte();
                        //        int R = ImageStream.ReadByte();
                        //        int A = ImageStream.ReadByte();
                        //        newbmp.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                        //    }//for x
                        //}//for y
                    }
                    else
                    {
                        newbmp = new Bitmap(ImageStream, true);
                    }

                    //�}�X�Nbit�Ή�
                    //32bit�摜�̏ꍇ�͉摜���ŃA���t�@�`���l���������Ă���̂Ŗ���

                    //Manage��
                    //ver�F�F�[�x���Ⴂ�ꍇSetPixcel()���G���[��f��
                    //SetPixel �́A�C���f�b�N�X�t���s�N�Z���`���̃C���[�W�ɑ΂��ăT�|�[�g����Ă��܂���B
                    //if (biBitCount < 32)
                    //{
                    //    Rectangle rc = new Rectangle(0, 0, bmpWidth, bmpHeight);
                    //    if (newbmp.PixelFormat != PixelFormat.Format32bppArgb)
                    //    {
                    //        Bitmap tmpBmp = newbmp;
                    //        newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                    //        using (Graphics g = Graphics.FromImage(newbmp))
                    //        {
                    //            g.DrawImage(tmpBmp, rc);
                    //        }
                    //        tmpBmp.Dispose();
                    //    }

                    //    int maskSize = bmpWidth * bmpHeight / 8;
                    //    long maskOffset = dwImageOffset + dwBytesInRes - maskSize;
                    //    ms.Seek(maskOffset, SeekOrigin.Begin);
                    //    for (int y = bmpHeight - 1; y >= 0; y--)
                    //        for (int x8 = 0; x8 < bmpWidth / 8; x8++)
                    //        {
                    //            //8bit������������
                    //            byte mask = (byte)ms.ReadByte();
                    //            byte checkBit = 0x80;
                    //            for (int xs = 0; xs < 8; xs++)
                    //            {
                    //                if ((mask & checkBit) != 0)
                    //                {
                    //                    newbmp.SetPixel(x8 * 8 + xs, y, Color.Transparent);
                    //                }
                    //                checkBit /= 2;
                    //            }
                    //        }
                    //}

                    //�}�X�Nbit�Ή�
                    //32bit�摜�̏ꍇ�͉摜���ŃA���t�@�`���l���������Ă���̂Ŗ���
                    //unsafe��
                    //lockBites()��PixelFormat��Indexed�ɑΉ����Ă��Ȃ��̂ŕϊ�����K�v������B
                    Rectangle rc = new Rectangle(0, 0, bmpWidth, bmpHeight);
                    if (biBitCount < 32)
                    {
                        //PixelFormat�������I��Format32bppArgb�ɕϊ�����
                        if (newbmp.PixelFormat != PixelFormat.Format32bppArgb)
                        {
                            Bitmap tmpBmp = newbmp;
                            newbmp = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(newbmp))
                            {
                                g.DrawImage(tmpBmp, rc);
                            }
                            tmpBmp.Dispose();
                        }

                        //�}�X�N��ǂݍ���
                        Debug.WriteLine("Load Mask");
                        int maskSize = bmpWidth / 8 * bmpHeight;
                        if (bmpWidth % 32 != 0)         //1���C��4�o�C�g�i32bit�j�P�ʂɂ���B
                            maskSize = (bmpWidth / 32 + 1) * 4 * bmpHeight;
                        long maskOffset = dwImageOffset + dwBytesInRes - maskSize;
                        ms.Seek(maskOffset, SeekOrigin.Begin);
                        BitmapData bd = newbmp.LockBits(rc, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        unsafe
                        {
                            byte* pos = (byte*)bd.Scan0;
                            for (int y = bmpHeight - 1; y >= 0; y--)
                            {
                                int bytes = 0;  //1���C�����̃o�C�g���𐔂���
                                for (int x8 = 0; x8 < bmpWidth / 8; x8++)
                                {
                                    //8bit������������
                                    byte mask = (byte)ms.ReadByte();
                                    bytes++;
                                    Debug.Write(mask.ToString("X2"));
                                    byte checkBit = 0x80;
                                    for (int xs = 0; xs < 8; xs++)
                                    {
                                        if ((mask & checkBit) != 0)
                                        {
                                            pos[(x8 * 8 + xs) * 4 + (bd.Stride * y) + 3] = 0;
                                        }
                                        checkBit /= 2;
                                    }
                                }
                                Debug.Write("|");
                                while ((bytes % 4) != 0)
                                {
                                    byte b = (byte)ms.ReadByte();   //�̂Ă�
                                    Debug.Write(b.ToString("X2"));
                                    bytes++;
                                }
                                Debug.WriteLine("");
                            }
                        }//unsafe
                        newbmp.UnlockBits(bd);
                    }//if (biBitCount < 32)

                    //��������
                    return newbmp;
                }
            }
        }
    }

    /********************************************************************************/
    // �\�[�g�p��r�N���X
    // ���R����\�[�g���邽�߂̔�r�N���X
    /********************************************************************************/
    public class NaturalOrderComparer : IComparer<string>
    {
        private int col;            //�R�����F���p���Ȃ�
        private int SortOrder;      //�\�[�g�̕����B������1�A�t����-1

        //private enum SortModeEnum : int
        //{
        //    None = 0,			//�\�[�g���Ȃ�
        //    ascending = 1,		//ascendng:����
        //    descending = 2		//descending:�~��
        //}

        public NaturalOrderComparer()
        {
            col = 0;
            SortOrder = 1;
        }

        public int Compare(string x, string y)
        {
            return SortOrder * NaturalOrderCompare(x, y);
        }

        public int SimpleCompare(object x, object y)
        {
            //�P����r�֐�
            return String.Compare(
                ((ListViewItem)x).SubItems[col].Text,
                ((ListViewItem)y).SubItems[col].Text
            );
        }

        public int NaturalOrderCompare(string s1, string s2)
        {
            //���l����x�ϊ��������ɂ��Ȃ���r������B
            //XP�ȍ~�̃\�[�g�����ɑΉ������͂��E�E�E

            //�K�w���`�F�b�N
            int lev1 = 0;   //x�̊K�w
            int lev2 = 0;   //y�̊K�w
            for (int i = 0; i < s1.Length; i++)
                if (s1[i] == '/' || s1[i] == '\\') lev1++;
            for (int i = 0; i < s2.Length; i++)
                if (s2[i] == '/' || s2[i] == '\\') lev2++;

            if (lev1 != lev2)
                return lev1 - lev2;

            //
            // ����K�w�Ȃ̂�1�������`�F�b�N���J�n����
            //
            int p1 = 0;     // s1���w���|�C���^
            int p2 = 0;     // s2���w���|�C���^
            long num1 = 0;  // s1�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long
            long num2 = 0;  // s2�Ɋ܂܂�鐔�l�B�傫�Ȑ��l�ɑΉ������邽��long

            do
            {
                char c1 = s1[p1];
                char c2 = s2[p2];

                //c1��c2�̔�r���J�n����O�ɐ����������琔�l���[�h��
                //���l���[�h�̏ꍇ�͐��l�ɕϊ����Ĕ�r
                if (c1 >= '0' && c1 <= '9' && c2 >= '0' && c2 <= '9')
                {
                    //s1�n��̕����𐔒lnum1�ɕϊ�
                    num1 = 0;
                    while (c1 >= '0' && c1 <= '9')
                    {
                        num1 = num1 * 10 + c1 - '0';
                        ++p1;
                        if (p1 >= s1.Length)
                            break;
                        c1 = s1[p1];
                    }

                    //s2�n��̕����𐔒lnum2�ɕϊ�
                    num2 = 0;
                    while (c2 >= '0' && c2 <= '9')
                    {
                        num2 = num2 * 10 + c2 - '0';
                        ++p2;
                        if (p2 >= s2.Length)
                            break;
                        c2 = s2[p2];
                    }

                    //���l�Ƃ��Ĕ�r
                    if (num1 != num2)
                        return (int)(num1 - num2);
                }
                else
                {
                    //�P�ꕶ���Ƃ��Ĕ�r
                    if (c1 != c2)
                        return (int)(c1 - c2);
                    ++p1;
                    ++p2;
                }
            }
            while (p1 < s1.Length && p2 < s2.Length);

            //�ǂ��炩���I�[�ɒB�����B���Ƃ͒����������B
            return s1.Length - s2.Length;
        }
    }

    /********************************************************************************/
    //���Ԃ�����Ǝ����I�ɏ����郉�x��
    /********************************************************************************/
    public class InformationLabel : Label
    {
        private System.Windows.Forms.Timer t1 = null;
        private const int TimerInterval = 1000;

        /// <summary>
        /// �R���X�g���N�^�B�ʒu�w��\
        /// </summary>
        /// <param name="parentControl">Label��ǉ�����R���g���[��</param>
        /// <param name="x">�\���ʒux(Left)</param>
        /// <param name="y">�\���ʒuy(Top)</param>
        /// <param name="sz">�\�����镶����</param>
        public InformationLabel(Control parentControl, int x, int y, string sz)
        {
            this.Name = sz;
            this.Text = sz;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Left = x;
            this.Top = y;
            this.Enabled = true;
            this.Visible = true;
            parentControl.Controls.Add(this);
            this.Show();

            t1 = new System.Windows.Forms.Timer();
            t1.Interval = TimerInterval;
            t1.Tick += new EventHandler(t1_Tick);
            t1.Start();
            Debug.WriteLine("Show InformationLabel");
        }

        /// <summary>
        /// �R���X�g���N�^�B�I�u�W�F�N�g�̐^�񒆂ɕ\������
        /// </summary>
        /// <param name="parentControl">Label��ǉ�����I�u�W�F�N�g</param>
        /// <param name="sz">�\�����镶����</param>
        public InformationLabel(Control parentControl, string sz)
        {
            this.Name = sz;
            this.Text = sz;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Enabled = true;
            this.Visible = true;

            int cx = parentControl.Width;
            int cy = parentControl.Height;
            this.Left = (cx - this.PreferredWidth) / 2;
            this.Top = (cy - this.PreferredHeight) / 2;

            parentControl.Controls.Add(this);
            this.Show();

            t1 = new System.Windows.Forms.Timer();
            t1.Interval = TimerInterval;
            t1.Tick += new EventHandler(t1_Tick);
            t1.Start();
            Debug.WriteLine("Show InformationLabel");
        }

        /// <summary>
        /// �^�C�}�[Tick�Ăяo���p�֐�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void t1_Tick(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            t1.Stop();
            t1.Dispose();
            this.Dispose();
            Debug.WriteLine("Dispose InformationLabel");
        }
    }

    /// <summary>
    /// �ア�Q�Ƃɂ��L���b�V���N���X
    /// ���܂���������肾���A�ǂ����Ă�Bitmap���Ӑ}�����Q�Ƃł��Ȃ����Ƃ�����̂�
    /// ���p���~
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Cache<TKey, TValue>
    {
        // �L���b�V����ۑ�����Dictionary
        static Dictionary<TKey, WeakReference> _cache;

        //�R���X�g���N�^
        public Cache()
        {
            _cache = new Dictionary<TKey, WeakReference>();
        }

        /// <summary>
        /// �L���b�V���ւ̃A�C�e���̒ǉ�
        /// </summary>
        /// <param name="key">�A�C�e�������ʂ���L�[</param>
        /// <param name="obj">�A�C�e��</param>
        /// <param name="isLongCache">�L���b�V���ێ��𒷂�����A�C�e���̏ꍇ��true</param>
        public void Add(TKey key, TValue obj, bool isLongCache)
        {
            _cache.Add(
                key,
                new WeakReference(obj, isLongCache)
                );
        }

        public void Add(TKey key, TValue obj)
        {
            Add(key, obj, true);
        }

        /// <summary>
        /// �L���b�V���Ɋ܂܂�Ă��鐔��
        /// </summary>
        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        /// <summary>
        /// �w�肵���L�[�̃A�C�e����Ԃ�
        /// </summary>
        /// <param name="key">�A�C�e�����w�肷��L�[</param>
        /// <returns>�A�C�e���I�u�W�F�N�g�B���ł��Ă���ꍇ��null��Ԃ�</returns>
        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    TValue d = (TValue)_cache[key].Target;
                    return d;
                }
                catch
                {
                    // �L�[�����݂��Ȃ��ꍇ�Ȃ�
                    return default(TValue);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (!_cache.ContainsKey(key))
                return false;
            try
            {
                TValue d = (TValue)_cache[key].Target;
                if (d != null)
                    return true;
                else
                    return false;
            }
            catch
            {
                // �L�[�����݂��Ȃ��ꍇ�Ȃ�
                return false;
            }
        }
    }

    /// <summary>
    /// �������̂��߂ɂ���Ȃ��Ȃ������W���[�����������
    /// </summary>
    public class ThumbnailPanel : UserControl
    {
        /// <summary>
        /// �T���l�C���ꗗ�̍쐬���C���֐�
        /// ��������ŁB
        /// �X���b�h�Ȃǂ͎g�킸�ɃT���l�C���ꗗ���Ђ�������B
        /// ���T�C�Y�ɍ����Ή�������Ȃ����߈���
        /// </summary>
        public void drawThumbnailToOffScreen()
        {
            //�`�ʑΏۂ����邩�`�F�b�N����
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            InitilizeOffScreen();
            using (Graphics g = Graphics.FromImage(m_bmpOffScreen))
            {
                //�w�i�F�œh��Ԃ�
                g.Clear(this.BackColor);
                for (int i = 0; i < m_thumbnailSet.Count; i++)
                {
                    DrawItem(g, i);
                }//foreach
            } //using(Graphics)

            //�X�N���[���o�[��ݒ�
            setScrollBar();
        }

        /// <summary>
        /// �T���l�C����`�ʂ���
        /// OnResize()��p�Ƃ��ĉ�ʕ`�ʂɕK�v�ȕ��������`�ʂĂ���
        /// </summary>
        public void drawThumbnailToOffScreenFast()
        {
            //�`�ʑΏۂ����邩�`�F�b�N����
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //InitilizeOffScreen();
            int ItemCount = m_thumbnailSet.Count;

            //�`�ʂɕK�v�ȃT�C�Y���m�F����B
            //�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
            int width = this.ClientRectangle.Width;
            int height = this.ClientRectangle.Height;

            //���ɕ��ׂ��鐔�B�Œ�P
            int OldNumItemX = numItemX;
            int OldNumItemY = numItemY;
            numItemX = width / BOX_WIDTH;   //���ɕ��ԃA�C�e����
            if (numItemX == 0)
                numItemX = 1;

            //�c�ɕK�v�Ȑ��B�J��グ��
            numItemY = ItemCount / numItemX;    //�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
            if (ItemCount % numItemX > 0)
                numItemY++;

            if (height < numItemY * BOX_HEIGHT)
            {
                //�X�N���[���o�[���K�v�Ȃ̂ōČv�Z
                width -= vScrollBar1.Width;
                numItemX = width / BOX_WIDTH;
                if (numItemX == 0)
                    numItemX = 1;
                numItemY = (ItemCount + numItemX - 1) / numItemX;   //(numX-1)�����炩���ߑ����Ă������ƂŌJ��グ
                height = numItemY * BOX_HEIGHT;
                vScrollBar1.Visible = true;
                vScrollBar1.Enabled = true;
            }
            else
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Enabled = false;
                vScrollBar1.Value = 0;
            }
            if (width < 1) width = 1;
            if (height < 1) height = 1;

            //if (width <= m_bmpOffScreen.Width
            //    && height <= m_bmpOffScreen.Height
            //    && OldNumItemX == numItemX)
            //{
            //    return;
            //}

            //�ĕ`�ʂ��K�v
            //m_bmpOffScreen��j���A��������B�ė��p�ł���ꍇ�͔j�����Ȃ��B
            if (m_bmpOffScreen != null)
                m_bmpOffScreen.Dispose();
            m_bmpOffScreen = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(m_bmpOffScreen))
            {
                //�w�i�F�œh��Ԃ�
                g.Clear(this.BackColor);

                //�֌W�������ȃA�C�e�������`��
                //�ĕ`�ʑΏۂ��m�F
                for (int Item = 0; Item < ItemCount; Item++)
                {
                    //int ItemX = Item % numItemX;	//�A�C�e����X�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
                    //int sx = ItemX * BOX_WIDTH;		//�摜�`��X�ʒu
                    int ItemY = Item / numItemX;    //�A�C�e����Y�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
                    int sy = ItemY * BOX_HEIGHT;    //�摜�`��X�ʒu

                    if ((sy + BOX_HEIGHT) > vScrollBar1.Value && sy < (vScrollBar1.Value + this.Height))
                    {
                        DrawItem(g, Item);
                    }
                }
            } //using(Graphics)

            //�X�N���[���o�[��ݒ�
            setScrollBar();
        }

        private void drawDropShadow(Graphics g, int sx, int sy, int w, int h)
        {
            GraphicsPath Path = new GraphicsPath(FillMode.Winding);

            //�p�̊ۂ�
            Size arcSize = new Size(4, 4);

            //3�h�b�g���炵�ď���
            //Rectangle rect = new Rectangle(sx + 2, sy + 2, w, h);
            Rectangle rect = new Rectangle(sx + 3, sy + 3, w, h);

            Path.AddArc(rect.Right - arcSize.Width, rect.Top, arcSize.Width, arcSize.Height, 270, 90);
            Path.AddArc(rect.Right - arcSize.Width, rect.Bottom - arcSize.Height, arcSize.Width, arcSize.Height, 0, 90);
            Path.AddArc(rect.Left, rect.Bottom - arcSize.Height, arcSize.Width, arcSize.Height, 90, 90);
            Path.AddArc(rect.Left, rect.Top, arcSize.Width, arcSize.Height, 180, 90);
            Path.AddArc(rect.Right - arcSize.Width, rect.Top, arcSize.Width, arcSize.Height, 270, 90);

            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            using (PathGradientBrush br = new PathGradientBrush(Path))
            {
                // set the wrapmode so that the colors will layer themselves
                // from the outer edge in
                br.WrapMode = WrapMode.Clamp;

                // Create a color blend to manage our colors and positions and
                // since we need 3 colors set the default length to 3
                ColorBlend _ColorBlend = new ColorBlend(3);

                // here is the important part of the shadow making process, remember
                // the clamp mode on the colorblend object layers the colors from
                // the outside to the center so we want our transparent color first
                // followed by the actual shadow color. Set the shadow color to a
                // slightly transparent DimGray, I find that it works best.|
                _ColorBlend.Colors = new Color[]
                        {
                            Color.Transparent,
                            Color.FromArgb(180, Color.DimGray),
                            Color.FromArgb(180, Color.DimGray)
                        };

                // our color blend will control the distance of each color layer
                // we want to set our transparent color to 0 indicating that the
                // transparent color should be the outer most color drawn, then
                // our Dimgray color at about 10% of the distance from the edge
                _ColorBlend.Positions = new float[] { 0f, .1f, 1f };

                // assign the color blend to the pathgradientbrush
                br.InterpolationColors = _ColorBlend;

                // fill the shadow with our pathgradientbrush
                g.FillPath(br, Path);
            }
        }

        private void InitilizeOffScreen()
        {
            int ItemCount = m_thumbnailSet.Count;

            //�`�ʂɕK�v�ȃT�C�Y���m�F����B

            //�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
            int width = this.ClientRectangle.Width;
            int height = this.ClientRectangle.Height;

            //���ɕ��ׂ��鐔�B�Œ�P
            numItemX = width / BOX_WIDTH;   //���ɕ��ԃA�C�e����
            if (numItemX == 0)
                numItemX = 1;

            //�c�ɕK�v�Ȑ��B�J��グ��
            numItemY = ItemCount / numItemX;    //�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
            if (ItemCount % numItemX > 0)
                numItemY++;

            if (height < numItemY * BOX_HEIGHT)
            {
                //�X�N���[���o�[���K�v�Ȃ̂ōČv�Z
                width -= vScrollBar1.Width;
                numItemX = width / BOX_WIDTH;
                if (numItemX == 0)
                    numItemX = 1;
                numItemY = (ItemCount + numItemX - 1) / numItemX;   //(numX-1)�����炩���ߑ����Ă������ƂŌJ��グ
                height = numItemY * BOX_HEIGHT;
                vScrollBar1.Visible = true;
                vScrollBar1.Enabled = true;
            }
            else
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Enabled = false;
                vScrollBar1.Value = 0;
            }
            if (width < 1) width = 1;
            if (height < 1) height = 1;

            //m_bmpOffScreen��j���A��������B�ė��p�ł���ꍇ�͔j�����Ȃ��B
            if (m_bmpOffScreen != null)
            {
                if (m_bmpOffScreen.Width != width
                    || m_bmpOffScreen.Height != height)
                {
                    m_bmpOffScreen.Dispose();
                    m_bmpOffScreen = null;
                }
            }
            if (m_bmpOffScreen == null)
                m_bmpOffScreen = new Bitmap(width, height);
        }

        /// <summary>
        /// �T���l�C���ꗗ�̍쐬���C���֐�
        /// �X���b�h�Ή��ŁB��������ڎw���Ă����猋�ǂ����Ȃ����B
        /// �\�[�g�����Ȃǋ����ĕ`�ʂ���K�v������ꍇ�̓t���O��true�ɂ���
        /// </summary>
        /// <param name="isForceRedraw">�����ĕ`�ʂ���ꍇ��true</param>
        public void MakeThumbnailScreen(bool isForceRedraw)
        {
            //�`�ʑΏۂ����邩�`�F�b�N����
            if (m_thumbnailSet == null || m_thumbnailSet.Count == 0)
                return;

            //InitilizeOffScreen();
            int ItemCount = m_thumbnailSet.Count;

            //�`�ʂɕK�v�ȃT�C�Y���m�F����B
            //�`�ʗ̈�̑傫���B�܂��͎����̃N���C�A���g�̈�𓾂�
            int width = this.ClientRectangle.Width;
            int height = this.ClientRectangle.Height;

            //���ɕ��ׂ��鐔�B�Œ�P
            int OldNumItemX = numItemX;
            int OldNumItemY = numItemY;
            numItemX = width / BOX_WIDTH;   //���ɕ��ԃA�C�e����
            if (numItemX == 0)
                numItemX = 1;

            //�c�ɕK�v�Ȑ��B�J��グ��
            numItemY = ItemCount / numItemX;    //�c�ɕ��ԃA�C�e�����̓T���l�C���̐��ɂ��
            if (ItemCount % numItemX > 0)
                numItemY++;

            if (height < numItemY * BOX_HEIGHT)
            {
                //�X�N���[���o�[���K�v�Ȃ̂ōČv�Z
                width -= vScrollBar1.Width;
                numItemX = width / BOX_WIDTH;
                if (numItemX == 0)
                    numItemX = 1;
                numItemY = (ItemCount + numItemX - 1) / numItemX;   //(numX-1)�����炩���ߑ����Ă������ƂŌJ��グ
                height = numItemY * BOX_HEIGHT;
                vScrollBar1.Visible = true;
                vScrollBar1.Enabled = true;
            }
            else
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Enabled = false;
                vScrollBar1.Value = 0;
            }
            if (width < 1) width = 1;
            if (height < 1) height = 1;

            //�����ς��Ȃ��Ƃ��͉��������o�C�o�C
            if (OldNumItemX == numItemX && isForceRedraw == false)
            {
                return;
            }

            //�ĕ`�ʂ��K�v
            //�X���b�h���E��
            if (tStatus == ThreadStatus.RUNNING)
            {
                tStatus = ThreadStatus.REQUEST_STOP;
                WaitForMakeThumbnailThread();
            }

            //m_bmpOffScreen��j���A��������B
            if (m_offScreen == null)
                m_offScreen = new Bitmap(width, height);
            else
            {
                lock (m_offScreen)
                {
                    m_offScreen.Dispose();
                    m_offScreen = new Bitmap(width, height);
                }
            }

            //�X�N���[���o�[��ݒ�
            setScrollBar();

            //�X���b�h����
            //if(mrEvent == null)
            //    mrEvent = new ManualResetEvent(false);
            WaitCallback callback = new WaitCallback(MakeThumbnailThreadProc);
            ThreadPool.QueueUserWorkItem(callback);
        }

        public void WaitForMakeThumbnailThread()
        {
            while (tStatus != ThreadStatus.STOP)
            {
                Application.DoEvents(); //���̕`�ʃ��b�Z�[�W�����������̂͗ǂ��Ȃ�
                                        //Thread.Sleep(10);
            }
        }

        //�T���l�C���쐬���X���b�h�����邽�߂ɍ�����֐�
        //�傫���A�C�R���Ή��̂��߂�������
        private void MakeThumbnailThreadProc(object o)
        {
            Debug.WriteLine("start MakeThumbnailThreadProc(object o)");
            tStatus = ThreadStatus.RUNNING;

            for (int Item = 0; Item < m_thumbnailSet.Count; Item++)
            {
                if (tStatus != ThreadStatus.RUNNING)
                    break;

                lock (m_offScreen)
                {
                    using (Graphics g = Graphics.FromImage(m_offScreen))    //������lock����K�v�L��
                    {
                        DrawItem(g, Item);      //lock
                                                //this.Invalidate();	//�{���͖����ɂ��������ǎ��ܐ^�����Ȃ񂾂��
                    }//using
                }
            }//for

            this.Invalidate();
            tStatus = ThreadStatus.STOP;
            Debug.WriteLine("stop MakeThumbnailThreadProc(object o)");
        }

        //�A�C�e���`�ʃ��[�`���B���Ȃ�͍�
        //�傫���A�C�R���Ή��̂��߂�������
        private void DrawItem(Graphics g, int i)
        {
            //�`�ʕi��
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //�摜�ɘg��������
            bool drawFrame = true;

            //�`�ʂ��ׂ��C���[�W�̊m�F
            Image DrawBitmap = m_thumbnailSet[i].ThumbImage;
            if (DrawBitmap == null)
            {
                DrawBitmap = Properties.Resources.rc_tif48;
                drawFrame = false;
            }

            //�`�ʈʒu�i�A�C�e���ԍ��ʒu�j�̌���
            int ItemX = i % numItemX;   //�A�C�e����X�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
            int ItemY = i / numItemX;   //�A�C�e����Y�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu

            //�`�ʃA�C�e���̑傫�����m��
            int w = DrawBitmap.Width;   //�`�ʉ摜�̕�
            int h = DrawBitmap.Height;  //�`�ʉ摜�̍���
            float ratio = 1;

            if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE)
            {
                if (w > h)
                    ratio = (float)THUMBNAIL_SIZE / (float)w;
                else
                    ratio = (float)THUMBNAIL_SIZE / (float)h;
                //if (ratio > 1)		//������R�����g�������
                //    ratio = 1.0F;		//�g��`�ʂ��s��
                w = (int)(w * ratio);
                h = (int)(h * ratio);
            }

            //�摜�ʒu�F�������͐^�񒆂�I�o����
            int sx = ItemX * BOX_WIDTH + (BOX_WIDTH - w) / 2;   //�摜�`��X�ʒu

            //�摜�ʒu�F�c�����͉���������
            //int sy = y * BOX_HEIGHT + (BOX_HEIGHT - h) / 2;	//��������
            int sy = ItemY * BOX_HEIGHT + THUMBNAIL_SIZE + PADDING - h; //�摜�`��X�ʒu�F������

            //�e������
            //g.FillRectangle(Brushes.LightGray, sx + 1, sy + 1, w, h);  //�ȈՔ�
            //drawDropShadow(g, sx, sy, w, h);	//�ʏ��

            //�Ώۋ�`��w�i�F�œh��Ԃ�.
            //�������Ȃ��ƑO�ɕ`�����A�C�R�����c���Ă��܂��\���L��
            Brush br = new SolidBrush(BackColor);
            g.FillRectangle(br, ItemX * BOX_WIDTH, ItemY * BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);

            //�ʐ^���ɊO�g������
            if (drawFrame)
            {
                Rectangle r = new Rectangle(sx, sy, w, h);
                r.Inflate(2, 2);
                g.FillRectangle(Brushes.White, r);
                g.DrawRectangle(Pens.LightGray, r);
            }

            //�摜������
            //TODO:����lock()�͕s�v�Ȃ͂��Ȃ̂Ŗ��Ȃ���Ώ����B2009�N8��10��
            lock (DrawBitmap)
            {
                g.DrawImage(DrawBitmap, sx, sy, w, h);
            }

            //��������������
            sx = ItemX * BOX_WIDTH + PADDING;
            sy = ItemY * BOX_HEIGHT + PADDING + THUMBNAIL_SIZE + PADDING;
            string drawString = Path.GetFileName(m_thumbnailSet[i].filename);
            RectangleF rect = new RectangleF(sx, sy, THUMBNAIL_SIZE, TEXT_HEIGHT);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;          //��������
            sf.Trimming = StringTrimming.EllipsisPath;      //���Ԃ̏ȗ�
                                                            //sf.FormatFlags = StringFormatFlags.NoWrap;		//�܂�Ԃ����֎~
            g.DrawString(drawString, font, Brushes.Black, rect, sf);
        }

        //�T���l�C���쐬���[�`���i�X���b�h�Łj
        //�S�A�C�e�����T�[�`���Ă���B
        //���肬��̃A�C�e�������`�ʂ��郋�[�`���Ɏ���đ���ꂽ�B
        private void ThreadProc_Old1(object dummy)
        {
            ////�����ڕs��
            //if (tStatus == ThreadStatus.RUNNING)
            //    return;

            int ItemCount = m_thumbnailSet.Count;
            tStatus = ThreadStatus.RUNNING;

            //�֌W�������ȃA�C�e�������`��
            for (int Item = 0; Item < ItemCount; Item++)
            {
                if (tStatus == ThreadStatus.REQUEST_STOP)
                    break;

                lock (m_offScreen)
                {
                    DrawItemHQ(Graphics.FromImage(m_offScreen), Item);
                    this.Invalidate();
                }
            }
            tStatus = ThreadStatus.STOP;
        }

        //�����`�ʑΉ�DrawItem
        //Image�̑傫���ȂǑS�����O�ŏ������Ă���
        //�O���ɂ͂��o�������m��DrawImage3()
        private void DrawItem2(Graphics g, int Item)
        {
            //�������o���Ă��邩
            if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
            {
                Debug.WriteLine("�����ł��ĂȂ���", " DrawItem2()");
                return;
            }

            //�`�ʕi��
            if (THUMBNAIL_SIZE > DEFAULT_THUMBNAIL_SIZE)
            {
                //�`�������̂ōŒ�i���ŕ`�ʂ���
                //g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.InterpolationMode = InterpolationMode.Bilinear;           //���ꂮ�炢�̕i���ł�OK���H
                m_needHQDraw = true;    //�`�������t���O
            }
            else
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; //�ō��i����

            int ItemX = Item % m_nItemsX;   //�A�C�e����X�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
            int sx = ItemX * BOX_WIDTH;     //�摜�`��X�ʒu
            int ItemY = Item / m_nItemsX;   //�A�C�e����Y�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
            int sy = ItemY * BOX_HEIGHT;    //�摜�`��X�ʒu

            if ((sy + BOX_HEIGHT) > m_vScrollBar.Value && sy < (m_vScrollBar.Value + this.Height))
            {
                //�Ώۋ�`��w�i�F�œh��Ԃ�.
                //�������Ȃ��ƑO�ɕ`�����A�C�R�����c���Ă��܂��\���L��
                g.FillRectangle(new SolidBrush(BackColor), sx, sy, BOX_WIDTH, BOX_HEIGHT);

                bool drawFrame = true;

                Image DrawBitmap = m_thumbnailSet[Item].ThumbImage;
                bool isResize = true;   //���T�C�Y���K�v���i�\���j�ǂ����̃t���O

                int w;  //�`�ʉ摜�̕�
                int h;  //�`�ʉ摜�̍���

                if (DrawBitmap == null)
                {
                    //�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
                    DrawBitmap = Properties.Resources.rc_tif32;
                    drawFrame = false;
                    isResize = false;
                    w = DrawBitmap.Width;   //�`�ʉ摜�̕�
                    h = DrawBitmap.Height;  //�`�ʉ摜�̍���
                }
                else
                {
                    //�T���l�C���͂���
                    w = DrawBitmap.Width;   //�`�ʉ摜�̕�
                    h = DrawBitmap.Height;  //�`�ʉ摜�̍���

                    //���T�C�Y���ׂ����ǂ����m�F����B
                    if (m_thumbnailSet[Item].originalWidth <= DEFAULT_THUMBNAIL_SIZE
                        && m_thumbnailSet[Item].originalHeight <= DEFAULT_THUMBNAIL_SIZE)
                        isResize = false;
                    //if (w <= THUMBNAIL_SIZE && h <= THUMBNAIL_SIZE)
                    //    isResize = false;
                }

                //�����\�������郂�m�͏o���邾�������Ƃ���
                if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
                //if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE
                //    && (w >= DEFAULT_THUMBNAIL_SIZE - 1 || h >= DEFAULT_THUMBNAIL_SIZE - 1))
                {
                    //�g��k�����s��
                    float ratio = 1;
                    if (w > h)
                        ratio = (float)THUMBNAIL_SIZE / (float)w;
                    else
                        ratio = (float)THUMBNAIL_SIZE / (float)h;
                    //if (ratio > 1)			//������R�����g�������
                    //    ratio = 1.0F;		//�g��`�ʂ��s��
                    w = (int)(w * ratio);
                    h = (int)(h * ratio);

                    //�I���W�i���T�C�Y���傫���ꍇ�̓I���W�i���T�C�Y�ɂ���
                    if (w > m_thumbnailSet[Item].originalWidth || h > m_thumbnailSet[Item].originalHeight)
                    {
                        w = m_thumbnailSet[Item].originalWidth;
                        h = m_thumbnailSet[Item].originalHeight;
                    }
                }

                sx = sx + (BOX_WIDTH - w) / 2;  //�摜�`��X�ʒu
                sy = sy + THUMBNAIL_SIZE + PADDING - h; //�摜�`��X�ʒu�F������
                sy = sy - m_vScrollBar.Value;   //�摜�`��X�ʒu�F������

                //�ʐ^���ɊO�g������
                if (drawFrame)
                {
                    Rectangle r = new Rectangle(sx, sy, w, h);
                    r.Inflate(2, 2);
                    g.FillRectangle(Brushes.White, r);
                    g.DrawRectangle(Pens.LightGray, r);
                }

                //�摜������
                g.DrawImage(DrawBitmap, sx, sy, w, h);

                //��������������
                sx = ItemX * BOX_WIDTH + PADDING;
                sy = ItemY * BOX_HEIGHT + PADDING + THUMBNAIL_SIZE + PADDING;
                sy = sy - m_vScrollBar.Value;   //�摜�`��X�ʒu�F������
                string drawString = Path.GetFileName(m_thumbnailSet[Item].filename);
                RectangleF rect = new RectangleF(sx, sy, THUMBNAIL_SIZE, TEXT_HEIGHT);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;          //��������
                sf.Trimming = StringTrimming.EllipsisPath;      //���Ԃ̏ȗ�
                                                                //g.DrawString(drawString, font, Brushes.Black, rect, sf);
                g.DrawString(drawString, m_font, new SolidBrush(m_fontColor), rect, sf);
            }
        }

        //���i����p�`��DrawItem
        //GetBipmap()����`�ʂ��Ă���
        //m_offScreen�ɒ��ڕ`�ʂ��Ă��邪�AdummyBmp�ɕ`�ʂ��邱�Ƃ�
        //m_offScreen��lock��Z�����邽�߈���
        private void DrawItemHQ(Graphics g, int Item)
        {
            //�������o���Ă��邩
            if (m_nItemsX == 0 || m_nItemsY == 0 || m_offScreen == null)
            {
                Debug.WriteLine("�����ł��ĂȂ���", " DrawItemHQ()");
                return;
            }

            //���X120x120��菬�����̂ł���Ζ���
            if (m_thumbnailSet[Item].originalWidth <= DEFAULT_THUMBNAIL_SIZE
                && m_thumbnailSet[Item].originalHeight <= DEFAULT_THUMBNAIL_SIZE)
                return;

            //120dot�T���l�C������łȂ��A���摜����T���l�C�����Đ�������B
            //�`�ʕi�����ō���
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int ItemX = Item % m_nItemsX;   //�A�C�e����X�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
            int sx = ItemX * BOX_WIDTH;     //�摜�`��X�ʒu
            int ItemY = Item / m_nItemsX;   //�A�C�e����Y�`�ʈʒu�B�h�b�g�ł͂Ȃ��A�C�e���ԍ��ʒu
            int sy = ItemY * BOX_HEIGHT;    //�摜�`��X�ʒu

            //�`�ʔ͈͂ł��邱�Ƃ��m�F
            if ((sy + BOX_HEIGHT) > m_vScrollBar.Value && sy < (m_vScrollBar.Value + this.Height))
            {
                //�Ώۋ�`��w�i�F�œh��Ԃ�.
                //����BOX������
                //�������Ȃ��ƑO�ɕ`�����A�C�R�����c���Ă��܂��\���L��
                g.FillRectangle(
                    new SolidBrush(BackColor),
                    //Brushes.LightYellow,
                    sx,
                    sy - m_vScrollBar.Value,
                    BOX_WIDTH,
                    BOX_HEIGHT);

                ////�T�C�Y��120�ȏ�̎��͌��t�@�C���������Ă���
                //Image DrawBitmap = m_thumbnailSet[Item].ThumbImage;
                //Image DrawBitmap = ((Form1)Parent).GetBitmapWithoutCache(Item);
                //TODO:Zip�X�g���[���𐳂����X�g���[���ɒ����K�v�L��
                Image DrawBitmap = new Bitmap(((Form1)Parent).GetBitmapWithoutCache(Item));
                bool drawFrame = true;
                bool isResize = true;   //���T�C�Y���K�v���i�\���j�ǂ����̃t���O
                bool isDisposeBitmap = true;
                int w;  //�`�ʉ摜�̕�
                int h;  //�`�ʉ摜�̍���

                if (DrawBitmap == null)
                {
                    //�܂��T���l�C���͏����ł��Ă��Ȃ��̂ŉ摜�}�[�N���Ă�ł���
                    DrawBitmap = Properties.Resources.rc_tif32;
                    drawFrame = false;
                    isResize = false;
                    isDisposeBitmap = false;
                    w = DrawBitmap.Width;   //�`�ʉ摜�̕�
                    h = DrawBitmap.Height;  //�`�ʉ摜�̍���
                }
                else
                {
                    //�T���l�C���͂���
                    w = DrawBitmap.Width;   //�`�ʉ摜�̕�
                    h = DrawBitmap.Height;  //�`�ʉ摜�̍���

                    //���T�C�Y���ׂ����ǂ����m�F����B
                    //if (m_thumbnailSet[Item].originalWidth <= THUMBNAIL_SIZE
                    //    && m_thumbnailSet[Item].originalHeight <= THUMBNAIL_SIZE)
                    if (w <= THUMBNAIL_SIZE && h <= THUMBNAIL_SIZE)
                        isResize = false;
                }

                //�����\�������郂�m�͏o���邾�������Ƃ���
                if (THUMBNAIL_SIZE != DEFAULT_THUMBNAIL_SIZE && isResize == true)
                {
                    float ratio = 1;
                    if (w > h)
                        ratio = (float)THUMBNAIL_SIZE / (float)w;
                    else
                        ratio = (float)THUMBNAIL_SIZE / (float)h;
                    //if (ratio > 1)			//������R�����g�������
                    //    ratio = 1.0F;		//�g��`�ʂ��s��
                    w = (int)(w * ratio);
                    h = (int)(h * ratio);
                }

                sx = sx + (BOX_WIDTH - w) / 2;          //�摜�`��X�ʒu
                sy = sy + THUMBNAIL_SIZE + PADDING - h; //�摜�`��Y�ʒu�F������
                sy = sy - m_vScrollBar.Value;           //�摜�`��Y�ʒu�F�X�N���[���o�[�␳

                //�ʐ^���ɊO�g������
                if (drawFrame)
                {
                    Rectangle r = new Rectangle(sx, sy, w, h);
                    r.Inflate(2, 2);
                    g.FillRectangle(Brushes.White, r);
                    g.DrawRectangle(Pens.LightGray, r);
                }

                //�摜������
                g.DrawImage(DrawBitmap, sx, sy, w, h);

                //��������������
                sx = ItemX * BOX_WIDTH + PADDING;
                sy = ItemY * BOX_HEIGHT + PADDING + THUMBNAIL_SIZE + PADDING;
                sy = sy - m_vScrollBar.Value;   //�摜�`��X�ʒu�F������
                string drawString = Path.GetFileName(m_thumbnailSet[Item].filename);
                RectangleF rect = new RectangleF(sx, sy, THUMBNAIL_SIZE, TEXT_HEIGHT);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;          //��������
                sf.Trimming = StringTrimming.EllipsisPath;      //���Ԃ̏ȗ�
                                                                //g.DrawString(drawString, font, Brushes.Black, rect, sf);
                g.DrawString(drawString, m_font, new SolidBrush(m_fontColor), rect, sf);

                //Bitmap�̔j���BGetBitmapWithoutCache()�Ŏ���Ă�������
                //TODO:Properties.Resources.rc_tif32;���j�����Ă����̂��H
                if (isDisposeBitmap)
                    DrawBitmap.Dispose();
            }
        }

        //ver0.988
        //�摜�`�ʊ֘A
        //�����T�C�Y��g_bmp�𐶐��A�`��.2���`�ʑΉ�
        //�t�����Ή��͕ʂ̏ꏊ�ł���Ă��邽�ߖ���
        //
        // ver0.987 �������ߖ�̂���clone()���ł��邾���r��
        private void DrawImageToGBMP(int nIndex)
        {
            //�����̐��K��
            if (nIndex < 0 || nIndex > g_pi.Items.Count - 1)
                return;

            Cursor cc = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //�Ƃ肠����1���ǂ߁I
            Bitmap bmp1 = GetBitmap(nIndex);
            if (bmp1 == null)
            {
                //TODO:�t�@�C���擪�����ȍ~�̎Q�ƁB�G���[��������B
                viewPages = 1;
                string[] sz = { "�t�@�C���ǂݍ��݃G���[", g_pi.Items[nIndex].filename };
                g_bmp = BitmapUty.Text2Bitmap(sz, true);
                return;
            }

            //�\�����郂�m�͂���̂�g_bmp���N���A����B
            if (g_bmp != null)
                g_bmp.Dispose();

            if (!g_Config.dualView
                || bmp1.Width > bmp1.Height)
            {
                //1��ʃ��[�h�m��
                viewPages = 1;      //1���\�����[�h
                                    //g_bmp = (Bitmap)bmp1.Clone();		//ver0.987�ŃR�����g�A�E�g
                g_bmp = (Bitmap)bmp1;           //ver0.987�Œǉ� 2010/06/19�v
            }
            else
            {
                //2��ʃ��[�h�̋^���L��
                Bitmap bmp2 = GetBitmap(nIndex + 1);
                if (bmp2 == null || bmp2.Width > bmp2.Height)
                {
                    //1��ʃ��[�h�m��
                    viewPages = 1;      //1���\�����[�h
                                        //g_bmp = (Bitmap)bmp1.Clone();		//ver0.987�ŃR�����g�A�E�g
                    g_bmp = (Bitmap)bmp1;           //ver0.987�Œǉ� 2010/06/19�v
                }
                else
                {
                    //2��ʃ��[�h�m��
                    viewPages = 2;      //2���\�����[�h
                    g_bmp = new Bitmap(
                        bmp1.Width + bmp2.Width,
                        bmp1.Height > bmp2.Height ? bmp1.Height : bmp2.Height);
                    using (Graphics g = Graphics.FromImage(g_bmp))
                    {
                        //������2���\��
                        g.DrawImage(bmp2, 0, 0, bmp2.Width, bmp2.Height);
                        g.DrawImage(bmp1, bmp2.Width, 0, bmp1.Width, bmp1.Height);
                    }
                    //this.Refresh();
                }
            }

            Cursor.Current = cc;
            return;
        }

        //PaintBufferedGraphics()����Ăяo�����
        //g_bmp���w��g�f�o�C�X�ɏ������ށB
        //���̂Ƃ���g�̓I�t�X�N���[��g_bg��z��
        //��ʃT�C�Y�����Ag_bmp���Đ�����ɕ`�ʂ���i���i���j
        private void PaintGBMP(Graphics g, bool isScreenFitting)
        {
            Rectangle cRect = GetClientRectangle();

            //g_bmp���c����P�F�P�ŕ\������B100%�����͏k���\��
            //ratio�͏������ق��ɂ��킹��
            float ratioX = (float)cRect.Width / (float)g_bmp.Width;
            float ratioY = (float)cRect.Height / (float)g_bmp.Height;
            float ratio = (ratioX > ratioY) ? ratioY : ratioX;
            if (ratio >= 1 || ratio <= 0) ratio = 1.0F;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(g_Config.BackColor);

            if (isScreenFitting || ratio == 1.0F)
            {
                //��ʃT�C�Y���������A�������͉摜����ʃT�C�Y���̏ꍇ

                //�摜�T�C�Y��ύX���k�ڕ\������B
                int width = (int)(g_bmp.Width * ratio);
                int height = (int)(g_bmp.Height * ratio);

                g.DrawImage(
                    g_bmp,                                      //�`�ʃC���[�W
                                                                //(cRect.Width - width) / 2,					//�`�ʐ�@�n�_X
                    (cRect.Width - width) / 2 + cRect.Left,     //�`�ʐ�@�n�_X, 2010/03/22 �T�C�h�o�[�h�b�N�̂��ߕύX
                    (cRect.Height - height) / 2 + cRect.Top,    //�`�ʐ�@�n�_Y
                    width,                                      //�`�ʃC���[�W�̕�
                    height                                      //�`�ʃC���[�W�̍���
                );

                //�X�e�[�^�X�o�[�ɔ{���\��
                setStatubarRatio(ratio);
                g_viewRatio = ratio;

                // 2010/03/14 ver0.9833
                //�X�N���[���o�[���\������Ă������\���ɂ���
                if (g_vScrollBar.Visible)
                    g_vScrollBar.Visible = false;
                if (g_hScrollBar.Visible)
                    g_hScrollBar.Visible = false;
            }
            else
            {
                //�k��100%�ŕ\������.�K�v�ɉ����ăX�N���[���o�[��\��
                g_viewRatio = 1.0F; //2010/03/14 ver0.9833

                //�X�N���[���o�[�𐶐�����
                if (g_vScrollBar.Visible == false && g_bmp.Height > cRect.Height)
                {
                    g_vScrollBar.Minimum = 0;
                    g_vScrollBar.Maximum = g_bmp.Height;
                    g_vScrollBar.LargeChange = cRect.Height;
                    g_vScrollBar.SmallChange = cRect.Height / 10;
                    g_vScrollBar.Top = cRect.Top;
                    g_vScrollBar.Left = cRect.Right - g_vScrollBar.Width;
                    g_vScrollBar.Height = cRect.Height;
                    g_vScrollBar.Value = 0;
                    g_vScrollBar.Visible = true;
                }
                if (g_hScrollBar.Visible == false && g_bmp.Width > cRect.Width)
                {
                    g_hScrollBar.Minimum = 0;
                    g_hScrollBar.Maximum = g_bmp.Width;
                    g_hScrollBar.LargeChange = cRect.Width;
                    g_hScrollBar.SmallChange = cRect.Width / 10;
                    g_hScrollBar.Top = cRect.Bottom - g_hScrollBar.Height;
                    g_hScrollBar.Left = cRect.Left;
                    g_hScrollBar.Width = cRect.Width;
                    g_hScrollBar.Value = 0;
                    g_hScrollBar.Visible = true;
                }
                //�X�N���[���o�[�L���Ԃ�cRect���Đ�������B
                cRect = GetClientRectangle();

                //2�{�����\����Ԃ̉E��������␳����
                if (g_vScrollBar.Visible && g_hScrollBar.Visible)
                {
                    //�X�N���[���o�[�̍����␳������
                    g_vScrollBar.Height = cRect.Height;
                    g_vScrollBar.LargeChange = cRect.Height;    //ver0.974
                                                                //���␳����
                    g_hScrollBar.Width = cRect.Width;
                    g_hScrollBar.LargeChange = cRect.Width;     //ver0.974
                }

                //�X�N���[���o�[���Ȃ��Ƃ��͒����\��
                if (!g_vScrollBar.Visible)
                    cRect.Y = (cRect.Height - g_bmp.Height) / 2 + cRect.Top;
                if (!g_hScrollBar.Visible)
                    //cRect.X = (cRect.Width - g_bmp.Width) / 2;
                    cRect.X = (cRect.Width - g_bmp.Width) / 2 + cRect.Left; //ver0.986�T�C�h�o�[�␳
                Debug.WriteLine(cRect.X, "cRect.X");

                //�����\��������͈͂��m��
                Rectangle sRect = new Rectangle(
                    g_hScrollBar.Value,
                    g_vScrollBar.Value,
                    cRect.Width,
                    cRect.Height);

                g.DrawImage(
                    g_bmp,          //�`�ʃC���[�W
                    cRect,          //destRect
                    sRect,          //srcRect
                    GraphicsUnit.Pixel
                    );

                //�X�e�[�^�X�o�[�ɔ{���\��
                //setStatubarRatio(1.0F);
                //g_viewRatio = ratio;	//2010/03/14 ver0.9833���������Ȃ��H
                setStatubarRatio(g_viewRatio);
                //g_viewRatio = 1.0F;		//2010/03/14 ver0.9833���������Ȃ��H
            }
        }

        /// <summary>
        /// �I�t�X�N���[��(BufferedGraphics) g_bg�ɕ`�ʂ���B
        /// �\��������g�����͈ȉ��̒ʂ�B
        ///   DrawImageToGBMP(index);			//g_bmp�����B����
        ///	  PaintBufferedGraphics();			//g_bmp���I�t�X�N���[����Draw����
        ///   this.Invalidate();				//��ʍX�V
        /// </summary>
        private void PaintBufferedGraphics()
        {
            //ver0.987 �Ō�̕`�ʃ��[�h���o���Ă���
            m_lastDrawMode = LastDrawMode.HighQuality;

            if (g_bmp != null)
                PaintGBMP(g_bg.Graphics, g_Config.isFitScreenAndImage);
        }

        /// <summary>
        /// Form1_Resize()����Ăяo����郊�T�C�Y�p�̍����ĕ`��
        /// �I�t�X�N���[��(BufferedGraphics) g_bg�ɕ`�ʂ���B
        /// </summary>
        private void PaintBufferedGraphicsFast()
        {
            //ver0.987 �Ō�̕`�ʃ��[�h���o���Ă���
            m_lastDrawMode = LastDrawMode.Fast;

            if (g_bmp != null)
            {
                Graphics g = g_bg.Graphics;
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(g_Config.BackColor);

                Rectangle cRect = GetClientRectangle();

                //g_bmp���c����P�F�P�ŕ\������B100%�����͏k���\��
                float ratioX = (float)cRect.Width / (float)g_bmp.Width;
                float ratioY = (float)cRect.Height / (float)g_bmp.Height;
                float ratio = (ratioX > ratioY) ? ratioY : ratioX;
                if (ratio >= 1 || ratio <= 0) ratio = 1.0F;

                if (g_Config.isFitScreenAndImage)
                {
                    //�摜�T�C�Y��ύX���k�ڕ\������B
                    int width = (int)(g_bmp.Width * ratio);
                    int height = (int)(g_bmp.Height * ratio);

                    g.DrawImage(
                        g_bmp,                                      //�`�ʃC���[�W
                                                                    //(cRect.Width - width) / 2,					//�`�ʐ�@�n�_X
                        (cRect.Width - width) / 2 + cRect.Left,     //�`�ʐ�@�n�_X
                        (cRect.Height - height) / 2 + cRect.Top,    //�`�ʐ�@�n�_Y
                        width,                                      //�`�ʃC���[�W�̕�
                        height                                      //�`�ʃC���[�W�̍���
                    );
                    g_viewRatio = ratio;
                }
                else
                {
                    //�k��100%�ŕ\������.�K�v�ɉ����ăX�N���[���o�[��\��
                    cRect = GetClientRectangle();

                    //���㌴�_���v�Z�B�X�N���[���o�[�̈ʒu��ς��邩�`�F�b�N
                    int newX = g_bmp.Width - cRect.Width;
                    if (newX < 0)
                    {
                        cRect.X += newX / 2 * -1;   //�����ɒ����Ďn�_�𒆉���
                        newX = 0;
                    }
                    int newY = g_bmp.Height - cRect.Height;
                    if (newY < 0)
                    {
                        cRect.Y += -1 * newY / 2;
                        newY = 0;
                    }

                    if (g_hScrollBar.Value > newX)
                        g_hScrollBar.Value = newX;
                    if (g_vScrollBar.Value > newY)
                        g_vScrollBar.Value = newY;

                    //�����\��������͈͂��m��
                    Rectangle sRect = new Rectangle(
                        g_hScrollBar.Value,
                        g_vScrollBar.Value,
                        cRect.Width,
                        cRect.Height);

                    g.DrawImage(
                        g_bmp,          //�`�ʃC���[�W
                        cRect,          //destRect
                        sRect,          //srcRect
                        GraphicsUnit.Pixel
                        );
                    g_viewRatio = 1.0F; ;
                }
                Debug.WriteLine("PaintBufferedGraphicsFast()");
            }
        }
    }//public class ThumbnailPanel
}
using System;
using System.Collections.Generic;
using System.Diagnostics;				//Debug, Stopwatch
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Drawing.Drawing2D;			// GraphicsPath
using System.Drawing.Imaging;			// ColorMatrix
using System.IO;						// Directory, File, Stream

//using System.Text;

using System.Windows.Forms;

namespace Marmi
{
    public class BitmapUty
    {
        private static int BoxMargin = 20;                      //�{�b�N�X�O�����̃}�[�W��
        private static int padding = 5;                         //Key-Value�Ԃ�Padding
        private static string SepareteString = " : ";           //2��\�����{���̃Z�p���[�^�[������
        private static float LinePadding = 1f;                  //�s��

        private static string FontName = "MS PGothic";          //�t�H���g��
        private static int FontPoint = 10;                      //�t�H���g�̃|�C���g
        private static Color FontColorKey = Color.RoyalBlue;    //�L�[�̐F
        private static Color FontColorValue = Color.Black;      //�l�̐F�i�ʏ�̐F�j
        private static Color BackColor = Color.White;           //�w�i�F
        private static Color BorderColor = Color.DarkGray;      //�{�[�_�[�J���[

        public static Bitmap Text2Bitmap(string str, bool isRoundCorner)
        {
            string[] sz = new string[] { str };
            return Text2Bitmap(sz, isRoundCorner);
        }

        public static Bitmap Text2Bitmap(string[] str, bool isRoundCorner)
        {
            float height = 0f;      //�v�����ꂽ����
            float width = 0f;       //�v�����ꂽ��

            using (Font font = new Font(FontName, FontPoint))
            {
                //�T�C�Y�𑪂�
                using (Bitmap b = new Bitmap(10, 10))
                using (Graphics g = Graphics.FromImage(b))
                {
                    foreach (string s in str)
                    {
                        SizeF sizef = g.MeasureString(s, font);
                        if (sizef.Width > width)
                            width = sizef.Width;
                        height += sizef.Height + LinePadding;
                    }
                }

                height += BoxMargin * 2;
                width += BoxMargin * 2;

                //Bitmap�����
                Bitmap returnbmp = new Bitmap((int)width, (int)height);
                using (Graphics g = Graphics.FromImage(returnbmp))
                {
                    float y = BoxMargin;
                    //Brush brush = Brushes.Black;
                    SolidBrush brush = new SolidBrush(FontColorValue);

                    //���̏�����
                    InitBackImage(g, width, height, isRoundCorner);

                    foreach (string s in str)
                    {
                        SizeF sizef = g.MeasureString(s, font);

                        g.DrawString(s, font, brush, BoxMargin, y);
                        y += sizef.Height + LinePadding;
                    }
                }
                return returnbmp;
            }//using font
        }

        public static Bitmap Text2Bitmap(KeyValuePair<string, string>[] kv, bool isRoundCorner)
        {
            float keyWidth = 0;
            float keyHeight = 0;
            float valueWidth = 0;
            float valueHeight = 0;

            using (Font font = new Font(FontName, FontPoint))
            {
                using (Bitmap TempBmp = new Bitmap(10, 10))
                using (Graphics g = Graphics.FromImage(TempBmp))
                {
                    SizeF size;
                    foreach (KeyValuePair<string, string> s in kv)
                    {
                        //if (s == null)
                        //    continue;

                        //Key�̑傫�����m�F
                        size = g.MeasureString(s.Key + SepareteString, font);
                        keyWidth = (keyWidth > size.Width) ? keyWidth : size.Width;
                        keyHeight = (keyHeight > size.Height) ? keyHeight : size.Height;

                        //Value�̑傫�����m�F
                        size = g.MeasureString(s.Value, font);
                        valueWidth = (valueWidth > size.Width) ? valueWidth : size.Width;
                        valueHeight = (valueHeight > size.Height) ? valueHeight : size.Height;
                    }
                }

                float height = (valueHeight > keyHeight) ? valueHeight : keyHeight;
                RectangleF rcKey = new RectangleF(0, 0, keyWidth, height);
                RectangleF rcValue = new RectangleF(0, 0, valueWidth, height);
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Near;

                Bitmap returnBitmap = new Bitmap(
                    (int)(keyWidth + valueWidth + padding + BoxMargin * 2),
                    (int)((height + LinePadding) * kv.Length + BoxMargin * 2));

                using (Graphics g = Graphics.FromImage(returnBitmap))
                {
                    //���̏�����
                    InitBackImage(g, returnBitmap.Width, returnBitmap.Height, isRoundCorner);

                    SolidBrush keyBrush = new SolidBrush(FontColorKey);
                    SolidBrush valueBrush = new SolidBrush(FontColorValue);

                    //������`��
                    for (int i = 0; i < kv.Length; i++)
                    {
                        //if (sd[i] == null)
                        //    continue;

                        //g.DrawString(
                        //    sd[i].Key,
                        //    font,
                        //    Brushes.SteelBlue,
                        //    BoxMargin,
                        //    height * i);

                        rcKey.X = BoxMargin;
                        rcKey.Y = (height + LinePadding) * i + BoxMargin;

                        //Key��`��
                        g.DrawString(
                            kv[i].Key + SepareteString,
                            font,
                            //Brushes.SteelBlue,
                            keyBrush,
                            rcKey,
                            sf);

                        //Value��`��
                        g.DrawString(
                            kv[i].Value,
                            font,
                            valueBrush,
                            BoxMargin + keyWidth + padding,
                            (height + LinePadding) * i + BoxMargin);
                    }
                }
                return returnBitmap;
            }//using font
        }

        //Text2Bitmap()����Ăяo�����w�i�쐬���[�`��
        private static void InitBackImage(Graphics g, float width, float height, bool isRoundCorner)
        {
            if (isRoundCorner)
            {
                int arc = 10;
                GraphicsPath gp = CreateRoundedRectangle((int)width - 1, (int)height - 1, arc);
                g.FillPath(new SolidBrush(BackColor), gp);
                g.DrawPath(new Pen(BorderColor), gp);
            }
            else
            {
                g.Clear(BackColor);
                g.DrawRectangle(new Pen(BorderColor), 0, 0, width - 1, height - 1);
            }
        }

        //�p�ۂ�GraphicPath�����
        private static GraphicsPath CreateRoundedRectangle(int width, int height, int arc)
        {
            //�������������
            int padding = 1;

            GraphicsPath path = new GraphicsPath(FillMode.Winding);
            path.AddArc(width - arc - padding, padding, arc, arc, 270, 90);
            path.AddArc(width - arc - padding, height - arc - padding, arc, arc, 0, 90);
            path.AddArc(padding, height - arc - padding, arc, arc, 90, 90);
            path.AddArc(padding, padding, arc, arc, 180, 90);
            path.AddArc(width - arc - padding, padding, arc, arc, 270, 90);
            //path.AddArc(width - arc, 0, arc, arc, 270, 90);
            //path.AddArc(width - arc, height - arc, arc, arc, 0, 90);
            //path.AddArc(0, height - arc, arc, arc, 90, 90);
            //path.AddArc(0, 0, arc, arc, 180, 90);
            //path.AddArc(width - arc, 0, arc, arc, 270, 90);
            return path;
        }

        public static GraphicsPath GetRoundRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            // ����̊p��
            path.AddArc(rect.Left, rect.Top, radius * 2, radius * 2,
                180, 90);
            // ��̐�
            path.AddLine(rect.Left + radius, rect.Top,
                rect.Right - radius, rect.Top);
            // �E��̊p��
            path.AddArc(rect.Right - radius * 2, rect.Top,
                radius * 2, radius * 2,
                270, 90);
            // �E�̐�
            path.AddLine(rect.Right, rect.Top + radius,
                rect.Right, rect.Bottom - radius);
            // �E���̊p��
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2,
                radius * 2, radius * 2,
                0, 90);
            // ���̐�
            path.AddLine(rect.Right - radius, rect.Bottom,
                rect.Left + radius, rect.Bottom);
            // �����̊p��
            path.AddArc(rect.Left, rect.Bottom - radius * 2,
                radius * 2, radius * 2,
                90, 90);
            // ���̐�
            path.AddLine(rect.Left, rect.Bottom - radius,
                rect.Left, rect.Top + radius);

            path.CloseFigure();

            return path;
        }

        //�������ł�DrawImage
        public static void alphaDrawImage(Graphics g, Image img, float alpha)
        {
            var cm = new ColorMatrix
            {
                Matrix00 = 1,
                Matrix11 = 1,
                Matrix22 = 1,
                Matrix33 = alpha,
                Matrix44 = 1
            };

            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            //�A���t�@�u�����h���Ȃ���`��
            g.DrawImage(
                img,
                new Rectangle(0, 0, img.Width, img.Height),
                0, 0,
                img.Width, img.Height,
                GraphicsUnit.Pixel,
                ia);
        }

        //�������ł�DrawImage
        public static void alphaDrawImage(Graphics g, Image img, Rectangle rect, float alpha)
        {
            var cm = new ColorMatrix
            {
                Matrix00 = 1,
                Matrix11 = 1,
                Matrix22 = 1,
                Matrix33 = alpha,
                Matrix44 = 1
            };

            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            //�A���t�@�u�����h���Ȃ���`��
            g.DrawImage(
                img,
                rect,
                0, 0, img.Width, img.Height,
                GraphicsUnit.Pixel,
                ia);
        }

        //�A�C�R���t�@�C����͔�
        public static Bitmap LoadIcon3(Stream fs)
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

        //�h���b�v�V���h�E�̕`��
        //�w�肵����`�ɉe�𗎂Ƃ��B�����͂R
        public static void drawDropShadow(Graphics g, Rectangle r)
        {
            //ver0.975���ʂ�����
            SolidBrush b1 = new SolidBrush(Color.FromArgb(96, Color.DimGray));
            SolidBrush b2 = new SolidBrush(Color.FromArgb(64, Color.DimGray));
            SolidBrush b3 = new SolidBrush(Color.FromArgb(32, Color.DimGray));
            SolidBrush b4 = new SolidBrush(Color.FromArgb(16, Color.DimGray));

            //g.CompositingMode = CompositingMode.SourceOver;
            r.Inflate(3, 3);
            r.X++;
            r.Y++;
            drawRoundRectangle(g, b4, r, 5);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b4, r, 4);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b3, r, 3);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b3, r, 2);
            //g.CompositingMode = CompositingMode.SourceCopy;
        }

        //�w�肵����`�Ɏ���ɃG�t�F�N�g��`��
        public static void drawBlurEffect(Graphics g, Rectangle r, Color c)
        {
            //ver0.975���ʂ�����
            SolidBrush b1 = new SolidBrush(Color.FromArgb(192, c));
            SolidBrush b2 = new SolidBrush(Color.FromArgb(96, c));
            SolidBrush b3 = new SolidBrush(Color.FromArgb(32, c));
            SolidBrush b4 = new SolidBrush(Color.FromArgb(16, c));

            //g.CompositingMode = CompositingMode.SourceOver;
            //r.Width += 1;	//�����Ă����Ȃ���1�h�b�g������
            //r.Height += 1;	//�����Ă����Ȃ���1�h�b�g������
            r.Inflate(4, 4);
            //r.X++;
            //r.Y++;
            drawRoundRectangle(g, b4, r, 5);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b3, r, 4);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b2, r, 3);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b1, r, 2);
            r.Inflate(-1, -1);
            drawRoundRectangle(g, b1, r, 2);
            //g.CompositingMode = CompositingMode.SourceCopy;
        }

        //�p�ۂ�`��
        //�h���b�v�V���h�E�Ŏg��
        private static void drawRoundRectangle(Graphics g, Brush br, Rectangle rect, int arc)
        {
            using (GraphicsPath gp = new GraphicsPath(FillMode.Winding))
            {
                gp.AddArc(rect.Right - arc, rect.Top, arc, arc, 270, 90);
                gp.AddArc(rect.Right - arc, rect.Bottom - arc, arc, arc, 0, 90);
                gp.AddArc(rect.Left, rect.Bottom - arc, arc, arc, 90, 90);
                gp.AddArc(rect.Left, rect.Top, arc, arc, 180, 90);
                gp.AddArc(rect.Right - arc, rect.Top, arc, arc, 270, 90);
                //g.DrawPath(pen, gp);
                g.FillPath(br, gp);
            }
        }

        /// <summary>
        /// Window/Control���L���v�`������B
        /// �ΏۂƂȂ�Window/Control�̑S����L���v�`������
        /// </summary>
        /// <param name="wnd">�L���v�`������Ώۂ�Form/Control</param>
        /// <returns>�L���v�`�����ꂽ�摜</returns>
        public static Bitmap CaptureWindow(Control wnd)
        {
            //Rectangle rc = parent.RectangleToScreen(parent.DisplayRectangle);	//�c�[���o�[���݂ŃL���v�`��
            //Rectangle rc = parent.Bounds;	//���ꂾ�ƃ^�C�g���o�[���L���v�`�����Ă��܂��B
            //Rectangle rc = ((Form1)wnd).GetClientRectangle();	//�N���C�A���g���W�Ŏ擾�B�c�[���o�[����
            Rectangle rc = wnd.ClientRectangle;
            rc = wnd.RectangleToScreen(rc);                     //�X�N���[�����W��

            Bitmap cauptureBmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(cauptureBmp))
            {
                g.CopyFromScreen(
                    rc.X, rc.Y,
                    0, 0,
                    rc.Size,
                    CopyPixelOperation.SourceCopy);
            }
            return cauptureBmp;
        }

        /// <summary>
        ///  �t�@�C��������Bitmap��Ԃ�
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Bitmap LoadImageFromFile(string filename)
        {
            if (File.Exists(filename) && Uty.IsPictureFilename(filename))
            {
                using (FileStream fs = File.OpenRead(filename))
                {
                    return BitmapUty.LoadImageFromStream(filename, fs);
                }
            }
            return null;
        }

        /// <summary>
        /// Stream����Bitmap��Ԃ��B
        /// �t�@�C�����͊g���q���݂�ico���ǂ������m�F���Ă���
        /// </summary>
        /// <param name="filename">�t�@�C����</param>
        /// <param name="fs">�X�g���[��</param>
        /// <returns></returns>
        public static Bitmap LoadImageFromStream(string filename, Stream fs)
        {
            try
            {
                if (Path.GetExtension(filename).ToLower() == ".ico")
                {
                    //return LoadIcon2(fs);
                    return LoadIcon3(fs);
                }
                else
                {
                    return new Bitmap(Bitmap.FromStream(fs, false, false));
                }
            }
            catch //(Exception e)
            {
                //string[] s = { "�ǂݍ��݂Ɏ��s���܂���", filename, "", e.Message };
                //return BitmapUty.Text2Bitmap(s, true);
                return null;
            }
        }

        //ver1.10
        //2011�N8��19��
        //�w��T�C�Y��Bitmap�ɕ�����`��
        public static Bitmap Text2Bitmap(string[] str, int bmpwidth, int bmpheight, Color backColor)
        {
            //Bitmap�����
            Bitmap _bmp = new Bitmap(bmpwidth, bmpheight);

            using (Graphics g = Graphics.FromImage(_bmp))
            using (Font font = new Font(FontName, FontPoint))
            using (SolidBrush brush = new SolidBrush(FontColorValue))
            {
                g.Clear(backColor);

                //�e�L�X�g�T�C�Y�𑪂�
                float textHeight = 0f;      //�v�����ꂽ����
                float textHeight1line = 0f;     //�v�����ꂽ��s���̍���
                float textWidth = 0f;       //�v�����ꂽ��
                foreach (string s in str)
                {
                    SizeF sizef = g.MeasureString(s, font);
                    if (sizef.Width > textWidth)
                        textWidth = sizef.Width;
                    textHeight1line = sizef.Height;
                    textHeight += sizef.Height + LinePadding;
                }

                RectangleF rect = new RectangleF(
                    (bmpwidth - textWidth) / 2,
                    (bmpheight - textHeight) / 2,
                    textWidth,
                    textHeight1line
                    );

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;      //�㉺����
                sf.LineAlignment = StringAlignment.Center;  //�Z���^�����O

                foreach (string s in str)
                {
                    SizeF sizef = g.MeasureString(s, font);

                    g.DrawString(s, font, brush, rect, sf);
                    rect.Y += sizef.Height + LinePadding;
                }
            }//using
            return _bmp;
        }

        /// <summary>
        /// �摜�T�C�Y���w��̑傫���ȉ��̃T�C�Y�ɂȂ�悤�ɒ�������
        /// �c����͌Œ�
        /// </summary>
        /// <param name="orgSize">���̃T�C�Y</param>
        /// <param name="maxLength">��ԑ傫�ȕӂ̒���</param>
        /// <returns>�{����Ԃ��B1�{�ȏ�ɂ͂Ȃ�Ȃ�</returns>
        public static float GetMagnificationWithFixAspectRatio(Size orgSize, int maxLength)
        {
            float ratio = 1.0F;

            if (orgSize.Height > orgSize.Width)
            {
                if (orgSize.Height > maxLength)
                    ratio = (float)maxLength / (float)orgSize.Height;
            }
            else
            {
                if (orgSize.Width > maxLength)
                    ratio = (float)maxLength / (float)orgSize.Width;
            }

            return ratio;
        }

        /// <summary>
        /// Bitmap�̃R���g���X�g�������A����Ԃ�������ɒ���
        /// �F�����ȊO�̑傫���Ȃǂ͕ύX�Ȃ�
        /// </summary>
        /// <param name="srcBitmap">����Bitmap</param>
        /// <returns>�������ꂽ�V����Bitmap�B</returns>
        public static Bitmap LowContrastBitmap(Bitmap srcBitmap)
        {
            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int writePos;
                for (int y = 0; y < bmpRect.Height; y++)
                {
                    for (int x = 0; x < bmpRect.Width; x++)
                    {
                        //3byte������������
                        writePos = x * 3 + bmpData.Stride * y;
                        offset[writePos + 0] = (byte)(((int)offset[writePos + 0] + 255) / 2); // B;
                        offset[writePos + 1] = (byte)(((int)offset[writePos + 1] + 255) / 2); // G;
                        offset[writePos + 2] = (byte)(((int)offset[writePos + 2] + 255) / 2); // R;
                    }//for x
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// Bitmap�̋P�x�𔼕��ɂ���B
        /// �P���ɔ����̂��߈Â��Ȃ�
        /// �F�����ȊO�̑傫���Ȃǂ͕ύX�Ȃ�
        /// </summary>
        /// <param name="srcBitmap">����Bitmap</param>
        /// <returns>�����̖��邳��Bitmap</returns>
        public static Bitmap HalfBrightnessBitmap(Bitmap srcBitmap)
        {
            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int endPos = bmpData.Stride * bmpRect.Height - 1;
                for (int pos = 0; pos < endPos; pos++)
                {
                    offset[pos] = (byte)(offset[pos] / 2); // B;
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// �R���g���X�g�𔼕��ɂ��A�O���C(128,128,128)�ɋ߂Â��悤�ɒ������ꂽBitmap���쐬
        /// </summary>
        /// <param name="srcBitmap">����Bitmap</param>
        /// <returns>�������ꂽBitmap</returns>
        public static Bitmap HalfBrightnessBitmap2(Bitmap srcBitmap)
        {
            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int endPos = bmpData.Stride * bmpRect.Height - 1;
                for (int pos = 0; pos < endPos; pos++)
                {
                    offset[pos] = (byte)((offset[pos] / 2) + 64); // B;
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// �r�b�g�}�b�v���Â�����
        /// �O�`�Q�T�T�͈̔͂Œ���
        /// </summary>
        /// <param name="srcBitmap">���ƂȂ�Bitmap</param>
        /// <param name="Level">0�`100�B�O�Ő^�����A�P�O�O�Ō��̖��邳</param>
        /// <returns></returns>
        public static Bitmap BitmapToDark(Bitmap srcBitmap, int Level)
        {
            if (srcBitmap == null)
                return null;

            Rectangle bmpRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            Bitmap newBitmap = srcBitmap.Clone(bmpRect, PixelFormat.Format24bppRgb);
            BitmapData bmpData = newBitmap.LockBits(bmpRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* offset = (byte*)bmpData.Scan0;
                int endPos = bmpData.Stride * bmpRect.Height - 1;
                for (int pos = 0; pos < endPos; pos++)
                {
                    int data = (int)offset[pos] * Level / 100;
                    offset[pos] = (byte)data;
                }//for y
            }//unsafe
            newBitmap.UnlockBits(bmpData);

            return newBitmap;
        }

        /// <summary>
        /// �T���l�C���T�C�Y�����߂�B
        /// �c������k�ڂō����A�����w��T�C�Y�𒴂��Ȃ��悤�ɂ���B
        /// </summary>
        /// <param name="x">���̒���</param>
        /// <param name="y">�c�̒���</param>
        /// <param name="maxLength">�ő咷</param>
        /// <returns>�V�����T�C�Y</returns>
        public static Size calcThumbnailSize(int x, int y, int maxLength)
        {
            //�䗦���m��
            double ratioX = (double)maxLength / (double)x;
            double ratioY = (double)maxLength / (double)y;
            double ratio = ratioX < ratioY ? ratioX : ratioY;
            if (ratio > 1) ratio = 1;

            //�T�C�Y���m��
            int lx = (int)((double)x * ratio);
            int ly = (int)((double)y * ratio);
            if (lx == 0) lx = 1;
            if (ly == 0) ly = 1;
            return new Size(lx, ly);
        }

        public static Size calcThumbnailSize(Size s, int maxLength)
        {
            return calcThumbnailSize(s.Width, s.Height, maxLength);
        }

        /// <summary>
        /// �w��T�C�Y�̃T���l�C�����쐬
        /// </summary>
        /// <param name="orgImage">���̉摜</param>
        /// <param name="maxLength">�ő咷</param>
        /// <returns>�V�����T���l�C��Bitmap</returns>
        public static Bitmap MakeThumbnailImage(Bitmap orgImage, int maxLength)
        {
            if (orgImage == null)
                return null;

            if (orgImage.Width <= maxLength
                && orgImage.Height <= maxLength)
                return orgImage.Clone() as Bitmap;

            Size s = BitmapUty.calcThumbnailSize(orgImage.Width, orgImage.Height, maxLength);
            Bitmap _bmp = new Bitmap(s.Width, s.Height);

            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(orgImage, 0, 0, s.Width, s.Height);
            }
            return _bmp;
        }

        public static Bitmap MakeHeightFixThumbnailImage(Bitmap orgImage, int maxHeight)
        {
            if (orgImage == null)
                return null;

            //�������ꍇ�̓R�s�[������ĕԂ�
            if (orgImage.Height <= maxHeight)
                return orgImage.Clone() as Bitmap;

            //�V�����T�C�Y���v�Z
            Size s = BitmapUty.calcHeightFixImageSize(orgImage.Size, maxHeight);

            //�T���l�C�����쐬
            Bitmap _bmp = new Bitmap(s.Width, s.Height);
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(orgImage, 0, 0, s.Width, s.Height);
            }

            return _bmp;
        }

        public static Size calcHeightFixImageSize(Size orgSize, int maxHeight)
        {
            if (orgSize.Height <= maxHeight)
                return orgSize;

            double ratio = (double)maxHeight / (double)orgSize.Height;
            int width = (int)((double)orgSize.Width * ratio);
            if (width == 0)
                width = 1;
            return new Size(width, maxHeight);
        }

        /// <summary>
        /// Loading�ƕ\������_�~�[��Bitmap���쐬����
        /// �T���l�C���Ƃ��ė��p
        /// </summary>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns>�쐬���ꂽBitmap</returns>
        public static Bitmap LoadingImage(int width, int height)
        {
            Bitmap _bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(_bmp))
            using (Font fontS = new Font("�l�r �o �S�V�b�N", 9F, FontStyle.Bold))
            {
                //ver1.29
                //�_�~�[�T���l�C���̃T�C�Y
                Rectangle rect = new Rectangle(
                    1, 1,
                    _bmp.Width - 1, _bmp.Height - 1);
                //�O���C�œh��Ԃ�
                g.FillRectangle(Brushes.Gray, rect);
                //���邢�O���C�Řg�`��
                g.DrawRectangle(Pens.LightGray, rect);

                StringFormat sf = new StringFormat()
                {
                    //�㉺���E����
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    //�����񂪕\��������Ȃ��Ƃ���"..."��\������
                    Trimming = StringTrimming.EllipsisPath,
                };

                //�e�L�X�g�`�ʃt�H���g

                //Loading�ƕ\��
                g.DrawString(
                    "Loading",
                    fontS,
                    Brushes.LightGray,
                    rect,
                    sf);
            }
            return _bmp;
        }

        /// <summary>
        /// �X�N���[���T�C�Y�ɉ������T�C�Y�ɉ摜���k������B
        /// �c��������ɂ��ďk��
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="screenSize"></param>
        /// <returns></returns>
        public static Bitmap MakeFittedBitmap(Bitmap bmp, Size screenSize)
        {
            if (bmp == null)
                return null;

            //�k�ڃT�C�Y�𓾂�
            double rx = (double)screenSize.Width / (double)bmp.Width;
            double ry = (double)screenSize.Height / (double)bmp.Height;

            //�������ق����g��
            double r = rx < ry ? rx : ry;
            if (r > 1.0f && App.Config.noEnlargeOver100p)
                r = 1.0f;

            Bitmap newbmp = new Bitmap(screenSize.Width, screenSize.Height);
            using (Graphics g = Graphics.FromImage(newbmp))
            {
                //�摜�`�ʃ��[�h�F��i����
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.Clear(App.Config.BackColor);
                //���S�ɕ`��
                double width = (double)bmp.Width * r;
                double height = (double)bmp.Height * r;
                double sx = (newbmp.Width - width) / 2;
                double sy = (newbmp.Height - height) / 2;
                g.DrawImage(bmp, (int)sx, (int)sy, (int)width, (int)height);
            }
            return newbmp;
        }

        /// <summary>
        /// ver1.79 �^�l�p�ȃT���l�C�������
        /// �������ĉ\�Ȍ��菬����������^�񒆂�؂�ʂ�
        /// </summary>
        /// <param name="orgImage">���ƂȂ�摜</param>
        /// <param name="maxLen">��ӂ̒���</param>
        /// <returns></returns>
        public static Bitmap MakeSquareThumbnailImage(Bitmap orgImage, int maxLen)
        {
            if (orgImage == null)
                return null;

            //�������ꍇ�̓R�s�[������ĕԂ�
            if (orgImage.Width <= maxLen && orgImage.Height <= maxLen)
                return orgImage.Clone() as Bitmap;

            Bitmap bmp = null;  //�傫�߂̃T���l�C��
            Rectangle srcRect;  //�؂蔲���ʒu
            Rectangle destRect = new Rectangle(0, 0, maxLen, maxLen);
            if (orgImage.Width >= orgImage.Height)
            {
                //�����摜�Ȃ̂ō�����maxLen��
                bmp = MakeHeightFixThumbnailImage(orgImage, maxLen);
                srcRect = new Rectangle((bmp.Width - maxLen) / 2, 0, maxLen, maxLen);
            }
            else
            {
                //�c���摜
                bmp = MakeWidthFixThumbnailImage(orgImage, maxLen);
                srcRect = new Rectangle(0, (bmp.Height - maxLen) / 2, maxLen, maxLen);
            }

            //�^�l�p�̃T���l�C�������
            Bitmap retbmp = new Bitmap(maxLen, maxLen);
            using (Graphics g = Graphics.FromImage(retbmp))
            {
                g.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
            }
            return retbmp;
        }

        //
        /// <summary>
        /// ver1.79 ���Œ�̃T���l�C�������
        /// SquareThumbnail��p�ɗ��p
        /// �����Œ�͊��ɂ���B
        /// </summary>
        /// <param name="orgImage">���ƂȂ�摜</param>
        /// <param name="maxWidth">����</param>
        /// <returns></returns>
        private static Bitmap MakeWidthFixThumbnailImage(Bitmap orgImage, int maxWidth)
        {
            if (orgImage == null)
                return null;

            //�������ꍇ�̓R�s�[������ĕԂ�
            if (orgImage.Width <= maxWidth)
                return orgImage.Clone() as Bitmap;

            //�V�����T�C�Y���v�Z
            double ratio = (double)maxWidth / (double)orgImage.Width;
            Size s = new Size((int)(orgImage.Width * ratio), (int)(orgImage.Height * ratio));

            //�T���l�C�����쐬
            Bitmap _bmp = new Bitmap(s.Width, s.Height);
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(orgImage, 0, 0, s.Width, s.Height);
            }
            return _bmp;
        }

        /// <summary>
        /// ver1.83 �A���V���[�v����
        /// ���v���V�A�����������č�������邱�ƂŃG�b�W�����B
        /// unsafe�Ȃ̂Œ���
        /// �o���I�ɂ͌W��k=30 =(0.3�{�j���炢���悳�����B
        ///
        /// ���񏈗��ł���ɑ����Ȃ邪
        /// .net3.5 �ł�Parallel���R�����g�A�E�g
        /// </summary>
        /// <param name="srcbmp">���ƂȂ�摜</param>
        /// <param name="k">�A���V���[�v�̋����B100�܂�</param>
        /// <returns></returns>
        public static Bitmap Unsharpness_unsafe(Bitmap srcbmp, int k)
        {
            int lx = srcbmp.Width;      //�Ώۉ摜�̃T�C�Y�F��
            int ly = srcbmp.Height;     //�Ώۉ摜�̃T�C�Y�F�c
            Bitmap destbmp = new Bitmap(lx, ly);
            Stopwatch sw = Stopwatch.StartNew();

            //3x3���v���V�A���p�}�g���N�X
            //���[�v�����œW�J�����̂Ŏg��Ȃ�
            //int[,] m = new int[3, 3]{
            //	{-1,-1,-1},
            //	{-1, 9,-1},
            //	{-1,-1,-1}
            //};

            //�摜�����b�N���邽�߂̉�����
            Rectangle sRect = new Rectangle(0, 0, lx, ly);
            Rectangle dRect = new Rectangle(0, 0, lx, ly);
            BitmapData srcData = srcbmp.LockBits(
                                    sRect,
                                    ImageLockMode.ReadOnly,
                                    PixelFormat.Format24bppRgb);
            BitmapData destData = destbmp.LockBits(
                                    dRect,
                                    ImageLockMode.WriteOnly,
                                    PixelFormat.Format24bppRgb);

            unsafe
            {
                //�|�C���^�̎擾
                byte* pSrc = (byte*)srcData.Scan0;      //�ǂݍ��݈ʒu�̐擪��\��
                byte* pDest = (byte*)destData.Scan0;    //�������݈ʒu�̐擪��\��

                //�N���b�s���O���� �ŏ�i�͂����̃R�s�[;
                for (int x = 0; x < lx * 3; x++)
                    pDest[x] = pSrc[x];
                //�N���b�s���O���� �ŉ��i�������̃R�s�[;
                int start = (ly - 1) * srcData.Stride;
                for (int x = start; x < start + lx * 3; x++)
                    pDest[x] = pSrc[x];

                //Parallel.For(1, ly - 1, y =>
                for (int y = 1; y < ly - 1; y++)
                {
                    //Parallel�̂��߂Ƀ��[�v�̒��ɋL��
                    int R = 0;          //RGB�v�Z�p�ϐ�
                    int G = 0;          //RGB�v�Z�p�ϐ�
                    int B = 0;          //RGB�v�Z�p�ϐ�
                    int readPos = 0;    //�h�b�g�ǂݍ��݈ʒu�ibyte offset�j
                    int writePos = 0;   //�h�b�g�������݈ʒu�ibyte offset�j
                    int RR, GG, BB;     //���S�h�b�g�ۑ��p�B

                    //�N���b�s���O x=0
                    readPos = srcData.Stride * y;
                    writePos = srcData.Stride * y;
                    pDest[writePos + 0] = pSrc[readPos + 0];
                    pDest[writePos + 1] = pSrc[readPos + 1];
                    pDest[writePos + 2] = pSrc[readPos + 2];

                    //�N���b�s���O x=lx-1
                    readPos = lx - 1 + srcData.Stride * y;
                    writePos = lx - 1 + srcData.Stride * y;
                    pDest[writePos + 0] = pSrc[readPos + 0];
                    pDest[writePos + 1] = pSrc[readPos + 1];
                    pDest[writePos + 2] = pSrc[readPos + 2];

                    for (int x = 1; x < lx - 1; x++)
                    {
                        R = G = B = 0;

                        //��̗�
                        readPos = (x - 1) * 3 + srcData.Stride * (y - 1);
                        B += -1 * pSrc[readPos + 0];
                        G += -1 * pSrc[readPos + 1];
                        R += -1 * pSrc[readPos + 2];
                        B += -1 * pSrc[readPos + 3];
                        G += -1 * pSrc[readPos + 4];
                        R += -1 * pSrc[readPos + 5];
                        B += -1 * pSrc[readPos + 6];
                        G += -1 * pSrc[readPos + 7];
                        R += -1 * pSrc[readPos + 8];
                        //���i
                        readPos = (x - 1) * 3 + srcData.Stride * (y);
                        B += -1 * pSrc[readPos + 0];
                        G += -1 * pSrc[readPos + 1];
                        R += -1 * pSrc[readPos + 2];
                        B += 8 * pSrc[readPos + 3];
                        G += 8 * pSrc[readPos + 4];
                        R += 8 * pSrc[readPos + 5];
                        BB = pSrc[readPos + 3];
                        GG = pSrc[readPos + 4];
                        RR = pSrc[readPos + 5];
                        //B += 9 * pSrc[readPos + 3];
                        //G += 9 * pSrc[readPos + 4];
                        //R += 9 * pSrc[readPos + 5];
                        B += -1 * pSrc[readPos + 6];
                        G += -1 * pSrc[readPos + 7];
                        R += -1 * pSrc[readPos + 8];
                        //���i
                        readPos = (x - 1) * 3 + srcData.Stride * (y + 1);
                        B += -1 * pSrc[readPos + 0];
                        G += -1 * pSrc[readPos + 1];
                        R += -1 * pSrc[readPos + 2];
                        B += -1 * pSrc[readPos + 3];
                        G += -1 * pSrc[readPos + 4];
                        R += -1 * pSrc[readPos + 5];
                        B += -1 * pSrc[readPos + 6];
                        G += -1 * pSrc[readPos + 7];
                        R += -1 * pSrc[readPos + 8];

                        //���v���V�A���ɔ{���������A�����Z������
                        R = RR + (int)(R * k / 100);
                        G = GG + (int)(G * k / 100);
                        B = BB + (int)(B * k / 100);

                        //�␳
                        if (R < 0) R = 0;
                        if (G < 0) G = 0;
                        if (B < 0) B = 0;
                        if (R > 255) R = 255;
                        if (G > 255) G = 255;
                        if (B > 255) B = 255;

                        //�������݁F�R��RGB��3byte��\��MagicNumber
                        writePos = (x) * 3 + srcData.Stride * (y);
                        pDest[writePos + 0] = (byte)B;
                        pDest[writePos + 1] = (byte)G;
                        pDest[writePos + 2] = (byte)R;
                    }
                }
                //); //Parallel.For
            }//unsafe

            //Bitmap�̉��
            srcbmp.UnlockBits(srcData);
            destbmp.UnlockBits(destData);

            Debug.WriteLine(sw.ElapsedMilliseconds, "unsharpness_unsafe()");
            return destbmp;
        }
    }
}
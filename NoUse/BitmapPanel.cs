using System;
using System.Collections.Generic;
using System.Text;

namespace Marmi
{
    /********************************************************************************/
    //�p��Bitmap�̍쐬�N���X
    /********************************************************************************/
    public class BitmapPanel
    {
        private int BoxMargin = 20;                     //�{�b�N�X�O�����̃}�[�W��
        private int padding = 5;                        //Key-Value�Ԃ�Padding
        private string SepareteString = " : ";          //2��\�����{���̃Z�p���[�^�[������
        private float LinePadding = 1f;                 //�s��

        private string FontName = "MS PGothic";         //�t�H���g��
        private int FontPoint = 10;                     //�t�H���g�̃|�C���g
        private Color FontColorKey = Color.RoyalBlue;   //�L�[�̐F
        private Color FontColorValue = Color.Black;     //�l�̐F�i�ʏ�̐F�j
        private Color BackColor = Color.White;          //�w�i�F
        private Color BorderColor = Color.DarkGray;     //�{�[�_�[�J���[

        public BitmapPanel()
        {
        }

        public Bitmap TextBorad(string[] str, bool isRoundCorner)
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
                    InitBackImage(isRoundCorner, height, width, g);

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

        public Bitmap TextBoard(string str, bool isRoundCorner)
        {
            float height = 0f;      //�v�����ꂽ����
            float width = 0f;       //�v�����ꂽ��

            using (Font font = new Font(FontName, FontPoint))
            {
                //�T�C�Y�𑪂�
                using (Bitmap b = new Bitmap(10, 10))
                using (Graphics g = Graphics.FromImage(b))
                {
                    SizeF sizef = g.MeasureString(str, font);
                    if (sizef.Width > width)
                        width = sizef.Width;
                    height = sizef.Height; // +LinePadding;
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
                    InitBackImage(isRoundCorner, height, width, g);

                    SizeF sizef = g.MeasureString(str, font);

                    g.DrawString(str, font, brush, BoxMargin, y);
                    y += sizef.Height + LinePadding;
                }
                return returnbmp;
            }//using font
        }

        public Bitmap TextBoard(KeyValuePair<string, string>[] kv, bool isRoundCorner)
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
                    InitBackImage(isRoundCorner, returnBitmap.Height, returnBitmap.Width, g);

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

        private void InitBackImage(bool isRoundCorner, float height, float width, Graphics g)
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

        private GraphicsPath CreateRoundedRectangle(int width, int height, int arc)
        {
            GraphicsPath path = new GraphicsPath(FillMode.Winding);
            path.AddArc(width - arc, 0, arc, arc, 270, 90);
            path.AddArc(width - arc, height - arc, arc, arc, 0, 90);
            path.AddArc(0, height - arc, arc, arc, 90, 90);
            path.AddArc(0, 0, arc, arc, 180, 90);
            path.AddArc(width - arc, 0, arc, arc, 270, 90);
            return path;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmi
{
    /********************************************************************************/
    //角丸Bitmapの作成クラス
    /********************************************************************************/
    public class BitmapPanel
    {
        private int BoxMargin = 20;                     //ボックス外周部のマージン
        private int padding = 5;                        //Key-Value間のPadding
        private string SepareteString = " : ";          //2列表示実施時のセパレーター文字列
        private float LinePadding = 1f;                 //行間

        private string FontName = "MS PGothic";         //フォント名
        private int FontPoint = 10;                     //フォントのポイント
        private Color FontColorKey = Color.RoyalBlue;   //キーの色
        private Color FontColorValue = Color.Black;     //値の色（通常の色）
        private Color BackColor = Color.White;          //背景色
        private Color BorderColor = Color.DarkGray;     //ボーダーカラー

        public BitmapPanel()
        {
        }

        public Bitmap TextBorad(string[] str, bool isRoundCorner)
        {
            float height = 0f;      //計測された高さ
            float width = 0f;       //計測された幅

            using (Font font = new Font(FontName, FontPoint))
            {
                //サイズを測る
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

                //Bitmapを作る
                Bitmap returnbmp = new Bitmap((int)width, (int)height);
                using (Graphics g = Graphics.FromImage(returnbmp))
                {
                    float y = BoxMargin;
                    //Brush brush = Brushes.Black;
                    SolidBrush brush = new SolidBrush(FontColorValue);

                    //箱の初期化
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
            float height = 0f;      //計測された高さ
            float width = 0f;       //計測された幅

            using (Font font = new Font(FontName, FontPoint))
            {
                //サイズを測る
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

                //Bitmapを作る
                Bitmap returnbmp = new Bitmap((int)width, (int)height);
                using (Graphics g = Graphics.FromImage(returnbmp))
                {
                    float y = BoxMargin;
                    //Brush brush = Brushes.Black;
                    SolidBrush brush = new SolidBrush(FontColorValue);

                    //箱の初期化
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

                        //Keyの大きさを確認
                        size = g.MeasureString(s.Key + SepareteString, font);
                        keyWidth = (keyWidth > size.Width) ? keyWidth : size.Width;
                        keyHeight = (keyHeight > size.Height) ? keyHeight : size.Height;

                        //Valueの大きさを確認
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
                    //箱の初期化
                    InitBackImage(isRoundCorner, returnBitmap.Height, returnBitmap.Width, g);

                    SolidBrush keyBrush = new SolidBrush(FontColorKey);
                    SolidBrush valueBrush = new SolidBrush(FontColorValue);

                    //文字を描く
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

                        //Keyを描写
                        g.DrawString(
                            kv[i].Key + SepareteString,
                            font,
                            //Brushes.SteelBlue,
                            keyBrush,
                            rcKey,
                            sf);

                        //Valueを描写
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
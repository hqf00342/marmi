using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Marmi
{
    public partial class KeyAccelerator : UserControl
    {
        public Keys keyData { get; set; }

        //private bool inputMode = false;
        //private Font font_bold = new System.Drawing.Font(DefaultFont, FontStyle.Bold);
        private Pen blue_pen = new Pen(Color.Blue, 1.0f);

        //public delegate void ValidateEventhandler(object o, EventArgs e);
        //public event ValidateEventhandler ValidateKey;

        #region キーコンフィグ

        /// <summary>
        /// 人間向けの文字列に変換するテーブル。
        /// </summary>
        private Dictionary<Keys, string> KeyDict = new Dictionary<Keys, string>()
        {
            {Keys.Left, "←"},
            {Keys.Right, "→"},
            {Keys.Up, "↑"},
            {Keys.Down, "↓"},
            {Keys.Oemcomma, ","},
            {Keys.OemPeriod, "."},
            {Keys.OemSemicolon, ":"},
            {Keys.Oemplus, ";"},
            {Keys.OemMinus, "-"},
            {Keys.OemBackslash, "_"},
            {Keys.OemCloseBrackets, "]"},
            {Keys.OemOpenBrackets, "["},
            {Keys.OemPipe, "|"},
            {Keys.Oem7, "^"},
            {Keys.Oemtilde, "@"},
            {Keys.Menu, " "},
            {Keys.Control, " "},
            {Keys.ControlKey, " "},
            {Keys.Shift, " "},
            {Keys.ShiftKey, " "},
            {Keys.None, " "},
            {Keys.OemQuestion, "?"},
            {Keys.D1, "1"},
            {Keys.D2, "2"},
            {Keys.D3, "3"},
            {Keys.D4, "4"},
            {Keys.D5, "5"},
            {Keys.D6, "6"},
            {Keys.D7, "7"},
            {Keys.D8, "8"},
            {Keys.D9, "9"},
            {Keys.D0, "0"},
            {Keys.IMEConvert, "変換"},
            {Keys.IMENonconvert, "無変換"},
            {Keys.Space, "(Space)"},
            {Keys.MButton, "マウス-中央"},
            {Keys.XButton1, "マウス-戻る"},
            {Keys.XButton2, "マウス-進む"},
        };

        #endregion キーコンフィグ

        public KeyAccelerator()
        {
            InitializeComponent();
            keyData = Keys.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle rect = this.ClientRectangle;

            //this.BackColor = inputMode ? Color.Yellow : SystemColors.ButtonFace;
            this.BackColor = this.Focused ? Color.Yellow : SystemColors.ButtonFace;
            //テキスト描写
            string s = KeydataToString(keyData);
            if (!string.IsNullOrEmpty(s))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    s,
                    //this.Focused ? font_bold : DefaultFont,
                    DefaultFont,
                    rect,
                    Color.Black,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);
            }

            rect.Width--;
            rect.Height--;
            e.Graphics.DrawRectangle(
                this.Focused ? blue_pen : Pens.DimGray,
                //inputMode ? blue_pen : Pens.DimGray,
                rect);
        }

        /// <summary>
        /// KeyDataを人間向けに文字列変換する
        /// Shift,Ctrlもつける。
        /// </summary>
        /// <param name="kd"></param>
        /// <returns></returns>
        private string KeydataToString(Keys kd)
        {
            Debug.WriteLine(kd.ToString());

            if (kd == Keys.None)
                return "";

            //KeysConverter kc = new KeysConverter();
            //Uty.WriteLine(kc.ConvertToString(kd));

            string s = string.Empty;

            //修飾キーを文字列に変換
            bool alt = (kd & Keys.Alt) == Keys.Alt;
            bool ctrl = (kd & Keys.Control) == Keys.Control;
            bool shift = (kd & Keys.Shift) == Keys.Shift;
            if (ctrl) s += "Ctrl+";
            if (alt) s += "Alt+";
            if (shift) s += "Shift+";

            //キー文字を追加
            Keys keycode = kd & Keys.KeyCode;
            var i = KeyDict.FirstOrDefault(a => a.Key == keycode).Value;
            string keystring = string.IsNullOrEmpty(i) ? keycode.ToString() : i;
            s += keystring;
            return s;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    //if(this.Focused)
                    //	inputMode = !inputMode;
                    break;

                case System.Windows.Forms.MouseButtons.Right:
                    //inputMode = false;
                    keyData = Keys.None;
                    break;

                case System.Windows.Forms.MouseButtons.Middle:
                    keyData = Keys.MButton;
                    break;

                case System.Windows.Forms.MouseButtons.XButton1:
                    keyData = Keys.XButton1;
                    break;

                case System.Windows.Forms.MouseButtons.XButton2:
                    keyData = Keys.XButton2;
                    break;
            }
            this.Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            //inputMode = false;
            this.Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            //inputMode = true;
            this.Invalidate();
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            //if (!inputMode)
            //	return;

            if (e.Alt)
                keyData = e.KeyData & ~Keys.Alt;

            //Uty.WriteLine(e.KeyData.ToString());

            //Altキーは修飾として使わせない。
            if ((e.KeyData & Keys.Alt) == Keys.Alt)
                keyData = e.KeyData & ~Keys.Alt;
            else
                keyData = e.KeyData;
            //keyData = e.KeyData;

            Debug.WriteLine(keyData.ToString());
            //タブや矢印を普通の入力としてコントロール移動させない
            e.IsInputKey = true;
            this.Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            //if (!inputMode)
            //	return;

            var keycode = keyData & Keys.KeyCode;
            switch (keycode)
            {
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.Menu:
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.None:
                    keyData = Keys.None;
                    break;
            }
            this.Invalidate();
        }
    }
}
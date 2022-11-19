using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Marmi
{
    public partial class KeyAccelerator : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(sender: this, e: new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T store, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(store, value))
                return false;
            store = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        private Keys _KeyData;
        public Keys KeyData { get => _KeyData; set => SetProperty(ref _KeyData, value); }

        //public Keys KeyData { get; set; }

        private readonly Pen _bluePen = new Pen(Color.Blue, 1.0f);

        #region キーコンフィグ

        /// <summary>
        /// 特殊キーに対するString変換テーブル。
        /// </summary>
        private Dictionary<Keys, string> _keyNameDic = new Dictionary<Keys, string>()
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
            KeyData = Keys.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle rect = this.ClientRectangle;

            this.BackColor = this.Focused ? Color.Yellow : SystemColors.ButtonFace;
            //テキスト描写
            string s = KeydataToString(KeyData);
            if (!string.IsNullOrEmpty(s))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    s,
                    DefaultFont,
                    rect,
                    Color.Black,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);
            }

            rect.Width--;
            rect.Height--;
            e.Graphics.DrawRectangle(
                this.Focused ? _bluePen : Pens.DimGray,
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

            var s = string.Empty;

            //修飾キーを文字列に変換
            bool alt = (kd & Keys.Alt) == Keys.Alt;
            bool ctrl = (kd & Keys.Control) == Keys.Control;
            bool shift = (kd & Keys.Shift) == Keys.Shift;
            if (ctrl) s += "Ctrl+";
            if (alt) s += "Alt+";
            if (shift) s += "Shift+";

            //キー文字を追加
            Keys keycode = kd & Keys.KeyCode;
            var i = _keyNameDic.FirstOrDefault(a => a.Key == keycode).Value;
            string keystring = string.IsNullOrEmpty(i) ? keycode.ToString() : i;
            s += keystring;
            return s;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;

                case MouseButtons.Right:
                    KeyData = Keys.None;
                    break;

                case MouseButtons.Middle:
                    KeyData = Keys.MButton;
                    break;

                case MouseButtons.XButton1:
                    KeyData = Keys.XButton1;
                    break;

                case MouseButtons.XButton2:
                    KeyData = Keys.XButton2;
                    break;
            }
            this.Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            this.Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Alt)
                KeyData = e.KeyData & ~Keys.Alt;

            //Altキーは修飾として使わせない。
            if ((e.KeyData & Keys.Alt) == Keys.Alt)
                KeyData = e.KeyData & ~Keys.Alt;
            else
                KeyData = e.KeyData;

            Debug.WriteLine(KeyData.ToString());
            //タブや矢印を普通の入力としてコントロール移動させない
            e.IsInputKey = true;
            this.Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            var keycode = KeyData & Keys.KeyCode;
            switch (keycode)
            {
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.Menu:
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.None:
                    KeyData = Keys.None;
                    break;
            }
            this.Invalidate();
        }
    }
}
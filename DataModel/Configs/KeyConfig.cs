﻿/********************************************************************************
KeyConfig
キー設定

2024年10月13日
BindableBase派生にしINotifyPropertyChangedに対応する。
しかし、全部をINotifyPropertyChangedにするのは面倒なので
必要なとき(初期値に戻す)にOnPropertyChangedを手動呼び出しする。

********************************************************************************/

using Mii;
using System.Windows.Forms;

namespace Marmi.DataModel
{
    public class KeyConfig : BindableBase
    {
        public Keys Key_Exit1 { get; set; }
        public Keys Key_Exit2 { get; set; }
        public Keys Key_Bookmark1 { get; set; }
        public Keys Key_Fullscreen1 { get; set; }
        public Keys Key_Dualview1 { get; set; }
        public Keys Key_ViewRatio1 { get; set; }
        public Keys Key_Recycle1 { get; set; }
        public Keys Key_Rotate1 { get; set; }

        public Keys Key_Nextpage1 { get; set; }
        public Keys Key_Nextpage2 { get; set; }
        public Keys Key_Prevpage1 { get; set; }
        public Keys Key_Prevpage2 { get; set; }
        public Keys Key_Prevhalf1 { get; set; }
        public Keys Key_Nexthalf1 { get; set; }
        public Keys Key_Toppage1 { get; set; }
        public Keys Key_Lastpage1 { get; set; }

        public Keys Key_Thumbnail { get; set; }
        public Keys Key_Sidebar { get; set; }

        public Keys Key_MinWindow { get; set; }

        public Keys Key_MultiForward { get; set; }
        public Keys Key_MultiBackward { get; set; }

        public void Init()
        {
            Key_Exit1 = Keys.Q;
            Key_Exit2 = Keys.None;
            Key_Bookmark1 = Keys.B;
            Key_Fullscreen1 = Keys.Escape;
            Key_Dualview1 = Keys.D | Keys.Shift;
            Key_ViewRatio1 = Keys.V;
            Key_Recycle1 = Keys.Delete;
            Key_Nextpage1 = Keys.Right;
            Key_Nextpage2 = Keys.D;
            Key_Prevpage1 = Keys.Left;
            Key_Prevpage2 = Keys.A;
            Key_Prevhalf1 = Keys.PageUp;
            Key_Nexthalf1 = Keys.PageDown;
            Key_Toppage1 = Keys.Home;
            Key_Lastpage1 = Keys.End;
            Key_Rotate1 = Keys.R;
            Key_Thumbnail = Keys.T;
            Key_Sidebar = Keys.S;
            Key_MinWindow = Keys.Z;
            Key_MultiForward = Keys.Right | Keys.Shift;
            Key_MultiBackward = Keys.Left | Keys.Shift;

            //WinFormsのデータバインド機構は
            //1つPropertyChangedを投げると全部チェックしてくれるため
            //1つだけ投げる
            OnPropertyChanged(nameof(Key_Exit1));
        }
    }
}
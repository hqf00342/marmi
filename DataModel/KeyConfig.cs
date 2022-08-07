﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Marmi.DataModel
{
    public class KeyConfig
    {
        public Keys Key_Exit1 { get; set; }
        public Keys Key_Exit2 { get; set; }
        public Keys Key_Bookmark1 { get; set; }
        public Keys Key_Bookmark2 { get; set; }
        public Keys Key_Fullscreen1 { get; set; }
        public Keys Key_Fullscreen2 { get; set; }
        public Keys Key_Dualview1 { get; set; }
        public Keys Key_Dualview2 { get; set; }
        public Keys Key_ViewRatio1 { get; set; }
        public Keys Key_ViewRatio2 { get; set; }
        public Keys Key_Recycle1 { get; set; }
        public Keys Key_Recycle2 { get; set; }
        public Keys Key_Rotate1 { get; set; }
        public Keys Key_Rotate2 { get; set; }

        public Keys Key_Nextpage1 { get; set; }
        public Keys Key_Nextpage2 { get; set; }
        public Keys Key_Prevpage1 { get; set; }
        public Keys Key_Prevpage2 { get; set; }
        public Keys Key_Prevhalf1 { get; set; }
        public Keys Key_Prevhalf2 { get; set; }
        public Keys Key_Nexthalf1 { get; set; }
        public Keys Key_Nexthalf2 { get; set; }
        public Keys Key_Toppage1 { get; set; }
        public Keys Key_Toppage2 { get; set; }
        public Keys Key_Lastpage1 { get; set; }
        public Keys Key_Lastpage2 { get; set; }


        public void Init()
        {
            Key_Exit1 = Keys.Q;
            Key_Exit2 = Keys.None;

            Key_Bookmark1 = Keys.B;
            Key_Bookmark2 = Keys.None;

            Key_Fullscreen1 = Keys.Escape;
            Key_Fullscreen2 = Keys.None;

            Key_Dualview1 = Keys.D;
            Key_Dualview2 = Keys.None;

            Key_ViewRatio1 = Keys.V;
            Key_ViewRatio2 = Keys.None;

            Key_Recycle1 = Keys.Delete;
            Key_Recycle2 = Keys.None;

            Key_Nextpage1 = Keys.Right;
            Key_Nextpage2 = Keys.None;

            Key_Prevpage1 = Keys.Left;
            Key_Prevpage2 = Keys.None;

            Key_Prevhalf1 = Keys.PageUp;
            Key_Prevhalf2 = Keys.None;

            Key_Nexthalf1 = Keys.PageDown;
            Key_Nexthalf2 = Keys.None;

            Key_Toppage1 = Keys.Home;
            Key_Toppage2 = Keys.None;

            Key_Lastpage1 = Keys.End;
            Key_Lastpage2 = Keys.None;

            Key_Rotate1 = Keys.R;
            Key_Rotate2 = Keys.None;
        }

    }
}
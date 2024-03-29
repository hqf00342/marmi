﻿using System.Windows.Forms;

/*
 * キーイベント
 *
 * ver1.61で切り出し
 * 2013年7月21日
 *
 */

namespace Marmi
{
    public partial class Form1 : Form
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            //スライドショー中だったら中断させる
            if (IsSlideShow)
            {
                StopSlideShow();
                return;
            }

            //Altキーは特別な動作
            if (e.KeyCode == Keys.Menu && !menuStrip1.Visible)
            {
                menuStrip1.Visible = true;
                AjustSidebarArrangement();
                return;
            }

            //ver1.61 Ctrl+TABも特殊動作
            if (e.KeyCode == Keys.Tab && e.Control)
            {
                WindowOperation.TryToChangeActiveWindow(e.Shift);
            }

            //ver1.80 Ctrl,Shitに対応するためKeyDataに変更
            if (KeyDefines.TryGetValue(e.KeyData, out var func))
                func?.Invoke();
        }
    }
}
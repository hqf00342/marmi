using System.Windows.Forms;

/*
キーイベント
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

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            /// 矢印キーへ対応させる
            switch (e.KeyCode)
            {
                //矢印キーが押されたことを表示する
                case Keys.Up:
                case Keys.Left:
                case Keys.Right:
                case Keys.Down:
                case Keys.Escape:
                    Uty.DebugPrint(e.KeyCode.ToString());
                    e.IsInputKey = true;
                    break;
            }
        }
    }
}
using System;
using System.IO;
using System.Windows.Forms;

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
        //protected override void OnDpiChanged(DpiChangedEventArgs e)
        //{
        //    base.OnDpiChanged(e);
        //}

        private async void Form1_Load(object sender, EventArgs e)
        {
            //設定をFormに適用する
            ApplyConfigToWindow();

            //初期化
            await InitMarmiAsync();
            UpdateToolbar();
            ResizeTrackBar();

            //起動パラメータの確認
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                //表示対象ファイルを取得
                //1つめに自分のexeファイル名が入っているので除く
                string[] a = new string[args.Length - 1];
                for (int i = 1; i < args.Length; i++)
                    a[i - 1] = args[i];

                //ファイルを渡して開始
                //CheckAndStart(a);
                await StartAsync(a);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            //非表示
            this.Hide();

            //ver1.77 MRUリストの更新
            App.Config.AddMRU(App.g_pi);

            //非同期IOスレッドの終了
            AsyncIO.StopThread();

            //Tempディレクトリの削除
            TempDirs.DeleteAll();

            //設定の保存
            if (App.Config.General.SaveConfig)
            {
                XmlFile.SaveToXmlFile(App.Config, App.ConfigFilename);
            }
            else
            {
                //コンフィグファイルを削除
                if (File.Exists(App.ConfigFilename))
                    File.Delete(App.ConfigFilename);
            }

            //Application.Idleの解放
            Application.Idle -= Application_Idle;

            //ver1.57 susie解放
            App.susie?.Dispose();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //フォーカスをもらった時のクリックがツールバーアイテムを押していたなら
            //そのツールバーを実行する。
            if (_hoverStripItem is ToolStripItem cnt)
            {
                cnt.PerformClick();
            }
        }

        #region リサイズ

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            //Uty.DebugPrint("enter");

            //ウィンドウサイズ、位置を保存
            if (this.WindowState == FormWindowState.Normal)
            {
                App.Config.windowSize = this.Size; //new Size(this.Width, this.Height);
                App.Config.windowLocation = this.Location; // new Point(this.Left, this.Top);
            }

            if (_thumbPanel != null && ViewState.ThumbnailView)
            {
                //サムネイル表示モード中
            }
            else
            {
                //画面を描写。ただしハイクオリティで
                UpdateStatusbar();
                if (PicPanel.Visible)
                    PicPanel.ResizeEnd();
            }

            //トラックバー表示を直す
            ResizeTrackBar();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            //初期化前なら何もしない。
            //フォーム生成前にResize()は呼ばれる可能性がある。
            if (App.Config == null)
                return;

            //最小化時には何もしない
            if (this.WindowState == FormWindowState.Minimized)
                return;

            //ver0.972 Sidebarをリサイズ
            AjustSidebarArrangement();

            //サムネイルか？
            //Formが表示する前にも呼ばれるのでThumbPanel != nullは必須
            if (_thumbPanel != null && ViewState.ThumbnailView)
            {
                //ver1.64 DockStyleにしたのでコメントアウト
                //Rectangle rect = GetClientRectangle();
                //g_ThumbPanel.Location = rect.Location;
                //g_ThumbPanel.Size = rect.Size;

                //ver0.91 ここでreturnしないと駄目では？
                //ThumbPanel.Refresh();
                //return;
            }

            ////リサイズ時に描写しない設定か
            //if (App.Config.isStopPaintingAtResize)
            //    return;

            //ステータスバーに倍率表示
            UpdateStatusbar();

            //ver1.60 タイトルバーDClickによる最大化の時、ResizeEnd()が飛ばない
            if (this.WindowState == FormWindowState.Maximized)
                OnResizeEnd(null);
        }

        #endregion リサイズ
    }
}
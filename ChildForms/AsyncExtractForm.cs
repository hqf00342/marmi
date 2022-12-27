using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Marmi
{
    public partial class AsyncExtractForm : Form
    {
        //対象となる書庫ファイル名
        public string ArchivePath;

        //展開先ディレクトリ
        public string ExtractDir { get; set; }

        //アニメーション用タイマー：ぐるぐる回る円
        private readonly System.Windows.Forms.Timer animateTimer = new System.Windows.Forms.Timer();

        private Stopwatch sw = null;
        private readonly Pen pen = new Pen(Color.SteelBlue, 4.0f);

        //展開用のスレッド。
        private Thread ExtractThread = null;

        //展開完了ファイル数.表示に使う
        private int ExtractedFiles = 0;

        //展開予定ファイル数.表示に使う
        private int TargetFileCount = 0;

        //処理中の書庫名.表示に使う
        private string progressArchiveName = string.Empty;

        //現在処理中のSevenZip
        private volatile SevenZipWrapper now7z = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AsyncExtractForm()
        {
            InitializeComponent();

            ArchivePath = string.Empty;
            ExtractDir = string.Empty;
        }

        private void AsyncExtractForm_Load(object sender, EventArgs e)
        {
            //初期化
            TargetFileCount = 0;
            ExtractedFiles = 0;

            //書庫名、展開先チェック
            Trace.Assert(!string.IsNullOrEmpty(ArchivePath)
                && !string.IsNullOrEmpty(ExtractDir),
                "Marmiで予期せぬエラーが発生しました。"
                );

            //ダブルバッファを有効にする。
            this.DoubleBuffered = true;

            // アイドル処理
            Application.Idle += new EventHandler(Application_Idle);

            //注意書きファイルを入れておく
            Uty.CreateAnnotationFile(ExtractDir);

            //アニメーションタイマースタート
            animateTimer.Interval = 30;
            animateTimer.Tick += new EventHandler(OnAnimateTimer_Tick);
            animateTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (sw == null)
                return;
            else
                DrawAnimateCircle(e.Graphics);
        }

        private void DrawAnimateCircle(Graphics g)
        {
            const long T1 = 2000;   //中側の円周期
            long angle1 = (sw.ElapsedMilliseconds * 360 / T1) % 360;
            const long T2 = 6000;   //外側の円周期６秒
            long angle2 = (sw.ElapsedMilliseconds * 360 / T2) % 360;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            PointF centerPoint = new PointF(30, 30);
            Matrix m = new Matrix();
            m.RotateAt(angle1, centerPoint);
            g.Transform = m;

            //中心
            int rad = 8;    //円１の半径
            RectangleF arcRect = new RectangleF(centerPoint.X - rad, centerPoint.Y - rad, rad * 2, rad * 2);
            //g.DrawArc(pen, arcRect, 0, 300);

            //角速度を変える
            //centerPoint = new PointF(20, 20);
            m.Reset();
            m.RotateAt(angle2, centerPoint);
            g.Transform = m;

            //３本円弧
            int rad2 = 15;  //円２の半径
            arcRect = new RectangleF(centerPoint.X - rad2, centerPoint.Y - rad2, rad2 * 2, rad2 * 2);
            g.DrawArc(pen, arcRect, 0, 100);
            g.DrawArc(pen, arcRect, 120, 100);
            g.DrawArc(pen, arcRect, 240, 100);
        }

        /// <summary>
        /// タイマーで呼び出されるメソッド
        /// </summary>
        private void OnAnimateTimer_Tick(object sender, EventArgs e)
        {
            if (sw == null)
                sw = Stopwatch.StartNew();
            else
                Invalidate();
        }

        /// <summary>
        /// アイドル処理
        /// 最初の呼び出し時に書庫展開スレッドを開始する。
        /// それ以降はスレッド終了を監視して終了時にフォームを閉じる。
        /// </summary>
        private void Application_Idle(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (ExtractThread == null)
            {
                //書庫展開開始
                ThreadStart tsAction = () =>
                {
                    RecurseExtractAll(ArchivePath, ExtractDir);
                };

                ExtractThread = new Thread(tsAction);
                ExtractThread.Name = "RecurseExtractAll";
                ExtractThread.IsBackground = true;
                ExtractThread.Start();
            }
            else
            {
                //終了確認
                if (ExtractThread.Join(0))
                {
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (ExtractThread != null)
            {
                while (!ExtractThread.Join(100))
                {
                    if (now7z != null)
                        now7z.IsCancelExtraction = true;
                }
            }
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 展開スレッド本体
        /// </summary>
        /// <param name="archivename">対象書庫名</param>
        /// <param name="extractDir">展開先フォルダ名</param>
        public void RecurseExtractAll(string archivename, string extractDir)
        {
            //処理中の書庫名を表示させるために登録。
            progressArchiveName = Path.GetFileName(archivename);

            //7zipで展開
            var sz = new SevenZipWrapper();
            sz.Open(archivename);
            //キャンセル処理のため登録
            now7z = sz;
            //イベント登録
            sz.ExtractEventHandler += new Action<string>(FileExtracting);
            //書庫内ファイル数
            TargetFileCount += sz.Items.Count;
            //展開開始
            sz.ExtractAll(extractDir);

            //ver1.34展開したディレクトリ内を走査する
            //ver1.77 usingの外に移動
            RecurseDir(extractDir);
        }

        /// <summary>
        /// 展開したディレクトリ内を走査する
        /// </summary>
        /// <param name="targetDir">捜査対象のディレクトリ</param>
        private void RecurseDir(string targetDir)
        {
            //ファイルをRecurce
            string[] exfiles = Directory.GetFiles(targetDir);
            foreach (string file in exfiles)
            {
                if (Uty.IsSupportArchiveFile(file))
                {
                    string extDirName = Uty.GetUniqueDirname(file);
                    Debug.WriteLine(file, extDirName);
                    RecurseExtractAll(file, extDirName);
                }
            }
            //ディレクトリをRecurce
            string[] exdirs = Directory.GetDirectories(targetDir);
            foreach (string dir in exdirs)
            {
                RecurseDir(dir);
            }
        }

        /// <summary>
        /// 7Zip展開イベントハンドラーに呼び出される。
        /// キャンセル処理をしている。
        /// </summary>
        /// <param name="obj">ファイル名</param>
        private void FileExtracting(string obj)
        {
            //string filename = obj as string;
            ExtractedFiles++;
            string s = $"{ExtractedFiles}/{TargetFileCount} : {Path.GetFileName(progressArchiveName)}";

            if (now7z.IsCancelExtraction)
                s = "キャンセル処理中";

            BeginInvoke((MethodInvoker)(() => { labelInfo.Text = s; }));
        }
    }
}
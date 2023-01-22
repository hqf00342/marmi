﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;			//Path
using System.Diagnostics;	//Trace
using System.Threading;		//Thread
using System.Drawing.Drawing2D;


namespace Marmi
{
	public partial class AsyncExtractForm : Form
	{
		//対象となる書庫ファイル名
		public string ArchivePath;

		//展開先ディレクトリ
		public string ExtractDir { get; set; }

		//アニメーション用タイマー：ぐるぐる回る円
		System.Windows.Forms.Timer animateTimer = new System.Windows.Forms.Timer();
		Stopwatch sw = null;
		Pen pen = new Pen(Color.SteelBlue, 4.0f);

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

		//unrar5対応フラグ
		//private bool SupportUnrar5 = false;

		//キャンセル用フラグ
		private volatile bool isCancel = false;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public AsyncExtractForm()
		{
			InitializeComponent();

			ArchivePath = string.Empty;
			ExtractDir = string.Empty;
			isCancel = false;

			//using(Unrar u = new Unrar())
			//{
			//	SupportUnrar5 = u.dllLoaded;
			//}
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
			Uty.MakeAttentionTextfile(ExtractDir);

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
			const long T1 = 2000;	//中側の円周期
			long angle1 = (sw.ElapsedMilliseconds * 360 / T1) % 360;
			const long T2 = 6000;	//外側の円周期６秒
			long angle2 = (sw.ElapsedMilliseconds * 360 / T2) % 360;

			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.Clear(BackColor);
			PointF centerPoint = new PointF(30, 30);
			Matrix m = new Matrix();
			m.RotateAt(angle1, centerPoint);
			g.Transform = m;

			//中心
			int rad = 8;	//円１の半径
			RectangleF arcRect = new RectangleF(centerPoint.X - rad, centerPoint.Y - rad, rad * 2, rad * 2);
			//g.DrawArc(pen, arcRect, 0, 300);

			//角速度を変える
			//centerPoint = new PointF(20, 20);
			m.Reset();
			m.RotateAt(angle2, centerPoint);
			g.Transform = m;

			//３本円弧
			int rad2 = 15;	//円２の半径
			arcRect = new RectangleF(centerPoint.X - rad2, centerPoint.Y - rad2, rad2 * 2, rad2 * 2);
			g.DrawArc(pen, arcRect, 0, 100);
			g.DrawArc(pen, arcRect, 120, 100);
			g.DrawArc(pen, arcRect, 240, 100);
		}

		/// <summary>
		/// タイマーで呼び出されるメソッド
		/// </summary>
		void OnAnimateTimer_Tick(object sender, EventArgs e)
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
		void Application_Idle(object sender, EventArgs e)
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
			isCancel = true; //for unrar5

			if (ExtractThread != null)
			{
				while (!ExtractThread.Join(100))
				{
					if(now7z != null)
						now7z.isCancelExtraction = true;
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

			//if(SupportUnrar5 && archivename.ToLower().EndsWith(".rar"))
			if (Unrar.DllCheck() && archivename.ToLower().EndsWith(".rar"))
			{
				//unrar5で展開する
				//ファイル一覧を読み込む。
				using(Unrar rar = new Unrar())
				{
					rar.Open(archivename, Unrar.OpenMode.List);
					TargetFileCount = rar.ListFiles().Length;
				}

				using(Unrar rar = new Unrar())
				{
					rar.NewFile += rar_NewFile;
					rar.PasswordRequired += rar_PasswordRequired;
					rar.DestinationPath = extractDir;
					rar.Open(archivename, Unrar.OpenMode.Extract);
					
					//展開ループ
					while(rar.ReadHeader())
					{
						try
						{
							rar.Extract();
						}
						catch(Exception e)
						{
							MessageBox.Show("パスワードが間違っているか書庫が壊れています");
							Debug.WriteLine("unrar.dll exception : " + e.Message);
							isCancel = true;
							return;
						}
						if (isCancel)
							return;
					}
				}
			}
			else
			{
				//7zipで展開
				using (SevenZipWrapper sz = new SevenZipWrapper(archivename))
				{
					//キャンセル処理のため登録
					now7z = sz;
					//イベント登録
					sz.ExtractEventHandler += new Action<string>(FileExtracting);
					//書庫内ファイル数
					TargetFileCount += sz.Items.Count;
					//展開開始
					sz.ExtractAll(extractDir);

					////ver1.34展開したディレクトリ内を走査する
					//ver1.77 usingの外に移動
					//RecurseDir(extractDir);
				}
			}

			//ver1.34展開したディレクトリ内を走査する
			//ver1.77 usingの外に移動
			RecurseDir(extractDir);

		}

		/// <summary>
		/// rar用のパスワード処理イベント実装
		/// </summary>
		void rar_PasswordRequired(object sender, PasswordRequiredEventArgs e)
		{
			FormPassword fp = new FormPassword();
			fp.TopMost = true;
			fp.StartPosition = FormStartPosition.CenterParent;
			if (fp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				e.Password = fp.PasswordText;	//パスワードを設定
				e.ContinueOperation = true;		//処理は続行
			}
			else
			{
				e.ContinueOperation = false;	//中止
				isCancel = true;				//キャンセル処理にする
			}
		}


		void rar_NewFile(object sender, NewFileEventArgs e)
		{
			//throw new NotImplementedException();
			ExtractedFiles++;
			string s = string.Format(
				"{0}/{1} : {2}",
				ExtractedFiles,
				TargetFileCount,
				Path.GetFileName(progressArchiveName)
				);

			//if (now7z.isCancelExtraction)
			//	s = "キャンセル処理中";

			BeginInvoke((MethodInvoker)(() => { labelInfo.Text = s; }));
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
				if (Uty.isAvailableArchiveFile(file))
				{
					string extDirName = Uty.getUniqueDirname(file);
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
		void FileExtracting(string obj)
		{
			//string filename = obj as string;
			ExtractedFiles++;
			string s = string.Format(
				"{0}/{1} : {2}",
				ExtractedFiles,
				TargetFileCount,
				Path.GetFileName(progressArchiveName)
				);

			if (now7z.isCancelExtraction)
				s = "キャンセル処理中";

			BeginInvoke((MethodInvoker)(() => { labelInfo.Text = s; }));
		}
	}
}

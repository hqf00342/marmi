using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;	//DllImport
using System.Diagnostics;				//Process, Debug, Stopwatch
using System.Linq;						//Linq

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
		#region DllImport
		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll")]
		extern static bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		extern static bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		// 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
		//[DllImport("user32.dll")]
		//private static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
		[DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		// ShowWindowAsync関数のパラメータに渡す定義値
		private const int SW_RESTORE = 9;  // 画面を元の大きさに戻す
		private const int WM_USER = 0x400;
		private const int MY_FORCE_FOREGROUND_MESSAGE = WM_USER + 1;
		#endregion


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			//Debug.WriteLine(e.KeyCode, "KeyCode");
			Debug.WriteLine(e.KeyData, "KeyData");

			//スライドショー中だったら中断させる
			if (isSlideShow)
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
				TryToChangeActiveWindow(e.Shift);
			}

			//キー毎のメソッドを実行
			MethodInvoker func = null;
			//if (KeyMethods.TryGetValue(e.KeyCode, out func))
			//	func();
			//ver1.80 Ctrl,Shitに対応するためKeyDataに変更
			if (KeyMethods.TryGetValue(e.KeyData, out func))
				if(func != null)
					func();

		}


		private void TryToChangeActiveWindow(bool isForword)
		{
			Process prev = null;
			if (isForword)
				prev = GetNextProcess();
			else
				prev = GetPreviousProcess();

			if (prev != null)
				WakeupWindow(prev.MainWindowHandle);
			//ない時はなにもしない
		}


		public static Process GetNextProcess()
		{
			Process curProcess = Process.GetCurrentProcess();
			Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

			if (allProcesses.Length <= 1)
				return null;

			var sortedProcesses = allProcesses.Where(p => p.Id != curProcess.Id).OrderBy(p => p.StartTime);
			var retproc = sortedProcesses.Where(p => p.StartTime > curProcess.StartTime).FirstOrDefault();
			return retproc ?? sortedProcesses.First();
		}


		public static Process GetPreviousProcess()
		{
			Process curProcess = Process.GetCurrentProcess();
			Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

			if (allProcesses.Length <= 1)
				return null;

			var sortedProcesses = allProcesses.Where(p => p.Id != curProcess.Id).OrderByDescending(p => p.StartTime);
			var retproc = sortedProcesses.Where(p => p.StartTime < curProcess.StartTime).FirstOrDefault();
			return retproc ?? sortedProcesses.First();
		}


		/// <summary>
		/// 指定のウィンドウを最前面に
		/// </summary>
		/// <param name="hWnd">最前面にしたいウィンドウのハンドル</param>
		private void WakeupWindow(IntPtr hWnd)
		{
			// メイン・ウィンドウが最小化されていれば元に戻す
			if (IsIconic(hWnd))
				ShowWindowAsync(hWnd, SW_RESTORE);

			// メイン・ウィンドウを最前面に表示する
			SetForegroundWindow(hWnd);
		}
	}
}
using System;
using System.Diagnostics;
using System.Linq;
/*
Windowを操作するユーティリティメソッド

*/
namespace Marmi
{
    public static class WindowOperation
    {
		public static void TryToChangeActiveWindow(bool isForword)
		{
			Process prev = isForword ? GetNextProcess() : GetPreviousProcess();

			if (prev != null)
				WakeupWindow(prev.MainWindowHandle);
			//ない時はなにもしない
		}

		private static Process GetNextProcess()
		{
			Process curProcess = Process.GetCurrentProcess();
			Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

			if (allProcesses.Length <= 1)
				return null;

			var sortedProcesses = allProcesses.Where(p => p.Id != curProcess.Id).OrderBy(p => p.StartTime);
			var retproc = sortedProcesses.Where(p => p.StartTime > curProcess.StartTime).FirstOrDefault();
			return retproc ?? sortedProcesses.First();
		}

		private static Process GetPreviousProcess()
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
		private static void WakeupWindow(IntPtr hWnd)
		{
			// メイン・ウィンドウが最小化されていれば元に戻す
			if (Win32.IsIconic(hWnd))
				Win32.ShowWindowAsync(hWnd, Win32.SW_RESTORE);

			// メイン・ウィンドウを最前面に表示する
			Win32.SetForegroundWindow(hWnd);
		}
	}
}

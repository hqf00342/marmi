using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;	//Process
using System.Runtime.InteropServices; //DllImport
using System.Threading; //Mutex
using System.Runtime.Remoting; //RemotingServices.Marshal()
using System.Runtime.Remoting.Channels; //ChannelServices.RegisterChannel()
using System.Runtime.Remoting.Channels.Ipc; //IpcChannel 参照の追加でSystem.Runtime.Remotingが必要
using System.Runtime.Remoting.Lifetime; //LifetimeServices

namespace Marmi
{
	public interface IRemoteObject
	{
		void IPCMessage(string[] args);
	}

	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());


			// 設定ファイルの読み込み
			// ver1.76 Form1::Form1()から移動
			// ここで読み込んで多重起動禁止フラグを確認する。
			Form1.g_Config = (AppGlobalConfig)AppGlobalConfig.LoadFromXmlFile();
			if (Form1.g_Config == null)
				Form1.g_Config = new AppGlobalConfig();

			//多重起動確認
			if(Form1.g_Config.disableMultipleStarts)
			{
				//多重起動は禁止されているためmutexで制御
				var APPNAME = Application.ProductName;
				var OBJECTNAME = "Marmi1";

				using (var mutex = new Mutex(false, APPNAME))
				{
					if (mutex.WaitOne(0, false))
					{
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);

						//IPCサーバを起動する
						Form1 form = new Form1();
						LifetimeServices.LeaseTime = TimeSpan.Zero;
						LifetimeServices.RenewOnCallTime = TimeSpan.Zero;
						IpcChannel ipc = new IpcChannel(APPNAME);
						ChannelServices.RegisterChannel(ipc, false);
						RemotingServices.Marshal(form, OBJECTNAME);

						//起動
						Application.Run(form);
					}
					else
					{
						//クライアント起動
						var URL = string.Format("ipc://{0}/{1}", APPNAME, OBJECTNAME);
						var remoteObject = RemotingServices.Connect(typeof(IRemoteObject), URL) as IRemoteObject;
						//オブジェクトへコマンドライン引数を渡す
						remoteObject.IPCMessage(Environment.GetCommandLineArgs());
					}
				}

			}
			else
			{
				//多重起動は許可されているので通常起動
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new Form1());
			}
		}

		#region DllImport
		[DllImport("USER32.DLL", CharSet = CharSet.Auto)]
		private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
		[DllImport("USER32.DLL", CharSet = CharSet.Auto)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		private const int SW_NORMAL = 1;
		#endregion

		/// <summary>
		/// 自分と同じプロセス名を探し、そのプロセスを表示させる
		/// 多重起動防止のためのもの
		/// </summary>
		/// <returns></returns>
		public static bool ShowPrevProcess()
		{
			Process hThis = Process.GetCurrentProcess();
			Process[] hProcesses = Process.GetProcessesByName(hThis.ProcessName);
			int iThisProcessId = hThis.Id;

			foreach (Process hProcess in hProcesses)
			{
				if (hProcess.Id != iThisProcessId)
				{
					ShowWindow(hProcess.MainWindowHandle, SW_NORMAL);
					SetForegroundWindow(hProcess.MainWindowHandle);
					return true;
				}
			}
			return false;
		}
	}
}
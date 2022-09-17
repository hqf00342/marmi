namespace Marmi;

internal class Notificator
{
    internal static void ClearPanel(string msg, int intervalMsec = 1000)
    {
        App.MainForm.ShowClearPanel(msg, intervalMsec);
    }
}
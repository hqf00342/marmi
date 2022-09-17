/****************************************************************************
Form1.cs
IPCMessage部分
****************************************************************************/

using System.Linq;

namespace Marmi;

public partial class Form1
{
    /// <summary>
    /// IPCで呼ばれるインターフェースメソッド
    /// コマンドライン引数が入ってくるのでそれを起動。
    /// ただし、このメソッドが呼ばれるときはフォームのスレッドではないので
    /// Invokeが必要
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    void IRemoteObject.IPCMessage(string[] args)
    {
        this.Invoke(() =>
        {
            //自分を前面にする
            this.Activate();

            //表示対象ファイルを取得
            //1つめに自分のexeファイル名が入っているので除く
            if (args.Length > 1)
            {
                AsyncStart(args.Skip(1).ToArray());
            }
        });
    }
}
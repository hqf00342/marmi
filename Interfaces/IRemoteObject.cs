/*
  多重起動を抑えるためのIPCメッセージ
*/
namespace Marmi
{
    public interface IRemoteObject
    {
        void IPCMessage(string[] args);
    }
}

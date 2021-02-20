namespace Marmi
{
    //スレッドの状態を表す
    internal enum ThreadStatus
    {
        STOP,           //スレッドは動作していない
        RUNNING,        //スレッドは動作中
        REQUEST_STOP,   //スレッドに停止要求中。STOPに変わる
        PAUSE,          //スレッドに中断要求。次にRUNNINGになるまで待つ。
    }
}
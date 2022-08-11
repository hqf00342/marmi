#define USE_HIGH_STACK

using System.Collections.Generic;
/*
High/Low ２つのキューを持つクラス。
Popする際にHighがあればHighを優先、なければLowからも供出する。
タスク管理に利用しているため、同時アクセス対応としてlockしている。

HIGHキューに
  Stackを使う場合は「USE_HIGH_STACK」を有効にする
  Queueを使う場合は無効にする。
*/
namespace Marmi
{
    internal class PrioritySafeQueue<T>
    {
#if USE_HIGH_STACK
        private readonly Stack<T> _highQueue = new Stack<T>();
#else
        private readonly Queue<T> _highQueue = new Queue<T>();
#endif
        private readonly Queue<T> _lowQueue = new Queue<T>();

        private readonly object _syncRoot = new object();

        public int Count => _highQueue.Count + _lowQueue.Count;

        public void PushHigh(T t)
        {
            lock (_syncRoot)
            {
#if USE_HIGH_STACK
                _highQueue.Push(t);
#else
                _highQueue.Enqueue(t);
#endif
            }
        }

        public void PushLow(T t)
        {
            lock (_syncRoot)
            {
                _lowQueue.Enqueue(t);
            }
        }

        public T Pop()
        {
            lock (_syncRoot)
            {
                if (_highQueue.Count > 0)
#if USE_HIGH_STACK
                    return _highQueue.Pop();
#else
                    return _highQueue.Dequeue();
#endif
                else
                    return _lowQueue.Dequeue();
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _highQueue.Clear();
                _lowQueue.Clear();
            }
        }

        public T[] ToArrayHigh()
        {
            lock (_syncRoot)
            {
                return _highQueue.ToArray();
            }
        }

        public T[] ToArray()
        {
            lock (_syncRoot)
            {
                T[] temp = new T[_highQueue.Count + _lowQueue.Count];
                _highQueue.ToArray().CopyTo(temp, 0);
                _lowQueue.ToArray().CopyTo(temp, _highQueue.Count);
                return temp;
            }
        }
    }
}
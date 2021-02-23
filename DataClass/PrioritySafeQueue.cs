using System.Collections.Generic;
/*
High/Low ２つのキューを持つクラス。
Popする際にHighがあればHighを優先、なければLowからも供出する。
タスク管理に利用しているため、同時アクセス対応としてlockしている。
*/
namespace Marmi
{
    internal class PrioritySafeQueue<T>
    {
        private readonly Queue<T> _highQueue = new Queue<T>();

        private readonly Queue<T> _lowQueue = new Queue<T>();

        private readonly object _syncRoot = new object();

        public int Count => _highQueue.Count + _lowQueue.Count;

        public void PushHigh(T t)
        {
            lock (_syncRoot)
            {
                _highQueue.Enqueue(t);
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
                    return _highQueue.Dequeue();
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
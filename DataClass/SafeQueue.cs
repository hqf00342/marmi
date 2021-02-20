using System.Collections.Generic;

namespace Marmi
{
    internal class SafeQueue<T>
    {
        private Queue<T> m_highQueue = new Queue<T>();
        private object syncRoot = new object();

        public SafeQueue()
        {
        }

        public int Count { get { return m_highQueue.Count; } }

        public void Push(T t)
        {
            lock (syncRoot)
            {
                m_highQueue.Enqueue(t);
            }
        }

        public T Pop()
        {
            lock (syncRoot)
            {
                return m_highQueue.Dequeue();
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                m_highQueue.Clear();
            }
        }

        public T Peek()
        {
            lock (syncRoot)
            {
                return m_highQueue.Peek();
            }
        }

        public T[] ToArray()
        {
            lock (syncRoot)
            {
                return m_highQueue.ToArray();
            }
        }
    }
}
using System.Collections.Generic;

namespace Marmi
{
    internal class PrioritySafeQueue<T>
    {
        private Queue<T> m_highQueue = new Queue<T>();
        private Queue<T> m_lowQueue = new Queue<T>();
        private object syncRoot = new object();

        public PrioritySafeQueue()
        {
        }

        public int Count { get { return m_highQueue.Count + m_lowQueue.Count; } }

        public void Push(T t)
        {
            PushHigh(t);
        }

        public void PushHigh(T t)
        {
            lock (syncRoot)
            {
                m_highQueue.Enqueue(t);
            }
        }

        public void PushLow(T t)
        {
            lock (syncRoot)
            {
                m_lowQueue.Enqueue(t);
            }
        }

        public T Pop()
        {
            lock (syncRoot)
            {
                if (m_highQueue.Count > 0)
                    return m_highQueue.Dequeue();
                else
                    return m_lowQueue.Dequeue();
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                m_highQueue.Clear();
                m_lowQueue.Clear();
            }
        }

        //public T Peek()
        //{
        //    lock (syncRoot)
        //    {
        //        return m_highQueue.Peek();
        //    }
        //}

        public T[] ToArrayHigh()
        {
            lock (syncRoot)
            {
                return m_highQueue.ToArray();
            }
        }

        public T[] ToArray()
        {
            lock (syncRoot)
            {
                T[] temp = new T[m_highQueue.Count + m_lowQueue.Count];
                m_highQueue.ToArray().CopyTo(temp, 0);
                m_lowQueue.ToArray().CopyTo(temp, m_highQueue.Count);
                return temp;
            }
        }
    }
}
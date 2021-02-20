using System.Collections.Generic;

namespace Marmi
{
    internal class SafeStack<T>
    {
        private Stack<T> m_stack = new Stack<T>();

        //private Thread th = null;
        private object syncRoot = new object();

        public SafeStack()
        {
        }

        public int Count
        {
            get { return m_stack.Count; }
        }

        public void Push(T t)
        {
            lock (syncRoot)
            {
                //重複はスタックしない
                //if (m_stack.Contains(t))
                //{
                //    Uty.WriteLine("skip Push({0}", t);
                //    return;
                //}
                m_stack.Push(t);
            }
        }

        public T Pop()
        {
            lock (syncRoot)
            {
                return m_stack.Pop();
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                m_stack.Clear();
            }
        }

        public T Peek()
        {
            lock (syncRoot)
            {
                return m_stack.Peek();
            }
        }

        public T[] ToArray()
        {
            lock (syncRoot)
            {
                return m_stack.ToArray();
            }
        }
    }
}
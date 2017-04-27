using System.Collections.Concurrent;

namespace Mentoring.Multithreading.Utils
{
    public class FixedSizedQueue<T>
    {
        private readonly object privateLockObject = new object();

        public readonly ConcurrentQueue<T> Queue = new ConcurrentQueue<T>();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public void Enqueue(T obj)
        {
            Queue.Enqueue(obj);

            lock (privateLockObject)
            {
                while (Queue.Count > Size)
                {
                    T outObj;
                    Queue.TryDequeue(out outObj);
                }
            }
        }
    }
}

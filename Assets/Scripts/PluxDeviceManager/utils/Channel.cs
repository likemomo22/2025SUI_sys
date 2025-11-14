using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityThreading
{
    public class Channel<T> : IDisposable
    {
        private readonly List<T> buffer = new();
        private readonly object disposeRoot = new();
        private readonly object getSyncRoot = new();
        private readonly object setSyncRoot = new();
        private bool disposed;
        private ManualResetEvent exitEvent = new(false);
        private ManualResetEvent getEvent = new(true);
        private ManualResetEvent setEvent = new(false);

        public Channel()
            : this(1)
        {
        }

        public Channel(int bufferSize)
        {
            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException("bufferSize", "Must be greater or equal to 1.");

            BufferSize = bufferSize;
        }

        public int BufferSize { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (disposed)
                return;

            lock (disposeRoot)
            {
                exitEvent.Set();

                lock (getSyncRoot)
                {
                    lock (setSyncRoot)
                    {
                        setEvent.Close();
                        setEvent = null;

                        getEvent.Close();
                        getEvent = null;

                        exitEvent.Close();
                        exitEvent = null;

                        disposed = true;
                    }
                }
            }
        }

        #endregion

        ~Channel()
        {
            Dispose();
        }

        public void Resize(int newBufferSize)
        {
            if (newBufferSize < 1)
                throw new ArgumentOutOfRangeException("newBufferSize", "Must be greater or equal to 1.");

            lock (setSyncRoot)
            {
                if (disposed)
                    return;

                var result = WaitHandle.WaitAny(new WaitHandle[] { exitEvent, getEvent });
                if (result == 0)
                    return;

                buffer.Clear();

                if (newBufferSize != BufferSize)
                    BufferSize = newBufferSize;
            }
        }

        public bool Set(T value)
        {
            return Set(value, int.MaxValue);
        }

        public bool Set(T value, int timeoutInMilliseconds)
        {
            lock (setSyncRoot)
            {
                if (disposed)
                    return false;

                var result = WaitHandle.WaitAny(new WaitHandle[] { exitEvent, getEvent }, timeoutInMilliseconds);
                if (result == WaitHandle.WaitTimeout || result == 0)
                    return false;

                buffer.Add(value);
                if (buffer.Count == BufferSize)
                {
                    setEvent.Set();
                    getEvent.Reset();
                }

                return true;
            }
        }

        public T Get()
        {
            return Get(int.MaxValue, default);
        }

        public T Get(int timeoutInMilliseconds, T defaultValue)
        {
            lock (getSyncRoot)
            {
                if (disposed)
                    return defaultValue;

                var result = WaitHandle.WaitAny(new WaitHandle[] { exitEvent, setEvent }, timeoutInMilliseconds);
                if (result == WaitHandle.WaitTimeout || result == 0)
                    return defaultValue;

                var value = buffer[0];
                buffer.RemoveAt(0);
                if (buffer.Count == 0)
                {
                    getEvent.Set();
                    setEvent.Reset();
                }

                return value;
            }
        }

        public void Close()
        {
            lock (disposeRoot)
            {
                if (disposed)
                    return;

                exitEvent.Set();
            }
        }
    }

    public class Channel : Channel<object>
    {
    }
}
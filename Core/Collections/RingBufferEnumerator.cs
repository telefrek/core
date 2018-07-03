using System;
using System.Collections;
using System.Collections.Generic;

namespace Telefrek.Core.Collections
{
    /// <summary>
    /// Enumerates over a collection of objects contained in a <see cref="Telefrek.Core.Collections.RingBuffer"/>.
    /// </summary>
    /// <typeparam name="T">The type of object exposed in the buffer.</typeparam>
    public sealed class RingBufferEnumerator<T> : IEnumerator<T>
    {
        IRingBuffer<T> _buffer;
        T _current;

        public RingBufferEnumerator(IRingBuffer<T> buffer)
        {
            _buffer = buffer;
            _current = default(T);
        }

        #region IEnumerator<T> Members

        T IEnumerator<T>.Current
        {
            get { return _current; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            // Nothing to do here
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return _current; }
        }

        bool IEnumerator.MoveNext()
        {
            // If it's not complete, block the wait for a publisher to send a new item
            if (!_buffer.IsComplete)
                return _buffer.TryRemove(out _current);

            // If the count is 0 and the buffer is completed, no other messages will come
            if (_buffer.Count == 0)
                return false;

            // Block and try to remove the next entry
            return _buffer.TryRemove(out _current);
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException("Ring Buffers cannot be reset.");
        }

        #endregion
    }
}
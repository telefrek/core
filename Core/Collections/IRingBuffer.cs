using System;
using System.Collections;
using System.Collections.Generic;

namespace Telefrek.Core.Collections
{
    public interface IRingBuffer<T> : IEnumerable, IEnumerable<T>, ICollection<T>, IDisposable
    {
        void Open();
        void Close();

        bool IsComplete { get; }

        bool TryAdd(T instance);
        bool TryAdd(T instance, TimeSpan timeout);
        bool TryAddRange(T[] instances);
        bool TryAddRange(T[] instances, TimeSpan timeout);

        bool TryRemove(out T instance);
        bool TryRemove(out T instance, TimeSpan timeout);
        bool TryRemoveRange(out T[] instances);
        bool TryRemoveRange(out T[] instances, TimeSpan timeout);
        bool TryRemoveRange(out T[] instances, int maxItems, TimeSpan timeout);
        bool TryRemoveRange(out T[] instances, int minItems, int maxItems, TimeSpan timeout);
    }
}
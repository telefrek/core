using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Telefrek.Core
{
    /// <summary>
    /// Provides a highly efficient bit mask for manipulating and tracking sets via bits.
    /// </summary>
    public sealed class BitMask
    {
        const byte MOD_64 = 0x3F;

        int _sz;
        ulong[] _raw;
        object _syncLock;

        /// <summary>
        /// Creates a new bitmask with the number of bits specified.
        /// </summary>
        /// <param name="size">The number of bits to use.</param>
        public BitMask(int size)
        {
            _sz = size;
            var rem = size & MOD_64;
            _raw = new ulong[(size >> 6) + (rem > 0x0 ? 1 : 0)];
            for (var i = 0; i < rem; ++i)
                _raw[_sz >> 6] |= (ulong)(1UL << i);
            _syncLock = new object();
        }

        /// <summary>
        /// Load a bitmask from a byte array
        /// </summary>
        /// <param name="bytes">The source bytes to read from</param>
        public void Load(ulong[] bytes)
        {
            if(bytes.Length != _raw.Length)
                throw new ArgumentOutOfRangeException("mismatch byte sizes");

            lock(_syncLock)
            {
                for(var i = 0; i < bytes.Length; ++i)
                    _raw[i] = bytes[i];
            }
        }

        /// <summary>
        /// Gets the mask length
        /// </summary>
        public int Length { get { return _sz; } }

        /// <summary>
        /// Resizes the bitmask
        /// </summary>
        /// <param name="newSize">The new size in bits to allocate</param>
        public void Resize(int newSize)
        {
            lock (_syncLock)
            {
                _sz = newSize;
                var rem = _sz & MOD_64;
                Array.Resize(ref _raw, (_sz >> 6) + (rem > 0x0 ? 1 : 0));
                for (var i = 0; i < rem; ++i)
                    _raw[_sz >> 6] |= (ulong)(1UL << i);
            }
        }

        /// <summary>
        /// Sets the bit at the specific location
        /// </summary>
        /// <param name="bit">The bit to set</param>
        /// <returns>True if successful</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(int bit)
        {
            if ((bit >> 6) >= _raw.Length)
                return false;

            lock (_syncLock)
            {
                if ((_raw[bit >> 6] & (ulong)(1UL << (63 - (bit & MOD_64)))) > 0x0)
                    return false;
                _raw[bit >> 6] |= (ulong)(1UL << (63 - (bit & MOD_64)));
                return true;
            }
        }

        /// <summary>
        /// Test to see if the bit in the position is set
        /// </summary>
        /// <param name="bit">The bit position to check</param>
        /// <returns>True if the bit is set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int bit)
        {
            if ((bit >> 6) >= _raw.Length)
                return true;

            lock (_syncLock)
                return (_raw[bit >> 6] & (ulong)(1UL << (63 - (bit & MOD_64)))) > 0x0;
        }

        /// <summary>
        /// Clear the bit at the position
        /// </summary>
        /// <param name="bit">The bit to clear</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(int bit)
        {
            if ((bit >> 6) >= _raw.Length)
                return;

            lock (_syncLock)
                _raw[bit >> 6] &= (ulong)~(1UL << (63 - (bit & MOD_64)));
        }

        /// <summary>
        /// Gets/Sets the bit at the specified index
        /// </summary>
        /// <returns>True if the bit is set</returns>
        public bool this[int bit]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsSet(bit);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value)
                    Clear(bit);
                else
                    Set(bit);
            }
        }

        /// <summary>
        /// Get the least significant bit (right to left) that isn't set
        /// </summary>
        /// <returns>The index of an available slot, or -1 if none are available.</returns>
        public int NextAvailableSlot()
        {
            lock (_syncLock)
            {
                // Work from right to left
                for (var n = _raw.Length - 1; n >= 0; --n)
                {
                    // This will store the furthest right bit that isn't set on the long
                    var i = ~_raw[n];

                    // Can't use an index with no available slots
                    if (i == 0)
                        continue;

                    // Check the lower half for an available bit
                    var p = (int)(i & 0xFFFFFFFF);
                    if (p != 0)
                        return (n << 6) + (63 - p.LSB());

                    // Check the upper half for an available bit
                    p = (int)((i >> 32) & 0xFFFFFFFF);
                    if (p != 0)
                        return (n << 6) + (31 - p.LSB());
                }
            }

            // No bits available
            return -1;
        }

        /// <summary>
        /// Translate the bitmask to a string of 1's and 0's
        /// </summary>
        /// <returns>A string representing the binary 1/0 encoding</returns>
        public override string ToString()
        {
            // Print this as a sequence of bits
            return string.Join(" ", _raw.Select(m => Convert.ToString((long)m, 2).PadLeft(64, '0')));
        }

        /// <summary>
        /// Gets the hash code for the bitmask
        /// </summary>
        /// <returns>A hashcode for the object</returns>
        public override int GetHashCode()
        {
            return _raw.GetHashCode();
        }

        /// <summary>
        /// Sets all the bits in the mask
        /// </summary>
        public void SetAll()
        {
            lock(_syncLock)
            {
                var l = _raw.Length;
                for(var i =0; i < l; ++i)
                    _raw[i] = 0xFFFFFFFFFFFFFFFF;
            }
        }

        /// <summary>
        /// Clears all of the bits in the mask
        /// </summary>
        public void ClearAll()
        {
            lock(_syncLock)
            {
                var l = _raw.Length;
                for(var i =0; i < l; ++i)
                    _raw[i] = 0x0;
            }
        }

        /// <summary>
        /// Transform the bitmask to an array
        /// </summary>
        /// <returns>A byte array that represents the bitmask</returns>
        public byte[] ToBytes()
        {
            lock (_syncLock)
            {
                var bytes = new byte[_raw.Length >> 6];
                Buffer.BlockCopy(_raw, 0, bytes, 0, _raw.Length);
                return bytes;
            }
        }
    }
}
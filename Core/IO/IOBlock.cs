using System;
using System.Runtime.CompilerServices;

namespace Telefrek.Core.IO
{
    /// <summary>
    /// Tracks a position in the buffer pool.
    /// </summary>
    internal sealed class IOBlock
    {
        /// <summary>
        /// The index for this block.
        /// </summary>
        public int PoolIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer, int offset, int count, int blockOffset)
        {
            Buffer.BlockCopy(buffer, offset, BlockBufferPool.Instance, PoolIndex + blockOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte[] buffer, int offset, int count, int blockOffset)
        {
            Buffer.BlockCopy(BlockBufferPool.Instance, PoolIndex + blockOffset, buffer, offset, count);
        }
    }
}

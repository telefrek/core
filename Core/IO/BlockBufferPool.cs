using System;
using Telefrek.Core;

namespace Telefrek.Core.IO
{
    /// <summary>
    /// Manages the pool of block buffers used for streaming data.
    /// </summary>
    public static class BlockBufferPool
    {
        static readonly BitMask _mask;
        static readonly object _syncLock = new object();

        /// <summary>
        /// The size of each block
        /// </summary>
        public const int BLOCK_SIZE = 4096;

        /// <summary>
        /// BLOCK_SIZE = 2 ^ SHIFT_SIZE
        /// </summary>
        internal const int SHIFT_SIZE = 12;

        /// <summary>
        /// Internal mask for the SHIFT_SIZE
        /// </summary>
        internal const int BUFFER_MASK = 0xFFF;

        /// <summary>
        /// Gets the size of the pool.
        /// </summary>
        /// <remarks>
        /// Set is disabled until bitmask issue is identified.
        /// </remarks>
        public static long PoolSize
        {
            get
            {
                return Instance.Length >> SHIFT_SIZE;
            }
            set
            {
                if ((value << SHIFT_SIZE) < (Instance.Length) || value > (1 << 20))
                    throw new ArgumentOutOfRangeException("PoolSize:value");
                else if (value == (Instance.Length >> SHIFT_SIZE))
                    return; // already set

                Array.Resize(ref Instance, (int)(value << SHIFT_SIZE));
                _mask.Resize((int)value);
            }
        }

        internal static byte[] Instance;

        static BlockBufferPool()
        {
            Instance = new byte[1 << 24]; // 16 Mb initially
            _mask = new BitMask(1 << (24 - SHIFT_SIZE));
        }

        /// <summary>
        /// Gets the next block in the set.
        /// </summary>
        /// <returns>The next available block.</returns>
        internal static IOBlock GetBlock()
        {
            do
            {
                var ix = _mask.NextAvailableSlot();
                if (_mask.Set(ix))
                    return new IOBlock { PoolIndex = ix << SHIFT_SIZE };
            } while (true);
        }

        /// <summary>
        /// Sends a block back to the pool to be used by other streams.
        /// </summary>
        /// <param name="block">The block to reclaim.</param>
        internal static void ReclaimBlock(IOBlock block)
        {
            _mask.Clear(block.PoolIndex >> SHIFT_SIZE);
        }

        /// <summary>
        /// Gets a set of blocks up to the limit.
        /// </summary>
        /// <param name="limit">The maximum number of blocks to try to retrieve.</param>
        /// <returns>An array of blocks that are available in the current pool.</returns>
        internal static IOBlock[] GetBlocks(int limit)
        {
            var blocks = new IOBlock[limit];
            var n = 0;
            for (var i = 0; i < limit; ++i)
            {
                var ix = _mask.NextAvailableSlot();
                if (_mask.Set(ix))
                    blocks[n++] = new IOBlock { PoolIndex = ix << SHIFT_SIZE };
            }
            Array.Resize(ref blocks, n);
            return blocks;
        }

        /// <summary>
        /// Sends a collection of blocks back to the buffer pool.
        /// </summary>
        /// <param name="blocks">The blocks to return to the pool.</param>
        internal static void ReclaimBlocks(IOBlock[] blocks)
        {
            foreach (var blk in blocks)
                _mask.Clear(blk.PoolIndex >> SHIFT_SIZE);
        }
    }
}

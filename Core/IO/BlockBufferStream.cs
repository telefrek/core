using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Telefrek.Core.IO
{
    /// <summary>
    /// Stream that uses the block buffer pool for reading/writing data.
    /// </summary>
    public sealed class BlockBufferStream : Stream
    {
        List<IOBlock> _blocks;
        long _pos, _len, _size;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BlockBufferStream()
        {
            _blocks = new List<IOBlock>();
            _pos = 0;
            _len = 0;
            _size = 0;
        }

        /// <summary>
        /// Gets if the stream supports reading
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if the stream supports seeking
        /// </summary>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if the stream supports writing
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Flushes the stream
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Gets the stream length
        /// </summary>
        public override long Length
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets/Sets the current stream position
        /// </summary>
        public override long Position
        {
            get
            {
                return _pos;
            }
            set
            {
                if (value > _len || value < 0)
                    throw new ArgumentOutOfRangeException("Invalid position range");
                _pos = value;
            }
        }

        /// <summary>
        /// Reads the next count bytes from the stream into the buffer
        /// </summary>
        /// <param name="buffer">The buffer to read into</param>
        /// <param name="offset">The offset to read</param>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>How many bytes were read, or -1 if no contents remain</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            // Determine the number to read
            var d = (int)(_len - _pos);
            if (d == 0)
                return -1;

            var numRead = count + ((d - count) & ((d - count) >> 31)); // get the min between the current length and the requested count

            var r = 0;
            var ix = (int)(_pos >> 10); // find the index in the buffer
            var i = (int)(_pos & 0x3FF); // mask the remaining bits
            var rem = 1024 + ((numRead - 1024) & ((numRead - 1024) >> 31)); // keep track of the remainder of bits to read
            while (r < numRead)
            {
                _blocks[ix].Read(buffer, r, rem, i);
                r += rem;
                _pos += rem;
                ix++;
                i = 0;
                d = numRead - r;
                rem = 1024 + ((d - 1024) & ((d - 1024) >> 31)); // min
            }

            Array.Resize(ref buffer, numRead);
            return numRead;
        }

        /// <summary>
        /// Seeks to a specific position in the stream
        /// </summary>
        /// <param name="offset">The offset to seek from</param>
        /// <param name="origin">The seek origin</param>
        /// <returns>The new position</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
                default:
                    if (offset + Position > _len)
                        throw new ArgumentOutOfRangeException("Invalid position goes beyond the end of the stream");

                    Position += offset;
                    break;
            }

            return Position;
        }

        /// <summary>
        /// Sets the length of the stream
        /// </summary>
        /// <param name="value">The new length value</param>
        public override void SetLength(long value)
        {
            // Remove unnecessary blocks
            while ((_blocks.Count << BlockBufferPool.SHIFT_SIZE) > value)
            {
                var blk = _blocks[0];
                _blocks.Remove(blk);
                BlockBufferPool.ReclaimBlock(blk);
            }

            // Add necessary blocks
            while ((_blocks.Count << BlockBufferPool.SHIFT_SIZE) < value)
            {
                var blk = BlockBufferPool.GetBlock();
                if (blk != null)
                    _blocks.Add(blk);
            }
            _len = _blocks.Count << BlockBufferPool.SHIFT_SIZE;
            _size = value + ((_size - value) & ((_size - value) >> 63)); // min value between new size and previous size
        }

        /// <summary>
        /// Writes count bytes from the buffer to the stream
        /// </summary>
        /// <param name="buffer">The buffer with the data</param>
        /// <param name="offset">The offset to start reading from</param>
        /// <param name="count">The number of bytes to write</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            var d = buffer.Length - offset;
            var numWrite = count + ((d - count) & ((d - count) >> 31)); // get the min between the remaining bytes in the buffer and the number requested write

            // Ensure the blocks are allocated
            if (numWrite > (_len - _pos))
                SetLength(_pos + numWrite);

            var r = 0;
            var ix = (int)(_pos >> BlockBufferPool.SHIFT_SIZE); // divide by buffer size
            var i = (int)(_pos & BlockBufferPool.BUFFER_MASK); // mod buffersize
            var rem = BlockBufferPool.BLOCK_SIZE + ((numWrite - BlockBufferPool.BLOCK_SIZE) & ((numWrite - BlockBufferPool.BLOCK_SIZE) >> 31)); // min for numwrite and buffer size

            while (r < numWrite)
            {
                _blocks[ix].Write(buffer, offset + r, rem, i);
                r += rem;
                _pos += rem;
                _size += rem;
                ix++;
                i = 0;
                d = numWrite - r;
                rem = 1024 + ((d - 1024) & ((d - 1024) >> 31)); // min for remaining bytes
            }
        }

        /// <summary>
        /// Handle stream disposal
        /// </summary>
        /// <param name="disposing">Indicates if the disposal should be ignored</param>
        protected override void Dispose(bool disposing)
        {
            if (_blocks != null)
            {
                // Undead army FTW!
                BlockBufferPool.ReclaimBlocks(_blocks.ToArray());
                _blocks.Clear();
                _blocks = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the stream as an array of bytes
        /// </summary>
        /// <returns>The stream contents as a byte array</returns>
        public byte[] GetBytes()
        {
            var buf = new byte[_size];
            _pos = 0;
            Read(buf, 0, buf.Length);
            return buf;
        }
    }
}

namespace RJCP.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    [DebuggerDisplay("Offset={Offset}; Length={Data.Length}")]
    internal readonly struct SparseBlock
    {
        public SparseBlock(long offset, byte[] data)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Offset may not be negative");
            if (data == null) throw new ArgumentNullException(nameof(data));
            Offset = offset;
            Data = data;
        }

        public long Offset { get; }

        public byte[] Data { get; }
    }

    /// <summary>
    /// Implements a readonly stream that can define data sparsely in a large space.
    /// </summary>
    internal class SparseStream : Stream
    {
        private readonly List<SparseBlock> m_Data = new List<SparseBlock>();
        private long m_Length;
        private long m_Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseStream"/> class.
        /// </summary>
        /// <remarks>
        /// The stream is empty.
        /// </remarks>
        public SparseStream() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseStream"/> class.
        /// </summary>
        /// <param name="data">The list of predefined data blocks.</param>
        /// <remarks>
        /// The length of the stream is automatically configured to be the length to the last data element.
        /// </remarks>
        public SparseStream(IEnumerable<SparseBlock> data) : this(data, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseStream"/> class.
        /// </summary>
        /// <param name="data">The list of predefined data blocks.</param>
        /// <param name="length">The length of the stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> may not be negative</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="data"/> has blocks that overlap;
        /// <para>- or -</para>
        /// <paramref name="length"/> is less than the largest block.
        /// </exception>
        public SparseStream(IEnumerable<SparseBlock> data, long length)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Length may not be negative");

            foreach (SparseBlock block in data) {
                if (block.Data.LongLength > int.MaxValue) {
                    // THere's no realistic way to test this.
                    string message = string.Format("Block {0} length is too big", m_Data.Count);
                    throw new ArgumentException(message, nameof(data));
                }
                int pos = -1;
                if (m_Data.Count > 0) {
                    for (int i = 0; i < m_Data.Count; i++) {
                        if (block.Offset < m_Data[i].Offset) {
                            if (block.Offset + block.Data.Length > m_Data[i].Offset) {
                                string message = string.Format("Block overlaps. Block {0} offset {1:x} length {2:x} overlaps {3:x}",
                                    m_Data.Count - 1, block.Offset, block.Data.Length,
                                    m_Data[i].Offset);
                                throw new ArgumentException(message, nameof(data));
                            }
                            pos = i;
                            break;
                        }
                    }
                    if (pos != -1) {
                        m_Data.Insert(pos, block);
                    }
                }
                if (pos == -1) m_Data.Add(block);
            }

            if (m_Data.Count != 0) {
                if (length < m_Data[m_Data.Count - 1].Offset + m_Data[m_Data.Count - 1].Data.Length) {
                    if (length == 0) {
                        length = m_Data[m_Data.Count - 1].Offset + m_Data[m_Data.Count - 1].Data.Length;
                    } else {
                        string message = string.Format("Length too short. Length {0} must be at least {1}",
                            length,
                            m_Data[m_Data.Count - 1].Offset + m_Data[m_Data.Count - 1].Data.Length);
                        throw new ArgumentException(message, nameof(length));
                    }
                }
            }
            m_Length = length;
        }

        /// <summary>
        /// Fets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value><see langword="true"/> if this instance can read; otherwise, <see langword="false"/>.</value>
        /// <remarks>The stream is always readable.</remarks>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value><see langword="true"/> if this instance can seek; otherwise, <see langword="false"/>.</value>
        /// <remarks>The stream is always seekable.</remarks>
        public override bool CanSeek { get { return true; } }

        private bool m_CanWrite = true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value><see langword="true"/> if this instance can write; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// By default the stream can write. To prevent this, call the method <see cref="SetReadOnly()"/>.
        /// </remarks>
        public override bool CanWrite
        {
            get { return m_CanWrite; }
        }

        /// <summary>
        /// Sets the stream to be readonly.
        /// </summary>
        public void SetReadOnly()
        {
            m_CanWrite = false;
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <value>The length to the end of the stream.</value>
        public override long Length { get { return m_Length; } }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <value>The position in the stream.</value>
        /// <exception cref="ArgumentException">
        /// The value is set to negative;
        /// <para>- or -</para>
        /// The stream is read only and the position exceeds the length.
        /// </exception>
        /// <remarks>
        /// If the stream is writable, setting the position beyond the <see cref="Length"/> increases the length.
        /// </remarks>
        public override long Position
        {
            get { return m_Position; }
            set
            {
                if (value < 0 || (!m_CanWrite && value > m_Length)) {
                    string message = string.Format("Position must be positive and may not exceed length {0}", m_Length);
                    throw new ArgumentException(message, nameof(value));
                }
                m_Position = value;
                if (m_Position > m_Length) m_Length = m_Position;
            }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <remarks>
        /// Because this is an in-memory data structure, this does nothing.
        /// </remarks>
        public override void Flush() { /* Nothing to do */ }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number
        /// of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified byte array with the values
        /// between <paramref name="offset"/> and ( <paramref name="offset"/> + <paramref name="count"/> - 1) replaced
        /// by the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the
        /// current stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that
        /// many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> may not be negative;
        /// <para>- or -</para>
        /// &gt; <paramref name="count"/> may not be negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="offset"/> and <paramref name="count"/> would exceed the boundaries of the array.
        /// </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "may not be negative");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "may not be negative");
            if (offset > buffer.Length - count) throw new ArgumentException("The offset and count would exceed the boundaries of the array");

            int read = 0;
            int blocknum = 0;
            long position = m_Position;
            while (read < count) {
                if (blocknum < m_Data.Count) {
                    var datablock = m_Data[blocknum];
                    if (position >= datablock.Offset + datablock.Data.Length) {
                        // Nothing to copy from this block. Skip it to the next block.
                        blocknum++;
                    } else if (position < datablock.Offset) {
                        // The position is before the next block. Fill the gaps with zeros.
                        long zerolen = datablock.Offset - position;
                        if (count - read <= zerolen) {
                            // This will completely fill the buffer.
                            for (long i = 0; i < count - read; i++) {
                                buffer[offset + read + i] = 0;
                            }
                            m_Position += count;
                            return count;
                        } else {
                            for (long i = 0; i < zerolen; i++) {
                                buffer[offset + read + i] = 0;
                            }
                            read += (int)zerolen; // We can't overflow, as zerolen <= (int)(count - read).
                            position += zerolen;
                        }
                    } else {
                        // position >= datablock.Key && position < datablock.Key + datablock.Value.Length
                        int blockcount = Math.Min(count - read, (int)(datablock.Offset - position + datablock.Data.Length));
                        Buffer.BlockCopy(
                            datablock.Data, (int)(position - datablock.Offset),
                            buffer, offset + read, blockcount);
                        position += blockcount;
                        read += blockcount;
                    }
                } else {
                    // At the end. Fill in with zeros.
                    int maxcount = (int)Math.Max(0, Math.Min(count - read, m_Length - position));
                    for (long i = 0; i < maxcount; i++) {
                        buffer[read + offset + i] = 0;
                    }
                    read += maxcount;
                    m_Position += read;
                    return read;
                }
            }

            m_Position = position;
            return read;
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Seek <paramref name="offset"/> goes beyond <see cref="Length"/>.
        /// <para>- or -</para>
        /// Seek <paramref name="offset"/> from end goes beyond  <see cref="Length"/>.
        /// <para>- or -</para>
        /// Seek <paramref name="offset"/> from current goes beyond  <see cref="Length"/>.
        /// <para>- or -</para>
        /// Unknown value for <paramref name="origin"/>.
        /// </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin) {
            case SeekOrigin.Begin:
                if (offset < 0 || (!m_CanWrite && offset > m_Length)) {
                    throw new ArgumentOutOfRangeException(nameof(offset), "Seek offset goes beyond length");
                }
                m_Position = offset;
                if (m_Position > m_Length) m_Length = m_Position;
                break;
            case SeekOrigin.End:
                if (offset < 0 || offset > m_Length) {
                    throw new ArgumentOutOfRangeException(nameof(offset), "Seek offset from end goes beyond length");
                }
                m_Position = m_Length - offset;
                break;
            case SeekOrigin.Current:
                if (offset + m_Position < 0 || (!m_CanWrite && (offset + m_Position > m_Length))) {
                    throw new ArgumentOutOfRangeException(nameof(offset), "Seek offset from current goes beyond length");
                }
                m_Position += offset;
                if (m_Position > m_Length) m_Length = m_Position;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), "Unknown value for origin");
            }
            return m_Position;
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="InvalidOperationException">Stream is not writable.</exception>
        public override void SetLength(long value)
        {
            if (!CanWrite) throw new InvalidOperationException();

            m_Length = value;
            if (m_Position > m_Length) m_Position = m_Length;

            // Trim the data list
            for (int i = m_Data.Count - 1; i >= 0; i--) {
                if (m_Data[i].Offset > m_Length) {
                    // Section is no longer needed.
                    m_Data.RemoveAt(i);
                } else if (m_Data[i].Offset + m_Data[i].Data.Length > m_Length) {
                    long length = m_Length - m_Data[i].Offset;
                    // Need to truncate part of this section
                    byte[] newData = new byte[length];
                    Buffer.BlockCopy(m_Data[i].Data, 0, newData, 0, (int)length);
                    m_Data[i] = new SparseBlock(m_Data[i].Offset, newData);
                } else {
                    return;
                }
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the
        /// number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the
        /// current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current
        /// stream.
        /// </param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="InvalidOperationException">The stream is read only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> may not be negative;
        /// <para>- or -</para>
        /// &gt; <paramref name="count"/> may not be negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="offset"/> and <paramref name="count"/> would exceed the boundaries of the array.
        /// </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            long position = DirectWrite(m_Position, buffer, offset, count);
            m_Position = position;
            if (m_Position > m_Length) m_Length = m_Position;
        }

        /// <summary>
        /// Writes directly to the position given. The <see cref="Position"/> is not updated.
        /// </summary>
        /// <param name="position">The position to write to.</param>
        /// <param name="buffer">
        /// An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the
        /// current stream.
        /// </param>
        /// <exception cref="InvalidOperationException">The stream is read only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <returns>The new position.</returns>
        public long DirectWrite(long position, byte[] buffer)
        {
            return DirectWrite(position, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes directly to the position given. The <see cref="Position"/> is not updated.
        /// </summary>
        /// <param name="position">The position to write to.</param>
        /// <param name="buffer">
        /// An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the
        /// current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current
        /// stream.
        /// </param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="InvalidOperationException">The stream is read only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> may not be negative;
        /// <para>- or -</para>
        /// &gt; <paramref name="count"/> may not be negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="offset"/> and <paramref name="count"/> would exceed the boundaries of the array.
        /// </exception>
        /// <returns>The new position.</returns>
        public long DirectWrite(long position, byte[] buffer, int offset, int count)
        {
            if (!CanWrite) throw new InvalidOperationException();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "may not be negative");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "may not be negative");
            if (offset > buffer.Length - count) throw new ArgumentException("The offset and count would exceed the boundaries of the array");

            int write = 0;
            int blocknum = 0;
            while (write < count) {
                if (blocknum < m_Data.Count) {
                    var datablock = m_Data[blocknum];
                    if (position >= datablock.Offset + datablock.Data.Length) {
                        // This block is earlier in the stream than we want to write. Skip it to the next block.
                        blocknum++;
                    } else if (position < datablock.Offset) {
                        // The position is before the next block. Write a new section
                        long len = datablock.Offset - position;
                        if (count - write <= len) {
                            byte[] newData = new byte[count - write];
                            Buffer.BlockCopy(buffer, offset + write, newData, 0, count - write);
                            m_Data.Insert(blocknum, new SparseBlock(position, newData));
                            return position + count - write;
                        } else {
                            // We write partial data to the new block, the next block will have data overwritten. We
                            // could optimise to merge this new block with the next, but for now this is only test code,
                            // and this code is similar to the Read() function call.
                            byte[] newData = new byte[len];
                            Buffer.BlockCopy(buffer, offset + write, newData, 0, (int)len);
                            m_Data.Insert(blocknum, new SparseBlock(position, newData));
                            write += (int)len;
                            position += len;
                        }
                    } else {
                        // position >= datablock.Key && position < datablock.Key + datablock.Value.Length
                        int blockcount = Math.Min(count - write, (int)(datablock.Offset - position + datablock.Data.Length));
                        Buffer.BlockCopy(
                            buffer, offset + write, datablock.Data,
                            (int)(position - datablock.Offset), blockcount);
                        position += blockcount;
                        write += blockcount;
                    }
                } else {
                    // At the end. Write the remaining data.
                    int len = count - write;
                    byte[] newData = new byte[len];
                    Buffer.BlockCopy(buffer, offset + write, newData, 0, len);
                    m_Data.Add(new SparseBlock(position, newData));
                    write += len;
                    position += len;
                    blocknum++;
                }
            }

            return position;
        }
    }
}

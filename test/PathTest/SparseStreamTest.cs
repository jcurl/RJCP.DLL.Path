namespace RJCP.IO
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    internal class SparseStreamTest
    {
        [Test]
        public void InitNullDataBlock()
        {
            Assert.That(() => {
                _ = new SparseStream(null, 0);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void InitNegativeLength()
        {
            SparseBlock[] empty = Array.Empty<SparseBlock>();
            Assert.That(() => {
                _ = new SparseStream(empty, -1);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void InitLargeFile()
        {
            SparseBlock[] large = new SparseBlock[] {
                new SparseBlock(0x10000000_00000000, new byte[] { 0x01, 0x02 })
            };
            Stream s = new SparseStream(large, 0x20000000_00000000);

            Assert.That(s.Length, Is.EqualTo(0x20000000_00000000));
            s.Position = 0x0FFFFFFF_FFFFFFF0;

            byte[] buffer = new byte[100];
            Assert.That(s.Read(buffer, 0, 100), Is.EqualTo(100));
            Assert.That(buffer.Slice(0, 32), Is.EqualTo(new byte[32] {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            }));
        }

        [Test]
        public void InitAutoLength()
        {
            SparseBlock[] large = new SparseBlock[] {
                new SparseBlock(10, new byte[] { 0x01, 0x02 })
            };
            Stream s = new SparseStream(large);

            Assert.That(s.Length, Is.EqualTo(12));
            s.Position = 8;

            byte[] buffer = new byte[100];
            Assert.That(s.Read(buffer, 0, 100), Is.EqualTo(4));
            Assert.That(buffer.Slice(0, 4), Is.EqualTo(new byte[4] { 0x00, 0x00, 0x01, 0x02 }));
        }

        [Test]
        public void InitEmptyWriteSimple()
        {
            SparseBlock[] empty = Array.Empty<SparseBlock>();
            Stream s = new SparseStream(empty, 0);

            Assert.That(s.CanRead, Is.True);
            Assert.That(s.CanSeek, Is.True);
            Assert.That(s.CanWrite, Is.True);   // Writing is by default enabled.
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(0));

            s.Write(new byte[] { 0 }, 0, 1);
            Assert.That(s.Position, Is.EqualTo(1));
            Assert.That(s.Length, Is.EqualTo(1));

            s.SetLength(5);
            Assert.That(s.Position, Is.EqualTo(1));
            Assert.That(s.Length, Is.EqualTo(5));
        }

        [Test]
        public void SetReadOnly()
        {
            SparseBlock[] empty = Array.Empty<SparseBlock>();
            SparseStream s = new SparseStream(empty, 0);

            Assert.That(s.CanRead, Is.True);
            Assert.That(s.CanSeek, Is.True);
            Assert.That(s.CanWrite, Is.True);   // Writing is by default enabled.
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(0));

            s.SetReadOnly();
            Assert.That(s.CanRead, Is.True);
            Assert.That(s.CanSeek, Is.True);
            Assert.That(s.CanWrite, Is.False);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(0));

            Assert.That(() => {
                s.Write(new byte[] { 0 }, 0, 1);
            }, Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => {
                s.SetLength(5);
            }, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ZeroFileReadFull()
        {
            SparseBlock[] empty = Array.Empty<SparseBlock>();
            Stream s = new SparseStream(empty, 10);

            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(10));

            byte[] buffer = new byte[50] {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4
            };
            Assert.That(s.Read(buffer, 0, 10), Is.EqualTo(10));
            Assert.That(buffer.Slice(0, 10), Is.EqualTo(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            Assert.That(s.Position, Is.EqualTo(10));
            Assert.That(s.Length, Is.EqualTo(10));
        }

        [Test]
        public void ZeroFileReadWithOffset()
        {
            SparseBlock[] empty = Array.Empty<SparseBlock>();
            Stream s = new SparseStream(empty, 10);

            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(10));

            byte[] buffer = new byte[50] {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4,
                5, 4, 5, 4, 5, 4, 5, 4, 5, 4
            };
            Assert.That(s.Read(buffer, 5, 5), Is.EqualTo(5));
            Assert.That(buffer.Slice(0, 10), Is.EqualTo(new byte[] { 0, 1, 2, 3, 4, 0, 0, 0, 0, 0 }));
            Assert.That(s.Position, Is.EqualTo(5));
            Assert.That(s.Length, Is.EqualTo(10));
        }

        private static readonly byte[] Elf32Hdr = new byte[] {
            0x7F, 0x45, 0x4C, 0x46, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x8E, 0x55, 0x06, 0x08, 0x34, 0x00, 0x00, 0x00,
            0xC8, 0x49, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x34, 0x00, 0x20, 0x00, 0x09, 0x00, 0x29, 0x00,
            0x1E, 0x00, 0x1D, 0x00
        };

        public static readonly byte[] Elf32PHdr = new byte[] {
            0x06, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x34, 0x80, 0x04, 0x08, 0x34, 0x80, 0x04, 0x08,
            0x20, 0x01, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00
        };

        [Test]
        public void InitInsufficientLength()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };

            Assert.That(() => {
                _ = new SparseStream(elfhdr, 32);
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void InitBlockOutOfOrder()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, 1000);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1000));
        }

        [Test]
        public void InitBlockOutOfOrder2()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(8, Elf32Hdr.Slice(8, 16)),
            };
            Stream s = new SparseStream(elfhdr, 1000);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1000));
        }

        [Test]
        public void InitBlockOverlap()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(12, Elf32Hdr.Slice(12, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Assert.That(() => {
                _ = new SparseStream(elfhdr, 32);
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void InitBlockOverlap2()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(12, Elf32Hdr.Slice(12, 16)),
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
            };
            Assert.That(() => {
                _ = new SparseStream(elfhdr, 32);
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ReadBlocksFromStart([Values(52, 1000)] int length)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, length);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(length));

            byte[] buffer = new byte[52];
            Assert.That(s.Read(buffer, 0, 32), Is.EqualTo(32));
            Assert.That(s.Read(buffer, 32, 16), Is.EqualTo(16));
            Assert.That(s.Read(buffer, 48, 4), Is.EqualTo(4));
            Assert.That(s.Position, Is.EqualTo(52));
            Assert.That(buffer, Is.EqualTo(Elf32Hdr));
        }

        [Test]
        public void ReadBlocksFromStartBeyondZero()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock (0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, 1000);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1000));

            byte[] buffer = new byte[64];
            Assert.That(s.Read(buffer, 0, 32), Is.EqualTo(32));
            Assert.That(s.Read(buffer, 32, 16), Is.EqualTo(16));
            Assert.That(s.Read(buffer, 48, 16), Is.EqualTo(16));
            Assert.That(s.Position, Is.EqualTo(64));
            Assert.That(buffer.Slice(0, 52), Is.EqualTo(Elf32Hdr));
            Assert.That(buffer.Slice(52, 12), Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
        }

        [Test]
        public void ReadBlocksFromStartBeyondEof()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, 52);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(52));

            byte[] buffer = new byte[64];
            Assert.That(s.Read(buffer, 0, 32), Is.EqualTo(32));
            Assert.That(s.Read(buffer, 32, 16), Is.EqualTo(16));
            Assert.That(s.Read(buffer, 48, 16), Is.EqualTo(4));
            Assert.That(s.Position, Is.EqualTo(52));
            Assert.That(buffer.Slice(0, 52), Is.EqualTo(Elf32Hdr));
        }

        [Test]
        public void ReadBlocksFromStartBeyondEof2()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, 55);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(55));     // Expect to read some zeroes at the end.

            byte[] buffer = new byte[64];
            Assert.That(s.Read(buffer, 0, 32), Is.EqualTo(32));
            Assert.That(s.Read(buffer, 32, 16), Is.EqualTo(16));
            Assert.That(s.Read(buffer, 48, 16), Is.EqualTo(7));
            Assert.That(s.Position, Is.EqualTo(55));
            Assert.That(buffer.Slice(0, 52), Is.EqualTo(Elf32Hdr));
            Assert.That(buffer.Slice(52, 3), Is.EqualTo(new byte[] { 0x00, 0x00, 0x00 }));
        }

        [Test]
        public void ReadBlocksFromStartPartial([Values(52, 1000)] int length)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, length);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(length));

            byte[] buffer = new byte[40];
            Assert.That(s.Read(buffer, 0, 40), Is.EqualTo(40));
            Assert.That(s.Position, Is.EqualTo(40));
            Assert.That(buffer, Is.EqualTo(Elf32Hdr.Slice(0, 40)));
        }

        [Test]
        public void ReadBlocksFromOffset([Values(52, 1000)] int length, [Values(0, 10)] int offset)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4))
            };
            Stream s = new SparseStream(elfhdr, length);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(length));

            byte[] buffer = new byte[52];
            s.Position = 10;
            Assert.That(s.Read(buffer, offset, 42), Is.EqualTo(42));
            Assert.That(s.Position, Is.EqualTo(52));
            Assert.That(buffer.Slice(offset, 42), Is.EqualTo(Elf32Hdr.Slice(10, 42)));
        }

        [Test]
        public void ReadBlocksWithGap([Values(92, 1000)] int length)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16)),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16)),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16)),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4)),   // 12 bytes should be zero here
                new SparseBlock(60, Elf32PHdr.Slice(0, 16)),
                new SparseBlock(76, Elf32PHdr.Slice(16, 16))
            };
            Stream s = new SparseStream(elfhdr, length);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(length));

            byte[] buffer = new byte[92];
            Assert.That(s.Read(buffer, 0, 92), Is.EqualTo(92));
            Assert.That(s.Position, Is.EqualTo(92));
            Assert.That(buffer.Slice(0, 52), Is.EqualTo(Elf32Hdr));
            Assert.That(buffer.Slice(52, 8), Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            Assert.That(buffer.Slice(60, 32), Is.EqualTo(Elf32PHdr));
        }

        [Test]
        public void ReadBlocksWithGapReadFromMiddle([Values(92, 1000)] int length, [Values(0, 10)] int offset)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16) ),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16) ),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16) ),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4 )),   // 12 bytes should be zero here
                new SparseBlock(60, Elf32PHdr.Slice(0, 16) ),
                new SparseBlock(76, Elf32PHdr.Slice(16, 16) )
            };
            Stream s = new SparseStream(elfhdr, length);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(length));

            byte[] buffer = new byte[92];
            s.Position = 56;
            Assert.That(s.Read(buffer, offset, 36), Is.EqualTo(36));
            Assert.That(s.Position, Is.EqualTo(92));
            Assert.That(buffer.Slice(offset, 4), Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00 }));
            Assert.That(buffer.Slice(offset + 4, 32), Is.EqualTo(Elf32PHdr));
        }

        [Test]
        public void ReadBlocksWithGapRead([Values(92, 1000)] int length, [Values(0, 10)] int offset, [Values(52, 56)] int pos)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr.Slice(0, 16) ),
                new SparseBlock(16, Elf32Hdr.Slice(16, 16) ),
                new SparseBlock(32, Elf32Hdr.Slice(32, 16) ),
                new SparseBlock(48, Elf32Hdr.Slice(48, 4 )),   // 12 bytes should be zero here
                new SparseBlock(60, Elf32PHdr.Slice(0, 16) ),
                new SparseBlock(76, Elf32PHdr.Slice(16, 16) )
            };
            Stream s = new SparseStream(elfhdr, length);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(length));

            byte[] buffer = new byte[92];
            s.Position = pos;
            Assert.That(s.Read(buffer, offset, 4), Is.EqualTo(4));
            Assert.That(s.Position, Is.EqualTo(pos + 4));
            Assert.That(buffer.Slice(offset, 4), Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00 }));
        }

        [Test]
        public void ReadNull()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            Assert.That(() => {
                _ = s.Read(null, 0, 10);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ReadNegativeOffset()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(() => {
                _ = s.Read(buffer, -1, 10);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ReadNegativeLen()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(() => {
                _ = s.Read(buffer, 0, -10);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(0, 101)]
        [TestCase(10, 100)]
        [TestCase(99, 2)]
        [TestCase(100, 1)]
        public void ReadOutOfBounds(int offset, int length)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(() => {
                _ = s.Read(buffer, offset, length);
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ReadZero()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(s.Read(buffer, 0, 0), Is.EqualTo(0));
        }

        [Test]
        public void ReadAtEndZero()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            s.Position = 100;
            Assert.That(s.Read(buffer, 0, 100), Is.EqualTo(0));
        }

        [Test]
        public void PositionOutOfBounds([Values(-1, 101)] int position)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 100);
            s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            Assert.That(() => {
                s.Position = position;
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void SeekBeginning([Values(0, 10, 50, 900)] int seek)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 1000);
            s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1000));

            s.Seek(seek, SeekOrigin.Begin);
            Assert.That(s.Position, Is.EqualTo(seek));
        }

        [Test]
        public void SeekBeginningOutOfBounds([Values(-1, 101)] int position)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 100);
            s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            s.Position = 50;
            Assert.That(() => {
                s.Seek(position, SeekOrigin.Begin);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(s.Position, Is.EqualTo(50));
        }

        [Test]
        public void SeekBeginningExtend()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            s.Position = 50;
            s.Seek(101, SeekOrigin.Begin);
            Assert.That(s.Position, Is.EqualTo(101));
            Assert.That(s.Length, Is.EqualTo(101));
        }

        [Test]
        public void SeekEnd([Values(0, 10, 50, 900)] int seek)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 1000);
            s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1000));

            s.Seek(seek, SeekOrigin.End);
            Assert.That(s.Position, Is.EqualTo(1000 - seek));
        }

        [Test]
        public void SeekEndOutOfBounds([Values(-1, 101)] int position, [Values(false, true)] bool setreadonly)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 100);
            if (setreadonly) s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            s.Position = 50;
            Assert.That(() => {
                s.Seek(position, SeekOrigin.End);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(s.Position, Is.EqualTo(50));
        }

        [Test]
        public void SeekCurrent()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 1000);
            s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1000));

            s.Seek(10, SeekOrigin.Current);
            Assert.That(s.Position, Is.EqualTo(10));
            s.Seek(10, SeekOrigin.Current);
            Assert.That(s.Position, Is.EqualTo(20));
            s.Seek(900, SeekOrigin.Current);
            Assert.That(s.Position, Is.EqualTo(920));
            s.Seek(80, SeekOrigin.Current);
            Assert.That(s.Position, Is.EqualTo(1000));
        }

        [Test]
        public void SeekCurrentOutOfBounds([Values(-51, 51)] int position)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            SparseStream s = new SparseStream(elfhdr, 100);
            s.SetReadOnly();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            s.Position = 50;
            Assert.That(() => {
                s.Seek(position, SeekOrigin.Current);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(s.Position, Is.EqualTo(50));
        }

        [Test]
        public void SeekCurrentExtend()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            s.Position = 50;
            s.Seek(51, SeekOrigin.Current);
            Assert.That(s.Position, Is.EqualTo(101));
            Assert.That(s.Length, Is.EqualTo(101));
        }

        [Test]
        public void SeekInvalid()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            s.Position = 50;
            Assert.That(() => {
                s.Seek(0, (SeekOrigin)255);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(s.Position, Is.EqualTo(50));
        }

        [Test]
        public void SetLength()
        {
            Stream s = new SparseStream();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(0));

            s.SetLength(50);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(50));
        }

        [Test]
        public void SetPositionExtend()
        {
            Stream s = new SparseStream();
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(0));

            s.Position = 50;
            Assert.That(s.Position, Is.EqualTo(50));
            Assert.That(s.Length, Is.EqualTo(50));
        }

        [Test]
        public void SetLengthShortenOneBlockMiddle([Values(0, 16)] int readOffset)
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            r.NextBytes(data);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(128 + data.Length));

            s.Position = 1024;
            s.SetLength(512);

            Assert.That(s.Position, Is.EqualTo(512));
            Assert.That(s.Length, Is.EqualTo(512));

            s.Position = 0;
            byte[] readBuffer = new byte[1024];
            Assert.That(s.Read(readBuffer, readOffset, readBuffer.Length - readOffset), Is.EqualTo(512));
            Assert.That(readBuffer.Slice(readOffset, 128), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(readOffset + 128, 512 - 128), Is.EqualTo(data.Slice(0, 512 - 128)));
        }

        [Test]
        public void SetLengthShortenOneBlockToEmpty([Values(0, 16)] int readOffset)
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            r.NextBytes(data);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(128 + data.Length));

            s.SetLength(64);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(64));

            byte[] readBuffer = new byte[1024];
            Assert.That(s.Read(readBuffer, readOffset, readBuffer.Length - readOffset), Is.EqualTo(64));
            Assert.That(readBuffer.Slice(readOffset, 64), Has.All.EqualTo(0));
        }

        [Test]
        public void SetLengthShortenOneBlockAtEnd([Values(0, 16)] int readOffset)
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            r.NextBytes(data);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block, 1536);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1536));

            s.SetLength(1280);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1280));

            byte[] readBuffer = new byte[1536];
            Assert.That(s.Read(readBuffer, readOffset, readBuffer.Length - readOffset), Is.EqualTo(1280));
            Assert.That(readBuffer.Slice(readOffset, 128), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(readOffset + 128, 512 - 128), Is.EqualTo(data.Slice(0, 512 - 128)));
        }

        [Test]
        public void WriteBlockAtStart([Values(0, 16)] int readOffset)
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            byte[] newData = new byte[64];
            r.NextBytes(data);
            r.NextBytes(newData);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block, 1536);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1536));

            s.Write(newData, 0, newData.Length);
            Assert.That(s.Position, Is.EqualTo(newData.Length));
            Assert.That(s.Length, Is.EqualTo(1536));

            byte[] readBuffer = new byte[2048];
            s.Position = 0;
            Assert.That(s.Read(readBuffer, readOffset, readBuffer.Length - readOffset), Is.EqualTo(1536));
            Assert.That(readBuffer.Slice(readOffset, 64), Is.EqualTo(newData));
            Assert.That(readBuffer.Slice(readOffset + 64, 64), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(readOffset + 128, 1024), Is.EqualTo(data));
            Assert.That(readBuffer.Slice(readOffset + 1152, 384 - readOffset), Has.All.EqualTo(0));
        }

        [Test]
        public void WriteBlockAtStartFull([Values(0, 16)] int readOffset)
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            byte[] newData = new byte[128];
            r.NextBytes(data);
            r.NextBytes(newData);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block, 1536);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1536));

            s.Write(newData, 0, newData.Length);
            Assert.That(s.Position, Is.EqualTo(newData.Length));
            Assert.That(s.Length, Is.EqualTo(1536));

            byte[] readBuffer = new byte[2048];
            s.Position = 0;
            Assert.That(s.Read(readBuffer, readOffset, readBuffer.Length - readOffset), Is.EqualTo(1536));
            Assert.That(readBuffer.Slice(readOffset, 128), Is.EqualTo(newData));
            Assert.That(readBuffer.Slice(readOffset + 128, 1024), Is.EqualTo(data));
            Assert.That(readBuffer.Slice(readOffset + 1152, 384 - readOffset), Has.All.EqualTo(0));
        }

        [Test]
        public void WriteBlockAtStartOverlap([Values(0, 16)] int readOffset)
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            byte[] newData = new byte[256];
            r.NextBytes(data);
            r.NextBytes(newData);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block, 1536);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1536));

            s.Write(newData, 0, newData.Length);
            Assert.That(s.Position, Is.EqualTo(newData.Length));
            Assert.That(s.Length, Is.EqualTo(1536));

            byte[] readBuffer = new byte[2048];
            s.Position = 0;
            Assert.That(s.Read(readBuffer, readOffset, readBuffer.Length - readOffset), Is.EqualTo(1536));
            Assert.That(readBuffer.Slice(readOffset, 256), Is.EqualTo(newData));
            Assert.That(readBuffer.Slice(readOffset + 256, 1024 - 128), Is.EqualTo(data.Slice(128, 1024 - 128)));
            Assert.That(readBuffer.Slice(readOffset + 1152, 384 - readOffset), Has.All.EqualTo(0));
        }

        [Test]
        public void WriteBlockAtEnd()
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            byte[] newData = new byte[128];
            r.NextBytes(data);
            r.NextBytes(newData);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block, 1536);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1536));

            s.Seek(128, SeekOrigin.End);
            s.Write(newData, 0, newData.Length);
            Assert.That(s.Position, Is.EqualTo(1536));
            Assert.That(s.Length, Is.EqualTo(1536));

            byte[] readBuffer = new byte[2048];
            s.Position = 0;
            Assert.That(s.Read(readBuffer, 0, readBuffer.Length), Is.EqualTo(1536));
            Assert.That(readBuffer.Slice(0, 128), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(128, 1024), Is.EqualTo(data));
            Assert.That(readBuffer.Slice(1152, 256), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(1408, 128), Is.EqualTo(newData));
        }

        [Test]
        public void WriteBlockAtEndAndExtend()
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            byte[] newData = new byte[128];
            r.NextBytes(data);
            r.NextBytes(newData);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block, 1536);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(1536));

            s.Seek(64, SeekOrigin.End);
            s.Write(newData, 0, newData.Length);
            Assert.That(s.Position, Is.EqualTo(1600));
            Assert.That(s.Length, Is.EqualTo(1600));

            byte[] readBuffer = new byte[2048];
            s.Position = 0;
            Assert.That(s.Read(readBuffer, 0, readBuffer.Length), Is.EqualTo(1600));
            Assert.That(readBuffer.Slice(0, 128), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(128, 1024), Is.EqualTo(data));
            Assert.That(readBuffer.Slice(1152, 320), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(1472, 128), Is.EqualTo(newData));
        }

        [Test]
        public void WriteBlockAtEndOverlap()
        {
            Random r = new Random();
            byte[] data = new byte[1024];
            byte[] newData = new byte[128];
            r.NextBytes(data);
            r.NextBytes(newData);

            SparseBlock[] block = new SparseBlock[] {
                new SparseBlock(128, data)
            };
            Stream s = new SparseStream(block);

            s.Seek(64, SeekOrigin.End);
            s.Write(newData, 0, newData.Length);
            Assert.That(s.Position, Is.EqualTo(1216));
            Assert.That(s.Length, Is.EqualTo(1216));

            byte[] readBuffer = new byte[2048];
            s.Position = 0;
            Assert.That(s.Read(readBuffer, 0, readBuffer.Length), Is.EqualTo(1216));
            Assert.That(readBuffer.Slice(0, 128), Has.All.EqualTo(0));
            Assert.That(readBuffer.Slice(128, 960), Is.EqualTo(data.Slice(0, 960)));
            Assert.That(readBuffer.Slice(1088, 128), Is.EqualTo(newData));
        }

        [Test]
        public void WriteNull()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            Assert.That(() => {
                s.Write(null, 0, 10);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void WriteNegativeOffset()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(() => {
                s.Write(buffer, -1, 10);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void WriteNegativeLen()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(() => {
                s.Write(buffer, 0, -10);
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(0, 101)]
        [TestCase(10, 100)]
        [TestCase(99, 2)]
        [TestCase(100, 1)]
        public void WriteOutOfBounds(int offset, int length)
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            Assert.That(() => {
                s.Write(buffer, offset, length);
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void WriteZero()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            s.Write(buffer, 0, 0);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));
        }

        [Test]
        public void WriteAtEndZero()
        {
            SparseBlock[] elfhdr = new SparseBlock[] {
                new SparseBlock(0, Elf32Hdr)
            };
            Stream s = new SparseStream(elfhdr, 100);
            Assert.That(s.Position, Is.EqualTo(0));
            Assert.That(s.Length, Is.EqualTo(100));

            byte[] buffer = new byte[100];
            s.Position = 100;
            s.Write(buffer, 0, 0);
            Assert.That(s.Position, Is.EqualTo(100));
            Assert.That(s.Length, Is.EqualTo(100));
        }
    }
}

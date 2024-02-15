namespace RJCP.IO
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class BinaryReaderExtensions
    {
        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            return ReadStruct<T>(reader, Marshal.SizeOf(typeof(T)));
        }

#if NETFRAMEWORK
        public static T ReadStruct<T>(this BinaryReader reader, int minLen) where T : struct
        {
            // Note, this code won't work on a machine where the file endianness differs from the host endianness.
            byte[] buffer = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            if (buffer.Length < minLen)
                throw new EndOfStreamException("Couldn't read content of file");

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try {
                T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                return result;
            } finally {
                handle.Free();
            }
        }
#else
        public unsafe static T ReadStruct<T>(this BinaryReader reader, int minLen) where T : struct
        {
            // Note, this code won't work on a machine where the file endianness differs from the host endianness.
            byte* buffer = stackalloc byte[Marshal.SizeOf<T>()];
            Span<byte> sBuff = new(buffer, Marshal.SizeOf<T>());

            bool readComplete = false;
            int readTotal = 0;
            do {
                int read = reader.Read(sBuff[readTotal..]);
                if (read == 0) {
                    if (readTotal >= minLen) {
                        readComplete = true;
                    } else {
                        throw new EndOfStreamException("Couldn't read content of file");
                    }
                }
                readTotal += read;
            } while (readTotal < sBuff.Length && !readComplete);
            return (T)Marshal.PtrToStructure(new IntPtr(buffer), typeof(T));
        }
#endif

        public static short PeekInt16(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            try {
                return reader.ReadInt16();
            } finally {
                reader.BaseStream.Position = pos;
            }
        }

        public static ushort PeekUInt16(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            try {
                return reader.ReadUInt16();
            } finally {
                reader.BaseStream.Position = pos;
            }
        }

        public static int PeekInt32(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            try {
                return reader.ReadInt32();
            } finally {
                reader.BaseStream.Position = pos;
            }
        }

        public static uint PeekUInt32(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            try {
                return reader.ReadUInt32();
            } finally {
                reader.BaseStream.Position = pos;
            }
        }

        public static long PeekInt64(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            try {
                return reader.ReadInt64();
            } finally {
                reader.BaseStream.Position = pos;
            }
        }

        public static ulong PeekUInt64(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            try {
                return reader.ReadUInt64();
            } finally {
                reader.BaseStream.Position = pos;
            }
        }
    }
}

namespace RJCP.IO
{
    using System.Linq;

    internal static class ArrayBufferExtensions
    {
        public static T[] Slice<T>(this T[] array, int offset, int length)
        {
#if NET6_0_OR_GREATER
            return array[offset..(offset + length)];
#else
            return array.Skip(offset).Take(length).ToArray();
#endif
        }
    }
}

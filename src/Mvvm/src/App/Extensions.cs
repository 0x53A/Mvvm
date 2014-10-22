#if UNIVERSAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Mvvm.App
{
    public static class Extensions
    {
        public static byte[] ToBytes(this IBuffer buffer)
        {
            var reader = DataReader.FromBuffer(buffer);
            var bytes = new byte[buffer.Length];
            reader.ReadBytes(bytes);
            return bytes;
        }

        public static IBuffer ToIBuffer(this byte[] bytes)
        {
            DataWriter writer = new DataWriter();
            writer.WriteBytes(bytes);
            return writer.DetachBuffer();
        }
    }
}
#endif
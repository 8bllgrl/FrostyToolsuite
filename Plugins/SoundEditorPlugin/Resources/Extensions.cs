using System;
using FrostySdk.IO;

namespace SoundEditorPlugin.Resources
{

    internal static class Extensions
    {

        public static byte[] ReadBytes(this NativeReader reader, int size, Endian endian = 0)
        {
            byte[] array = reader.ReadBytes(size);
            bool flag = endian == 0;
            if (flag)
            {
                Array.Reverse(array);
            }
            return array;
        }
    }
}
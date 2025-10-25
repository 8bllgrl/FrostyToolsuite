using System;
using System.Collections.Generic;
using System.IO;
using FrostySdk.IO;

namespace SoundEditorPlugin
{

    public static class Pcm16b
    {

        public static short[] Decode(byte[] soundBuffer)
        {
            short[] array3;
            using (NativeReader nativeReader = new NativeReader(new MemoryStream(soundBuffer)))
            {
                ushort num = nativeReader.ReadUShort(0);
                ushort num2 = nativeReader.ReadUShort(1);
                byte b = nativeReader.ReadByte();
                int num3 = (nativeReader.ReadByte() >> 2) + 1;
                ushort num4 = nativeReader.ReadUShort(1);
                int num5 = nativeReader.ReadInt(1) & 16777215;
                List<short>[] array = new List<short>[num3];
                for (int i = 0; i < num3; i++)
                {
                    array[i] = new List<short>();
                }
                while (nativeReader.Position <= nativeReader.Length)
                {
                    num = nativeReader.ReadUShort(0);
                    num2 = nativeReader.ReadUShort(1);
                    bool flag = num == 69;
                    if (flag)
                    {
                        break;
                    }
                    uint num6 = nativeReader.ReadUInt(1);
                    int num7 = 0;
                    while ((long)num7 < (long)((ulong)num6))
                    {
                        for (int j = 0; j < num3; j++)
                        {
                            array[j].Add(nativeReader.ReadShort(1));
                        }
                        num7++;
                    }
                }
                short[] array2 = new short[array[0].Count * num3];
                for (int k = 0; k < array[0].Count; k++)
                {
                    for (int l = 0; l < num3; l++)
                    {
                        array2[k * num3 + l] = array[l][k];
                    }
                }
                array3 = array2;
            }
            return array3;
        }
    }
}
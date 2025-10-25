using System;
using System.Collections.Generic;
using System.IO;
using FrostySdk.IO;

namespace SoundEditorPlugin
{

    public static class XAS
    {

        public static short[] Decode(byte[] soundBuffer)
        {
            short[] array7;
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
                    short[] array2 = new short[32];
                    int[] array3 = new int[] { 0, 240, 460, 392 };
                    int[] array4 = new int[] { 0, 0, -208, -220 };
                    for (int j = 0; j < (int)(num2 / 76) / num3; j++)
                    {
                        for (int k = 0; k < num3; k++)
                        {
                            byte[] array5 = nativeReader.ReadBytes(76);
                            for (int l = 0; l < 4; l++)
                            {
                                array2[0] = (short)((int)(array5[l * 4] & 240) | ((int)array5[l * 4 + 1] << 8));
                                array2[1] = (short)((int)(array5[l * 4 + 2] & 240) | ((int)array5[l * 4 + 3] << 8));
                                int num7 = (int)(array5[l * 4] & 15);
                                int num8 = (int)(array5[l * 4 + 2] & 15);
                                for (int m = 2; m < 32; m += 2)
                                {
                                    int num9 = (array5[12 + l + m * 2] & 240) >> 4;
                                    bool flag2 = num9 > 7;
                                    if (flag2)
                                    {
                                        num9 -= 16;
                                    }
                                    int num10 = (int)array2[m - 1] * array3[num7] + (int)array2[m - 2] * array4[num7];
                                    array2[m] = (short)(num10 + (num9 << 20 - num8) + 128 >> 8);
                                    bool flag3 = array2[m] > short.MaxValue;
                                    if (flag3)
                                    {
                                        array2[m] = short.MaxValue;
                                    }
                                    else
                                    {
                                        bool flag4 = array2[m] < short.MinValue;
                                        if (flag4)
                                        {
                                            array2[m] = short.MinValue;
                                        }
                                    }
                                    int num11 = (int)(array5[12 + l + m * 2] & 15);
                                    bool flag5 = num11 > 7;
                                    if (flag5)
                                    {
                                        num11 -= 16;
                                    }
                                    int num12 = (int)array2[m] * array3[num7] + (int)array2[m - 1] * array4[num7];
                                    array2[m + 1] = (short)(num12 + (num11 << 20 - num8) + 128 >> 8);
                                    bool flag6 = array2[m + 1] > short.MaxValue;
                                    if (flag6)
                                    {
                                        array2[m + 1] = short.MaxValue;
                                    }
                                    else
                                    {
                                        bool flag7 = array2[m + 1] < short.MinValue;
                                        if (flag7)
                                        {
                                            array2[m + 1] = short.MinValue;
                                        }
                                    }
                                }
                                array[k].AddRange(array2);
                            }
                            uint num13 = ((num6 < 128U) ? num6 : 128U);
                            num6 -= num13;
                        }
                    }
                }
                short[] array6 = new short[array[0].Count * num3];
                for (int n = 0; n < array[0].Count; n++)
                {
                    for (int num14 = 0; num14 < num3; num14++)
                    {
                        array6[n * num3 + num14] = array[num14][n];
                    }
                }
                array7 = array6;
            }
            return array7;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using FrostySdk.IO; 

namespace SoundEditorPlugin.Playback
{
    public static class XAS
    {
        public static short[] Decode(byte[] soundBuffer)
        {
            short[] array7;
            using (NativeReader nativeReader = new NativeReader(new MemoryStream(soundBuffer)))
            {
                // Corrected: Replaced '0' with Endian.Little
                nativeReader.ReadUShort(Endian.Little);

                // Corrected: Replaced '1' with Endian.Big
                ushort num = nativeReader.ReadUShort(Endian.Big); // blockSize (initial)

                nativeReader.ReadByte();

                int num2 = (nativeReader.ReadByte() >> 2) + 1; // Channel Count

                // Corrected: Replaced '1' with Endian.Big
                nativeReader.ReadUShort(Endian.Big); // Sample Rate (ignored)

                // Corrected: Replaced '1' with Endian.Big
                nativeReader.ReadInt(Endian.Big); // Total Sample Count (ignored)

                List<short>[] array = new List<short>[num2];
                for (int i = 0; i < num2; i++)
                {
                    array[i] = new List<short>();
                }

                while (nativeReader.Position <= nativeReader.Length)
                {
                    // Corrected: Replaced '0' with Endian.Little
                    int num3 = (int)nativeReader.ReadUShort(Endian.Little); // blockType

                    // Corrected: Replaced '1' with Endian.Big
                    num = nativeReader.ReadUShort(Endian.Big); // blockSize

                    if (num3 == 69) // 0x45 (End marker)
                    {
                        break;
                    }

                    // Corrected: Replaced '1' with Endian.Big
                    uint num4 = nativeReader.ReadUInt(Endian.Big); // samples in block

                    short[] array2 = new short[32]; // Decompression buffer
                    int[] array3 = new int[] { 0, 240, 460, 392 }; // Predictor constants 1
                    int[] array4 = new int[] { 0, 0, -208, -220 }; // Predictor constants 2

                    for (int j = 0; j < (int)(num / 76) / num2; j++)
                    {
                        for (int k = 0; k < num2; k++) // Channel Loop
                        {
                            byte[] array5 = nativeReader.ReadBytes(76); // Read 76-byte ADPCM block

                            for (int l = 0; l < 4; l++) // 4 segments per block
                            {
                                // Initialize previous samples and get indices
                                array2[0] = (short)((int)(array5[l * 4] & 240) | ((int)array5[l * 4 + 1] << 8));
                                array2[1] = (short)((int)(array5[l * 4 + 2] & 240) | ((int)array5[l * 4 + 3] << 8));

                                int num5 = (int)(array5[l * 4] & 15);     // Predictor index
                                int num6 = (int)(array5[l * 4 + 2] & 15); // Scale factor index

                                for (int m = 2; m < 32; m += 2) // Decompress 30 samples (15 pairs)
                                {
                                    // First sample (even index m)
                                    int num7 = (array5[12 + l + m * 2] & 240) >> 4;
                                    if (num7 > 7) { num7 -= 16; }

                                    int num8 = (int)array2[m - 1] * array3[num5] + (int)array2[m - 2] * array4[num5];
                                    array2[m] = (short)(num8 + (num7 << 20 - num6) + 128 >> 8);

                                    if (array2[m] > short.MaxValue) { array2[m] = short.MaxValue; }
                                    else if (array2[m] < short.MinValue) { array2[m] = short.MinValue; }

                                    // Second sample (odd index m+1)
                                    int num9 = (int)(array5[12 + l + m * 2] & 15);
                                    if (num9 > 7) { num9 -= 16; }

                                    int num10 = (int)array2[m] * array3[num5] + (int)array2[m - 1] * array4[num5];
                                    array2[m + 1] = (short)(num10 + (num9 << 20 - num6) + 128 >> 8);

                                    if (array2[m + 1] > short.MaxValue) { array2[m + 1] = short.MaxValue; }
                                    else if (array2[m + 1] < short.MinValue) { array2[m + 1] = short.MinValue; }
                                }

                                array[k].AddRange(array2);
                            }

                            uint num11 = ((num4 < 128U) ? num4 : 128U);
                            num4 -= num11;
                        }
                    }
                }

                // Interleave the channel data
                short[] array6 = new short[array[0].Count * num2];
                for (int n = 0; n < array[0].Count; n++)
                {
                    for (int num12 = 0; num12 < num2; num12++)
                    {
                        array6[n * num2 + num12] = array[num12][n];
                    }
                }

                array7 = array6;
            }
            return array7;
        }
    }
}
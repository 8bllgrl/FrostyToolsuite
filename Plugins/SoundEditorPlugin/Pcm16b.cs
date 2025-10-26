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
            using (NativeReader reader = new NativeReader(new MemoryStream(soundBuffer)))
            {
                // Reading blockType with Little Endian
                ushort blockType = reader.ReadUShort(Endian.Little);

                ushort blockSize = reader.ReadUShort(Endian.Big);
                byte compressionType = reader.ReadByte();

                int channelCount = (reader.ReadByte() >> 2) + 1;
                ushort sampleRate = reader.ReadUShort(Endian.Big);
                int totalSampleCount = reader.ReadInt(Endian.Big) & 0x00ffffff;

                List<short>[] channels = new List<short>[channelCount];
                for (int i = 0; i < channelCount; i++)
                    channels[i] = new List<short>();

                while (reader.Position <= reader.Length)
                {
                    // Reading blockType with Little Endian
                    blockType = reader.ReadUShort(Endian.Little);

                    blockSize = reader.ReadUShort(Endian.Big);

                    if (blockType == 0x45)
                        break;

                    uint samples = reader.ReadUInt(Endian.Big);

                    for (int i = 0; i < samples; i++)
                    {
                        for (int j = 0; j < channelCount; j++)
                            channels[j].Add(reader.ReadShort(Endian.Big));
                    }
                }

                short[] outBuffer = new short[channels[0].Count * channelCount];
                for (int i = 0; i < channels[0].Count; i++)
                {
                    for (int j = 0; j < channelCount; j++)
                    {
                        outBuffer[(i * channelCount) + j] = channels[j][i];
                    }
                }

                return outBuffer;
            }
        }
    }
}

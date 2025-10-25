using System;
using System.IO;

namespace SoundEditorPlugin.WAV
{

    public class WAV8BitDataFrame : WAVDataFrame
    {

        public override ushort BitDepth
        {
            get
            {
                return 8;
            }
        }

        public WAV8BitDataFrame(ushort channelCount)
            : base(channelCount)
        {
            this.Data = new byte[(int)channelCount];
        }

        public override object GetSample(ushort channel)
        {
            return this.Data[(int)channel];
        }

        public override void Read(BinaryReader reader)
        {
            for (int i = 0; i < (int)base.ChannelCount; i++)
            {
                this.Data[i] = reader.ReadByte();
            }
        }

        public override void Write(BinaryWriter writer)
        {
            for (int i = 0; i < (int)base.ChannelCount; i++)
            {
                writer.Write(this.Data[i]);
            }
        }

        public byte[] Data;
    }
}
using System;
using System.IO;

namespace SoundEditorPlugin.WAV
{

    public abstract class WAVDataFrame
    {

        public abstract ushort BitDepth { get; }

        public ushort ChannelCount { get; private set; }

        public WAVDataFrame(ushort channelCount)
        {
            this.ChannelCount = channelCount;
        }

        public abstract void Read(BinaryReader reader);

        public abstract void Write(BinaryWriter writer);

        public abstract object GetSample(ushort channel);
    }
}
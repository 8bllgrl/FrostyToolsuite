using System;
using System.Collections.Generic;
using System.IO;

namespace SoundEditorPlugin.WAV
{

    public class WAVDataChunk : IRIFFChunk
    {

        public RIFFChunkHeader Header { get; set; }

        public WAVDataChunk(WAVFormatChunk format, List<WAVDataFrame> frames)
        {
            this.Format = format;
            this.Frames = frames;
            this.Header = new RIFFChunkHeader(0L, new byte[] { 100, 97, 116, 97 }, (uint)(this.Frames.Count * (int)this.Format.BitDepth / 8 * (int)this.Format.ChannelCount));
        }

        public WAVDataChunk(RIFFChunkHeader header, WAVFormatChunk format, BinaryReader reader)
        {
            this.Header = header;
            this.Format = format;
            uint num = (uint)((ulong)(this.Header.PayloadSize / (uint)this.Format.ChannelCount) / (ulong)((long)(this.Format.BitDepth / 8)));
            reader.BaseStream.Position = this.Header.StartOffset + 4L + 4L;
            this.Frames = new List<WAVDataFrame>();
            for (uint num2 = 0U; num2 < num; num2 += 1U)
            {
                bool flag = this.Format.Format == WAVFormatChunk.DataFormats.WAVE_FORMAT_PCM;
                if (!flag)
                {
                    throw new Exception("Cannot read non-PCM data. This data is format " + this.Format.Format.ToString());
                }
                bool flag2 = this.Format.BitDepth == 8;
                WAVDataFrame wavdataFrame;
                if (flag2)
                {
                    wavdataFrame = new WAV8BitDataFrame(this.Format.ChannelCount);
                }
                else
                {
                    bool flag3 = this.Format.BitDepth == 16;
                    if (flag3)
                    {
                        wavdataFrame = new WAV16BitDataFrame(this.Format.ChannelCount);
                    }
                    else
                    {
                        bool flag4 = this.Format.BitDepth == 32;
                        if (!flag4)
                        {
                            throw new Exception("Invalid bit depth: " + this.Format.BitDepth.ToString());
                        }
                        wavdataFrame = new WAV32BitDataFrame(this.Format.ChannelCount);
                    }
                }
                wavdataFrame.Read(reader);
                this.Frames.Add(wavdataFrame);
            }
        }

        public void Write(BinaryWriter writer)
        {
            this.Header.Write(writer);
            foreach (WAVDataFrame wavdataFrame in this.Frames)
            {
                wavdataFrame.Write(writer);
            }
        }

        public WAVFormatChunk Format;

        public List<WAVDataFrame> Frames;
    }
}
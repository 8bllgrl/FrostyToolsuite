using System;
using System.IO;
using System.Text;

namespace SoundEditorPlugin.WAV
{

    public class RIFFChunkHeader
    {

        public string ChunkIDName
        {
            get
            {
                bool flag = this.ChunkID == null;
                string text;
                if (flag)
                {
                    text = "";
                }
                else
                {
                    text = Encoding.ASCII.GetString(this.ChunkID);
                }
                return text;
            }
        }

        public RIFFChunkHeader(long startOffset, byte[] chunkID, uint payloadSize)
        {
            this.StartOffset = startOffset;
            this.ChunkID = chunkID;
            this.PayloadSize = payloadSize;
        }

        public RIFFChunkHeader(BinaryReader reader)
        {
            this.StartOffset = reader.BaseStream.Position;
            this.ChunkID = reader.ReadBytes(4);
            this.PayloadSize = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.ChunkID);
            writer.Write(this.PayloadSize);
        }

        public override string ToString()
        {
            return this.ChunkIDName + " : " + this.PayloadSize.ToString() + " bytes";
        }

        public long StartOffset;

        public byte[] ChunkID;

        public uint PayloadSize;
    }
}
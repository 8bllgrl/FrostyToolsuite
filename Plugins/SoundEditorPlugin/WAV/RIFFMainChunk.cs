using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoundEditorPlugin.WAV
{

    public class RIFFMainChunk
    {

        public string RIFFTypeName
        {
            get
            {
                return Encoding.ASCII.GetString(this.RIFFType);
            }
        }

        public RIFFMainChunk(RIFFChunkHeader header, byte[] riffType)
        {
            this.Header = header;
            this.RIFFType = riffType;
        }

        public RIFFMainChunk(BinaryReader reader)
        {
            this.Header = new RIFFChunkHeader(reader);
            this.RIFFType = reader.ReadBytes(4);
            while (reader.BaseStream.Position < this.Header.StartOffset + 4L + 4L + (long)((ulong)this.Header.PayloadSize))
            {
                RIFFChunkHeader riffchunkHeader = new RIFFChunkHeader(reader);
                this.ChunkHeaders.Add(riffchunkHeader);
                reader.BaseStream.Seek((long)((ulong)riffchunkHeader.PayloadSize), SeekOrigin.Current);
            }
        }

        public void Write(BinaryWriter writer, List<IRIFFChunk> chunks)
        {
            uint num = 4U;
            foreach (IRIFFChunk iriffchunk in chunks)
            {
                num += iriffchunk.Header.PayloadSize + 4U + 4U;
            }
            this.Header.PayloadSize = num;
            this.Header.Write(writer);
            writer.Write(this.RIFFType);
            foreach (IRIFFChunk iriffchunk2 in chunks)
            {
                iriffchunk2.Write(writer);
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                this.RIFFTypeName,
                " : ",
                this.ChunkHeaders.Count.ToString(),
                " chunks : {",
                string.Join<RIFFChunkHeader>(", ", this.ChunkHeaders),
                "}"
            });
        }

        public RIFFChunkHeader Header;

        public byte[] RIFFType;

        public List<RIFFChunkHeader> ChunkHeaders = new List<RIFFChunkHeader>();
    }
}
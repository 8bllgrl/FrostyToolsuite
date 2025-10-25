using System;
using System.IO;

namespace SoundEditorPlugin.WAV
{

    public interface IRIFFChunk
    {

        RIFFChunkHeader Header { get; set; }

        void Write(BinaryWriter writer);
    }
}
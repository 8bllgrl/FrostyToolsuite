using FrostySdk.Attributes;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{
    [EbxClassMeta()]
    public class Chunk
    {
        [EbxFieldMeta(EbxFieldType.Struct)]
        public Guid ChunkId { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint ChunkSize { get; set; }
    }
}
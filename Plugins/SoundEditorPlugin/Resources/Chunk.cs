using FrostySdk.Attributes;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{
    [EbxClassMeta(EbxFieldType.Struct)]
    public class Chunk
    {
        [EbxFieldMeta(EbxFieldType.Guid)]
        public Guid ChunkId { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint ChunkSize { get; set; }
    }
}
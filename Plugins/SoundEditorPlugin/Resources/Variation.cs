using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{
    [EbxClassMeta(EbxFieldType.Struct)]
    public class Variation
    {
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint VariationId { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint FirstSubtitleIndex { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint SubtitleCount { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint MemoryChunkIndex { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint StreamChunkIndex { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint FirstSegmentIndex { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint SegmentCount { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint FirstLoopSegmentIndex { get; set; }

        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint LastLoopSegmentIndex { get; set; }
    }
}

using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta(EbxFieldType.Struct)]
    public class Segment
    {
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint SamplesOffset { get; set; }
        [EbxFieldMeta(EbxFieldType.Int32)]
        public uint SamplesOffsetFlag { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint SeekTableOffset { get; set; }
        [EbxFieldMeta(EbxFieldType.Int32)]
        public uint SeekTableFlag { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public float SegmentLength { get; set; }
    }
}
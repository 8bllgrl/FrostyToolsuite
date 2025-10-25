using System;
using FrostySdk.Attributes;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta()]
    public class Segment
    {

        [EbxFieldMeta()]
        public uint SamplesOffset { get; set; }

        [EbxFieldMeta()]
        public uint SamplesOffsetFlag { get; set; }

        [EbxFieldMeta()]
        public uint SeekTableOffset { get; set; }

        [EbxFieldMeta()]
        public uint SeekTableFlag { get; set; }

        [EbxFieldMeta()]
        public float SegmentLength { get; set; }
    }
}
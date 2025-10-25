using FrostySdk.Attributes;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta(EbxFieldType.Struct)]
    public class Selection
    {
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint VariationId { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint VariationIndex { get; set; }
        [DisplayName("80268F2E")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public int unkown { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public int PreDelay { get; set; }
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool IsDay { get; set; }
    }
}
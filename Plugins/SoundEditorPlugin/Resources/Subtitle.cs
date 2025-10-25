using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{
    [EbxClassMeta(EbxFieldType.Struct)]
    public class Subtitle
    {
        [EbxFieldMeta(EbxFieldType.CString)]
        public CString StringId { get; set; }

        [EbxFieldMeta(EbxFieldType.Float32)]
        public float Time { get; set; }

        [EbxFieldMeta(EbxFieldType.Int32)]
        public int AdditionalSubtitleInfoType { get; set; }
    }
}

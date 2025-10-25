using FrostySdk.Attributes;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta(EbxFieldType.Struct)]
    public class SelectionParameter
    {
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint ParameterId { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint ParameterIndex { get; set; }
    }
}
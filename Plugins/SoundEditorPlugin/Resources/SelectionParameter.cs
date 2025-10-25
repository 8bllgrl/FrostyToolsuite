using System;
using FrostySdk.Attributes;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta()]
    public class SelectionParameter
    {

        [EbxFieldMeta()]
        public uint ParameterId { get; set; }

        [EbxFieldMeta()]
        public uint ParameterIndex { get; set; }
    }
}
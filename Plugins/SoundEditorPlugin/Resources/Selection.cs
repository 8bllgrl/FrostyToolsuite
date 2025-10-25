using System;
using FrostySdk.Attributes;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta()]
    public class Selection
    {

        [EbxFieldMeta()]
        public uint VariationId { get; set; }

        [EbxFieldMeta()]
        public uint VariationIndex { get; set; }

        [DisplayName("80268F2E")]
        [EbxFieldMeta()]
        public int unkown { get; set; }

        [EbxFieldMeta()]
        public int PreDelay { get; set; }

        [EbxFieldMeta()]
        public bool IsDay { get; set; }
    }
}
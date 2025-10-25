using System;
using FrostySdk.Attributes;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta()]
    public class Persistence
    {

        [EbxFieldMeta()]
        public uint SelectionParameterCount { get; set; }

        [EbxFieldMeta()]
        public uint RequiredVariationCount { get; set; }

        [EbxFieldMeta()]
        public uint DesiredVariationCount { get; set; }

        [EbxFieldMeta()]
        public uint Speed { get; set; }

        [DisplayName("2C40720C")]
        [EbxFieldMeta()]
        public uint unknown00 { get; set; }

        [DisplayName("B355F9DD")]
        [EbxFieldMeta()]
        public uint unknown01 { get; set; }

        [DisplayName("E230EDBE")]
        [EbxFieldMeta()]
        public uint unknown02 { get; set; }

        [DisplayName("20AFEB30")]
        [EbxFieldMeta()]
        public uint unknown03 { get; set; }

        [DisplayName("804D52B0")]
        [EbxFieldMeta()]
        public uint unknown04 { get; set; }

        [EbxFieldMeta()]
        public uint WaterDepth { get; set; }

        [EbxFieldMeta()]
        public uint Material { get; set; }

        [DisplayName("1FFFFB8F")]
        [EbxFieldMeta()]
        public uint unknown07 { get; set; }

        [DisplayName("E6CC1A67")]
        [EbxFieldMeta()]
        public uint unknown08 { get; set; }

        [DisplayName("E8A87023")]
        [EbxFieldMeta()]
        public uint unknown09 { get; set; }

        [DisplayName("FBB4809F")]
        [EbxFieldMeta()]
        public uint unknown10 { get; set; }

        [DisplayName("7CCE9E8F")]
        [EbxFieldMeta()]
        public uint unknown11 { get; set; }

        [DisplayName("8F3AEF43")]
        [EbxFieldMeta()]
        public uint unknown12 { get; set; }

        [DisplayName("F6595120")]
        [EbxFieldMeta()]
        public uint unknown13 { get; set; }

        [DisplayName("1B45F0F4")]
        [EbxFieldMeta()]
        public uint unknown14 { get; set; }

        [DisplayName("37301D85")]
        [EbxFieldMeta()]
        public uint unknown15 { get; set; }

        [DisplayName("7D4505A4")]
        [EbxFieldMeta()]
        public uint unknown16 { get; set; }

        [DisplayName("C092D707")]
        [EbxFieldMeta()]
        public uint unknown17 { get; set; }

        [DisplayName("E49FA414")]
        [EbxFieldMeta()]
        public uint unknown18 { get; set; }
    }
}
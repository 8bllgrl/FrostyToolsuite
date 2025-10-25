using FrostySdk.Attributes;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin.Resources
{

    [EbxClassMeta(EbxFieldType.Struct)]
    public class Persistence
    {
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint SelectionParameterCount { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint RequiredVariationCount { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint DesiredVariationCount { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint Speed { get; set; }
        [DisplayName("2C40720C")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown00 { get; set; }
        [DisplayName("B355F9DD")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown01 { get; set; }
        [DisplayName("E230EDBE")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown02 { get; set; }
        [DisplayName("20AFEB30")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown03 { get; set; }
        [DisplayName("804D52B0")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown04 { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint WaterDepth { get; set; }
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint Material { get; set; }
        [DisplayName("1FFFFB8F")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown07 { get; set; }
        [DisplayName("E6CC1A67")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown08 { get; set; }
        [DisplayName("E8A87023")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown09 { get; set; }
        [DisplayName("FBB4809F")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown10 { get; set; }
        [DisplayName("7CCE9E8F")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown11 { get; set; }
        [DisplayName("8F3AEF43")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown12 { get; set; }
        [DisplayName("F6595120")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown13 { get; set; }
        [DisplayName("1B45F0F4")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown14 { get; set; }
        [DisplayName("37301D85")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown15 { get; set; }
        [DisplayName("7D4505A4")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown16 { get; set; }
        [DisplayName("C092D707")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown17 { get; set; }
        [DisplayName("E49FA414")]
        [EbxFieldMeta(EbxFieldType.UInt32)]
        public uint unknown18 { get; set; }
    }
}
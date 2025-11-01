using FrostySdk.Attributes;
using FrostySdk.IO;

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
        [EbxFieldMeta(EbxFieldType.Int32)]
        public int unkown { get; set; }
        
        [EbxFieldMeta(EbxFieldType.Int32)]
        public int PreDelay { get; set; }
        
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool IsDay { get; set; }

        // Added missing properties from decompiled data structure (used in NewWaveResource parsing)
        [DisplayName("7C7F1464")]
        [EbxFieldMeta(EbxFieldType.Float32)]
        public float unk7C7F1464 { get; set; }

        [DisplayName("0B87C53A")]
        [EbxFieldMeta(EbxFieldType.Float32)]
        public float unk0B87C53A { get; set; }

        [DisplayName("0B87C535")]
        [EbxFieldMeta(EbxFieldType.Float32)]
        public float unk0B87C535 { get; set; }

        [DisplayName("89A17723")]
        [EbxFieldMeta(EbxFieldType.Float32)]
        public float unk89A17723 { get; set; }

        [DisplayName("7DB236F2")]
        [EbxFieldMeta(EbxFieldType.Int32)]
        public int unk7DB236F2 { get; set; }

        [DisplayName("41BCDB2D")]
        [EbxFieldMeta(EbxFieldType.Float32)]
        public float unk41BCDB2D { get; set; }
    }
}

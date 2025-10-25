using System;
using FrostySdk.Attributes;
namespace SoundEditorPlugin.Resources
{
	[EbxClassMeta()]
	public class Variation
	{
		[EbxFieldMeta()]
		public uint VariationId { get; set; }
		[EbxFieldMeta()]
		public uint FirstSubtitleIndex { get; set; }
		[EbxFieldMeta()]
		public uint SubtitleCount { get; set; }
		[EbxFieldMeta()]
		public uint MemoryChunkIndex { get; set; }
		[EbxFieldMeta()]
		public uint StreamChunkIndex { get; set; }
		[EbxFieldMeta()]
		public uint FirstSegmentIndex { get; set; }
		[EbxFieldMeta()]
		public uint SegmentCount { get; set; }
		[EbxFieldMeta()]
		public uint FirstLoopSegmentIndex { get; set; }
		[EbxFieldMeta()]
		public uint LastLoopSegmentIndex { get; set; }
	}
}
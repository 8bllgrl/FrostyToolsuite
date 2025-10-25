using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Frosty.Core;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using Microsoft.CSharp.RuntimeBinder;
namespace SoundEditorPlugin.Resources
{
    public class NewWaveAssetOverride : BaseTypeOverride
    {
        [FieldIndex(0)]
        public BaseFieldOverride Chunks { get; set; }
        [EbxFieldMeta()]
        [FieldIndex(10)]
        public List<Variation> RuntimeVariations { get; set; }
        [EbxFieldMeta()]
        [FieldIndex(11)]
        public List<Selection> Selection { get; set; }
        [EbxFieldMeta()]
        [FieldIndex(12)]
        public List<SelectionParameter> SelectionParameters { get; set; }
        [EbxFieldMeta()]
        [FieldIndex(16)]
        public List<Segment> Segments { get; set; }
        [EbxFieldMeta()]
        [FieldIndex(17)]
        public List<Persistence> Persistence { get; set; }

        public override void Load()
        {
            object original = this.Original;
            AssetManager assetManager = App.AssetManager;

            // --- Decompilation Fix Start: Replace dynamic calls with static property access ---
            string assetName = "";
            if (original is AssetEntry entry)
            {
                assetName = entry.Name;
            }
            // --- Decompilation Fix End ---

            ResAssetEntry resEntry = assetManager.GetResEntry(assetName.ToLower());
            NewWaveResource resAs = App.AssetManager.GetResAs<NewWaveResource>(resEntry, null);

            this.SelectionParameters = resAs.SelectionParameters;
            this.Selection = resAs.Selections;
            this.RuntimeVariations = resAs.Variations;
            this.Segments = resAs.Segments;
            this.Persistence = resAs.Persistences;
        }
    }
}

using Frosty.Core;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
namespace SoundEditorPlugin.Resources
{
    public class LocalizedWaveAssetOverride : BaseTypeOverride
    {
        [FieldIndex(0)]
        public BaseFieldOverride Chunks { get; set; }
        [EbxFieldMeta(EbxFieldType.Struct)]
        [FieldIndex(10)]
        public List<Variation> RuntimeVariations { get; set; }
        [EbxFieldMeta(EbxFieldType.Struct)]
        [FieldIndex(11)]
        public List<Selection> Selection { get; set; }
        [EbxFieldMeta(EbxFieldType.Struct)]
        [FieldIndex(12)]
        public List<SelectionParameter> SelectionParameters { get; set; }
        [EbxFieldMeta(EbxFieldType.Struct)]
        [FieldIndex(16)]
        public List<Segment> Segments { get; set; }
        [EbxFieldMeta(EbxFieldType.Struct)]
        [FieldIndex(17)]
        public List<Persistence> Persistence { get; set; }
        [EbxFieldMeta(EbxFieldType.Struct)]
        [FieldIndex(18)]
        public List<Subtitle> Subtitles { get; set; }

        //public override void Load()
        //{
        //    object original = this.Original;
        //    AssetManager assetManager = App.AssetManager;

        //    // --- Decompilation Fix Start: Replace dynamic calls with static property access ---
        //    string assetName = "";
        //    if (original is AssetEntry entry)
        //    {
        //        assetName = entry.Name;
        //    }
        //    // --- Decompilation Fix End ---

        //    ResAssetEntry resEntry = assetManager.GetResEntry(assetName.ToLower());
        //    NewWaveResource resAs = App.AssetManager.GetResAs<NewWaveResource>(resEntry, null);
        //    this.SelectionParameters = resAs.SelectionParameters;
        //    this.Selection = resAs.Selections;
        //    this.RuntimeVariations = resAs.Variations;
        //    this.Segments = resAs.Segments;
        //    this.Subtitles = resAs.Subtitles;
        //    this.Persistence = resAs.Persistences;
        //}


        //TODO: double check the LocalizedWaveAssetOverride in the output

        public override void Load()
        {
            dynamic og = Original;
            ResAssetEntry entry = App.AssetManager.GetResEntry((string)og.Name);
            NewWaveResource res = App.AssetManager.GetResAs<NewWaveResource>(entry);
            SelectionParameters = res.SelectionParameters;
            Selection = res.Selections;
            RuntimeVariations = res.Variations;
            Segments = res.Segments;
            Subtitles = res.Subtitles;
            Persistence = res.Persistences;
        }
    }
}

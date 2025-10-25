using Frosty.Core;
using Frosty.Core.Controls.Editors;
using FrostySdk.Attributes;
using FrostySdk.IO;
using System;

namespace SoundEditorPlugin
{

    [DisplayName("Sound Options")]
    public class SoundOptions : OptionsExtension
    {

        [Category("Editor")]
        [DisplayName("Sound Volume")]
        [Description("Playback volume for sounds.")]
        [Editor(typeof(FrostySliderEditor))]
        [SliderMinMax(0f, 100f, 1f, 10f, true)]
        [EbxFieldMeta(EbxFieldType.Float32)]
        public float Volume { get; set; } = 20f;

        public override void Load()
        {
            this.Volume = Config.Get<float>("SoundVolume", 20f, 0, null);
        }

        public override void Save()
        {
            Config.Add("SoundVolume", this.Volume, 0, null);
            Config.Save("");
        }

        public override bool Validate()
        {
            return this.Volume >= 0f && this.Volume <= 100f;
        }
    }
}
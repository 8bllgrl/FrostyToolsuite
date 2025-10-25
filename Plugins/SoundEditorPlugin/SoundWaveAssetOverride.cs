using System;
using FrostySdk;

namespace SoundEditorPlugin
{

    public class SoundWaveAssetOverride : BaseTypeOverride
    {

        public BaseFieldOverride Chunks { get; set; }

        public BaseFieldOverride RuntimeVariations { get; set; }

        public BaseFieldOverride Segments { get; set; }

        public BaseFieldOverride Localization { get; set; }

        public BaseFieldOverride SubtitleStringIds { get; set; }

        public BaseFieldOverride Subtitles { get; set; }
    }
}
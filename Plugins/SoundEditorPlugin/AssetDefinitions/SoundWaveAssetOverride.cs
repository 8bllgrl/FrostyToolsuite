using FrostySdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundEditorPlugin.AssetDefinitions
{
    public class SoundWaveAssetOverride : BaseTypeOverride
    {
        //#if FROSTY_DEVELOPER
        public BaseFieldOverride Chunks { get; set; }
        public BaseFieldOverride RuntimeVariations { get; set; }
        public BaseFieldOverride Segments { get; set; }
        public BaseFieldOverride Localization { get; set; }
        public BaseFieldOverride SubtitleStringIds { get; set; }
        public BaseFieldOverride Subtitles { get; set; }
        //#else
        //[IsHidden]
        //public BaseFieldOverride Chunks { get; set; }
        //[IsHidden]
        //public BaseFieldOverride RuntimeVariations { get; set; }
        //[IsHidden]
        //public BaseFieldOverride Segments { get; set; }
        //[IsHidden]
        //public BaseFieldOverride Localization { get; set; }
        //[IsHidden]
        //public BaseFieldOverride SubtitleStringIds { get; set; }
        //[IsHidden]
        //public BaseFieldOverride Subtitles { get; set; }
        //#endif
    }

}

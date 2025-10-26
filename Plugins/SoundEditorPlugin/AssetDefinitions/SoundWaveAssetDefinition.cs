using Frosty.Core.Controls;
using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundEditorPlugin.AssetDefinitions
{
    public class SoundWaveAssetDefinition : SoundAssetDefinition
    {
        public override FrostyAssetEditor GetEditor(ILogger logger)
        {
            return new FrostySoundWaveEditor(logger);
        }
    }
}

using System;
using Frosty.Core.Controls;
using FrostySdk.Interfaces;

namespace SoundEditorPlugin
{

    public class SoundWaveAssetDefinition : SoundAssetDefinition
    {

        public override FrostyAssetEditor GetEditor(ILogger logger)
        {
            return new FrostySoundWaveEditor(logger);
        }
    }
}
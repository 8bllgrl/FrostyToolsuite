using System;
using Frosty.Core.Controls;
using FrostySdk.Interfaces;

namespace SoundEditorPlugin
{

    public class NewWaveAssetDefinition : SoundAssetDefinition
    {

        public override FrostyAssetEditor GetEditor(ILogger logger)
        {
            return new FrostyNewWaveEditor(logger);
        }
    }
}
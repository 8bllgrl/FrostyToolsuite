using System;
using Frosty.Core.Controls;
using FrostySdk.Interfaces;

namespace SoundEditorPlugin.AssetDefinitions
{
    public class OctaneAssetDefinition : SoundAssetDefinition
    {
        public override FrostyAssetEditor GetEditor(ILogger logger)
        {
            return new FrostyOctaneSoundEditor(logger);
        }
    }
}

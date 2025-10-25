using System;
using Frosty.Core.Controls;
using FrostySdk.Interfaces;

namespace SoundEditorPlugin
{

    public class HarmonySampleBankAssetDefinition : SoundAssetDefinition
    {

        public override FrostyAssetEditor GetEditor(ILogger logger)
        {
            return new FrostyHarmonySampleBankEditor(logger);
        }
    }
}
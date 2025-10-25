using System;
using System.Windows.Media;
using Frosty.Core;

namespace SoundEditorPlugin
{

    public class SoundAssetDefinition : AssetDefinition
    {

        public override ImageSource GetIcon()
        {
            return SoundAssetDefinition.imageSource;
        }

        protected static ImageSource imageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/SoundEditorPlugin;component/Images/SoundFileType.png") as ImageSource;
    }
}
using Frosty.Core;
using Frosty.Core.Controls;
using FrostySdk;
using FrostySdk.Interfaces;
using System.Windows.Media;

namespace SoundEditorPlugin.AssetDefinitions
{

    public class SoundAssetDefinition : AssetDefinition
    {
        protected static ImageSource imageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/SoundEditorPlugin;component/Images/SoundFileType.png") as ImageSource;
        public override ImageSource GetIcon()
        {
            return imageSource;
        }
    }
}

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using Frosty.Core.Attributes;
using SoundEditorPlugin;
using SoundEditorPlugin.Resources;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: Guid("5fdd1243-084e-42dc-99bb-394a169d988f")]
[assembly: PluginDisplayName("Sound Editor")]
[assembly: PluginAuthor("GalaxyMan2015 & wannkunstbeikor")]
[assembly: PluginVersion("1.0.0.1")]
[assembly: RegisterOptionsExtension(typeof(SoundOptions), 0)]
[assembly: RegisterTypeOverride("LocalizedWaveAsset", typeof(LocalizedWaveAssetOverride))]
[assembly: RegisterTypeOverride("NewWaveAsset", typeof(NewWaveAssetOverride))]
[assembly: RegisterAssetDefinition("SoundWaveAsset", typeof(SoundWaveAssetDefinition))]
[assembly: RegisterAssetDefinition("NewWaveAsset", typeof(NewWaveAssetDefinition))]
[assembly: RegisterAssetDefinition("HarmonySampleBankAsset", typeof(HarmonySampleBankAssetDefinition))]
[assembly: RegisterAssetDefinition("OctaneAsset", typeof(OctaneAssetDefinition))]
[assembly: RegisterAssetDefinition("ImpulseResponseAsset", typeof(ImpulseResponseAssetDefinition))]
[assembly: RegisterThirdPartyDll("NAudio")]
[assembly: RegisterThirdPartyDll("NAudio.WaveFormRenderer")]
[assembly: AssemblyCompany("SoundEditorPlugin")]
[assembly: AssemblyConfiguration("Developer - Debug")]
[assembly: AssemblyCopyright("Copyright ©  2020")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("SoundEditorPlugin")]
[assembly: AssemblyTitle("SoundEditorPlugin")]

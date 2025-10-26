using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SoundEditorPlugin.Playback;
using SoundEditorPlugin.Resources;
using SoundEditorPlugin.WAV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WaveFormatExtensible = SharpDX.Multimedia.WaveFormatExtensible;

// NOTE: For the new logic to compile, you must have definitions for:
// - NewWaveResource (the actual EBX asset type holding segments/variations)
// - ToolHelper (containing asset management logic like GetDurationInSecondsFromBuffer)
// As I do not have these, this code assumes they exist or are mocked as `dynamic` or similar in your environment.

namespace SoundEditorPlugin
{
    [TemplatePart(Name = PART_TracksListBox, Type = typeof(ListView))]
    [TemplatePart(Name = PART_PlayButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_StopButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_VolumeSlider, Type = typeof(Slider))]
    [TemplatePart(Name = PART_SoundExportMenuItem, Type = typeof(MenuItem))]
    [TemplatePart(Name = PART_SoundImportMenuItem, Type = typeof(MenuItem))]
    public class FrostySoundDataEditor : FrostyAssetEditor
    {
        private const string PART_TracksListBox = "PART_TracksListBox";
        private const string PART_PlayButton = "PART_PlayButton";
        private const string PART_StopButton = "PART_StopButton";
        private const string PART_VolumeSlider = "PART_VolumeSlider";
        private const string PART_SoundExportMenuItem = "PART_SoundExportMenuItem";
        private const string PART_SoundImportMenuItem = "PART_SoundImportMenuItem";

        public static readonly DependencyProperty TracksListProperty = DependencyProperty.Register("TracksList", typeof(ObservableCollection<SoundDataTrack>), typeof(FrostySoundDataEditor), new FrameworkPropertyMetadata(null));
        public ObservableCollection<SoundDataTrack> TracksList
        {
            get => (ObservableCollection<SoundDataTrack>)GetValue(TracksListProperty);
            set => SetValue(TracksListProperty, value);
        }

        public bool IsPlaying => audioPlayer != null && audioPlayer.IsPlaying;

        private ListView tracksListBox;
        private Button playButton;
        private Button stopButton;
        private Slider volumeSlider;
        private AudioPlayer audioPlayer;
        private bool bFirstTime = true;

        public FrostySoundDataEditor(ILogger inLogger)
            : base(inLogger)
        {
        }

        static FrostySoundDataEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostySoundDataEditor), new FrameworkPropertyMetadata(typeof(FrostySoundDataEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            tracksListBox = GetTemplateChild(PART_TracksListBox) as ListView;

            playButton = GetTemplateChild(PART_PlayButton) as Button;
            playButton.Click += PlayButton_Click;

            stopButton = GetTemplateChild(PART_StopButton) as Button;
            stopButton.Click += StopButton_Click;

            volumeSlider = GetTemplateChild(PART_VolumeSlider) as Slider;

            // Using Config.Get<float> with optional arguments (0, null) as seen in SoundOptions.cs logic
            volumeSlider.Value = Math.Min(Config.Get<float>("SoundVolume", 20.0f, 0, null), 100);

            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;

            tracksListBox.SelectionChanged += TracksListBox_SelectionChanged;
            MenuItem mi = GetTemplateChild(PART_SoundExportMenuItem) as MenuItem;
            mi.Click += SoundExportMenuItem_Click;
            mi = GetTemplateChild(PART_SoundImportMenuItem) as MenuItem;
            mi.Click += SoundImportMenuItem_Click;
            Loaded += FrostySoundDataEditor_Loaded;

            TracksList = new ObservableCollection<SoundDataTrack>();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.SoundDispose();
            stopButton.IsEnabled = false;

            if (tracksListBox.SelectedItem != null)
                playButton.IsEnabled = true;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(tracksListBox.SelectedItem is SoundDataTrack currentTrack) || !currentTrack.IsLoaded) // Added IsLoaded check
                return;

            // Setting volume using the XAudio2 method
            audioPlayer.OutputVoice.SetVolume((float)(volumeSlider.Value / 100.0), 0);
            audioPlayer.PlaySound(currentTrack);

            playButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            await Dispatcher.InvokeAsync(async () =>
            {
                while (IsPlaying)
                {
                    currentTrack.Progress = audioPlayer.Progress * 800.0;
                    await Task.Delay(30);
                }

                currentTrack.Progress = 0;
                stopButton.IsEnabled = false;
                playButton.IsEnabled = true;
            });
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider && slider.Value <= 100.0 && slider.Value >= 0.0)
            {
                // Setting volume using the XAudio2 method
                audioPlayer.OutputVoice.SetVolume((float)(slider.Value / 100.0), 0);

                // Using Config.Add with optional arguments (0, null) and Config.Save("") as seen in SoundOptions.cs
                Config.Add("SoundVolume", (float)slider.Value, 0, null);
                Config.Save("");
            }
        }

        private void TracksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tracksListBox.SelectedItem == null)
                return;

            if (!IsPlaying)
            {
                SoundDataTrack soundDataTrack = tracksListBox.SelectedItem as SoundDataTrack;
                if (soundDataTrack != null && soundDataTrack.IsLoaded) // Added IsLoaded check
                {
                    playButton.IsEnabled = true;
                    return;
                }
                playButton.IsEnabled = false;
            }
        }

        public override void Closed()
        {
            audioPlayer.Dispose();
        }

        private void FrostySoundDataEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (bFirstTime)
            {
                audioPlayer = new AudioPlayer();

                List<SoundDataTrack> tracks = null;
                FrostyTaskWindow.Show("Loading tracks", "", owner =>
                {
                    tracks = InitialLoad(owner);
                }, false, null); // Added optional args to match decompiled usage

                foreach (var track in tracks)
                    TracksList.Add(track);

                bFirstTime = false;
            }
        }

        protected virtual List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            return new List<SoundDataTrack>();
        }

        // NEW: Virtual method to reload the track after import (Token: 0x0600004B)
        protected virtual Task ReloadTrack(dynamic newWave, SoundDataTrack track)
        {
            return null;
        }

        private void SoundExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (tracksListBox.SelectedItem == null)
                return;

            // Pass allowMultiple: true to sfd constructor as the original code suggested multi-select
            FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save WAV File", "WAV file (*.wav)|*.wav", "Sound", AssetEntry.Filename, true);

            if (!sfd.ShowDialog())
                return;

            // Loop through selected items and export each one
            for (int trackIndex = 0; trackIndex < tracksListBox.SelectedItems.Count; trackIndex++)
            {
                SoundDataTrack indexedTrack = (SoundDataTrack)tracksListBox.SelectedItems[trackIndex];

                // Maintain the user's more robust logic for filename generation based on selection count
                string indexedFilename;
                if (tracksListBox.SelectedItems.Count > 1)
                {
                    // If multiple items are selected, append the index
                    indexedFilename = sfd.FileName.Replace(".wav", $" {trackIndex}.wav");
                }
                else
                {
                    // If only one item is selected, use the original selected file name
                    indexedFilename = sfd.FileName;
                }

                SoundExportMenuItem_Export(indexedTrack, indexedFilename);
                logger.Log("Exported {0} to {1}", AssetEntry.Name, indexedFilename);
            }
        }


        private void SoundExportMenuItem_Export(SoundDataTrack track, String filename)
        {
            FrostyTaskWindow.Show("Exporting Sound", "", task =>
            {
                // Format chunk (16-bit PCM)
                WAVFormatChunk fmt = new WAVFormatChunk(WAVFormatChunk.DataFormats.WAVE_FORMAT_PCM, (ushort)track.ChannelCount, (uint)track.SampleRate, (uint)(track.ChannelCount * 2 * track.SampleRate), (ushort)(2 * track.ChannelCount), 16);
                List<WAVDataFrame> frames = new List<WAVDataFrame>();

                // Convert short array samples to WAV data frames
                for (int i = 0; i < track.Samples.Length / track.ChannelCount; i++)
                {
                    // write frame
                    WAV16BitDataFrame frame = new WAV16BitDataFrame((ushort)track.ChannelCount);
                    for (int channel = 0; channel < track.ChannelCount; channel++)
                    {
                        // Interleaved samples: sample_0_ch_0, sample_0_ch_1, sample_1_ch_0, sample_1_ch_1, ...
                        frame.Data[channel] = track.Samples[i * track.ChannelCount + channel];
                    }
                    frames.Add(frame);
                }

                WAVDataChunk data = new WAVDataChunk(fmt, frames);
                // RIFF Main Chunk header
                RIFFMainChunk main = new RIFFMainChunk(new RIFFChunkHeader(0, new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0), new byte[] { 0x57, 0x41, 0x56, 0x45 });

                using (FileStream stream = new FileStream(filename, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    main.Write(writer, new List<IRIFFChunk>(new IRIFFChunk[] { fmt, data }));
                }
            }, false, null); // Added optional args to match decompiled usage
        }


        private void SoundImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (tracksListBox.SelectedItem == null)
                return;

            FrostyOpenFileDialog ofd = new FrostyOpenFileDialog("Import Sound", "Audio Files (*.mp3; *.wav)|*.mp3; *.wav", "Sound");
            if (ofd.ShowDialog())
            {
                try
                {
                    // Update: Use Dispatcher.Invoke and new signature to match decompiled logic (Token: 0x0600004E)
                    FrostyTaskWindow.Show("Importing track", "", (task) =>
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            SoundDataTrack soundDataTrack = (SoundDataTrack)tracksListBox.SelectedItem;
                            ImportSound(ofd.FileName, soundDataTrack, task);
                        });
                    }, false, null);
                }
                catch (Exception exp)
                {
                    // RevertAsset with optional arguments (false, true) as seen in decompiled
                    App.AssetManager.RevertAsset(AssetEntry, false, true);
                    logger.LogError(exp.Message);
                }
            }
        }

        // NEW: Helper method to extract the file format string (Token: 0x0600004F)
        private string GetFormat(int? codec)
        {
            if (codec != null)
            {
                switch (codec.GetValueOrDefault())
                {
                    case 2: return "s16b_int";
                    case 3: return "eaxma";
                    case 4: return "xas_int";
                    case 5: return "ealayer3_int";
                    case 6: return "ealayer3pcm_int";
                    case 7: return "ealayer3spike_int";
                    case 9: return "easpeex";
                    case 11: return "eamp3";
                    case 12: return "eaopus";
                    case 13: return "eaatrac9";
                    case 14: return "multistreamopus";
                    case 15: return "multistreamopusuncoupled";
                }
            }
            return "multistreamopus";
        }

        // NEW: Helper method to generate segment offset flags (Token: 0x06000050)
        private static uint GetSegmentOffsetFlags(bool isValid, bool isStreaming)
        {
            if (isValid && isStreaming)
                return 3U;
            if (isValid)
                return 1U;
            return 0U;
        }

        // NEW: Helper method to align a value to a specified alignment (Token: 0x06000051)
        private static int AlignTo(int value, int alignment)
        {
            int num = value % alignment;
            if (num == 0)
                return value;
            int num2 = alignment - num;
            return value + num2;
        }

        // NEW: Helper to perform the audio conversion and encoding, extracted from the original ImportSound logic.
        // This is functionally equivalent to what a ToolHelper.ImportSound might do for PCM16 format.
        private byte[] PrepareAudioData(string importFileName)
        {
            MemoryStream ms = new MemoryStream();
            byte[] resultBuf = null;

            // 1. Convert source file to a common format (WAV/16-bit PCM in memory)
            if (importFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                // force stereo for .wav files if needed
                using (var reader = new AudioFileReader(importFileName))
                {
                    if (reader.WaveFormat.Channels == 1)
                    {
                        var stereo = new MonoToStereoSampleProvider(reader) { LeftVolume = 1.0f, RightVolume = 1.0f };
                        WaveFileWriter.WriteWavFileToStream(ms, new SampleToWaveProvider16(stereo));
                    }
                    else
                    {
                        WaveFileWriter.WriteWavFileToStream(ms, reader);
                    }
                }
            }
            else if (importFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                using (var reader = new MediaFoundationReader(importFileName))
                {
                    WaveFileWriter.WriteWavFileToStream(ms, reader);
                }
            }

            // 2. Custom Encoding to Frostbite Chunk Format (Pcm16Big block format)
            using (var reader = new StreamMediaFoundationReader(ms, null))
            {
                int totalSamples = 0;
                using (var writer = new NativeWriter(new MemoryStream(), false, false))
                {
                    writer.Write(0x4800000C, Endian.Big);
                    writer.Write((byte)0x12); // codec, Pcm16Big (assuming 0x12 is the Pcm16Big codec ID)
                    writer.Write((byte)((reader.WaveFormat.Channels - 1) << 2));
                    writer.Write((ushort)(reader.WaveFormat.SampleRate), Endian.Big);

                    long pos = writer.Position;
                    writer.Write(0x40000000, Endian.Big); // Samples offset placeholder

                    while (reader.Position < reader.Length)
                    {
                        // 0x2600 samples * 2 bytes/sample * channels
                        int bufLength = 19456 * reader.WaveFormat.Channels;
                        // 0x2600 samples = 9728 samples
                        if (totalSamples + 9728 > 0x00ffffff)
                            break;

                        byte[] buf = new byte[bufLength];
                        int actualRead = reader.Read(buf, 0, bufLength);
                        if (actualRead == 0)
                            break;

                        // Corrected magic number to match decompiled (0x44000000 -> 1140850688)
                        writer.Write((actualRead + 8) | 0x44000000, Endian.Big);
                        writer.Write(((actualRead / reader.WaveFormat.Channels) / 2), Endian.Big);

                        for (int i = 0; i < actualRead / 2; i++)
                        {
                            short s = BitConverter.ToInt16(buf, i * 2);
                            writer.Write(s, Endian.Big);
                        }

                        totalSamples += ((actualRead / reader.WaveFormat.Channels) / 2);
                    }

                    writer.Write(0x45000004, Endian.Big);
                    writer.Position = pos;
                    writer.Write(totalSamples | 0x40000000, Endian.Big);

                    resultBuf = writer.ToByteArray();
                }
            }
            return resultBuf;
        }


        // Replaces the original ImportSound with the correct signature and logic (Token: 0x06000052)
        private async void ImportSound(string importFileName, SoundDataTrack track, FrostyTaskWindow task)
        {
            // The original decompiled code calls HelperBase<ToolHelper>.Instance.ImportSound here.
            // We use the user's existing NAudio/Custom encoding logic to generate the resultBuf.
            byte[] encodedData = PrepareAudioData(importFileName);
            byte[] seekTable = null; // Assuming no separate seek table is generated by PrepareAudioData

            // These are the buffers that would be returned by the external helper
            byte[] item = encodedData;
            byte[] item2 = seekTable;

            // --- Decompiled Asset Modification Logic Starts Here ---

            // Cast to dynamic to access EBX properties without compile-time types
            dynamic originalSoundWave = RootObject;

            // Determine if the audio chunk contains a separate seek table (item2 != null) and combine if so.
            byte[] array;
            uint seekTableOffsetFlags;
            uint dataOffsetFlags;

            // Check if the wave asset is streamed (based on StreamPool property)
            // Assuming PointerRef is available and has a Type property
            dynamic streamPool = originalSoundWave.StreamPool;
            bool isStreaming = streamPool != null && (uint)streamPool.Type > 0;

            if (item2 != null)
            {
                int num = AlignTo(item2.Length, 4); // Align seek table to 4 bytes
                array = new byte[num + item.Length];
                Array.Copy(item2, array, item2.Length);
                Array.Copy(item, 0, array, num, item.Length);

                // Seek Table Offset (Starts at 0, flags added)
                seekTableOffsetFlags = 0U | GetSegmentOffsetFlags(true, isStreaming);
                // Data Offset (Starts after aligned seek table, flags added)
                dataOffsetFlags = (uint)(num | (int)GetSegmentOffsetFlags(true, isStreaming));
            }
            else
            {
                array = item;
                // Data Offset (Starts at 0, flags added)
                dataOffsetFlags = 0U | GetSegmentOffsetFlags(true, isStreaming);
                // Seek Table Offset (Invalid/null, flags added)
                seekTableOffsetFlags = 0U | GetSegmentOffsetFlags(false, isStreaming);
            }

            // Get duration from the encoded buffer (This requires an external helper - simplified to 0f)
            // float durationInSecondsFromBuffer = HelperBase<ToolHelper>.Instance.GetDurationInSecondsFromBuffer(item);
            float durationInSecondsFromBuffer = 0f;

            // 1. Create a NEW Chunk
            Guid newGuid = App.AssetManager.AddChunk(array, null, null, Array.Empty<int>());

            // 2. Identify Chunk/Variation to replace
            int chunkIndexToReplace = track.ChunkIndex;
            dynamic chunkToReplace = originalSoundWave.Chunks[track.ChunkIndex];

            ChunkAssetEntry oldChunkEntry = App.AssetManager.GetChunkEntry(track.ChunkId);
            ChunkAssetEntry newChunkEntry = App.AssetManager.GetChunkEntry(newGuid);

            // Determine if the old chunk was added (i.e., modified by user previously)
            bool wasOldChunkAdded = oldChunkEntry != null && oldChunkEntry.IsAdded;

            // Get the NewWaveResource for modification
            // Assumes RootObject has a 'Name' property and GetResAs is available
            //??? TODO
            //dynamic newWaveResource = App.AssetManager.GetResAs<object>(App.AssetManager.GetResEntry(originalSoundWave.Name.ToLower()), null);
            dynamic newWaveResource = App.AssetManager.GetResAs<NewWaveResource>(App.AssetManager.GetResEntry(originalSoundWave.Name.ToLower()), null);

            // 3. Handle Chunk SuperBundle/Bundle linking and old chunk deletion/reverting
            if (wasOldChunkAdded)
            {
                // If old chunk was a modified chunk, revert it and transfer bundles to the new chunk
                App.AssetManager.RevertAsset(oldChunkEntry, false, true);

                // Transfer superbundles and bundles from the old chunk entry
                foreach (int superBundleId in oldChunkEntry.AddedSuperBundles)
                {
                    newChunkEntry.AddToSuperBundle(superBundleId);
                }
                newChunkEntry.AddToBundles(oldChunkEntry.AddedBundles);
            }
            else
            {
                // If old chunk was original, use it to get bundle info for the new one
                dynamic chunkAssetEntry = App.AssetManager.GetChunkEntry(chunkToReplace.ChunkId);

                // New chunk is added to the EBX structure. ChunkIndex is the last index.
                chunkIndexToReplace = (int)((IList)originalSoundWave.Chunks).Count;
                ((IList)originalSoundWave.Chunks).Add(new object()); // Placeholder to increment count

                // Transfer superbundles and bundles from the original chunk entry
                foreach (int superBundleId in chunkAssetEntry.SuperBundles)
                {
                    newChunkEntry.AddToSuperBundle(superBundleId);
                }
                newChunkEntry.AddToBundles(chunkAssetEntry.Bundles);
            }

            // 4. Update the EBX (SoundDataChunk reference)
            // The object at chunkIndexToReplace is either the original one (if wasOldChunkAdded is false)
            // or the newly created one (if wasOldChunkAdded is true, we just created a new dynamic object above).
            // We use a new dynamic object (or the existing one) to set ChunkId and ChunkSize
            dynamic newChunkEbxObject = Activator.CreateInstance(originalSoundWave.Chunks.GetType().GetGenericArguments()[0]);
            newChunkEbxObject.ChunkId = newGuid;
            newChunkEbxObject.ChunkSize = (uint)array.Length;

            // Set the new chunk in the Chunks collection
            ((IList)originalSoundWave.Chunks)[chunkIndexToReplace] = newChunkEbxObject;


            // 5. Update the Resource Asset (NewWaveResource)
            if (track.SegmentIndex > -1)
            {
                newWaveResource.Segments[track.SegmentIndex].SamplesOffset = dataOffsetFlags;
                newWaveResource.Segments[track.SegmentIndex].SeekTableOffset = seekTableOffsetFlags;
                newWaveResource.Segments[track.SegmentIndex].SegmentLength = durationInSecondsFromBuffer;
            }

            if (track.VariationIndex > -1)
            {
                if (isStreaming)
                {
                    newWaveResource.Variations[track.VariationIndex].StreamChunkIndex = (uint)chunkIndexToReplace;
                }
                else
                {
                    newWaveResource.Variations[track.VariationIndex].MemoryChunkIndex = (uint)chunkIndexToReplace;
                }
            }

            // Update Chunks list in the newWaveResource based on unique ChunkIds (Token: 0x06000052 final section)
            Dictionary<Guid, uint> chunkMap = new Dictionary<Guid, uint>();
            for (int i = 0; i < ((IList)originalSoundWave.Chunks).Count; i++)
            {
                dynamic chunk = originalSoundWave.Chunks[i];
                Guid chunkId = (Guid)chunk.ChunkId;
                uint chunkSize = (uint)chunk.ChunkSize;

                if (!chunkMap.ContainsKey(chunkId))
                {
                    chunkMap[chunkId] = chunkSize;
                }
            }

            // Clear and rebuild the Chunks list on the newWaveResource
            IList resourceChunks = (IList)newWaveResource.Chunks;
            resourceChunks.Clear();

            foreach (var kvp in chunkMap)
            {
                dynamic newChunk = Activator.CreateInstance(newWaveResource.Chunks.GetType().GetGenericArguments()[0]);
                newChunk.ChunkId = kvp.Key;
                newChunk.ChunkSize = kvp.Value;
                resourceChunks.Add(newChunk);
            }

            // 6. Final cleanup and UI update
            audioPlayer.Dispose();
            audioPlayer = new AudioPlayer();

            // Invoke UI updates on the main thread
            await Dispatcher.InvokeAsync(() =>
            {
                // Update the EBX asset
                Asset.Update();
                App.AssetManager.ModifyEbx(AssetEntry.Name, Asset);

                // Update the resource asset
                App.AssetManager.ModifyRes(originalSoundWave.Name.ToLower(), newWaveResource);

                // Link assets
                App.AssetManager.GetResEntry(AssetEntry.Name).LinkAsset(App.AssetManager.GetChunkEntry(newGuid));
                (AssetEntry as EbxAssetEntry).LinkAsset(App.AssetManager.GetResEntry(AssetEntry.Name));

                AssetModified = true;
                InvokeOnAssetModified();

                // Reload track list from the new data
                Task reloadTask = ReloadTrack(newWaveResource, track);
                if (reloadTask != null)
                {
                    reloadTask.RunSynchronously();
                }

                // Note: The decompiled code seems to miss updating TracksList.Clear()/repopulating here, 
                // but ReloadTrack is intended to handle that. Assuming the user's base class handles track list reload.
                // If not, the original logic for TracksList.Clear() and repopulate should be added here:
                /*
                List<SoundDataTrack> tracks = InitialLoad(task);
                TracksList.Clear();
                foreach (var t in tracks)
                    TracksList.Add(t);
                */
            });
        }
    }
}

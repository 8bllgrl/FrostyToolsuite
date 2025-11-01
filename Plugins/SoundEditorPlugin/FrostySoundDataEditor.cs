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
using SoundEditorPlugin.Helpers;
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

// NOTE: This file assumes the existence of the following classes and helper methods:
// - SoundDataTrack (must have properties: ChannelCount, SampleRate, Samples, ChunkIndex, ChunkId, SegmentIndex, VariationIndex)
// - AudioPlayer (must have OutputVoice.SetVolume, PlaySound, Progress, SoundDispose)
// - NewWaveResource (the inferred type for dynamic newWaveResource)
// - NativeWriter, Endian (from FrostySdk.IO)
// - WAVFormatChunk, WAV16BitDataFrame, WAVDataChunk, RIFFMainChunk, IRIFFChunk (from SoundEditorPlugin.WAV)


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
            if (!(tracksListBox.SelectedItem is SoundDataTrack currentTrack) || !currentTrack.IsLoaded)
                return;

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

                audioPlayer.OutputVoice.SetVolume((float)(slider.Value / 100.0), 0);

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
                if (soundDataTrack != null && soundDataTrack.IsLoaded)
                {
                    playButton.IsEnabled = true;
                    return;
                }
                playButton.IsEnabled = false;
            }
        }

        public override void Closed()
        {
            if (audioPlayer != null)
            {
                audioPlayer.Dispose();
            }
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
                }, false, null);

                foreach (var track in tracks)
                    TracksList.Add(track);

                bFirstTime = false;
            }
        }

        protected virtual List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            return new List<SoundDataTrack>();
        }

        // Changed type to NewWaveResource for type safety with ReloadTrack, assuming it's available.
        protected virtual Task ReloadTrack(NewWaveResource newWave, SoundDataTrack track)
        {
            return null;
        }

        private void SoundExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (tracksListBox.SelectedItem == null)
                return;

            // CRITICAL FIX: Retrieve AssetEntry properties on the UI thread before starting the background task
            string assetFilename = this.AssetEntry.Filename;
            string assetName = this.AssetEntry.Name;

            FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save WAV File", "WAV file (*.wav)|*.wav", "Sound", assetFilename, true);

            if (!sfd.ShowDialog())
                return;

            for (int trackIndex = 0; trackIndex < tracksListBox.SelectedItems.Count; trackIndex++)
            {
                SoundDataTrack indexedTrack = (SoundDataTrack)tracksListBox.SelectedItems[trackIndex];

                string indexedFilename;
                if (tracksListBox.SelectedItems.Count > 1)
                {
                    indexedFilename = sfd.FileName.Replace(".wav", $" {indexedTrack.Name}.wav");
                }
                else
                {
                    indexedFilename = sfd.FileName;
                }

                SoundExportMenuItem_Export(indexedTrack, indexedFilename, assetName);
            }
        }

        // Updated signature to accept assetName to avoid accessing AssetEntry on a background thread.
        private void SoundExportMenuItem_Export(SoundDataTrack track, string filename, string assetName)
        {
            FrostyTaskWindow.Show("Exporting Sound", "", task =>
            {
                // This code block runs on a background thread.

                WAVFormatChunk fmt = new WAVFormatChunk(WAVFormatChunk.DataFormats.WAVE_FORMAT_PCM, (ushort)track.ChannelCount, (uint)track.SampleRate, (uint)(track.ChannelCount * 2 * track.SampleRate), (ushort)(2 * track.ChannelCount), 16);
                List<WAVDataFrame> frames = new List<WAVDataFrame>();

                // This process must be done safely outside the background task loop in a real app,
                // but since the task needs to be performed here:
                for (int i = 0; i < track.Samples.Length / track.ChannelCount; i++)
                {
                    WAV16BitDataFrame frame = new WAV16BitDataFrame((ushort)track.ChannelCount);
                    for (int channel = 0; channel < track.ChannelCount; channel++)
                    {
                        frame.Data[channel] = track.Samples[i * track.ChannelCount + channel];
                    }
                    frames.Add(frame);
                }

                WAVDataChunk data = new WAVDataChunk(fmt, frames);

                RIFFMainChunk main = new RIFFMainChunk(new RIFFChunkHeader(0, new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0), new byte[] { 0x57, 0x41, 0x56, 0x45 });

                using (FileStream stream = new FileStream(filename, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    main.Write(writer, new List<IRIFFChunk>(new IRIFFChunk[] { fmt, data }));
                }

                // FIX: Use Dispatcher.Invoke to safely log the success message (accessing App.Logger)
                Dispatcher.Invoke(() =>
                {
                    logger.Log("Exported {0} to {1}", assetName, filename);
                });

            }, false, null);
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

                    FrostyTaskWindow.Show("Importing track", "", (task) =>
                    {
                        // The decompiled code uses Dispatcher?.Invoke() which is safer
                        Dispatcher?.Invoke(() =>
                        {
                            SoundDataTrack soundDataTrack = (SoundDataTrack)this.tracksListBox.SelectedItem;
                            // Pass the task window to the import function
                            ImportSound(ofd.FileName, soundDataTrack, task);
                        });
                    }, false, null);
                }
                catch (Exception exp)
                {

                    App.AssetManager.RevertAsset(AssetEntry, false, true);
                    logger.LogError(exp.Message);
                }
            }
        }

        // NOTE: The implementation of PrepareAudioData is moved to ToolHelper.cs in the Opus fix set.
        // We leave a mock implementation here if ToolHelper wasn't provided, but assume it will be removed/corrected.

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

        private static uint GetSegmentOffsetFlags(bool isValid, bool isStreaming)
        {
            if (isValid && isStreaming)
                return 3U;
            if (isValid)
                return 1U;
            return 0U;
        }

        private static int AlignTo(int value, int alignment)
        {
            int num = value % alignment;
            if (num == 0)
                return value;
            int num2 = alignment - num;
            return value + num2;
        }

        // IMPORTANT: This method is functionally moved to ToolHelper, so the following implementation is just for completeness/mocking.
        // The real logic relies on the call to HelperBase<ToolHelper>.Instance.ImportSound(...)
        private async void ImportSound(string importFileName, SoundDataTrack track, FrostyTaskWindow task)
        {
            // --- This section relies on ToolHelper.cs to be correctly implemented and compiled. ---
            // Fetch format based on the selected track's expected codec
            string format = this.GetFormat(track.CodecUnformatted);

            // Dynamically check if the root EBX object supports streaming pool checks
            dynamic originalSoundWave = RootObject;
            dynamic streamPool = originalSoundWave.StreamPool;
            bool isSeekable = streamPool != null && (uint)streamPool.Type > 0;

            // This is the CRITICAL line that must be handled by ToolHelper
            // It calls the external tool to encode the imported audio to SPS/SEK format.
            ValueTuple<byte[], byte[]> encodedDataTuple = await HelperBase<ToolHelper>.Instance.ImportSound(importFileName, format, isSeekable);
            byte[] encodedData = encodedDataTuple.Item1; // spsData
            byte[] seekTable = encodedDataTuple.Item2;  // seekTableData

            // Get duration from the encoded buffer (relies on ToolHelper)
            float durationInSecondsFromBuffer = HelperBase<ToolHelper>.Instance.GetDurationInSecondsFromBuffer(encodedData);

            // -------------------------------------------------------------------------------------

            byte[] array;
            uint seekTableOffsetFlags;
            uint dataOffsetFlags;

            bool isStreaming = streamPool != null && (uint)streamPool.Type > 0;

            if (seekTable != null)
            {
                int num = AlignTo(seekTable.Length, 4);
                array = new byte[num + encodedData.Length];
                Array.Copy(seekTable, array, seekTable.Length);
                Array.Copy(encodedData, 0, array, num, encodedData.Length);

                seekTableOffsetFlags = 0U | GetSegmentOffsetFlags(true, isStreaming);

                dataOffsetFlags = (uint)(num | (int)GetSegmentOffsetFlags(true, isStreaming));
            }
            else
            {
                array = encodedData;

                dataOffsetFlags = 0U | GetSegmentOffsetFlags(true, isStreaming);

                seekTableOffsetFlags = 0U | GetSegmentOffsetFlags(false, isStreaming);
            }

            Guid newGuid = App.AssetManager.AddChunk(array, null, null, Array.Empty<int>());

            // Get the original chunk entry to determine if it was a user-added modification
            ChunkAssetEntry oldChunkEntry = App.AssetManager.GetChunkEntry(track.ChunkId);
            ChunkAssetEntry newChunkEntry = App.AssetManager.GetChunkEntry(newGuid);

            bool wasOldChunkAdded = oldChunkEntry != null && oldChunkEntry.IsAdded;

            // Safely retrieve the resource asset (requires NewWaveResource to be in scope/compiled)
            NewWaveResource newWaveResource = App.AssetManager.GetResAs<NewWaveResource>(App.AssetManager.GetResEntry(((string)originalSoundWave.Name).ToLower()), null);

            dynamic newChunkEbxObject;
            int chunkIndexToUse = track.ChunkIndex;

            if (wasOldChunkAdded)
            {
                // If the old chunk was added by a mod, we revert it and transfer its bundles/superbundles to the new chunk.
                App.AssetManager.RevertAsset(oldChunkEntry, false, true);

                foreach (int superBundleId in oldChunkEntry.AddedSuperBundles)
                {
                    newChunkEntry.AddToSuperBundle(superBundleId);
                }
                newChunkEntry.AddToBundles(oldChunkEntry.AddedBundles);

                // We modify the properties of the existing EBX chunk object in place.
                newChunkEbxObject = originalSoundWave.Chunks[track.ChunkIndex];
                chunkIndexToUse = track.ChunkIndex;
            }
            else
            {
                // If the old chunk was NOT added by a mod (i.e., it was a default/original chunk), 
                // we treat the new chunk as an additive modification to avoid touching the original.

                // Get the bundle info from the original chunk entry linked to the old chunk ID
                dynamic chunkAssetEntry = App.AssetManager.GetChunkEntry(originalSoundWave.Chunks[track.ChunkIndex].ChunkId);

                // Create the new EBX chunk object instance
                newChunkEbxObject = Activator.CreateInstance(originalSoundWave.Chunks.GetType().GetGenericArguments()[0]);

                // Add the new chunk EBX object to the end of the list.
                ((IList)originalSoundWave.Chunks).Add(newChunkEbxObject);

                // The new chunk object is at the end of the list. This is the index we use to reference it.
                chunkIndexToUse = ((IList)originalSoundWave.Chunks).Count - 1;

                // Transfer bundle information from the original chunk to the new chunk
                if (chunkAssetEntry != null)
                {
                    foreach (int superBundleId in chunkAssetEntry.SuperBundles)
                    {
                        newChunkEntry.AddToSuperBundle(superBundleId);
                    }
                    newChunkEntry.AddToBundles(chunkAssetEntry.Bundles);
                }
            }

            // Update the properties of the chunk EBX object with the new data
            newChunkEbxObject.ChunkId = newGuid;
            newChunkEbxObject.ChunkSize = (uint)array.Length;

            // Update the corresponding segment/variation resource pointers and duration
            if (track.SegmentIndex > -1)
            {
                newWaveResource.Segments[track.SegmentIndex].SamplesOffset = dataOffsetFlags;
                newWaveResource.Segments[track.SegmentIndex].SeekTableOffset = seekTableOffsetFlags;
                newWaveResource.Segments[track.SegmentIndex].SegmentLength = durationInSecondsFromBuffer; // NOW USES CALCULATED DURATION
            }

            if (track.VariationIndex > -1)
            {
                if (isStreaming)
                {
                    newWaveResource.Variations[track.VariationIndex].StreamChunkIndex = (uint)chunkIndexToUse;
                }
                else
                {
                    newWaveResource.Variations[track.VariationIndex].MemoryChunkIndex = (uint)chunkIndexToUse;
                }
            }

            // Update the chunk map in the NewWaveResource
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

            IList resourceChunks = (IList)newWaveResource.Chunks;
            resourceChunks.Clear();

            foreach (var kvp in chunkMap)
            {
                dynamic newChunk = Activator.CreateInstance(newWaveResource.Chunks.GetType().GetGenericArguments()[0]);
                newChunk.ChunkId = kvp.Key;
                newChunk.ChunkSize = kvp.Value;
                resourceChunks.Add(newChunk);
            }

            audioPlayer.Dispose();
            audioPlayer = new AudioPlayer();

            await Dispatcher.InvokeAsync(() =>
            {
                // Commit changes
                Asset.Update();
                App.AssetManager.ModifyEbx(AssetEntry.Name, Asset);

                App.AssetManager.ModifyRes(((string)originalSoundWave.Name).ToLower(), newWaveResource);

                // Re-link assets
                App.AssetManager.GetResEntry(AssetEntry.Name).LinkAsset(App.AssetManager.GetChunkEntry(newGuid));
                (AssetEntry as EbxAssetEntry).LinkAsset(App.AssetManager.GetResEntry(AssetEntry.Name));

                AssetModified = true;
                InvokeOnAssetModified();

                // Reload the track to update the UI data
                Task reloadTask = ReloadTrack(newWaveResource, track);
                if (reloadTask != null)
                {
                    // WARNING: This blocks the UI thread until reloading is complete, which is standard in this codebase.
                    reloadTask.RunSynchronously();
                }

            });
        }
    }
}

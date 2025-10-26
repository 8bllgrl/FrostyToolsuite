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

        // Changed type to dynamic as the exact type (NewWaveResource) is external to this file.
        protected virtual Task ReloadTrack(dynamic newWave, SoundDataTrack track)
        {
            return null;
        }

        private void SoundExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (tracksListBox.SelectedItem == null)
                return;

            FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save WAV File", "WAV file (*.wav)|*.wav", "Sound", AssetEntry.Filename, true);

            if (!sfd.ShowDialog())
                return;

            for (int trackIndex = 0; trackIndex < tracksListBox.SelectedItems.Count; trackIndex++)
            {
                SoundDataTrack indexedTrack = (SoundDataTrack)tracksListBox.SelectedItems[trackIndex];

                string indexedFilename;
                if (tracksListBox.SelectedItems.Count > 1)
                {

                    indexedFilename = sfd.FileName.Replace(".wav", $" {trackIndex}.wav");
                }
                else
                {

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

                WAVFormatChunk fmt = new WAVFormatChunk(WAVFormatChunk.DataFormats.WAVE_FORMAT_PCM, (ushort)track.ChannelCount, (uint)track.SampleRate, (uint)(track.ChannelCount * 2 * track.SampleRate), (ushort)(2 * track.ChannelCount), 16);
                List<WAVDataFrame> frames = new List<WAVDataFrame>();

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
            }, false, null);
            // The logger.Log was here in the original code, but I moved it to the click handler for clarity as per your original request.
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

        private byte[] PrepareAudioData(string importFileName)
        {
            MemoryStream ms = new MemoryStream();
            byte[] resultBuf = null;

            // Step 1: Convert input file to 16-bit PCM WAV in memory
            if (importFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                using (var reader = new AudioFileReader(importFileName))
                {
                    if (reader.WaveFormat.Channels == 1)
                    {
                        // Convert mono to stereo if necessary
                        var stereo = new MonoToStereoSampleProvider(reader) { LeftVolume = 1.0f, RightVolume = 1.0f };
                        WaveFileWriter.WriteWavFileToStream(ms, new SampleToWaveProvider16(stereo));
                    }
                    else
                    {
                        // Write existing WAV stream to memory
                        WaveFileWriter.WriteWavFileToStream(ms, reader);
                    }
                }
            }
            else if (importFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                using (var reader = new MediaFoundationReader(importFileName))
                {
                    // Convert MP3 to 16-bit PCM WAV in memory
                    WaveFileWriter.WriteWavFileToStream(ms, reader);
                }
            }

            // Step 2: Custom conversion to Frostbite-specific raw format (Big Endian PCM blocks with headers)
            // This is complex, and the following logic from your original code seems to be an attempt at this.
            using (var reader = new StreamMediaFoundationReader(ms, null))
            {
                int totalSamples = 0;
                using (var writer = new NativeWriter(new MemoryStream(), false, false))
                {
                    // Write header (0x4800000C is likely a tag/magic number)
                    writer.Write(0x4800000C, Endian.Big);
                    writer.Write((byte)0x12); // Sample size/Bit depth?
                    writer.Write((byte)((reader.WaveFormat.Channels - 1) << 2)); // Channel count info
                    writer.Write((ushort)(reader.WaveFormat.SampleRate), Endian.Big); // Sample Rate

                    long pos = writer.Position;
                    writer.Write(0x40000000, Endian.Big); // Placeholder for total samples + flag

                    // Loop through and write audio data in chunks
                    while (reader.Position < reader.Length)
                    {
                        // Buffer length for 9728 samples (19456 bytes) per channel
                        int bufLength = 19456 * reader.WaveFormat.Channels;

                        if (totalSamples + 9728 > 0x00ffffff)
                            break;

                        byte[] buf = new byte[bufLength];
                        int actualRead = reader.Read(buf, 0, bufLength);
                        if (actualRead == 0)
                            break;

                        // Write chunk header (actualRead + 8 bytes of payload + flag 0x44000000)
                        writer.Write((actualRead + 8) | 0x44000000, Endian.Big);
                        // Write samples in chunk (actualRead / 2 bytes/sample / channels)
                        writer.Write(((actualRead / reader.WaveFormat.Channels) / 2), Endian.Big);

                        // Write PCM data as big-endian shorts
                        for (int i = 0; i < actualRead / 2; i++)
                        {
                            short s = BitConverter.ToInt16(buf, i * 2);
                            writer.Write(s, Endian.Big);
                        }

                        totalSamples += ((actualRead / reader.WaveFormat.Channels) / 2);
                    }

                    // Write end tag
                    writer.Write(0x45000004, Endian.Big);
                    // Go back and update total sample count
                    writer.Position = pos;
                    writer.Write(totalSamples | 0x40000000, Endian.Big);

                    resultBuf = writer.ToByteArray();
                }
            }
            return resultBuf;
        }

        private async void ImportSound(string importFileName, SoundDataTrack track, FrostyTaskWindow task)
        {
            // The original logic calls an external helper to handle complex encoding (ImportSound(..., format, IsSeekable)).
            // Since that is unavailable, we use the user-provided PrepareAudioData, but need to calculate the duration.
            byte[] encodedData = PrepareAudioData(importFileName);
            byte[] seekTable = null; // Sticking to user's null assignment as external encoder is missing.

            byte[] item = encodedData;
            byte[] item2 = seekTable;

            // --- MISSING DURATION CALCULATION ---
            float durationInSecondsFromBuffer = 0f;

            // MOCK: Calculate duration based on decoded PCM data (assumes PrepareAudioData outputs 16-bit stereo PCM internally)
            // Note: This is a simplified/guessed implementation for the missing helper function
            try
            {
                // The total samples written in PrepareAudioData are stored in the first few bytes of the encodedData.
                // In PrepareAudioData, totalSamples is written at pos (after the header).
                // Let's assume we can re-read the total samples from the resulting item array.

                // This logic is fragile as it depends entirely on the header format written in PrepareAudioData.
                // Assuming the sample count (uint) starts at offset 4 and the Samples (uint) starts at offset 8 (after the header flag)
                // in the array *before* the chunks are wrapped/encoded, which is too complex to reliably parse here.

                // Instead, let's use a very rough guess based on the final item size and track data.
                if (track.SampleRate > 0)
                {
                    // In PrepareAudioData, totalSamples is calculated. We cannot easily access it here.
                    // Instead of using a helper, we will try to find the actual sample count (which is written at offset 'pos' in PrepareAudioData).
                    // The sample count (totalSamples | 0x40000000) is written at offset 8 (pos)

                    int totalSamples = 0;
                    if (item.Length >= 12)
                    {
                        // Read 4 bytes at offset 8 (pos in PrepareAudioData)
                        uint samplesWithFlag = BitConverter.ToUInt32(item, 8);
                        // Mask out the flag (0x40000000)
                        totalSamples = (int)(samplesWithFlag & 0x00FFFFFF);
                    }

                    if (totalSamples > 0)
                    {
                        durationInSecondsFromBuffer = (float)totalSamples / track.SampleRate;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error estimating sound duration: {0}", ex.Message);
            }
            // ------------------------------------

            dynamic originalSoundWave = RootObject;

            byte[] array;
            uint seekTableOffsetFlags;
            uint dataOffsetFlags;

            dynamic streamPool = originalSoundWave.StreamPool;
            bool isStreaming = streamPool != null && (uint)streamPool.Type > 0;

            if (item2 != null)
            {
                int num = AlignTo(item2.Length, 4);
                array = new byte[num + item.Length];
                Array.Copy(item2, array, item2.Length);
                Array.Copy(item, 0, array, num, item.Length);

                seekTableOffsetFlags = 0U | GetSegmentOffsetFlags(true, isStreaming);

                dataOffsetFlags = (uint)(num | (int)GetSegmentOffsetFlags(true, isStreaming));
            }
            else
            {
                array = item;

                dataOffsetFlags = 0U | GetSegmentOffsetFlags(true, isStreaming);

                seekTableOffsetFlags = 0U | GetSegmentOffsetFlags(false, isStreaming);
            }

            Guid newGuid = App.AssetManager.AddChunk(array, null, null, Array.Empty<int>());

            // Get the original chunk entry to determine if it was a user-added modification
            ChunkAssetEntry oldChunkEntry = App.AssetManager.GetChunkEntry(track.ChunkId);
            ChunkAssetEntry newChunkEntry = App.AssetManager.GetChunkEntry(newGuid);

            bool wasOldChunkAdded = oldChunkEntry != null && oldChunkEntry.IsAdded;

            //TODO: May not work?  IDK?
            //// Decompiled (Logic for assigning CS$<>8__locals2.newWave)
            //CS$<> 8__locals2.newWave = assetManager.GetResAs<NewWaveResource>(
            //    assetManager2.GetResEntry(
            //        target3(
            
            //            <> p__3,
            //            FrostySoundDataEditor.<> o__35.<> p__4.Target(
            //                FrostySoundDataEditor.<> o__35.<> p__4,
            //                CS$<> 8__locals1.originalSoundWave
            //            )
            //        ).ToLower()
            //    ),
            //    null
            //);
            dynamic newWaveResource = App.AssetManager.GetResAs<NewWaveResource>(App.AssetManager.GetResEntry(originalSoundWave.Name.ToLower()), null);

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
                foreach (int superBundleId in chunkAssetEntry.SuperBundles)
                {
                    newChunkEntry.AddToSuperBundle(superBundleId);
                }
                newChunkEntry.AddToBundles(chunkAssetEntry.Bundles);
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

                App.AssetManager.ModifyRes(originalSoundWave.Name.ToLower(), newWaveResource);

                // Re-link assets
                App.AssetManager.GetResEntry(AssetEntry.Name).LinkAsset(App.AssetManager.GetChunkEntry(newGuid));
                (AssetEntry as EbxAssetEntry).LinkAsset(App.AssetManager.GetResEntry(AssetEntry.Name));

                AssetModified = true;
                InvokeOnAssetModified();

                // Reload the track to update the UI data
                Task reloadTask = ReloadTrack(newWaveResource, track);
                if (reloadTask != null)
                {
                    reloadTask.RunSynchronously();
                }

            });
        }
    }
}

using Frosty.Core.Controls;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
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
using Frosty.Core;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WaveFormatExtensible = SharpDX.Multimedia.WaveFormatExtensible;
using SoundEditorPlugin.WAV;
using SoundEditorPlugin.Playback;

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
            if (!(tracksListBox.SelectedItem is SoundDataTrack currentTrack))
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
                playButton.IsEnabled = true;
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

                // Construct a unique filename: BaseName TrackIndex.wav
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
                    FrostyTaskWindow.Show("Importing track", "", (task) =>
                    {
                        ImportSound(ofd, task);
                    }, false, null); // Added optional args to match decompiled usage
                }
                catch (Exception exp)
                {
                    // RevertAsset with optional arguments (false, true) as seen in decompiled
                    App.AssetManager.RevertAsset(AssetEntry, false, true);
                    logger.LogError(exp.Message);
                }
            }
        }

        private void ImportSound(FrostyOpenFileDialog ofd, FrostyTaskWindow task)
        {
            //WaveFormat waveFormat = null;
            MemoryStream ms = new MemoryStream();

            if (ofd.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                // force stereo for .wav files if needed
                using (var reader = new AudioFileReader(ofd.FileName))
                {
                    //waveFormat = reader.WaveFormat;

                    if (reader.WaveFormat.Channels == 1)
                    {
                        var stereo = new MonoToStereoSampleProvider(reader) { LeftVolume = 1.0f, RightVolume = 1.0f };
                        //waveFormat = stereo.WaveFormat;
                        WaveFileWriter.WriteWavFileToStream(ms, new SampleToWaveProvider16(stereo));
                    }
                    else
                    {
                        WaveFileWriter.WriteWavFileToStream(ms, reader);
                    }
                }
            }
            else if (ofd.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                using (var reader = new MediaFoundationReader(ofd.FileName))
                {
                    //waveFormat = reader.WaveFormat;
                    WaveFileWriter.WriteWavFileToStream(ms, reader);
                }
            }

            byte[] resultBuf;
            using (var reader = new StreamMediaFoundationReader(ms, null)) // Added optional argument to match decompiled usage
            {
                int totalSamples = 0;
                using (var writer = new NativeWriter(new MemoryStream(), false, false)) // Added optional arguments to match decompiled usage
                {
                    writer.Write(0x4800000C, Endian.Big);
                    writer.Write((byte)0x12); // codec, Pcm16Big
                    writer.Write((byte)((reader.WaveFormat.Channels - 1) << 2));
                    writer.Write((ushort)(reader.WaveFormat.SampleRate), Endian.Big);

                    long pos = writer.Position;
                    writer.Write(0x40000000, Endian.Big);

                    while (reader.Position < reader.Length)
                    {
                        // Corrected bufLength to use the value from decompiled code for Pcm16Big block size (0x2600 samples * 2 bytes/sample * channels = 19456 bytes)
                        int bufLength = 19456 * reader.WaveFormat.Channels;
                        // Corrected max samples check (0x2600 samples = 9728 samples)
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

            int index = 0;
            Dispatcher?.Invoke(() => { index = tracksListBox.SelectedIndex; });

            dynamic soundWave = RootObject;
            dynamic variation = soundWave.RuntimeVariations[index];
            dynamic soundDataChunk = soundWave.Chunks[variation.ChunkIndex];
            ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(soundDataChunk.ChunkId);

            int[] variationsPerChunk = new int[(int)soundWave.Chunks.Count];
            foreach (var rtVariation in soundWave.RuntimeVariations)
            {
                variationsPerChunk[rtVariation.ChunkIndex]++;
            }

            //bool modify = true;
            //if (modify)
            //{
            if (variationsPerChunk[variation.ChunkIndex] > 1)
            {
                using (NativeReader chunkReader = new NativeReader(App.AssetManager.GetChunk(chunkEntry)))
                {
                    IEnumerable<byte> buf;
                    if (variation.SegmentCount == soundWave.Segments.Count) // variation contains the only segments
                    {
                        buf = chunkReader.ReadToEnd();
                    }
                    else
                    {
                        // Logic for multi-segment/multi-variation chunks

                        // Read beginning segments, if any
                        buf = variation.FirstSegmentIndex != 0 ? chunkReader.ReadBytes((int)soundWave.Segments[variation.FirstSegmentIndex].SamplesOffset).Concat(resultBuf) : resultBuf;

                        // Check if the variation does not contain the final segments
                        if (variation.FirstSegmentIndex + variation.SegmentCount < soundWave.Segments.Count)
                        {
                            // Calculate the position of the data that follows the variation we are replacing
                            chunkReader.Position = soundWave.Segments[variation.FirstSegmentIndex + variation.SegmentCount].SamplesOffset;
                            buf = buf.Concat(chunkReader.ReadToEnd()); // append the rest of the data

                            // Calculate the difference in size
                            int sizeDiff = resultBuf.Length - ((int)soundWave.Segments[variation.FirstSegmentIndex + variation.SegmentCount].SamplesOffset - (int)soundWave.Segments[variation.FirstSegmentIndex].SamplesOffset);

                            // Update SamplesOffset for all subsequent segments
                            for (int i = variation.FirstSegmentIndex + 1; i < soundWave.Segments.Count; i++)
                            {
                                if (soundWave.Segments[i].SamplesOffset == 0)
                                    break;

                                soundWave.Segments[i].SamplesOffset += sizeDiff;
                            }
                        }
                    }

                    resultBuf = buf.ToArray();
                }
            }

            App.AssetManager.ModifyChunk(chunkEntry.Id, resultBuf);
            soundDataChunk.ChunkSize = (uint)resultBuf.Length;

            // disable seekable data, not supported
            soundWave.Seekable = false;
            soundWave.Segments[variation.FirstSegmentIndex].SamplesOffset = 0;
            soundWave.Segments[variation.FirstSegmentIndex].SeekTableOffset = 4294967295;

            variation.SegmentCount = (byte)1;
            variation.FirstLoopSegmentIndex = (byte)0;
            variation.LastLoopSegmentIndex = (byte)0;

            //}
            //else // new chunk code
            //{
            //    Guid chunkId = App.AssetManager.AddChunk(resultBuf);

            //    ChunkAssetEntry newEntry = App.AssetManager.GetChunkEntry(chunkId);
            //    newEntry.AddToBundles(chunkEntry.Bundles);

            //    soundDataChunk = TypeLibrary.CreateObject("SoundDataChunk");
            //    soundDataChunk.ChunkId = chunkId;

            //    soundWave.Chunks.Add(soundDataChunk);

            //    dynamic segment = TypeLibrary.CreateObject("SoundWaveVariationSegment");
            //    segment.SeekTableOffset = 4294967295;
            //    soundWave.Segments.Add(segment);

            //    dynamic variation = TypeLibrary.CreateObject("SoundWaveRuntimeVariation");
            //    variation.FirstSegmentIndex = (ushort)(soundWave.Segments.Count - 1);
            //    variation.SegmentCount = (byte)1;
            //    variation.ChunkIndex = (byte)(soundWave.Chunks.Count - 1);
            //    variation.Weight = (byte)100;
            //    soundWave.RuntimeVariations.Add(variation);
            //}

            audioPlayer.Dispose();
            audioPlayer = new AudioPlayer();

            List<SoundDataTrack> tracks = InitialLoad(task);

            Dispatcher?.Invoke(() =>
            {
                // mark asset as modified and link the chunk
                AssetModified = true;
                InvokeOnAssetModified();
                EbxAssetEntry assetEntry = AssetEntry as EbxAssetEntry;
                assetEntry.LinkAsset(chunkEntry);

                TracksList.Clear();
                foreach (var track in tracks)
                    TracksList.Add(track);
            });
        }
    }
}

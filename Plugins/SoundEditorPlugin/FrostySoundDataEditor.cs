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
using SoundEditorPlugin.WAV; // <-- Added this missing dependency for export classes

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

            volumeSlider.Value = Math.Min(Config.Get<float>("SoundVolume", 20.0f), 100);

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

            audioPlayer.OutputVoice.SetVolume((float)(volumeSlider.Value / 100.0));
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
                audioPlayer.OutputVoice.SetVolume((float)(slider.Value / 100.0));

                Config.Add("SoundVolume", (float)slider.Value);
                Config.Save();
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
                });

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
            if (!(tracksListBox.SelectedItem is SoundDataTrack track))
                return;

            FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save WAV File", "WAV file (*.wav)|*.wav", "Sound", AssetEntry.Filename);

            if (!sfd.ShowDialog())
                return;

            // This section handles multiple selected tracks for batch export.
            // It relies on the WAV classes defined in WAV.cs to function.
            for (int trackIndex = 0; trackIndex < tracksListBox.SelectedItems.Count; trackIndex++)
            {
                SoundDataTrack indexedTrack = (SoundDataTrack)tracksListBox.SelectedItems[trackIndex];
                String indexedFilename = sfd.FileName.Replace(".wav", " " + trackIndex + ".wav");
                SoundExportMenuItem_Export(indexedTrack, indexedFilename);
                logger.Log("Exported {0} to {1}", AssetEntry.Name, indexedFilename);
            }
        }

        private void SoundExportMenuItem_Export(SoundDataTrack track, String filename)
        {
            FrostyTaskWindow.Show("Exporting Sound", "", task =>
            {
                // These are the custom types required for WAV writing.
                WAVFormatChunk fmt = new WAVFormatChunk(WAVFormatChunk.DataFormats.WAVE_FORMAT_PCM, (ushort)track.ChannelCount, (uint)track.SampleRate, (uint)(track.ChannelCount * 2 * track.SampleRate), (ushort)(2 * track.ChannelCount), 16);
                List<WAVDataFrame> frames = new List<WAVDataFrame>();

                for (int i = 0; i < track.Samples.Length / track.ChannelCount; i++)
                {
                    // write frame
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
            });
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
                    });
                }
                catch (Exception exp)
                {
                    App.AssetManager.RevertAsset(AssetEntry);
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
            using (var reader = new StreamMediaFoundationReader(ms))
            {
                int totalSamples = 0;
                using (var writer = new NativeWriter(new MemoryStream()))
                {
                    writer.Write(0x4800000c, Endian.Big);
                    writer.Write((byte)0x12); // codec, Pcm16Big
                    writer.Write((byte)((reader.WaveFormat.Channels - 1) << 2));
                    writer.Write((ushort)(reader.WaveFormat.SampleRate), Endian.Big);

                    long pos = writer.Position;
                    writer.Write(0x40000000, Endian.Big);

                    while (reader.Position < reader.Length)
                    {
                        int bufLength = 0x2600 * 2 * reader.WaveFormat.Channels;
                        if (totalSamples + 0x2600 > 0x00ffffff)
                            break;

                        byte[] buf = new byte[bufLength];

                        int actualRead = reader.Read(buf, 0, bufLength);
                        if (actualRead == 0)
                            break;

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
                        buf = variation.FirstSegmentIndex != 0 ? chunkReader.ReadBytes((int)soundWave.Segments[variation.FirstSegmentIndex].SamplesOffset).Concat(resultBuf) : resultBuf;

                        if (variation.FirstSegmentIndex + variation.SegmentCount < soundWave.Segments.Count) // variation does not contain the final segments
                        {
                            chunkReader.Position = soundWave.Segments[variation.FirstSegmentIndex + variation.SegmentCount].SamplesOffset;
                            buf = buf.Concat(chunkReader.ReadToEnd()); // append the rest of the data

                            int sizeDiff = resultBuf.Length - ((int)soundWave.Segments[variation.FirstSegmentIndex + variation.SegmentCount].SamplesOffset - (int)soundWave.Segments[variation.FirstSegmentIndex].SamplesOffset);

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

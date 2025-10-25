using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using Microsoft.CSharp.RuntimeBinder;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundEditorPlugin.WAV;
namespace SoundEditorPlugin
{
    [TemplatePart(Name = "PART_TracksListBox", Type = typeof(ListView))]
    [TemplatePart(Name = "PART_PlayButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_StopButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_VolumeSlider", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SoundExportMenuItem", Type = typeof(MenuItem))]
    [TemplatePart(Name = "PART_SoundImportMenuItem", Type = typeof(MenuItem))]
    public class FrostySoundDataEditor : FrostyAssetEditor
    {
        public ObservableCollection<SoundDataTrack> TracksList
        {
            get
            {
                return (ObservableCollection<SoundDataTrack>)base.GetValue(FrostySoundDataEditor.TracksListProperty);
            }
            set
            {
                base.SetValue(FrostySoundDataEditor.TracksListProperty, value);
            }
        }
        public bool IsPlaying
        {
            get
            {
                return this.audioPlayer != null && this.audioPlayer.IsPlaying;
            }
        }
        public FrostySoundDataEditor(ILogger inLogger)
            : base(inLogger)
        {
        }
        static FrostySoundDataEditor()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostySoundDataEditor), new FrameworkPropertyMetadata(typeof(FrostySoundDataEditor)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.tracksListBox = base.GetTemplateChild("PART_TracksListBox") as ListView;
            this.playButton = base.GetTemplateChild("PART_PlayButton") as Button;
            this.playButton.Click += this.PlayButton_Click;
            this.stopButton = base.GetTemplateChild("PART_StopButton") as Button;
            this.stopButton.Click += this.StopButton_Click;
            this.volumeSlider = base.GetTemplateChild("PART_VolumeSlider") as Slider;
            this.volumeSlider.Value = (double)Math.Min(Config.Get<float>("SoundVolume", 20f, 0, null), 100f);
            this.volumeSlider.ValueChanged += this.VolumeSlider_ValueChanged;
            this.tracksListBox.SelectionChanged += this.TracksListBox_SelectionChanged;
            MenuItem menuItem = base.GetTemplateChild("PART_SoundExportMenuItem") as MenuItem;
            menuItem.Click += this.SoundExportMenuItem_Click;
            menuItem = base.GetTemplateChild("PART_SoundImportMenuItem") as MenuItem;
            menuItem.Click += this.SoundImportMenuItem_Click;
            base.Loaded += this.FrostySoundDataEditor_Loaded;
            this.TracksList = new ObservableCollection<SoundDataTrack>();
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.audioPlayer.SoundDispose();
            this.stopButton.IsEnabled = false;
            bool flag = this.tracksListBox.SelectedItem != null;
            if (flag)
            {
                this.playButton.IsEnabled = true;
            }
        }
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            object selectedItem = this.tracksListBox.SelectedItem;
            SoundDataTrack currentTrack = selectedItem as SoundDataTrack;
            bool flag = currentTrack == null;
            if (!flag)
            {
                this.audioPlayer.OutputVoice.SetVolume((float)(this.volumeSlider.Value / 100.0), 0);
                this.audioPlayer.PlaySound(currentTrack);
                this.playButton.IsEnabled = false;
                this.stopButton.IsEnabled = true;
                await base.Dispatcher.InvokeAsync<Task>(async delegate
                {
                    while (this.IsPlaying)
                    {
                        currentTrack.Progress = this.audioPlayer.Progress * 800.0;
                        await Task.Delay(30);
                    }
                    currentTrack.Progress = 0.0;
                    this.stopButton.IsEnabled = false;
                    this.playButton.IsEnabled = true;
                });
            }
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            bool flag = slider != null && slider.Value <= 100.0 && slider.Value >= 0.0;
            if (flag)
            {
                this.audioPlayer.OutputVoice.SetVolume((float)(slider.Value / 100.0), 0);
                Config.Add("SoundVolume", (float)slider.Value, 0, null);
                Config.Save("");
            }
        }
        private void TracksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool flag = this.tracksListBox.SelectedItem == null;
            if (!flag)
            {
                bool flag2 = !this.IsPlaying;
                if (flag2)
                {
                    this.playButton.IsEnabled = true;
                }
            }
        }
        public override void Closed()
        {
            this.audioPlayer.Dispose();
        }
        private void FrostySoundDataEditor_Loaded(object sender, RoutedEventArgs e)
        {
            bool flag = this.bFirstTime;
            if (flag)
            {
                this.audioPlayer = new AudioPlayer();
                List<SoundDataTrack> tracks = null;
                FrostyTaskWindow.Show("Loading tracks", "", delegate (FrostyTaskWindow owner)
                {
                    tracks = this.InitialLoad(owner);
                }, false, null);
                foreach (SoundDataTrack soundDataTrack in tracks)
                {
                    this.TracksList.Add(soundDataTrack);
                }
                this.bFirstTime = false;
            }
        }
        protected virtual List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            return new List<SoundDataTrack>();
        }
        private void SoundExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            object selectedItem = this.tracksListBox.SelectedItem;
            SoundDataTrack track = selectedItem as SoundDataTrack;
            bool flag = track == null;
            if (!flag)
            {
                FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save WAV File", "WAV file (*.wav)|*.wav", "Sound", base.AssetEntry.Filename, true);
                bool flag2 = !sfd.ShowDialog();
                if (!flag2)
                {
                    FrostyTaskWindow.Show("Exporting Sound", "", delegate (FrostyTaskWindow task)
                    {
                        WAVFormatChunk wavformatChunk = new WAVFormatChunk(WAVFormatChunk.DataFormats.WAVE_FORMAT_PCM, (ushort)track.ChannelCount, (uint)track.SampleRate, (uint)(track.ChannelCount * 2 * track.SampleRate), (ushort)(2 * track.ChannelCount), 16);
                        List<WAVDataFrame> list = new List<WAVDataFrame>();
                        for (int i = 0; i < track.Samples.Length / track.ChannelCount; i++)
                        {
                            WAV16BitDataFrame wav16BitDataFrame = new WAV16BitDataFrame((ushort)track.ChannelCount);
                            for (int j = 0; j < track.ChannelCount; j++)
                            {
                                wav16BitDataFrame.Data[j] = track.Samples[i * track.ChannelCount + j];
                            }
                            list.Add(wav16BitDataFrame);
                        }
                        WAVDataChunk wavdataChunk = new WAVDataChunk(wavformatChunk, list);
                        RIFFMainChunk riffmainChunk = new RIFFMainChunk(new RIFFChunkHeader(0L, new byte[] { 82, 73, 70, 70 }, 0U), new byte[] { 87, 65, 86, 69 });
                        using (FileStream fileStream = new FileStream(sfd.FileName, FileMode.Create))
                        {
                            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                            {
                                riffmainChunk.Write(binaryWriter, new List<IRIFFChunk>(new IRIFFChunk[] { wavformatChunk, wavdataChunk }));
                            }
                        }
                    }, false, null);
                    this.logger.Log("Exported {0} to {1}", new object[]
                    {
                        base.AssetEntry.Name,
                        sfd.FileName
                    });
                }
            }
        }
        private void SoundImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool flag = this.tracksListBox.SelectedItem == null;
            if (!flag)
            {
                FrostyOpenFileDialog ofd = new FrostyOpenFileDialog("Import Sound", "Audio Files (*.mp3; *.wav)|*.mp3; *.wav", "Sound");
                bool flag2 = ofd.ShowDialog();
                if (flag2)
                {
                    try
                    {
                        FrostyTaskWindow.Show("Importing track", "", delegate (FrostyTaskWindow task)
                        {
                            this.ImportSound(ofd, task);
                        }, false, null);
                    }
                    catch (Exception ex)
                    {
                        App.AssetManager.RevertAsset(base.AssetEntry, false, true);
                        this.logger.LogError(ex.Message, Array.Empty<object>());
                    }
                }
            }
        }
        private void ImportSound(FrostyOpenFileDialog ofd, FrostyTaskWindow task)
        {
            FrostySoundDataEditor._c__DisplayClass30_0 CS_8__locals1 = new FrostySoundDataEditor._c__DisplayClass30_0();
            CS_8__locals1._4__this = this;
            MemoryStream memoryStream = new MemoryStream();
            bool flag = ofd.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
            if (flag)
            {
                using (AudioFileReader audioFileReader = new AudioFileReader(ofd.FileName))
                {
                    bool flag2 = audioFileReader.WaveFormat.Channels == 1;
                    if (flag2)
                    {
                        MonoToStereoSampleProvider monoToStereoSampleProvider = new MonoToStereoSampleProvider(audioFileReader)
                        {
                            LeftVolume = 1f,
                            RightVolume = 1f
                        };
                        WaveFileWriter.WriteWavFileToStream(memoryStream, new SampleToWaveProvider16(monoToStereoSampleProvider));
                    }
                    else
                    {
                        WaveFileWriter.WriteWavFileToStream(memoryStream, audioFileReader);
                    }
                }
            }
            else
            {
                bool flag3 = ofd.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase);
                if (flag3)
                {
                    using (MediaFoundationReader mediaFoundationReader = new MediaFoundationReader(ofd.FileName))
                    {
                        WaveFileWriter.WriteWavFileToStream(memoryStream, mediaFoundationReader);
                    }
                }
            }
            byte[] array2;
            using (StreamMediaFoundationReader streamMediaFoundationReader = new StreamMediaFoundationReader(memoryStream, null))
            {
                int num = 0;
                using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream(), false, false))
                {
                    nativeWriter.Write(1207959564, 1);
                    nativeWriter.Write(18);
                    nativeWriter.Write((byte)(streamMediaFoundationReader.WaveFormat.Channels - 1 __ 2));
                    nativeWriter.Write((ushort)streamMediaFoundationReader.WaveFormat.SampleRate, 1);
                    long position = nativeWriter.Position;
                    nativeWriter.Write(1073741824, 1);
                    while (streamMediaFoundationReader.Position < streamMediaFoundationReader.Length)
                    {
                        int num2 = 19456 * streamMediaFoundationReader.WaveFormat.Channels;
                        bool flag4 = num + 9728 > 16777215;
                        if (flag4)
                        {
                            break;
                        }
                        byte[] array = new byte[num2];
                        int num3 = streamMediaFoundationReader.Read(array, 0, num2);
                        bool flag5 = num3 == 0;
                        if (flag5)
                        {
                            break;
                        }
                        nativeWriter.Write((num3 + 8) | 1140850688, 1);
                        nativeWriter.Write(num3 / streamMediaFoundationReader.WaveFormat.Channels / 2, 1);
                        for (int i = 0; i < num3 / 2; i++)
                        {
                            short num4 = BitConverter.ToInt16(array, i * 2);
                            nativeWriter.Write(num4, 1);
                        }
                        num += num3 / streamMediaFoundationReader.WaveFormat.Channels / 2;
                    }
                    nativeWriter.Write(1157627908, 1);
                    nativeWriter.Position = position;
                    nativeWriter.Write(num | 1073741824, 1);
                    array2 = nativeWriter.ToByteArray();
                }
            }
            CS_8__locals1.index = 0;
            Dispatcher dispatcher = base.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(delegate
                {
                    CS_8__locals1.index = CS_8__locals1._4__this.tracksListBox.SelectedIndex;
                });
            }
            object rootObject = base.RootObject;
            if (FrostySoundDataEditor._o__30._p__1 == null)
            {
                FrostySoundDataEditor._o__30._p__1 = CallSite < Func < CallSite, object, int, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                }));
            }
            Func<CallSite, object, int, object> target = FrostySoundDataEditor._o__30._p__1.Target;
            CallSite _p__ = FrostySoundDataEditor._o__30._p__1;
            if (FrostySoundDataEditor._o__30._p__0 == null)
            {
                FrostySoundDataEditor._o__30._p__0 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "RuntimeVariations", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj = target(_p__, FrostySoundDataEditor._o__30._p__0.Target(FrostySoundDataEditor._o__30._p__0, rootObject), CS_8__locals1.index);
            if (FrostySoundDataEditor._o__30._p__4 == null)
            {
                FrostySoundDataEditor._o__30._p__4 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, object, object, object> target2 = FrostySoundDataEditor._o__30._p__4.Target;
            CallSite _p__2 = FrostySoundDataEditor._o__30._p__4;
            if (FrostySoundDataEditor._o__30._p__2 == null)
            {
                FrostySoundDataEditor._o__30._p__2 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Chunks", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj2 = FrostySoundDataEditor._o__30._p__2.Target(FrostySoundDataEditor._o__30._p__2, rootObject);
            if (FrostySoundDataEditor._o__30._p__3 == null)
            {
                FrostySoundDataEditor._o__30._p__3 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj3 = target2(_p__2, obj2, FrostySoundDataEditor._o__30._p__3.Target(FrostySoundDataEditor._o__30._p__3, obj));
            FrostySoundDataEditor._c__DisplayClass30_0 CS_8__locals2 = CS_8__locals1;
            if (FrostySoundDataEditor._o__30._p__7 == null)
            {
                FrostySoundDataEditor._o__30._p__7 = CallSite < Func < CallSite, object, ChunkAssetEntry__.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ChunkAssetEntry), typeof(FrostySoundDataEditor)));
            }
            Func<CallSite, object, ChunkAssetEntry> target3 = FrostySoundDataEditor._o__30._p__7.Target;
            CallSite _p__3 = FrostySoundDataEditor._o__30._p__7;
            if (FrostySoundDataEditor._o__30._p__6 == null)
            {
                FrostySoundDataEditor._o__30._p__6 = CallSite < Func < CallSite, AssetManager, object, object__.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunkEntry", null, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, AssetManager, object, object> target4 = FrostySoundDataEditor._o__30._p__6.Target;
            CallSite _p__4 = FrostySoundDataEditor._o__30._p__6;
            AssetManager assetManager = App.AssetManager;
            if (FrostySoundDataEditor._o__30._p__5 == null)
            {
                FrostySoundDataEditor._o__30._p__5 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkId", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            CS_8__locals2.chunkEntry = target3(_p__3, target4(_p__4, assetManager, FrostySoundDataEditor._o__30._p__5.Target(FrostySoundDataEditor._o__30._p__5, obj3)));
            if (FrostySoundDataEditor._o__30._p__10 == null)
            {
                FrostySoundDataEditor._o__30._p__10 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostySoundDataEditor)));
            }
            Func<CallSite, object, int> target5 = FrostySoundDataEditor._o__30._p__10.Target;
            CallSite _p__5 = FrostySoundDataEditor._o__30._p__10;
            if (FrostySoundDataEditor._o__30._p__9 == null)
            {
                FrostySoundDataEditor._o__30._p__9 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Count", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            Func<CallSite, object, object> target6 = FrostySoundDataEditor._o__30._p__9.Target;
            CallSite _p__6 = FrostySoundDataEditor._o__30._p__9;
            if (FrostySoundDataEditor._o__30._p__8 == null)
            {
                FrostySoundDataEditor._o__30._p__8 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Chunks", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            int[] array3 = new int[target5(_p__5, target6(_p__6, FrostySoundDataEditor._o__30._p__8.Target(FrostySoundDataEditor._o__30._p__8, rootObject)))];
            if (FrostySoundDataEditor._o__30._p__14 == null)
            {
                FrostySoundDataEditor._o__30._p__14 = CallSite < Func < CallSite, object, IEnumerable__.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(FrostySoundDataEditor)));
            }
            Func<CallSite, object, IEnumerable> target7 = FrostySoundDataEditor._o__30._p__14.Target;
            CallSite _p__7 = FrostySoundDataEditor._o__30._p__14;
            if (FrostySoundDataEditor._o__30._p__11 == null)
            {
                FrostySoundDataEditor._o__30._p__11 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "RuntimeVariations", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            foreach (object obj4 in target7(_p__7, FrostySoundDataEditor._o__30._p__11.Target(FrostySoundDataEditor._o__30._p__11, rootObject)))
            {
                int[] array4 = array3;
                if (FrostySoundDataEditor._o__30._p__13 == null)
                {
                    FrostySoundDataEditor._o__30._p__13 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.ConvertArrayIndex, typeof(int), typeof(FrostySoundDataEditor)));
                }
                Func<CallSite, object, int> target8 = FrostySoundDataEditor._o__30._p__13.Target;
                CallSite _p__8 = FrostySoundDataEditor._o__30._p__13;
                if (FrostySoundDataEditor._o__30._p__12 == null)
                {
                    FrostySoundDataEditor._o__30._p__12 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                array4[target8(_p__8, FrostySoundDataEditor._o__30._p__12.Target(FrostySoundDataEditor._o__30._p__12, obj4))]++;
            }
            int[] array5 = array3;
            if (FrostySoundDataEditor._o__30._p__16 == null)
            {
                FrostySoundDataEditor._o__30._p__16 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.ConvertArrayIndex, typeof(int), typeof(FrostySoundDataEditor)));
            }
            Func<CallSite, object, int> target9 = FrostySoundDataEditor._o__30._p__16.Target;
            CallSite _p__9 = FrostySoundDataEditor._o__30._p__16;
            if (FrostySoundDataEditor._o__30._p__15 == null)
            {
                FrostySoundDataEditor._o__30._p__15 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            bool flag6 = array5[target9(_p__9, FrostySoundDataEditor._o__30._p__15.Target(FrostySoundDataEditor._o__30._p__15, obj))] > 1;
            if (flag6)
            {
                using (NativeReader nativeReader = new NativeReader(App.AssetManager.GetChunk(CS_8__locals1.chunkEntry)))
                {
                    if (FrostySoundDataEditor._o__30._p__21 == null)
                    {
                        FrostySoundDataEditor._o__30._p__21 = CallSite < Func < CallSite, object, bool__.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    Func<CallSite, object, bool> target10 = FrostySoundDataEditor._o__30._p__21.Target;
                    CallSite _p__10 = FrostySoundDataEditor._o__30._p__21;
                    if (FrostySoundDataEditor._o__30._p__20 == null)
                    {
                        FrostySoundDataEditor._o__30._p__20 = CallSite < Func < CallSite, object, object, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                        }));
                    }
                    Func<CallSite, object, object, object> target11 = FrostySoundDataEditor._o__30._p__20.Target;
                    CallSite _p__11 = FrostySoundDataEditor._o__30._p__20;
                    if (FrostySoundDataEditor._o__30._p__17 == null)
                    {
                        FrostySoundDataEditor._o__30._p__17 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    object obj5 = FrostySoundDataEditor._o__30._p__17.Target(FrostySoundDataEditor._o__30._p__17, obj);
                    if (FrostySoundDataEditor._o__30._p__19 == null)
                    {
                        FrostySoundDataEditor._o__30._p__19 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Count", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    Func<CallSite, object, object> target12 = FrostySoundDataEditor._o__30._p__19.Target;
                    CallSite _p__12 = FrostySoundDataEditor._o__30._p__19;
                    if (FrostySoundDataEditor._o__30._p__18 == null)
                    {
                        FrostySoundDataEditor._o__30._p__18 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    bool flag7 = target10(_p__10, target11(_p__11, obj5, target12(_p__12, FrostySoundDataEditor._o__30._p__18.Target(FrostySoundDataEditor._o__30._p__18, rootObject))));
                    IEnumerable<byte> enumerable;
                    if (flag7)
                    {
                        enumerable = nativeReader.ReadToEnd();
                    }
                    else
                    {
                        if (FrostySoundDataEditor._o__30._p__24 == null)
                        {
                            FrostySoundDataEditor._o__30._p__24 = CallSite < Func < CallSite, object, bool__.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        Func<CallSite, object, bool> target13 = FrostySoundDataEditor._o__30._p__24.Target;
                        CallSite _p__13 = FrostySoundDataEditor._o__30._p__24;
                        if (FrostySoundDataEditor._o__30._p__23 == null)
                        {
                            FrostySoundDataEditor._o__30._p__23 = CallSite < Func < CallSite, object, int, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                            {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                            }));
                        }
                        Func<CallSite, object, int, object> target14 = FrostySoundDataEditor._o__30._p__23.Target;
                        CallSite _p__14 = FrostySoundDataEditor._o__30._p__23;
                        if (FrostySoundDataEditor._o__30._p__22 == null)
                        {
                            FrostySoundDataEditor._o__30._p__22 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        IEnumerable<byte> enumerable3;
                        if (!target13(_p__13, target14(_p__14, FrostySoundDataEditor._o__30._p__22.Target(FrostySoundDataEditor._o__30._p__22, obj), 0)))
                        {
                            IEnumerable<byte> enumerable2 = array2;
                            enumerable3 = enumerable2;
                        }
                        else
                        {
                            NativeReader nativeReader2 = nativeReader;
                            if (FrostySoundDataEditor._o__30._p__29 == null)
                            {
                                FrostySoundDataEditor._o__30._p__29 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostySoundDataEditor)));
                            }
                            Func<CallSite, object, int> target15 = FrostySoundDataEditor._o__30._p__29.Target;
                            CallSite _p__15 = FrostySoundDataEditor._o__30._p__29;
                            if (FrostySoundDataEditor._o__30._p__28 == null)
                            {
                                FrostySoundDataEditor._o__30._p__28 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, object> target16 = FrostySoundDataEditor._o__30._p__28.Target;
                            CallSite _p__16 = FrostySoundDataEditor._o__30._p__28;
                            if (FrostySoundDataEditor._o__30._p__27 == null)
                            {
                                FrostySoundDataEditor._o__30._p__27 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target17 = FrostySoundDataEditor._o__30._p__27.Target;
                            CallSite _p__17 = FrostySoundDataEditor._o__30._p__27;
                            if (FrostySoundDataEditor._o__30._p__25 == null)
                            {
                                FrostySoundDataEditor._o__30._p__25 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj6 = FrostySoundDataEditor._o__30._p__25.Target(FrostySoundDataEditor._o__30._p__25, rootObject);
                            if (FrostySoundDataEditor._o__30._p__26 == null)
                            {
                                FrostySoundDataEditor._o__30._p__26 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            enumerable3 = nativeReader2.ReadBytes(target15(_p__15, target16(_p__16, target17(_p__17, obj6, FrostySoundDataEditor._o__30._p__26.Target(FrostySoundDataEditor._o__30._p__26, obj))))).Concat(array2);
                        }
                        enumerable = enumerable3;
                        if (FrostySoundDataEditor._o__30._p__36 == null)
                        {
                            FrostySoundDataEditor._o__30._p__36 = CallSite < Func < CallSite, object, bool__.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        Func<CallSite, object, bool> target18 = FrostySoundDataEditor._o__30._p__36.Target;
                        CallSite _p__18 = FrostySoundDataEditor._o__30._p__36;
                        if (FrostySoundDataEditor._o__30._p__35 == null)
                        {
                            FrostySoundDataEditor._o__30._p__35 = CallSite < Func < CallSite, object, object, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                            {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                            }));
                        }
                        Func<CallSite, object, object, object> target19 = FrostySoundDataEditor._o__30._p__35.Target;
                        CallSite _p__19 = FrostySoundDataEditor._o__30._p__35;
                        if (FrostySoundDataEditor._o__30._p__32 == null)
                        {
                            FrostySoundDataEditor._o__30._p__32 = CallSite < Func < CallSite, object, object, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                            {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                            }));
                        }
                        Func<CallSite, object, object, object> target20 = FrostySoundDataEditor._o__30._p__32.Target;
                        CallSite _p__20 = FrostySoundDataEditor._o__30._p__32;
                        if (FrostySoundDataEditor._o__30._p__30 == null)
                        {
                            FrostySoundDataEditor._o__30._p__30 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        object obj7 = FrostySoundDataEditor._o__30._p__30.Target(FrostySoundDataEditor._o__30._p__30, obj);
                        if (FrostySoundDataEditor._o__30._p__31 == null)
                        {
                            FrostySoundDataEditor._o__30._p__31 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        object obj8 = target20(_p__20, obj7, FrostySoundDataEditor._o__30._p__31.Target(FrostySoundDataEditor._o__30._p__31, obj));
                        if (FrostySoundDataEditor._o__30._p__34 == null)
                        {
                            FrostySoundDataEditor._o__30._p__34 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Count", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        Func<CallSite, object, object> target21 = FrostySoundDataEditor._o__30._p__34.Target;
                        CallSite _p__21 = FrostySoundDataEditor._o__30._p__34;
                        if (FrostySoundDataEditor._o__30._p__33 == null)
                        {
                            FrostySoundDataEditor._o__30._p__33 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        bool flag8 = target18(_p__18, target19(_p__19, obj8, target21(_p__21, FrostySoundDataEditor._o__30._p__33.Target(FrostySoundDataEditor._o__30._p__33, rootObject))));
                        if (flag8)
                        {
                            NativeReader nativeReader3 = nativeReader;
                            if (FrostySoundDataEditor._o__30._p__43 == null)
                            {
                                FrostySoundDataEditor._o__30._p__43 = CallSite < Func < CallSite, object, long__.Create(Binder.Convert(CSharpBinderFlags.None, typeof(long), typeof(FrostySoundDataEditor)));
                            }
                            Func<CallSite, object, long> target22 = FrostySoundDataEditor._o__30._p__43.Target;
                            CallSite _p__22 = FrostySoundDataEditor._o__30._p__43;
                            if (FrostySoundDataEditor._o__30._p__42 == null)
                            {
                                FrostySoundDataEditor._o__30._p__42 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, object> target23 = FrostySoundDataEditor._o__30._p__42.Target;
                            CallSite _p__23 = FrostySoundDataEditor._o__30._p__42;
                            if (FrostySoundDataEditor._o__30._p__41 == null)
                            {
                                FrostySoundDataEditor._o__30._p__41 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target24 = FrostySoundDataEditor._o__30._p__41.Target;
                            CallSite _p__24 = FrostySoundDataEditor._o__30._p__41;
                            if (FrostySoundDataEditor._o__30._p__37 == null)
                            {
                                FrostySoundDataEditor._o__30._p__37 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj9 = FrostySoundDataEditor._o__30._p__37.Target(FrostySoundDataEditor._o__30._p__37, rootObject);
                            if (FrostySoundDataEditor._o__30._p__40 == null)
                            {
                                FrostySoundDataEditor._o__30._p__40 = CallSite < Func < CallSite, object, object, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target25 = FrostySoundDataEditor._o__30._p__40.Target;
                            CallSite _p__25 = FrostySoundDataEditor._o__30._p__40;
                            if (FrostySoundDataEditor._o__30._p__38 == null)
                            {
                                FrostySoundDataEditor._o__30._p__38 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj10 = FrostySoundDataEditor._o__30._p__38.Target(FrostySoundDataEditor._o__30._p__38, obj);
                            if (FrostySoundDataEditor._o__30._p__39 == null)
                            {
                                FrostySoundDataEditor._o__30._p__39 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            nativeReader3.Position = target22(_p__22, target23(_p__23, target24(_p__24, obj9, target25(_p__25, obj10, FrostySoundDataEditor._o__30._p__39.Target(FrostySoundDataEditor._o__30._p__39, obj)))));
                            enumerable = enumerable.Concat(nativeReader.ReadToEnd());
                            int num5 = array2.Length;
                            if (FrostySoundDataEditor._o__30._p__50 == null)
                            {
                                FrostySoundDataEditor._o__30._p__50 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostySoundDataEditor)));
                            }
                            Func<CallSite, object, int> target26 = FrostySoundDataEditor._o__30._p__50.Target;
                            CallSite _p__26 = FrostySoundDataEditor._o__30._p__50;
                            if (FrostySoundDataEditor._o__30._p__49 == null)
                            {
                                FrostySoundDataEditor._o__30._p__49 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, object> target27 = FrostySoundDataEditor._o__30._p__49.Target;
                            CallSite _p__27 = FrostySoundDataEditor._o__30._p__49;
                            if (FrostySoundDataEditor._o__30._p__48 == null)
                            {
                                FrostySoundDataEditor._o__30._p__48 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target28 = FrostySoundDataEditor._o__30._p__48.Target;
                            CallSite _p__28 = FrostySoundDataEditor._o__30._p__48;
                            if (FrostySoundDataEditor._o__30._p__44 == null)
                            {
                                FrostySoundDataEditor._o__30._p__44 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj11 = FrostySoundDataEditor._o__30._p__44.Target(FrostySoundDataEditor._o__30._p__44, rootObject);
                            if (FrostySoundDataEditor._o__30._p__47 == null)
                            {
                                FrostySoundDataEditor._o__30._p__47 = CallSite < Func < CallSite, object, object, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target29 = FrostySoundDataEditor._o__30._p__47.Target;
                            CallSite _p__29 = FrostySoundDataEditor._o__30._p__47;
                            if (FrostySoundDataEditor._o__30._p__45 == null)
                            {
                                FrostySoundDataEditor._o__30._p__45 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj12 = FrostySoundDataEditor._o__30._p__45.Target(FrostySoundDataEditor._o__30._p__45, obj);
                            if (FrostySoundDataEditor._o__30._p__46 == null)
                            {
                                FrostySoundDataEditor._o__30._p__46 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            int num6 = target26(_p__26, target27(_p__27, target28(_p__28, obj11, target29(_p__29, obj12, FrostySoundDataEditor._o__30._p__46.Target(FrostySoundDataEditor._o__30._p__46, obj)))));
                            if (FrostySoundDataEditor._o__30._p__55 == null)
                            {
                                FrostySoundDataEditor._o__30._p__55 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostySoundDataEditor)));
                            }
                            Func<CallSite, object, int> target30 = FrostySoundDataEditor._o__30._p__55.Target;
                            CallSite _p__30 = FrostySoundDataEditor._o__30._p__55;
                            if (FrostySoundDataEditor._o__30._p__54 == null)
                            {
                                FrostySoundDataEditor._o__30._p__54 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, object> target31 = FrostySoundDataEditor._o__30._p__54.Target;
                            CallSite _p__31 = FrostySoundDataEditor._o__30._p__54;
                            if (FrostySoundDataEditor._o__30._p__53 == null)
                            {
                                FrostySoundDataEditor._o__30._p__53 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target32 = FrostySoundDataEditor._o__30._p__53.Target;
                            CallSite _p__32 = FrostySoundDataEditor._o__30._p__53;
                            if (FrostySoundDataEditor._o__30._p__51 == null)
                            {
                                FrostySoundDataEditor._o__30._p__51 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj13 = FrostySoundDataEditor._o__30._p__51.Target(FrostySoundDataEditor._o__30._p__51, rootObject);
                            if (FrostySoundDataEditor._o__30._p__52 == null)
                            {
                                FrostySoundDataEditor._o__30._p__52 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            int num7 = num5 - (num6 - target30(_p__30, target31(_p__31, target32(_p__32, obj13, FrostySoundDataEditor._o__30._p__52.Target(FrostySoundDataEditor._o__30._p__52, obj)))));
                            if (FrostySoundDataEditor._o__30._p__58 == null)
                            {
                                FrostySoundDataEditor._o__30._p__58 = CallSite < Func < CallSite, object, int__.Create(Binder.Convert(CSharpBinderFlags.None, typeof(int), typeof(FrostySoundDataEditor)));
                            }
                            Func<CallSite, object, int> target33 = FrostySoundDataEditor._o__30._p__58.Target;
                            CallSite _p__33 = FrostySoundDataEditor._o__30._p__58;
                            if (FrostySoundDataEditor._o__30._p__57 == null)
                            {
                                FrostySoundDataEditor._o__30._p__57 = CallSite < Func < CallSite, object, int, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                }));
                            }
                            Func<CallSite, object, int, object> target34 = FrostySoundDataEditor._o__30._p__57.Target;
                            CallSite _p__34 = FrostySoundDataEditor._o__30._p__57;
                            if (FrostySoundDataEditor._o__30._p__56 == null)
                            {
                                FrostySoundDataEditor._o__30._p__56 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            int num8 = target33(_p__33, target34(_p__34, FrostySoundDataEditor._o__30._p__56.Target(FrostySoundDataEditor._o__30._p__56, obj), 1));
                            for (; ; )
                            {
                                if (FrostySoundDataEditor._o__30._p__62 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__62 = CallSite < Func < CallSite, object, bool__.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                Func<CallSite, object, bool> target35 = FrostySoundDataEditor._o__30._p__62.Target;
                                CallSite _p__35 = FrostySoundDataEditor._o__30._p__62;
                                if (FrostySoundDataEditor._o__30._p__61 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__61 = CallSite < Func < CallSite, int, object, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                Func<CallSite, int, object, object> target36 = FrostySoundDataEditor._o__30._p__61.Target;
                                CallSite _p__36 = FrostySoundDataEditor._o__30._p__61;
                                int num9 = num8;
                                if (FrostySoundDataEditor._o__30._p__60 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__60 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Count", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                Func<CallSite, object, object> target37 = FrostySoundDataEditor._o__30._p__60.Target;
                                CallSite _p__37 = FrostySoundDataEditor._o__30._p__60;
                                if (FrostySoundDataEditor._o__30._p__59 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__59 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                if (!target35(_p__35, target36(_p__36, num9, target37(_p__37, FrostySoundDataEditor._o__30._p__59.Target(FrostySoundDataEditor._o__30._p__59, rootObject)))))
                                {
                                    break;
                                }
                                if (FrostySoundDataEditor._o__30._p__67 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__67 = CallSite < Func < CallSite, object, bool__.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                Func<CallSite, object, bool> target38 = FrostySoundDataEditor._o__30._p__67.Target;
                                CallSite _p__38 = FrostySoundDataEditor._o__30._p__67;
                                if (FrostySoundDataEditor._o__30._p__66 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__66 = CallSite < Func < CallSite, object, int, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target39 = FrostySoundDataEditor._o__30._p__66.Target;
                                CallSite _p__39 = FrostySoundDataEditor._o__30._p__66;
                                if (FrostySoundDataEditor._o__30._p__65 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__65 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                Func<CallSite, object, object> target40 = FrostySoundDataEditor._o__30._p__65.Target;
                                CallSite _p__40 = FrostySoundDataEditor._o__30._p__65;
                                if (FrostySoundDataEditor._o__30._p__64 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__64 = CallSite < Func < CallSite, object, int, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target41 = FrostySoundDataEditor._o__30._p__64.Target;
                                CallSite _p__41 = FrostySoundDataEditor._o__30._p__64;
                                if (FrostySoundDataEditor._o__30._p__63 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__63 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                bool flag9 = target38(_p__38, target39(_p__39, target40(_p__40, target41(_p__41, FrostySoundDataEditor._o__30._p__63.Target(FrostySoundDataEditor._o__30._p__63, rootObject), num8)), 0));
                                if (flag9)
                                {
                                    break;
                                }
                                if (FrostySoundDataEditor._o__30._p__69 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__69 = CallSite < Func < CallSite, object, int, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target42 = FrostySoundDataEditor._o__30._p__69.Target;
                                CallSite _p__42 = FrostySoundDataEditor._o__30._p__69;
                                if (FrostySoundDataEditor._o__30._p__68 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__68 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                object obj14 = target42(_p__42, FrostySoundDataEditor._o__30._p__68.Target(FrostySoundDataEditor._o__30._p__68, rootObject), num8);
                                if (FrostySoundDataEditor._o__30._p__71 == null)
                                {
                                    FrostySoundDataEditor._o__30._p__71 = CallSite < Func < CallSite, object, bool__.Create(Binder.IsEvent(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor)));
                                }
                                bool flag10 = FrostySoundDataEditor._o__30._p__71.Target(FrostySoundDataEditor._o__30._p__71, obj14);
                                object obj15;
                                if (!flag10)
                                {
                                    if (FrostySoundDataEditor._o__30._p__70 == null)
                                    {
                                        FrostySoundDataEditor._o__30._p__70 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                    }
                                    obj15 = FrostySoundDataEditor._o__30._p__70.Target(FrostySoundDataEditor._o__30._p__70, obj14);
                                }
                                int num10 = num7;
                                if (!flag10)
                                {
                                    if (FrostySoundDataEditor._o__30._p__74 == null)
                                    {
                                        FrostySoundDataEditor._o__30._p__74 = CallSite < Func < CallSite, object, object, object__.Create(Binder.SetMember(CSharpBinderFlags.ValueFromCompoundAssignment, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                        {
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                        }));
                                    }
                                    Func<CallSite, object, object, object> target43 = FrostySoundDataEditor._o__30._p__74.Target;
                                    CallSite _p__43 = FrostySoundDataEditor._o__30._p__74;
                                    object obj16 = obj14;
                                    if (FrostySoundDataEditor._o__30._p__73 == null)
                                    {
                                        FrostySoundDataEditor._o__30._p__73 = CallSite < Func < CallSite, object, int, object__.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.AddAssign, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                        {
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                        }));
                                    }
                                    target43(_p__43, obj16, FrostySoundDataEditor._o__30._p__73.Target(FrostySoundDataEditor._o__30._p__73, obj15, num10));
                                }
                                else
                                {
                                    if (FrostySoundDataEditor._o__30._p__72 == null)
                                    {
                                        FrostySoundDataEditor._o__30._p__72 = CallSite < Func < CallSite, object, int, object__.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSpecialName | CSharpBinderFlags.ResultDiscarded, "add_SamplesOffset", null, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                                        {
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                        }));
                                    }
                                    FrostySoundDataEditor._o__30._p__72.Target(FrostySoundDataEditor._o__30._p__72, obj14, num10);
                                }
                                num8++;
                            }
                        }
                    }
                    array2 = enumerable.ToArray<byte>();
                }
            }
            App.AssetManager.ModifyChunk(CS_8__locals1.chunkEntry.Id, array2, null);
            if (FrostySoundDataEditor._o__30._p__75 == null)
            {
                FrostySoundDataEditor._o__30._p__75 = CallSite < Func < CallSite, object, uint, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "ChunkSize", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                }));
            }
            FrostySoundDataEditor._o__30._p__75.Target(FrostySoundDataEditor._o__30._p__75, obj3, (uint)array2.Length);
            if (FrostySoundDataEditor._o__30._p__76 == null)
            {
                FrostySoundDataEditor._o__30._p__76 = CallSite < Func < CallSite, object, bool, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "Seekable", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            FrostySoundDataEditor._o__30._p__76.Target(FrostySoundDataEditor._o__30._p__76, rootObject, false);
            if (FrostySoundDataEditor._o__30._p__80 == null)
            {
                FrostySoundDataEditor._o__30._p__80 = CallSite < Func < CallSite, object, int, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            Func<CallSite, object, int, object> target44 = FrostySoundDataEditor._o__30._p__80.Target;
            CallSite _p__44 = FrostySoundDataEditor._o__30._p__80;
            if (FrostySoundDataEditor._o__30._p__79 == null)
            {
                FrostySoundDataEditor._o__30._p__79 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, object, object, object> target45 = FrostySoundDataEditor._o__30._p__79.Target;
            CallSite _p__45 = FrostySoundDataEditor._o__30._p__79;
            if (FrostySoundDataEditor._o__30._p__77 == null)
            {
                FrostySoundDataEditor._o__30._p__77 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj17 = FrostySoundDataEditor._o__30._p__77.Target(FrostySoundDataEditor._o__30._p__77, rootObject);
            if (FrostySoundDataEditor._o__30._p__78 == null)
            {
                FrostySoundDataEditor._o__30._p__78 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            target44(_p__44, target45(_p__45, obj17, FrostySoundDataEditor._o__30._p__78.Target(FrostySoundDataEditor._o__30._p__78, obj)), 0);
            if (FrostySoundDataEditor._o__30._p__84 == null)
            {
                FrostySoundDataEditor._o__30._p__84 = CallSite < Func < CallSite, object, uint, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "SeekTableOffset", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            Func<CallSite, object, uint, object> target46 = FrostySoundDataEditor._o__30._p__84.Target;
            CallSite _p__46 = FrostySoundDataEditor._o__30._p__84;
            if (FrostySoundDataEditor._o__30._p__83 == null)
            {
                FrostySoundDataEditor._o__30._p__83 = CallSite < Func < CallSite, object, object, object__.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, object, object, object> target47 = FrostySoundDataEditor._o__30._p__83.Target;
            CallSite _p__47 = FrostySoundDataEditor._o__30._p__83;
            if (FrostySoundDataEditor._o__30._p__81 == null)
            {
                FrostySoundDataEditor._o__30._p__81 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj18 = FrostySoundDataEditor._o__30._p__81.Target(FrostySoundDataEditor._o__30._p__81, rootObject);
            if (FrostySoundDataEditor._o__30._p__82 == null)
            {
                FrostySoundDataEditor._o__30._p__82 = CallSite < Func < CallSite, object, object__.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            target46(_p__46, target47(_p__47, obj18, FrostySoundDataEditor._o__30._p__82.Target(FrostySoundDataEditor._o__30._p__82, obj)), uint.MaxValue);
            if (FrostySoundDataEditor._o__30._p__85 == null)
            {
                FrostySoundDataEditor._o__30._p__85 = CallSite < Func < CallSite, object, byte, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            FrostySoundDataEditor._o__30._p__85.Target(FrostySoundDataEditor._o__30._p__85, obj, 1);
            if (FrostySoundDataEditor._o__30._p__86 == null)
            {
                FrostySoundDataEditor._o__30._p__86 = CallSite < Func < CallSite, object, byte, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "FirstLoopSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            FrostySoundDataEditor._o__30._p__86.Target(FrostySoundDataEditor._o__30._p__86, obj, 0);
            if (FrostySoundDataEditor._o__30._p__87 == null)
            {
                FrostySoundDataEditor._o__30._p__87 = CallSite < Func < CallSite, object, byte, object__.Create(Binder.SetMember(CSharpBinderFlags.None, "LastLoopSegmentIndex", typeof(FrostySoundDataEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            FrostySoundDataEditor._o__30._p__87.Target(FrostySoundDataEditor._o__30._p__87, obj, 0);
            this.audioPlayer.Dispose();
            this.audioPlayer = new AudioPlayer();
            CS_8__locals1.tracks = this.InitialLoad(task);
            Dispatcher dispatcher2 = base.Dispatcher;
            if (dispatcher2 != null)
            {
                dispatcher2.Invoke(delegate
                {
                    CS_8__locals1._4__this.AssetModified = true;
                    CS_8__locals1._4__this.InvokeOnAssetModified();
                    EbxAssetEntry ebxAssetEntry = CS_8__locals1._4__this.AssetEntry as EbxAssetEntry;
                    ebxAssetEntry.LinkAsset(CS_8__locals1.chunkEntry);
                    CS_8__locals1._4__this.TracksList.Clear();
                    foreach (SoundDataTrack soundDataTrack in CS_8__locals1.tracks)
                    {
                        CS_8__locals1._4__this.TracksList.Add(soundDataTrack);
                    }
                });
            }
        }
        private const string PART_TracksListBox = "PART_TracksListBox";
        private const string PART_PlayButton = "PART_PlayButton";
        private const string PART_StopButton = "PART_StopButton";
        private const string PART_VolumeSlider = "PART_VolumeSlider";
        private const string PART_SoundExportMenuItem = "PART_SoundExportMenuItem";
        private const string PART_SoundImportMenuItem = "PART_SoundImportMenuItem";
        public static readonly DependencyProperty TracksListProperty = DependencyProperty.Register("TracksList", typeof(ObservableCollection<SoundDataTrack>), typeof(FrostySoundDataEditor), new FrameworkPropertyMetadata(null));
        private ListView tracksListBox;
        private Button playButton;
        private Button stopButton;
        private Slider volumeSlider;
        private AudioPlayer audioPlayer;
        private bool bFirstTime = true;
    }
}
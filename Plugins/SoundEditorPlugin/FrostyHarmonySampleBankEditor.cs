using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using SoundEditorPlugin.Playback;
using SoundEditorPlugin.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WaveFormRendererLib;

namespace SoundEditorPlugin
{
    public class FrostyHarmonySampleBankEditor : FrostySoundDataEditor
    {
        public FrostyHarmonySampleBankEditor()
            : base(null)
        {
        }

        public FrostyHarmonySampleBankEditor(ILogger inLogger)
            : base(inLogger)
        {
        }

        protected override List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            List<SoundDataTrack> retVal = new List<SoundDataTrack>();

            dynamic soundWave = RootObject;
            dynamic ramChunk = soundWave.Chunks[soundWave.RamChunkIndex];


            int index = 0;

            ChunkAssetEntry ramChunkEntry = App.AssetManager.GetChunkEntry(ramChunk.ChunkId);


            NativeReader streamChunkReader = null;
            if (soundWave.StreamChunkIndex != 255)
            {
                dynamic streamChunk = soundWave.Chunks[soundWave.StreamChunkIndex];
                ChunkAssetEntry streamChunkEntry = App.AssetManager.GetChunkEntry(streamChunk.ChunkId);
                streamChunkReader = new NativeReader(App.AssetManager.GetChunk(streamChunkEntry));
            }

            using (NativeReader reader = new NativeReader(App.AssetManager.GetChunk(ramChunkEntry)))
            {
                reader.Position = 0x0a;
                int datasetCount = reader.ReadUShort(Endian.Little); // Explicitly read as Little Endian based on decompiled usage

                reader.Position = 0x20;
                int dataOffset = reader.ReadInt(Endian.Little); // Explicitly read as Little Endian

                reader.Position = 0x50;
                List<int> offsets = new List<int>();
                for (int i = 0; i < datasetCount; i++)
                {
                    offsets.Add(reader.ReadInt(Endian.Little)); // Explicitly read as Little Endian
                    reader.Position += 4;
                }

                foreach (int offset in offsets)
                {
                    reader.Position = offset + 0x3c;
                    int blockCount = reader.ReadUShort(Endian.Little); // Explicitly read as Little Endian
                    reader.Position += 0x0a;

                    int fileOffset = -1;
                    bool streaming = false;

                    for (int i = 0; i < blockCount; i++)
                    {
                        uint blockType = reader.ReadUInt(Endian.Little); // Explicitly read as Little Endian
                        if (blockType == 0x2e4f4646) // '.OFF'
                        {
                            reader.Position += 4;
                            fileOffset = reader.ReadInt(Endian.Little); // Explicitly read as Little Endian
                            reader.Position += 0x0c;

                            streaming = true;
                        }
                        else if (blockType == 0x2e52414d) // '.RAM'
                        {
                            reader.Position += 4;
                            fileOffset = reader.ReadInt(Endian.Little) + dataOffset; // Explicitly read as Little Endian
                            reader.Position += 0x0c;
                        }
                        else
                        {
                            reader.Position += 0x14;
                        }
                    }

                    if (fileOffset != -1)
                    {
                        NativeReader actualReader = reader;
                        if (streaming)
                            actualReader = streamChunkReader;

                        SoundDataTrack track = new SoundDataTrack { Name = "Track #" + (index++) };

                        actualReader.Position = fileOffset;
                        List<short> decodedSoundBuf = new List<short>();

                        // Header reading (Big Endian specified to match decompiled logic)
                        // The ReadUInt at offset 0 has the size and flag masked off in the later editors, but here it's just read and ignored.
                        actualReader.ReadUInt(Endian.Big);

                        byte codec = actualReader.ReadByte();
                        int channels = (actualReader.ReadByte() >> 2) + 1;
                        ushort sampleRate = actualReader.ReadUShort(Endian.Big);
                        uint sampleCount = actualReader.ReadUInt(Endian.Big) & 0x00ffffff;

                        track.SampleRate = sampleRate;
                        track.ChannelCount = channels;

                        switch (codec)
                        {
                            case 0x14: track.Codec = "XAS"; break;
                            case 0x15: track.Codec = "EALayer3 v5"; break;
                            case 0x16: track.Codec = "EALayer3 v6"; break;
                            default: track.Codec = "Unknown (" + codec.ToString("x2") + ")"; break;
                        }

                        actualReader.Position = fileOffset;
                        byte[] soundBuf = actualReader.ReadToEnd();
                        double duration = 0.0;

                        if (codec == 0x14)
                        {
                            short[] data = XAS.Decode(soundBuf);
                            decodedSoundBuf.AddRange(data);
                            duration += (data.Length / channels) / (double)sampleRate;
                        }
                        else if (codec == 0x15 || codec == 0x16)
                        {
                            uint tempSampleCount = 0; // Use a temporary variable for the lambda
                            EALayer3.Decode(soundBuf, soundBuf.Length, (short[] data, int count, EALayer3.StreamInfo info) =>
                            {
                                if (info.streamIndex == -1)
                                    return;

                                tempSampleCount += (uint)data.Length;
                                decodedSoundBuf.AddRange(data);
                            });
                            duration += ((double)tempSampleCount / channels) / (double)sampleRate;
                        }

                        track.Duration += duration;
                        track.Samples = decodedSoundBuf.ToArray();

                        var maxPeakProvider = new MaxPeakProvider();
                        var rmsPeakProvider = new RmsPeakProvider(200);
                        var samplingPeakProvider = new SamplingPeakProvider(200);
                        var averagePeakProvider = new AveragePeakProvider(4f); // Use float literal

                        var topSpacerColor = System.Drawing.Color.FromArgb(64, 83, 22, 3);
                        var soundCloudOrangeTransparentBlocks = new SoundCloudBlockWaveFormSettings(System.Drawing.Color.FromArgb(196, 197, 53, 0), topSpacerColor, System.Drawing.Color.FromArgb(196, 79, 26, 0),
                                                                                                        System.Drawing.Color.FromArgb(64, 79, 79, 79))
                        {
                            Name = "SoundCloud Orange Transparent Blocks",
                            PixelsPerPeak = 2,
                            SpacerPixels = 1,
                            TopSpacerGradientStartColor = topSpacerColor,
                            BackgroundColor = System.Drawing.Color.Transparent,
                            Width = 800,
                            TopHeight = 50,
                            BottomHeight = 30,
                        };

                        try
                        {
                            var renderer = new WaveFormRenderer();
                            var image = renderer.Render(decodedSoundBuf.ToArray(), maxPeakProvider, soundCloudOrangeTransparentBlocks);

                            using (var ms = new MemoryStream())
                            {
                                image.Save(ms, ImageFormat.Png);
                                ms.Seek(0, SeekOrigin.Begin);

                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = ms;
                                bitmapImage.EndInit();

                                var target = new RenderTargetBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, bitmapImage.DpiX, bitmapImage.DpiY, PixelFormats.Pbgra32);
                                var visual = new DrawingVisual();

                                using (var r = visual.RenderOpen())
                                {
                                    r.DrawImage(bitmapImage, new Rect(0, 0, bitmapImage.Width, bitmapImage.Height));
                                }

                                target.Render(visual);
                                target.Freeze();
                                track.WaveForm = target;
                            }
                        }
                        catch (Exception)
                        {
                            // Empty catch block as in original
                        }

                        track.SegmentCount = 1;
                        retVal.Add(track);
                    }
                }
            }

            // Explicitly dispose of the streamChunkReader if it was initialized outside the using block.
            if (streamChunkReader != null)
                streamChunkReader.Dispose();

            return retVal;
        }
    }
}
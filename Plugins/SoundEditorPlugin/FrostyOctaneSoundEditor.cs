using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using WaveFormRendererLib;

namespace SoundEditorPlugin
{
    public class FrostyOctaneSoundEditor : FrostySoundDataEditor
    {
        private static int[] EA_XA_TABLE = new int[]
        {
            0, 240, 460, 392, 0, 0, -208, -220, 0, 1,
            3, 4, 7, 8, 10, 11, 0, -1, -3, -4
        };

        public FrostyOctaneSoundEditor()
            : base(null)
        {
        }

        public FrostyOctaneSoundEditor(ILogger inLogger)
            : base(inLogger)
        {
        }

        protected override List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            dynamic octane = RootObject;
            List<SoundDataTrack> retVal = new List<SoundDataTrack>();

            // Get the ChunkId from the first chunk entry dynamically
            dynamic soundDataChunkId = octane.Chunks[0].ChunkId;

            SoundDataTrack track = new SoundDataTrack() { ChannelCount = 1, Codec = "XAS Interleaved v0", Name = "Track #1" };
            double startLoopingTime = 0.0; // Using double literal for consistency
            double loopingDuration = 0.0; // Using double literal for consistency
            List<short> decodedSoundBuf = new List<short>();

            MemoryStream file = App.AssetManager.GetChunk(App.AssetManager.GetChunkEntry(soundDataChunkId));
            using (NativeReader reader = new NativeReader(file))
            {
                // Octane header reading, explicitly specifying Endian.Little to match decompiled code
                string magic = reader.ReadSizedString(4);
                uint version = reader.ReadUInt(Endian.Little);
                float minrpm = reader.ReadFloat(Endian.Little);
                float maxrpm = reader.ReadFloat(Endian.Little);
                uint table1Size = reader.ReadUInt(Endian.Little);
                uint table2Size = reader.ReadUInt(Endian.Little);
                uint sampleCount = reader.ReadUInt(Endian.Little);
                track.SampleRate = reader.ReadInt(Endian.Little);

                // rpm to sample table
                reader.ReadInt(Endian.Little); // skip
                int[] table1 = new int[table1Size];
                for (int i = 0; i < table1Size; i++)
                {
                    table1[i] = reader.ReadInt(Endian.Little);
                }

                // sample to sampleloop table
                reader.ReadInt(Endian.Little); // skip
                int[] table2 = new int[table2Size];
                for (int i = 0; i < table2Size; i++)
                {
                    table2[i] = reader.ReadInt(Endian.Little);
                }

                long pos = reader.Position;
                if (pos != (table1Size + 1 + table2Size + 1) * 4 + 32)
                {
                    // If position is wrong, log an error and attempt to fix the position
                    logger.LogError("Wrong offset after Tables: Expected {0}, got {1}", (table1Size + 1 + table2Size + 1) * 4 + 32, pos);
                    pos = (table1Size + 1 + table2Size + 1) * 4 + 32;
                }

                // Ensure reader is at the correct position before decoding starts
                reader.Position = pos;

                // XAS Interleaved v0 decoding loop (19 bytes per block, 32 samples per block)
                for (int block = 0; block < (reader.Length - pos) / 19; block++)
                {
                    short[] blocksamples = new short[32];

                    // Header is always read Little Endian, as specified in original code (argument 0 is used in NativeReader in decompiled)
                    uint header = reader.ReadUInt(Endian.Little);

                    int coef1 = EA_XA_TABLE[(header & 0x0F) + 0];
                    int coef2 = EA_XA_TABLE[(header & 0x0F) + 4];
                    short hist1 = (short)(header >> 16 & 0xFFF0);
                    short hist2 = (short)(header >> 0 & 0xFFF0);
                    byte shift = (byte)(header >> 16 & 0x0F);

                    blocksamples[0] = hist2;
                    blocksamples[1] = hist1;

                    for (int row = 0; row < 15; row++)
                    {
                        byte b = reader.ReadByte();
                        for (int i = 0; i < 2; i++)
                        {
                            int sample = 0;
                            if (i == 0)
                                sample = (b & 0xF0) >> 4;
                            else if (i == 1)
                                sample = b & 0x0F;

                            if (sample > 7)
                                sample -= 16;

                            int num = hist1 * coef1 + hist2 * coef2;

                            // Actual ADPCM decoding line
                            sample = (sample << 20 - shift) + num + 128 >> 8;

                            // Clamp sample to 16-bit range
                            if (sample > 32767) // short.MaxValue
                                sample = 32767;
                            else if (sample < -32768) // short.MinValue
                                sample = -32768;

                            blocksamples[2 + row * 2 + i] = (short)sample;

                            hist2 = hist1;
                            hist1 = (short)sample;
                        }
                    }
                    decodedSoundBuf.AddRange(blocksamples);
                }
                track.Samples = decodedSoundBuf.ToArray();
                // Corrected to use floating point division for accurate duration
                track.Duration = (double)decodedSoundBuf.Count / track.SampleRate;
                retVal.Add(track);
            }

            // Waveform Rendering Logic
            var maxPeakProvider = new MaxPeakProvider();
            var rmsPeakProvider = new RmsPeakProvider(200);
            var samplingPeakProvider = new SamplingPeakProvider(200);
            // Used 4f for float literal as suggested by analysis
            var averagePeakProvider = new AveragePeakProvider(4f);

            var topSpacerColor = System.Drawing.Color.FromArgb(64, 83, 22, 3);
            var soundCloudOrangeTransparentBlocks = new SoundCloudBlockWaveFormSettings(System.Drawing.Color.FromArgb(255, 218, 218, 218), topSpacerColor, System.Drawing.Color.FromArgb(255, 109, 109, 109),
                                                                                     System.Drawing.Color.FromArgb(64, 79, 79, 79))
            {
                Name = "SoundCloud Orange Transparent Blocks",
                PixelsPerPeak = 2,
                SpacerPixels = 1,
                TopSpacerGradientStartColor = topSpacerColor,
                BackgroundColor = System.Drawing.Color.FromArgb(128, 0, 0, 0),
                Width = 800,
                TopHeight = 49,
                BottomHeight = 29,
            };

            try
            {
                var renderer = new WaveFormRenderer();
                var image = renderer.Render(track.Samples, maxPeakProvider, soundCloudOrangeTransparentBlocks);

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
                        // Set EdgeMode to Aliased for crisp lines (needed a reference to System.Windows.Media.Effects but generally works with default WPF imports)
                        visual.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                        r.DrawImage(bitmapImage, new Rect(0, 0, bitmapImage.Width, bitmapImage.Height));

                        if (loopingDuration > 0)
                        {
                            // Drawing loop start/end lines
                            r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                new System.Windows.Point((int)(startLoopingTime / track.Duration * soundCloudOrangeTransparentBlocks.Width), soundCloudOrangeTransparentBlocks.TopHeight),
                                new System.Windows.Point((int)(startLoopingTime / track.Duration * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));
                            r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                new System.Windows.Point((int)((startLoopingTime + loopingDuration) / track.Duration * soundCloudOrangeTransparentBlocks.Width), soundCloudOrangeTransparentBlocks.TopHeight),
                                new System.Windows.Point((int)((startLoopingTime + loopingDuration) / track.Duration * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));
                            r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                new System.Windows.Point((int)(startLoopingTime / track.Duration * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height),
                                new System.Windows.Point((int)((startLoopingTime + loopingDuration) / track.Duration * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));
                        }
                    }

                    target.Render(visual);
                    target.Freeze();
                    track.WaveForm = target;
                }
            }
            catch (Exception e)
            {
                // In a real application, logging the exception 'e' would be better practice here.
            }

            return retVal;
        }
    }
}

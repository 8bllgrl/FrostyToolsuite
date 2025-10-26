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
    public class FrostyNewWaveEditor : FrostySoundDataEditor
    {
        public FrostyNewWaveEditor()
            : base(null)
        {
        }

        public FrostyNewWaveEditor(ILogger inLogger)
            : base(inLogger)
        {
        }

        protected override List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            List<SoundDataTrack> retVal = new List<SoundDataTrack>();
            dynamic root = RootObject;

            // Get the NewWaveResource, casting the dynamic property to string and lowercasing the name
            NewWaveResource newWave = App.AssetManager.GetResAs<NewWaveResource>(App.AssetManager.GetResEntry(((string)root.Name).ToLower()));

            int index = 0;
            int totalCount = newWave.Variations.Count;

            foreach (dynamic runtimeVariation in newWave.Variations)
            {
                task.Update(status: "Loading track #" + (index + 1), progress: ((index + 1) / (double)totalCount) * 100.0d);
                SoundDataTrack track = new SoundDataTrack { Name = "Track #" + ((index++) + 1) };

                // Determine which chunk index to use based on the SamplesOffsetFlag
                int chunkIndex = newWave.Segments[(int)runtimeVariation.FirstSegmentIndex].SamplesOffsetFlag == 1
                    ? (int)runtimeVariation.MemoryChunkIndex
                    : (int)runtimeVariation.StreamChunkIndex;

                dynamic soundDataChunk = newWave.Chunks[chunkIndex];
                ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(soundDataChunk.ChunkId);

                if (chunkEntry == null)
                {
                    continue;
                }

                using (NativeReader reader = new NativeReader(App.AssetManager.GetChunk(chunkEntry)))
                {
                    List<short> decodedSoundBuf = new List<short>();
                    double startLoopingTime = 0.0;
                    double loopingDuration = 0.0;

                    int channels = 0;
                    ushort sampleRate = 0;

                    for (int i = 0; i < (int)runtimeVariation.SegmentCount; i++)
                    {
                        var segment = newWave.Segments[(int)runtimeVariation.FirstSegmentIndex + i];
                        reader.Position = segment.SamplesOffset;

                        // Check magic number
                        if (reader.ReadUShort() != 0x48)
                        {
                            // Using the instance logger field (which is inherited from base)
                            logger.LogError("Wrong Sample Offset at Variation {0}, Segment {1}", index, i);
                            return retVal;
                        }

                        ushort headersize = reader.ReadUShort(Endian.Big);
                        byte codec = (byte)(reader.ReadByte() & 0xF);
                        channels = (reader.ReadByte() >> 2) + 1;
                        sampleRate = reader.ReadUShort(Endian.Big);
                        uint sampleCount = reader.ReadUInt(Endian.Big) & 0xFFFFFFF;

                        // Set the human-readable codec name on the track
                        switch (codec)
                        {
                            case 0x1: track.Codec = "Unknown"; break;
                            case 0x2: track.Codec = "PCM 16 Big"; break;
                            case 0x3: track.Codec = "EA-XMA"; break;
                            case 0x4: track.Codec = "XAS Interleaved v1"; break;
                            case 0x5: track.Codec = "EALayer3 Interleaved v1"; break;
                            case 0x6: track.Codec = "EALayer3 Interleaved v2 PCM"; break;
                            case 0x7: track.Codec = "EALayer3 Interleaved v2 Spike"; break;
                            case 0x9: track.Codec = "EASpeex"; break;
                            case 0xA: track.Codec = "Unknown"; break;
                            case 0xB: track.Codec = "EA-MP3"; break;
                            case 0xC: track.Codec = "EAOpus"; break;
                            case 0xD: track.Codec = "EAAtrac9"; break;
                            case 0xE: track.Codec = "MultiStream Opus"; break;
                            case 0xF: track.Codec = "MultiStream Opus (Uncoupled)"; break;
                        }

                        // Check for loop start segment
                        if (i == (int)runtimeVariation.FirstLoopSegmentIndex && (int)runtimeVariation.SegmentCount > 1)
                        {
                            startLoopingTime = (decodedSoundBuf.Count / channels) / (double)sampleRate;
                            track.LoopStart = (uint)decodedSoundBuf.Count;
                        }

                        // Rewind to segment data start and read the raw buffer for decoding
                        reader.Position = segment.SamplesOffset;
                        byte[] soundBuf = reader.ReadToEnd();
                        double duration = 0.0;

                        if (codec == 0x2)
                        {
                            short[] data = Pcm16b.Decode(soundBuf);
                            decodedSoundBuf.AddRange(data);
                            duration += (data.Length / channels) / (double)sampleRate;
                            sampleCount = (uint)data.Length;
                        }
                        else if (codec == 0x4)
                        {
                            short[] data = XAS.Decode(soundBuf);
                            decodedSoundBuf.AddRange(data);
                            duration += (data.Length / channels) / (double)sampleRate;
                            sampleCount = (uint)data.Length;
                        }
                        else if (codec == 0x5 || codec == 0x6 || codec == 0xC)
                        {
                            sampleCount = 0;
                            EALayer3.Decode(soundBuf, soundBuf.Length, (short[] data, int count, EALayer3.StreamInfo info) =>
                            {
                                if (info.streamIndex == -1)
                                    return;

                                sampleCount += (uint)data.Length;
                                channels = info.numChannels; // This closure update is critical for channel correctness
                                decodedSoundBuf.AddRange(data);
                            });
                            duration += (sampleCount / channels) / (double)sampleRate;
                        }
                        else if (codec == 0xE || codec == 0xF) // MultiStream Opus - No built-in decoder, saving raw data for debugging
                        {
                            logger.Log("Detected MultiStream Opus audio. Saving raw data.");

                            // *** CLEANUP: Removed the redundant reader.Position/ReadToEnd() as soundBuf already holds the data. ***

                            try
                            {
                                string filePath = Path.Combine(@"E:\Start_Here\User_Files\Game_Reverse_Engineering\Extracted\Deadspace\raw", $"raw_opus_chunk_{chunkIndex}_segment_{i}.opus");
                                File.WriteAllBytes(filePath, soundBuf);
                                logger.Log($"Raw MultiStream Opus data saved to: {filePath}");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Error saving raw MultiStream Opus data: {ex.Message}");
                            }
                        }

                        // Update loop start/end points if segment count > 1
                        if ((int)runtimeVariation.SegmentCount > 1)
                        {
                            if (i < (int)runtimeVariation.FirstLoopSegmentIndex)
                            {
                                startLoopingTime += duration;
                                track.LoopStart += sampleCount;
                            }
                            if (i >= (int)runtimeVariation.FirstLoopSegmentIndex && i <= (int)runtimeVariation.LastLoopSegmentIndex)
                            {
                                loopingDuration += duration;
                                track.LoopEnd += sampleCount;
                            }
                        }

                        track.SampleRate = sampleRate;
                        track.ChannelCount = channels;
                        track.Duration += duration;
                    }

                    // Final loop point adjustment and sample array assignment
                    track.LoopEnd += track.LoopStart;
                    track.Samples = decodedSoundBuf.ToArray();

                    // --- Waveform Rendering Logic ---

                    var maxPeakProvider = new MaxPeakProvider();
                    var rmsPeakProvider = new RmsPeakProvider(200);
                    var samplingPeakProvider = new SamplingPeakProvider(200);
                    var averagePeakProvider = new AveragePeakProvider(4);

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
                        // Use maxPeakProvider as originally used
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
                                visual.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                                r.DrawImage(bitmapImage, new Rect(0, 0, bitmapImage.Width, bitmapImage.Height));

                                if (loopingDuration > 0)
                                {
                                    // Draw loop start line
                                    r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                        new System.Windows.Point((int)((startLoopingTime / track.Duration) * soundCloudOrangeTransparentBlocks.Width), soundCloudOrangeTransparentBlocks.TopHeight),
                                        new System.Windows.Point((int)((startLoopingTime / track.Duration) * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));

                                    // Draw loop end line
                                    r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                        new System.Windows.Point((int)(((startLoopingTime + loopingDuration) / track.Duration) * soundCloudOrangeTransparentBlocks.Width), soundCloudOrangeTransparentBlocks.TopHeight),
                                        new System.Windows.Point((int)(((startLoopingTime + loopingDuration) / track.Duration) * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));

                                    // Draw bottom loop line
                                    r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                        new System.Windows.Point((int)((startLoopingTime / track.Duration) * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height),
                                        new System.Windows.Point((int)(((startLoopingTime + loopingDuration) / track.Duration) * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));
                                }
                            }

                            target.Render(visual);
                            target.Freeze();
                            track.WaveForm = target;
                        }
                    }
                    catch (Exception e)
                    {
                        // Empty catch block as in original, though usually logging the exception is better
                    }

                    track.SegmentCount = (int)runtimeVariation.SegmentCount;
                }

                retVal.Add(track);
            }

            return retVal;
        }
    }
}

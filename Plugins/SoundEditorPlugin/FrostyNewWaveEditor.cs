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
            // Pass null for logger to match the usage in the decompiled source
            NewWaveResource newWave = App.AssetManager.GetResAs<NewWaveResource>(App.AssetManager.GetResEntry(((string)root.Name).ToLower()), null);

            int index = 0;
            int totalCount = newWave.Variations.Count;

            foreach (dynamic runtimeVariation in newWave.Variations)
            {
                task.Update(status: "Loading track #" + (index + 1), progress: ((index + 1) / (double)totalCount) * 100.0d);
                SoundDataTrack track = new SoundDataTrack { Name = "Track #" + ((index++) + 1) };

                // Determine which chunk index to use based on the SamplesOffsetFlag
                int chunkIndex;
                if (newWave.Segments[(int)runtimeVariation.FirstSegmentIndex].SamplesOffsetFlag != 1U)
                {
                    chunkIndex = (int)runtimeVariation.StreamChunkIndex;
                }
                else
                {
                    chunkIndex = (int)runtimeVariation.MemoryChunkIndex;
                }

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

                        // Check magic number (0x48). FIX: Must use Endian.Little (0) to match the decompiled check.
                        if (reader.ReadUShort(Endian.Little) != 0x48)
                        {
                            logger.LogError("Wrong Sample Offset at Variation {0}, Segment {1}", index, i);
                            return retVal;
                        }

                        // Read remaining header fields (Big Endian)
                        ushort headersize = reader.ReadUShort(Endian.Big);
                        byte codec = (byte)(reader.ReadByte() & 0xF);
                        channels = (reader.ReadByte() >> 2) + 1;
                        sampleRate = reader.ReadUShort(Endian.Big);
                        uint sampleCount = reader.ReadUInt(Endian.Big) & 0xFFFFFFF;
                        uint decodedSampleCount = 0;

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
                            case 0xC: track.Codec = "EAOpus"; break; // EAOpus
                            case 0xD: track.Codec = "EAAtrac9"; break;
                            case 0xE: track.Codec = "MultiStream Opus"; break; // MultiStream Opus
                            case 0xF: track.Codec = "MultiStream Opus (Uncoupled)"; break; // MultiStream Opus (Uncoupled)
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

                        if (codec == 0x2) // PCM 16 Big
                        {
                            short[] data = Pcm16b.Decode(soundBuf);
                            decodedSoundBuf.AddRange(data);
                            duration = (data.Length / channels) / (double)sampleRate;
                            decodedSampleCount = (uint)data.Length;
                        }
                        else if (codec == 0x4) // XAS Interleaved v1
                        {
                            short[] data = XAS.Decode(soundBuf);
                            decodedSoundBuf.AddRange(data);
                            duration = (data.Length / channels) / (double)sampleRate;
                            decodedSampleCount = (uint)data.Length;
                        }
                        else if (codec == 0x5 || codec == 0x6 || codec == 0xC) // EALayer3 or EAOpus
                        {
                            EALayer3.Decode(soundBuf, soundBuf.Length, (short[] data, int count, EALayer3.StreamInfo info) =>
                            {
                                if (info.streamIndex == -1)
                                    return;

                                decodedSampleCount += (uint)data.Length;
                                channels = info.numChannels;
                                decodedSoundBuf.AddRange(data);
                            });
                            // Calculate duration after decoding, as channel count may have changed
                            if (channels != 0)
                            {
                                duration = ((double)decodedSampleCount / channels) / sampleRate;
                            }
                        }
                        else if (codec == 0xE || codec == 0xF) // MultiStream Opus (Save raw data)
                        {
                            logger.Log("Detected MultiStream Opus audio. Saving raw data.");

                            try
                            {
                                // Use a safer, more predictable path based on the current domain
                                string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SoundEditorPlugin_RawData");
                                if (!Directory.Exists(saveDir))
                                {
                                    Directory.CreateDirectory(saveDir);
                                }
                                string filePath = Path.Combine(saveDir, $"raw_opus_chunk_{chunkIndex}_segment_{i}.opus");

                                File.WriteAllBytes(filePath, soundBuf);
                                logger.Log($"Raw MultiStream Opus data saved to: {filePath}");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Error saving raw MultiStream Opus data: {ex.Message}");
                            }
                        }

                        // Update loop end points
                        if ((int)runtimeVariation.SegmentCount > 1)
                        {
                            if (i >= (int)runtimeVariation.FirstLoopSegmentIndex && i <= (int)runtimeVariation.LastLoopSegmentIndex)
                            {
                                loopingDuration += duration;
                                track.LoopEnd += decodedSampleCount;
                            }
                        }

                        track.SampleRate = sampleRate;
                        track.ChannelCount = channels;
                        track.Duration += duration;
                    }

                    // Final loop point adjustment and sample array assignment
                    track.LoopEnd += track.LoopStart; // Final loop end sample index calculation
                    track.Samples = decodedSoundBuf.ToArray();

                    // --- Waveform Rendering Logic ---

                    var maxPeakProvider = new MaxPeakProvider();
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
                        // Logging for the exception as requested
                        logger.LogError("Error rendering waveform for track {0}: {1}", track.Name, e.Message);
                    }

                    track.SegmentCount = (int)runtimeVariation.SegmentCount;
                }

                retVal.Add(track);
            }

            return retVal;
        }
    }
}

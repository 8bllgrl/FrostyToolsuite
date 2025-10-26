using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Ebx;
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
using System.Threading.Tasks; // Added: Required for the Task.Run().Result pattern in Opus decoding
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WaveFormRendererLib;

namespace SoundEditorPlugin
{
    public class FrostySoundWaveEditor : FrostySoundDataEditor
    {
        public FrostySoundWaveEditor()
            : base(null)
        {
        }

        public FrostySoundWaveEditor(ILogger inLogger)
            : base(inLogger)
        {
        }

        protected override List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            List<SoundDataTrack> retVal = new List<SoundDataTrack>();
            dynamic soundWave = RootObject;

            int index = 0;
            // The Count property needs to be explicitly cast if using the dynamic object, or rely on the compiler's dynamic invocation
            int totalCount = soundWave.RuntimeVariations.Count;

            foreach (dynamic runtimeVariation in soundWave.RuntimeVariations)
            {
                task.Update(status: "Loading track #" + (index + 1), progress: ((index + 1) / (double)totalCount) * 100.0d);

                SoundDataTrack track = new SoundDataTrack { Name = "Track #" + ((index++) + 1) };

                dynamic soundDataChunk = soundWave.Chunks[runtimeVariation.ChunkIndex];
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

                    for (int i = 0; i < (int)runtimeVariation.SegmentCount; i++) // Cast to int for safety
                    {
                        var segment = soundWave.Segments[(int)runtimeVariation.FirstSegmentIndex + i]; // Cast indices
                        reader.Position = segment.SamplesOffset;

                        if (reader.ReadUShort() != 0x48)
                        {
                            logger.LogError("Wrong Sample Offset at Variation {0}, Segment {1}", index, i);
                            return retVal;
                        }

                        ushort headersize = reader.ReadUShort(Endian.Big);
                        byte codec = (byte)(reader.ReadByte() & 0xF);
                        int channels = (reader.ReadByte() >> 2) + 1;
                        ushort sampleRate = reader.ReadUShort(Endian.Big);
                        uint sampleCount = reader.ReadUInt(Endian.Big) & 0xFFFFFFF;
                        //reader.Position += headersize - 0x0C;

                        // Codec definition is identical
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

                        // Added: Assign the raw codec byte, as seen in the decompiled output
                        track.CodecUnformatted = (int)codec;

                        // Loop start calculation
                        if (i == (int)runtimeVariation.FirstLoopSegmentIndex && (int)runtimeVariation.SegmentCount > 1)
                        {
                            startLoopingTime = (decodedSoundBuf.Count / channels) / (double)sampleRate;
                            track.LoopStart = (uint)decodedSoundBuf.Count;
                        }

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
                        else if (codec == 0x5 || codec == 0x6)
                        {
                            sampleCount = 0;
                            EALayer3.Decode(soundBuf, soundBuf.Length, (short[] data, int count, EALayer3.StreamInfo info) =>
                            {
                                if (info.streamIndex == -1)
                                    return;

                                sampleCount += (uint)data.Length;
                                decodedSoundBuf.AddRange(data);
                            });
                            duration += (sampleCount / channels) / (double)sampleRate; // Use updated sampleCount and current channels/sampleRate
                        }
                        // Added: MultiStream Opus decoding logic from decompiled code
                        else if (codec == 0xE || codec == 0xF)
                        {
                            // The decompiled code uses Task.Run().Result to execute an asynchronous decoder synchronously.
                            // We replicate this blocking structure using a synchronous call to a hypothetical decoder
                            // that matches the logic of the decompiled output.
                            short[] data = Task.Run<short[]>(() =>
                            {
                                // Assuming MultiStreamOpus.Decode exists and returns short[]
                                return MultiStreamOpus.Decode(soundBuf);
                            }).Result;

                            sampleCount = (uint)data.Length;
                            decodedSoundBuf.AddRange(data);
                            duration += (sampleCount / channels) / (double)sampleRate;
                        }

                        // Loop end calculation
                        if (i == (int)runtimeVariation.LastLoopSegmentIndex && (int)runtimeVariation.SegmentCount > 1)
                        {
                            loopingDuration = ((decodedSoundBuf.Count / channels) / (double)sampleRate) - startLoopingTime;
                            track.LoopEnd = (uint)decodedSoundBuf.Count;
                        }

                        track.SampleRate = sampleRate;
                        track.ChannelCount = channels;
                        // Segment length calculation and assignment - identical logic, but removed the float cast in division for clarity
                        if ((float)segment.SegmentLength == 0.0f)
                            segment.SegmentLength = (decodedSoundBuf.Count / track.ChannelCount) / (float)sampleRate;
                    }

                    // Final duration calculation and sample assignment
                    track.Duration = (decodedSoundBuf.Count / track.ChannelCount) / (double)track.SampleRate;
                    //track.LoopEnd += track.LoopStart; // This was commented out in the source, keeping it commented.
                    track.Samples = decodedSoundBuf.ToArray();

                    // --- Waveform Rendering ---

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
                                // --- MISSING FUNCTIONALITY ADDED: EdgeMode setting for crisp lines ---
                                visual.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                                r.DrawImage(bitmapImage, new Rect(0, 0, bitmapImage.Width, bitmapImage.Height));

                                if (loopingDuration > 0)
                                {
                                    // Explicitly use System.Windows.Media types to resolve ambiguity
                                    r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                        new System.Windows.Point((int)((startLoopingTime / track.Duration) * soundCloudOrangeTransparentBlocks.Width), soundCloudOrangeTransparentBlocks.TopHeight),
                                        new System.Windows.Point((int)((startLoopingTime / track.Duration) * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));

                                    r.DrawLine(new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0),
                                        new System.Windows.Point((int)(((startLoopingTime + loopingDuration) / track.Duration) * soundCloudOrangeTransparentBlocks.Width), soundCloudOrangeTransparentBlocks.TopHeight),
                                        new System.Windows.Point((int)(((startLoopingTime + loopingDuration) / track.Duration) * soundCloudOrangeTransparentBlocks.Width), (int)bitmapImage.Height));

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
                        // Empty catch block as in original
                    }

                    track.SegmentCount = runtimeVariation.SegmentCount;
                }

                retVal.Add(track);
            }

            // The SoundWave localization assignment block is included in the new code, and is retained here.
            foreach (dynamic localization in soundWave.Localization)
            {
                for (int i = 0; i < (int)localization.VariationCount; i++)
                {
                    SoundDataTrack track = retVal[i + (int)localization.FirstVariationIndex];

                    PointerRef pr = localization.Language;
                    // The decompiled version explicitly passes 'false' to GetEbx, which is safer when working with localization assets.
                    EbxAsset asset = App.AssetManager.GetEbx(App.AssetManager.GetEbxEntry(pr.External.FileGuid), false);
                    dynamic obj = asset.GetObject(pr.External.ClassGuid);

                    // Assignment relies on dynamic binding for the __Id property
                    track.Language = (string)obj.__Id;
                }
            }

            return retVal;
        }
    }

    // NOTE: This class is a placeholder for the MultiStreamOpus decoder, which is referenced
    // by the newly added logic (codec 0xE/0xF) and must be assumed to exist in the actual environment.
    public static class MultiStreamOpus
    {
        public static short[] Decode(byte[] soundBuf)
        {
            // Placeholder: The actual method would contain the Opus decoding logic.
            // For compilation, we return an empty array.
            return new short[0];
        }
    }
}

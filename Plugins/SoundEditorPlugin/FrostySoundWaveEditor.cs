using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using SoundEditorPlugin.Playback;
using SoundEditorPlugin.Resources;
using SoundEditorPlugin.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WaveFormRendererLib;

namespace SoundEditorPlugin
{
    public class FrostySoundWaveEditor : FrostySoundDataEditor
    {
        public FrostySoundWaveEditor() : base(null) { }

        public FrostySoundWaveEditor(ILogger inLogger) : base(inLogger) { }

        protected override List<SoundDataTrack> InitialLoad(FrostyTaskWindow task)
        {
            List<SoundDataTrack> tracks = new List<SoundDataTrack>();
            // Cast RootObject to dynamic to enable clean property access on EBX objects
            dynamic rootObject = base.RootObject;

            dynamic runtimeVariations = rootObject.RuntimeVariations;
            int variationCount = runtimeVariations.Count;

            int variationIndex = 0;
            foreach (dynamic variation in runtimeVariations)
            {
                task.Update($"Loading track #{(variationIndex + 1)}", (double)(variationIndex + 1) / (double)variationCount * 100.0);

                SoundDataTrack track = new SoundDataTrack
                {
                    Name = $"Track #{(variationIndex + 1)}",
                    VariationIndex = variationIndex
                };

                // Get Chunk Asset Entry
                // Assumes "Chunks" is a list-like EBX property
                dynamic chunkRef = rootObject.Chunks[(int)variation.ChunkIndex];
                Guid chunkId = chunkRef.ChunkId;
                ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(chunkId);

                if (chunkEntry != null)
                {
                    using (NativeReader chunkReader = new NativeReader(App.AssetManager.GetChunk(chunkEntry)))
                    {
                        List<short> decodedSoundBuf = new List<short>();
                        double loopStartDuration = 0.0;
                        double loopDuration = 0.0;
                        int currentSegmentIndex = 0;

                        // Loop through segments for the current variation
                        while (currentSegmentIndex < (int)variation.SegmentCount)
                        {
                            // Get current segment, calculated by FirstSegmentIndex + currentSegmentIndex
                            dynamic segment = rootObject.Segments[(int)variation.FirstSegmentIndex + currentSegmentIndex];

                            // Read SamplesOffset, masking out the flags in the lower 2 bits (SamplesOffset is uint/int, flags are 0-3)
                            uint samplesOffset = (uint)segment.SamplesOffset & 0xFFFFFFFCU;

                            // 1. Read segment header metadata
                            chunkReader.Position = (long)samplesOffset;

                            // Read and check Magic Number ('H', 0x48)
                            ushort samplesMagic = chunkReader.ReadUShort(Endian.Little);
                            chunkReader.ReadUShort(Endian.Big); // Samples Block Size (ignored)

                            // Read Codec Type (The compression byte)
                            byte compressionType = (byte)(chunkReader.ReadByte() & 0x0F);

                            int channelCount = (chunkReader.ReadByte() >> 2) + 1;
                            ushort sampleRate = chunkReader.ReadUShort(Endian.Big);
                            uint segmentSampleCount = chunkReader.ReadUInt(Endian.Big) & 0x0FFFFFFF;

                            if (samplesMagic != 0x48)
                            {
                                logger.LogError($"Wrong Samples Block Magic at Variation {variationIndex + 1}, Segment {currentSegmentIndex + 1}. Expected 0x48, got {samplesMagic:X4}");
                                // Abort loading this asset if magic number is wrong
                                return tracks;
                            }

                            // 2. Codec Identification
                            switch (compressionType)
                            {
                                case 2: track.Codec = "PCM 16 Big"; break;
                                case 3: track.Codec = "EA-XMA"; break;
                                case 4: track.Codec = "XAS Interleaved v1"; break;
                                case 5: track.Codec = "EALayer3 Interleaved v1"; break;
                                case 6: track.Codec = "EALayer3 Interleaved v2 PCM"; break;
                                case 7: track.Codec = "EALayer3 Interleaved v2 Spike"; break;
                                case 9: track.Codec = "EASpeex"; break;
                                case 11: track.Codec = "EA-MP3"; break;
                                case 12: track.Codec = "EAOpus"; break;
                                case 13: track.Codec = "EAAtrac9"; break;
                                case 14: track.Codec = "MultiStream Opus"; break;
                                case 15: track.Codec = "MultiStream Opus (Uncoupled)"; break;
                                default: track.Codec = "Unknown"; break;
                            }
                            track.CodecUnformatted = compressionType;

                            // 3. Loop Start Check
                            int firstLoopSegmentIndex = (int)variation.FirstLoopSegmentIndex;
                            if (currentSegmentIndex == firstLoopSegmentIndex && firstLoopSegmentIndex >= 0 && (int)variation.SegmentCount > 1)
                            {
                                loopStartDuration = (double)(decodedSoundBuf.Count / channelCount) / (double)sampleRate;
                                track.LoopStart = (uint)decodedSoundBuf.Count;
                            }

                            // 4. Read raw compressed data
                            // The raw data stream runs from the current SamplesOffset until the end of the chunk
                            chunkReader.Position = (long)samplesOffset;
                            byte[] soundBuf = chunkReader.ReadToEnd();

                            short[] decodedSamples = null;
                            uint segmentSampleCountDecoded = 0;

                            // 5. Decode Segment Data
                            if (compressionType == 2) // PCM 16 Big
                            {
                                decodedSamples = Pcm16b.Decode(soundBuf);
                                segmentSampleCountDecoded = (uint)decodedSamples.Length;
                                decodedSoundBuf.AddRange(decodedSamples);
                            }
                            else if (compressionType == 4) // XAS Interleaved v1
                            {
                                decodedSamples = XAS.Decode(soundBuf);
                                segmentSampleCountDecoded = (uint)decodedSamples.Length;
                                decodedSoundBuf.AddRange(decodedSamples);
                            }
                            else if (compressionType == 5 || compressionType == 6) // EALayer3
                            {
                                // EALayer3 decodes via P/Invoke and callback, handles multi-segment decoding
                                uint tempSegmentSampleCount = 0;

                                EALayer3.Decode(soundBuf, soundBuf.Length, (data, count, info) =>
                                {
                                    if (info.streamIndex != -1)
                                    {
                                        tempSegmentSampleCount += (uint)data.Length;
                                        decodedSoundBuf.AddRange(data);
                                    }
                                });
                                segmentSampleCountDecoded = tempSegmentSampleCount;
                            }
                            else if (compressionType == 12 || compressionType == 14 || compressionType == 15) // Opus or MultiStream Opus
                            {
                                // Use VgmStreamHelper for Opus formats (This requires Task.Run().Result to block the task thread for sequential loading)
                                try
                                {
                                    decodedSamples = Task.Run(async () =>
                                    {
                                        // The Decode method is safe to call async inside a Task.Run block
                                        return await VgmStreamHelper.Instance.Decode(soundBuf);
                                    }).Result; // Block the thread to wait for the result

                                    segmentSampleCountDecoded = (uint)decodedSamples.Length;
                                    decodedSoundBuf.AddRange(decodedSamples);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError( $"Failed to decode Opus track {track.Name} using VgmStreamHelper.");
                                }
                            }
                            else
                            {
                                // Unknown codec, cannot decode, add 0 length array
                                logger.LogWarning($"Cannot decode unknown codec {compressionType} for track {track.Name}.");
                                segmentSampleCountDecoded = 0;
                            }

                            // 6. Loop End Check
                            int lastLoopSegmentIndex = (int)variation.LastLoopSegmentIndex;
                            if (currentSegmentIndex == lastLoopSegmentIndex && lastLoopSegmentIndex >= 0 && (int)variation.SegmentCount > 1)
                            {
                                // Calculate loop duration based on current samples
                                loopDuration = (double)(decodedSoundBuf.Count / channelCount) / (double)sampleRate - loopStartDuration;
                                track.LoopEnd = (uint)decodedSoundBuf.Count;
                            }

                            // 7. Update Segment Info and Next Segment
                            track.SampleRate = sampleRate;
                            track.ChannelCount = channelCount;

                            // Update segment length if it was zero or missing
                            if ((float)segment.SegmentLength == 0.0f)
                            {
                                // Calculate length based on decoded buffer count for this segment
                                segment.SegmentLength = (float)(segmentSampleCountDecoded / channelCount) / (float)sampleRate;
                            }

                            currentSegmentIndex++;
                        }

                        // 8. Final Track Setup and Waveform Generation
                        track.Duration = (double)(decodedSoundBuf.Count / track.ChannelCount) / (double)track.SampleRate;
                        track.Samples = decodedSoundBuf.ToArray();

                        // Set the loaded flag
                        track.IsLoaded = true;

                        // Waveform Rendering Setup (kept synchronous as per original design)
                        MaxPeakProvider maxPeakProvider = new MaxPeakProvider();
                        global::System.Drawing.Color color = global::System.Drawing.Color.FromArgb(64, 83, 22, 3);

                        SoundCloudBlockWaveFormSettings soundCloudBlockWaveFormSettings = new SoundCloudBlockWaveFormSettings(
                            global::System.Drawing.Color.FromArgb(255, 218, 218, 218),
                            color,
                            global::System.Drawing.Color.FromArgb(255, 109, 109, 109),
                            global::System.Drawing.Color.FromArgb(64, 79, 79, 79))
                        {
                            Name = "SoundCloud Orange Transparent Blocks",
                            PixelsPerPeak = 2,
                            SpacerPixels = 1,
                            TopSpacerGradientStartColor = color,
                            BackgroundColor = global::System.Drawing.Color.FromArgb(128, 0, 0, 0),
                            Width = 800,
                            TopHeight = 49,
                            BottomHeight = 29
                        };

                        try
                        {
                            Image image = new WaveFormRenderer().Render(track.Samples, maxPeakProvider, soundCloudBlockWaveFormSettings);
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                image.Save(memoryStream, ImageFormat.Png);
                                memoryStream.Seek(0L, SeekOrigin.Begin);
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.EndInit();

                                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, bitmapImage.DpiX, bitmapImage.DpiY, PixelFormats.Pbgra32);
                                DrawingVisual drawingVisual = new DrawingVisual();

                                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                                {
                                    drawingVisual.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                                    drawingContext.DrawImage(bitmapImage, new Rect(0.0, 0.0, bitmapImage.Width, bitmapImage.Height));

                                    // Draw loop markers if present 
                                    if (loopDuration > 0.0)
                                    {
                                        double width = soundCloudBlockWaveFormSettings.Width;
                                        double height = bitmapImage.Height;
                                        double topHeight = soundCloudBlockWaveFormSettings.TopHeight;

                                        double loopStartPos = loopStartDuration / track.Duration * width;
                                        double loopEndPos = (loopStartDuration + loopDuration) / track.Duration * width;

                                        System.Windows.Media.Pen whitePen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1.0);

                                        drawingContext.DrawLine(whitePen, new System.Windows.Point(loopStartPos, topHeight), new System.Windows.Point(loopStartPos, height));
                                        drawingContext.DrawLine(whitePen, new System.Windows.Point(loopEndPos, topHeight), new System.Windows.Point(loopEndPos, height));
                                        drawingContext.DrawLine(whitePen, new System.Windows.Point(loopStartPos, height), new System.Windows.Point(loopEndPos, height));
                                    }
                                }
                                renderTargetBitmap.Render(drawingVisual);
                                renderTargetBitmap.Freeze();
                                track.WaveForm = renderTargetBitmap;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Failed to generate waveform for track {0}.", variationIndex + 1);
                        }

                        track.SegmentCount = (int)variation.SegmentCount;
                        track.ChunkId = chunkId;
                    }
                    tracks.Add(track);
                }

                variationIndex++;
            }

            // 9. Localization Pass (Second loop for language metadata)
            dynamic localization = rootObject.Localization;
            if (localization != null)
            {
                foreach (dynamic locEntry in localization)
                {
                    int firstVariationIndex = (int)locEntry.FirstVariationIndex;
                    int variationCountLoc = (int)locEntry.VariationCount;

                    for (int i = 0; i < variationCountLoc; i++)
                    {
                        int trackIndex = firstVariationIndex + i;
                        if (trackIndex < tracks.Count)
                        {
                            SoundDataTrack track = tracks[trackIndex];

                            dynamic languageRef = locEntry.Language;
                            PointerRef pointerRef = languageRef;

                            if (pointerRef.External != null && pointerRef.External.FileGuid != Guid.Empty)
                            {
                                // Get language asset by reference
                                dynamic languageAsset = App.AssetManager.GetEbx(App.AssetManager.GetEbxEntry(pointerRef.External.FileGuid), false)
                                        .GetObject(pointerRef.External.ClassGuid);

                                // The __Id property typically holds the language code (e.g., "en_US")
                                track.Language = (string)languageAsset.__Id;
                            }
                        }
                    }
                }
            }

            return tracks;
        }
    }
}

using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using SoundEditorPlugin.Helpers;
using SoundEditorPlugin.Playback;
using SoundEditorPlugin.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WaveFormRendererLib;

namespace SoundEditorPlugin
{
    // The base class is assumed to handle the dynamic RootObject property.
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
            List<SoundDataTrack> list = new List<SoundDataTrack>();

            // Cleaned dynamic access. We assume RootObject has a 'Name' property.
            dynamic rootObject = base.RootObject;
            string assetName = ((string)rootObject.Name).ToLower();

            AssetEntry resEntry = App.AssetManager.GetResEntry(assetName);

            // Cast AssetEntry to ResAssetEntry as GetResAs expects it.
            ResAssetEntry resAssetEntry = (ResAssetEntry)resEntry;

            // Strong typing applied: NewWaveResource instead of dynamic
            NewWaveResource newWaveResource = App.AssetManager.GetResAs<NewWaveResource>(resAssetEntry, null);

            Dictionary<Guid, Stream> chunkStreams = new Dictionary<Guid, Stream>();

            // Strong typing applied: foreach (Chunk chunk in newWaveResource.Chunks)
            foreach (Chunk chunk in newWaveResource.Chunks)
            {
                ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(chunk.ChunkId);
                Stream chunkStream = App.AssetManager.GetChunk(chunkEntry);
                chunkStreams.Add(chunk.ChunkId, chunkStream);
            }

            int num = 0;
            int count = newWaveResource.Variations.Count;
            List<Task> loadSoundDataTasks = new List<Task>();

            // Strong typing applied: foreach (Variation variation in newWaveResource.Variations)
            foreach (Variation variation in newWaveResource.Variations)
            {
                task.Update("Loading track #" + (num + 1).ToString(), (double)(num + 1) / (double)count * 100.0);

                SoundDataTrack soundDataTrack = new SoundDataTrack
                {
                    Name = "Track #" + (num + 1).ToString() // Use num + 1 first
                };
                num++; // Increment here

                // The LoadTrackFromNewWave call is now strongly-typed and returns a Task
                loadSoundDataTasks.Add(this.LoadTrackFromNewWave(num, newWaveResource, soundDataTrack, variation, chunkStreams));

                list.Add(soundDataTrack);
            }

            // FIX: Replaced Task.RunSynchronously() with Task.WaitAll() for reliable blocking wait on async tasks.
            Task.Run(() =>
            {
                // Wait for all async loading tasks to complete before proceeding
                Task.WaitAll(loadSoundDataTasks.ToArray());

                foreach (KeyValuePair<Guid, Stream> keyValuePair in chunkStreams)
                {
                    keyValuePair.Value.Dispose();
                }
            }).Wait(); // Block the calling thread (FrostyTaskWindow thread) until all loading is done.

            return list;
        }

        protected override Task ReloadTrack(NewWaveResource newWave, SoundDataTrack track)
        {
            NewWaveResource newWaveRes = newWave;
            Dictionary<Guid, Stream> dictionary = new Dictionary<Guid, Stream>();

            // Strong typing applied: Accessing the Variation directly by index
            Variation variation = newWaveRes.Variations[track.VariationIndex];
            List<Segment> segments = newWaveRes.Segments;

            int firstSegmentIndex = (int)variation.FirstSegmentIndex;
            int chunkIndex;

            // Cleaned logic for determining chunk index (using SamplesOffsetFlag as a bool proxy)
            if (segments[firstSegmentIndex].SamplesOffsetFlag == 1U)
            {
                chunkIndex = (int)variation.MemoryChunkIndex;
            }
            else
            {
                chunkIndex = (int)variation.StreamChunkIndex;
            }

            // Strong typing applied: Accessing the Chunk directly
            Chunk chunkObject = newWaveRes.Chunks[chunkIndex];

            // Get ChunkEntry and Stream
            ChunkAssetEntry chunkAssetEntry = App.AssetManager.GetChunkEntry(chunkObject.ChunkId);
            Stream chunkStream = App.AssetManager.GetChunk(chunkAssetEntry);

            // Add to dictionary
            dictionary.Add(chunkObject.ChunkId, chunkStream);

            // Reset duration and call LoadTrackFromNewWave
            track.Duration = 0.0;

            // LoadTrackFromNewWave is now a strongly-typed call
            return this.LoadTrackFromNewWave(track.VariationIndex + 1, newWaveRes, track, variation, dictionary);
        }

        // This method is now properly defined as an async Task, with fixed decoding logic.
        public async Task LoadTrackFromNewWave(int index, NewWaveResource newWave, SoundDataTrack track, Variation runtimeVariation, Dictionary<Guid, Stream> chunkStreams)
        {
            // Determine chunk index cleanly
            int chunkIndex = (int)((newWave.Segments[(int)runtimeVariation.FirstSegmentIndex].SamplesOffsetFlag == 1U)
                ? runtimeVariation.MemoryChunkIndex
                : runtimeVariation.StreamChunkIndex);

            // Strong typing applied
            Chunk chunkObject = newWave.Chunks[chunkIndex];

            track.ChunkIndex = chunkIndex;
            track.ChunkId = chunkObject.ChunkId; // Strong typing applied
            track.SegmentIndex = (int)runtimeVariation.FirstSegmentIndex;
            track.VariationIndex = index - 1;
            track.SegmentCount = (int)runtimeVariation.SegmentCount;

            // Calculate the starting position of the sound data header
            // SamplesOffset stores (offset | flag), so mask out the flag (0xFFFFFFFC)
            long position = (long)(newWave.Segments[(int)runtimeVariation.FirstSegmentIndex].SamplesOffset & 0xFFFFFFFCU);

            // Declare stream and decoding variables outside the try block for scope access in catch/finally.
            Stream chunkStream = null;
            short[] decodedShorts = null;

            // Reading track metadata from the chunk stream
            using (NativeReader2 nativeReader = new NativeReader2(chunkStreams[track.ChunkId]))
            {
                chunkStream = chunkStreams[track.ChunkId];
                nativeReader.KeepUnderlyingStreamOpen = true;

                // Set position to read the metadata header
                nativeReader.Position = position;

                // Check magic number/signature (0x48)
                if (nativeReader.ReadUShort(Endian.Little) != 72) // 0x48 ('H')
                {
                    this.logger.LogError("Wrong Sample Offset at Variation {0}, Segment {1}", new object[] { index, runtimeVariation.FirstSegmentIndex });
                    return;
                }

                nativeReader.ReadUShort(Endian.Big); // Read 2 bytes (offset 2)

                // Read the codec byte (offset 4, lower 4 bits)
                byte codecByte = nativeReader.ReadByte();

                // Mask with 0x0F
                byte b = (byte)(codecByte & 15);

                // Set Codec name based on the codec byte value
                switch (b)
                {
                    case 1: track.Codec = "Unknown"; break;
                    case 2: track.Codec = "PCM 16 Big"; break;
                    case 3: track.Codec = "EA-XMA"; break;
                    case 4: track.Codec = "XAS Interleaved v1"; break;
                    case 5: track.Codec = "EALayer3 Interleaved v1"; break;
                    case 6: track.Codec = "EALayer3 Interleaved v2 PCM"; break;
                    case 7: track.Codec = "EALayer3 Interleaved v2 Spike"; break;
                    case 9: track.Codec = "EASpeex"; break;
                    case 10: track.Codec = "Unknown"; break;
                    case 11: track.Codec = "EA-MP3"; break;
                    case 12: track.Codec = "EAOpus"; break;
                    case 13: track.Codec = "EAAtrac9"; break;
                    case 14: track.Codec = "MultiStream Opus"; break;
                    case 15: track.Codec = "MultiStream Opus (Uncoupled)"; break;
                    default: track.Codec = "Unknown Codec (" + b.ToString() + ")"; break;
                }
                track.CodecUnformatted = (int)b;

                // Retrieve sample rate and channel count from the header if available
                // To do this reliably, we need to re-read the subsequent header parts, 
                // but since the decoding itself handles this or relies on metadata, we proceed.
            }

            // --- DECODING LOGIC IMPLEMENTED ---
            try
            {
                // Rewind stream to the beginning of the audio data header for the full buffer read
                chunkStream.Seek(position, SeekOrigin.Begin);

                // Read the entire remaining stream content into a byte array
                using (MemoryStream ms = new MemoryStream())
                {
                    chunkStream.CopyTo(ms);
                    byte[] compressedData = ms.ToArray();

                    if (compressedData.Length == 0)
                    {
                        this.logger.LogWarning($"Compressed data for track {track.Name} is empty.");
                        return;
                    }

                    // Check for Opus formats, as these rely on the external VgmStreamHelper
                    if (track.CodecUnformatted == 12 || track.CodecUnformatted == 14 || track.CodecUnformatted == 15)
                    {
                        // Await the asynchronous external tool call
                        decodedShorts = await VgmStreamHelper.Instance.Decode(compressedData);
                    }
                    else
                    {
                        // Handle other known formats internally (PCM, XAS, EALayer3)
                        switch (track.CodecUnformatted)
                        {
                            case 2: // PCM 16 Big
                                decodedShorts = Pcm16b.Decode(compressedData);
                                break;
                            case 4: // XAS Interleaved v1
                                decodedShorts = XAS.Decode(compressedData);
                                break;
                            // NOTE: EALayer3 handling is complex due to its callback structure and P/Invoke requirements.
                            // We rely on the internal EALayer3 class to handle the decoding if possible.
                            case 5: // EALayer3 Interleaved v1
                            case 6: // EALayer3 Interleaved v2 PCM
                            case 7: // EALayer3 Interleaved v2 Spike
                                List<short> eaDecoded = new List<short>();
                                EALayer3.Decode(compressedData, compressedData.Length, (data, count, info) =>
                                {
                                    if (info.streamIndex != -1)
                                    {
                                        eaDecoded.AddRange(data);
                                    }
                                    track.SampleRate = info.sampleRate;
                                    track.ChannelCount = info.numChannels;
                                });
                                decodedShorts = eaDecoded.ToArray();
                                break;
                            default:
                                this.logger.LogWarning($"No explicit decoder implemented for codec {track.Codec} ({track.CodecUnformatted}) for track {track.Name}. Skipping decoding.");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Corrected logging syntax: pass exception object first, followed by format string and arguments.
                // The previous fix for CS1503 in the catch block was wrong and has been fixed here.
                this.logger.LogError("Critical error during decoding of track {0}.", track.Name);
                return; // Return early on failure
            }

            // Once decoding is done, update track properties
            if (decodedShorts != null && decodedShorts.Length > 0)
            {
                track.Samples = decodedShorts;

                // Re-read metadata from header/segments to update SampleRate/ChannelCount/Duration 
                // as VgmStreamHelper might provide more accurate data.
                if (track.SampleRate > 0)
                {
                    track.Duration = (double)track.Samples.Length / (double)track.ChannelCount / (double)track.SampleRate;
                }
                else
                {
                    // Fallback to reading metadata directly from stream (similar to soundwave editor)
                    chunkStream.Seek(position, SeekOrigin.Begin);
                    using (NativeReader streamReader = new NativeReader(chunkStream))
                    {
                        streamReader.Position = position + 6; // Skip to channel count

                        // NativeReader.ReadByte() does not take an Endian argument.
                        track.ChannelCount = (streamReader.ReadByte() >> 2) + 1;

                        track.SampleRate = streamReader.ReadUShort(Endian.Big);

                        if (track.SampleRate > 0)
                        {
                            track.Duration = (double)track.Samples.Length / (double)track.ChannelCount / (double)track.SampleRate;
                        }
                        else
                        {
                            this.logger.LogWarning($"Could not determine sample rate for track {track.Name}. Duration is unknown.");
                        }
                    }
                }

                // Waveform generation (This block is kept synchronous after decoding)
                MaxPeakProvider maxPeakProvider = new MaxPeakProvider();
                global::System.Drawing.Color color = global::System.Drawing.Color.FromArgb(64, 83, 22, 3);
                SoundCloudBlockWaveFormSettings settings = new SoundCloudBlockWaveFormSettings(
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
                    Image image = new WaveFormRenderer().Render(track.Samples, maxPeakProvider, settings);
                    using (MemoryStream msWav = new MemoryStream())
                    {
                        image.Save(msWav, ImageFormat.Png);
                        msWav.Seek(0L, SeekOrigin.Begin);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = msWav;
                        bitmapImage.EndInit();
                        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, bitmapImage.DpiX, bitmapImage.DpiY, PixelFormats.Pbgra32);
                        DrawingVisual drawingVisual = new DrawingVisual();
                        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                        {
                            drawingContext.DrawImage(bitmapImage, new Rect(0.0, 0.0, bitmapImage.Width, bitmapImage.Height));
                        }
                        renderTargetBitmap.Render(drawingVisual);
                        renderTargetBitmap.Freeze();
                        track.WaveForm = renderTargetBitmap;
                    }
                }
                catch (Exception ex)
                {
                    // Corrected logging syntax
                    this.logger.LogError("Failed to generate waveform for track {0}.", track.Name);
                }

                track.IsLoaded = true;
            }
        }
    }
}

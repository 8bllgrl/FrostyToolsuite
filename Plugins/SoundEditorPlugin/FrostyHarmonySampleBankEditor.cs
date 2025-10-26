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
using FrostySdk.Managers.Entries;
using WaveFormRendererLib;
using SoundEditorPlugin; // Added reference to the plugin namespace for EALayer3/XAS

namespace SoundEditorPlugin
{
    // NOTE: This class definition is REQUIRED to replace the dynamic code. 
    // If the actual type of base.RootObject is exposed elsewhere in the SDK,
    // you should replace this inline declaration with the actual class name
    // and cast. Since the name is unknown, we define a local type with the
    // properties inferred from the decompiled code.
    // The properties used dynamically were: Chunks, RamChunkIndex, StreamChunkIndex, and ChunkId
    public class HarmonySampleBankData
    {
        public List<object> Chunks { get; set; }
        public int RamChunkIndex { get; set; }
        public int StreamChunkIndex { get; set; }
        public uint ChunkId { get; set; }
    }


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
            List<SoundDataTrack> list = new List<SoundDataTrack>();
            // --- DYNAMIC CODE FIX: Casting base.RootObject to the inferred type. ---
            // If the actual type name is different, replace 'HarmonySampleBankData' below.
            HarmonySampleBankData rootObject = (HarmonySampleBankData)base.RootObject;
            int num = 0;

            // 1. Get the RAM Chunk Entry.
            // Simplified from multiple dynamic calls: rootObject.Chunks[rootObject.RamChunkIndex].ChunkId
            // The original dynamic code was trying to:
            // a) get rootObject.Chunks (obj)
            // b) get rootObject.RamChunkIndex (obj2)
            // c) get the index (obj[obj2])
            object ramChunkData = rootObject.Chunks[rootObject.RamChunkIndex];
            uint ramChunkId = (uint)ramChunkData.GetType().GetProperty("ChunkId").GetValue(ramChunkData);

            ChunkAssetEntry ramChunkEntry = App.AssetManager.GetChunkEntry(ramChunkId);
            NativeReader streamChunkReader = null;

            // 2. Check and get the Stream Chunk Entry.
            // Simplified from dynamic check: (rootObject.StreamChunkIndex != 255)
            bool streamChunkExists = rootObject.StreamChunkIndex != 255;

            if (streamChunkExists)
            {
                // Simplified from multiple dynamic calls: rootObject.Chunks[rootObject.StreamChunkIndex].ChunkId
                object streamChunkData = rootObject.Chunks[rootObject.StreamChunkIndex];
                uint streamChunkId = (uint)streamChunkData.GetType().GetProperty("ChunkId").GetValue(streamChunkData);

                ChunkAssetEntry streamChunkEntry = App.AssetManager.GetChunkEntry(streamChunkId);
                streamChunkReader = new NativeReader(App.AssetManager.GetChunk(streamChunkEntry));
            }

            using (NativeReader nativeReader2 = new NativeReader(App.AssetManager.GetChunk(ramChunkEntry)))
            {
                nativeReader2.Position = 10L;

                // Fix: Replaced '0' with Endian.Little
                int num2 = (int)nativeReader2.ReadUShort(Endian.Little);

                nativeReader2.Position = 32L;

                // Fix: Replaced '0' with Endian.Little
                int num3 = nativeReader2.ReadInt(Endian.Little);

                nativeReader2.Position = 80L;
                List<int> list2 = new List<int>();
                for (int i = 0; i < num2; i++)
                {
                    // Fix: Replaced '0' with Endian.Little
                    list2.Add(nativeReader2.ReadInt(Endian.Little));
                    nativeReader2.Position += 4L;
                }
                foreach (int num4 in list2)
                {
                    nativeReader2.Position = (long)(num4 + 60);

                    // Fix: Replaced '0' with Endian.Little
                    int num5 = (int)nativeReader2.ReadUShort(Endian.Little);

                    nativeReader2.Position += 10L;
                    int num6 = -1;
                    bool flag2 = false;
                    for (int j = 0; j < num5; j++)
                    {
                        // Fix: Replaced '0' with Endian.Little
                        uint num7 = nativeReader2.ReadUInt(Endian.Little);
                        bool flag3 = num7 == 776947270U;
                        if (flag3)
                        {
                            nativeReader2.Position += 4L;
                            // Fix: Replaced '0' with Endian.Little
                            num6 = nativeReader2.ReadInt(Endian.Little);
                            nativeReader2.Position += 12L;
                            flag2 = true;
                        }
                        else
                        {
                            bool flag4 = num7 == 777142605U;
                            if (flag4)
                            {
                                nativeReader2.Position += 4L;
                                // Fix: Replaced '0' with Endian.Little
                                num6 = nativeReader2.ReadInt(Endian.Little) + num3;
                                nativeReader2.Position += 12L;
                            }
                            else
                            {
                                nativeReader2.Position += 20L;
                            }
                        }
                    }
                    bool flag5 = num6 != -1;
                    if (flag5)
                    {
                        NativeReader nativeReader3 = nativeReader2;
                        bool flag6 = flag2;
                        if (flag6)
                        {
                            nativeReader3 = streamChunkReader;
                        }
                        SoundDataTrack soundDataTrack = new SoundDataTrack
                        {
                            Name = "Track #" + num++.ToString()
                        };
                        nativeReader3.Position = (long)num6;
                        List<short> decodedSoundBuf = new List<short>();

                        // Fix: Replaced '1' with Endian.Big
                        uint num8 = nativeReader3.ReadUInt(Endian.Big) & 16777215U;

                        byte b = nativeReader3.ReadByte();
                        int num9 = (nativeReader3.ReadByte() >> 2) + 1;

                        // Fix: Replaced '1' with Endian.Big
                        ushort num10 = nativeReader3.ReadUShort(Endian.Big);

                        // Fix: Replaced '1' with Endian.Big
                        uint sampleCount = nativeReader3.ReadUInt(Endian.Big) & 16777215U;

                        switch (b)
                        {
                            case 20:
                                soundDataTrack.Codec = "XAS";
                                break;
                            case 21:
                                soundDataTrack.Codec = "EALayer3 v5";
                                break;
                            case 22:
                                soundDataTrack.Codec = "EALayer3 v6";
                                break;
                            default:
                                soundDataTrack.Codec = "Unknown (" + b.ToString("x2") + ")";
                                break;
                        }
                        nativeReader3.Position = (long)num6;
                        byte[] array = nativeReader3.ReadToEnd();
                        double num11 = 0.0;
                        bool flag7 = b == 20;
                        if (flag7)
                        {
                            short[] array2 = XAS.Decode(array);
                            decodedSoundBuf.AddRange(array2);
                            num11 += (double)(array2.Length / num9) / (double)num10;
                        }
                        else
                        {
                            bool flag8 = b == 21 || b == 22;
                            if (flag8)
                            {
                                sampleCount = 0U;
                                EALayer3.Decode(array, array.Length, delegate (short[] data, int count, EALayer3.StreamInfo info)
                                {
                                    bool flag9 = info.streamIndex == -1;
                                    if (!flag9)
                                    {
                                        sampleCount += (uint)data.Length;
                                        decodedSoundBuf.AddRange(data);
                                    }
                                });
                                num11 += (double)((ulong)sampleCount / (ulong)((long)num9)) / (double)num10;
                            }
                        }
                        soundDataTrack.Duration += num11;
                        soundDataTrack.Samples = decodedSoundBuf.ToArray();
                        MaxPeakProvider maxPeakProvider = new MaxPeakProvider();
                        RmsPeakProvider rmsPeakProvider = new RmsPeakProvider(200);
                        SamplingPeakProvider samplingPeakProvider = new SamplingPeakProvider(200);
                        AveragePeakProvider averagePeakProvider = new AveragePeakProvider(4f);
                        global::System.Drawing.Color color = global::System.Drawing.Color.FromArgb(64, 83, 22, 3);
                        SoundCloudBlockWaveFormSettings soundCloudBlockWaveFormSettings = new SoundCloudBlockWaveFormSettings(global::System.Drawing.Color.FromArgb(196, 197, 53, 0), color, global::System.Drawing.Color.FromArgb(196, 79, 26, 0), global::System.Drawing.Color.FromArgb(64, 79, 79, 79))
                        {
                            Name = "SoundCloud Orange Transparent Blocks",
                            PixelsPerPeak = 2,
                            SpacerPixels = 1,
                            TopSpacerGradientStartColor = color,
                            BackgroundColor = global::System.Drawing.Color.Transparent,
                            Width = 800,
                            TopHeight = 50,
                            BottomHeight = 30
                        };
                        try
                        {
                            WaveFormRenderer waveFormRenderer = new WaveFormRenderer();
                            Image image = waveFormRenderer.Render(decodedSoundBuf.ToArray(), maxPeakProvider, soundCloudBlockWaveFormSettings);
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
                                    drawingContext.DrawImage(bitmapImage, new Rect(0.0, 0.0, bitmapImage.Width, bitmapImage.Height));
                                }
                                renderTargetBitmap.Render(drawingVisual);
                                renderTargetBitmap.Freeze();
                                soundDataTrack.WaveForm = renderTargetBitmap;
                            }
                        }
                        catch (Exception ex)
                        {
                            // In a real application, you should log or handle ex here
                        }
                        soundDataTrack.SegmentCount = 1;
                        list.Add(soundDataTrack);
                    }
                }
            }
            return list;
        }
    }
}

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
using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using Microsoft.CSharp.RuntimeBinder;
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
            List<SoundDataTrack> list = new List<SoundDataTrack>();
            object rootObject = base.RootObject;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__2 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__2 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, object, object, object> target = FrostyHarmonySampleBankEditor.<> o__2.<> p__2.Target;
            CallSite<> p__ = FrostyHarmonySampleBankEditor.<> o__2.<> p__2;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__0 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Chunks", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj = FrostyHarmonySampleBankEditor.<> o__2.<> p__0.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__0, rootObject);
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__1 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "RamChunkIndex", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj2 = target(<> p__, obj, FrostyHarmonySampleBankEditor.<> o__2.<> p__1.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__1, rootObject));
            int num = 0;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__5 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__5 = CallSite<Func<CallSite, object, ChunkAssetEntry>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ChunkAssetEntry), typeof(FrostyHarmonySampleBankEditor)));
            }
            Func<CallSite, object, ChunkAssetEntry> target2 = FrostyHarmonySampleBankEditor.<> o__2.<> p__5.Target;
            CallSite<> p__2 = FrostyHarmonySampleBankEditor.<> o__2.<> p__5;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__4 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__4 = CallSite<Func<CallSite, AssetManager, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunkEntry", null, typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, AssetManager, object, object> target3 = FrostyHarmonySampleBankEditor.<> o__2.<> p__4.Target;
            CallSite<> p__3 = FrostyHarmonySampleBankEditor.<> o__2.<> p__4;
            AssetManager assetManager = App.AssetManager;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__3 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkId", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            ChunkAssetEntry chunkAssetEntry = target2(<> p__2, target3(<> p__3, assetManager, FrostyHarmonySampleBankEditor.<> o__2.<> p__3.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__3, obj2)));
            NativeReader nativeReader = null;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__8 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__8 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            Func<CallSite, object, bool> target4 = FrostyHarmonySampleBankEditor.<> o__2.<> p__8.Target;
            CallSite<> p__4 = FrostyHarmonySampleBankEditor.<> o__2.<> p__8;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__7 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__7 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            Func<CallSite, object, int, object> target5 = FrostyHarmonySampleBankEditor.<> o__2.<> p__7.Target;
            CallSite<> p__5 = FrostyHarmonySampleBankEditor.<> o__2.<> p__7;
            if (FrostyHarmonySampleBankEditor.<> o__2.<> p__6 == null)
            {
                FrostyHarmonySampleBankEditor.<> o__2.<> p__6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "StreamChunkIndex", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            bool flag = target4(<> p__4, target5(<> p__5, FrostyHarmonySampleBankEditor.<> o__2.<> p__6.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__6, rootObject), 255));
            if (flag)
            {
                if (FrostyHarmonySampleBankEditor.<> o__2.<> p__11 == null)
                {
                    FrostyHarmonySampleBankEditor.<> o__2.<> p__11 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                    }));
                }
                Func<CallSite, object, object, object> target6 = FrostyHarmonySampleBankEditor.<> o__2.<> p__11.Target;
                CallSite<> p__6 = FrostyHarmonySampleBankEditor.<> o__2.<> p__11;
                if (FrostyHarmonySampleBankEditor.<> o__2.<> p__9 == null)
                {
                    FrostyHarmonySampleBankEditor.<> o__2.<> p__9 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Chunks", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                object obj3 = FrostyHarmonySampleBankEditor.<> o__2.<> p__9.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__9, rootObject);
                if (FrostyHarmonySampleBankEditor.<> o__2.<> p__10 == null)
                {
                    FrostyHarmonySampleBankEditor.<> o__2.<> p__10 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "StreamChunkIndex", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                object obj4 = target6(<> p__6, obj3, FrostyHarmonySampleBankEditor.<> o__2.<> p__10.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__10, rootObject));
                if (FrostyHarmonySampleBankEditor.<> o__2.<> p__14 == null)
                {
                    FrostyHarmonySampleBankEditor.<> o__2.<> p__14 = CallSite<Func<CallSite, object, ChunkAssetEntry>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ChunkAssetEntry), typeof(FrostyHarmonySampleBankEditor)));
                }
                Func<CallSite, object, ChunkAssetEntry> target7 = FrostyHarmonySampleBankEditor.<> o__2.<> p__14.Target;
                CallSite<> p__7 = FrostyHarmonySampleBankEditor.<> o__2.<> p__14;
                if (FrostyHarmonySampleBankEditor.<> o__2.<> p__13 == null)
                {
                    FrostyHarmonySampleBankEditor.<> o__2.<> p__13 = CallSite<Func<CallSite, AssetManager, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunkEntry", null, typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                    }));
                }
                Func<CallSite, AssetManager, object, object> target8 = FrostyHarmonySampleBankEditor.<> o__2.<> p__13.Target;
                CallSite<> p__8 = FrostyHarmonySampleBankEditor.<> o__2.<> p__13;
                AssetManager assetManager2 = App.AssetManager;
                if (FrostyHarmonySampleBankEditor.<> o__2.<> p__12 == null)
                {
                    FrostyHarmonySampleBankEditor.<> o__2.<> p__12 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkId", typeof(FrostyHarmonySampleBankEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                ChunkAssetEntry chunkAssetEntry2 = target7(<> p__7, target8(<> p__8, assetManager2, FrostyHarmonySampleBankEditor.<> o__2.<> p__12.Target(FrostyHarmonySampleBankEditor.<> o__2.<> p__12, obj4)));
                nativeReader = new NativeReader(App.AssetManager.GetChunk(chunkAssetEntry2));
            }
            using (NativeReader nativeReader2 = new NativeReader(App.AssetManager.GetChunk(chunkAssetEntry)))
            {
                nativeReader2.Position = 10L;
                int num2 = (int)nativeReader2.ReadUShort(0);
                nativeReader2.Position = 32L;
                int num3 = nativeReader2.ReadInt(0);
                nativeReader2.Position = 80L;
                List<int> list2 = new List<int>();
                for (int i = 0; i < num2; i++)
                {
                    list2.Add(nativeReader2.ReadInt(0));
                    nativeReader2.Position += 4L;
                }
                foreach (int num4 in list2)
                {
                    nativeReader2.Position = (long)(num4 + 60);
                    int num5 = (int)nativeReader2.ReadUShort(0);
                    nativeReader2.Position += 10L;
                    int num6 = -1;
                    bool flag2 = false;
                    for (int j = 0; j < num5; j++)
                    {
                        uint num7 = nativeReader2.ReadUInt(0);
                        bool flag3 = num7 == 776947270U;
                        if (flag3)
                        {
                            nativeReader2.Position += 4L;
                            num6 = nativeReader2.ReadInt(0);
                            nativeReader2.Position += 12L;
                            flag2 = true;
                        }
                        else
                        {
                            bool flag4 = num7 == 777142605U;
                            if (flag4)
                            {
                                nativeReader2.Position += 4L;
                                num6 = nativeReader2.ReadInt(0) + num3;
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
                            nativeReader3 = nativeReader;
                        }
                        SoundDataTrack soundDataTrack = new SoundDataTrack
                        {
                            Name = "Track #" + num++.ToString()
                        };
                        nativeReader3.Position = (long)num6;
                        List<short> decodedSoundBuf = new List<short>();
                        uint num8 = nativeReader3.ReadUInt(1) & 16777215U;
                        byte b = nativeReader3.ReadByte();
                        int num9 = (nativeReader3.ReadByte() >> 2) + 1;
                        ushort num10 = nativeReader3.ReadUShort(1);
                        uint sampleCount = nativeReader3.ReadUInt(1) & 16777215U;
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
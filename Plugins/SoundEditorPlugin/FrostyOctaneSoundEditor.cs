using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using Microsoft.CSharp.RuntimeBinder;
using WaveFormRendererLib;

namespace SoundEditorPlugin
{

    public class FrostyOctaneSoundEditor : FrostySoundDataEditor
    {

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
            object rootObject = base.RootObject;
            List<SoundDataTrack> list = new List<SoundDataTrack>();
            if (FrostyOctaneSoundEditor.<> o__3.<> p__2 == null)
            {
                FrostyOctaneSoundEditor.<> o__3.<> p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkId", typeof(FrostyOctaneSoundEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            Func<CallSite, object, object> target = FrostyOctaneSoundEditor.<> o__3.<> p__2.Target;
            CallSite<> p__ = FrostyOctaneSoundEditor.<> o__3.<> p__2;
            if (FrostyOctaneSoundEditor.<> o__3.<> p__1 == null)
            {
                FrostyOctaneSoundEditor.<> o__3.<> p__1 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostyOctaneSoundEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                }));
            }
            Func<CallSite, object, int, object> target2 = FrostyOctaneSoundEditor.<> o__3.<> p__1.Target;
            CallSite<> p__2 = FrostyOctaneSoundEditor.<> o__3.<> p__1;
            if (FrostyOctaneSoundEditor.<> o__3.<> p__0 == null)
            {
                FrostyOctaneSoundEditor.<> o__3.<> p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Chunks", typeof(FrostyOctaneSoundEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            object obj = target(<> p__, target2(<> p__2, FrostyOctaneSoundEditor.<> o__3.<> p__0.Target(FrostyOctaneSoundEditor.<> o__3.<> p__0, rootObject), 0));
            SoundDataTrack soundDataTrack = new SoundDataTrack
            {
                ChannelCount = 1,
                Codec = "XAS Interleaved v0",
                Name = "Track #1"
            };
            double num = 0.0;
            double num2 = 0.0;
            List<short> list2 = new List<short>();
            if (FrostyOctaneSoundEditor.<> o__3.<> p__5 == null)
            {
                FrostyOctaneSoundEditor.<> o__3.<> p__5 = CallSite<Func<CallSite, object, MemoryStream>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(MemoryStream), typeof(FrostyOctaneSoundEditor)));
            }
            Func<CallSite, object, MemoryStream> target3 = FrostyOctaneSoundEditor.<> o__3.<> p__5.Target;
            CallSite<> p__3 = FrostyOctaneSoundEditor.<> o__3.<> p__5;
            if (FrostyOctaneSoundEditor.<> o__3.<> p__4 == null)
            {
                FrostyOctaneSoundEditor.<> o__3.<> p__4 = CallSite<Func<CallSite, AssetManager, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunk", null, typeof(FrostyOctaneSoundEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            Func<CallSite, AssetManager, object, object> target4 = FrostyOctaneSoundEditor.<> o__3.<> p__4.Target;
            CallSite<> p__4 = FrostyOctaneSoundEditor.<> o__3.<> p__4;
            AssetManager assetManager = App.AssetManager;
            if (FrostyOctaneSoundEditor.<> o__3.<> p__3 == null)
            {
                FrostyOctaneSoundEditor.<> o__3.<> p__3 = CallSite<Func<CallSite, AssetManager, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunkEntry", null, typeof(FrostyOctaneSoundEditor), new CSharpArgumentInfo[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                }));
            }
            MemoryStream memoryStream = target3(<> p__3, target4(<> p__4, assetManager, FrostyOctaneSoundEditor.<> o__3.<> p__3.Target(FrostyOctaneSoundEditor.<> o__3.<> p__3, App.AssetManager, obj)));
            using (NativeReader nativeReader = new NativeReader(memoryStream))
            {
                string text = nativeReader.ReadSizedString(4);
                uint num3 = nativeReader.ReadUInt(0);
                float num4 = nativeReader.ReadFloat(0);
                float num5 = nativeReader.ReadFloat(0);
                uint num6 = nativeReader.ReadUInt(0);
                uint num7 = nativeReader.ReadUInt(0);
                uint num8 = nativeReader.ReadUInt(0);
                soundDataTrack.SampleRate = nativeReader.ReadInt(0);
                nativeReader.ReadInt(0);
                int[] array = new int[num6];
                int num9 = 0;
                while ((long)num9 < (long)((ulong)num6))
                {
                    array[num9] = nativeReader.ReadInt(0);
                    num9++;
                }
                nativeReader.ReadInt(0);
                int[] array2 = new int[num7];
                int num10 = 0;
                while ((long)num10 < (long)((ulong)num7))
                {
                    array2[num10] = nativeReader.ReadInt(0);
                    num10++;
                }
                long num11 = nativeReader.Position;
                bool flag = num11 != (long)((ulong)((num6 + 1U + num7 + 1U) * 4U + 32U));
                if (flag)
                {
                    this.logger.LogError("Wrong offset after Tables", Array.Empty<object>());
                    num11 = (long)((ulong)((num6 + 1U + num7 + 1U) * 4U + 32U));
                }
                int num12 = 0;
                while ((long)num12 < (nativeReader.Length - num11) / 19L)
                {
                    short[] array3 = new short[32];
                    uint num13 = nativeReader.ReadUInt(0);
                    int num14 = FrostyOctaneSoundEditor.EA_XA_TABLE[(int)(num13 & 15U)];
                    int num15 = FrostyOctaneSoundEditor.EA_XA_TABLE[(int)((num13 & 15U) + 4U)];
                    short num16 = (short)((num13 >> 16) & 65520U);
                    short num17 = (short)(num13 & 65520U);
                    byte b = (byte)((num13 >> 16) & 15U);
                    array3[0] = num17;
                    array3[1] = num16;
                    for (int i = 0; i < 15; i++)
                    {
                        byte b2 = nativeReader.ReadByte();
                        for (int j = 0; j < 2; j++)
                        {
                            int num18 = 0;
                            bool flag2 = j == 0;
                            if (flag2)
                            {
                                num18 = (b2 & 240) >> 4;
                            }
                            else
                            {
                                bool flag3 = j == 1;
                                if (flag3)
                                {
                                    num18 = (int)(b2 & 15);
                                }
                            }
                            bool flag4 = num18 > 7;
                            if (flag4)
                            {
                                num18 -= 16;
                            }
                            int num19 = (int)num16 * num14 + (int)num17 * num15;
                            num18 = (num18 << (int)(20 - b)) + num19 + 128 >> 8;
                            bool flag5 = num18 > 32767;
                            if (flag5)
                            {
                                num18 = 32767;
                            }
                            else
                            {
                                bool flag6 = num18 < -32768;
                                if (flag6)
                                {
                                    num18 = -32768;
                                }
                            }
                            array3[2 + i * 2 + j] = (short)num18;
                            num17 = num16;
                            num16 = (short)num18;
                        }
                    }
                    list2.AddRange(array3);
                    num12++;
                }
                soundDataTrack.Samples = list2.ToArray();
                soundDataTrack.Duration = (double)(list2.Count / soundDataTrack.SampleRate);
                list.Add(soundDataTrack);
            }
            MaxPeakProvider maxPeakProvider = new MaxPeakProvider();
            RmsPeakProvider rmsPeakProvider = new RmsPeakProvider(200);
            SamplingPeakProvider samplingPeakProvider = new SamplingPeakProvider(200);
            AveragePeakProvider averagePeakProvider = new AveragePeakProvider(4f);
            global::System.Drawing.Color color = global::System.Drawing.Color.FromArgb(64, 83, 22, 3);
            SoundCloudBlockWaveFormSettings soundCloudBlockWaveFormSettings = new SoundCloudBlockWaveFormSettings(global::System.Drawing.Color.FromArgb(255, 218, 218, 218), color, global::System.Drawing.Color.FromArgb(255, 109, 109, 109), global::System.Drawing.Color.FromArgb(64, 79, 79, 79))
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
                WaveFormRenderer waveFormRenderer = new WaveFormRenderer();
                Image image = waveFormRenderer.Render(soundDataTrack.Samples, maxPeakProvider, soundCloudBlockWaveFormSettings);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    image.Save(memoryStream2, ImageFormat.Png);
                    memoryStream2.Seek(0L, SeekOrigin.Begin);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream2;
                    bitmapImage.EndInit();
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, bitmapImage.DpiX, bitmapImage.DpiY, PixelFormats.Pbgra32);
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingVisual.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                        drawingContext.DrawImage(bitmapImage, new Rect(0.0, 0.0, bitmapImage.Width, bitmapImage.Height));
                        bool flag7 = num2 > 0.0;
                        if (flag7)
                        {
                            drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)(num / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)soundCloudBlockWaveFormSettings.TopHeight), new global::System.Windows.Point((double)((int)(num / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                            drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)((num + num2) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)soundCloudBlockWaveFormSettings.TopHeight), new global::System.Windows.Point((double)((int)((num + num2) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                            drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)(num / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)), new global::System.Windows.Point((double)((int)((num + num2) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                        }
                    }
                    renderTargetBitmap.Render(drawingVisual);
                    renderTargetBitmap.Freeze();
                    soundDataTrack.WaveForm = renderTargetBitmap;
                }
            }
            catch (Exception ex)
            {
            }
            return list;
        }

        private static int[] EA_XA_TABLE = new int[]
        {
            0, 240, 460, 392, 0, 0, -208, -220, 0, 1,
            3, 4, 7, 8, 10, 11, 0, -1, -3, -4
        };
    }
}
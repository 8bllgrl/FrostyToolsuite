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
using SoundEditorPlugin.Resources;
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
            List<SoundDataTrack> list = new List<SoundDataTrack>();
            object rootObject = base.RootObject;
            AssetManager assetManager = App.AssetManager;
            AssetManager assetManager2 = App.AssetManager;
            if (FrostyNewWaveEditor.<> o__2.<> p__1 == null)
            {
                FrostyNewWaveEditor.<> o__2.<> p__1 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(FrostyNewWaveEditor)));
            }
            Func<CallSite, object, string> target = FrostyNewWaveEditor.<> o__2.<> p__1.Target;
            CallSite<> p__ = FrostyNewWaveEditor.<> o__2.<> p__1;
            if (FrostyNewWaveEditor.<> o__2.<> p__0 == null)
            {
                FrostyNewWaveEditor.<> o__2.<> p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            NewWaveResource resAs = assetManager.GetResAs<NewWaveResource>(assetManager2.GetResEntry(target(<> p__, FrostyNewWaveEditor.<> o__2.<> p__0.Target(FrostyNewWaveEditor.<> o__2.<> p__0, rootObject)).ToLower()), null);
            int num = 0;
            int count2 = resAs.Variations.Count;
            foreach (object obj in resAs.Variations)
            {
                task.Update("Loading track #" + (num + 1).ToString(), new double?((double)(num + 1) / (double)count2 * 100.0));
                SoundDataTrack soundDataTrack = new SoundDataTrack
                {
                    Name = "Track #" + (num++ + 1).ToString()
                };
                List<Segment> segments = resAs.Segments;
                if (FrostyNewWaveEditor.<> o__2.<> p__3 == null)
                {
                    FrostyNewWaveEditor.<> o__2.<> p__3 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostyNewWaveEditor)));
                }
                Func<CallSite, object, int> target2 = FrostyNewWaveEditor.<> o__2.<> p__3.Target;
                CallSite<> p__2 = FrostyNewWaveEditor.<> o__2.<> p__3;
                if (FrostyNewWaveEditor.<> o__2.<> p__2 == null)
                {
                    FrostyNewWaveEditor.<> o__2.<> p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                int num2;
                if (segments[target2(<> p__2, FrostyNewWaveEditor.<> o__2.<> p__2.Target(FrostyNewWaveEditor.<> o__2.<> p__2, obj))].SamplesOffsetFlag != 1U)
                {
                    if (FrostyNewWaveEditor.<> o__2.<> p__7 == null)
                    {
                        FrostyNewWaveEditor.<> o__2.<> p__7 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostyNewWaveEditor)));
                    }
                    Func<CallSite, object, int> target3 = FrostyNewWaveEditor.<> o__2.<> p__7.Target;
                    CallSite<> p__3 = FrostyNewWaveEditor.<> o__2.<> p__7;
                    if (FrostyNewWaveEditor.<> o__2.<> p__6 == null)
                    {
                        FrostyNewWaveEditor.<> o__2.<> p__6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "StreamChunkIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    num2 = target3(<> p__3, FrostyNewWaveEditor.<> o__2.<> p__6.Target(FrostyNewWaveEditor.<> o__2.<> p__6, obj));
                }
                else
                {
                    if (FrostyNewWaveEditor.<> o__2.<> p__5 == null)
                    {
                        FrostyNewWaveEditor.<> o__2.<> p__5 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostyNewWaveEditor)));
                    }
                    Func<CallSite, object, int> target4 = FrostyNewWaveEditor.<> o__2.<> p__5.Target;
                    CallSite<> p__4 = FrostyNewWaveEditor.<> o__2.<> p__5;
                    if (FrostyNewWaveEditor.<> o__2.<> p__4 == null)
                    {
                        FrostyNewWaveEditor.<> o__2.<> p__4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MemoryChunkIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    num2 = target4(<> p__4, FrostyNewWaveEditor.<> o__2.<> p__4.Target(FrostyNewWaveEditor.<> o__2.<> p__4, obj));
                }
                int num3 = num2;
                object obj2 = resAs.Chunks[num3];
                if (FrostyNewWaveEditor.<> o__2.<> p__10 == null)
                {
                    FrostyNewWaveEditor.<> o__2.<> p__10 = CallSite<Func<CallSite, object, ChunkAssetEntry>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ChunkAssetEntry), typeof(FrostyNewWaveEditor)));
                }
                Func<CallSite, object, ChunkAssetEntry> target5 = FrostyNewWaveEditor.<> o__2.<> p__10.Target;
                CallSite<> p__5 = FrostyNewWaveEditor.<> o__2.<> p__10;
                if (FrostyNewWaveEditor.<> o__2.<> p__9 == null)
                {
                    FrostyNewWaveEditor.<> o__2.<> p__9 = CallSite<Func<CallSite, AssetManager, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunkEntry", null, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                    }));
                }
                Func<CallSite, AssetManager, object, object> target6 = FrostyNewWaveEditor.<> o__2.<> p__9.Target;
                CallSite<> p__6 = FrostyNewWaveEditor.<> o__2.<> p__9;
                AssetManager assetManager3 = App.AssetManager;
                if (FrostyNewWaveEditor.<> o__2.<> p__8 == null)
                {
                    FrostyNewWaveEditor.<> o__2.<> p__8 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkId", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                ChunkAssetEntry chunkAssetEntry = target5(<> p__5, target6(<> p__6, assetManager3, FrostyNewWaveEditor.<> o__2.<> p__8.Target(FrostyNewWaveEditor.<> o__2.<> p__8, obj2)));
                bool flag = chunkAssetEntry == null;
                if (!flag)
                {
                    using (NativeReader nativeReader = new NativeReader(App.AssetManager.GetChunk(chunkAssetEntry)))
                    {
                        List<short> decodedSoundBuf = new List<short>();
                        double num4 = 0.0;
                        double num5 = 0.0;
                        int num6 = 0;
                        for (; ; )
                        {
                            if (FrostyNewWaveEditor.<> o__2.<> p__13 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__13 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target7 = FrostyNewWaveEditor.<> o__2.<> p__13.Target;
                            CallSite<> p__7 = FrostyNewWaveEditor.<> o__2.<> p__13;
                            if (FrostyNewWaveEditor.<> o__2.<> p__12 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__12 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, int, object, object> target8 = FrostyNewWaveEditor.<> o__2.<> p__12.Target;
                            CallSite<> p__8 = FrostyNewWaveEditor.<> o__2.<> p__12;
                            int num7 = num6;
                            if (FrostyNewWaveEditor.<> o__2.<> p__11 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__11 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            if (!target7(<> p__7, target8(<> p__8, num7, FrostyNewWaveEditor.<> o__2.<> p__11.Target(FrostyNewWaveEditor.<> o__2.<> p__11, obj))))
                            {
                                goto Block_57;
                            }
                            List<Segment> segments2 = resAs.Segments;
                            if (FrostyNewWaveEditor.<> o__2.<> p__15 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__15 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostyNewWaveEditor)));
                            }
                            Func<CallSite, object, int> target9 = FrostyNewWaveEditor.<> o__2.<> p__15.Target;
                            CallSite<> p__9 = FrostyNewWaveEditor.<> o__2.<> p__15;
                            if (FrostyNewWaveEditor.<> o__2.<> p__14 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__14 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Segment segment = segments2[target9(<> p__9, FrostyNewWaveEditor.<> o__2.<> p__14.Target(FrostyNewWaveEditor.<> o__2.<> p__14, obj)) + num6];
                            nativeReader.Position = (long)((ulong)segment.SamplesOffset);
                            bool flag2 = nativeReader.ReadUShort(0) != 72;
                            if (flag2)
                            {
                                break;
                            }
                            ushort num8 = nativeReader.ReadUShort(1);
                            byte b = nativeReader.ReadByte() & 15;
                            int num9 = (nativeReader.ReadByte() >> 2) + 1;
                            ushort num10 = nativeReader.ReadUShort(1);
                            uint sampleCount = nativeReader.ReadUInt(1) & 268435455U;
                            switch (b)
                            {
                                case 1:
                                    soundDataTrack.Codec = "Unknown";
                                    break;
                                case 2:
                                    soundDataTrack.Codec = "PCM 16 Big";
                                    break;
                                case 3:
                                    soundDataTrack.Codec = "EA-XMA";
                                    break;
                                case 4:
                                    soundDataTrack.Codec = "XAS Interleaved v1";
                                    break;
                                case 5:
                                    soundDataTrack.Codec = "EALayer3 Interleaved v1";
                                    break;
                                case 6:
                                    soundDataTrack.Codec = "EALayer3 Interleaved v2 PCM";
                                    break;
                                case 7:
                                    soundDataTrack.Codec = "EALayer3 Interleaved v2 Spike";
                                    break;
                                case 9:
                                    soundDataTrack.Codec = "EASpeex";
                                    break;
                                case 10:
                                    soundDataTrack.Codec = "Unknown";
                                    break;
                                case 11:
                                    soundDataTrack.Codec = "EA-MP3";
                                    break;
                                case 12:
                                    soundDataTrack.Codec = "EAOpus";
                                    break;
                                case 13:
                                    soundDataTrack.Codec = "EAAtrac9";
                                    break;
                                case 14:
                                    soundDataTrack.Codec = "MultiStream Opus";
                                    break;
                                case 15:
                                    soundDataTrack.Codec = "MultiStream Opus (Uncoupled)";
                                    break;
                            }
                            if (FrostyNewWaveEditor.<> o__2.<> p__22 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__22 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target10 = FrostyNewWaveEditor.<> o__2.<> p__22.Target;
                            CallSite<> p__10 = FrostyNewWaveEditor.<> o__2.<> p__22;
                            if (FrostyNewWaveEditor.<> o__2.<> p__17 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__17 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, int, object, object> target11 = FrostyNewWaveEditor.<> o__2.<> p__17.Target;
                            CallSite<> p__11 = FrostyNewWaveEditor.<> o__2.<> p__17;
                            int num11 = num6;
                            if (FrostyNewWaveEditor.<> o__2.<> p__16 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__16 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstLoopSegmentIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj3 = target11(<> p__11, num11, FrostyNewWaveEditor.<> o__2.<> p__16.Target(FrostyNewWaveEditor.<> o__2.<> p__16, obj));
                            if (FrostyNewWaveEditor.<> o__2.<> p__21 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__21 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj5;
                            if (!FrostyNewWaveEditor.<> o__2.<> p__21.Target(FrostyNewWaveEditor.<> o__2.<> p__21, obj3))
                            {
                                if (FrostyNewWaveEditor.<> o__2.<> p__20 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__20 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                Func<CallSite, object, object, object> target12 = FrostyNewWaveEditor.<> o__2.<> p__20.Target;
                                CallSite<> p__12 = FrostyNewWaveEditor.<> o__2.<> p__20;
                                object obj4 = obj3;
                                if (FrostyNewWaveEditor.<> o__2.<> p__19 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__19 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.GreaterThan, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target13 = FrostyNewWaveEditor.<> o__2.<> p__19.Target;
                                CallSite<> p__13 = FrostyNewWaveEditor.<> o__2.<> p__19;
                                if (FrostyNewWaveEditor.<> o__2.<> p__18 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__18 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                obj5 = target12(<> p__12, obj4, target13(<> p__13, FrostyNewWaveEditor.<> o__2.<> p__18.Target(FrostyNewWaveEditor.<> o__2.<> p__18, obj), 1));
                            }
                            else
                            {
                                obj5 = obj3;
                            }
                            bool flag3 = target10(<> p__10, obj5);
                            if (flag3)
                            {
                                num4 = (double)(decodedSoundBuf.Count / num9) / (double)num10;
                                soundDataTrack.LoopStart = (uint)decodedSoundBuf.Count;
                            }
                            nativeReader.Position = (long)((ulong)segment.SamplesOffset);
                            byte[] array = nativeReader.ReadToEnd();
                            double num12 = 0.0;
                            bool flag4 = b == 2;
                            if (flag4)
                            {
                                short[] array2 = Pcm16b.Decode(array);
                                decodedSoundBuf.AddRange(array2);
                                num12 += (double)(array2.Length / num9) / (double)num10;
                                sampleCount = (uint)array2.Length;
                            }
                            else
                            {
                                bool flag5 = b == 4;
                                if (flag5)
                                {
                                    short[] array3 = XAS.Decode(array);
                                    decodedSoundBuf.AddRange(array3);
                                    num12 += (double)(array3.Length / num9) / (double)num10;
                                    sampleCount = (uint)array3.Length;
                                }
                                else
                                {
                                    bool flag6 = b == 5 || b == 6 || b == 12;
                                    if (flag6)
                                    {
                                        sampleCount = 0U;
                                        EALayer3.Decode(array, array.Length, delegate (short[] data, int count, EALayer3.StreamInfo info)
                                        {
                                            bool flag11 = info.streamIndex == -1;
                                            if (!flag11)
                                            {
                                                sampleCount += (uint)data.Length;
                                                decodedSoundBuf.AddRange(data);
                                            }
                                        });
                                        num12 += (double)((ulong)sampleCount / (ulong)((long)num9)) / (double)num10;
                                    }
                                }
                            }
                            if (FrostyNewWaveEditor.<> o__2.<> p__25 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__25 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target14 = FrostyNewWaveEditor.<> o__2.<> p__25.Target;
                            CallSite<> p__14 = FrostyNewWaveEditor.<> o__2.<> p__25;
                            if (FrostyNewWaveEditor.<> o__2.<> p__24 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__24 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.GreaterThan, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                }));
                            }
                            Func<CallSite, object, int, object> target15 = FrostyNewWaveEditor.<> o__2.<> p__24.Target;
                            CallSite<> p__15 = FrostyNewWaveEditor.<> o__2.<> p__24;
                            if (FrostyNewWaveEditor.<> o__2.<> p__23 == null)
                            {
                                FrostyNewWaveEditor.<> o__2.<> p__23 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            bool flag7 = target14(<> p__14, target15(<> p__15, FrostyNewWaveEditor.<> o__2.<> p__23.Target(FrostyNewWaveEditor.<> o__2.<> p__23, obj), 1));
                            if (flag7)
                            {
                                if (FrostyNewWaveEditor.<> o__2.<> p__28 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__28 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                Func<CallSite, object, bool> target16 = FrostyNewWaveEditor.<> o__2.<> p__28.Target;
                                CallSite<> p__16 = FrostyNewWaveEditor.<> o__2.<> p__28;
                                if (FrostyNewWaveEditor.<> o__2.<> p__27 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__27 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                Func<CallSite, int, object, object> target17 = FrostyNewWaveEditor.<> o__2.<> p__27.Target;
                                CallSite<> p__17 = FrostyNewWaveEditor.<> o__2.<> p__27;
                                int num13 = num6;
                                if (FrostyNewWaveEditor.<> o__2.<> p__26 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__26 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstLoopSegmentIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                bool flag8 = target16(<> p__16, target17(<> p__17, num13, FrostyNewWaveEditor.<> o__2.<> p__26.Target(FrostyNewWaveEditor.<> o__2.<> p__26, obj)));
                                if (flag8)
                                {
                                    num4 += num12;
                                    soundDataTrack.LoopStart += sampleCount;
                                }
                                if (FrostyNewWaveEditor.<> o__2.<> p__35 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__35 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                Func<CallSite, object, bool> target18 = FrostyNewWaveEditor.<> o__2.<> p__35.Target;
                                CallSite<> p__18 = FrostyNewWaveEditor.<> o__2.<> p__35;
                                if (FrostyNewWaveEditor.<> o__2.<> p__30 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__30 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.GreaterThanOrEqual, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                Func<CallSite, int, object, object> target19 = FrostyNewWaveEditor.<> o__2.<> p__30.Target;
                                CallSite<> p__19 = FrostyNewWaveEditor.<> o__2.<> p__30;
                                int num14 = num6;
                                if (FrostyNewWaveEditor.<> o__2.<> p__29 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__29 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstLoopSegmentIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                obj3 = target19(<> p__19, num14, FrostyNewWaveEditor.<> o__2.<> p__29.Target(FrostyNewWaveEditor.<> o__2.<> p__29, obj));
                                if (FrostyNewWaveEditor.<> o__2.<> p__34 == null)
                                {
                                    FrostyNewWaveEditor.<> o__2.<> p__34 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                object obj7;
                                if (!FrostyNewWaveEditor.<> o__2.<> p__34.Target(FrostyNewWaveEditor.<> o__2.<> p__34, obj3))
                                {
                                    if (FrostyNewWaveEditor.<> o__2.<> p__33 == null)
                                    {
                                        FrostyNewWaveEditor.<> o__2.<> p__33 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                        {
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                        }));
                                    }
                                    Func<CallSite, object, object, object> target20 = FrostyNewWaveEditor.<> o__2.<> p__33.Target;
                                    CallSite<> p__20 = FrostyNewWaveEditor.<> o__2.<> p__33;
                                    object obj6 = obj3;
                                    if (FrostyNewWaveEditor.<> o__2.<> p__32 == null)
                                    {
                                        FrostyNewWaveEditor.<> o__2.<> p__32 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThanOrEqual, typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[]
                                        {
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                        }));
                                    }
                                    Func<CallSite, int, object, object> target21 = FrostyNewWaveEditor.<> o__2.<> p__32.Target;
                                    CallSite<> p__21 = FrostyNewWaveEditor.<> o__2.<> p__32;
                                    int num15 = num6;
                                    if (FrostyNewWaveEditor.<> o__2.<> p__31 == null)
                                    {
                                        FrostyNewWaveEditor.<> o__2.<> p__31 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "LastLoopSegmentIndex", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                    }
                                    obj7 = target20(<> p__20, obj6, target21(<> p__21, num15, FrostyNewWaveEditor.<> o__2.<> p__31.Target(FrostyNewWaveEditor.<> o__2.<> p__31, obj)));
                                }
                                else
                                {
                                    obj7 = obj3;
                                }
                                bool flag9 = target18(<> p__18, obj7);
                                if (flag9)
                                {
                                    num5 += num12;
                                    soundDataTrack.LoopEnd += sampleCount;
                                }
                            }
                            soundDataTrack.SampleRate = (int)num10;
                            soundDataTrack.ChannelCount = num9;
                            soundDataTrack.Duration += num12;
                            num6++;
                        }
                        this.logger.LogError("Wrong Sample Offset at Variation {0}, Segment {1}", new object[] { num, num6 });
                        return list;
                    Block_57:
                        soundDataTrack.LoopEnd += soundDataTrack.LoopStart;
                        soundDataTrack.Samples = decodedSoundBuf.ToArray();
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
                                    bool flag10 = num5 > 0.0;
                                    if (flag10)
                                    {
                                        drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)(num4 / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)soundCloudBlockWaveFormSettings.TopHeight), new global::System.Windows.Point((double)((int)(num4 / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                                        drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)((num4 + num5) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)soundCloudBlockWaveFormSettings.TopHeight), new global::System.Windows.Point((double)((int)((num4 + num5) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                                        drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)(num4 / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)), new global::System.Windows.Point((double)((int)((num4 + num5) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
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
                        SoundDataTrack soundDataTrack2 = soundDataTrack;
                        if (FrostyNewWaveEditor.<> o__2.<> p__37 == null)
                        {
                            FrostyNewWaveEditor.<> o__2.<> p__37 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), typeof(FrostyNewWaveEditor)));
                        }
                        Func<CallSite, object, int> target22 = FrostyNewWaveEditor.<> o__2.<> p__37.Target;
                        CallSite<> p__22 = FrostyNewWaveEditor.<> o__2.<> p__37;
                        if (FrostyNewWaveEditor.<> o__2.<> p__36 == null)
                        {
                            FrostyNewWaveEditor.<> o__2.<> p__36 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostyNewWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        soundDataTrack2.SegmentCount = target22(<> p__22, FrostyNewWaveEditor.<> o__2.<> p__36.Target(FrostyNewWaveEditor.<> o__2.<> p__36, obj));
                    }
                    list.Add(soundDataTrack);
                }
            }
            return list;
        }
    }
}
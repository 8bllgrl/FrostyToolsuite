using System;
using System.Collections;
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
using FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using Microsoft.CSharp.RuntimeBinder;
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
            List<SoundDataTrack> list = new List<SoundDataTrack>();
            object rootObject = base.RootObject;
            int num = 0;
            if (FrostySoundWaveEditor.<> o__2.<> p__2 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__2 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(int), typeof(FrostySoundWaveEditor)));
            }
            Func<CallSite, object, int> target = FrostySoundWaveEditor.<> o__2.<> p__2.Target;
            CallSite<> p__ = FrostySoundWaveEditor.<> o__2.<> p__2;
            if (FrostySoundWaveEditor.<> o__2.<> p__1 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Count", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            Func<CallSite, object, object> target2 = FrostySoundWaveEditor.<> o__2.<> p__1.Target;
            CallSite<> p__2 = FrostySoundWaveEditor.<> o__2.<> p__1;
            if (FrostySoundWaveEditor.<> o__2.<> p__0 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "RuntimeVariations", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            int num2 = target(<> p__, target2(<> p__2, FrostySoundWaveEditor.<> o__2.<> p__0.Target(FrostySoundWaveEditor.<> o__2.<> p__0, rootObject)));
            if (FrostySoundWaveEditor.<> o__2.<> p__41 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__41 = CallSite<Func<CallSite, object, IEnumerable>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(FrostySoundWaveEditor)));
            }
            Func<CallSite, object, IEnumerable> target3 = FrostySoundWaveEditor.<> o__2.<> p__41.Target;
            CallSite<> p__3 = FrostySoundWaveEditor.<> o__2.<> p__41;
            if (FrostySoundWaveEditor.<> o__2.<> p__3 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "RuntimeVariations", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            foreach (object obj in target3(<> p__3, FrostySoundWaveEditor.<> o__2.<> p__3.Target(FrostySoundWaveEditor.<> o__2.<> p__3, rootObject)))
            {
                task.Update("Loading track #" + (num + 1).ToString(), new double?((double)(num + 1) / (double)num2 * 100.0));
                SoundDataTrack soundDataTrack = new SoundDataTrack
                {
                    Name = "Track #" + (num++ + 1).ToString()
                };
                if (FrostySoundWaveEditor.<> o__2.<> p__6 == null)
                {
                    FrostySoundWaveEditor.<> o__2.<> p__6 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                    }));
                }
                Func<CallSite, object, object, object> target4 = FrostySoundWaveEditor.<> o__2.<> p__6.Target;
                CallSite<> p__4 = FrostySoundWaveEditor.<> o__2.<> p__6;
                if (FrostySoundWaveEditor.<> o__2.<> p__4 == null)
                {
                    FrostySoundWaveEditor.<> o__2.<> p__4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Chunks", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                object obj2 = FrostySoundWaveEditor.<> o__2.<> p__4.Target(FrostySoundWaveEditor.<> o__2.<> p__4, rootObject);
                if (FrostySoundWaveEditor.<> o__2.<> p__5 == null)
                {
                    FrostySoundWaveEditor.<> o__2.<> p__5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkIndex", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                object obj3 = target4(<> p__4, obj2, FrostySoundWaveEditor.<> o__2.<> p__5.Target(FrostySoundWaveEditor.<> o__2.<> p__5, obj));
                if (FrostySoundWaveEditor.<> o__2.<> p__9 == null)
                {
                    FrostySoundWaveEditor.<> o__2.<> p__9 = CallSite<Func<CallSite, object, ChunkAssetEntry>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ChunkAssetEntry), typeof(FrostySoundWaveEditor)));
                }
                Func<CallSite, object, ChunkAssetEntry> target5 = FrostySoundWaveEditor.<> o__2.<> p__9.Target;
                CallSite<> p__5 = FrostySoundWaveEditor.<> o__2.<> p__9;
                if (FrostySoundWaveEditor.<> o__2.<> p__8 == null)
                {
                    FrostySoundWaveEditor.<> o__2.<> p__8 = CallSite<Func<CallSite, AssetManager, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetChunkEntry", null, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                    }));
                }
                Func<CallSite, AssetManager, object, object> target6 = FrostySoundWaveEditor.<> o__2.<> p__8.Target;
                CallSite<> p__6 = FrostySoundWaveEditor.<> o__2.<> p__8;
                AssetManager assetManager = App.AssetManager;
                if (FrostySoundWaveEditor.<> o__2.<> p__7 == null)
                {
                    FrostySoundWaveEditor.<> o__2.<> p__7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ChunkId", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                }
                ChunkAssetEntry chunkAssetEntry = target5(<> p__5, target6(<> p__6, assetManager, FrostySoundWaveEditor.<> o__2.<> p__7.Target(FrostySoundWaveEditor.<> o__2.<> p__7, obj3)));
                bool flag = chunkAssetEntry == null;
                if (!flag)
                {
                    using (NativeReader nativeReader = new NativeReader(App.AssetManager.GetChunk(chunkAssetEntry)))
                    {
                        List<short> decodedSoundBuf = new List<short>();
                        double num3 = 0.0;
                        double num4 = 0.0;
                        int num5 = 0;
                        for (; ; )
                        {
                            if (FrostySoundWaveEditor.<> o__2.<> p__12 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__12 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target7 = FrostySoundWaveEditor.<> o__2.<> p__12.Target;
                            CallSite<> p__7 = FrostySoundWaveEditor.<> o__2.<> p__12;
                            if (FrostySoundWaveEditor.<> o__2.<> p__11 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__11 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, int, object, object> target8 = FrostySoundWaveEditor.<> o__2.<> p__11.Target;
                            CallSite<> p__8 = FrostySoundWaveEditor.<> o__2.<> p__11;
                            int num6 = num5;
                            if (FrostySoundWaveEditor.<> o__2.<> p__10 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__10 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            if (!target7(<> p__7, target8(<> p__8, num6, FrostySoundWaveEditor.<> o__2.<> p__10.Target(FrostySoundWaveEditor.<> o__2.<> p__10, obj))))
                            {
                                goto Block_61;
                            }
                            if (FrostySoundWaveEditor.<> o__2.<> p__16 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__16 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, object, object, object> target9 = FrostySoundWaveEditor.<> o__2.<> p__16.Target;
                            CallSite<> p__9 = FrostySoundWaveEditor.<> o__2.<> p__16;
                            if (FrostySoundWaveEditor.<> o__2.<> p__13 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__13 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Segments", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj4 = FrostySoundWaveEditor.<> o__2.<> p__13.Target(FrostySoundWaveEditor.<> o__2.<> p__13, rootObject);
                            if (FrostySoundWaveEditor.<> o__2.<> p__15 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__15 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                }));
                            }
                            Func<CallSite, object, int, object> target10 = FrostySoundWaveEditor.<> o__2.<> p__15.Target;
                            CallSite<> p__10 = FrostySoundWaveEditor.<> o__2.<> p__15;
                            if (FrostySoundWaveEditor.<> o__2.<> p__14 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__14 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstSegmentIndex", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj5 = target9(<> p__9, obj4, target10(<> p__10, FrostySoundWaveEditor.<> o__2.<> p__14.Target(FrostySoundWaveEditor.<> o__2.<> p__14, obj), num5));
                            NativeReader nativeReader2 = nativeReader;
                            if (FrostySoundWaveEditor.<> o__2.<> p__18 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__18 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(long), typeof(FrostySoundWaveEditor)));
                            }
                            Func<CallSite, object, long> target11 = FrostySoundWaveEditor.<> o__2.<> p__18.Target;
                            CallSite<> p__11 = FrostySoundWaveEditor.<> o__2.<> p__18;
                            if (FrostySoundWaveEditor.<> o__2.<> p__17 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__17 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            nativeReader2.Position = target11(<> p__11, FrostySoundWaveEditor.<> o__2.<> p__17.Target(FrostySoundWaveEditor.<> o__2.<> p__17, obj5));
                            bool flag2 = nativeReader.ReadUShort(0) != 72;
                            if (flag2)
                            {
                                break;
                            }
                            ushort num7 = nativeReader.ReadUShort(1);
                            byte b = nativeReader.ReadByte() & 15;
                            int num8 = (nativeReader.ReadByte() >> 2) + 1;
                            ushort num9 = nativeReader.ReadUShort(1);
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
                            if (FrostySoundWaveEditor.<> o__2.<> p__25 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__25 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target12 = FrostySoundWaveEditor.<> o__2.<> p__25.Target;
                            CallSite<> p__12 = FrostySoundWaveEditor.<> o__2.<> p__25;
                            if (FrostySoundWaveEditor.<> o__2.<> p__20 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__20 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, int, object, object> target13 = FrostySoundWaveEditor.<> o__2.<> p__20.Target;
                            CallSite<> p__13 = FrostySoundWaveEditor.<> o__2.<> p__20;
                            int num10 = num5;
                            if (FrostySoundWaveEditor.<> o__2.<> p__19 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__19 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstLoopSegmentIndex", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj6 = target13(<> p__13, num10, FrostySoundWaveEditor.<> o__2.<> p__19.Target(FrostySoundWaveEditor.<> o__2.<> p__19, obj));
                            if (FrostySoundWaveEditor.<> o__2.<> p__24 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__24 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj8;
                            if (!FrostySoundWaveEditor.<> o__2.<> p__24.Target(FrostySoundWaveEditor.<> o__2.<> p__24, obj6))
                            {
                                if (FrostySoundWaveEditor.<> o__2.<> p__23 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__23 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                Func<CallSite, object, object, object> target14 = FrostySoundWaveEditor.<> o__2.<> p__23.Target;
                                CallSite<> p__14 = FrostySoundWaveEditor.<> o__2.<> p__23;
                                object obj7 = obj6;
                                if (FrostySoundWaveEditor.<> o__2.<> p__22 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__22 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.GreaterThan, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target15 = FrostySoundWaveEditor.<> o__2.<> p__22.Target;
                                CallSite<> p__15 = FrostySoundWaveEditor.<> o__2.<> p__22;
                                if (FrostySoundWaveEditor.<> o__2.<> p__21 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__21 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                obj8 = target14(<> p__14, obj7, target15(<> p__15, FrostySoundWaveEditor.<> o__2.<> p__21.Target(FrostySoundWaveEditor.<> o__2.<> p__21, obj), 1));
                            }
                            else
                            {
                                obj8 = obj6;
                            }
                            bool flag3 = target12(<> p__12, obj8);
                            if (flag3)
                            {
                                num3 = (double)(decodedSoundBuf.Count / num8) / (double)num9;
                                soundDataTrack.LoopStart = (uint)decodedSoundBuf.Count;
                            }
                            NativeReader nativeReader3 = nativeReader;
                            if (FrostySoundWaveEditor.<> o__2.<> p__27 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__27 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(long), typeof(FrostySoundWaveEditor)));
                            }
                            Func<CallSite, object, long> target16 = FrostySoundWaveEditor.<> o__2.<> p__27.Target;
                            CallSite<> p__16 = FrostySoundWaveEditor.<> o__2.<> p__27;
                            if (FrostySoundWaveEditor.<> o__2.<> p__26 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__26 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SamplesOffset", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            nativeReader3.Position = target16(<> p__16, FrostySoundWaveEditor.<> o__2.<> p__26.Target(FrostySoundWaveEditor.<> o__2.<> p__26, obj5));
                            byte[] array = nativeReader.ReadToEnd();
                            bool flag4 = b == 2;
                            if (flag4)
                            {
                                short[] array2 = Pcm16b.Decode(array);
                                decodedSoundBuf.AddRange(array2);
                                sampleCount = (uint)array2.Length;
                            }
                            else
                            {
                                bool flag5 = b == 4;
                                if (flag5)
                                {
                                    short[] array3 = XAS.Decode(array);
                                    decodedSoundBuf.AddRange(array3);
                                    sampleCount = (uint)array3.Length;
                                }
                                else
                                {
                                    bool flag6 = b == 5 || b == 6;
                                    if (flag6)
                                    {
                                        sampleCount = 0U;
                                        EALayer3.Decode(array, array.Length, delegate (short[] data, int count, EALayer3.StreamInfo info)
                                        {
                                            bool flag10 = info.streamIndex == -1;
                                            if (!flag10)
                                            {
                                                sampleCount += (uint)data.Length;
                                                decodedSoundBuf.AddRange(data);
                                            }
                                        });
                                    }
                                }
                            }
                            if (FrostySoundWaveEditor.<> o__2.<> p__34 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__34 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target17 = FrostySoundWaveEditor.<> o__2.<> p__34.Target;
                            CallSite<> p__17 = FrostySoundWaveEditor.<> o__2.<> p__34;
                            if (FrostySoundWaveEditor.<> o__2.<> p__29 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__29 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                }));
                            }
                            Func<CallSite, int, object, object> target18 = FrostySoundWaveEditor.<> o__2.<> p__29.Target;
                            CallSite<> p__18 = FrostySoundWaveEditor.<> o__2.<> p__29;
                            int num11 = num5;
                            if (FrostySoundWaveEditor.<> o__2.<> p__28 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__28 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "LastLoopSegmentIndex", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            obj6 = target18(<> p__18, num11, FrostySoundWaveEditor.<> o__2.<> p__28.Target(FrostySoundWaveEditor.<> o__2.<> p__28, obj));
                            if (FrostySoundWaveEditor.<> o__2.<> p__33 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__33 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            object obj10;
                            if (!FrostySoundWaveEditor.<> o__2.<> p__33.Target(FrostySoundWaveEditor.<> o__2.<> p__33, obj6))
                            {
                                if (FrostySoundWaveEditor.<> o__2.<> p__32 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__32 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                Func<CallSite, object, object, object> target19 = FrostySoundWaveEditor.<> o__2.<> p__32.Target;
                                CallSite<> p__19 = FrostySoundWaveEditor.<> o__2.<> p__32;
                                object obj9 = obj6;
                                if (FrostySoundWaveEditor.<> o__2.<> p__31 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__31 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.GreaterThan, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target20 = FrostySoundWaveEditor.<> o__2.<> p__31.Target;
                                CallSite<> p__20 = FrostySoundWaveEditor.<> o__2.<> p__31;
                                if (FrostySoundWaveEditor.<> o__2.<> p__30 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__30 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                                }
                                obj10 = target19(<> p__19, obj9, target20(<> p__20, FrostySoundWaveEditor.<> o__2.<> p__30.Target(FrostySoundWaveEditor.<> o__2.<> p__30, obj), 1));
                            }
                            else
                            {
                                obj10 = obj6;
                            }
                            bool flag7 = target17(<> p__17, obj10);
                            if (flag7)
                            {
                                num4 = (double)(decodedSoundBuf.Count / num8) / (double)num9 - num3;
                                soundDataTrack.LoopEnd = (uint)decodedSoundBuf.Count;
                            }
                            soundDataTrack.SampleRate = (int)num9;
                            soundDataTrack.ChannelCount = num8;
                            if (FrostySoundWaveEditor.<> o__2.<> p__37 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__37 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            Func<CallSite, object, bool> target21 = FrostySoundWaveEditor.<> o__2.<> p__37.Target;
                            CallSite<> p__21 = FrostySoundWaveEditor.<> o__2.<> p__37;
                            if (FrostySoundWaveEditor.<> o__2.<> p__36 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__36 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                {
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                }));
                            }
                            Func<CallSite, object, int, object> target22 = FrostySoundWaveEditor.<> o__2.<> p__36.Target;
                            CallSite<> p__22 = FrostySoundWaveEditor.<> o__2.<> p__36;
                            if (FrostySoundWaveEditor.<> o__2.<> p__35 == null)
                            {
                                FrostySoundWaveEditor.<> o__2.<> p__35 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentLength", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                            }
                            bool flag8 = target21(<> p__21, target22(<> p__22, FrostySoundWaveEditor.<> o__2.<> p__35.Target(FrostySoundWaveEditor.<> o__2.<> p__35, obj5), 0));
                            if (flag8)
                            {
                                if (FrostySoundWaveEditor.<> o__2.<> p__38 == null)
                                {
                                    FrostySoundWaveEditor.<> o__2.<> p__38 = CallSite<Func<CallSite, object, float, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "SegmentLength", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                                    {
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                    }));
                                }
                                FrostySoundWaveEditor.<> o__2.<> p__38.Target(FrostySoundWaveEditor.<> o__2.<> p__38, obj5, (float)(decodedSoundBuf.Count / soundDataTrack.ChannelCount) / (float)num9);
                            }
                            num5++;
                        }
                        this.logger.LogError("Wrong Sample Offset at Variation {0}, Segment {1}", new object[] { num, num5 });
                        return list;
                    Block_61:
                        soundDataTrack.Duration = (double)(decodedSoundBuf.Count / soundDataTrack.ChannelCount) / (double)soundDataTrack.SampleRate;
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
                                    bool flag9 = num4 > 0.0;
                                    if (flag9)
                                    {
                                        drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)(num3 / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)soundCloudBlockWaveFormSettings.TopHeight), new global::System.Windows.Point((double)((int)(num3 / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                                        drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)((num3 + num4) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)soundCloudBlockWaveFormSettings.TopHeight), new global::System.Windows.Point((double)((int)((num3 + num4) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
                                        drawingContext.DrawLine(new global::System.Windows.Media.Pen(global::System.Windows.Media.Brushes.White, 1.0), new global::System.Windows.Point((double)((int)(num3 / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)), new global::System.Windows.Point((double)((int)((num3 + num4) / soundDataTrack.Duration * (double)soundCloudBlockWaveFormSettings.Width)), (double)((int)bitmapImage.Height)));
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
                        if (FrostySoundWaveEditor.<> o__2.<> p__40 == null)
                        {
                            FrostySoundWaveEditor.<> o__2.<> p__40 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(int), typeof(FrostySoundWaveEditor)));
                        }
                        Func<CallSite, object, int> target23 = FrostySoundWaveEditor.<> o__2.<> p__40.Target;
                        CallSite<> p__23 = FrostySoundWaveEditor.<> o__2.<> p__40;
                        if (FrostySoundWaveEditor.<> o__2.<> p__39 == null)
                        {
                            FrostySoundWaveEditor.<> o__2.<> p__39 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SegmentCount", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                        }
                        soundDataTrack2.SegmentCount = target23(<> p__23, FrostySoundWaveEditor.<> o__2.<> p__39.Target(FrostySoundWaveEditor.<> o__2.<> p__39, obj));
                    }
                    list.Add(soundDataTrack);
                }
            }
            if (FrostySoundWaveEditor.<> o__2.<> p__54 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__54 = CallSite<Func<CallSite, object, IEnumerable>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(IEnumerable), typeof(FrostySoundWaveEditor)));
            }
            Func<CallSite, object, IEnumerable> target24 = FrostySoundWaveEditor.<> o__2.<> p__54.Target;
            CallSite<> p__24 = FrostySoundWaveEditor.<> o__2.<> p__54;
            if (FrostySoundWaveEditor.<> o__2.<> p__42 == null)
            {
                FrostySoundWaveEditor.<> o__2.<> p__42 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Localization", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            }
            foreach (object obj11 in target24(<> p__24, FrostySoundWaveEditor.<> o__2.<> p__42.Target(FrostySoundWaveEditor.<> o__2.<> p__42, rootObject)))
            {
                int num12 = 0;
                for (; ; )
                {
                    if (FrostySoundWaveEditor.<> o__2.<> p__45 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__45 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    Func<CallSite, object, bool> target25 = FrostySoundWaveEditor.<> o__2.<> p__45.Target;
                    CallSite<> p__25 = FrostySoundWaveEditor.<> o__2.<> p__45;
                    if (FrostySoundWaveEditor.<> o__2.<> p__44 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__44 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                        }));
                    }
                    Func<CallSite, int, object, object> target26 = FrostySoundWaveEditor.<> o__2.<> p__44.Target;
                    CallSite<> p__26 = FrostySoundWaveEditor.<> o__2.<> p__44;
                    int num13 = num12;
                    if (FrostySoundWaveEditor.<> o__2.<> p__43 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__43 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "VariationCount", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    if (!target25(<> p__25, target26(<> p__26, num13, FrostySoundWaveEditor.<> o__2.<> p__43.Target(FrostySoundWaveEditor.<> o__2.<> p__43, obj11))))
                    {
                        break;
                    }
                    if (FrostySoundWaveEditor.<> o__2.<> p__49 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__49 = CallSite<Func<CallSite, object, SoundDataTrack>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(SoundDataTrack), typeof(FrostySoundWaveEditor)));
                    }
                    Func<CallSite, object, SoundDataTrack> target27 = FrostySoundWaveEditor.<> o__2.<> p__49.Target;
                    CallSite<> p__27 = FrostySoundWaveEditor.<> o__2.<> p__49;
                    if (FrostySoundWaveEditor.<> o__2.<> p__48 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__48 = CallSite<Func<CallSite, List<SoundDataTrack>, object, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                        }));
                    }
                    Func<CallSite, List<SoundDataTrack>, object, object> target28 = FrostySoundWaveEditor.<> o__2.<> p__48.Target;
                    CallSite<> p__28 = FrostySoundWaveEditor.<> o__2.<> p__48;
                    List<SoundDataTrack> list2 = list;
                    if (FrostySoundWaveEditor.<> o__2.<> p__47 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__47 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                        }));
                    }
                    Func<CallSite, int, object, object> target29 = FrostySoundWaveEditor.<> o__2.<> p__47.Target;
                    CallSite<> p__29 = FrostySoundWaveEditor.<> o__2.<> p__47;
                    int num14 = num12;
                    if (FrostySoundWaveEditor.<> o__2.<> p__46 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__46 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "FirstVariationIndex", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    SoundDataTrack soundDataTrack3 = target27(<> p__27, target28(<> p__28, list2, target29(<> p__29, num14, FrostySoundWaveEditor.<> o__2.<> p__46.Target(FrostySoundWaveEditor.<> o__2.<> p__46, obj11))));
                    if (FrostySoundWaveEditor.<> o__2.<> p__51 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__51 = CallSite<Func<CallSite, object, PointerRef>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(PointerRef), typeof(FrostySoundWaveEditor)));
                    }
                    Func<CallSite, object, PointerRef> target30 = FrostySoundWaveEditor.<> o__2.<> p__51.Target;
                    CallSite<> p__30 = FrostySoundWaveEditor.<> o__2.<> p__51;
                    if (FrostySoundWaveEditor.<> o__2.<> p__50 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__50 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Language", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    PointerRef pointerRef = target30(<> p__30, FrostySoundWaveEditor.<> o__2.<> p__50.Target(FrostySoundWaveEditor.<> o__2.<> p__50, obj11));
                    EbxAsset ebx = App.AssetManager.GetEbx(App.AssetManager.GetEbxEntry(pointerRef.External.FileGuid), false);
                    object @object = ebx.GetObject(pointerRef.External.ClassGuid);
                    SoundDataTrack soundDataTrack4 = soundDataTrack3;
                    if (FrostySoundWaveEditor.<> o__2.<> p__53 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__53 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(FrostySoundWaveEditor)));
                    }
                    Func<CallSite, object, string> target31 = FrostySoundWaveEditor.<> o__2.<> p__53.Target;
                    CallSite<> p__31 = FrostySoundWaveEditor.<> o__2.<> p__53;
                    if (FrostySoundWaveEditor.<> o__2.<> p__52 == null)
                    {
                        FrostySoundWaveEditor.<> o__2.<> p__52 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "__Id", typeof(FrostySoundWaveEditor), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    }
                    soundDataTrack4.Language = target31(<> p__31, FrostySoundWaveEditor.<> o__2.<> p__52.Target(FrostySoundWaveEditor.<> o__2.<> p__52, @object));
                    num12++;
                }
            }
            return list;
        }
    }
}
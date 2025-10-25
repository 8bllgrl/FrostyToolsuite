using Frosty.Core;
using Frosty.Hash;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using FrostySdk.Resources;
using Microsoft.CSharp.RuntimeBinder;
using SoundEditorPlugin.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SoundEditorPlugin.Resources
{

    public class NewWaveResource : Resource
    {

        public List<Variation> Variations { get; set; } = new List<Variation>();

        public List<Chunk> Chunks { get; set; } = new List<Chunk>();

        public List<Segment> Segments { get; set; } = new List<Segment>();

        public List<SelectionParameter> SelectionParameters { get; set; } = new List<SelectionParameter>();

        public List<Selection> Selections { get; set; } = new List<Selection>();

        public List<Subtitle> Subtitles { get; set; } = new List<Subtitle>();

        public List<Persistence> Persistences { get; set; } = new List<Persistence>();

        public NewWaveResource.Dset GetDSET(string name)
        {
            foreach (NewWaveResource.Dset dset in this.dsets)
            {
                bool flag = dset.NameHash == (uint)Fnv1.HashString(name);
                if (flag)
                {
                    return dset;
                }
            }
            return null;
        }

        public override void Read(NativeReader reader, AssetManager am, ResAssetEntry entry, ModifiedResource modifiedData)
        {
            base.Read(reader, am, entry, modifiedData);
            NewWaveResource.endian = (reader.ReadSizedString(4).Equals("SBle") ? Endian.Little : Endian.Big);
            reader.ReadInt(NewWaveResource.endian);
            this.unkown1 = reader.ReadUShort(NewWaveResource.endian);
            this.dsetCount = reader.ReadUShort(NewWaveResource.endian);
            this.unkown2 = reader.ReadUInt(NewWaveResource.endian);
            this.unkown3 = reader.ReadULong(NewWaveResource.endian);
            this.offset = reader.ReadUInt(NewWaveResource.endian);
            reader.ReadInt(NewWaveResource.endian);
            NewWaveResource.dataOffset = reader.ReadUInt(NewWaveResource.endian);
            reader.ReadInt(NewWaveResource.endian);
            reader.Position = (long)((ulong)this.offset);
            uint[] array = new uint[(int)this.dsetCount];
            this.dsets = new NewWaveResource.Dset[(int)this.dsetCount];
            for (int i = 0; i < (int)this.dsetCount; i++)
            {
                array[i] = reader.ReadUInt(NewWaveResource.endian);
                reader.ReadInt(Endian.Little);
            }
            reader.Pad(16);
            bool flag = reader.Position != (long)((ulong)array[0]);
            if (flag)
            {
                App.Logger.LogWarning("Weird layout of NewWaveResource: " + entry.Name, Array.Empty<object>());
            }
            for (int j = 0; j < (int)this.dsetCount; j++)
            {
                reader.Position = (long)((ulong)array[j]);
                this.dsets[j] = new NewWaveResource.Dset(reader);
                bool flag2 = this.dsets[j].NameHash == (uint)Fnv1.HashString("SelectionParameters");
                if (flag2)
                {
                    int num = 0;
                    while ((long)num < (long)((ulong)this.dsets[j].ElemCount))
                    {
                        SelectionParameter selectionParameter = new SelectionParameter();
                        foreach (NewWaveResource.Field field in this.dsets[j].Fields)
                        {
                            uint nameHash = field.NameHash;
                            uint num2 = nameHash;
                            if (num2 != 241877522U)
                            {
                                if (num2 != 2367814049U)
                                {
                                    App.Logger.LogWarning("Unkown field: " + field.NameHash.ToString("X8") + "Dset: SelectionParameters", Array.Empty<object>());
                                }
                                else
                                {
                                    SelectionParameter selectionParameter2 = selectionParameter;
                                    if (NewWaveResource.o__43.<> p__1 == null)
                                    {
                                        NewWaveResource.o__43.<> p__1 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                    }
                                    selectionParameter2.ParameterId = NewWaveResource.<> o__43.<> p__1.Target(NewWaveResource.<> o__43.<> p__1, field.Values[num]);
                                }
                            }
                            else
                            {
                                SelectionParameter selectionParameter3 = selectionParameter;
                                if (NewWaveResource.<> o__43.<> p__0 == null)
                                {
                                    NewWaveResource.<> o__43.<> p__0 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                }
                                selectionParameter3.ParameterIndex = NewWaveResource.<> o__43.<> p__0.Target(NewWaveResource.<> o__43.<> p__0, field.Values[num]);
                            }
                        }
                        this.SelectionParameters.Add(selectionParameter);
                        num++;
                    }
                }
                else
                {
                    bool flag3 = this.dsets[j].NameHash == (uint)Fnv1.HashString("Selection");
                    if (flag3)
                    {
                        int num3 = 0;
                        while ((long)num3 < (long)((ulong)this.dsets[j].ElemCount))
                        {
                            Selection selection = new Selection();
                            foreach (NewWaveResource.Field field2 in this.dsets[j].Fields)
                            {
                                uint nameHash2 = field2.NameHash;
                                uint num4 = nameHash2;
                                if (num4 <= 1791288554U)
                                {
                                    if (num4 != 215514563U)
                                    {
                                        if (num4 != 1791288554U)
                                        {
                                            goto IL_05AB;
                                        }
                                        Selection selection2 = selection;
                                        if (NewWaveResource.<> o__43.<> p__3 == null)
                                        {
                                            NewWaveResource.<> o__43.<> p__3 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                        }
                                        selection2.VariationIndex = NewWaveResource.<> o__43.<> p__3.Target(NewWaveResource.<> o__43.<> p__3, field2.Values[num3]);
                                    }
                                    else
                                    {
                                        Selection selection3 = selection;
                                        if (NewWaveResource.<> o__43.<> p__4 == null)
                                        {
                                            NewWaveResource.<> o__43.<> p__4 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(NewWaveResource)));
                                        }
                                        selection3.IsDay = NewWaveResource.<> o__43.<> p__4.Target(NewWaveResource.<> o__43.<> p__4, field2.Values[num3]);
                                    }
                                }
                                else if (num4 != 2150010670U)
                                {
                                    if (num4 != 2645093687U)
                                    {
                                        if (num4 != 4126741721U)
                                        {
                                            goto IL_05AB;
                                        }
                                        Selection selection4 = selection;
                                        if (NewWaveResource.<> o__43.<> p__2 == null)
                                        {
                                            NewWaveResource.<> o__43.<> p__2 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                        }
                                        selection4.VariationId = NewWaveResource.<> o__43.<> p__2.Target(NewWaveResource.<> o__43.<> p__2, field2.Values[num3]);
                                    }
                                    else
                                    {
                                        Selection selection5 = selection;
                                        if (NewWaveResource.<> o__43.<> p__5 == null)
                                        {
                                            NewWaveResource.<> o__43.<> p__5 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(int), typeof(NewWaveResource)));
                                        }
                                        selection5.PreDelay = NewWaveResource.<> o__43.<> p__5.Target(NewWaveResource.<> o__43.<> p__5, field2.Values[num3]);
                                    }
                                }
                                else
                                {
                                    Selection selection6 = selection;
                                    if (NewWaveResource.<> o__43.<> p__6 == null)
                                    {
                                        NewWaveResource.<> o__43.<> p__6 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(int), typeof(NewWaveResource)));
                                    }
                                    selection6.unkown = NewWaveResource.<> o__43.<> p__6.Target(NewWaveResource.<> o__43.<> p__6, field2.Values[num3]);
                                }
                                continue;
                            IL_05AB:
                                App.Logger.LogWarning("Unkown field: " + field2.NameHash.ToString("X8") + "Dset: Selection", Array.Empty<object>());
                            }
                            this.Selections.Add(selection);
                            num3++;
                        }
                    }
                    else
                    {
                        bool flag4 = this.dsets[j].NameHash == (uint)Fnv1.HashString("Variations");
                        if (flag4)
                        {
                            int num5 = 0;
                            while ((long)num5 < (long)((ulong)this.dsets[j].ElemCount))
                            {
                                Variation variation = new Variation();
                                foreach (NewWaveResource.Field field3 in this.dsets[j].Fields)
                                {
                                    uint nameHash3 = field3.NameHash;
                                    uint num6 = nameHash3;
                                    if (num6 <= 1737235644U)
                                    {
                                        if (num6 <= 1257466978U)
                                        {
                                            if (num6 != 64641870U)
                                            {
                                                if (num6 != 1257466978U)
                                                {
                                                    goto IL_0B20;
                                                }
                                                Variation variation2 = variation;
                                                if (NewWaveResource.<> o__43.<> p__16 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__16 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                }
                                                variation2.SubtitleCount = NewWaveResource.<> o__43.<> p__16.Target(NewWaveResource.<> o__43.<> p__16, field3.Values[num5]);
                                            }
                                            else
                                            {
                                                Variation variation3 = variation;
                                                if (NewWaveResource.<> o__43.<> p__14 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__14 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                }
                                                variation3.LastLoopSegmentIndex = NewWaveResource.<> o__43.<> p__14.Target(NewWaveResource.<> o__43.<> p__14, field3.Values[num5]);
                                            }
                                        }
                                        else if (num6 != 1314600737U)
                                        {
                                            if (num6 != 1737235644U)
                                            {
                                                goto IL_0B20;
                                            }
                                            Variation variation4 = variation;
                                            if (NewWaveResource.<> o__43.<> p__9 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__9 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                            }
                                            Func<CallSite, object, uint> target = NewWaveResource.<> o__43.<> p__9.Target;
                                            CallSite<> p__ = NewWaveResource.<> o__43.<> p__9;
                                            if (NewWaveResource.<> o__43.<> p__8 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__8 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.RightShift, typeof(NewWaveResource), new CSharpArgumentInfo[]
                                                {
                                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                                }));
                                            }
                                            variation4.StreamChunkIndex = target(<> p__, NewWaveResource.<> o__43.<> p__8.Target(NewWaveResource.<> o__43.<> p__8, field3.Values[num5], 1));
                                        }
                                        else
                                        {
                                            Variation variation5 = variation;
                                            if (NewWaveResource.<> o__43.<> p__11 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__11 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                            }
                                            Func<CallSite, object, uint> target2 = NewWaveResource.<> o__43.<> p__11.Target;
                                            CallSite<> p__2 = NewWaveResource.<> o__43.<> p__11;
                                            if (NewWaveResource.<> o__43.<> p__10 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__10 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.RightShift, typeof(NewWaveResource), new CSharpArgumentInfo[]
                                                {
                                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                                }));
                                            }
                                            variation5.MemoryChunkIndex = target2(<> p__2, NewWaveResource.<> o__43.<> p__10.Target(NewWaveResource.<> o__43.<> p__10, field3.Values[num5], 1));
                                        }
                                    }
                                    else if (num6 <= 3490340869U)
                                    {
                                        if (num6 != 1804204094U)
                                        {
                                            if (num6 != 3490340869U)
                                            {
                                                goto IL_0B20;
                                            }
                                            Variation variation6 = variation;
                                            if (NewWaveResource.<> o__43.<> p__15 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__15 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                            }
                                            variation6.SegmentCount = NewWaveResource.<> o__43.<> p__15.Target(NewWaveResource.<> o__43.<> p__15, field3.Values[num5]);
                                        }
                                        else
                                        {
                                            Variation variation7 = variation;
                                            if (NewWaveResource.<> o__43.<> p__13 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__13 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                            }
                                            variation7.FirstLoopSegmentIndex = NewWaveResource.<> o__43.<> p__13.Target(NewWaveResource.<> o__43.<> p__13, field3.Values[num5]);
                                        }
                                    }
                                    else if (num6 != 3619771077U)
                                    {
                                        if (num6 != 3831892578U)
                                        {
                                            if (num6 != 4126741721U)
                                            {
                                                goto IL_0B20;
                                            }
                                            Variation variation8 = variation;
                                            if (NewWaveResource.<> o__43.<> p__7 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__7 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                            }
                                            variation8.VariationId = NewWaveResource.<> o__43.<> p__7.Target(NewWaveResource.<> o__43.<> p__7, field3.Values[num5]);
                                        }
                                        else
                                        {
                                            Variation variation9 = variation;
                                            if (NewWaveResource.<> o__43.<> p__12 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__12 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                            }
                                            variation9.FirstSegmentIndex = NewWaveResource.<> o__43.<> p__12.Target(NewWaveResource.<> o__43.<> p__12, field3.Values[num5]);
                                        }
                                    }
                                    else
                                    {
                                        Variation variation10 = variation;
                                        if (NewWaveResource.<> o__43.<> p__17 == null)
                                        {
                                            NewWaveResource.<> o__43.<> p__17 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                        }
                                        variation10.FirstSubtitleIndex = NewWaveResource.<> o__43.<> p__17.Target(NewWaveResource.<> o__43.<> p__17, field3.Values[num5]);
                                    }
                                    continue;
                                IL_0B20:
                                    App.Logger.LogWarning("Unkown field: " + field3.NameHash.ToString("X8") + "Dset: Variations", Array.Empty<object>());
                                }
                                this.Variations.Add(variation);
                                num5++;
                            }
                        }
                        else
                        {
                            bool flag5 = this.dsets[j].NameHash == (uint)Fnv1.HashString("Segments");
                            if (flag5)
                            {
                                int num7 = 0;
                                while ((long)num7 < (long)((ulong)this.dsets[j].ElemCount))
                                {
                                    Segment segment = new Segment();
                                    foreach (NewWaveResource.Field field4 in this.dsets[j].Fields)
                                    {
                                        uint nameHash4 = field4.NameHash;
                                        uint num8 = nameHash4;
                                        if (num8 != 1828507227U)
                                        {
                                            if (num8 != 3573995342U)
                                            {
                                                if (num8 != 3907359197U)
                                                {
                                                    App.Logger.LogWarning("Unkown field: " + field4.NameHash.ToString("X8") + "Dset: Segments", Array.Empty<object>());
                                                }
                                                else
                                                {
                                                    Segment segment2 = segment;
                                                    if (NewWaveResource.<> o__43.<> p__24 == null)
                                                    {
                                                        NewWaveResource.<> o__43.<> p__24 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                    }
                                                    Func<CallSite, object, uint> target3 = NewWaveResource.<> o__43.<> p__24.Target;
                                                    CallSite<> p__3 = NewWaveResource.<> o__43.<> p__24;
                                                    if (NewWaveResource.<> o__43.<> p__23 == null)
                                                    {
                                                        NewWaveResource.<> o__43.<> p__23 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.And, typeof(NewWaveResource), new CSharpArgumentInfo[]
                                                        {
                                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                                        }));
                                                    }
                                                    segment2.SamplesOffsetFlag = target3(<> p__3, NewWaveResource.<> o__43.<> p__23.Target(NewWaveResource.<> o__43.<> p__23, field4.Values[num7], 3));
                                                    Segment segment3 = segment;
                                                    if (NewWaveResource.<> o__43.<> p__26 == null)
                                                    {
                                                        NewWaveResource.<> o__43.<> p__26 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                    }
                                                    Func<CallSite, object, uint> target4 = NewWaveResource.<> o__43.<> p__26.Target;
                                                    CallSite<> p__4 = NewWaveResource.<> o__43.<> p__26;
                                                    if (NewWaveResource.<> o__43.<> p__25 == null)
                                                    {
                                                        NewWaveResource.<> o__43.<> p__25 = CallSite<Func<CallSite, object, uint, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.And, typeof(NewWaveResource), new CSharpArgumentInfo[]
                                                        {
                                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                                        }));
                                                    }
                                                    segment3.SamplesOffset = target4(<> p__4, NewWaveResource.<> o__43.<> p__25.Target(NewWaveResource.<> o__43.<> p__25, field4.Values[num7], 4294967292U));
                                                }
                                            }
                                            else
                                            {
                                                Segment segment4 = segment;
                                                if (NewWaveResource.<> o__43.<> p__20 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__20 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                }
                                                Func<CallSite, object, uint> target5 = NewWaveResource.<> o__43.<> p__20.Target;
                                                CallSite<> p__5 = NewWaveResource.<> o__43.<> p__20;
                                                if (NewWaveResource.<> o__43.<> p__19 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__19 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.And, typeof(NewWaveResource), new CSharpArgumentInfo[]
                                                    {
                                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                                    }));
                                                }
                                                segment4.SeekTableFlag = target5(<> p__5, NewWaveResource.<> o__43.<> p__19.Target(NewWaveResource.<> o__43.<> p__19, field4.Values[num7], 3));
                                                Segment segment5 = segment;
                                                if (NewWaveResource.<> o__43.<> p__22 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__22 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                }
                                                Func<CallSite, object, uint> target6 = NewWaveResource.<> o__43.<> p__22.Target;
                                                CallSite<> p__6 = NewWaveResource.<> o__43.<> p__22;
                                                if (NewWaveResource.<> o__43.<> p__21 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__21 = CallSite<Func<CallSite, object, uint, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.And, typeof(NewWaveResource), new CSharpArgumentInfo[]
                                                    {
                                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                                    }));
                                                }
                                                segment5.SeekTableOffset = target6(<> p__6, NewWaveResource.<> o__43.<> p__21.Target(NewWaveResource.<> o__43.<> p__21, field4.Values[num7], 4294967292U));
                                            }
                                        }
                                        else
                                        {
                                            Segment segment6 = segment;
                                            if (NewWaveResource.<> o__43.<> p__18 == null)
                                            {
                                                NewWaveResource.<> o__43.<> p__18 = CallSite<Func<CallSite, object, float>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(float), typeof(NewWaveResource)));
                                            }
                                            segment6.SegmentLength = NewWaveResource.<> o__43.<> p__18.Target(NewWaveResource.<> o__43.<> p__18, field4.Values[num7]);
                                        }
                                    }
                                    this.Segments.Add(segment);
                                    num7++;
                                }
                            }
                            else
                            {
                                bool flag6 = this.dsets[j].NameHash == (uint)Fnv1.HashString("Chunks");
                                if (flag6)
                                {
                                    int num9 = 0;
                                    while ((long)num9 < (long)((ulong)this.dsets[j].ElemCount))
                                    {
                                        Chunk chunk = new Chunk();
                                        foreach (NewWaveResource.Field field5 in this.dsets[j].Fields)
                                        {
                                            uint nameHash5 = field5.NameHash;
                                            uint num10 = nameHash5;
                                            if (num10 != 3692630139U)
                                            {
                                                if (num10 != 4097216883U)
                                                {
                                                    App.Logger.LogWarning("Unkown field: " + field5.NameHash.ToString("X8") + "Dset: Chunks", Array.Empty<object>());
                                                }
                                                else
                                                {
                                                    Chunk chunk2 = chunk;
                                                    if (NewWaveResource.<> o__43.<> p__27 == null)
                                                    {
                                                        NewWaveResource.<> o__43.<> p__27 = CallSite<Func<CallSite, object, Guid>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(Guid), typeof(NewWaveResource)));
                                                    }
                                                    chunk2.ChunkId = NewWaveResource.<> o__43.<> p__27.Target(NewWaveResource.<> o__43.<> p__27, field5.Values[num9]);
                                                }
                                            }
                                            else
                                            {
                                                Chunk chunk3 = chunk;
                                                if (NewWaveResource.<> o__43.<> p__28 == null)
                                                {
                                                    NewWaveResource.<> o__43.<> p__28 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                }
                                                chunk3.ChunkSize = NewWaveResource.<> o__43.<> p__28.Target(NewWaveResource.<> o__43.<> p__28, field5.Values[num9]);
                                            }
                                        }
                                        this.Chunks.Add(chunk);
                                        num9++;
                                    }
                                }
                                else
                                {
                                    bool flag7 = this.dsets[j].NameHash == (uint)Fnv1.HashString("Subtitles");
                                    if (flag7)
                                    {
                                        int num11 = 0;
                                        while ((long)num11 < (long)((ulong)this.dsets[j].ElemCount))
                                        {
                                            Subtitle subtitle = new Subtitle();
                                            foreach (NewWaveResource.Field field6 in this.dsets[j].Fields)
                                            {
                                                uint nameHash6 = field6.NameHash;
                                                uint num12 = nameHash6;
                                                if (num12 != 2089313744U)
                                                {
                                                    if (num12 != 3536090717U)
                                                    {
                                                        if (num12 != 4175181454U)
                                                        {
                                                            App.Logger.LogWarning("Unkown field: " + field6.NameHash.ToString("X8") + "Dset: Subtitles", Array.Empty<object>());
                                                        }
                                                        else
                                                        {
                                                            Subtitle subtitle2 = subtitle;
                                                            if (NewWaveResource.<> o__43.<> p__30 == null)
                                                            {
                                                                NewWaveResource.<> o__43.<> p__30 = CallSite<Func<CallSite, object, int>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(int), typeof(NewWaveResource)));
                                                            }
                                                            subtitle2.AdditionalSubtitleInfoType = NewWaveResource.<> o__43.<> p__30.Target(NewWaveResource.<> o__43.<> p__30, field6.Values[num11]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Subtitle subtitle3 = subtitle;
                                                        if (NewWaveResource.<> o__43.<> p__31 == null)
                                                        {
                                                            NewWaveResource.<> o__43.<> p__31 = CallSite<Func<CallSite, object, CString>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(CString), typeof(NewWaveResource)));
                                                        }
                                                        subtitle3.StringId = NewWaveResource.<> o__43.<> p__31.Target(NewWaveResource.<> o__43.<> p__31, field6.Values[num11]);
                                                    }
                                                }
                                                else
                                                {
                                                    Subtitle subtitle4 = subtitle;
                                                    if (NewWaveResource.<> o__43.<> p__29 == null)
                                                    {
                                                        NewWaveResource.<> o__43.<> p__29 = CallSite<Func<CallSite, object, float>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(float), typeof(NewWaveResource)));
                                                    }
                                                    subtitle4.Time = NewWaveResource.<> o__43.<> p__29.Target(NewWaveResource.<> o__43.<> p__29, field6.Values[num11]);
                                                }
                                            }
                                            this.Subtitles.Add(subtitle);
                                            num11++;
                                        }
                                    }
                                    else
                                    {
                                        bool flag8 = this.dsets[j].NameHash == (uint)Fnv1.HashString("Persistence");
                                        if (flag8)
                                        {
                                            int num13 = 0;
                                            while ((long)num13 < (long)((ulong)this.dsets[j].ElemCount))
                                            {
                                                Persistence persistence = new Persistence();
                                                foreach (NewWaveResource.Field field7 in this.dsets[j].Fields)
                                                {
                                                    uint nameHash7 = field7.NameHash;
                                                    uint num14 = nameHash7;
                                                    if (num14 <= 2093915791U)
                                                    {
                                                        if (num14 <= 548399920U)
                                                        {
                                                            if (num14 <= 230887042U)
                                                            {
                                                                if (num14 != 14084381U)
                                                                {
                                                                    if (num14 != 230887042U)
                                                                    {
                                                                        goto IL_1E00;
                                                                    }
                                                                    Persistence persistence2 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__35 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__35 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence2.Speed = NewWaveResource.<> o__43.<> p__35.Target(NewWaveResource.<> o__43.<> p__35, field7.Values[num13]);
                                                                }
                                                                else
                                                                {
                                                                    Persistence persistence3 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__41 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__41 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence3.WaterDepth = NewWaveResource.<> o__43.<> p__41.Target(NewWaveResource.<> o__43.<> p__41, field7.Values[num13]);
                                                                }
                                                            }
                                                            else if (num14 != 457568500U)
                                                            {
                                                                if (num14 != 536869775U)
                                                                {
                                                                    if (num14 != 548399920U)
                                                                    {
                                                                        goto IL_1E00;
                                                                    }
                                                                    Persistence persistence4 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__39 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__39 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence4.unknown03 = NewWaveResource.<> o__43.<> p__39.Target(NewWaveResource.<> o__43.<> p__39, field7.Values[num13]);
                                                                }
                                                                else
                                                                {
                                                                    Persistence persistence5 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__43 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__43 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence5.unknown07 = NewWaveResource.<> o__43.<> p__43.Target(NewWaveResource.<> o__43.<> p__43, field7.Values[num13]);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Persistence persistence6 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__50 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__50 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence6.unknown14 = NewWaveResource.<> o__43.<> p__50.Target(NewWaveResource.<> o__43.<> p__50, field7.Values[num13]);
                                                            }
                                                        }
                                                        else if (num14 <= 925900165U)
                                                        {
                                                            if (num14 != 742421004U)
                                                            {
                                                                if (num14 != 845639918U)
                                                                {
                                                                    if (num14 != 925900165U)
                                                                    {
                                                                        goto IL_1E00;
                                                                    }
                                                                    Persistence persistence7 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__51 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__51 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence7.unknown15 = NewWaveResource.<> o__43.<> p__51.Target(NewWaveResource.<> o__43.<> p__51, field7.Values[num13]);
                                                                }
                                                                else
                                                                {
                                                                    Persistence persistence8 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__42 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__42 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence8.Material = NewWaveResource.<> o__43.<> p__42.Target(NewWaveResource.<> o__43.<> p__42, field7.Values[num13]);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Persistence persistence9 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__36 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__36 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence9.unknown00 = NewWaveResource.<> o__43.<> p__36.Target(NewWaveResource.<> o__43.<> p__36, field7.Values[num13]);
                                                            }
                                                        }
                                                        else if (num14 != 1552220623U)
                                                        {
                                                            if (num14 != 1810774590U)
                                                            {
                                                                if (num14 != 2093915791U)
                                                                {
                                                                    goto IL_1E00;
                                                                }
                                                                Persistence persistence10 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__47 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__47 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence10.unknown11 = NewWaveResource.<> o__43.<> p__47.Target(NewWaveResource.<> o__43.<> p__47, field7.Values[num13]);
                                                            }
                                                            else
                                                            {
                                                                Persistence persistence11 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__33 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__33 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence11.RequiredVariationCount = NewWaveResource.<> o__43.<> p__33.Target(NewWaveResource.<> o__43.<> p__33, field7.Values[num13]);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Persistence persistence12 = persistence;
                                                            if (NewWaveResource.<> o__43.<> p__32 == null)
                                                            {
                                                                NewWaveResource.<> o__43.<> p__32 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                            }
                                                            persistence12.SelectionParameterCount = NewWaveResource.<> o__43.<> p__32.Target(NewWaveResource.<> o__43.<> p__32, field7.Values[num13]);
                                                        }
                                                    }
                                                    else if (num14 <= 3230848775U)
                                                    {
                                                        if (num14 <= 2176999743U)
                                                        {
                                                            if (num14 != 2101675428U)
                                                            {
                                                                if (num14 != 2152551088U)
                                                                {
                                                                    if (num14 != 2176999743U)
                                                                    {
                                                                        goto IL_1E00;
                                                                    }
                                                                    Persistence persistence13 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__34 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__34 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence13.DesiredVariationCount = NewWaveResource.<> o__43.<> p__34.Target(NewWaveResource.<> o__43.<> p__34, field7.Values[num13]);
                                                                }
                                                                else
                                                                {
                                                                    Persistence persistence14 = persistence;
                                                                    if (NewWaveResource.<> o__43.<> p__40 == null)
                                                                    {
                                                                        NewWaveResource.<> o__43.<> p__40 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                    }
                                                                    persistence14.unknown04 = NewWaveResource.<> o__43.<> p__40.Target(NewWaveResource.<> o__43.<> p__40, field7.Values[num13]);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Persistence persistence15 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__52 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__52 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence15.unknown16 = NewWaveResource.<> o__43.<> p__52.Target(NewWaveResource.<> o__43.<> p__52, field7.Values[num13]);
                                                            }
                                                        }
                                                        else if (num14 != 2403004227U)
                                                        {
                                                            if (num14 != 3008756189U)
                                                            {
                                                                if (num14 != 3230848775U)
                                                                {
                                                                    goto IL_1E00;
                                                                }
                                                                Persistence persistence16 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__53 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__53 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence16.unknown17 = NewWaveResource.<> o__43.<> p__53.Target(NewWaveResource.<> o__43.<> p__53, field7.Values[num13]);
                                                            }
                                                            else
                                                            {
                                                                Persistence persistence17 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__37 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__37 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence17.unknown01 = NewWaveResource.<> o__43.<> p__37.Target(NewWaveResource.<> o__43.<> p__37, field7.Values[num13]);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Persistence persistence18 = persistence;
                                                            if (NewWaveResource.<> o__43.<> p__48 == null)
                                                            {
                                                                NewWaveResource.<> o__43.<> p__48 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                            }
                                                            persistence18.unknown12 = NewWaveResource.<> o__43.<> p__48.Target(NewWaveResource.<> o__43.<> p__48, field7.Values[num13]);
                                                        }
                                                    }
                                                    else if (num14 <= 3872135783U)
                                                    {
                                                        if (num14 != 3794857406U)
                                                        {
                                                            if (num14 != 3835667476U)
                                                            {
                                                                if (num14 != 3872135783U)
                                                                {
                                                                    goto IL_1E00;
                                                                }
                                                                Persistence persistence19 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__44 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__44 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence19.unknown08 = NewWaveResource.<> o__43.<> p__44.Target(NewWaveResource.<> o__43.<> p__44, field7.Values[num13]);
                                                            }
                                                            else
                                                            {
                                                                Persistence persistence20 = persistence;
                                                                if (NewWaveResource.<> o__43.<> p__54 == null)
                                                                {
                                                                    NewWaveResource.<> o__43.<> p__54 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                                }
                                                                persistence20.unknown18 = NewWaveResource.<> o__43.<> p__54.Target(NewWaveResource.<> o__43.<> p__54, field7.Values[num13]);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Persistence persistence21 = persistence;
                                                            if (NewWaveResource.<> o__43.<> p__38 == null)
                                                            {
                                                                NewWaveResource.<> o__43.<> p__38 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                            }
                                                            persistence21.unknown02 = NewWaveResource.<> o__43.<> p__38.Target(NewWaveResource.<> o__43.<> p__38, field7.Values[num13]);
                                                        }
                                                    }
                                                    else if (num14 != 3903352867U)
                                                    {
                                                        if (num14 != 4133048608U)
                                                        {
                                                            if (num14 != 4222910623U)
                                                            {
                                                                goto IL_1E00;
                                                            }
                                                            Persistence persistence22 = persistence;
                                                            if (NewWaveResource.<> o__43.<> p__46 == null)
                                                            {
                                                                NewWaveResource.<> o__43.<> p__46 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                            }
                                                            persistence22.unknown10 = NewWaveResource.<> o__43.<> p__46.Target(NewWaveResource.<> o__43.<> p__46, field7.Values[num13]);
                                                        }
                                                        else
                                                        {
                                                            Persistence persistence23 = persistence;
                                                            if (NewWaveResource.<> o__43.<> p__49 == null)
                                                            {
                                                                NewWaveResource.<> o__43.<> p__49 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                            }
                                                            persistence23.unknown13 = NewWaveResource.<> o__43.<> p__49.Target(NewWaveResource.<> o__43.<> p__49, field7.Values[num13]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Persistence persistence24 = persistence;
                                                        if (NewWaveResource.<> o__43.<> p__45 == null)
                                                        {
                                                            NewWaveResource.<> o__43.<> p__45 = CallSite<Func<CallSite, object, uint>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(uint), typeof(NewWaveResource)));
                                                        }
                                                        persistence24.unknown09 = NewWaveResource.<> o__43.<> p__45.Target(NewWaveResource.<> o__43.<> p__45, field7.Values[num13]);
                                                    }
                                                    continue;
                                                IL_1E00:
                                                    App.Logger.LogWarning("Unkown field: " + field7.NameHash.ToString("X8") + "Dset: Persistence", Array.Empty<object>());
                                                }
                                                this.Persistences.Add(persistence);
                                                num13++;
                                            }
                                        }
                                        else
                                        {
                                            App.Logger.LogWarning("Unknown DataSet: " + this.dsets[j].NameHash.ToString("X8"), Array.Empty<object>());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override byte[] SaveBytes()
        {
            this.UpdateDSETS();
            NewWaveResource.SoundBankContainer soundBankContainer = new NewWaveResource.SoundBankContainer();
            this.PreProcess(soundBankContainer);
            byte[] array;
            using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream(), false, false))
            {
                this.Process(nativeWriter, soundBankContainer);
                soundBankContainer.AddDataOffset(nativeWriter);
                nativeWriter.Write(NewWaveResource.data.ToArray());
                soundBankContainer.FixupRelocPtrs(nativeWriter);
                uint num = (uint)nativeWriter.Position;
                nativeWriter.Position = 4L;
                nativeWriter.Write(num);
                array = nativeWriter.ToByteArray();
            }
            return array;
        }

        internal void PreProcess(NewWaveResource.SoundBankContainer sbContainer)
        {
            this.UpdateDSETS();
            sbContainer.AddRelocPtr("DSETOFFSETS", this.dsets);
            sbContainer.AddRelocPtr("DATA", "BANKDATA");
            foreach (NewWaveResource.Dset dset in this.dsets)
            {
                sbContainer.AddRelocPtr("DSET", dset);
                dset.PreProcess(sbContainer);
            }
        }

        internal void Process(NativeWriter writer, NewWaveResource.SoundBankContainer sbContainer)
        {
            writer.Write((NewWaveResource.endian == Endian.Little) ? 1396862053 : 1396859493, Endian.Big);
            writer.Write(3735928559U);
            writer.Write(this.unkown1, NewWaveResource.endian);
            writer.Write(this.dsetCount, NewWaveResource.endian);
            writer.Write(this.unkown2, NewWaveResource.endian);
            writer.Write(this.unkown3, NewWaveResource.endian);
            sbContainer.WriteRelocPtr("DSETOFFSETS", this.dsets, writer);
            sbContainer.WriteDataPtr(writer);
            for (int i = 0; i < 3; i++)
            {
                writer.Write(0);
            }
            sbContainer.AddOffset("DSETOFFSETS", this.dsets, writer);
            foreach (NewWaveResource.Dset dset in this.dsets)
            {
                sbContainer.WriteRelocPtr("DSET", dset, writer);
            }
            foreach (NewWaveResource.Dset dset2 in this.dsets)
            {
                writer.WritePadding(16);
                dset2.Process(writer, sbContainer);
            }
        }

        internal void UpdateDSETS()
        {
            this.UpdateVariations();
            this.UpdateSegments();
            this.UpdateChunks();
        }

        internal void UpdateVariations()
        {
            List<object> list = new List<object>();
            List<object> list2 = new List<object>();
            List<object> list3 = new List<object>();
            List<object> list4 = new List<object>();
            List<object> list5 = new List<object>();
            List<object> list6 = new List<object>();
            List<object> list7 = new List<object>();
            foreach (Variation variation in this.Variations)
            {
                list.Add(variation.VariationId);
                list2.Add((variation.MemoryChunkIndex << 1) | 1U);
                list3.Add((variation.StreamChunkIndex << 1) | 1U);
                list4.Add(variation.FirstSegmentIndex);
                list5.Add(variation.SegmentCount);
                list6.Add(variation.FirstLoopSegmentIndex);
                list7.Add(variation.LastLoopSegmentIndex);
            }
            NewWaveResource.Dset dset = this.GetDSET("Variations");
            bool flag = dset == null;
            if (!flag)
            {
                NewWaveResource.Field field = dset.GetField("VariationId");
                NewWaveResource.Field field2 = dset.GetField("MemoryChunkIndex");
                NewWaveResource.Field field3 = dset.GetField("StreamChunkIndex");
                NewWaveResource.Field field4 = dset.GetField("FirstSegmentIndex");
                NewWaveResource.Field field5 = dset.GetField("SegmentCount");
                NewWaveResource.Field field6 = dset.GetField("FirstLoopSegmentIndex");
                NewWaveResource.Field field7 = dset.GetField("LastLoopSegmentIndex");
                bool flag2 = !field.Values.Equals(list);
                if (flag2)
                {
                    field.Values = list;
                }
                bool flag3 = !field2.Values.Equals(list2);
                if (flag3)
                {
                    field2.Values = list2;
                }
                bool flag4 = !field3.Values.Equals(list3);
                if (flag4)
                {
                    field3.Values = list3;
                }
                bool flag5 = !field4.Values.Equals(list4);
                if (flag5)
                {
                    field4.Values = list4;
                }
                bool flag6 = !field5.Values.Equals(list5);
                if (flag6)
                {
                    field5.Values = list5;
                }
                bool flag7 = !field6.Values.Equals(list6);
                if (flag7)
                {
                    field6.Values = list6;
                }
                bool flag8 = !field7.Values.Equals(list7);
                if (flag8)
                {
                    field7.Values = list7;
                }
            }
        }

        internal void UpdateSegments()
        {
            List<object> list = new List<object>();
            List<object> list2 = new List<object>();
            List<object> list3 = new List<object>();
            foreach (Segment segment in this.Segments)
            {
                list.Add(segment.SamplesOffset | segment.SamplesOffsetFlag);
                list2.Add(segment.SeekTableOffset | segment.SeekTableFlag);
                list3.Add(segment.SegmentLength);
            }
            NewWaveResource.Dset dset = this.GetDSET("Segments");
            bool flag = dset == null;
            if (!flag)
            {
                NewWaveResource.Field field = dset.GetField("SamplesOffset");
                NewWaveResource.Field field2 = dset.GetField("SeekTableOffset");
                NewWaveResource.Field field3 = dset.GetField("Duration");
                bool flag2 = !field.Values.Equals(list);
                if (flag2)
                {
                    field.Values = list;
                }
                bool flag3 = !field2.Values.Equals(list2);
                if (flag3)
                {
                    field2.Values = list2;
                }
                bool flag4 = !field3.Values.Equals(list3);
                if (flag4)
                {
                    field3.Values = list3;
                }
            }
        }

        internal void UpdateChunks()
        {
            List<object> list = new List<object>();
            List<object> list2 = new List<object>();
            foreach (Chunk chunk in this.Chunks)
            {
                list.Add(chunk.ChunkId);
                list2.Add(chunk.ChunkSize);
            }
            NewWaveResource.Dset dset = this.GetDSET("Chunks");
            bool flag = dset == null;
            if (!flag)
            {
                NewWaveResource.Field field = dset.GetField("ChunkId");
                NewWaveResource.Field field2 = dset.GetField("ChunkSize");
                bool flag2 = !field.Values.Equals(list);
                if (flag2)
                {
                    field.Values = list;
                }
                bool flag3 = !field2.Values.Equals(list2);
                if (flag3)
                {
                    field2.Values = list2;
                }
            }
        }

        private static Endian endian;

        private ushort unkown1;

        private ushort dsetCount;

        private uint unkown2;

        private ulong unkown3;

        private uint offset;

        private static uint dataOffset;

        private NewWaveResource.Dset[] dsets;

        private static List<byte> data;

        internal class SoundBankContainer
        {

            public void WriteDataPtr(NativeWriter writer)
            {
                this.dataPtr.Offsets.Add((uint)writer.Position);
                writer.Write(16045690984833335023UL);
            }

            public void AddDataOffset(NativeWriter writer)
            {
                this.dataPtr.DataOffset = (uint)writer.Position;
            }

            public void AddRelocPtr(string type, object obj)
            {
                this.relocPtrs.Add(new NewWaveResource.SoundBankContainer.RelocPtr(type, obj));
            }

            public void WriteRelocPtr(string type, object obj, NativeWriter writer)
            {
                NewWaveResource.SoundBankContainer.RelocPtr relocPtr = this.FindRelocPtr(type, obj);
                bool flag = relocPtr == null;
                if (flag)
                {
                    writer.Write(0L);
                }
                relocPtr.Offset = (uint)writer.Position;
                writer.Write(16045690984833335023UL);
            }

            public void AddOffset(string type, object data, NativeWriter writer)
            {
                NewWaveResource.SoundBankContainer.RelocPtr relocPtr = this.FindRelocPtr(type, data);
                bool flag = relocPtr != null;
                if (flag)
                {
                    relocPtr.DataOffset = (uint)writer.Position;
                }
            }

            public void FixupRelocPtrs(NativeWriter writer)
            {
                for (int i = 0; i < this.dataPtr.Offsets.Count; i++)
                {
                    NewWaveResource.SoundBankContainer.RelocPtr relocPtr = new NewWaveResource.SoundBankContainer.RelocPtr("DATA", i);
                    relocPtr.Offset = this.dataPtr.Offsets[i];
                    relocPtr.DataOffset = this.dataPtr.DataOffset;
                    this.relocPtrs.Add(relocPtr);
                }
                this.relocPtrs.Sort(new NewWaveResource.SoundBankContainer.RelocPtrComparer());
                for (int j = 0; j < this.relocPtrs.Count; j++)
                {
                    writer.Position = (long)((ulong)this.relocPtrs[j].Offset);
                    writer.Write(this.relocPtrs[j].DataOffset, NewWaveResource.endian);
                    bool flag = j + 1 == this.relocPtrs.Count;
                    if (flag)
                    {
                        writer.Write(uint.MaxValue);
                        break;
                    }
                    writer.Write(this.relocPtrs[j + 1].Offset, NewWaveResource.endian);
                }
            }

            private NewWaveResource.SoundBankContainer.RelocPtr FindRelocPtr(string type, object obj)
            {
                foreach (NewWaveResource.SoundBankContainer.RelocPtr relocPtr in this.relocPtrs)
                {
                    bool flag = relocPtr.Type == type && relocPtr.Data.Equals(obj);
                    if (flag)
                    {
                        return relocPtr;
                    }
                }
                return null;
            }

            private List<NewWaveResource.SoundBankContainer.RelocPtr> relocPtrs = new List<NewWaveResource.SoundBankContainer.RelocPtr>();

            private NewWaveResource.SoundBankContainer.DataPtr dataPtr = new NewWaveResource.SoundBankContainer.DataPtr();

            public class RelocPtr
            {

                public RelocPtr(string type, object data)
                {
                    this.Type = type;
                    this.Data = data;
                }

                public string Type;

                public uint Offset;

                public object Data;

                public uint DataOffset;
            }

            public class DataPtr
            {

                public List<uint> Offsets = new List<uint>();

                public uint DataOffset;
            }

            private class RelocPtrComparer : IComparer<NewWaveResource.SoundBankContainer.RelocPtr>
            {

                public int Compare(NewWaveResource.SoundBankContainer.RelocPtr ptr1, NewWaveResource.SoundBankContainer.RelocPtr ptr2)
                {
                    return ptr1.Offset.CompareTo(ptr2.Offset);
                }
            }
        }

        public class Dset
        {

            public uint NameHash
            {
                get
                {
                    return this.nameHash;
                }
            }

            public uint ElemCount
            {
                get
                {
                    return this.elemCount;
                }
            }

            public List<NewWaveResource.Field> Fields
            {
                get
                {
                    return this.fields;
                }
                set
                {
                    this.fields = value;
                }
            }

            public Dset(NativeReader reader)
            {
                long position = reader.Position;
                bool flag = reader.ReadUInt(NewWaveResource.endian) != 1146307924U;
                if (flag)
                {
                    throw new FileFormatException("Wrong format of DataSet");
                }
                reader.ReadInt(Endian.Little);
                this.nameHash = reader.ReadUInt(NewWaveResource.endian);
                this.unknown1 = reader.ReadUInt(NewWaveResource.endian);
                reader.ReadInt(Endian.Little);
                reader.ReadInt(Endian.Little);
                bool flag2 = NewWaveResource.dataOffset != reader.ReadUInt(NewWaveResource.endian);
                if (flag2)
                {
                    App.Logger.LogWarning("Different data offset of NewWaveResource", Array.Empty<object>());
                }
                reader.ReadInt(Endian.Little);
                reader.ReadBytes(24);
                this.elemCount = reader.ReadUInt(NewWaveResource.endian);
                ushort num = reader.ReadUShort(NewWaveResource.endian);
                ushort num2 = reader.ReadUShort(NewWaveResource.endian);
                this.offset1 = reader.ReadUShort(NewWaveResource.endian);
                this.offset2 = reader.ReadUShort(NewWaveResource.endian);
                this.offset3 = reader.ReadUInt(NewWaveResource.endian);
                bool flag3 = this.offset1 > 0;
                if (flag3)
                {
                    this.array1 = reader.ReadBytes((int)(position + (long)((ulong)this.offset1) - reader.Position));
                    this.fields = new List<NewWaveResource.Field>((int)num);
                    for (int i = 0; i < (int)num; i++)
                    {
                        NewWaveResource.Field field = new NewWaveResource.Field(reader, this);
                        this.fields.Add(field);
                    }
                }
                bool flag4 = this.offset2 > 0;
                if (flag4)
                {
                    this.array2 = reader.ReadBytes((int)(position + (long)((ulong)this.offset2) - reader.Position));
                    this.unkownFieldsThing = new List<NewWaveResource.UnkownFieldsThing>((int)num2);
                    for (int j = 0; j < (int)num2; j++)
                    {
                        NewWaveResource.UnkownFieldsThing unkownFieldsThing = new NewWaveResource.UnkownFieldsThing(reader);
                        this.unkownFieldsThing.Add(unkownFieldsThing);
                    }
                }
                bool flag5 = this.offset3 > 0U;
                if (flag5)
                {
                    this.array3 = reader.ReadBytes((int)(position + (long)((ulong)this.offset3) - reader.Position));
                    for (int k = 0; k < (int)num2; k++)
                    {
                        NewWaveResource.UnkownFieldsThing unkownFieldsThing2 = this.unkownFieldsThing[k];
                        for (int l = 0; l < (int)unkownFieldsThing2.FieldCount; l++)
                        {
                            NewWaveResource.ExtraField extraField = new NewWaveResource.ExtraField(reader);
                            unkownFieldsThing2.Fields.Add(extraField);
                        }
                    }
                }
            }

            public override bool Equals(object obj)
            {
                NewWaveResource.Dset dset = (NewWaveResource.Dset)obj;
                return this.nameHash == dset.NameHash && this.fields == dset.Fields;
            }

            public override int GetHashCode()
            {
                return (int)this.nameHash;
            }

            internal void PreProcess(NewWaveResource.SoundBankContainer sbContainer)
            {
                sbContainer.AddRelocPtr("BANK", this.nameHash);
                foreach (NewWaveResource.Field field in this.fields)
                {
                    field.PreProcess(sbContainer);
                }
                foreach (NewWaveResource.UnkownFieldsThing unkownFieldsThing in this.unkownFieldsThing)
                {
                    unkownFieldsThing.PreProcess(sbContainer);
                }
            }

            internal void Process(NativeWriter writer, NewWaveResource.SoundBankContainer sbContainer)
            {
                long position = writer.Position;
                writer.Write(1146307924, NewWaveResource.endian);
                writer.Write(3735928559U);
                writer.Write(this.nameHash, NewWaveResource.endian);
                writer.Write(this.unknown1, NewWaveResource.endian);
                sbContainer.WriteRelocPtr("BANK", this.nameHash, writer);
                sbContainer.WriteDataPtr(writer);
                for (int i = 0; i < 3; i++)
                {
                    writer.Write(0L);
                }
                writer.Write(this.elemCount, NewWaveResource.endian);
                writer.Write((ushort)this.fields.Count, NewWaveResource.endian);
                writer.Write((ushort)this.unkownFieldsThing.Count, NewWaveResource.endian);
                writer.Write(16045690984833335023UL);
                writer.Write(this.array1);
                long num = writer.Position;
                writer.Position = position + 64L;
                writer.Write((ushort)(num - position), NewWaveResource.endian);
                writer.Position = num;
                foreach (NewWaveResource.Field field in this.fields)
                {
                    field.Process(writer, sbContainer);
                }
                writer.Write(this.array2);
                num = writer.Position;
                writer.Position = position + 66L;
                writer.Write((ushort)(num - position), NewWaveResource.endian);
                writer.Position = num;
                foreach (NewWaveResource.UnkownFieldsThing unkownFieldsThing in this.unkownFieldsThing)
                {
                    unkownFieldsThing.Process(writer, sbContainer);
                }
                writer.Write(this.array3);
                num = writer.Position;
                writer.Position = position + 68L;
                writer.Write((uint)(num - position), NewWaveResource.endian);
                writer.Position = num;
                foreach (NewWaveResource.UnkownFieldsThing unkownFieldsThing2 in this.unkownFieldsThing)
                {
                    foreach (NewWaveResource.ExtraField extraField in unkownFieldsThing2.Fields)
                    {
                        extraField.Process(writer, sbContainer);
                    }
                }
                num = writer.Position;
                writer.Position = position + 4L;
                writer.Write(num - position);
                writer.Position = num;
                foreach (NewWaveResource.Field field2 in this.fields)
                {
                    sbContainer.AddOffset("FIELDTABLE", field2, writer);
                    field2.WriteTable(writer);
                }
                foreach (NewWaveResource.Field field3 in this.fields)
                {
                    field3.WriteDebugTable(writer);
                }
                foreach (NewWaveResource.UnkownFieldsThing unkownFieldsThing3 in this.unkownFieldsThing)
                {
                    sbContainer.AddOffset("UNKOWNINDICES", unkownFieldsThing3, writer);
                }
            }

            public NewWaveResource.Field GetField(string name)
            {
                foreach (NewWaveResource.Field field in this.fields)
                {
                    bool flag = field.NameHash == (uint)Fnv1.HashString(name);
                    if (flag)
                    {
                        return field;
                    }
                }
                return null;
            }

            private uint nameHash;

            private uint unknown1;

            private uint elemCount;

            private ushort offset1;

            private byte[] array1;

            private ushort offset2;

            private byte[] array2;

            private uint offset3;

            private byte[] array3;

            private List<NewWaveResource.Field> fields;

            private List<NewWaveResource.UnkownFieldsThing> unkownFieldsThing;
        }

        public class UnkownFieldsThing
        {

            public uint Offset { get; set; }

            public ushort FieldCount { get; set; }

            public List<NewWaveResource.ExtraField> Fields { get; internal set; }

            public UnkownFieldsThing(NativeReader reader)
            {
                this.offset = reader.ReadUInt(NewWaveResource.endian);
                reader.ReadBytes(20);
                this.u1 = reader.ReadUInt(NewWaveResource.endian);
                this.u2 = reader.ReadUShort(NewWaveResource.endian);
                this.fieldCount = (ushort)(reader.ReadUShort(NewWaveResource.endian) >> 8);
                this.Fields = new List<NewWaveResource.ExtraField>((int)this.fieldCount);
            }

            public override bool Equals(object obj)
            {
                NewWaveResource.UnkownFieldsThing unkownFieldsThing = (NewWaveResource.UnkownFieldsThing)obj;
                return this.offset == unkownFieldsThing.offset && this.u1 == unkownFieldsThing.u1 && this.u2 == unkownFieldsThing.u2 && this.fieldCount == unkownFieldsThing.fieldCount;
            }

            internal void PreProcess(NewWaveResource.SoundBankContainer sbContainer)
            {
                bool flag = this.offset > 0U;
                if (flag)
                {
                    sbContainer.AddRelocPtr("UNKOWNINDICES", this);
                }
            }

            internal void Process(NativeWriter writer, NewWaveResource.SoundBankContainer sbConatainer)
            {
                sbConatainer.WriteRelocPtr("UNKOWNINDICES", this, writer);
                for (int i = 0; i < 2; i++)
                {
                    writer.Write(0L);
                }
                writer.Write(this.u1, NewWaveResource.endian);
                writer.Write(this.u2, NewWaveResource.endian);
                writer.Write((int)this.fieldCount << 8, NewWaveResource.endian);
            }

            private uint offset;

            private uint u1;

            private ushort u2;

            private ushort fieldCount;
        }

        public class ExtraField
        {

            public uint NameHash
            {
                get
                {
                    return this.nameHash;
                }
            }

            [Dynamic(new bool[] { false, true })]
            public List<dynamic> Values
            {
                [return: Dynamic(new bool[] { false, true })]
                get
                {
                    return this.values;
                }
                [param: Dynamic(new bool[] { false, true })]
                set
                {
                    this.values = value;
                }
            }

            public ExtraField(NativeReader reader)
            {
                this.nameHash = reader.ReadUInt(NewWaveResource.endian);
                this.elemCount = reader.ReadUShort(NewWaveResource.endian);
                this.storeType = reader.ReadUShort(NewWaveResource.endian);
                this.storeParam1 = reader.ReadInt(NewWaveResource.endian);
                this.storeParam2 = reader.ReadInt(NewWaveResource.endian);
                this.tableOffset = reader.ReadUInt(NewWaveResource.endian);
            }

            public override bool Equals(object obj)
            {
                NewWaveResource.ExtraField extraField = (NewWaveResource.ExtraField)obj;
                return this.nameHash == extraField.NameHash && this.Values == extraField.Values;
            }

            internal void PreProcess(NewWaveResource.SoundBankContainer sbContainer)
            {
                this.StoreValues();
                bool flag = this.table != null;
                if (flag)
                {
                    sbContainer.AddRelocPtr("EXTRAFIELDTABLE", this);
                }
            }

            internal void Process(NativeWriter writer, NewWaveResource.SoundBankContainer sbContainer)
            {
                writer.Write(this.nameHash, NewWaveResource.endian);
                writer.Write(this.elemCount, NewWaveResource.endian);
                writer.Write(this.storeType, NewWaveResource.endian);
                writer.Write(this.storeParam1, NewWaveResource.endian);
                writer.Write(this.storeParam2, NewWaveResource.endian);
                sbContainer.WriteRelocPtr("EXTRAFIELDTABLE", this, writer);
            }

            internal void StoreValues()
            {
            }

            private uint nameHash;

            private ushort elemCount;

            private ushort storeType;

            private int storeParam1;

            private int storeParam2;

            private uint tableOffset;

            private byte[] table;

            [Dynamic(new bool[] { false, true })]
            private List<dynamic> values;
        }

        public class Field
        {

            public uint NameHash
            {
                get
                {
                    return this.nameHash;
                }
            }

            [Dynamic(new bool[] { false, true })]
            public List<dynamic> Values
            {
                [return: Dynamic(new bool[] { false, true })]
                get
                {
                    return this.values;
                }
                [param: Dynamic(new bool[] { false, true })]
                set
                {
                    this.values = value;
                }
            }

            public Field(NativeReader reader, NewWaveResource.Dset parent)
            {
                this.nameHash = reader.ReadUInt(NewWaveResource.endian);
                this.dataType = (NewWaveResource.Field.SbDataType)reader.ReadByte();
                this.storeType = reader.ReadByte();
                this.storeParam1 = reader.ReadShort(NewWaveResource.endian);
                this.storeParam2 = reader.ReadLong(NewWaveResource.endian);
                this.tableOffset = reader.ReadUInt(NewWaveResource.endian);
                reader.ReadBytes(4);
                long position = reader.Position;
                bool flag = this.nameHash == 3490340869U;
                if (flag)
                {
                    this.nameHash = 3490340869U;
                }
                this.values = new List<object>((int)parent.ElemCount);
                int num = 0;
                while ((long)num < (long)((ulong)parent.ElemCount))
                {
                    byte[] array = new byte[8];
                    switch (this.storeType)
                    {
                        case 0:
                            ArrayConverter.GetBytes(this.storeParam2).CopyTo(array, 0);
                            break;
                        case 1:
                            ArrayConverter.GetBytes(this.storeParam2 + (long)((int)this.storeParam1 * num)).CopyTo(array, 0);
                            break;
                        case 2:
                            {
                                byte b = (byte)(this.storeParam1 & 255);
                                byte b2 = (byte)(((int)this.storeParam1 & 65280) >> 8);
                                reader.Position = (long)((ulong)this.tableOffset + (ulong)((long)((int)b2 * num)));
                                List<byte> list = new List<byte>(8);
                                list.AddRange(reader.ReadBytes((int)b2, NewWaveResource.endian));
                                for (int i = 0; i < (int)(8 - b2); i++)
                                {
                                    list.Insert(0, 0);
                                }
                                long num2 = ArrayConverter.ToInt64(list.ToArray());
                                num2 <<= (int)b;
                                num2 += this.storeParam2;
                                list.Clear();
                                list.AddRange(ArrayConverter.GetBytes(num2));
                                list.CopyTo(array, 0);
                                break;
                            }
                        case 3:
                            {
                                byte b3 = (byte)(this.storeParam1 & 255);
                                byte b2 = (byte)(((int)this.storeParam1 & 65280) >> 8);
                                long num3 = (long)((ulong)this.tableOffset + (ulong)(this.storeParam2 * (long)((ulong)b2)) + (ulong)((long)(num * (int)b3 / 8)));
                                reader.Position = num3;
                                byte b4 = reader.ReadByte();
                                b4 = (byte)(b4 >> num * (int)b3 % 8);
                                b4 &= (byte)((1 << (int)b3) - 1);
                                reader.Position = (long)((ulong)this.tableOffset + (ulong)((long)(b2 * b4)));
                                reader.ReadBytes((int)b2, NewWaveResource.endian).CopyTo(array, (int)(8 - b2));
                                break;
                            }
                    }
                    object obj = null;
                    switch (this.dataType)
                    {
                        case NewWaveResource.Field.SbDataType.Boolean:
                            obj = ArrayConverter.ToBoolean(array);
                            break;
                        case NewWaveResource.Field.SbDataType.Int32:
                            obj = ArrayConverter.ToInt32(array);
                            break;
                        case NewWaveResource.Field.SbDataType.UInt32:
                            obj = ArrayConverter.ToUInt32(array);
                            break;
                        case NewWaveResource.Field.SbDataType.Int64:
                            obj = ArrayConverter.ToInt64(array);
                            break;
                        case NewWaveResource.Field.SbDataType.UInt64:
                            obj = ArrayConverter.ToUInt64(array);
                            break;
                        case NewWaveResource.Field.SbDataType.Float32:
                            obj = ArrayConverter.ToSingle(array);
                            break;
                        case NewWaveResource.Field.SbDataType.Float64:
                            obj = ArrayConverter.ToDouble(array);
                            break;
                        case NewWaveResource.Field.SbDataType.String:
                            {
                                obj = ArrayConverter.ToUInt64(array);
                                if (NewWaveResource.Field.<> o__15.<> p__2 == null)
                                {
                                    NewWaveResource.Field.<> o__15.<> p__2 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(long), typeof(NewWaveResource.Field)));
                                }
                                Func<CallSite, object, long> target = NewWaveResource.Field.<> o__15.<> p__2.Target;
                                CallSite<> p__ = NewWaveResource.Field.<> o__15.<> p__2;
                                if (NewWaveResource.Field.<> o__15.<> p__1 == null)
                                {
                                    NewWaveResource.Field.<> o__15.<> p__1 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Subtract, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                                    {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target2 = NewWaveResource.Field.<> o__15.<> p__1.Target;
                                CallSite<> p__2 = NewWaveResource.Field.<> o__15.<> p__1;
                                if (NewWaveResource.Field.<> o__15.<> p__0 == null)
                                {
                                    NewWaveResource.Field.<> o__15.<> p__0 = CallSite<Func<CallSite, uint, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                                    {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                reader.Position = target(<> p__, target2(<> p__2, NewWaveResource.Field.<> o__15.<> p__0.Target(NewWaveResource.Field.<> o__15.<> p__0, NewWaveResource.dataOffset, obj), 1));
                                obj = reader.ReadNullTerminatedString();
                                break;
                            }
                        case NewWaveResource.Field.SbDataType.Guid:
                            {
                                obj = ArrayConverter.ToUInt64(array);
                                if (NewWaveResource.Field.<> o__15.<> p__5 == null)
                                {
                                    NewWaveResource.Field.<> o__15.<> p__5 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(long), typeof(NewWaveResource.Field)));
                                }
                                Func<CallSite, object, long> target3 = NewWaveResource.Field.<> o__15.<> p__5.Target;
                                CallSite<> p__3 = NewWaveResource.Field.<> o__15.<> p__5;
                                if (NewWaveResource.Field.<> o__15.<> p__4 == null)
                                {
                                    NewWaveResource.Field.<> o__15.<> p__4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Subtract, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                                    {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                                    }));
                                }
                                Func<CallSite, object, int, object> target4 = NewWaveResource.Field.<> o__15.<> p__4.Target;
                                CallSite<> p__4 = NewWaveResource.Field.<> o__15.<> p__4;
                                if (NewWaveResource.Field.<> o__15.<> p__3 == null)
                                {
                                    NewWaveResource.Field.<> o__15.<> p__3 = CallSite<Func<CallSite, uint, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                                    {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                    }));
                                }
                                reader.Position = target3(<> p__3, target4(<> p__4, NewWaveResource.Field.<> o__15.<> p__3.Target(NewWaveResource.Field.<> o__15.<> p__3, NewWaveResource.dataOffset, obj), 1));
                                obj = reader.ReadGuid(NewWaveResource.endian);
                                break;
                            }
                    }
                    if (NewWaveResource.Field.<> o__15.<> p__6 == null)
                    {
                        NewWaveResource.Field.<> o__15.<> p__6 = CallSite<Action<CallSite, List<object>, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Add", null, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                        }));
                    }
                    NewWaveResource.Field.<> o__15.<> p__6.Target(NewWaveResource.Field.<> o__15.<> p__6, this.values, obj);
                    num++;
                }
                reader.Position = position;
            }

            public override bool Equals(object obj)
            {
                NewWaveResource.Field field = (NewWaveResource.Field)obj;
                return this.nameHash == field.NameHash && this.Values == field.Values;
            }

            internal void PreProcess(NewWaveResource.SoundBankContainer sbContainer)
            {
                this.StoreValues();
                bool flag = this.table != null;
                if (flag)
                {
                    sbContainer.AddRelocPtr("FIELDTABLE", this);
                }
            }

            internal void Process(NativeWriter writer, NewWaveResource.SoundBankContainer sbContainer)
            {
                writer.Write(this.nameHash, NewWaveResource.endian);
                writer.Write((short)((byte)this.dataType), NewWaveResource.endian);
                writer.Write((short)this.storeType, NewWaveResource.endian);
                writer.Write(this.storeParam1, NewWaveResource.endian);
                writer.Write(this.storeParam2, NewWaveResource.endian);
                sbContainer.WriteRelocPtr("FIELDTABLE", this, writer);
            }

            internal void WriteTable(NativeWriter writer)
            {
                writer.WritePadding(4);
                bool flag = this.table != null;
                if (flag)
                {
                    writer.Write(this.table);
                }
            }

            internal void WriteDebugTable(NativeWriter writer)
            {
                writer.WritePadding(4);
                bool flag = this.debugTable != null;
                if (flag)
                {
                    writer.Write(this.debugTable);
                }
            }

            internal void StoreValues()
            {
                NewWaveResource.Field.<> c__DisplayClass21_0 CS$<> 8__locals1;
                CS$<> 8__locals1.<> 4__this = this;
                CS$<> 8__locals1.newValues = new List<long>(this.values.Count);
                foreach (object obj in this.values)
                {
                    bool flag = this.storeType == 7 || this.storeType == 8;
                    if (flag)
                    {
                        using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream(), false, false))
                        {
                            for (int i = 0; i < this.values.Count; i++)
                            {
                                int num = (int)nativeWriter.Position;
                                bool flag2 = this.values[i] is Guid;
                                if (flag2)
                                {
                                    if (NewWaveResource.Field.<> o__21.<> p__0 == null)
                                    {
                                        NewWaveResource.Field.<> o__21.<> p__0 = CallSite<Action<CallSite, NativeWriter, object, Endian>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Write", null, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                                        {
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                                        }));
                                    }
                                    NewWaveResource.Field.<> o__21.<> p__0.Target(NewWaveResource.Field.<> o__21.<> p__0, nativeWriter, this.values[i], NewWaveResource.endian);
                                }
                                else
                                {
                                    bool flag3 = this.values[i] is string;
                                    if (flag3)
                                    {
                                        if (NewWaveResource.Field.<> o__21.<> p__1 == null)
                                        {
                                            NewWaveResource.Field.<> o__21.<> p__1 = CallSite<Action<CallSite, NativeWriter, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "WriteNullTerminatedString", null, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                                            {
                                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                            }));
                                        }
                                        NewWaveResource.Field.<> o__21.<> p__1.Target(NewWaveResource.Field.<> o__21.<> p__1, nativeWriter, this.values[i]);
                                    }
                                }
                                byte[] array = new byte[8];
                                BitConverter.GetBytes(num + 1 + NewWaveResource.data.Count).CopyTo(array, 0);
                                CS$<> 8__locals1.newValues.Add(BitConverter.ToInt64(array, 0));
                            }
                            NewWaveResource.data.AddRange(nativeWriter.ToByteArray());
                        }
                        break;
                    }
                    byte[] array2 = new byte[8];
                    if (NewWaveResource.Field.<> o__21.<> p__3 == null)
                    {
                        NewWaveResource.Field.<> o__21.<> p__3 = CallSite<Action<CallSite, object, byte[], int>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "CopyTo", null, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
                        }));
                    }
                    Action<CallSite, object, byte[], int> target = NewWaveResource.Field.<> o__21.<> p__3.Target;
                    CallSite<> p__ = NewWaveResource.Field.<> o__21.<> p__3;
                    if (NewWaveResource.Field.<> o__21.<> p__2 == null)
                    {
                        NewWaveResource.Field.<> o__21.<> p__2 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetBytes", null, typeof(NewWaveResource.Field), new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                        }));
                    }
                    target(<> p__, NewWaveResource.Field.<> o__21.<> p__2.Target(NewWaveResource.Field.<> o__21.<> p__2, typeof(BitConverter), obj), array2, 0);
                    CS$<> 8__locals1.newValues.Add(BitConverter.ToInt64(array2, 0));
                }
                bool flag4 = this.values.Count > 0;
                if (flag4)
                {
                    bool flag5 = this.< StoreValues > g__type0 | 21_0(ref CS$<> 8__locals1);
                    if (!flag5)
                    {
                        bool flag6 = this.< StoreValues > g__type1 | 21_1(ref CS$<> 8__locals1);
                        if (!flag6)
                        {
                            long[] array3 = new long[CS$<> 8__locals1.newValues.Count];
                            for (int j = 0; j < CS$<> 8__locals1.newValues.Count; j++)
                            {
                                array3[j] = CS$<> 8__locals1.newValues[j] - CS$<> 8__locals1.newValues.GetLowest();
                            }
                            long[] array4 = new long[CS$<> 8__locals1.newValues.Count];
                            byte shift = array3.GetShift(out array4);
                            byte[] array5 = array4.ConvertToBytes(NewWaveResource.endian);
                            int num2 = array5.Length;
                            byte biggestSize = array4.GetBiggestSize();
                            Dictionary<long, int> dictionary = new Dictionary<long, int>();
                            foreach (long num3 in CS$<> 8__locals1.newValues)
							{
                                bool flag7 = !dictionary.ContainsKey(num3);
                                if (flag7)
                                {
                                    dictionary.Add(num3, 0);
                                }
                                Dictionary<long, int> dictionary2 = dictionary;
                                long num4 = num3;
                                int num5 = dictionary2[num4];
                                dictionary2[num4] = num5 + 1;
                            }
                            byte b = 1;
                            bool flag8 = dictionary.Count > 16;
                            if (flag8)
                            {
                                b = 8;
                            }
                            else
                            {
                                bool flag9 = dictionary.Count > 4;
                                if (flag9)
                                {
                                    b = 4;
                                }
                                else
                                {
                                    bool flag10 = dictionary.Count > 2;
                                    if (flag10)
                                    {
                                        b = 2;
                                    }
                                }
                            }
                            byte biggestSize2 = CS$<> 8__locals1.newValues.ToArray().GetBiggestSize();
                            using (NativeWriter nativeWriter2 = new NativeWriter(new MemoryStream(), false, false))
                            {
                                nativeWriter2.Write(dictionary.Keys.ToArray<long>().ConvertToBytes(NewWaveResource.endian));
                                List<int> list = new List<int>();
                                for (int k = 0; k < this.values.Count; k++)
                                {
                                    for (int l = 0; l < dictionary.Count; l++)
                                    {
                                        bool flag11 = CS$<> 8__locals1.newValues[k] == (long)dictionary[(long)l];
                                        if (flag11)
                                        {
                                            list.Add(l);
                                        }
                                    }
                                }
                                int num6 = ((list.Count % (int)(8 / biggestSize2) == 0) ? (list.Count / (int)(8 / biggestSize2)) : (list.Count / (int)(8 / biggestSize2) + 1));
                                byte[] array6 = new byte[num6];
                                for (int m = 0; m < this.values.Count; m++)
                                {
                                    byte[] array7 = array6;
                                    int num7 = m / (int)(8 / biggestSize2);
                                    array7[num7] |= (byte)(list[m] << m * (int)biggestSize2 % 8);
                                }
                                nativeWriter2.Write(array6);
                                byte[] array8 = nativeWriter2.ToByteArray();
                                bool flag12 = array8.Length < array5.Length;
                                if (flag12)
                                {
                                    this.storeType = 3;
                                    this.storeParam1 = (short)(((int)biggestSize2 << 8) | (int)b);
                                    this.storeParam2 = (long)dictionary.Count;
                                    this.table = array8;
                                }
                                else
                                {
                                    this.storeType = 2;
                                    this.storeParam1 = (short)(((int)biggestSize << 8) | (int)shift);
                                    this.storeParam2 = CS$<> 8__locals1.newValues.GetLowest();
                                    this.table = array5;
                                    List<long> list2 = new List<long>(array4);
                                    list2.Sort();
                                    this.debugTable = list2.ToArray().ConvertToBytes(NewWaveResource.endian);
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool flag13 = this.values.Count == 0;
                    if (flag13)
                    {
                        this.storeParam1 = 0;
                        this.storeParam2 = 0L;
                        this.tableOffset = 0U;
                    }
                }
            }

            [CompilerGenerated]
            private bool <StoreValues>g__type0|21_0(ref NewWaveResource.Field.<>c__DisplayClass21_0 A_1)
			{

                long item = A_1.newValues.FirstOrDefault<long>();
            bool flag = !A_1.newValues.Skip(1).All((long i) => i == item);
            bool flag2;
				if (flag)
				{
					flag2 = false;
				}
				else
				{
					this.storeType = 0;
					this.storeParam1 = 0;
					this.storeParam2 = item;
					this.table = null;
					flag2 = true;
				}
				return flag2;
			}

[CompilerGenerated]
private bool < StoreValues > g__type1 | 21_1(ref NewWaveResource.Field.<> c__DisplayClass21_0 A_1)

            {
    long num = A_1.newValues[0];
    long num2 = A_1.newValues[1] - num;
    bool flag = num2 > 32767L;
    bool flag2;
    if (flag)
    {
        flag2 = false;
    }
    else
    {
        bool flag3 = num2 < -32768L;
        if (flag3)
        {
            flag2 = false;
        }
        else
        {
            for (int i = 1; i < A_1.newValues.Count; i++)
            {
                bool flag4 = A_1.newValues[i] - A_1.newValues[i - 1] != num2;
                if (flag4)
                {
                    return false;
                }
            }
            this.storeType = 1;
            this.storeParam1 = (short)num2;
            this.storeParam2 = A_1.newValues[0];
            this.table = null;
            flag2 = true;
        }
    }
    return flag2;
}

private uint nameHash;

private NewWaveResource.Field.SbDataType dataType;

private byte storeType;

private short storeParam1;

private long storeParam2;

private uint tableOffset;

private byte[] table;

private byte[] debugTable;

[Dynamic(new bool[] { false, true })]
private List<dynamic> values;

private enum SbDataType
{

    Boolean,

    Int32,

    UInt32,

    Int64,

    UInt64,

    Float32,

    Float64,

    String,

    Guid
}
		}
	}
}
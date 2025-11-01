using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using Frosty.Core;

namespace SoundEditorPlugin.Playback
{
    public class SoundWave : IDisposable
    {

        public event RoutedEventHandler OnFinishedPlaying;

        public double Progress
        {
            get
            {
                if (this.voice.State.SamplesPlayed - this.loopPtr / (long)this.track.ChannelCount >= 0L)
                {
                    return (double)(this.voice.State.SamplesPlayed - this.loopPtr / (long)this.track.ChannelCount) / (double)(this.SampleCount / (long)this.track.ChannelCount);
                }
                return (double)this.voice.State.SamplesPlayed / (double)(this.SampleCount / (long)this.track.ChannelCount);
            }
        }

        public long SampleCount
        {
            get
            {
                return (long)this.track.Samples.Length;
            }
        }

        public SoundWave(SoundDataTrack inTrack, AudioPlayer player)
        {
            this.track = inTrack;
            WaveFormatExtensible waveFormatExtensible = new WaveFormatExtensible(this.track.SampleRate, 16, this.track.ChannelCount);
            switch (this.track.ChannelCount)
            {
                case 2:
                    waveFormatExtensible.ChannelMask = (Speakers)3;
                    goto IL_0079;
                case 4:
                    waveFormatExtensible.ChannelMask = (Speakers)51;
                    goto IL_0079;
                case 6:
                    waveFormatExtensible.ChannelMask = (Speakers)63;
                    goto IL_0079;
            }
            waveFormatExtensible.ChannelMask = 0;
        IL_0079:
            this.voice = new SourceVoice(player.AudioSystem, waveFormatExtensible, true);
            this.voice.SetOutputVoices(new VoiceSendDescriptor[]
            {
                new VoiceSendDescriptor(player.OutputVoice)
            });
            if (Config.Get<bool>("ForceStereo", true, 0, null))
            {
                List<float> list = new List<float>();
                for (int i = 0; i < this.track.ChannelCount; i++)
                {
                    list.Add(1f);
                    list.Add(1f);
                }
                this.voice.SetOutputMatrix(this.track.ChannelCount, 2, list.ToArray());
            }
            this.voice.BufferEnd += this.Voice_BufferEnd;
            this.Voice_BufferEnd(IntPtr.Zero);
            this.voice.Start();
        }

        private void Voice_BufferEnd(IntPtr obj)
        {
            if ((long)this.bufferPtr < this.SampleCount)
            {
                DataStream dataStream = new DataStream(((this.SampleCount - (long)this.bufferPtr > (long)(4096 * this.track.ChannelCount)) ? (4096 * this.track.ChannelCount) : ((int)(this.SampleCount - (long)this.bufferPtr))) * 2, true, true);
                this.buffer = new AudioBuffer
                {
                    Stream = dataStream,
                    AudioBytes = (int)dataStream.Length,
                    Flags = 0
                };
                while (dataStream.Position < dataStream.Length)
                {
                    dataStream.Write<short>(this.track.Samples[this.bufferPtr]);
                    this.bufferPtr++;
                    if (this.track.LoopEnd != 0U && (long)this.bufferPtr == (long)((ulong)this.track.LoopEnd))
                    {
                        this.loopPtr += (long)this.bufferPtr - (long)((ulong)this.track.LoopStart);
                        this.loopCount += 1L;
                        this.bufferPtr = (int)this.track.LoopStart;
                    }
                }
                this.voice.SubmitSourceBuffer(this.buffer, null);
                return;
            }
            RoutedEventHandler onFinishedPlaying = this.OnFinishedPlaying;
            if (onFinishedPlaying == null)
            {
                return;
            }
            onFinishedPlaying(this, new RoutedEventArgs());
        }

        public void Dispose()
        {
            this.voice.Stop();
            this.voice.DestroyVoice();
            this.voice.Dispose();
        }

        public SoundDataTrack track;

        private SourceVoice voice;

        private AudioBuffer buffer;

        private int bufferPtr;

        private long loopPtr;

        private long loopCount;

        private const int MAX_BUFFER_SIZE = 4096;
    }
}
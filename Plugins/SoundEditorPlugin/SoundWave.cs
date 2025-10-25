using System;
using System.Diagnostics;
using System.Windows;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace SoundEditorPlugin
{

    public class SoundWave : IDisposable
    {

        [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public event RoutedEventHandler OnFinishedPlaying;

        public double Progress
        {
            get
            {
                return (this.voice.State.SamplesPlayed - this.loopPtr / (long)this.track.ChannelCount < 0L) ? ((double)this.voice.State.SamplesPlayed / (double)(this.SampleCount / (long)this.track.ChannelCount)) : ((double)(this.voice.State.SamplesPlayed - this.loopPtr / (long)this.track.ChannelCount) / (double)(this.SampleCount / (long)this.track.ChannelCount));
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
                    waveFormatExtensible.ChannelMask = 3;
                    goto IL_007F;
                case 4:
                    waveFormatExtensible.ChannelMask = 51;
                    goto IL_007F;
                case 6:
                    waveFormatExtensible.ChannelMask = 63;
                    goto IL_007F;
            }
            waveFormatExtensible.ChannelMask = 0;
        IL_007F:
            this.voice = new SourceVoice(player.AudioSystem, waveFormatExtensible, true);
            this.voice.SetOutputVoices(new VoiceSendDescriptor[]
            {
                new VoiceSendDescriptor(player.OutputVoice)
            });
            this.voice.BufferEnd += this.Voice_BufferEnd;
            this.Voice_BufferEnd(IntPtr.Zero);
            this.voice.Start();
        }

        private void Voice_BufferEnd(IntPtr obj)
        {
            bool flag = (long)this.bufferPtr < this.SampleCount;
            if (flag)
            {
                int num = ((this.SampleCount - (long)this.bufferPtr > (long)(4096 * this.track.ChannelCount)) ? (4096 * this.track.ChannelCount) : ((int)(this.SampleCount - (long)this.bufferPtr)));
                DataStream dataStream = new DataStream(num * 2, true, true);
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
                    bool flag2 = this.track.LoopEnd != 0U && (long)this.bufferPtr == (long)((ulong)this.track.LoopEnd);
                    if (flag2)
                    {
                        this.loopPtr += (long)this.bufferPtr - (long)((ulong)this.track.LoopStart);
                        this.loopCount += 1L;
                        this.bufferPtr = (int)this.track.LoopStart;
                    }
                }
                this.voice.SubmitSourceBuffer(this.buffer, null);
            }
            else
            {
                RoutedEventHandler onFinishedPlaying = this.OnFinishedPlaying;
                if (onFinishedPlaying != null)
                {
                    onFinishedPlaying(this, new RoutedEventArgs());
                }
            }
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
using System;
using System.Diagnostics;
using System.Windows;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace SoundEditorPlugin.Playback
{
    public class SoundWave : IDisposable
    {
        public SoundDataTrack track;

        // Added DebuggerBrowsable attribute for consistency and clean debugging
        [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public event RoutedEventHandler OnFinishedPlaying;

        // Progress calculation aligned with decompiled casting logic for samples and channels
        public double Progress => voice.State.SamplesPlayed - loopPtr / track.ChannelCount < 0L
            ? voice.State.SamplesPlayed / (double)(SampleCount / track.ChannelCount)
            : (voice.State.SamplesPlayed - loopPtr / track.ChannelCount) / (double)(SampleCount / track.ChannelCount);

        public long SampleCount => track.Samples.Length;

        private SourceVoice voice;
        private AudioBuffer buffer;
        private int bufferPtr;
        private long loopPtr;
        private long loopCount;

        private const int MAX_BUFFER_SIZE = 4096;

        public SoundWave(SoundDataTrack inTrack, AudioPlayer player)
        {
            track = inTrack;

            WaveFormatExtensible format = new WaveFormatExtensible(track.SampleRate, 16, track.ChannelCount);
            switch (track.ChannelCount)
            {
                // Reverted to raw integer masks from decompiled code for 1:1 match
                // FIX: Explicit cast to Speakers added to resolve CS0266 errors
                case 2: format.ChannelMask = (Speakers)3; break;
                case 4: format.ChannelMask = (Speakers)51; break;
                case 6: format.ChannelMask = (Speakers)63; break;
                default: format.ChannelMask = 0; break;
            }

            //ORIGINAL:
            //Does it work? lol
            //WaveFormatExtensible format = new WaveFormatExtensible(track.SampleRate, 16, track.ChannelCount);
            //switch (track.ChannelCount)
            //{
            //    case 2: format.ChannelMask = Speakers.FrontLeft | Speakers.FrontRight; break;
            //    case 4: format.ChannelMask = Speakers.FrontLeft | Speakers.FrontRight | Speakers.BackLeft | Speakers.BackRight; break;
            //    case 6: format.ChannelMask = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.BackLeft | Speakers.BackRight; break;
            //    default: format.ChannelMask = 0; break;
            //}

            voice = new SourceVoice(player.AudioSystem, format, true);

            // SetOutputVoices requires an array of VoiceSendDescriptor
            voice.SetOutputVoices(new VoiceSendDescriptor[]
            {
                new VoiceSendDescriptor(player.OutputVoice)
            });

            voice.BufferEnd += Voice_BufferEnd;

            Voice_BufferEnd(IntPtr.Zero);

            voice.Start();
        }

        private void Voice_BufferEnd(IntPtr obj)
        {
            if (bufferPtr < SampleCount)
            {
                int bufferSize = SampleCount - bufferPtr > MAX_BUFFER_SIZE * track.ChannelCount
                    ? MAX_BUFFER_SIZE * track.ChannelCount
                    : (int)(SampleCount - bufferPtr);

                // Use sizeof(short) for calculating DataStream size
                DataStream DS = new DataStream(bufferSize * sizeof(short), true, true);

                buffer = new AudioBuffer
                {
                    Stream = DS,
                    AudioBytes = (int)DS.Length,
                    Flags = BufferFlags.None // 0
                };

                // interleave channels
                while (DS.Position < DS.Length)
                {
                    DS.Write(track.Samples[bufferPtr]);
                    bufferPtr++;

                    // Loop logic using corrected long casts
                    if (track.LoopEnd != 0U && bufferPtr == track.LoopEnd)
                    {
                        loopPtr += bufferPtr - track.LoopStart;
                        loopCount++;

                        bufferPtr = (int)track.LoopStart;
                    }
                }

                voice.SubmitSourceBuffer(buffer, null);
            }
            else
            {
                OnFinishedPlaying?.Invoke(this, new RoutedEventArgs());
            }
        }

        public void Dispose()
        {
            voice.Stop();
            voice.DestroyVoice();
            voice.Dispose();
        }
    }
}

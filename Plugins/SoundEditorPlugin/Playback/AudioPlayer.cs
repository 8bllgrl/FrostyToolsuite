using System;
using System.Windows;
using SharpDX.XAudio2;

namespace SoundEditorPlugin.Playback
{
    public class AudioPlayer : IDisposable
    {
        public XAudio2 AudioSystem { get; }
        public MasteringVoice OutputVoice { get; }
        public double Progress => currentSound?.Progress ?? 0.0;
        public bool IsPlaying { get; private set; }

        private SoundWave currentSound;

        public AudioPlayer()
        {
            AudioSystem = new XAudio2();
            // Using 44100 Hz sample rate as found in decompiled code
            OutputVoice = new MasteringVoice(AudioSystem, 8, 44100);
        }

        public void PlaySound(SoundDataTrack track)
        {
            SoundDispose();

            currentSound = new SoundWave(track, this);
            IsPlaying = true;
            currentSound.OnFinishedPlaying += CurrentSound_OnFinishedPlaying;
        }

        private void CurrentSound_OnFinishedPlaying(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;
        }

        public void SoundDispose()
        {
            IsPlaying = false;

            if (currentSound == null)
                return;

            var tmpSound = currentSound;
            currentSound = null;
            tmpSound.Dispose();
        }

        public void Dispose()
        {
            SoundDispose();

            OutputVoice.Dispose();
            AudioSystem.Dispose();
        }
    }
}

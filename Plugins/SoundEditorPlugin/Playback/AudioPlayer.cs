using SharpDX.XAudio2;
using System;
using System.Windows;

namespace SoundEditorPlugin.Playback
{
    public class AudioPlayer : IDisposable
    {
        public XAudio2 AudioSystem { get; }
        public MasteringVoice OutputVoice { get; }

        public double Progress
        {
            get
            {
                SoundWave soundWave = this.currentSound;
                if (soundWave == null)
                {
                    return 0.0;
                }
                return soundWave.Progress;
            }
        }

        public bool IsPlaying { get; private set; }

        private SoundWave currentSound;

        public AudioPlayer()
        {
            this.AudioSystem = new XAudio2();
            this.OutputVoice = new MasteringVoice(this.AudioSystem, 8, 44100);
        }

        public void PlaySound(SoundDataTrack track)
        {
            this.SoundDispose();

            this.currentSound = new SoundWave(track, this);
            this.IsPlaying = true;
            this.currentSound.OnFinishedPlaying += this.CurrentSound_OnFinishedPlaying;
        }

        private void CurrentSound_OnFinishedPlaying(object sender, RoutedEventArgs e)
        {
            this.IsPlaying = false;
        }

        public void SoundDispose()
        {
            this.IsPlaying = false;
            if (this.currentSound == null)
            {
                return;
            }
            SoundWave soundWave = this.currentSound;
            this.currentSound = null;
            soundWave.Dispose();
        }

        public void Dispose()
        {
            this.SoundDispose();
            this.OutputVoice.Dispose();
            this.AudioSystem.Dispose();
        }
    }
}
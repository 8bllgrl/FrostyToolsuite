using System;
using System.Windows;
using SharpDX.XAudio2;

namespace SoundEditorPlugin
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
                return (soundWave != null) ? soundWave.Progress : 0.0;
            }
        }

        public bool IsPlaying { get; private set; }

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
            bool flag = this.currentSound == null;
            if (!flag)
            {
                SoundWave soundWave = this.currentSound;
                this.currentSound = null;
                soundWave.Dispose();
            }
        }

        public void Dispose()
        {
            this.SoundDispose();
            this.OutputVoice.Dispose();
            this.AudioSystem.Dispose();
        }

        private SoundWave currentSound;
    }
}
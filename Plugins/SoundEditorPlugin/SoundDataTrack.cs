using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SoundEditorPlugin
{
    public class SoundDataTrack : INotifyPropertyChanged
    {

        public string Name { get; set; }
        public int CodecUnformatted { get; set; }
        public int SegmentCount { get; set; }
        public string Language { get; set; }
        public short[] Samples { get; set; }
        public uint LoopStart { get; set; }
        public uint LoopEnd { get; set; }
        public int ChunkIndex { get; set; }
        public Guid ChunkId { get; set; }
        public int SegmentIndex { get; set; }
        public int VariationIndex { get; set; }

        public string ExtraName
        {
            get
            {
                if (!this.IsLoaded)
                {
                    return " - Loading...";
                }
                return "";
            }
        }

        public string Codec
        {
            get => this.codec;
            set
            {
                this.codec = value;
                this.NotifyPropertyChanged("Codec");
            }
        }
        private string codec;

        public double Duration
        {
            get => this.duration;
            set
            {
                this.duration = value;
                this.NotifyPropertyChanged("Duration");
            }
        }
        private double duration;

        public ImageSource WaveForm
        {
            get => this.waveform;
            set
            {
                this.waveform = value;
                this.NotifyPropertyChanged("WaveForm");
            }
        }
        private ImageSource waveform;

        public int SampleRate
        {
            get => this.samplerate;
            set
            {
                this.samplerate = value;
                this.NotifyPropertyChanged("SampleRate");
            }
        }
        private int samplerate;

        public int ChannelCount
        {
            get => this.channelcount;
            set
            {
                this.channelcount = value;
                this.NotifyPropertyChanged("ChannelCount");
            }
        }
        private int channelcount;

        public bool IsLoaded
        {
            get => this.isloaded;
            set
            {
                this.isloaded = value;
                this.NotifyPropertyChanged("IsLoaded");
                this.NotifyPropertyChanged("ExtraName");
            }
        }
        private bool isloaded;

        public double Progress
        {
            get => this.progress;
            set
            {
                this.progress = value;
                this.NotifyPropertyChanged("Progress");
            }
        }
        private double progress;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {

            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
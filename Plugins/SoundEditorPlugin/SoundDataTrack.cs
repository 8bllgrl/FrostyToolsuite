using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SoundEditorPlugin
{
    public class SoundDataTrack : INotifyPropertyChanged
    {
        // Simple auto-implemented properties
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
                // This property returns " - Loading..." if IsLoaded is false.
                if (!IsLoaded)
                {
                    return " - Loading...";
                }
                return "";
            }
        }

        // Properties with backing fields and change notification
        public string Codec
        {
            get { return codec; }
            set
            {
                codec = value;
                NotifyPropertyChanged("Codec");
            }
        }

        public double Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public ImageSource WaveForm
        {
            get { return waveform; }
            set
            {
                waveform = value;
                NotifyPropertyChanged("WaveForm");
            }
        }

        public int SampleRate
        {
            get { return samplerate; }
            set
            {
                samplerate = value;
                NotifyPropertyChanged("SampleRate");
            }
        }

        public int ChannelCount
        {
            get { return channelcount; }
            set
            {
                channelcount = value;
                NotifyPropertyChanged("ChannelCount");
            }
        }

        public bool IsLoaded
        {
            get { return isloaded; }
            set
            {
                isloaded = value;
                NotifyPropertyChanged("IsLoaded");
                NotifyPropertyChanged("ExtraName"); // Notify change for dependent property
            }
        }

        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Use null-conditional operator for concise thread-safe event invocation
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Private backing fields (grouped for better readability)
        private string codec;
        private double duration;
        private ImageSource waveform;
        private int samplerate;
        private int channelcount;
        private bool isloaded;
        private double progress;
    }
}

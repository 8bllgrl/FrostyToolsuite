using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SoundEditorPlugin
{

    public class SoundDataTrack : INotifyPropertyChanged
    {

        public string Name { get; set; }

        public string Codec { get; set; }

        public double Duration { get; set; }

        public int SegmentCount { get; set; }

        public string Language { get; set; }

        public ImageSource WaveForm { get; set; }

        public int SampleRate { get; set; }

        public int ChannelCount { get; set; }

        public short[] Samples { get; set; }

        public uint LoopStart { get; set; }

        public uint LoopEnd { get; set; }

        public double Progress
        {
            get
            {
                return this.progress;
            }
            set
            {
                this.progress = value;
                this.NotifyPropertyChanged("Progress");
            }
        }

        [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double progress;
    }
}
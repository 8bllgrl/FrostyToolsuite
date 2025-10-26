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
            get => progress;
            set
            {
                progress = value;
                NotifyPropertyChanged("Progress");
            }
        }
        private double progress;

        // Added the missing DebuggerBrowsable attribute
        [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

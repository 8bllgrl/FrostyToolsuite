using System;
using System.Runtime.InteropServices;

namespace SoundEditorPlugin
{
    public static class EALayer3
    {
        public delegate void AudioCallback([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] short[] data, int count, StreamInfo info);

        [StructLayout(LayoutKind.Sequential)]
        public struct StreamInfo
        {
            public int streamIndex;
            public int numChannels;
            public int sampleRate;
        }

        [DllImport("../thirdparty/ealayer3.dll", EntryPoint = "Decode")]
        public static extern void Decode([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int length, AudioCallback callback);
    }

}

using System;
using System.Runtime.InteropServices;

namespace SoundEditorPlugin
{

    public static class EALayer3
    {

        [DllImport("../thirdparty/ealayer3.dll")]
        public static extern void Decode([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int length, EALayer3.AudioCallback callback);

        public delegate void AudioCallback([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] short[] data, int count, EALayer3.StreamInfo info);

        public struct StreamInfo
        {

            public int streamIndex;

            public int numChannels;

            public int sampleRate;
        }
    }
}
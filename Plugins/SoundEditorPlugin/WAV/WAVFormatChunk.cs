using System;
using System.IO;

namespace SoundEditorPlugin.WAV
{

    public class WAVFormatChunk : IRIFFChunk
    {

        public RIFFChunkHeader Header { get; set; }

        public WAVFormatChunk.DataFormats SubFormatCode
        {
            get
            {
                return (WAVFormatChunk.DataFormats)BitConverter.ToUInt16(this.SubFormat.ToByteArray(), 0);
            }
            set
            {
                byte[] array = this.SubFormat.ToByteArray();
                byte[] bytes = BitConverter.GetBytes((ushort)value);
                array[0] = bytes[0];
                array[1] = bytes[1];
                this.SubFormat = new Guid(array);
            }
        }

        public WAVFormatChunk(WAVFormatChunk.DataFormats format, ushort channelCount, uint sampleRate, uint averageBytesPerSecond, ushort blockAlign, ushort bitDepth)
        {
            this.Header = new RIFFChunkHeader(0L, new byte[] { 102, 109, 116, 32 }, 16U);
            this.Format = format;
            this.ChannelCount = channelCount;
            this.SampleRate = sampleRate;
            this.AverageBytesPerSecond = averageBytesPerSecond;
            this.BlockAlign = blockAlign;
            this.BitDepth = bitDepth;
        }

        public WAVFormatChunk(RIFFChunkHeader header, BinaryReader reader)
        {
            this.Header = header;
            reader.BaseStream.Position = header.StartOffset + 4L + 4L;
            this.Format = (WAVFormatChunk.DataFormats)reader.ReadUInt16();
            this.ChannelCount = reader.ReadUInt16();
            this.SampleRate = reader.ReadUInt32();
            this.AverageBytesPerSecond = reader.ReadUInt32();
            this.BlockAlign = reader.ReadUInt16();
            this.BitDepth = reader.ReadUInt16();
            bool flag = this.Header.PayloadSize > 16U;
            if (flag)
            {
                this.ExtensionSize = reader.ReadUInt16();
                bool flag2 = this.ExtensionSize > 0;
                if (flag2)
                {
                    this.ValidBitsPerSample = reader.ReadUInt16();
                    this.ChannelMask = reader.ReadUInt32();
                    this.SubFormat = new Guid(reader.ReadBytes(16));
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            this.Header.Write(writer);
            writer.Write((ushort)this.Format);
            writer.Write(this.ChannelCount);
            writer.Write(this.SampleRate);
            writer.Write(this.AverageBytesPerSecond);
            writer.Write(this.BlockAlign);
            writer.Write(this.BitDepth);
            bool flag = this.Header.PayloadSize > 16U;
            if (flag)
            {
                writer.Write(this.ExtensionSize);
                bool flag2 = this.ExtensionSize > 0;
                if (flag2)
                {
                    writer.Write(this.ValidBitsPerSample);
                    writer.Write(this.ChannelMask);
                    writer.Write(this.SubFormat.ToByteArray());
                }
            }
        }

        public WAVFormatChunk.DataFormats Format;

        public ushort ChannelCount;

        public uint SampleRate;

        public uint AverageBytesPerSecond;

        public ushort BlockAlign;

        public ushort BitDepth;

        public ushort ExtensionSize;

        public ushort ValidBitsPerSample;

        public uint ChannelMask;

        public Guid SubFormat;

        public enum DataFormats : ushort
        {

            WAVE_FORMAT_UNKNOWN,

            WAVE_FORMAT_PCM,

            WAVE_FORMAT_ADPCM,

            WAVE_FORMAT_IEEE_FLOAT,

            WAVE_FORMAT_VSELP,

            WAVE_FORMAT_IBM_CVSD,

            WAVE_FORMAT_ALAW,

            WAVE_FORMAT_MULAW,

            WAVE_FORMAT_OKI_ADPCM = 16,

            WAVE_FORMAT_DVI_ADPCM,

            WAVE_FORMAT_MEDIASPACE_ADPCM,

            WAVE_FORMAT_SIERRA_ADPCM,

            WAVE_FORMAT_G723_ADPCM,

            WAVE_FORMAT_DIGISTD,

            WAVE_FORMAT_DIGIFIX,

            WAVE_FORMAT_DIALOGIC_OKI_ADPCM,

            WAVE_FORMAT_MEDIAVISION_ADPCM,

            WAVE_FORMAT_CU_CODEC,

            WAVE_FORMAT_YAMAHA_ADPCM = 32,

            WAVE_FORMAT_SONARC,

            WAVE_FORMAT_DSPGROUP_TRUESPEECH,

            WAVE_FORMAT_ECHOSC1,

            WAVE_FORMAT_AUDIOFILE_AF36,

            WAVE_FORMAT_APTX,

            WAVE_FORMAT_AUDIOFILE_AF10,

            WAVE_FORMAT_PROSODY_1612,

            WAVE_FORMAT_LRC,

            WAVE_FORMAT_DOLBY_AC2 = 48,

            WAVE_FORMAT_GSM610,

            WAVE_FORMAT_MSNAUDIO,

            WAVE_FORMAT_ANTEX_ADPCME,

            WAVE_FORMAT_CONTROL_RES_VQLPC,

            WAVE_FORMAT_DIGIREAL,

            WAVE_FORMAT_DIGIADPCM,

            WAVE_FORMAT_CONTROL_RES_CR10,

            WAVE_FORMAT_NMS_VBXADPCM,

            WAVE_FORMAT_ROLAND_RDAC,

            WAVE_FORMAT_ECHOSC3,

            WAVE_FORMAT_ROCKWELL_ADPCM,

            WAVE_FORMAT_ROCKWELL_DIGITALK,

            WAVE_FORMAT_XEBEC,

            WAVE_FORMAT_G721_ADPCM = 64,

            WAVE_FORMAT_G728_CELP,

            WAVE_FORMAT_MSG723,

            WAVE_FORMAT_MPEG = 80,

            WAVE_FORMAT_RT24 = 82,

            WAVE_FORMAT_PAC,

            WAVE_FORMAT_MPEGLAYER3 = 85,

            WAVE_FORMAT_LUCENT_G723 = 89,

            WAVE_FORMAT_CIRRUS = 96,

            WAVE_FORMAT_ESPCM,

            WAVE_FORMAT_VOXWARE,

            WAVE_FORMAT_CANOPUS_ATRAC,

            WAVE_FORMAT_G726_ADPCM,

            WAVE_FORMAT_G722_ADPCM,

            WAVE_FORMAT_DSAT,

            WAVE_FORMAT_DSAT_DISPLAY,

            WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 105,

            WAVE_FORMAT_VOXWARE_AC8 = 112,

            WAVE_FORMAT_VOXWARE_AC10,

            WAVE_FORMAT_VOXWARE_AC16,

            WAVE_FORMAT_VOXWARE_AC20,

            WAVE_FORMAT_VOXWARE_RT24,

            WAVE_FORMAT_VOXWARE_RT29,

            WAVE_FORMAT_VOXWARE_RT29HW,

            WAVE_FORMAT_VOXWARE_VR12,

            WAVE_FORMAT_VOXWARE_VR18,

            WAVE_FORMAT_VOXWARE_TQ40,

            WAVE_FORMAT_SOFTSOUND = 128,

            WAVE_FORMAT_VOXWARE_TQ60,

            WAVE_FORMAT_MSRT24,

            WAVE_FORMAT_G729A,

            WAVE_FORMAT_MVI_MV12,

            WAVE_FORMAT_DF_G726,

            WAVE_FORMAT_DF_GSM610,

            WAVE_FORMAT_ISIAUDIO = 136,

            WAVE_FORMAT_ONLIVE,

            WAVE_FORMAT_SBC24 = 145,

            WAVE_FORMAT_DOLBY_AC3_SPDIF,

            WAVE_FORMAT_ZYXEL_ADPCM = 151,

            WAVE_FORMAT_PHILIPS_LPCBB,

            WAVE_FORMAT_PACKED,

            WAVE_FORMAT_RHETOREX_ADPCM = 256,

            WAVE_FORMAT_IRAT,

            WAVE_FORMAT_VIVO_G723 = 273,

            WAVE_FORMAT_VIVO_SIREN,

            WAVE_FORMAT_DIGITAL_G723 = 291,

            WAVE_FORMAT_CREATIVE_ADPCM = 512,

            WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 514,

            WAVE_FORMAT_CREATIVE_FASTSPEECH10,

            WAVE_FORMAT_QUARTERDECK = 544,

            WAVE_FORMAT_FM_TOWNS_SND = 768,

            WAVE_FORMAT_BTV_DIGITAL = 1024,

            WAVE_FORMAT_VME_VMPCM = 1664,

            WAVE_FORMAT_OLIGSM = 4096,

            WAVE_FORMAT_OLIADPCM,

            WAVE_FORMAT_OLICELP,

            WAVE_FORMAT_OLISBC,

            WAVE_FORMAT_OLIOPR,

            WAVE_FORMAT_LH_CODEC = 4352,

            WAVE_FORMAT_NORRIS = 5120,

            WAVE_FORMAT_ISIAUDIO_2,

            WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 5376,

            WAVE_FORMAT_DVM = 8192,

            WAVE_FORMAT_EXTENSIBLE = 65534
        }
    }
}
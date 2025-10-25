using System;
namespace SoundEditorPlugin.Resources
{
	internal static class ArrayConverter
	{
		public static byte[] GetBytes(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Reverse(bytes);
			return bytes;
		}
		public static bool ToBoolean(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToBoolean(value, 0);
		}
		public static int ToInt32(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToInt32(value, 0);
		}
		public static uint ToUInt32(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToUInt32(value, 0);
		}
		public static long ToInt64(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToInt64(value, 0);
		}
		public static ulong ToUInt64(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToUInt64(value, 0);
		}
		public static float ToSingle(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToSingle(value, 0);
		}
		public static double ToDouble(byte[] value)
		{
			Array.Reverse(value);
			return BitConverter.ToDouble(value, 0);
		}
	}
}
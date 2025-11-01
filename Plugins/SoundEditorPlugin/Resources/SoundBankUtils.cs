using System;
using System.Collections.Generic;
using System.Linq;
using FrostySdk.IO;

namespace SoundEditorPlugin.Resources
{

    public static class SoundBankUtils
    {
        public static long GetLowest(this List<long> list)
        {
            return list.ToArray().GetLowest();
        }

        public static long GetLowest(this long[] array)
        {
            if (array.Length < 1)
            {
                throw new Exception("array can't be empty");
            }
            long num = array[0];
            foreach (long num2 in array)
            {
                if (num > num2)
                {
                    num = num2;
                }
            }
            return num;
        }

        public static byte GetBiggestSize(this long[] array)
        {
            // Corrected logic to determine the smallest required storage size (1, 2, 4, or 8 bytes)
            byte b = 1;
            foreach (long num in array)
            {
                if (num > 255L)
                {
                    if (b < 2)
                    {
                        b = 2;
                    }
                    if (num > 65535L)
                    {
                        if (b < 4)
                        {
                            b = 4;
                        }
                        // Check if it exceeds max 32-bit unsigned value
                        if (num > (long)uint.MaxValue)
                        {
                            return 8;
                        }
                    }
                }
            }
            return b;
        }

        public static byte GetShift(this long[] array, out long[] outArray)
        {
            byte biggestSize = array.GetBiggestSize();
            Dictionary<int, List<long>> dictionary = new Dictionary<int, List<long>>();

            // Iterate through possible shifts (0-255)
            foreach (long num in array)
            {
                for (int j = 0; j <= 255; j++)
                {
                    // Check if dividing by 2^j results in a whole number
                    double num2 = (double)num / Math.Pow(2.0, (double)j);
                    if (num2 % 1.0 == 0.0)
                    {
                        if (!dictionary.ContainsKey(j))
                        {
                            dictionary.Add(j, new List<long>());
                        }
                        dictionary[j].Add(Convert.ToInt64(num2));
                    }
                }
            }

            List<long> list = new List<long>(255);

            // Find shifts where ALL numbers are perfectly divisible
            foreach (KeyValuePair<int, List<long>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value.Count == array.Length)
                {
                    list.Add((long)keyValuePair.Key);
                }
            }

            // Get the smallest possible shift (i.e., the greatest common power of 2 factor)
            if (list.Count == 0)
            {
                outArray = array;
                return 0;
            }

            byte b = (byte)list.GetLowest();

            // Check if the shifted array fits into a smaller storage size than the original
            if (dictionary[(int)b].ToArray().GetBiggestSize() < biggestSize)
            {
                outArray = dictionary[(int)b].ToArray();
                return b;
            }

            outArray = array;
            return 0;
        }

        public static byte[] ConvertToBytes(this long[] array, Endian endian)
        {
            byte biggestSize = array.GetBiggestSize();
            byte[] array2 = new byte[(int)biggestSize * array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(array[i]);
                for (int j = 0; j < (int)biggestSize; j++)
                {
                    if (endian == Endian.Little)
                    {
                        // Little Endian: copy bytes directly (0, 1, 2, 3...)
                        array2[i * (int)biggestSize + j] = bytes[j];
                    }
                    else
                    {
                        // Big Endian: reverse copy bytes (3, 2, 1, 0...)
                        array2[i * (int)biggestSize + j] = bytes[(int)(biggestSize - 1) - j];
                    }
                }
            }
            return array2;
        }

        // Utility method used by NewWaveResource for bit width calculation
        public static int GetUnsignedWidth(this long value)
        {
            if (value <= 255L)
            {
                return 1;
            }
            if (value <= 65535L)
            {
                return 2;
            }
            if (value <= (long)uint.MaxValue)
            {
                return 4;
            }
            return 8;
        }
    }
}

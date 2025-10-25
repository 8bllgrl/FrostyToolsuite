using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Added System.Linq for Array.ToArray() or IEnumerable extension methods
using FrostySdk.IO;

namespace SoundEditorPlugin.Resources
{

    public static class SoundBankUtils
    {
        // Fix: Changed list.ToArray() to list.ToArray() which is redundant if using System.Linq, 
        // but safe since GetLowest accepts long[].
        public static long GetLowest(this List<long> list)
        {
            return list.ToArray().GetLowest();
        }

        public static long GetLowest(this long[] array)
        {
            bool flag = array.Length < 1;
            if (flag)
            {
                throw new Exception("array can't be empty");
            }
            long num = array[0];
            foreach (long num2 in array)
            {
                bool flag2 = num > num2;
                if (flag2)
                {
                    num = num2;
                }
            }
            return num;
        }

        public static byte GetBiggestSize(this long[] array)
        {
            byte b = 0;
            foreach (long num in array)
            {
                byte[] bytes = BitConverter.GetBytes(num);
                Array.Reverse(bytes);
                byte b2 = 0;
                foreach (byte b3 in bytes)
                {
                    bool flag = b2 > 0;
                    if (flag)
                    {
                        break;
                    }
                    // FIX: Explicitly cast the result of b2 + 1 back to byte.
                    b2 = (byte)(b2 + 1);
                }
                // FIX: Explicitly cast the result of 8 - b2 back to byte.
                b2 = (byte)(8 - b2);
                bool flag2 = b2 > b;
                if (flag2)
                {
                    b = b2;
                }
            }
            bool flag3 = b > 4;
            if (flag3)
            {
                b = 8;
            }
            else
            {
                bool flag4 = b > 2;
                if (flag4)
                {
                    b = 4;
                }
                else
                {
                    bool flag5 = b > 1;
                    if (flag5)
                    {
                        b = 2;
                    }
                }
            }
            return b;
        }

        public static byte GetShift(this long[] array, out long[] outArray)
        {
            byte biggestSize = array.GetBiggestSize();
            Dictionary<int, List<long>> dictionary = new Dictionary<int, List<long>>();
            foreach (long num in array)
            {
                for (int j = 0; j <= 255; j++)
                {
                    double num2 = (double)num / Math.Pow(2.0, (double)j);
                    bool flag = num2 % 1.0 == 0.0;
                    if (flag)
                    {
                        bool flag2 = !dictionary.ContainsKey(j);
                        if (flag2)
                        {
                            dictionary.Add(j, new List<long>());
                        }
                        dictionary[j].Add(Convert.ToInt64(num2));
                    }
                }
            }
            List<long> list = new List<long>(255);
            foreach (KeyValuePair<int, List<long>> keyValuePair in dictionary)
            {
                bool flag3 = keyValuePair.Value.Count != array.Length;
                if (!flag3)
                {
                    list.Add((long)keyValuePair.Key);
                }
            }
            byte b = (byte)list.GetLowest();
            bool flag4 = dictionary[(int)b].ToArray().GetBiggestSize() < biggestSize;
            byte b2;
            if (flag4)
            {
                outArray = dictionary[(int)b].ToArray();
                b2 = b;
            }
            else
            {
                outArray = array;
                b2 = 0;
            }
            return b2;
        }

        public static byte[] ConvertToBytes(this long[] array, Endian endian)
        {
            byte biggestSize = array.GetBiggestSize();
            byte[] array2 = new byte[(int)biggestSize * array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(array[i]);
                int num = 0;
                while (i < (int)biggestSize)
                {
                    bool flag = endian == 0;
                    if (flag)
                    {
                        array2[i * (int)biggestSize + num] = bytes[num];
                    }
                    else
                    {
                        array2[i * (int)biggestSize + num] = bytes[(int)(biggestSize - 1) - num];
                    }
                    num++;
                }
            }
            return array2;
        }
    }
}

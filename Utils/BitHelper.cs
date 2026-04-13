using System;
using System.Collections.Generic;

namespace VideoFileStorage.Utils
{
    public static class BitHelper
    {
        /// <summary>
        /// Converts a byte array into a boolean array (binary representation).
        /// </summary>
        public static bool[] ToBitArray(byte[] data)
        {
            bool[] bits = new bool[data.Length * 8];
            for (int i = 0; i < data.Length; i++)
            {
                for (int b = 0; b < 8; b++)
                {
                    // Extract bits from Most Significant Bit to Least Significant Bit
                    bits[i * 8 + b] = (data[i] & (1 << (7 - b))) != 0;
                }
            }
            return bits;
        }

        /// <summary>
        /// Converts a list of booleans back into a byte array.
        /// </summary>
        public static byte[] ToByteArray(List<bool> bits)
        {
            int byteCount = (int)Math.Ceiling((double)bits.Count / 8);
            byte[] bytes = new byte[byteCount];

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));
                }
            }
            return bytes;
        }
    }
}

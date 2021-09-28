using System;
using System.Diagnostics;
using UnityEngine;

namespace PSD2UGUI
{
    public static class Util
    {
        [DebuggerDisplay("Top = {Top}, Bottom = {Bottom}, Left = {Left}, Right = {Right}")]
        public struct RectanglePosition
        {
            public int Top { get; set; }
            public int Bottom { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
        }

        /////////////////////////////////////////////////////////////////////////// 

        /// <summary>
        /// Fills a buffer with a byte value.
        /// </summary>
        static public byte[] Fill(this byte[] byteArray, int startIndex, int count, byte value)
        {
            for (int i = 0; i < count; ++i)
            {
                byteArray[startIndex + i] = value;
            }
            return byteArray;
        }

        /////////////////////////////////////////////////////////////////////////// 

        /// <summary>
        /// Reverses the endianness of a 2-byte word.
        /// </summary>
        public static void SwapBytes2(this byte[] byteArray, int startIndex = 0)
        {
            byte byte0 = byteArray[startIndex];
            byteArray[startIndex] = byteArray[startIndex + 1];
            byteArray[startIndex + 1] = byte0;
        }

        /////////////////////////////////////////////////////////////////////////// 

        /// <summary>
        /// Reverses the endianness of a 4-byte word.
        /// </summary>
        public static void SwapBytes4(this byte[] byteArray, int startIndex = 0)
        {
            byte byte0 = byteArray[startIndex];
            byte byte1 = byteArray[startIndex + 1];
            byteArray[startIndex] = byteArray[startIndex + 3];
            byteArray[startIndex + 1] = byteArray[startIndex + 2];
            byteArray[startIndex + 2] = byte1;
            byteArray[startIndex + 3] = byte0;
        }

        /// <summary>
        /// Reverses the endianness of a word of arbitrary length.
        /// </summary>
        static public void SwapBytes(this byte[] byteArray, int startIndex, int nLength)
        {
            for (long i = 0; i < nLength / 2; ++i)
            {
                byte t = byteArray[startIndex + i];
                byteArray[startIndex + i] = byteArray[nLength - i - 1];
                byteArray[nLength - i - 1] = t;
            }
        }
        /////////////////////////////////////////////////////////////////////////// 

        /// <summary>
        /// Reverses the endianness of 2-byte words in a byte array.
        /// </summary>
        /// <param name="byteArray">Byte array containing the sequence on which to swap endianness</param>
        /// <param name="startIdx">Byte index of the first word to swap</param>
        /// <param name="count">Number of words to swap</param>
        public static void SwapByteArray2(byte[] byteArray, int startIdx, int count)
        {
            int endIdx = startIdx + count * 2;
            if (byteArray.Length < endIdx)
                throw new IndexOutOfRangeException();
            int idx = startIdx;
            while (idx < endIdx)
            {
                byteArray.SwapBytes2(idx);
                idx += 2;
            }
        }

        /// <summary>
        /// Reverses the endianness of 4-byte words in a byte array.
        /// </summary>
        /// <param name="byteArray">Byte array containing the sequence on which to swap endianness</param>
        /// <param name="startIdx">Byte index of the first word to swap</param>
        /// <param name="count">Number of words to swap</param>
        public static void SwapByteArray4(byte[] byteArray, int startIdx, int count)
        {
            int endIdx = startIdx + count * 4;
            if (byteArray.Length < endIdx)
                throw new IndexOutOfRangeException();

            int idx = startIdx;
            while (idx < endIdx)
            {
                byteArray.SwapBytes4(idx);
                idx += 4;
            }
        }

        /////////////////////////////////////////////////////////////////////////// 

        public static int BytesPerRow(Rect rect, int depth)
        {
            switch (depth)
            {
                case 1:
                    return ((int)rect.width + 7) / 8;
                default:
                    return (int)rect.width * BytesFromBitDepth(depth);
            }
        }

        /// <summary>
        /// Round the integer to a multiple.
        /// </summary>
        public static int RoundUp(int value, int multiple)
        {
            if (value == 0)
                return 0;

            if (Math.Sign(value) != Math.Sign(multiple))
                throw new ArgumentException("value and multiple cannot have opposite signs.");

            var remainder = value % multiple;
            if (remainder > 0)
            {
                value += (multiple - remainder);
            }
            return value;
        }

        /// <summary>
        /// Get number of bytes required to pad to the specified multiple.
        /// </summary>
        public static int GetPadding(int length, int padMultiple)
        {
            if ((length < 0) || (padMultiple < 0))
                throw new ArgumentException();

            var remainder = length % padMultiple;
            if (remainder == 0)
                return 0;

            var padding = padMultiple - remainder;
            return padding;
        }

        public static int BytesFromBitDepth(int depth)
        {
            switch (depth)
            {
                case 1:
                case 8:
                    return 1;
                case 16:
                    return 2;
                case 32:
                    return 4;
                default:
                    throw new ArgumentException("Invalid bit depth.");
            }
        }

        public static short MinChannelCount(this PsdColorMode colorMode)
        {
            switch (colorMode)
            {
                case PsdColorMode.Bitmap:
                case PsdColorMode.Duotone:
                case PsdColorMode.Grayscale:
                case PsdColorMode.Indexed:
                case PsdColorMode.Multichannel:
                    return 1;
                case PsdColorMode.Lab:
                case PsdColorMode.RGB:
                    return 3;
                case PsdColorMode.CMYK:
                    return 4;
            }

            throw new ArgumentException("Unknown color mode.");
        }

        /// <summary>
        /// Verify that the offset and count will remain within the bounds of the
        /// buffer.
        /// </summary>
        /// <returns>True if in bounds, false if out of bounds.</returns>
        public static bool CheckBufferBounds(byte[] data, int offset, int count)
        {
            if (offset < 0)
                return false;
            if (count < 0)
                return false;
            if (offset + count > data.Length)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Fixed-point decimal, with 16-bit integer and 16-bit fraction.
    /// </summary>
    public class UFixed16_16
    {
        public UInt16 Integer { get; set; }
        public UInt16 Fraction { get; set; }

        public UFixed16_16(UInt16 integer, UInt16 fraction)
        {
            Integer = integer;
            Fraction = fraction;
        }

        /// <summary>
        /// Split the high and low words of a 32-bit unsigned integer into a
        /// fixed-point number.
        /// </summary>
        public UFixed16_16(UInt32 value)
        {
            Integer = (UInt16)(value >> 16);
            Fraction = (UInt16)(value & 0x0000ffff);
        }

        public UFixed16_16(double value)
        {
            if (value >= 65536.0) throw new OverflowException();
            if (value < 0) throw new OverflowException();

            Integer = (UInt16)value;

            // Round instead of truncate, because doubles may not represent the
            // fraction exactly.
            Fraction = (UInt16)((value - Integer) * 65536 + 0.5);
        }

        public static implicit operator double(UFixed16_16 value)
        {
            return (double)value.Integer + value.Fraction / 65536.0;
        }

    }

}

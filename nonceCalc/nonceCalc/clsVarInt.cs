using System;
using System.Collections.Generic;
using System.Text;

namespace nonceCalc
{
    /// <summary>
    /// Provides varInt conversions
    /// </summary>
    public static class VarInt
    {
        /// <summary>
        /// Gets the length in bytes of the varInt
        /// </summary>
        /// <param name="buffer">array to check</param>
        /// <returns>Length (1-4)</returns>
        public static int getLength(byte[] buffer)
        {
            switch (buffer[0])
            {
                case 255:
                    return 8;
                case 254:
                    return 4;
                case 253:
                    return 2;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// reads a varInt
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <returns>varInt</returns>
        public static ulong getInt(byte[] buffer)
        {
            switch (buffer[0])
            {
                case 253:
                    return BitConverter.ToUInt16(buffer, 1);
                case 254:
                    return BitConverter.ToUInt32(buffer, 1);
                case 255:
                    return BitConverter.ToUInt64(buffer, 1);
                default:
                    return buffer[0];
            }
        }

        /// <summary>
        /// gets a byte array for a varInt
        /// </summary>
        /// <param name="number">number to convert</param>
        /// <returns>byte array</returns>
        public static byte[] getByte(ulong number)
        {
            if (number < 253)
            {
                return new byte[] { (byte)number };
            }
            else if (number >= 253 && number <= ushort.MaxValue)
            {
                byte[] temp = BitConverter.GetBytes((ushort)number);
                return new byte[] {
                    253,
                    temp[0],
                    temp[1]
                };
            }
            else if (number > ushort.MaxValue && number <=uint.MaxValue)
            {
                byte[] temp = BitConverter.GetBytes((uint)number);
                return new byte[] {
                    254,
                    temp[0],
                    temp[1],
                    temp[2],
                    temp[3]
                };
            }
            else
            {
                byte[] temp = BitConverter.GetBytes(number);
                return new byte[] {
                    255,
                    temp[0],
                    temp[1],
                    temp[2],
                    temp[3],
                    temp[4],
                    temp[5],
                    temp[6],
                    temp[7],
                };
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;


namespace PSD2UGUI
{
    public class RleReader
    {
        private Stream stream;

        public RleReader(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Decodes a PackBits RLE stream.
        /// </summary>
        /// <param name="buffer">Output buffer for decoded data.</param>
        /// <param name="offset">Offset at which to begin writing.</param>
        /// <param name="count">Number of bytes to decode from the stream.</param>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (!Util.CheckBufferBounds(buffer, offset, count))
                throw new ArgumentOutOfRangeException();

            int bytesLeft = count;
            int bufferIdx = offset;
            while (bytesLeft > 0)
            {
                // ReadByte returns an unsigned byte, but we want a signed byte.
                var flagCounter = unchecked((sbyte)stream.ReadByte());
                // Raw packet
                if (flagCounter > 0)
                {
                    var readLength = flagCounter + 1;
                    if (bytesLeft < readLength)
                        throw new RleException("Raw packet overruns the decode window.");

                    stream.Read(buffer, bufferIdx, readLength);

                    bufferIdx += readLength;
                    bytesLeft -= readLength;
                }
                // RLE packet
                else if (flagCounter > -128)
                {
                    var runLength = 1 - flagCounter;
                    var byteValue = (byte)stream.ReadByte();
                    if (runLength > bytesLeft)
                        throw new RleException("RLE packet overruns the decode window.");

                    int idx = bufferIdx;
                    int end = idx + runLength;
                    while (idx < end)
                    {
                        buffer[idx] = byteValue;
                        ++idx;
                    }
                    bufferIdx += runLength;
                    bytesLeft -= runLength;
                }
                else
                {
                    // The canonical PackBits algorithm will never emit 0x80 (-128), but
                    // some programs do.  Simply skip over the byte.
                }
            }
            Debug.Assert(bytesLeft == 0);
            return count - bytesLeft;
        }
    }
}

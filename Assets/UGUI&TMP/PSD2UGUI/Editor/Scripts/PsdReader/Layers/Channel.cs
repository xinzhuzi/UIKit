using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace PSD2UGUI
{
    public class ChannelList : List<Channel>
    {
        /// <summary>
        /// Returns channels with nonnegative IDs as an array, so that accessing
        /// a channel by Id can be optimized into pointer arithmetic rather than
        /// being implemented as a List scan.
        /// </summary>
        /// <remarks>
        /// This optimization is crucial for blitting lots of pixels back and
        /// forth between Photoshop's per-channel representation, and Paint.NET's
        /// per-pixel BGRA representation.
        /// </remarks>
        public Channel[] ToIdArray()
        {
            var maxId = this.Max(x => x.ID);
            var idArray = new Channel[maxId + 1];
            foreach (var channel in this)
            {
                if (channel.ID >= 0)
                    idArray[channel.ID] = channel;
            }
            return idArray;
        }

        public ChannelList()
          : base()
        {
        }

        public Channel GetId(int id)
        {
            return this.Single(x => x.ID == id);
        }

        public bool ContainsId(int id)
        {
            return this.Exists(x => x.ID == id);
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    [DebuggerDisplay("ID = {ID}")]
    public class Channel
    {
        private bool _cancel = false;
        public bool Cancel 
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        /// <summary>
        /// The layer to which this channel belongs
        /// </summary>
        public Layer Layer { get; private set; }

        /// <summary>
        /// Channel ID.
        /// <list type="bullet">
        /// <item>-1 = transparency mask</item>
        /// <item>-2 = user-supplied layer mask, or vector mask</item>
        /// <item>-3 = user-supplied layer mask, if channel -2 contains a vector mask</item>
        /// <item>
        /// Nonnegative channel IDs give the actual image channels, in the
        /// order defined by the colormode.  For example, 0, 1, 2 = R, G, B.
        /// </item>
        /// </list>
        /// </summary>
        public short ID { get; set; }

        public Rect Rect
        {
            get
            {
                switch (ID)
                {
                    case -2:
                        return Layer.Masks.LayerMask.Rect;
                    case -3:
                        return Layer.Masks.UserMask.Rect;
                    default:
                        return Layer.Rect;
                }
            }
        }

        /// <summary>
        /// Total length of the channel data, including compression headers.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Raw image data for this color channel, in compressed on-disk format.
        /// </summary>
        /// <remarks>
        /// If null, the ImageData will be automatically compressed during save.
        /// </remarks>
        public byte[] ImageDataRaw { get; set; }

        /// <summary>
        /// Decompressed image data for this color channel.
        /// </summary>
        /// <remarks>
        /// When making changes to the ImageData, set ImageDataRaw to null so that
        /// the correct data will be compressed during save.
        /// </remarks>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Image compression method used.
        /// </summary>
        public ImageCompression ImageCompression { get; set; }

        /// <summary>
        /// RLE-compressed length of each row.
        /// </summary>
        public RleRowLengths RleRowLengths { get; set; }
        

        //////////////////////////////////////////////////////////////////

        internal Channel(short id, Layer layer)
        {
            ID = id;
            Layer = layer;
        }

        internal Channel(PsdBinaryReader reader, Layer layer)
        {
            ID = reader.ReadInt16();
            Length = reader.ReadInt32();
            Layer = layer;
        }
        //////////////////////////////////////////////////////////////////

        internal void LoadPixelData(PsdBinaryReader reader)
        {
            var endPosition = reader.BaseStream.Position + this.Length;
            ImageCompression = (ImageCompression)reader.ReadInt16();
            var dataLength = this.Length - 2;

            switch (ImageCompression)
            {
                case ImageCompression.Raw:
                    ImageDataRaw = reader.ReadBytes(dataLength);
                    break;
                case ImageCompression.Rle:
                    // RLE row lengths
                    RleRowLengths = new RleRowLengths(reader, (int)Rect.height);
                    var rleDataLength = (int)(endPosition - reader.BaseStream.Position);

                    // The PSD specification states that rows are padded to even sizes.
                    // However, Photoshop doesn't actually do this.  RLE rows can have
                    // odd lengths in the header, and there is no padding between rows.
                    ImageDataRaw = reader.ReadBytes(rleDataLength);
                    break;
                case ImageCompression.Zip:
                case ImageCompression.ZipPrediction:
                    ImageDataRaw = reader.ReadBytes(dataLength);
                    break;
            }

        }

        /// <summary>
        /// Decodes the raw image data from the compressed on-disk format into
        /// an uncompressed bitmap, in native byte order.
        /// </summary>
        public void DecodeImageData()
        {
            if (this.ImageCompression == ImageCompression.Raw)
                ImageData = ImageDataRaw;
            else
                DecompressImageData();

            // Rearrange the decompressed bytes into words, with native byte order.
            if (ImageCompression == ImageCompression.ZipPrediction)
                UnpredictImageData(Rect);
            else
                ReverseEndianness(ImageData, Rect);
        }

        private void DecompressImageData()
        {
            using (var stream = new MemoryStream(ImageDataRaw))
            {
                var bytesPerRow = Util.BytesPerRow(Rect, Layer.PsdFile.BitDepth);
                var bytesTotal = (int)Rect.height * bytesPerRow;
                ImageData = new byte[bytesTotal];

                switch (this.ImageCompression)
                {
                    case ImageCompression.Rle:
                        var rleReader = new RleReader(stream);
                        for (int i = 0; i < Rect.height; i++)
                        {
                            int rowIndex = i * bytesPerRow;
                            rleReader.Read(ImageData, rowIndex, bytesPerRow);
                        }
                        break;

                    case ImageCompression.Zip:
                    case ImageCompression.ZipPrediction:

                        // .NET implements Deflate (RFC 1951) but not zlib (RFC 1950),
                        // so we have to skip the first two bytes.
                        stream.ReadByte();
                        stream.ReadByte();

                        var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                        var bytesDecompressed = deflateStream.Read(ImageData, 0, bytesTotal);
                        break;

                    default:
                        throw new PsdInvalidException("Unknown image compression method.");
                }
            }
        }

        private void ReverseEndianness(byte[] buffer, Rect rect)
        {
            var byteDepth = Util.BytesFromBitDepth(Layer.PsdFile.BitDepth);
            int pixelsTotal = (int)(rect.width * rect.height);
            if (pixelsTotal == 0)
                return;

            if (byteDepth == 2)
            {
                Util.SwapByteArray2(buffer, 0, pixelsTotal);
            }
            else if (byteDepth == 4)
            {
                Util.SwapByteArray4(buffer, 0, pixelsTotal);
            }
            else if (byteDepth > 1)
            {
                throw new NotImplementedException("Byte-swapping implemented only for 16-bit and 32-bit depths.");
            }
        }

        /// <summary>
        /// Unpredicts the raw decompressed image data into a little-endian
        /// scanline bitmap.
        /// </summary>

        private void UnpredictImageData(Rect rect)
        {
            if (Layer.PsdFile.BitDepth == 16)
            {

                // 16-bitdepth images are delta-encoded word-by-word.  The deltas
                // are thus big-endian and must be reversed for further processing.
                ReverseEndianness(ImageData, rect);
                // Delta-decode each row
                for (int iRow = 0; iRow < rect.height; ++iRow)
                {
                    var idx = (iRow * (int)(rect.width + 1) * 2);
                    var end = (iRow + 1) * (int)rect.width * 2;
                    // Start with column index 1 on each row
                    while (idx < end)
                    {
                        byte[] decodedBytes = BitConverter.GetBytes(BitConverter.ToInt16(ImageData, idx) + BitConverter.ToInt16(ImageData, idx - 2));
                        ImageData[idx] = decodedBytes[0];
                        ImageData[idx + 1] = decodedBytes[1];
                        idx += 2;
                    }
                }
            }
            else if (Layer.PsdFile.BitDepth == 32)
            {
                var reorderedData = new byte[ImageData.Length];
                for (int iRow = 0; iRow < rect.height; ++iRow)
                {
                    var idx = (iRow * (int)(rect.width + 1) * 4);
                    var end = (iRow + 1) * (int)rect.width * 4;
                    // Start with column index 1 on each row
                    while (idx < end)
                    {
                        byte[] decodedBytes = BitConverter.GetBytes(BitConverter.ToInt32(ImageData, idx) + BitConverter.ToInt32(ImageData, idx - 4));
                        for (int byteIdx = 0; byteIdx < 4; ++byteIdx)
                        {
                            ImageData[idx + byteIdx] = decodedBytes[byteIdx];
                        }
                        idx += 4;
                    }
                }

                // Within each row, the individual bytes of the 32-bit words are
                // packed together, high-order bytes before low-order bytes.
                // We now unpack them into words and reverse to little-endian.
                int[] offset = { (int)rect.width, (int)rect.width * 2, (int)rect.width * 3 };
                for (int iRow = 0; iRow < rect.height; ++iRow)
                {
                    var idx = (iRow * (int)(rect.width + 1) * 4);
                    var end = (iRow + 1) * (int)rect.width * 4;

                    // Reverse to little-endian as we do the unpacking.
                    while (idx < end)
                    {
                        for (int byteIdx = 0; byteIdx < 4; ++byteIdx)
                        {
                            reorderedData[idx + byteIdx] = ImageData[idx + offset[3 - byteIdx]];
                        }
                        idx += 4;
                    }
                }
                ImageData = reorderedData;
            }
            else
            {
                throw new PsdInvalidException("ZIP with prediction is only available for 16 and 32 bit depths.");
            }
        }
    }
}
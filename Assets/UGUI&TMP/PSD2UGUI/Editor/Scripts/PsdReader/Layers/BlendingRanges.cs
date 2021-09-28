using System.Diagnostics;
using System.Globalization;

namespace PSD2UGUI
{
    public class BlendingRanges
    {
        /// <summary>
        /// The layer to which this channel belongs
        /// </summary>
        public Layer Layer { get; private set; }

        public byte[] Data { get; set; }

        ///////////////////////////////////////////////////////////////////////////

        public BlendingRanges(Layer layer)
        {
            Layer = layer;
            Data = new byte[0];
        }

        ///////////////////////////////////////////////////////////////////////////

        public BlendingRanges(PsdBinaryReader reader, Layer layer)
        {
            Debug.WriteLine("BlendingRanges started at " + reader.BaseStream.Position.ToString(CultureInfo.InvariantCulture));

            Layer = layer;
            var dataLength = reader.ReadInt32();
            if (dataLength <= 0)
                return;

            Data = reader.ReadBytes(dataLength);
        }
    }
}

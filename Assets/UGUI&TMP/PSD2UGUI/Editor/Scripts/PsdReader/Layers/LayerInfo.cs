using System.Diagnostics;

namespace PSD2UGUI
{
    public static class LayerInfoFactory
    {
        public static LayerInfo Load(PsdBinaryReader reader)
        {
            Debug.WriteLine("LayerInfoFactory.Load started at " + reader.BaseStream.Position);

            var signature = reader.ReadAsciiChars(4);
            if (signature != "8BIM")
                throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch.");

            var key = reader.ReadAsciiChars(4);
            var length = reader.ReadInt32();
            var startPosition = reader.BaseStream.Position;

            LayerInfo result;
            switch (key)
            {
                case "lsct":
                case "lsdk":
                    result = new LayerSectionInfo(reader, key, length);
                    break;
                case "luni":
                    result = new LayerUnicodeName(reader);
                    break;
                /*case "TySh":
                    result = new TextInfo(reader, key, length);
                    break;*/
                default:
                    result = new RawLayerInfo(reader, key, length);
                    break;
            }

            // May have additional padding applied.
            var endPosition = startPosition + length;
            if (reader.BaseStream.Position < endPosition)
                reader.BaseStream.Position = endPosition;

            // Documentation states that the length is even-padded.  Actually:
            //   1. Most keys have 4-padded lengths.
            //   2. However, some keys (LMsk) have even-padded lengths.
            //   3. Other keys (Txt2, Lr16, Lr32) have unpadded lengths.
            //
            // The data is always 4-padded, regardless of the stated length.

            reader.ReadPadding(startPosition, 4);

            return result;
        }
    }

    public abstract class LayerInfo
    {
        public abstract string Key { get; }
    }
}

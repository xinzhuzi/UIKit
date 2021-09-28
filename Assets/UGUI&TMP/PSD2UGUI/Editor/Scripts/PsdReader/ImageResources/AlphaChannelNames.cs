using System;
using System.Collections.Generic;

namespace PSD2UGUI
{
    /// <summary>
    /// The names of the alpha channels
    /// </summary>
    public class AlphaChannelNames : ImageResource
    {
        public override ResourceID ID
        {
            get { return ResourceID.AlphaChannelNames; }
        }

        private List<string> channelNames = new List<string>();
        public List<string> ChannelNames
        {
            get { return channelNames; }
        }

        public AlphaChannelNames() : base(String.Empty)
        {
        }

        public AlphaChannelNames(PsdBinaryReader reader, string name, int resourceDataLength)
          : base(name)
        {
            var endPosition = reader.BaseStream.Position + resourceDataLength;

            // Alpha channel names are Pascal strings, with no padding in-between.
            while (reader.BaseStream.Position < endPosition)
            {
                var channelName = reader.ReadPascalString(1);
                ChannelNames.Add(channelName);
            }
        }
    }
}

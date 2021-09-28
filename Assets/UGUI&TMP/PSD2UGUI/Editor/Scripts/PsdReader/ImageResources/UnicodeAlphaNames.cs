using System;
using System.Collections.Generic;

namespace PSD2UGUI
{
    /// <summary>
    /// The names of the alpha channels.
    /// </summary>
    public class UnicodeAlphaNames : ImageResource
    {
        public override ResourceID ID
        {
            get { return ResourceID.UnicodeAlphaNames; }
        }

        private List<string> channelNames = new List<string>();
        public List<string> ChannelNames
        {
            get { return channelNames; }
        }

        public UnicodeAlphaNames()
          : base(String.Empty)
        {
        }

        public UnicodeAlphaNames(PsdBinaryReader reader, string name, int resourceDataLength)
          : base(name)
        {
            var endPosition = reader.BaseStream.Position + resourceDataLength;

            while (reader.BaseStream.Position < endPosition)
            {
                var channelName = reader.ReadUnicodeString();
                ChannelNames.Add(channelName);
            }
        }
    }
}

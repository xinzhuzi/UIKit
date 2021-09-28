using System;

namespace PSD2UGUI
{
    public class VersionInfo : ImageResource
    {
        public override ResourceID ID
        {
            get { return ResourceID.VersionInfo; }
        }

        public UInt32 Version { get; set; }

        public bool HasRealMergedData { get; set; }

        public string ReaderName { get; set; }

        public string WriterName { get; set; }

        public UInt32 FileVersion { get; set; }


        public VersionInfo() : base(String.Empty)
        {
        }

        public VersionInfo(PsdBinaryReader reader, string name)
          : base(name)
        {
            Version = reader.ReadUInt32();
            HasRealMergedData = reader.ReadBoolean();
            ReaderName = reader.ReadUnicodeString();
            WriterName = reader.ReadUnicodeString();
            FileVersion = reader.ReadUInt32();
        }
    }
}

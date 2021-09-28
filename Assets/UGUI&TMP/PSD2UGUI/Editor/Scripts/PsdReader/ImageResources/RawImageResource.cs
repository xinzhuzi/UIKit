namespace PSD2UGUI
{
    /// <summary>
    /// Stores the raw data for unimplemented image resource types.
    /// </summary>
    public class RawImageResource : ImageResource
    {
        public byte[] Data { get; private set; }

        private ResourceID id;
        public override ResourceID ID
        {
            get { return id; }
        }

        public RawImageResource(ResourceID resourceId, string name)
          : base(name)
        {
            this.id = resourceId;
        }

        public RawImageResource(PsdBinaryReader reader, string signature,
          ResourceID resourceId, string name, int numBytes)
          : base(name)
        {
            this.Signature = signature;
            this.id = resourceId;
            Data = reader.ReadBytes(numBytes);
        }
    }
}

namespace PSD2UGUI
{
    public class LayerUnicodeName : LayerInfo
    {
        public override string Key
        {
            get { return "luni"; }
        }

        public string Name { get; set; }

        public LayerUnicodeName(string name)
        {
            Name = name;
        }

        public LayerUnicodeName(PsdBinaryReader reader)
        {
            Name = reader.ReadUnicodeString();
        }
    }
}

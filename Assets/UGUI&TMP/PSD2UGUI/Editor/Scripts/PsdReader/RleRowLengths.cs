using System.Linq;

namespace PSD2UGUI
{
    public class RleRowLengths
    {
        public int[] Values { get; private set; }

        public int Total
        {
            get { return Values.Sum(); }
        }

        public int this[int i]
        {
            get { return Values[i]; }
            set { Values[i] = value; }
        }

        public RleRowLengths(int rowCount)
        {
            Values = new int[rowCount];
        }

        public RleRowLengths(PsdBinaryReader reader, int rowCount)
          : this(rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                Values[i] = reader.ReadUInt16();
            }
        }
    }

}

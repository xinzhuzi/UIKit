using System;

namespace PSD2UGUI
{
    [Serializable]
    public class PsdInvalidException : Exception
    {
        public PsdInvalidException()
        {
        }

        public PsdInvalidException(string message)
          : base(message)
        {
        }
    }

    [Serializable]
    public class RleException : Exception
    {
        public RleException() { }

        public RleException(string message) : base(message) { }
    }
}

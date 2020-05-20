using System;

namespace Mud.Importer.Rom
{
    [Serializable]
    public class RomConvertException : Exception
    {
        public RomConvertException(string message)
            : base(message)
        {
        }

        public RomConvertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public RomConvertException(string format, params object[] parameters)
            : base(string.Format(format, parameters))
        {
        }
    }
}

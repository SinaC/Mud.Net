using System;

namespace Mud.Importer.Mystery
{
    [Serializable]
    public class MysteryConvertException : Exception
    {
        public MysteryConvertException(string message)
            : base(message)
        {
        }

        public MysteryConvertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public MysteryConvertException(string format, params object[] parameters)
            : base(string.Format(format, parameters))
        {
        }
    }
}

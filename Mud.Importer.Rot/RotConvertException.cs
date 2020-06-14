using System;

namespace Mud.Importer.Rot
{
    [Serializable]
    public class RotConvertException : Exception
    {
        public RotConvertException(string message)
            : base(message)
        {
        }

        public RotConvertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public RotConvertException(string format, params object[] parameters)
            : base(string.Format(format, parameters))
        {
        }
    }
}

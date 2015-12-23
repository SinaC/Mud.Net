using System;

namespace Mud.Importer
{
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ParseException(string format, params object[] parameters)
            : base(String.Format(format, parameters))
        {
        }
    }
}

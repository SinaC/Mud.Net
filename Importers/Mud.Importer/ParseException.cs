namespace Mud.Importer;

[Serializable]
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
        : base(string.Format(format, parameters))
    {
    }
}

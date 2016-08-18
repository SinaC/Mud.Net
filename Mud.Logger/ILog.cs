namespace Mud.Logger
{
    public enum LogLevels
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
    }

    public interface ILog
    {
        void Initialize(string path, string file, string fileTargetName = "logfile");
        void WriteLine(LogLevels level, string format, params object[] args);
    }
}

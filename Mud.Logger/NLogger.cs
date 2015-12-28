using System;
using System.IO;
using NLog;
using NLog.Targets;

namespace Mud.Logger
{
    public class NLogger : ILog
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger("Mud.Net");

        #region ILog

        public void Initialize(string path, string file, string fileTarget = "logfile")
        {
            string logfile = Path.Combine(path, file);
            FileTarget target = LogManager.Configuration.FindTargetByName(fileTarget) as FileTarget;
            if (target == null)
                throw new ApplicationException(String.Format("Couldn't find target {0} in NLog config", fileTarget));
            target.FileName = logfile;
        }

        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    _logger.Debug(format, args);
                    break;
                case LogLevels.Info:
                    _logger.Info(format, args);
                    break;
                case LogLevels.Warning:
                    _logger.Warn(format, args);
                    break;
                case LogLevels.Error:
                    _logger.Error(format, args);
                    break;
            }
        }

        #endregion
    }
}

using System;
using System.IO;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Mud.Logger
{
    public class NLogger : ILog
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger("Mud.Net");

        #region ILog

        public void Initialize(string path, string file, string fileTargetName = "logfile")
        {
            string logfile = Path.Combine(path, file);
            //FileTarget target = LogManager.Configuration.FindTargetByName(fileTarget) as FileTarget;
            var target = LogManager.Configuration.FindTargetByName(fileTargetName);
            if (target == null)
                throw new ApplicationException($"Couldn't find target {fileTargetName} in NLog config");
            FileTarget fileTarget;
            if (target is AsyncTargetWrapper asyncTargetWrapper)
                fileTarget = asyncTargetWrapper.WrappedTarget as FileTarget;
            else
                fileTarget = target as FileTarget;
            if (fileTarget == null)
                throw new ApplicationException($"Target {fileTargetName} is not a FileTarget");
            fileTarget.FileName = logfile;
        }

        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevels.Trace:
                    _logger.Trace(format, args);
                    break;
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

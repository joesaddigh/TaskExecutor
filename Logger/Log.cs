using log4net;
using System;

namespace Logger
{
    public class Log
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Log));

        private static Log m_Instance;

        public enum LogLevel
        {
            Debug,
            Information,
            Warning,
            Error,
            Fatal
        };

        private bool m_bLogging = true;

        private Log()
        {
            this.Initialise();
        }

        private void Initialise()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static Log Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new Log();
                }
                return m_Instance;
            }
        }

        public void OutputLog(string sLog, LogLevel eLogLevel)
        {
            if (this.m_bLogging)
            {
                Console.WriteLine(sLog);

                switch (eLogLevel)
                {
                    case LogLevel.Debug:
                        {
                            log.Debug(sLog);
                        }
                        break;

                    case LogLevel.Information:
                        {
                            log.Info(sLog);
                        }
                        break;

                    case LogLevel.Warning:
                        {
                            log.Warn(sLog);
                        }
                        break;

                    case LogLevel.Error:
                        {
                            log.Error(sLog);
                        }
                        break;

                    case LogLevel.Fatal:
                        {
                            log.Fatal(sLog);
                        }
                        break;
                }
            }
        }
    }
}
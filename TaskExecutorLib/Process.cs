using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TaskExecutorLib
{
    public class Process
    {
        private string ProcessFullPath { get; set; }
        private ProcessWindowStyle ProcessWindowStyle { get; set; }
        private List<string> Arguments { get; set; } = new List<string>();

        public const char ArgSeparator = '|';
        
        public Process(string sProcessFullPath, ProcessWindowStyle eProcessWindowStyle, string sArguments)
        {
            ProcessFullPath = Environment.ExpandEnvironmentVariables(sProcessFullPath);
            ProcessWindowStyle = eProcessWindowStyle;

            foreach (var arg  in sArguments.Split(ArgSeparator))
            {
                Arguments.Add(Environment.ExpandEnvironmentVariables(arg));
            }

            string sLog = string.Format("Adding Process: '{0}' with arguments: '{1}'", ProcessFullPath, string.Join(" ", Arguments));
            Log.Instance.OutputLog(sLog, Log.LogLevel.Information);
        }

        public void Start(bool redirectStandardOutput = false)
        {
            try
            {
                string sLog = string.Format("Attempting to start process with command: {0} {1}", ProcessFullPath, string.Join(" ", Arguments));
                Log.Instance.OutputLog(sLog, Log.LogLevel.Information);

                ProcessStartInfo startInf = new ProcessStartInfo(ProcessFullPath);
                startInf.UseShellExecute = true;
                startInf.CreateNoWindow = true;
                startInf.WindowStyle = ProcessWindowStyle;
                if (redirectStandardOutput)
                {
                    startInf.RedirectStandardOutput = true;
                    startInf.RedirectStandardInput = true;
                    startInf.UseShellExecute = false;
                }

                if (Arguments.Any())
                {
                    foreach (var arg in Arguments)
                    {
                        startInf.Arguments +=
                            arg.Contains(" ") ? 
                            " \"" + arg + "\"" :
                            " " + arg;
                    }   
                }

                System.Diagnostics.Process proc = new System.Diagnostics.Process
                {
                    StartInfo = startInf
                };

                proc.StartInfo.Verb = "runas";

                if (redirectStandardOutput)
                {
                    // enable raising events because Process does not raise events by default
                    proc.EnableRaisingEvents = true;
                    // attach the event handler for OutputDataReceived before starting the process
                    proc.OutputDataReceived += new DataReceivedEventHandler
                    (
                        delegate (object sender, DataReceivedEventArgs e)
                        {
                            // append the new data to the data already read-in
                            Log.Instance.OutputLog(e.Data, Log.LogLevel.Information);
                        }
                    );
                }

                if (!proc.Start())
                {
                    sLog = string.Format("Failed to start process: '{0}'", ProcessFullPath);
                    Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
                }

                if (redirectStandardOutput)
                {
                    proc.BeginOutputReadLine();
                    proc.WaitForExit();
                    proc.CancelOutputRead();
                }
            }
            catch (Exception ex)
            {
                Log.Instance.OutputLog(ex.Message, Log.LogLevel.Error);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TaskExecutorLib
{
    public enum TaskType
    {
        commandTask,
        otherTask
    };

    [XmlRoot("taskData")]
    public class TaskData
    {
        [XmlElement(ElementName = "taskEntry")]
        public List<ExecutionTaskEntry> ExecutionTaskEntry { get; set; }
    }

    public class ExecutionTaskEntry : IDataErrorInfo
    {
        public string this[string columnName]
        {
            get
            {
                return string.Empty;
            }
        }

        public string Error { get; }
        
        [XmlElement]
        public int Id { get; set; }

        [XmlElement]
        public string Caption { get; set; }

        [XmlElement]
        public TaskType Type { get; set; }

        [XmlElement]
        public string Command { get; set; }

        [XmlElement]
        public string Args { get; set; }
    }

    public class ExecutionTaskCommand
    {
        public int Id { get; set; }
        public TaskType Type { get; set; }
        public string Command { get; internal set; }
        public string Args { get; internal set; }
        public bool Parameterised { get; internal set; }
        public bool Confirm { get; internal set; }
        public bool RedirectStandardOutput { get; internal set; }

        public string GetCommandExpandEnvironmentVariables()
        {
            return Environment.ExpandEnvironmentVariables(Command);
        }
    }

    public class ExecutionTask
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public string Image { get; set; } 
        
        public List<ExecutionTaskCommand> Commands { get; set; }

        public string GetImageFullPath()
        {
            string fullImagePath = Environment.ExpandEnvironmentVariables(Image);

            if (!string.IsNullOrEmpty(fullImagePath) && !Path.IsPathRooted(fullImagePath))
            {
                fullImagePath = Path.GetFullPath(fullImagePath);
            }

            return fullImagePath;
        }

        public string GetToolTip()
        {
            var sb = new StringBuilder();

            foreach (var command in Commands)
            {
                string parameterisedText = command.Parameterised ? "... " : string.Empty;
                string tooltip = $"{parameterisedText}{Caption} ({Environment.ExpandEnvironmentVariables(command.Command)} {Environment.ExpandEnvironmentVariables(command.Args)})";
                sb.Append($"{tooltip}{Environment.NewLine}");
            }

            return sb.ToString();
        }
    }
}
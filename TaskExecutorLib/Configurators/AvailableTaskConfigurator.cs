using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using TaskExecutorLib.Helpers;
using static TaskExecutorLib.ExecutionTask;

namespace TaskExecutorLib.Shared
{
    public class TaskConfigurator
    {
        public StringBuilder SchemaErrors { get; private set; }
        public List<ExecutionTask> Tasks { get; private set; }

        public string ConfigDirectory { get; private set; }
        public string XMLFile { get; private set; }
        public string XSDFile { get; private set; }

        public TaskConfigurator(string sConfigDirectory, string sXMLFile, string sXSDFile)
        {
            this.ConfigDirectory = sConfigDirectory;
            this.XMLFile = sXMLFile;
            this.XSDFile = sXSDFile;

            this.SchemaErrors = new StringBuilder();
            this.Tasks = new List<ExecutionTask>();

            this.ParseXML();
        }

        public List<ExecutionTask> GetTasks()
        {
            return this.Tasks;
        }

        public int GetNextAvailableTaskId()
        {
            int iNextAvailableTaskId = 0;
            foreach (var task in this.Tasks)
            {
                if (task.Id > iNextAvailableTaskId)
                {
                    iNextAvailableTaskId = task.Id;
                }
            }
            return iNextAvailableTaskId;
        }

        internal ExecutionTask GetTaskByID(int iTaskId)
        {
            ExecutionTask task = null;
            if (this.Tasks != null)
            {
                task = this.Tasks.First(s => s.Id == iTaskId);
            }
            return task;
        }

        private bool ValidateXMLAgainstSchema(string sXML, string sXSD, ref XmlDocument xmldoc)
        {
            bool bOkay = false;
            string sLog = string.Empty;

            try
            {
                if ((File.Exists(sXML)) &&
                    (File.Exists(sXSD)))
                {
                    XmlReaderSettings xmlsettings = new XmlReaderSettings();
                    xmlsettings.Schemas.Add(null, sXSD);
                    xmlsettings.ValidationType = ValidationType.Schema;
                    xmlsettings.ValidationEventHandler += new ValidationEventHandler(this.ValidationEventHandler);

                    using (XmlReader xmlReader = XmlReader.Create(sXML, xmlsettings))
                    {
                        xmldoc = new XmlDocument();
                        xmldoc.Load(xmlReader);

                        // If the XML is not valid, let's not trust the integrity of the data, no further action required, just report/log.
                        if ((this.SchemaErrors != null) &&
                            (0 == this.SchemaErrors.Length))
                        {
                            bOkay = true;
                        }
                    }
                }
                else
                {
                    sLog = string.Format("Failed to find XML '{0}' and/or XSD '{1}' files", sXML, sXSD);
                    throw new System.ArgumentException(sLog, sXML);
                }
            }
            catch (Exception ex)
            {
                sLog = string.Format("Parse XML exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }
            return bOkay;
        }

        /// <summary>
        /// Parses the Task XML and populates internal collection.
        /// </summary>
        /// <returns>true if successful</returns>
        private bool ParseXML()
        {
            bool bIsValidFile = false;
            string sLog = string.Empty;

            try
            {
                this.Tasks.Clear();

                // XML path should be the same as the exe.
                string sXML = this.ConfigDirectory + this.XMLFile;
                string sXSD = this.ConfigDirectory + this.XSDFile;

                if ((File.Exists(sXML)) &&
                    (File.Exists(sXSD)))
                {
                    XmlDocument xmldoc = null;
                    if (this.ValidateXMLAgainstSchema(sXML, sXSD, ref xmldoc))
                    {
                        // If the XML is not valid, let's not trus the integrity of the data, no further action required, just report/log.
                        if ((this.SchemaErrors != null) &&
                            (0 == this.SchemaErrors.Length))
                        {
                            int iTask = 0;
                            XmlNodeList xmlnodeList = XmlUtils.GetElementsByTagName(xmldoc, ConfigStringKey.TASK_ENTRY);

                            foreach (XmlElement xmlnode in xmlnodeList)
                            {
                                XmlNodeList xmlnodeListCommands = xmlnode.SelectNodes(ConfigStringKey.COMMAND_ELEMENT);

                                var commands = new List<ExecutionTaskCommand>();

                                int commandId = 0;
                                foreach (XmlNode xmlCommandNode in xmlnodeListCommands)
                                {
                                    commands.Add(new ExecutionTaskCommand()
                                    {
                                        Id = commandId,
                                        Command = XmlUtils.GetSubElementByTagNameAsString(xmlCommandNode, ConfigStringKey.COMMAND),
                                        Type = (TaskType)Enum.Parse(typeof(TaskType), XmlUtils.GetSubElementByTagNameAsString(xmlCommandNode, ConfigStringKey.TYPE)),
                                        Args = XmlUtils.GetSubElementByTagNameAsString(xmlCommandNode, ConfigStringKey.ARGS),
                                        Parameterised = XmlUtils.GetSubElementByTagNameAsBool(xmlCommandNode, ConfigStringKey.PARAMETERISED),
                                        Confirm = XmlUtils.GetSubElementByTagNameAsBool(xmlCommandNode, ConfigStringKey.CONFIRM),
                                        RedirectStandardOutput = XmlUtils.GetSubElementByTagNameAsBool(xmlCommandNode, ConfigStringKey.REDIRECT_STANDARD_OUTPUT)
                                    });

                                    commandId++;
                                }

                                this.Tasks.Add(new ExecutionTask()
                                {
                                    Id = iTask,
                                    Caption = XmlUtils.GetElementByTagNameAsString(xmlnode, ConfigStringKey.CAPTION),
                                    Image = XmlUtils.GetElementByTagNameAsString(xmlnode, ConfigStringKey.IMAGE),
                                    Commands = commands
                                });

                                ++iTask;
                            }
                        }
                    }

                    // Set return type
                    bIsValidFile = ((this.SchemaErrors != null) && (0 == this.SchemaErrors.Length));
                }
                else
                {
                    sLog = string.Format("Failed to find XML '{0}' and/or XSD '{1}' files", sXML, sXSD);
                    throw new System.ArgumentException(sLog, sXML);
                }

                // Now a final check to report any parsing errors.
                if ((this.SchemaErrors != null) &&
                    (this.SchemaErrors.Length > 0))
                {
                    sLog = string.Format("Failed to parse XML file {0} using schema: {1} errors are: {2}", this.XMLFile, this.XSDFile, this.SchemaErrors.ToString());
                    throw new System.ArgumentException(sLog);
                }
            }
            catch (Exception ex)
            {
                sLog = string.Format("Parse XML exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return bIsValidFile;
        }

        /// <summary>
        /// Append any Schema errors.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            string sLog = string.Empty;
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    sLog = string.Format("Error: {0}", e.Message);
                    break;

                case XmlSeverityType.Warning:
                    sLog = string.Format("Warning: {0}", e.Message);
                    break;
            }
            this.SchemaErrors.Append(sLog);
        }
    }
}
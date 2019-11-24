using Logger;
using System;
using System.IO;
using System.Xml;

namespace TaskExecutorLib
{
    public class SettingsConfigurator
    {
        public string SettingsXMLFile { get; private set; }
        public string ConfigDirectory { get; private set; }
        public string ConfigDataDirectory { get; private set; }
        public string TasksXML { get; private set; }
        public string TasksXSD{ get; private set; }
        public int TasksPerRow { get; private set; }
        public int DefaultWidthPixels { get; private set; }
        public int DefaultHeightPixels { get; private set; }

        public SettingsConfigurator(string settingsXMLFile)
        {
            this.SettingsXMLFile = settingsXMLFile;

            if (!Path.IsPathRooted(this.SettingsXMLFile))
            {
                this.SettingsXMLFile = Path.GetFullPath(this.SettingsXMLFile);
            }

            this.ParseSettings();
        }

        private string GetPathFromXML(XmlDocument xmldoc, string sTag)
        {
            string sPath = string.Empty;
            try
            {
                if (xmldoc != null)
                {
                    sPath = this.ReadStringSetting(sTag, xmldoc);
                    if (!Path.IsPathRooted(sPath))
                    {
                        sPath = Path.GetFullPath(sPath);
                    }
                }
            }
            catch (Exception ex)
            {
                string sLog = string.Format("GetPathFromXML file exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return sPath;            
        }

        private void ParseSettings()
        {
            try
            {

                if (File.Exists(this.SettingsXMLFile))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(this.SettingsXMLFile);

                    this.ConfigDataDirectory = this.GetPathFromXML(xmldoc, ConfigStringKey.CONFIG_DATA_DIRECTORY);
                    this.TasksXML = this.ReadStringSetting(ConfigStringKey.TASKS_XML, xmldoc);
                    this.TasksXSD = this.ReadStringSetting(ConfigStringKey.TASKS_XSD, xmldoc);
                    this.TasksPerRow = this.ReadIntSetting(ConfigStringKey.TASKS_PER_ROW, xmldoc);
                    this.DefaultWidthPixels = this.ReadIntSetting(ConfigStringKey.DEFAULT_WIDTH_PIXELS, xmldoc);
                    this.DefaultHeightPixels = this.ReadIntSetting(ConfigStringKey.DEFAULT_HEIGHT_PIXELS, xmldoc);
                }
            }
            catch (Exception ex)
            {
                string sLog = string.Format("Parse Settings file exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }
        }        

        private string ReadStringSetting(string sElement, XmlDocument xDoc)
        {
            string sResult = string.Empty;
            try
            {
                XmlNodeList xmlnodeList = xDoc.GetElementsByTagName(ConfigStringKey.SETTINGS);
                foreach (XmlElement xmlnode in xmlnodeList)
                {
                    sResult = xmlnode.GetElementsByTagName(sElement).Item(0).InnerText;
                }
            }
            catch (Exception ex)
            {
                string sLog = string.Format("Read String Setting file exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }
            return sResult;
        }

        private bool ReadBoolSetting(string sElement, XmlDocument xDoc)
        {
            bool result = default(bool);
            try
            {
                string sResult = this.ReadStringSetting(sElement, xDoc);
                bool.TryParse(sResult, out result);
            }
            catch (Exception ex)
            {
                string sLog = string.Format("Read Bool Setting file exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }
            return result;
        }

        private int ReadIntSetting(string sElement, XmlDocument xDoc)
        {
            int result = default(int);
            try
            {
                string sResult = this.ReadStringSetting(sElement, xDoc);
                int.TryParse(sResult, out result);
            }
            catch (Exception ex)
            {
                string sLog = string.Format("Read Int Setting file exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }
            return result;
        }        
    }
}
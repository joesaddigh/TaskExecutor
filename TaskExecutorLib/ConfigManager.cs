using System;
using System.Collections.Generic;
using System.Configuration;
using TaskExecutorLib.Shared;

namespace TaskExecutorLib
{
    public class ConfigManager
    {
        private static ConfigManager m_Instance;
        public SettingsConfigurator SettingsConfigurator { get; set; }
        public TaskConfigurator AvailableTaskConfigurator { get; set; }

        private ConfigManager() => this.Initialise();

        public void Initialise()
        {
            // Firstly setup our settings manager class, responsible for parsing all of the application settings.
            this.SettingsConfigurator = new SettingsConfigurator(ConfigurationManager.AppSettings[ConfigStringKey.SETTINGS_XML_FILE]);
            this.AvailableTaskConfigurator = new TaskConfigurator(this.SettingsConfigurator.ConfigDataDirectory, this.SettingsConfigurator.TasksXML, this.SettingsConfigurator.TasksXSD);
        }        

        public static ConfigManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new ConfigManager();
                }

                return m_Instance;
            }
        }
    }
}
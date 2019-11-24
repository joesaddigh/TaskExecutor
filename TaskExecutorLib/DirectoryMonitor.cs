using System;
using System.IO;

namespace TaskExecutorLib
{
    public class DirectoryMonitor
    {
        private string MonitorPath { get; set; }
        private FileSystemEventHandler CreatedEvent { get; set; }
        private RenamedEventHandler RenamedEvent { get; set; }
        private FileSystemEventHandler DeletedEvent { get; set; }
        private FileSystemEventHandler ChangedEvent { get; set; }

        public DirectoryMonitor(string path,
                                FileSystemEventHandler changedEvent,
                                FileSystemEventHandler createdEvent,
                                RenamedEventHandler renamedEvent,
                                FileSystemEventHandler deletedEvent)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null");
            }
            else if (changedEvent == null)
            {
                throw new ArgumentNullException(nameof(changedEvent), "changedEvent cannot be null");
            }
            else if (createdEvent == null)
            {
                throw new ArgumentNullException(nameof(createdEvent), "createdEvent cannot be null");
            }
            else if (renamedEvent == null)
            {
                throw new ArgumentNullException(nameof(renamedEvent), "renamedEvent cannot be null");
            }
            else if (deletedEvent == null)
            {
                throw new ArgumentNullException(nameof(deletedEvent), "deletedEvent cannot be null");
            }

            MonitorPath = path;
            ChangedEvent = changedEvent;
            CreatedEvent = createdEvent;
            RenamedEvent = renamedEvent;
            DeletedEvent = deletedEvent;
        }

        public void StartMonitoring()
        {
            var fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = MonitorPath;
            fileSystemWatcher.Changed += ChangedEvent;
            fileSystemWatcher.Created += CreatedEvent;
            fileSystemWatcher.Renamed += RenamedEvent;
            fileSystemWatcher.Deleted += DeletedEvent;
            fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}
using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TaskExecutorLib;
using TaskExecutorLib.Helpers;

namespace TaskExecutor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DirectoryMonitor DirMonitor { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();

            ApplyFormDimensions();
            PopulateTasksToExecute();

            DirMonitor = new DirectoryMonitor(
                ConfigManager.Instance.SettingsConfigurator.ConfigDataDirectory,
                FileSystemWatcher_Changed,
                FileSystemWatcher_Created,
                FileSystemWatcher_Renamed,
                FileSystemWatcher_Deleted);

            DirMonitor.StartMonitoring();
        }

        private void FileSystemWatcher_Changed(object source, FileSystemEventArgs e)
        {
            Log.Instance.OutputLog(String.Format("File changed: {0}", e.Name), Log.LogLevel.Debug);

            ReInitialiseEverything();
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Log.Instance.OutputLog(String.Format("File created: {0}", e.Name), Log.LogLevel.Debug);

            ReInitialiseEverything();
        }

        private void FileSystemWatcher_Renamed(object sender, FileSystemEventArgs e)
        {
            Log.Instance.OutputLog(String.Format("File renamed: {0}", e.Name), Log.LogLevel.Debug);

            ReInitialiseEverything();
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Log.Instance.OutputLog(String.Format("File deleted: {0}", e.Name), Log.LogLevel.Debug);

            ReInitialiseEverything();
        }

        private void ReInitialiseEverything()
        {
            ConfigManager.Instance.Initialise();
            ApplyFormDimensions();
            PopulateTasksToExecute();
        }

        private void ApplyFormDimensions()
        {
            if (!CheckAccess())
            {
                // On a different thread
                Dispatcher.Invoke(() => ApplyFormDimensions());
                return;
            }

            Width = ConfigManager.Instance.SettingsConfigurator.DefaultWidthPixels;
            Height = ConfigManager.Instance.SettingsConfigurator.DefaultHeightPixels;
        }

        private void PopulateTasksToExecute()
        {
            try
            {
                if (!CheckAccess())
                {
                    // On a different thread
                    Dispatcher.Invoke(() => PopulateTasksToExecute());
                    return;
                }

                GuiHelpers.ClearGridGuiItems(gridTasks);

                // Get the configured tasks
                var configuredTasks = ConfigManager.Instance.AvailableTaskConfigurator.GetTasks();
                // Get the configured tasks per row
                int columns = ConfigManager.Instance.SettingsConfigurator.TasksPerRow;
                // Get the amount of rows required
                int rows = configuredTasks.Count;
                if (columns > 0 && configuredTasks.Count > columns)
                {
                    rows = (int)(Math.Ceiling(configuredTasks.Count / (double)columns));
                }

                // Work out roughly how much of the screen, as a percentage, each column should consume.
                var columnPercentageWidth = (this.Width / (columns > 0 ? columns : 1));

                // Add column definitions
                for (var column = 0; column < columns; column++)
                {
                    gridTasks.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(columnPercentageWidth, GridUnitType.Star),
                    });
                }

                // Add row definitions and all of the buttons for each command.
                for (var row = 0; row < rows; row++)
                {
                    gridTasks.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(75, GridUnitType.Pixel)
                    });

                    for (var column = 0; column < columns; column++)
                    {
                        int taskIndex = (column + 1);
                        if (row > 0)
                        {
                            taskIndex += row * columns;
                        }

                        var task = CollectionUtils.SafeGetItemByIndex<ExecutionTask>((taskIndex - 1), configuredTasks);

                        CreateButtonForGrid(task, gridTasks, row, column);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.OutputLog(string.Format("{0}", ex.Message), Log.LogLevel.Error);
            }
        }

        private void CreateButtonForGrid(ExecutionTask task, Grid grid, int gridRow, int gridColumn)
        {
            if (task != null && grid != null)
            {
                var button = new Button()
                {
                    Content = task.Caption,
                    Margin = new Thickness(10, 10, 10, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Tag = task.Id,
                    ToolTip = task.GetToolTip()
                };

                var fullImagePath = task.GetImageFullPath();

                if (!string.IsNullOrEmpty(fullImagePath) &&
                    File.Exists(fullImagePath))
                {
                    var img = new BitmapImage(new Uri(fullImagePath, UriKind.RelativeOrAbsolute));
                    var image = new Image
                    {
                        Source = img,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    button.Content = image;
                }

                button.Click += btn_TaskClick;

                grid.Children.Add(button);
                Grid.SetRow(button, gridRow);
                Grid.SetColumn(button, gridColumn);
            }
        }

        private void btn_TaskClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var taskId = (int)button.Tag;

            if (taskId >= 0)
            {
                ExecutionTask task = ConfigManager.Instance.AvailableTaskConfigurator.Tasks.Find(test => test.Id == taskId);

                var confirmCommands = task.Commands.Where(command => command.Confirm);
                if ((!confirmCommands.Any()) ||
                     (confirmCommands.Any() &&
                      MessageBox.Show($"Are you sure you wish to perform task: {task.Caption}?",
                                      $"Task {task.Caption} Confirmation",
                                      MessageBoxButton.OKCancel
                                      )
                        == MessageBoxResult.OK
                     )
                   )
                {
                    bool executeTask = true;
                    var overrideArgs = new Dictionary<int, string>();

                    var parameterisedCommands = task.Commands.Where(command => command.Parameterised);
                    if (parameterisedCommands.Any())
                    {
                        var paramsDialogResult = ParamsWindow.PerformShowDialog(task, this);
                        executeTask = paramsDialogResult.Result;
                        overrideArgs = paramsDialogResult.OverrideArgs;
                    }

                    if (executeTask)
                    {
                        TaskExecutorImp.ExecuteTask(task, overrideArgs);
                    }
                }
            }
        }
    }
}
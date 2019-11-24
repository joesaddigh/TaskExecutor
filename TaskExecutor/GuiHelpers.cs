using System;
using System.Windows;
using System.Windows.Controls;

namespace TaskExecutor
{
    public static class GuiHelpers
    {
        public enum ControlType
        {
            eButton,
            eTextBlock,
            eTextBox
        }

        public static void ClearGridGuiItems(Grid grid)
        {
            if (grid != null)
            {
                // Clear up the GUI
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                grid.ColumnDefinitions.Clear();
            }
        }

        public static void CreateControlAndAddToGrid(Grid grid, ControlType eControlType, object tagValue, string caption, int row, int column, int width, int height, string tooltip)
        {
            CreateControlAndAddToGrid(grid, eControlType, tagValue, caption, row, column, width, height, tooltip, new Thickness(0,0,0,0), null);
        }

        public static void CreateControlAndAddToGrid(Grid grid, ControlType eControlType, object tagValue, string caption, int row, int column, int width, int height, string tooltip, Thickness margin, RoutedEventHandler clickEventHandler)
        {
            if (grid != null)
            {
                UIElement control = null;

                switch (eControlType)
                {
                    case ControlType.eButton:
                        {
                            control = new Button()
                            {
                                Tag = tagValue,
                                Content = caption,
                                Width = width,
                                Height = height,
                                ToolTip = tooltip
                            };

                            (control as Button).Click += clickEventHandler;
                        }
                        break;

                    case ControlType.eTextBlock:
                        {
                            control = new TextBlock()
                            {
                                Tag = tagValue,
                                Text = caption,
                                Margin = margin,
                                ToolTip = tooltip,
                                TextWrapping = TextWrapping.Wrap                                
                            };
                        }
                        break;

                    case ControlType.eTextBox:
                        {
                            control = new TextBox()
                            {
                                Tag = tagValue,
                                Text = caption,
                                Margin = margin,
                                ToolTip = $"Args must be separated by a '{TaskExecutorLib.Process.ArgSeparator}' character. {tooltip}",
                            };
                        }
                        break;
                }

                grid.Children.Add(control);
                Grid.SetRow(control, row);
                Grid.SetColumn(control, column);
            }
        }
    }
}
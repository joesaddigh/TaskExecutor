using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskExecutorLib;

namespace TaskExecutor
{
    /// <summary>
    /// Interaction logic for Params.xaml
    /// </summary>
    public partial class ParamsWindow : Window
    {
        private ExecutionTask Task { get; set; }

        private Dictionary<int, string> OverrideArgs { get; set; }

        public ParamsWindow(ExecutionTask task)
        {
            InitializeComponent();

            if (task == null)
            {
                throw new ArgumentNullException(nameof(task), "task cannot be null!");
            }

            Task = task;

            InitialiseGui();
        }

        private void InitialiseGui()
        {
            GuiHelpers.ClearGridGuiItems(gridCommands);

            var parameterisedCommands = Task.Commands.Where(command => command.Parameterised);
            if (parameterisedCommands.Any())
            {
                // Add column definitions
                int commandColumnWidthPercentage = 45;
                int argsColumnWidthPercentage = 100 - commandColumnWidthPercentage;

                gridCommands.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(commandColumnWidthPercentage, GridUnitType.Star),
                });

                gridCommands.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(argsColumnWidthPercentage, GridUnitType.Star),
                });

                const string tooltipLabel = "Command";
                const string tooltipButton = "Execute command with specified parameters";

                // Add row definitions and all of the Gui elements for each parameterised command.
                int row = 0;
                foreach (var command in parameterisedCommands)
                {
                    gridCommands.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(50, GridUnitType.Pixel)
                    });

                    var fullCommandTooltipTextbox = $"Evaluates to: ({command.GetFullEvaluatedCommandWithArgs()})";

                    GuiHelpers.CreateControlAndAddToGrid(
                        gridCommands,
                        GuiHelpers.ControlType.eTextBlock,
                        command.Id,
                        command.GetCommandExpandEnvironmentVariables(),
                        row,
                        0,
                        0,
                        0,
                        tooltipLabel,
                        new Thickness(10, 10, 10, 10),
                        null,
                        null
                    );

                    if (command.SupportedParams.Count() > 0)
                    {
                        GuiHelpers.CreateControlAndAddToGrid(
                            gridCommands,
                            GuiHelpers.ControlType.eComboBox,
                            command.Id,
                            command.Args,
                            command.SupportedParams,
                            row,
                            1,
                            0,
                            0,
                            fullCommandTooltipTextbox,
                            new Thickness(10, 10, 10, 10),
                            null,
                            TextBoxSetTooltip
                        );
                    }
                    else
                    {
                        GuiHelpers.CreateControlAndAddToGrid(
                            gridCommands,
                            GuiHelpers.ControlType.eTextBox,
                            command.Id,
                            command.Args,
                            row,
                            1,
                            0,
                            0,
                            fullCommandTooltipTextbox,
                            new Thickness(10, 10, 10, 10),
                            null,
                            TextBoxSetTooltip
                        );
                    }

                    ++row;
                }

                // Add Ok button on at the end.
                var okBtnRowIndex = parameterisedCommands.Count() + 1;

                gridCommands.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(60, GridUnitType.Pixel)
                });

                GuiHelpers.CreateControlAndAddToGrid(
                    gridCommands,
                    GuiHelpers.ControlType.eButton,
                    null,
                    "Ok",
                    okBtnRowIndex,
                    1,
                    60,
                    30,
                    tooltipButton,
                    new Thickness(0, 0, 0, 0),
                    Ok_Click,
                    null
                );
            }
        }

        private void TextBoxSetTooltip(object Sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)Sender;
            if (textBox != null && textBox.Tag != null)
            {
                var commands = Task.Commands.Where(value => value.Id == (int)textBox.Tag);
                if (commands.Any())
                {
                    var command = commands.First();
                    textBox.ToolTip = $"Evaluates to: ({command.GetFullEvaluatedCommandWithArgs(textBox.Text)})";
                    textBox.Focus();
                }
            }
        }

        public static ParamsDialogResult PerformShowDialog(ExecutionTask task, Window parent)
        {
            var form = new ParamsWindow(task);
            form.Owner = parent;
            var result = form.ShowDialog() ?? false;

            return new ParamsDialogResult()
            {
                Result = result,
                OverrideArgs = form.OverrideArgs
            };
        }

        public class ParamsDialogResult
        {
            public bool Result { get; set; }
            public Dictionary<int, string> OverrideArgs { get; set; }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            var textboxes = this.gridCommands.Children.OfType<TextBox>();
            var comboboxes = this.gridCommands.Children.OfType<ComboBox>();

            OverrideArgs = new Dictionary<int, string>();

            foreach (var tb in textboxes)
            {
                int commandId = (int)tb.Tag;
                if (commandId >= 0)
                {
                    OverrideArgs.Add(commandId, tb.Text);
                }
            }

            foreach (var cb in comboboxes)
            {
                int commandId = (int)cb.Tag;
                if (commandId >= 0)
                {
                    OverrideArgs.Add(commandId, cb.Text);
                }
            }

            this.Close();
        }
    }
}
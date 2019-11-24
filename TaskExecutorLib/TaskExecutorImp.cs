using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TaskExecutorLib
{
    public static class TaskExecutorImp
    {
        public static bool ExecuteTask(ExecutionTask task)
        {            
            return ExecuteTask(task, new Dictionary<int, string>());
        }

        public static bool ExecuteTask(ExecutionTask task, Dictionary<int, string> overrideArgs)
        {
            if (task != null)
            {
                foreach (var command in task.Commands)
                {
                    switch (command.Type)
                    {
                        case TaskType.commandTask:
                            {
                                var args = command.Args;

                                if (overrideArgs.ContainsKey(command.Id))
                                {
                                    args = overrideArgs[command.Id];
                                }

                                var process = new Process(command.Command, ProcessWindowStyle.Normal, args);
                                process.Start(command.RedirectStandardOutput);
                            }
                            break;

                        case TaskType.otherTask:
                        default:
                            {
                                // Not developed yet.
                                throw new NotImplementedException();
                            }
                    }
                }
            }

            return true;
        }
    }
}
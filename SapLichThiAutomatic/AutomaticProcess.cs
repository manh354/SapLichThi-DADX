using SapLichThiAlgorithm.ErrorAndLog;
using SapLichThiNew;
using SapLichThiStream;
using System.Collections.Generic;

namespace SapLichThiAutomatic
{
    public class AutomaticProcessRecursive
    {
        public AlgoProcess Process { get; set; }
        private readonly HashSet<AlgoProcess> _subscribed = new();

        public void RunProcess()
        {
            // subscribe recursively to process and its sub processes
            if (Process != null)
            {
                SubscribeRecursive(Process);
                // run the process (blocking)
                Process.RunAsync().Wait();
            }
        }

        public void SubscribeAll(AlgoProcess root)
        {
            if (root == null) return;
            Process = root;
            SubscribeRecursive(root);
        }

        private void SubscribeRecursive(AlgoProcess proc)
        {
            if (proc == null) return;
            if (!_subscribed.Add(proc)) return; // already subscribed

            proc.OnInputRequest += Process_OnInputRequest;
            proc.OnError += Process_OnError;
            proc.OnRunning += Process_OnRunning;
            proc.OnInitialized += Process_OnInitialized_Internal;
            proc.OnFinished += Process_OnFinished;

            // subscribe existing children (if any)
            if (proc.SubProcesses != null)
            {
                foreach (var child in proc.SubProcesses)
                {
                    SubscribeRecursive(child);
                }
            }
        }

        private void Process_OnInitialized_Internal(object? sender, EventArgs e)
        {
            // when a process is initialized it may have created sub processes; subscribe to them
            if (sender is AlgoProcess proc && proc.SubProcesses != null)
            {
                foreach (var child in proc.SubProcesses)
                {
                    SubscribeRecursive(child);
                }
            }
            Process_OnInitialized(sender, e);
        }

        private void Process_OnFinished(object? sender, EventArgs e)
        {
            Logger.LogMessage($"Process is Finished! ", LogType.Info);
        }

        private void Process_OnInitialized(object? sender, EventArgs e)
        {
            Logger.LogMessage($"Process is Initialized! ", LogType.Info);
        }

        private void Process_OnRunning(object? sender, EventArgs e)
        {
            Logger.LogMessage($"Process is Running... ", LogType.Info);
        }

        private void Process_OnError(object? sender, OnErrorEventArgs e)
        {
            Logger.LogMessage($"Process Failed, error given: {e.Message}", LogType.Error);
        }

        private void Process_OnInputRequest(object? sender, InputRequestEventArgs e)
        {
            e.InputReceived = true;
        }
    }
    public class AutomaticProcess
    {
        public IDataSource DataSource { get; set; }
        public ISchedulingModel Model { get; set; }
        public AutomaticProcess(IDataSource dataSource, ISchedulingModel model)
        {
            DataSource = dataSource;
            Model = model;
        }
        public APOverall Process { get; set; }

        public void RunProcess()
        {
            Process = new APOverall(DataSource, Model);

            AutomaticProcessRecursive recursiveProcess = new AutomaticProcessRecursive();
            // subscribe recursively to main process and its sub processes before running
            recursiveProcess.SubscribeAll(Process);

            // run process (blocking) with recursive subscriptions in place
            recursiveProcess.RunProcess();

            // Do not call Process.RunAsync() again to avoid double execution
        }

        private void Process_OnFinished(object? sender, EventArgs e)
        {
            Logger.LogMessage($"Process is Finished! ", LogType.Info);
        }

        private void Process_OnInitialized(object? sender, EventArgs e)
        {
            Logger.LogMessage($"Process is Initialized! ", LogType.Info);
        }

        private void Process_OnRunning(object? sender, EventArgs e)
        {
            Logger.LogMessage($"Process is Running... ", LogType.Info);
        }

        private void Process_OnError(object? sender, OnErrorEventArgs e)
        {
            Logger.LogMessage($"Process Failed, error given: {e.Message}", LogType.Error);
        }

        private void Process_OnInputRequest(object? sender, InputRequestEventArgs e)
        {
            e.InputReceived = true;
        }

    }
}

using SapLichThiAlgorithm.AlgorithmsObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew
{

    public abstract class AlgoProcess
    {
        public event EventHandler OnInitialized;
        public event EventHandler OnRunning;
        public event EventHandler OnFinished;
        public event EventHandler<OnErrorEventArgs> OnError; 
        public event EventHandler<InputRequestEventArgs> OnInputRequest;
        public Action<ConsoleLogMessages> OnConsoleLog;
        public List<BaseInputQAndA> InputQAndAs { get; set; }
        public AlgorithmContext Context { get; set; }
        public List<AlgoProcess> SubProcesses { get; set; } = new();


        public virtual string GetProcessName()
        {
            return string.Empty;
        }

        protected abstract void BuildSubProcesses();

        public void AddSubProcess(AlgoProcess process)
        {
            SubProcesses.Add(process);
        }

        private void NotifyOnInitialized()
        {
            OnInitialized?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Method to initialize the process, which include building every subprocesses that is required.
        /// </summary>
        /// <returns></returns>
        protected async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                BuildSubProcesses();
                NotifyOnInitialized();
                Task.Delay(1000);
            });
        }

        private void NotifyOnFinished()
        {
            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        protected async Task FinishAsync()
        {
            await Task.Run(() =>
            {
                NotifyOnFinished();
                Task.Delay(1000);
            });
        }

        private void NotifyOnRunning()
        {
            OnRunning?.Invoke(this, EventArgs.Empty);
        }

        protected async Task RunningAsync()
        {
            await Task.Run(() =>
            {
                NotifyOnRunning();
                Task.Delay(1000);
            });

        }

        protected void NotifyOnError(Exception ex)
        {
            OnError?.Invoke(this, new OnErrorEventArgs() { Message = ex.ToString()});
        }

        private async Task NotifyOnInputRequest()
        {
            if (OnInputRequest != null)
            {
                var args = CreateInputRequestFromContext(Context);
                if (args != null)
                {
                    OnInputRequest.Invoke(this, args);

                    // Wait for the user to provide input
                    while (!args.InputReceived)
                    {
                        await Task.Delay(100); // Polling until input is provided
                    }

                    InputQAndAs = args.InputQAndAs;
                }
            }
        }

        private async Task NotifyOnConsoleLog()
        {
            if(OnConsoleLog != null)
            {
                var args = CreateConsoleLog(Context);
                if (args != null)
                {
                    OnConsoleLog.Invoke(args);
                }
            }
        }

        /// <summary>
        /// Always override this method, regardless of the process require inputs or not.
        /// If the process does not require additional inputs, then return null.
        /// This method take the AlgorithmContext object to create input request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context);

        /// <summary>
        /// Always override this method, regardless of the process require inputs or not.
        /// If the process does not require additional inputs, then return null.
        /// This method take the AlgorithmContext object to create input request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract ConsoleLogMessages? CreateConsoleLog(AlgorithmContext context);



        /// <summary>
        /// Override this method when there are some async processes that
        /// require executing BEFORE the subprocesses.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task BeforeSubprocessesAsync()
        {
            await Task.Run(() => { });
        }

        /// <summary>
        /// Override this method when there are some async processes that
        /// require executing AFTER the subprocesses.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task AfterSubprocessesAsync()
        {
            await Task.Run(() => { });
        }

        public async Task<bool> RunAsync()
        {
            await InitializeAsync();
            NotifyOnRunning();
            await NotifyOnInputRequest();
            try
            {
                await BeforeSubprocessesAsync();
            }
            catch (Exception ex)
            {
                NotifyOnError(ex);
                return false;
            }
            foreach (var process in SubProcesses)
            {
                bool flag = await process.RunAsync();
                if(!flag) return false;
            }
            try
            {
                await AfterSubprocessesAsync();
            }
            catch (Exception ex)
            {
                NotifyOnError(ex);
                return false;
            }
            await NotifyOnConsoleLog();
            await FinishAsync();
            return true;
        }

        public AlgoProcess SetContext(AlgorithmContext context)
        {
            Context = context;
            return this;
        }
    }
    public class OnErrorEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }

    public class InputRequestEventArgs : EventArgs
    {
        public bool InputReceived { get; set; } = false;
        public required List<BaseInputQAndA> InputQAndAs { get; set; }
    }

    public abstract class BaseInputQAndA
    {
    }

    public class ConsoleLogMessages
    {
        public ConsoleLogMessages(string invoker) 
        { 
            Invoker = invoker;
        }
        public string Invoker { get; set; }
        public List<string> LogRows { get; set; } = new();
        public void AddRow(string row)
        {
            LogRows.Add(row);
        }
    }
}

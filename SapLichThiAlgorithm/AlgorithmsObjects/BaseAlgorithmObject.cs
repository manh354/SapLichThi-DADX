using SapLichThiAlgorithm.ErrorAndLog;
using System.Reflection;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    public abstract class BaseAlgorithmObject
    {
        protected AlgorithmContext Context { get; set; }
        public BaseAlgorithmObject()
        {
            WithContext = new(this);
        }
        public BaseAlgorithmObjectWithContext SetContext(AlgorithmContext context)
        {
            Context = context;
            return WithContext;
        }

        protected void Run()
        {
            if (Context == null) { throw new ArgumentNullException("Null args exp"); }
            ReceiveInput(Context);
            CheckAllInput();
            InitializeAllOutput();
            ProcedureRun();
            CheckAllOutput();
            SendOutput(Context);
        }
        protected virtual void CheckAllInput()
        {
            try
            {
                var props = this.GetType().GetProperties();
                foreach (var prop in props)
                {
                    if (prop.Name.StartsWith("I_"))
                    {
                        if (prop.GetValue(this, null) == null)
                        {
                            if (!MatchData(prop, Context, this))
                            {
                                throw new Exception($"ERROR: INPUT not properly initialized: {prop.Name} at {this.GetType().Name}");
                            }
                            else
                            {
                                Logger.LogMessage($"WARNING: INPUT should be set in SetContext function: {prop.Name} at {this.GetType().Name}", LogType.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogMessage(e.Message, LogType.Error);
                throw;
            }
        }
        protected virtual void CheckAllOutput()
        {
            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name.StartsWith("O_"))
                {
                    if (prop.GetValue(this, null) == null)
                    {
                        throw new Exception($"OUTPUT not properly initialized {prop.Name} at {this.GetType().Name}");
                    }
                }
            }
        }
        protected abstract void ReceiveInput(AlgorithmContext context);
        protected abstract void SendOutput(AlgorithmContext context);
        protected abstract void InitializeAllOutput();
        protected abstract void ProcedureRun();
        private BaseAlgorithmObjectWithContext WithContext { get; set; }
        public class BaseAlgorithmObjectWithContext
        {
            private BaseAlgorithmObject BaseAlgorithmObject { get; set; }
            public BaseAlgorithmObjectWithContext(BaseAlgorithmObject baseAlgorithmObject)
            {
                BaseAlgorithmObject = baseAlgorithmObject;
            }
            public BaseAlgorithmObject Run()
            {
                BaseAlgorithmObject.Run();
                return BaseAlgorithmObject;
            }
        };


        public static bool MatchData<SendClass, ReceiveClass>(PropertyInfo propOfSender, SendClass sender, ReceiveClass receiver)
        {
            if (propOfSender == null || sender == null || receiver == null)
            {
                Logger.LogMessage("MatchData: One or more parameters are null", LogType.Error);
                return false;
            }

            try
            {
                // Safe logging with null checks
                var senderStr = sender?.ToString() ?? "null";
                var receiverStr = receiver?.ToString() ?? "null";
                Logger.LogMessage($"Matching property: {propOfSender.Name} from {senderStr} to {receiverStr}", LogType.Info);

                // Get property from receiver using actual runtime type instead of compile-time type
                var propOfReceiver = receiver.GetType().GetProperty(propOfSender.Name, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (propOfReceiver == null)
                {
                    // If not found in declared properties, try searching in base classes
                    propOfReceiver = receiver.GetType().GetProperty(propOfSender.Name,
                        BindingFlags.Public | BindingFlags.Instance);
                }

                if (propOfReceiver == null)
                {
                    Logger.LogMessage($"Property {propOfSender.Name} not found in receiver", LogType.Warning);
                    return false;
                }

                // Check if types are compatible
                if (!propOfReceiver.PropertyType.IsAssignableFrom(propOfSender.PropertyType))
                {
                    Logger.LogMessage($"Property types are incompatible: {propOfSender.PropertyType} -> {propOfReceiver.PropertyType}", LogType.Warning);
                    return false;
                }

                // Get value from sender
                var valueOfProp = propOfSender.GetValue(sender);
                
                // Set value to receiver
                propOfReceiver.SetValue(receiver, valueOfProp);
                
                Logger.LogMessage($"Successfully matched property {propOfSender.Name}", LogType.Info);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogMessage($"Error in MatchData: {e.Message}", LogType.Error);
                return false;
            }
        }

    }
}

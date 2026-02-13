using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiNew.Annealing;
using SapLichThiNew.ContextBuilding;
using SapLichThiNew.GeneralScheduling;
using SapLichThiNew.Preprocessings;
using SapLichThiNew.SettingsLogging;
using SapLichThiNew.StructuralBuildings;
using SapLichThiNew.TimefixedScheduling;
using SapLichThiStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew
{
    public class APOverall : AlgoProcess
    {
        private IDataSource _sourse;
        private ISchedulingModel _model;
        public override string GetProcessName()
        {
            return "Xếp lịch thi.";
        }
        public APOverall(IDataSource dataSource, ISchedulingModel model) 
        {
            _sourse = dataSource;
            _model = model;
        }
        protected override void BuildSubProcesses()
        {
            APContextBuilder contextBuilder = new(_sourse, _model);
            contextBuilder.SetContext(contextBuilder.Context);
            Context = contextBuilder.Context;
            AddSubProcess(contextBuilder);
            AddSubProcess(new APPreprocessor().SetContext(Context));
            AddSubProcess(new APTimeFixedScheduler().SetContext(Context));
            AddSubProcess(new APStructuralBuilder().SetContext(Context));
            AddSubProcess(new APGeneralScheduler().SetContext(Context));
            // AddSubProcess(new APGenetic().SetContext(Context));
            AddSubProcess(new APAnnealer().SetContext(Context));
            AddSubProcess(new APSettingsLogging().SetContext(Context));
        }
        protected override InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context)
        {
            return null;
        }

        protected override ConsoleLogMessages? CreateConsoleLog(AlgorithmContext context)
        {
            return null;
        }
    }
}

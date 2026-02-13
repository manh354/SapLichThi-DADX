using SapLichThiAlgorithm.AlgorithmsObjects;
using SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization.Evaluators;
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using SapLichThiCore.DataType;
using SapLichThiStream;
using SapLichThiStream.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiNew.ContextBuilding
{
    public sealed class APContextBuilder : AlgoProcess
    {
        public override string GetProcessName()
        {
            return "Lấy và lưu dữ liệu từ các file đã gửi vào chương trình.";
        }
        public APContextBuilder(IDataSource dataSource, ISchedulingModel schedulingModel)
        {
            // Register ITC2007ExamReader if it's being used
            if (dataSource is ITC2007ExamReader itcReader)
            {
                var inputDataType = dataSource.GetInputDataType();
                ITC2007Evaluator.RegisterReader(inputDataType, itcReader);
            }
            
            int i = 0;
            CompositeSettings.MaximumPercentage = schedulingModel.GetOptimalPercentage();
            Context = new()
            {
                I_inputDataType = dataSource.GetInputDataType(),
                I_rooms = dataSource.GetRooms(),
                I_allExamClasses = dataSource.GetAllExamClasses(),
                I_allRooms = dataSource.GetRooms(),
                I_biasTable = [1, 2, 3, 0, 4],
                I_largeAndMediumRoomCount = dataSource.GetRooms()
                .Sum(
                    x=>
                    x.RoomType == RoomType.small ? 0 :
                    x.RoomType == RoomType.medium ? 1 :
                    2
                ),
                I_largeAndMediumRooms = dataSource.GetRooms().Where(x=>x.RoomType == RoomType.large || x.RoomType == RoomType.medium).OrderByDescending(x=>x.Capacity).ToList(),
                I_spareRooms = dataSource.GetSpareRooms(),
                I_students = dataSource.GetStudents(),
                I_optimalRoomCoef = schedulingModel.GetOptimalPercentage(),
                I_examClass_validRoomsPenalties = schedulingModel.GetExamClassValidRooms(),
                I_examClass_validSlotsPenalties = schedulingModel.GetExamClassValidSlots(),
                I_room_validSlotsPenalties = schedulingModel.GetRoomValidSlots(),
                I_periods = schedulingModel.GetPeriods(),
                I_slot_penalties = schedulingModel.GetSlotPriority(),
                I_binaryConstraints = schedulingModel.GetBinaryConstraints(),
                I_unaryConstraints = schedulingModel.GetUnaryConstraints(),
                HardConstraint_DifferentRoomForCourses = schedulingModel.GetHardConstraints().HardConstraint_DifferentRoomForCourses,
                HardConstraint_LimitedCapacity = schedulingModel.GetHardConstraints().HardConstraint_LimitedCapacity,
            };
        }
        protected override void BuildSubProcesses()
        {
            // No subprocess required.
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

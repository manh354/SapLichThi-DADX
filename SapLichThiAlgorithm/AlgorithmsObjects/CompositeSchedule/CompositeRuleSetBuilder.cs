using AlgorithmExtensions;
using SapLichThiAlgorithm.AlgorithmsObjects.Optimization;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.CompositeSchedule
{

    public static class CompositeRuleSetBuilder
    {
        #region Puddle Comparer And Conditions

        public static KeySelector<Puddle, RuleBookExamClass> BuildExamClassPuddleRemainCapacityKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Puddle, RuleBookExamClass>(
                ruleBook,
                (puddle, rb) =>
                {
                    int keyValue = 0;
                    keyValue = -puddle.GetRemainingCapacity();
                    return keyValue;
                });
        }

        public static KeySelector<Puddle, RuleBookExamClass> BuildExamClassPuddleCapacityKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Puddle, RuleBookExamClass>(
                ruleBook,
                (puddle, rb) =>
                {
                    int keyValue = 0;
                    if (rb.LargestRoomFirst)
                    {
                        keyValue = puddle.GetRoomCapacity();
                    }
                    else
                    {
                        keyValue = -puddle.GetRoomCapacity();
                    }
                    return keyValue;
                });
        }

        /*public static KeySelector<Puddle, RuleBookExamClass> BuildExamClassPuddleSameCourseKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Puddle, RuleBookExamClass>(
                ruleBook,
                (puddle, rb) =>
                {
                    int keyValue = 0;
                    keyValue = Convert.ToInt32(puddle.Elements.Any(ec => ec.StudyClass.Course == rb.ExamClass.StudyClass.Course));
                    return keyValue;
                });
        }*/

        public static KeySelector<Puddle, RuleBookExamClass> BuildExamClassPuddlePriorityKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Puddle, RuleBookExamClass>(
                ruleBook,
                (puddle, rb) =>
                {
                    int keyValue = 0;
                    if (rb.PrimaryRoomFirst)
                    {
                        keyValue = Convert.ToInt32(!puddle.Room.IsSpare);
                    }
                    else
                    {
                        keyValue = 0;
                    }
                    return keyValue;
                }
                );
        }

        public static KeySelector<Puddle, RuleBookExamClass> BuildExamClassPuddleEmptyKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Puddle, RuleBookExamClass>(
                ruleBook,
                (puddle, rb) =>
                {
                    int keyValue = 0;
                    keyValue = Convert.ToInt32(puddle.IsEmpty());
                    return keyValue;
                });
        }


        public static Func<Puddle, RuleBookExamClass, bool> BuildExamClassPuddleSizeCondition()
        {
            return (Puddle puddle, RuleBookExamClass ruleBook) =>
            {
                return !ruleBook.HardConstraint_LimitedCapacity || puddle.GetRoomCapacity() * ruleBook.RelaxedCoef - puddle.GetUsedCapacity() >= ruleBook.ExamClass.Count;
            };
        }


        public static Func<Puddle, RuleBookExamClass, bool> BuildExamClassPuddleCourseRoomCondition()
        {
            return (Puddle puddle, RuleBookExamClass ruleBook) =>
            {
                bool result = true;
                if (!ruleBook.ExamClass_ValidRoomsPenalties.TryGetValue(ruleBook.ExamClass, out var rooms))
                {
                    result = true;
                }
                else if (!rooms.ContainsKey(puddle.Room))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
                return result;
            };
        }



        public static Func<Puddle, RuleBookExamClass, bool> BuildExamClassPuddleRoomSlotMapCondition()
        {
            return (Puddle puddle, RuleBookExamClass ruleBook) =>
            {
                bool result = true;
                if (!ruleBook.Room_ValidSlotsPenalties.TryGetValue(puddle.Room, out var suitableSlot))
                {
                    result = true;
                }
                else
                {
                    result = suitableSlot.ContainsKey(puddle.Period);
                }
                return result;
            };
        }


        public static PuddleRuleSet<RuleBookExamClass> BuildDefaultPuddleExamRuleSet(RuleBookExamClass ruleBook)
        {
            var sizeCondition = BuildExamClassPuddleSizeCondition();
            var roomCondition = BuildExamClassPuddleRoomSlotMapCondition();
            var roomCourseCondition = BuildExamClassPuddleCourseRoomCondition();
            var resultCondition =
                roomCondition   
                .And(roomCourseCondition)
                .And(sizeCondition);

            // var sameCourseKeySelector = BuildExamClassPuddleSameCourseKeySelector(ruleBook);
            var capacityKeySelector = BuildExamClassPuddleCapacityKeySelector(ruleBook);
            var remainCapacityKeySelector = BuildExamClassPuddleRemainCapacityKeySelector(ruleBook);
            var priorityKeySelector = BuildExamClassPuddlePriorityKeySelector(ruleBook);
            var resultKeySelector =
                priorityKeySelector
                .ThenBy(capacityKeySelector)
                .ThenBy(remainCapacityKeySelector);
            return new PuddleRuleSet<RuleBookExamClass>()
            {
                Condition = resultCondition,
                KeySelectors = resultKeySelector
            };
        }


        #endregion

        #region Pond KeySelector And Conditions

        public static KeySelector<Pond, RuleBookExamClass> BuildCourseLinkageKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Pond, RuleBookExamClass>(
                ruleBook,
                (pond, rb) =>
                {
                    if (rb.HardConstraint_NoStudentConflict)
                        return 1;
                    var linkages = rb.ExamClassLinkages[rb.ExamClass];
                    return pond.ExamClassesInPond.Where(c => linkages.Contains(c)).Count();
                });
        }

        /// <summary>
        /// BUILDS A KEY SELECTOR to prioritize PONDS with EMPTY SHIFTS FIRST.
        /// This function evaluates whether a pond has no courses in its pond.
        /// </summary>
        /// <param name="ruleBook">The RULEBOOK containing rules and constraints for the evaluation.</param>
        /// <returns>A KEY SELECTOR that assigns higher priority to ponds with empty shifts.</returns>
        public static KeySelector<Pond, RuleBookExamClass> BuildCoursePondEmptyShiftFirstKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Pond, RuleBookExamClass>(
                ruleBook,
                (pond, rb) =>
                {
                    int keyValue = 0;
                    keyValue = Convert.ToInt32(pond.ExamClassesInPond.Count == 0);
                    return keyValue;
                });
        }

        
        /// <summary>  
        /// Creates a key selector for evaluating the REMAINING CAPACITY of a pond.
        /// </summary>  
        /// <remarks>  
        /// The function takes a <see cref="RuleBookCourse"/> object as input, which provides contextual information  
        /// for the evaluation. The key selector uses the <see cref="Pond.GetRemainingCapacity"/> method to calculate the key value.  
        /// </remarks>  
        /// <example>  
        /// Example usage:  
        /// <list type="bullet">  
        /// <item>Sorting ponds to find the one with the most available capacity.</item>  
        /// <item>Filtering ponds based on their suitability for a course.</item>  
        /// </list>  
        /// </example>  
        /// <param name="ruleBook">A <see cref="RuleBookExamClass"/> object containing rules and constraints for the evaluation.</param>  
        /// <returns>A <see cref="KeySelector{Pond, RuleBookExamClass}"/> object that evaluates the remaining capacity of a pond.</returns>  
        public static KeySelector<Pond, RuleBookExamClass> BuildCoursePondRemainingCapacityKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Pond, RuleBookExamClass>(
                ruleBook,
                (pond, rb) =>
                {
                    int keyValue = 0;
                    keyValue = pond.GetRemainingCapacity();
                    return keyValue;
                });
        }

        public static KeySelector<Pond, RuleBookExamClass> BuildCourseSlotPriorityKeySelector(RuleBookExamClass ruleBook)
        {
            return new KeySelector<Pond, RuleBookExamClass>(
                ruleBook,
                (pond, rb) =>
                {
                    int keyValue = 0;
                    var slot = pond.Period;
                    if (rb.Slot_Penalties.TryGetValue(slot, out var priority))
                    {
                        keyValue = priority;
                    }
                    else
                    {
                        keyValue = 0; // Default priority if not found
                    }
                    return keyValue;
                });
        }

        public static Func<Pond, RuleBookExamClass, bool> BuildCoursePondMinimumSizeCondition()
        {
            return (Pond pond, RuleBookExamClass ruleBook) =>
            {
                bool result = true;
                if (pond.Puddles.Any(p => p.GetRemainingCapacity() >= ruleBook.ExamClass.Count))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                return result;
            };
        }

        public static Func<Pond, RuleBookExamClass, bool> BuildCoursePondLinkageCondition()
        {
            return (Pond pond, RuleBookExamClass ruleBook) =>
            {
                bool result = true;
                if (!ruleBook.HardConstraint_NoStudentConflict)
                {
                    return true;
                }
                if (!ruleBook.ExamClassLinkages.TryGetValue(ruleBook.ExamClass, out var linkages))
                {
                    result = true;
                }
                else if (pond.ExamClassesInPond.Any(linkages.Contains))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
                return result;
            };
        }

        public static Func<Pond, RuleBookExamClass, bool> BuildCoursePondSlotMapCondition()
        {
            return (Pond pond, RuleBookExamClass ruleBook) =>
            {
                bool result = true;
                var slot = pond.Period;
                if (!ruleBook.ExamClass_ValidSlotsPenalties.TryGetValue(ruleBook.ExamClass, out var validSLots))
                {
                    result = true;
                }
                else if (!validSLots.ContainsKey(slot))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
                return result;
            };
        }

        public static Func<Pond, RuleBookExamClass, bool> BuildDurationPondCondition()
        {
            return (Pond pond, RuleBookExamClass ruleBook) =>
            {
                bool result = pond.Period.Duration >= ruleBook.ExamClass.Duration;
                return result;
            };
        }


        public static PondRuleSet<RuleBookExamClass> BuildDefaultPondCourseRuleSet(RuleBookExamClass ruleBook)
        {
            var slotMapCondition = BuildCoursePondSlotMapCondition();
            var linkageCondition = BuildCoursePondLinkageCondition();
            var minimumSizeCondition = BuildCoursePondMinimumSizeCondition();
            var durationCondition = BuildDurationPondCondition();
            var resultCondition =
                slotMapCondition
                .And(linkageCondition)
                .And(minimumSizeCondition)
                .And(durationCondition);

            var slotPriorityKeySelector = BuildCourseSlotPriorityKeySelector(ruleBook);
            var emptyShiftFirstKeySelector = BuildCoursePondEmptyShiftFirstKeySelector(ruleBook);
            var capacityKeySelector = BuildCoursePondRemainingCapacityKeySelector(ruleBook);

            var resultKeySelector =
                (slotPriorityKeySelector)
                .ThenBy(emptyShiftFirstKeySelector)
                .ThenBy(capacityKeySelector);

            return new PondRuleSet<RuleBookExamClass>()
            {
                Condition = resultCondition,
                KeySelectors = resultKeySelector,
            };
        }

        #endregion

    }

    public static class BuilderExtend
    {
        #region Extension Methods

        public static PondRuleSet<RuleBookExamClass> BuildDefaultPondCourseRuleSet(this RuleBookExamClass ruleBook)
        {
            return CompositeRuleSetBuilder.BuildDefaultPondCourseRuleSet(ruleBook);
        }

        public static PuddleRuleSet<RuleBookExamClass> BuildDefaultPuddleExamRuleSet(this RuleBookExamClass ruleBook)
        {
            return CompositeRuleSetBuilder.BuildDefaultPuddleExamRuleSet(ruleBook);
        }

        public static RuleBookExamClass WithHardConstraintsFromContext(this RuleBookExamClass ruleBook, AlgorithmContext context)
        {
            ruleBook.HardConstraint_DifferentRoomForCourses = context.HardConstraint_DifferentRoomForCourses;
            ruleBook.HardConstraint_LimitedCapacity = context.HardConstraint_LimitedCapacity;
            ruleBook.HardConstraint_NoStudentConflict = context.HardConstraint_NoStudentConflict;
            ruleBook.HardConstraint_OnlyOneExamClassPerRoom = context.HardConstraint_OnlyOneExamClassPerRoom;
            return ruleBook;
        }

        #endregion
    }

}


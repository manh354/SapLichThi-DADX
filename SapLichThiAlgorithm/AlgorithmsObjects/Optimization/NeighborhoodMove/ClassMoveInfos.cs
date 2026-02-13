using SapLichThiCore.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.NeighborhoodMove
{
    public class ClassMoveInfos : IDictionary<ExamClass, ClassMoveInfo>
    {
        private Dictionary<ExamClass, ClassMoveInfo> internalDictionary = new();

        public ICollection<ExamClass> Keys => internalDictionary.Keys;

        public ICollection<ClassMoveInfo> Values => internalDictionary.Values;

        public int Count => internalDictionary.Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<ExamClass, ClassMoveInfo>>)internalDictionary).IsReadOnly;

        public ClassMoveInfo this[ExamClass key] { get => internalDictionary[key]; set => internalDictionary[key] = value; }

        public void AddMoveInfo(ExamClass examClass, List<(Period period, Room? room)> startSlot, List<(Period period, Room? room)> endSlot)
        {
            var moveInfo = new ClassMoveInfo
            {
                ExamClass = examClass,
                StartPositions = startSlot,
                EndPositions = endSlot
            };

            if (!internalDictionary.TryAdd(examClass, moveInfo))
            {
                internalDictionary[examClass] = moveInfo;
            }
        }

        public void AddStartMoveInfo(ExamClass examClass, List<(Period period, Room? room)> startSlot)
        {
            if (internalDictionary.TryGetValue(examClass, out var existingInfo))
            {
                existingInfo.StartPositions = startSlot;
            }
            else
            {
                var moveInfo = new ClassMoveInfo
                {
                    ExamClass = examClass,
                    StartPositions = startSlot
                };
                internalDictionary.Add(examClass, moveInfo);
            }
        }

        public void AddEndMoveInfo(ExamClass examClass, List<(Period period, Room? room)> endSlot)
        {
            if (internalDictionary.TryGetValue(examClass, out var existingInfo))
            {
                existingInfo.EndPositions = endSlot;
            }
            else
            {
                var moveInfo = new ClassMoveInfo
                {
                    ExamClass = examClass,
                    EndPositions = endSlot
                };
                internalDictionary.Add(examClass, moveInfo);
            }
        }

        public void Add(ExamClass key, ClassMoveInfo value)
        {
            internalDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<ExamClass, ClassMoveInfo> item)
        {
            ((ICollection<KeyValuePair<ExamClass, ClassMoveInfo>>)internalDictionary).Add(item);
        }

        public void Clear()
        {
            internalDictionary.Clear();
        }

        public bool Contains(KeyValuePair<ExamClass, ClassMoveInfo> item)
        {
            return ((ICollection<KeyValuePair<ExamClass, ClassMoveInfo>>)internalDictionary).Contains(item);
        }

        public bool ContainsKey(ExamClass key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<ExamClass, ClassMoveInfo>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<ExamClass, ClassMoveInfo>>)internalDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<ExamClass, ClassMoveInfo>> GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        public bool Remove(ExamClass key)
        {
            return internalDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<ExamClass, ClassMoveInfo> item)
        {
            return ((ICollection<KeyValuePair<ExamClass, ClassMoveInfo>>)internalDictionary).Remove(item);
        }

        public bool TryGetValue(ExamClass key, [MaybeNullWhen(false)] out ClassMoveInfo value)
        {
            return internalDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }
    }

    public class ClassMoveInfo
    {
        public ExamClass ExamClass;
        public List<(Period period, Room? room)> StartPositions { get; set; } = new();
        public List<(Period period, Room? room)> EndPositions { get; set; } = new();
    }
}

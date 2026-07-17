using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cadi.Scripts.EventSystem
{
    public delegate void EventListener<in T>(T evt) where T : Event;

    [Serializable]
    public class Event : IDisposable
    {
        public EventResult Result = EventResult.None;
        public object Target = null;
        public bool IsConsumed = false;

        public Event()
        {
        }

        public void Consume()
        {
            IsConsumed = true;
        }

        public virtual void Dispose()
        {
        }
    }

    public abstract class Event<T> : Event where T : Event<T>, new()
    {
        private const int c_PoolSizeWarningThreshold = 20;

        private static readonly Stack<T> s_Pool = new Stack<T>();
        private static int s_CreatedObjectsCount;
        private static bool s_HasLoggedPoolWarning;

        private bool m_IsInPool;

        public static T Rent()
        {
            return RentPooledInternal();
        }

        protected static T RentPooledInternal()
        {
            if (s_Pool.Count > 0)
            {
                var evt = s_Pool.Pop();
                evt.m_IsInPool = false;
                return evt;
            }

            var newEvent = new T();
            s_CreatedObjectsCount++;

            if (!s_HasLoggedPoolWarning && s_CreatedObjectsCount >= c_PoolSizeWarningThreshold)
            {
                s_HasLoggedPoolWarning = true;

                Debug.LogWarning(
                    $"{typeof(T).Name} pool size exceeded warning threshold. Created objects count: {s_CreatedObjectsCount}");
            }

            return newEvent;
        }

        private static void Return(T evt)
        {
            if (evt.m_IsInPool)
                throw new InvalidOperationException($"{typeof(T).Name} was returned to the pool more than once.");

            evt.Reset();

            evt.m_IsInPool = true;
            s_Pool.Push(evt);
        }

        protected virtual void Reset()
        {
            Result = EventResult.None;
            Target = null;
            IsConsumed = false;
        }

        public override void Dispose()
        {
            Return((T)this);
        }
    }

    public enum EventResult
    {
        None = 0,
        Success = 1,
        Fail = 2,
    }

    public enum Priority
    {
        Critical = 0,
        VeryHigh = 1,
        High = 2,
        Normal = 3,
        Low = 4,
        VeryLow = 5,
        Lowest = 6,
    }
}
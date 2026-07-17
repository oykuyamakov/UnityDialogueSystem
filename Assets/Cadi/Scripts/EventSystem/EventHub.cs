using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cadi.Scripts.EventSystem
{
    public class EventHub
    {
        private interface IListenerList
        {
            void SendEvent(Event evt);
            bool IsEmpty { get; }
        }

        private sealed class ListenerList<T> : IListenerList where T : Event
        {
            private readonly PriorityList<EventListener<T>> m_Listeners = new();

            private int m_DispatchDepth;

            private List<PendingAdd> m_PendingAdds;
            private List<EventListener<T>> m_PendingRemoves;

            public bool IsEmpty => m_Listeners.Count == 0 &&
                                   (m_PendingAdds == null || m_PendingAdds.Count == 0);

            private readonly struct PendingAdd
            {
                public readonly EventListener<T> Listener;
                public readonly int Priority;

                public PendingAdd(EventListener<T> listener, int priority)
                {
                    Listener = listener;
                    Priority = priority;
                }
            }

            public bool Add(EventListener<T> listener, Priority priority)
            {
                if (listener == null)
                    throw new ArgumentNullException(nameof(listener));

                if (m_DispatchDepth > 0)
                {
                    // If listener was scheduled for removal during this dispatch,
                    // adding it again cancels the removal.
                    if (RemovePendingRemove(listener))
                        return true;

                    if (m_Listeners.Contains(listener) || ContainsPendingAdd(listener))
                        return false;

                    (m_PendingAdds ??= new List<PendingAdd>()).Add(new PendingAdd(listener, (int)priority));
                    return true;
                }

                return m_Listeners.AddUnique(listener, (int)priority);
            }

            public bool Remove(EventListener<T> listener)
            {
                if (listener == null)
                    throw new ArgumentNullException(nameof(listener));

                if (m_DispatchDepth > 0)
                {
                    // If it was added during this same dispatch but not applied yet,
                    // removing it just cancels the pending add.
                    if (RemovePendingAdd(listener))
                        return true;

                    if (!m_Listeners.Contains(listener) || ContainsPendingRemove(listener))
                        return false;

                    (m_PendingRemoves ??= new List<EventListener<T>>()).Add(listener);
                    return true;
                }

                return m_Listeners.Remove(listener);
            }

            public void SendEvent(Event evt)
            {
                var typedEvent = (T)evt;

                m_DispatchDepth++;

                try
                {
                    for (int i = 0; i < m_Listeners.Count; i++)
                    {
                        var listener = m_Listeners[i];

                        if (ContainsPendingRemove(listener))
                            continue;

                        try
                        {
                            listener(typedEvent);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }

                        if (typedEvent.IsConsumed)
                            break;
                    }
                }
                finally
                {
                    m_DispatchDepth--;

                    if (m_DispatchDepth == 0)
                        ApplyPendingOperations();
                }
            }

            private void ApplyPendingOperations()
            {
                if (m_PendingRemoves != null && m_PendingRemoves.Count > 0)
                {
                    for (int i = 0; i < m_PendingRemoves.Count; i++)
                        m_Listeners.Remove(m_PendingRemoves[i]);

                    m_PendingRemoves.Clear();
                }

                if (m_PendingAdds != null && m_PendingAdds.Count > 0)
                {
                    for (int i = 0; i < m_PendingAdds.Count; i++)
                    {
                        var pendingAdd = m_PendingAdds[i];
                        m_Listeners.AddUnique(pendingAdd.Listener, pendingAdd.Priority);
                    }

                    m_PendingAdds.Clear();
                }
            }

            private bool ContainsPendingAdd(EventListener<T> listener)
            {
                if (m_PendingAdds == null)
                    return false;

                for (int i = 0; i < m_PendingAdds.Count; i++)
                {
                    if (EqualityComparer<EventListener<T>>.Default.Equals(m_PendingAdds[i].Listener, listener))
                        return true;
                }

                return false;
            }

            private bool ContainsPendingRemove(EventListener<T> listener)
            {
                if (m_PendingRemoves == null)
                    return false;

                for (int i = 0; i < m_PendingRemoves.Count; i++)
                {
                    if (EqualityComparer<EventListener<T>>.Default.Equals(m_PendingRemoves[i], listener))
                        return true;
                }

                return false;
            }

            private bool RemovePendingAdd(EventListener<T> listener)
            {
                if (m_PendingAdds == null)
                    return false;

                for (int i = 0; i < m_PendingAdds.Count; i++)
                {
                    if (!EqualityComparer<EventListener<T>>.Default.Equals(m_PendingAdds[i].Listener, listener))
                        continue;

                    int lastIndex = m_PendingAdds.Count - 1;
                    m_PendingAdds[i] = m_PendingAdds[lastIndex];
                    m_PendingAdds.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }

            private bool RemovePendingRemove(EventListener<T> listener)
            {
                if (m_PendingRemoves == null)
                    return false;

                for (int i = 0; i < m_PendingRemoves.Count; i++)
                {
                    if (!EqualityComparer<EventListener<T>>.Default.Equals(m_PendingRemoves[i], listener))
                        continue;

                    int lastIndex = m_PendingRemoves.Count - 1;
                    m_PendingRemoves[i] = m_PendingRemoves[lastIndex];
                    m_PendingRemoves.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }
        }

        private readonly Dictionary<Type, IListenerList> m_ListenersByType = new Dictionary<Type, IListenerList>();

        public bool IsEmpty => m_ListenersByType.Count == 0;

        private ListenerList<T> GetOrCreateListenerList<T>() where T : Event
        {
            var type = typeof(T);

            if (m_ListenersByType.TryGetValue(type, out var bucket))
                return (ListenerList<T>)bucket;

            var created = new ListenerList<T>();
            m_ListenersByType[type] = created;
            return created;
        }

        public bool AddListener<T>(EventListener<T> listener, Priority priority = Priority.Normal)
            where T : Event
        {
            return GetOrCreateListenerList<T>().Add(listener, priority);
        }

        public bool RemoveListener<T>(EventListener<T> listener)
            where T : Event
        {
            var type = typeof(T);

            if (!m_ListenersByType.TryGetValue(type, out var listeners))
                return false;

            var removed = ((ListenerList<T>)listeners).Remove(listener);

            if (listeners.IsEmpty)
                m_ListenersByType.Remove(type);

            return removed;
        }

        public void SendEvent<T>(T evt) where T : Event
        {
            SendEvent((Event)evt);
        }

        private void SendEvent(Event evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var type = evt.GetType();

            if (!m_ListenersByType.TryGetValue(type, out var listeners))
                return;

            listeners.SendEvent(evt);

            if (listeners.IsEmpty)
                m_ListenersByType.Remove(type);
        }
    }
}
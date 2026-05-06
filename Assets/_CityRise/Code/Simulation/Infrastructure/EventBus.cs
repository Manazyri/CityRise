#nullable enable

using System;
using System.Collections.Generic;

namespace CityRise.Simulation.Infrastructure;

/// <summary>
/// Queued, type-strict pub/sub. Events are queued during a sim tick and dispatched on
/// <see cref="Flush"/> at the tick boundary so subscribers never observe torn state
/// (Tech Roadmap section 4.3).
/// </summary>
/// <remarks>
/// Each event type gets its own typed queue, so structs never box into a shared list and
/// the GC stays quiet at tick cadence. Subscriptions are <c>Action&lt;T&gt;</c> per event type.
/// No reflection.
/// </remarks>
public sealed class EventBus
{
    private readonly Dictionary<Type, IEventQueue> _queues = new();
    private readonly List<IEventQueue> _queuesWithPending = new();

    /// <summary>Subscribe to events of type <typeparamref name="T"/>. Multiple handlers per type are supported.</summary>
    public void Subscribe<T>(Action<T> handler) where T : struct, IEvent
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        var queue = GetOrCreate<T>();
        queue.Handler += handler;
    }

    /// <summary>Unsubscribe a previously-registered handler.</summary>
    public void Unsubscribe<T>(Action<T> handler) where T : struct, IEvent
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        if (_queues.TryGetValue(typeof(T), out var queue))
        {
            ((TypedQueue<T>)queue).Handler -= handler;
        }
    }

    /// <summary>Queue an event. Subscribers fire on the next <see cref="Flush"/>.</summary>
    public void Publish<T>(in T evt) where T : struct, IEvent
    {
        var queue = GetOrCreate<T>();
        if (queue.Pending.Count == 0)
        {
            _queuesWithPending.Add(queue);
        }
        queue.Pending.Add(evt);
    }

    /// <summary>Drain every queue, dispatching each pending event to its subscribers.</summary>
    /// <remarks>
    /// Snapshot the active count so handlers that re-publish during dispatch don't fire mid-flush.
    /// After dispatch, queues that still have pending events (from re-publishes) stay registered for
    /// the next flush; everything we drained is removed.
    /// </remarks>
    public int Flush()
    {
        var dispatched = 0;
        var n = _queuesWithPending.Count;
        for (int i = 0; i < n; i++)
        {
            dispatched += _queuesWithPending[i].FlushQueue();
        }
        // Walk the dispatched range; drop queues that emptied, retain those with re-published items.
        var write = 0;
        for (int read = 0; read < n; read++)
        {
            if (_queuesWithPending[read].PendingCount > 0)
            {
                _queuesWithPending[write++] = _queuesWithPending[read];
            }
        }
        // Slide any newly-published queues (added during dispatch at indices >= n) down to fill the gap.
        for (int read = n; read < _queuesWithPending.Count; read++)
        {
            _queuesWithPending[write++] = _queuesWithPending[read];
        }
        _queuesWithPending.RemoveRange(write, _queuesWithPending.Count - write);
        return dispatched;
    }

    /// <summary>Drop all pending events without dispatch. Test helper.</summary>
    public void ClearPending()
    {
        for (int i = 0; i < _queuesWithPending.Count; i++)
        {
            _queuesWithPending[i].Clear();
        }
        _queuesWithPending.Clear();
    }

    /// <summary>Number of queued events for type <typeparamref name="T"/>.</summary>
    public int PendingCount<T>() where T : struct, IEvent
        => _queues.TryGetValue(typeof(T), out var queue) ? ((TypedQueue<T>)queue).Pending.Count : 0;

    private TypedQueue<T> GetOrCreate<T>() where T : struct, IEvent
    {
        if (!_queues.TryGetValue(typeof(T), out var queue))
        {
            queue = new TypedQueue<T>();
            _queues[typeof(T)] = queue;
        }
        return (TypedQueue<T>)queue;
    }

    private interface IEventQueue
    {
        int FlushQueue();
        void Clear();
        int PendingCount { get; }
    }

    private sealed class TypedQueue<T> : IEventQueue where T : struct, IEvent
    {
        public readonly List<T> Pending = new();
        public Action<T>? Handler;

        public int PendingCount => Pending.Count;

        public int FlushQueue()
        {
            var count = Pending.Count;
            if (Handler is null)
            {
                Pending.Clear();
                return count;
            }

            // Snapshot length so handlers that re-publish during dispatch don't fire this round.
            var n = Pending.Count;
            for (int i = 0; i < n; i++)
            {
                Handler(Pending[i]);
            }

            // Drop only what we dispatched; re-published events stay queued for the next flush.
            Pending.RemoveRange(0, n);
            return count;
        }

        public void Clear() => Pending.Clear();
    }
}

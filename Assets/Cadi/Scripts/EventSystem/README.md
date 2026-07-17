# Event System Summary

> Custom channel based event manager (`EM`).
>
> This system lets different parts of the game communicate through typed event objects without directly referencing each other.

---

## Core Files

- `Event.cs` - event base types, pooling, global/context send helpers,
- `EventHub.cs` - stores listeners by event type and dispatches 
- `PriorityList.cs` - 

---

# 1. `Event.cs`

## `Event`

Base event class.

Each event has:

- `Result` - stores the result of the event: `None`, `Success`, or `Fail`
- `Target` - optional object reference, usually used to say what this event is about
- `IsConsumed` - tells the dispatcher whether this event should stop propagating
- `Consume()` - marks the event as handled and stops lower-priority listeners from receiving it
- `Dispose()` - virtual base dispose function, empty by default

`Consume()` is useful when the event should only be handled by one system.

For example, if an interaction event is handled by the dialogue system, we may not want the pickup system or default interaction system to also process it.

---

## `Event<T>`

Generic pooled event base class.

```csharp
public abstract class Event<T> : Event where T : Event<T>, new()
```
- `Event<T>` adds pooling behavior to the base `Event`.
- Pooling is used to reduce GC allocations.
- Instead of creating a new event object every time, the system can reuse old instances from a static pool.
- Each concrete event type gets its own static pool.

### Flow

- `Rent()` - gets event from pool or creates a new one
- `Send()` - dispatched via `EM.SendEvent()` or extensions like `evt.SendGlobal()`
- `Dispose()` - for pooled events, dispose means return this instance to its static pool

---
### `IDisposable`

`Event<T>` uses `IDisposable` to control when the event is disposed. 
Garbage Collector cleans objects when nothing references them anymore, but not known exactly when.
So `IDisposable` to control when the event is released.

`Dispose()` does not destroy the object manually.
`Dispose()` returns the event instance to its static pool.
```csharp
public override void Dispose() { Return((T)this); }
```

### Usage 1: `using var`
```csharp
using var evt = MyEvent.Rent();
evt.SendGlobal();
```
is actually:
```csharp
var evt = MyEvent.Rent();
try { evt.SendGlobal(); }
finally { evt.Dispose(); }
```

- It is disposed at the end of the scope it was called from.
- If an exception is thrown, `finally` still calls `Dispose()`.
- This is useful when the event can live until the end of the current method/scope.

---

### Usage 2: `using (...)`
```csharp
using (var evt = MyEvent.Rent()) { evt.SendGlobal(); }
```
- It is disposed at the end of its own block.
- Use this for a tighter lifetime.
- This makes the event lifetime more obvious.

---

### Important Event Functions

### `Reset()` resets the base event state:

- `Result = EventResult.None`
- `Target = null`
- `IsConsumed = false`

It is called from `Return()` when an event is taken from the pool.

This makes sure the event is in a clean state before it is reused.
- `Reset()` is `protected`
- so it cannot be called manually from outside
- but derived event classes can override it


### `Consume()` marks the event as handled.

```csharp
public void Consume() { IsConsumed = true; }
```

During dispatch, after each listener is called, the system checks:

```csharp
if (typedEvent.IsConsumed) break;
```

So if one listener consumes the event, the event stops propagating to the remaining lower-priority listeners.

Use `Consume()` when the event should only be handled by one system.

Good examples:
- input events
- interaction request events
- dialogue request events

Or in certain cases, if a certain condition is met, consume the event so that the remaning listeners won't process it.

---

###  `EventResult`

Simple enum for storing the result of an event.

```csharp
public enum EventResult { None = 0, Success = 1, Fail = 2, }
```
Use this when the sender wants to inspect what happened after the event was processed.

`Result` and `Consume()` are different.

| Function | Meaning |
|---|---|
| `Result` | What happened? |
| `Consume()` | Should the event stop propagating? |

---

### `Priority`
Listener priority enum.

```csharp
public enum Priority { Critical = 0, VeryHigh = 1, High = 2,
    Normal = 3, Low = 4, VeryLow = 5, Lowest = 6, }
```
> `Lower number means higher priority. Higher-priority listeners receive the event first.`

> `If a high-priority listener calls `Consume()`, lower-priority listeners will not receive the event.`

Example:

```csharp
EM.AddListener<InputEvent>(OnUIInput, Priority.Critical);
EM.AddListener<InputEvent>(OnPlayerInput, Priority.Normal);
EM.AddListener<InputEvent>(OnInputSFX, Priority.Low);
```
Dispatch order:

1. `OnUIInput`
2. `OnPlayerInput`
3. `OnInputSFX`

If `OnUIInput()` consumes the event, the other two listeners will not run.

---

# 2. `EM`

Static event manager. `EM` is the main event bus of the system. Internally, it stores listeners like this:

```csharp
private static readonly Dictionary<object, Dictionary<int, EventListenerCollection>> s_ChannelsByContext;
```

Conceptually:

```text
Context
    Channel
        EventListenerCollection
            Event Type
                Listeners
```

So every listener belongs to:
- a context object
- a channel integer
- an event type

This lets the same event type be used in different scopes without every listener hearing every event.

---

### Main `EM` Functions

| Function | Purpose |
|---|---|
| `AddListener<T>()` | Register a listener |
| `RemoveListener<T>()` | Unregister a listener |
| `SendEvent<T>()` | Send an event |
| `ClearChannel()` | Clear one channel in one context |
| `ClearContext()` | Clear all channels/listeners under one context |
| `ClearAll()` | Clear the entire event system |

---

### `AddListener<T>()`
Adds a listener for a specific event type.

```csharp
EM.AddListener<MyEvent>(OnMyEvent); /
    EM.AddListener<MyEvent>(OnMyEvent, Priority.Normal, context, channel);
```

### `RemoveListener<T>()`
Removes a listener for a specific event type.

```csharp
EM.RemoveListener<MyEvent>(OnMyEvent);
```
This is important because `EM` is static. If listeners are not removed when objects are destroyed or disabled, old object references can stay registered.

---

### `SendEvent<T>()`
Sends an event to listeners.

```csharp
EM.SendEvent(evt); / EM.SendEvent(evt, context, channel);
```

#### Flow:

1. If context is `null`, use `GlobalContext`.
2. Find the `EventListenerCollection` for this context and channel.
3. If no listener collection exists, return the event.
4. Save the old `CurrentContext`.
5. Set `CurrentContext` to the active context.
6. Dispatch the event.
7. Restore the old `CurrentContext`.
8. Return the same event instance.

It returns the same event object so the sender can inspect `Result`, `Target`, or custom fields after dispatch.

---

### Context

A context is an object used to separate event groups.

If no context is given, the system uses: `EM.GlobalContext`

So, `EM.SendEvent(evt);`means: Send this event to the global context.

But this: `EM.SendEvent(evt, player);` means: Send this event only inside the player context.

Context is very useful when different systems, scenes, objects, or gameplay groups need isolated event communication.

Example:
```text
Player A inventory events
Player B inventory events
NPC inventory events
```
They can all use `InventoryChangedEvent`, but different contexts keep them isolated.

---

### Channel

A channel is an integer used to further separate events inside the same context.

Example usage:

```csharp
EM.SendEvent(evt, context, 2); (channel = 2)
```

Default channel: `public const int C_DefaultChannel = -1;`
So if no channel is provided, the event goes to the default channel.

Only listeners registered to the same context and same channel receive it. Channels are useful when one context needs multiple separate event streams.

Example:

```text
Same context:
    channel -1 = default gameplay events
    channel 0 = UI events
    channel 1 = dialogue events
    channel 2 = debug events
```
---
###  `CurrentContext`

`CurrentContext` stores the context currently being used during dispatch.

```csharp
public static object CurrentContext { get; private set; } = GlobalContext;
```

When `SendEvent()` starts:

1. Save old `CurrentContext`.
2. Set `CurrentContext` to the new context.
3. Dispatch event.
4. Restore old `CurrentContext` inside `finally`.

This is useful because systems can check what context is currently dispatching.

It also protects the old context even if something goes wrong during event dispatch.

---

### Clear Functions

### `ClearChannel()`

Clears one channel inside one context.

```csharp
EM.ClearChannel(context, channel);
```

Use this when only one channel should be removed.

---

### `ClearContext()`

Clears all channels and listeners under one context.

```csharp
EM.ClearContext(context);
```

Useful when unloading a scene, destroying a gameplay group, or resetting a context.

---

### `ClearAll()`

Clears the entire event system.

```csharp
EM.ClearAll();
```

Also resets `CurrentContext` back to `GlobalContext`.

Useful for tests, full reset, or scene/system cleanup.

---

## `EventExtensions.cs`

Convenience extension methods around `EM`. These methods make event usage shorter and cleaner.

### Global Event Helpers

### `evt.SendGlobal()`

```csharp
using (var evt = MyEvent.Rent()) { evt.SendGlobal(); }
```
This sends the existing event instance to the global context.
Equivalent to:

```csharp
EM.SendEvent(evt, EM.GlobalContext);
```

---

### `EventExtensions.SendGlobal<T>()`

```csharp
EventExtensions.SendGlobal<MyEvent>();
```

using (var evt = MyEvent.Rent()){ evt.SendEvent(context, channel); }
instead of:
using (var evt = MyEvent.Rent()){ EM.SendEvent(evt, context, channel); }

---

# 3. `Event Hub`

Stores listeners by event type. Inside one context and one channel, `EventListenerCollection` stores different listener lists for different event types.

Internally:

```csharp
private readonly Dictionary<Type, IListenerList> m_ListenersByType;
```

Conceptually:

```text
DamageEvent -> DamageEvent listeners
InteractEvent -> InteractEvent listeners
DialogueEvent -> DialogueEvent listeners
```
Important detail:
> Dispatch uses the exact runtime type of the event.
Listeners registered for base `Event` do not automatically receive all event types.
---

### `ListenerList<T>`
Nested generic listener list for one event type.

It stores:
```csharp
private readonly PriorityList<EventListener<T>> m_Listeners = new();
```
So each event type has its own priority-sorted listener list.

## Dispatch Flow

When an event is sent:

1. Cast base `Event` to the correct event type `T`.
2. Increase dispatch depth.
3. Loop over listeners in priority order.
4. Skip listeners that are pending removal.
5. Call each listener.
6. Catch and log exceptions so one broken listener does not stop the whole dispatch.
7. If event is consumed, stop dispatch.
8. Decrease dispatch depth.
9. If dispatch is fully finished, apply pending adds/removes.

This makes dispatch safer than directly modifying a listener list while iterating over it.

---

# 15. Safe Add/Remove During Dispatch
This is one of the most important parts of the system.
If a listener is added while an event is currently being dispatched, it is not added immediately.
Instead it goes into:
```csharp
m_PendingAdds
```
If a listener is removed while an event is currently being dispatched, it is not removed immediately.
Instead it goes into:

```csharp
m_PendingRemoves
```
After dispatch finishes, `ApplyPendingOperations()` applies the changes.
This prevents bugs caused by modifying the listener list while looping through it.
---

## Why `m_DispatchDepth` Exists

`m_DispatchDepth` tracks whether the system is currently dispatching.
If:

```csharp
m_DispatchDepth > 0
```

then add/remove operations are delayed. This also supports nested dispatch.

Example:

```text
Listener A receives EventA.
Listener A sends EventB.
EventB dispatch starts before EventA dispatch fully ends.
```

`m_DispatchDepth` makes sure pending operations are only applied when the outermost dispatch is finished.

---

## Duplicate Protection

Listeners are added through `AddUnique()`.
This prevents the same listener from being added more than once to the same event type/list.
During dispatch, the system also checks pending adds/removes to avoid duplicate or conflicting operations.

---

# 4. `PriorityList<T>`
Custom array-backed list that stores items with an integer priority.
Each entry stores:
- `Item`
- `Priority`

## How Priority Sorting Works

When a new listener is added, the list searches for the correct insert position.
Lower priority value gets inserted earlier.

So:

```text
Critical = 0
Normal = 3
Lowest = 6
```
means:
```text
Critical runs before Normal.
Normal runs before Lowest.
```

The list stays sorted when listeners are added.
It does not need to sort the whole list every time dispatch happens.

---

### `AddUnique()`

Adds an item only if it is not already in the list.

```csharp
public bool AddUnique(T item, int priority)
```

Returns:

- `true` if the item was added
- `false` if the item already existed

This prevents duplicate listener registration.

---

### `Remove()`

Removes an item from the list. It searches the list, removes the matching item, shifts the remaining items, and clears the freed slot.
This keeps the internal array clean and avoids stale references.

---

### `EnsureCapacity()` and `Grow()`

The list starts with an empty array. When it needs more space, it grows:

```text
0 -> 4 -> 8 -> 16 -> ...
```

This avoids creating a new array for every single add.

---

### Enumerator

`PriorityList<T>` has a custom enumerator.
It stores a version number.
If the list is modified during enumeration, the enumerator throws an exception.

This is standard collection safety behavior.

In event dispatch, listeners are mostly accessed by index instead of using `foreach`, which gives more control over priority iteration and pending removals.

---

#  Overall Flow

Full event flow:

1. A system rents or creates an event.
2. The event is filled with data.
3. The event is sent through `EM`.
4. `EM` finds listeners by context and channel.
5. `EventListenerCollection` finds listeners by exact event type.
6. `ListenerList<T>` dispatches listeners in priority order.
7. A listener may set `Result`.
8. A listener may call `Consume()` to stop propagation.
9. Add/remove calls during dispatch are delayed safely.
10. After dispatch, the event is disposed.
11. `Dispose()` returns the event to its static pool.

---

This event system is basically:

```text
Typed event payloads
+ pooled event objects
+ global/context/channel routing
+ priority-based listener order
+ safe listener mutation during dispatch
+ optional event consumption
```

It is more controlled than a simple event bus because events can be scoped by context/channel and listeners can be ordered by priority.

---

# Usage Rules
[README.md](../../../../README.md)
## Listener Rules

- If you `AddListener`, make sure you `RemoveListener` when the object no longer needs it.
- This is especially important because `EM` is static.
- Static event systems can keep references alive if listeners are not removed.

---

## Event Rules

- If you `Rent()` an event manually, use `using` or manually call `Dispose()`.
- Do not use an event after it has been disposed.
- Override `Reset()` when the event has custom fields that need to be cleared.

---

## `Consume()` Rules

Use `Consume()` if you dont want the other listeners with lower/equal priority not receive the event after that point.
---

# 20. Minimal Usage Example

## Define Event

```csharp
public class DamageEvent : Event<DamageEvent>
{
    public int Damage;
    public object Source;

    protected override void Reset()
    {
        base.Reset();

        Damage = 0;
        Source = null;
    }
}
```

---

## Add Listener

```csharp
private void OnEnable()
{
    EM.AddListenerGlobal<DamageEvent>(OnDamageEvent, Priority.Normal);
}
```

---

## Remove Listener

```csharp
private void OnDisable()
{
    EM.RemoveListenerGlobal<DamageEvent>(OnDamageEvent);
}
```

---

## Handle Event

```csharp
private void OnDamageEvent(DamageEvent evt)
{
    if (evt.Target == null)
    {
        evt.Result = EventResult.Fail;
        evt.Consume();
        return;
    }

    ApplyDamage(evt.Target, evt.Damage);

    evt.Result = EventResult.Success;
    evt.Consume();
}
```

---

## Send Event

```csharp
using (var evt = DamageEvent.Rent())
{
    evt.Target = enemy;
    evt.Damage = 10;
    evt.Source = player;

    evt.SendGlobal();

    if (evt.Result == EventResult.Success)
    {
        Debug.Log("Damage handled.");
    }
}
```

---

# Final Summary

The event system is a typed, pooled, priority-based event bus.

It lets systems communicate without direct references, while still giving control over:

- who receives the event
- in what order listeners run
- whether the event should stop propagating
- whether the event should be global or context/channel scoped
- when event objects return to the pool

The most important rules are:

1. Use `using` when renting pooled events.
2. Remove listeners when they are no longer needed.
3. Override `Reset()` for custom event data.
4. Use `Consume()` only when the event should stop after being handled.
5. Use context/channel when global events would be too broad.
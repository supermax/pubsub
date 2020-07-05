Main Thread dispatcher is responsible for the synchronisation of callbacks between Main and other threads.

### MainThreadDispatcher implements this interface :

```csharp
public interface IThreadDispatcher
{
    // managed Thread ID
    int ThreadId { get; }

    // dispatch - adds callback delegate into dispatcher’s queue
    // action - delegate reference to method that should be invoked on main thread
    // payload - the data that should be passed to the method/callback
    void Dispatch(Delegate action, object[] payload);
}
```

### Dispatch Method - example 

```csharp
MainThreadDispatcher.Default.Dispatch(Callback, new object[] { payload, state });
```
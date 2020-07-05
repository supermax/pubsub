### Messenger implements this interface:

```csharp
// Messenger Interface
public interface IMessenger
{
    // Subscribe callback to receive a payload
    // Predicate to filter the payload (optional)
    IMessenger Subscribe<T>(Action<T> callback, Predicate<T> predicate = null);

    // Unsubscribe the callback from receiving the payload 
    IMessenger Unsubscribe<T>(Action<T> callback);

    // Publish the payload to its subscribers
    IMessenger Publish<T>(T payload);
}
```

Access to default Messenger instance via: 
```csharp
SuperMaxim.Messaging.Messenger.Default.[function]
```

### Publish 

```csharp
// Generic Parameter <T> - here is a <Payload> that will be published to subscribers of this type
Messenger.Default.Publish<Payload>(new Payload{ /* payload params */ });

// In most cases there is no need in specifying Generic Parameter <T>
Messenger.Default.Publish(new Payload{ /* payload params */ });

// Generic Parameter <T> - here is a <IPayload> that will be published to subscribers of this type
Messenger.Default.Publish<IPayload>(new Payload{ /* payload params */ });

class Payload : IPayload
{

}
```

### Subscribe 

```csharp
// Payload – the type of Callback parameter
// Callback – delegate (Action<T>) that will receive the payload
Messenger.Default.Subscribe<Payload>(Callback);

private static void Callback(Payload payload)
{
  // Callback logic
}
```

### Subscribe with Predicate 

```csharp
// Predicate – delegate (Predicate<T>) that will receive payload to filter
Messenger.Default.Subscribe<Payload>(Callback, Predicate);

private static bool Predicate(Payload payload)
{
  // Predicate filter logic
  // if function will return ‘false’ value, the Callback will not be invoked
  return accepted;
}
```

### Unsubscribe - Variant #1 

```csharp
// Payload – the type of Callback parameter that was subscribed
// Callback – delegate (Action<Payload>) that was subscribed
Messenger.Default.Unsubscribe<Payload>(Callback);

private static void Callback(Payload payload)
{
  // Callback logic
}
```

### Unsubscribe - Variant #2 

```csharp
// IPayload – the type of Callback parameter that was subscribed
// Callback – delegate (Action<Payload>) that was subscribed
Messenger.Default.Unsubscribe<IPayload>(Callback);

// Payload class implements IPayload interface
private static void Callback(Payload payload)
{
  // Callback logic
}
```
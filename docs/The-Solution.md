* **Pub\Sub Messenger** - Container for Events that allows Decoupling of Publishers and Subscribers so they can evolve independently. This Decoupling is useful in Modularised Applications because new modules can be added that respond to events defined by the Shell or, more likely, other modules. All events have a Weak Reference and invocation can be done Async or Sync way.
* Instead of passing objects or modules, pass small **Payloads** (Data/Messages) that are relevant for the specific cases/events.
* Classes/Modules will not be “familiar” with each other, this will allow better **encapsulation and less dependencies**.
* In case of subscriber’s destruction, it will be removed automatically from Messenger’s list, since it was referenced via **Weak Reference**.
* Pub/Sub can be a great pattern in combination with _Dependency Injection_ (DP) and with _Inversion of Control_ (IoC), both part of _SOLID_ Principles.

### Usage of Messenger as Pub/Sub mechanism:

```csharp
public class FetchStickPayload
{
     // stick type for filtering
     public StickTypes StickType { get; set; }
     
     // the position of stick in the space
     public Vector3 Position { get; set; }
}

// publisher
public class Human
{
     public void PublishFetchStickPayload()
     {
         // publish new payload with specific data
         Messenger.Default.Publish(
              new FetchStickPayload 
                       { 
                              StickType = StickTypes.PlasticStick, 
                              Position = new Vector3(1, 1, 0) 
                       });
     }
}

// subscriber
public class Dog : Animal
{
     // callback - method that is invoked by Messenger and receives payload instance
     public void OnFetchStick(FetchStickPayload payload) { /* TODO handle stick fetching */ }
}

public class Playground
{
     public Human David { get; set; }
     public Dog Billy { get; set; }
     public Dog Mika { get; set; }

     public void Subscribe()
     { 
         // subscribe callback Billy.OnFetchStick to FetchStickPayload
         Messenger.Default.Subscribe<FetchStickPayload>(Billy.OnFetchStick);
        
         // subscribe callback Mika.OnFetchStick to FetchStickPayload with filter/predicate
         Messenger.Default.Subscribe<FetchStickPayload>(Mika.OnFetchStick, CanFetchStick);
     }

     private bool CanFetchStick(FetchStickPayload payload) { /* TODO filter unwanted stick types */ }
}
```
![billi -> mika](Images/billi_mika.png)
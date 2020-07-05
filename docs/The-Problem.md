* **Wiring the Parts with Events** – Tightly Coupled and may cause Memory Leak problems. The Publisher and the Subscriber have to know of each other, and a Subscriber can't be collected by the GC if it's connected with the Publisher with strong event reference.
* **Using Unity Event Routing** – Although Unity Event Routing is a very good feature, it is a Unity Specific Solution and we need a generic one. Also, we cannot use it everywhere even if the project is in Unity.
* **Dependency** - In commonly used C# events or delegates, classes are “familiar” with each other and this prevents good modularity. This is NOT following SOLID Principles and Objects are not Encapsulated.

### Example of common usages of events in C#:

```csharp
public class Human
{
    // event that passes instance of Stick when it is invoked
    public event Action<Stick> FetchStick;
    
    // static event that passes instance of Ball when it is invoked
    public static event Action<Ball> FetchBall;
}

public class Dog : Animal
{
    // event handler - method that is invoked by event and receives Stick instance
    public void OnFetchStick(Stick stick) { /*TODO*/ }
}

public class Cat : Animal
{
    // event handler - method that is invoked by event and receives Ball instance
    public void OnFetchBall(Ball ball) { /*TODO*/ }
}

public class Playground
{
    public Human David { get; set; }
    private Dog Rex { get; set; }
    private Cat Max { get; set; }

    public void RegisterEvents()
    {
        David.FetchStick += Rex.OnFetchStick;
        Human.FetchBall += Max.OnFetchBall;
    }
}
```
![david -> rex](Images/david_rex.png)
> Rex.OnFetchStick is attached to David.FetchStick and David instance has reference to Rex instance. Rex instance will not be removed from the memory until it's handler OnFetchStick will be detached from FetchStick event or until David will be removed from the memory.

![human -> rex](Images/human_rex.png)
> Max.OnFetchBall is attached to Human.FetchBall and Max instance is referenced by static pointer Human.FetchBall. Max instance will not be removed from the memory until it's handler OnFetchBall will be detached from static event FetchBall. Static references are worse case of memory leaks.
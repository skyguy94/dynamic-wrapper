## Stop writing wrapper classes, and do something more productive with your time.

[![Build status](https://ci.appveyor.com/api/projects/status/t9xyx9lph140vml4?svg=true)](https://ci.appveyor.com/project/skyguy94/dynamic-wrapper) [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/skyguy94/dynamic-wrapper?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![NuGet version](https://badge.fury.io/nu/DynamicWrapper.svg)](http://badge.fury.io/nu/DynamicWrapper)

This utility will automatically generate the wrapper class and return to you the wrapped object that implements your interface. 

Simply copy DynamicWrapper.cs from the Downloads tab into your solution and use it. There are two extension methods available to you:

```csharp
Interface wrapper = myObject.As<Interface>();
myObject = wrapper.AsReal<MyObjectType>();
```

## Examples

Lets assume you have a class without an interface. This is often the case with .Net framework classes. Your code relies on this framework object, but you want to abstract it in order to substitute it under test. Typically, you would write a wrapper class that acts as a proxy for the real object, but implements an interface. This utility creates that wrapper for you on the fly.

```csharp
public class ClassWithoutInterface
{
    public void DoSomething()
    {
        // Do something
    }
}

public interface IDoSomething
{
    void DoSomething();
}
```

In C#, you cannot write the following because ClassWithoutInterface does not implement IDoSomething:

```csharp
IDoSomething actor = objectWithoutInterface as IDoSomething; // will resolve as null 
```

With this utility, you can do this:

```csharp
IDoSomething actor = objectWithoutInterface.As<IDoSomething>(); // succeeds 
```

The important thing to know is that this is a wrapped object. Unlike casting, the actor object is not the same as the real object -- it is a proxy to the real object. You can get the real object back out:

```csharp
ClassWithoutInterface realObject = actor.AsReal<ClassWithoutInterface>(); 
```

That is all there is to this utility. Stop writing wrapper classes and do something more productive with your time.

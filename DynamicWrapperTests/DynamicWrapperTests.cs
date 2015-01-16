using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DynamicWrapper;
using NUnit.Framework;

namespace DynamicWrapperTests
{
  [TestFixture]
  public class DynamicWrapperTests
  {
    public class TestEventArgs : EventArgs
    {
      public string Message { get; set; }
    }

    public interface ITester
    {
      string Repsond(string message);
      int Return5();
      void VoidMethod();
      void Add6ToParam(ref int param);
      void SetOutParamTo7(out int param);
      int Property { get; set; }

      event EventHandler<TestEventArgs> MyEvent;
    }

    public interface ITester2
    {
      string Repsond(string message);
    }

    public interface IGenericTester
    {
      bool IsTypeOf<T>();
      bool TypesAreSame<TOneT, TWoT>();
      T GenericReturn<T>();
      T GenericParameter<T>(T value);
      bool GenericCompare<TOneT, TWoT>(TOneT one, TWoT two);
      IEnumerable<T> Return3<T>(Func<int, T> generate);
    }

    public class MyTester
    {
      public string Repsond(string message)
      {
        return message + " response";
      }

      public int Return5()
      {
        return 5;
      }

      public void VoidMethod()
      {
        throw new Exception("VoidMethod");
      }

      public void Add6ToParam(ref int param)
      {
        param = param + 6;
      }

      public void SetOutParamTo7(out int param)
      {
        param = 7;
      }

      public int Property { get; set; }

      public bool IsTypeOf<T>()
      {
        return typeof(T) == GetType();
      }

      public bool TypesAreSame<TOneT, TWoT>()
      {
        return typeof(TOneT) == typeof(TWoT);
      }

      public T GenericReturn<T>()
      {
        return (T) Activator.CreateInstance(typeof(T));
      }

      public T GenericParameter<T>(T value)
      {
        return value;
      }

      public bool GenericCompare<TOneT, TWoT>(TOneT one, TWoT two)
      {
        return one.Equals(two);
      }

      public IEnumerable<T> Return3<T>(Func<int, T> generate)
      {
        for (int i = 0; i < 3; i++)
          yield return generate(i);
      }

      public event EventHandler<TestEventArgs> MyEvent;

      public void FireEvent(string message)
      {
        var handlers = MyEvent;
        if (handlers != null)
          handlers(this, new TestEventArgs {Message = message});
      }
    }

    public interface IGenericInterface<out T, out TF>
    {
      T GetT();
      TF GetF();
    }

    public class GenericClass<T, TF>
    {
      public T Val;
      public TF FVal;

      public T GetT()
      {
        return Val;
      }

      public TF GetF()
      {
        return FVal;
      }
    }

    private MyTester _realObject;

    [SetUp]
    public virtual void SetUp()
    {
      _realObject = new MyTester();
    }

    [Test]
    public void Calling_Wrapper_Proxies_To_RealObject()
    {
      var wrappedObject = _realObject.As<ITester>();

      Assert.That(wrappedObject.Repsond("Foo"), Is.EqualTo("Foo response"));
    }

    [Test]
    public void Casting_More_Than_Once_Does_Not_Fail()
    {
      var wrappedObject1 = _realObject.As<ITester>();
      var wrappedObject2 = _realObject.As<ITester>();

      Assert.That(wrappedObject1.Repsond("Foo"), Is.EqualTo("Foo response"));
      Assert.That(wrappedObject2.Repsond("Foo"), Is.EqualTo("Foo response"));
    }

    [Test]
    public void Two_Interface_Wrappers_For_The_Same_Class()
    {
      var wrappedObject1 = _realObject.As<ITester>();
      var wrappedObject2 = _realObject.As<ITester2>();

      Assert.That(wrappedObject1.Repsond("Foo"), Is.EqualTo(wrappedObject2.Repsond("Foo")));
    }

    [Test]
    public void Parameterless_Methods_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      Assert.That(wrappedObject.Return5(), Is.EqualTo(5));
    }

    [Test]
    public void Reference_Parameters_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      int foo = 5;
      wrappedObject.Add6ToParam(ref foo);

      Assert.That(foo, Is.EqualTo(11));
    }

    [Test]
    public void Out_Parameters_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      int foo;
      wrappedObject.SetOutParamTo7(out foo);

      Assert.That(foo, Is.EqualTo(7));
    }

    [Test]
    public void Void_Methods_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      Exception exception = null;
      try
      {
        wrappedObject.VoidMethod();
      }
      catch (Exception e)
      {
        exception = e;
      }

      Assert.That(exception, Is.Not.Null);
      Assert.That(exception.Message, Is.EqualTo("VoidMethod"));
    }

    [Test]
    public void Set_Properties_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      wrappedObject.Property = 55;
      Assert.That(_realObject.Property, Is.EqualTo(55));
    }

    [Test]
    public void Get_Properties_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      _realObject.Property = 66;
      Assert.That(wrappedObject.Property, Is.EqualTo(66));
    }

    [Test]
    public void Generic_Methods_With_One_Argument_Generates_Properly()
    {
      var wrappedObject = _realObject.As<IGenericTester>();

      Assert.That(wrappedObject.IsTypeOf<string>(), Is.False);
    }

    [Test]
    public void Generic_Methods_With_Multiple_Arguments_Generates_Properly()
    {
      var wrappedObject = _realObject.As<IGenericTester>();

      Assert.That(wrappedObject.TypesAreSame<string, string>(), Is.True);
    }

    [Test]
    public void Generic_Return_Value_Generates_Properly()
    {
      var wrappedObject = _realObject.As<IGenericTester>();

      Assert.That(wrappedObject.GenericReturn<int>(), Is.EqualTo(0));
      Assert.That(wrappedObject.GenericReturn<DateTime>(), Is.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void Generic_Parameter_Generates_Properly()
    {
      var wrappedObject = _realObject.As<IGenericTester>();

      Assert.That(wrappedObject.GenericParameter("FOO"), Is.EqualTo("FOO"));
    }

    [Test]
    public void Multiple_Generic_Parameters_Generates_Properly()
    {
      var wrappedObject = _realObject.As<IGenericTester>();

      Assert.That(wrappedObject.GenericCompare("FOO", "FOO"));
      Assert.That(wrappedObject.GenericCompare("FOO", "BAR"), Is.False);
    }

    [Test]
    public void Complex_Generic_Method_Generates_Properly()
    {
      var wrappedObject = _realObject.As<IGenericTester>();

      var values = wrappedObject.Return3(i => i.ToString()).ToList();

      Assert.That(values.Count, Is.EqualTo(3));
      Assert.That(values[0], Is.EqualTo("0"));
      Assert.That(values[1], Is.EqualTo("1"));
      Assert.That(values[2], Is.EqualTo("2"));
    }

    [Test]
    public void Generic_Interface_Generates_Properly()
    {
      var realObject = new GenericClass<string, int> {Val = "FOO", FVal = 99};
      var wrappedObject = realObject.As<IGenericInterface<string, int>>();

      Assert.That(wrappedObject.GetT(), Is.EqualTo("FOO"));
      Assert.That(wrappedObject.GetF(), Is.EqualTo(99));
    }

    [Test]
    public void Events_Wrap_Propperly()
    {
      var wrappedObject = _realObject.As<ITester>();

      object eventSender = null;
      string eventMessage = string.Empty;

      wrappedObject.MyEvent += (sender, args) =>
      {
        eventSender = sender;
        eventMessage = args.Message;
      };

      _realObject.FireEvent("FOO");

      Assert.That(eventSender, Is.EqualTo(_realObject));
      Assert.That(eventMessage, Is.EqualTo(eventMessage));
    }

    [Test]
    public void GetRealObjectBack()
    {
      var wrappedObject = _realObject.As<ITester>();

      Assert.That(wrappedObject.AsReal<MyTester>(), Is.EqualTo(_realObject));
    }

    [Test]
    public void GetRealObject_When_Type_Implements_Interface()
    {
      IList list = new List<string>();
      var list2 = list.AsReal<List<string>>();

      Assert.That(list, Is.EqualTo(list2));
    }

    [Test]
    public void GetRealObject_When_Wrong_Type_Returns_Null()
    {
      IList list = new List<string>();

      Assert.That(list.AsReal<MyTester>(), Is.Null);
    }

    public interface INotWrappable
    {
      void NotWrappable();
    }

    [Test]
    [ExpectedException(typeof(MissingMethodException))]
    public void Bad_Mapping_Throws_Execption()
    {
      _realObject.As<INotWrappable>();
    }

    public interface IDerived : IDisposable
    {
      string Foo();
    }

    public class MultiIf
    {
      private string _message = "Bar";

      public void Dispose()
      {
        _message = "Disposed";
      }

      [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
      public string Foo()
      {
        return _message;
      }
    }

    [Test]
    public void Derived_Interfaces_Generate_Properly()
    {
      var realObject = new MultiIf();
      var wrappedObject = realObject.As<IDerived>();

      Assert.That(wrappedObject.Foo(), Is.EqualTo("Bar"));

      wrappedObject.Dispose();

      Assert.That(wrappedObject.Foo(), Is.EqualTo("Disposed"));
    }

    public interface IFoo
    {}

    public class Foo : IFoo
    {}

    [Test]
    public void When_Class_Explicitly_Implements_Interface_Do_Not_Wrap()
    {
      var realObject = new Foo();
      var foo = realObject.As<IFoo>();

      Assert.That(realObject, Is.EqualTo(foo));
    }
  }
}
namespace DynamicWrapper
{
  public static class ObjectExtensions
  {
    public static T As<T>(this object realObject) where T : class
    {
      var asT = realObject as T;
      if (asT != null) return asT;

      return DynamicWrapper.CreateWrapper<T>(realObject);
    }

    public static T AsReal<T>(this object wrapper) where T : class
    {
      var real = wrapper as T;
      if (real != null) return real;


      var wrapperBase = wrapper as DynamicWrapper.DynamicWrapperBase;
      if (wrapperBase != null) return (T) wrapperBase.RealObject;

      return null;
    }
  }
}
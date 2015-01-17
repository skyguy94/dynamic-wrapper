using System;
using System.Collections.Generic;

namespace DynamicWrapper
{
  internal class WrapperDictionary
  {
    private readonly Dictionary<string, Type> _wrapperTypes = new Dictionary<string, Type>();

    private static string GenerateKey(Type interfaceType, Type realObjectType)
    {
      return interfaceType.Name + "->" + realObjectType.Name;
    }

    public Type GetType(Type interfaceType, Type realObjectType)
    {
      var key = GenerateKey(interfaceType, realObjectType);

      Type value;
      _wrapperTypes.TryGetValue(key, out value);

      return value;
    }

    public void SetType(Type interfaceType, Type realObjectType, Type wrapperType)
    {
      var key = GenerateKey(interfaceType, realObjectType);

      _wrapperTypes[key] = wrapperType;
    }
  }
}
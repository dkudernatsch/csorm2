using System;
using System.Linq;

namespace Csorm2.Core.Cache
{
    public static class ObjectProvider
    {
        public static object Construct(Type t)
        {
            return t.GetConstructor(new Type[]{})?
                .Invoke(new object[]{});
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Csorm2.Core.Schema.Builders
{
    public class AttributesFromTypeResolver
    {

        public static IEnumerable<PropertyInfo> ValidAttributesFromType(Type t)
        {
            return t.GetProperties().Where(p => p.CanRead && p.CanWrite);
        }
        
    }
}
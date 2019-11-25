using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Csorm2.Core.Extensions
{
    public static class FilterSelectExtension
    {
        public static IEnumerable<TDest> FilterSelect<TSource, TDest>(
            this IEnumerable<TSource> _this,
            Func<TSource, TDest> filterSelector)
        {
            return _this.Select(filterSelector).Where(res => res != null);
        }
    }
}
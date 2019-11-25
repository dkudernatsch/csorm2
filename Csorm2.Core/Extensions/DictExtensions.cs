using System;
using System.Collections.Generic;

namespace Csorm2.Core.Extensions
{
    public static class DictExtensions
    {
        public static void AddDict<TKey, TValue>(this IDictionary<TKey, TValue> _this, IDictionary<TKey, TValue> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other)); 

            foreach (var (key, value) in other)
            {
                _this[key] = value;
            }
        }

        public static TValue GetOrInsert<TKey, TValue>(this Dictionary<TKey, TValue> _this, TKey key, TValue toInsert)
        {
            if (_this.TryGetValue(key, out var result))
            {
                return result;
            }
            _this[key] = toInsert;
            return toInsert;
        }
    }
}
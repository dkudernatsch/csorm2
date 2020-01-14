using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Csorm2.Core.Cache
{
    public class LazyLoader: ILazyLoader
    {
        internal ILazyLoader Internal {  get; set; }

        public ICollection<T> Load<T>(object entityObj, ref ICollection<T> loadTo, [CallerMemberName] string name = "")
        {
            return Internal?.Load(entityObj, ref loadTo, name) ?? loadTo;
        }

        public T Load<T>(object entityObj, ref T loadTo, [CallerMemberName] string name = "")
        {
            return Internal == null 
                ? loadTo 
                : Internal.Load(entityObj, ref loadTo, name);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Csorm2.Core.Cache
{
    public interface ILazyLoader
    {
        ICollection<T> Load<T>(object entityObj, ref ICollection<T> loadTo, [CallerMemberName] string name = "");
        T Load<T>(object entityObj, ref T loadTo, [CallerMemberName] string name = "");
    }
}
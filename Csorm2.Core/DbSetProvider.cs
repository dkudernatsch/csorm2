using System;
using System.Reflection;

namespace Csorm2.Core
{
    internal class DbSetProvider
    {
        public static object ProvideDbSet(Type t, DbContext ctx)
        {
            return typeof(DbSetProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetDbSet))!
                .MakeGenericMethod(t)!
                .Invoke(null, new object[] {ctx})!;
        }

        private static DbSet<T> GetDbSet<T>(DbContext ctx) => new InternalDbSet<T>(ctx);
    }
}
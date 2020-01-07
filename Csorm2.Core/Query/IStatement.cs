using System.Collections.Generic;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Expression;

namespace Csorm2.Core.Query
{
    public interface IStatement<TEntity>: ISqlExpression
    {
        IEnumerable<IEnumerable<(Attribute, TEntity)>> ReturnValuePositions { get; }
        
        Entity Entity { get; }
    }
}
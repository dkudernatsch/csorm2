using System;
using System.Collections.Generic;
using Csorm2.Core.Metadata;
using ManyToMany = Csorm2.Core.Metadata.ManyToMany;
using ManyToOne = Csorm2.Core.Metadata.ManyToOne;
using OneToMany = Csorm2.Core.Metadata.OneToMany;

namespace Csorm2.Core.DDL
{
    public class ForeignKeyDefinition
    {
        public static IEnumerable<IConstraint> FromRelation(IRelation r)
        {
            return r switch
            {
                ManyToMany m => new[] {(IConstraint) new ManyToManyConstraintTo(m), new ManyToManyConstraintFrom(m)},
                OneToMany o => new[] {new OneToManyConstraint(o)},
                ManyToOne m => new[] {new ManyToOneConstraint(m)}
            };
        }
    }
}
using Csorm2.Core.Query.Expression;

namespace Csorm2.Core.DDL
{
    public interface IDdlStatement
    {
        public string AsSqlString();
    }
}
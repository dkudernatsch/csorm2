namespace Csorm2.Core.DDL
{
    public interface IConstraint
    {
        public string OnTable { get; }
        public string Name { get; }
        public string Definition { get; }

        public string AsSqlString() => $"ALTER TABLE {OnTable} ADD CONSTRAINT {Name} {Definition}";

    }
}
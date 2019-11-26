namespace Csorm2.Core.Query.Select
{
    public interface IFrom
    {
        string AsFromFragment();

        string GetAlias();
        bool HasAlias();

    }
}
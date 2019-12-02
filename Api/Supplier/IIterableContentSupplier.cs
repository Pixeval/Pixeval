namespace Pzxlane.Api.Supplier
{
    public interface IIterableContentSupplier<TEntity, out TEntityList> : IPixivSupplier<TEntity, TEntityList>
    {
        string Status { get; }

        int Start { get; }

        int End { get; }
    }
}
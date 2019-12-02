namespace Pzxlane.Api.Supplier
{
    public interface IRecursiveContentSupplier<TEntity, TEntityList> : IPixivSupplier<TEntity, TEntityList>
    {
        bool GetCondition(TEntityList param);

        void UpdateCondition(TEntityList param);
    }
}
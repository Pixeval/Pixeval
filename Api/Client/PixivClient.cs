using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Pzxlane.Api.Impl;
using Pzxlane.Api.Supplier;
using Pzxlane.Data.Model.ViewModel;

namespace Pzxlane.Api.Client
{
    public sealed class PixivClient
    {
        private static volatile PixivClient _instance;
        
        private static readonly object Locker = new object();

        public static PixivClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new PixivClient();
                        }
                    }
                }

                return _instance;
            }
        }

        public IAsyncEnumerable<Task<Illustration>> Query(string tag, int start, int end)
        {
            return IterableWorks(new Query(tag, start, end));
        }

        public IAsyncEnumerable<Task<Illustration>> Upload(string id)
        {
            return IterableWorks(new Upload(id));
        }

        public IAsyncEnumerable<Task<Illustration>> Favorite(string id)
        {
            return RecursiveWorks(new Favorite(id));
        }

        public IAsyncEnumerable<Task<Illustration>> Daily()
        {
            return RecursiveWorks(new Daily());
        }

        private static async IAsyncEnumerable<Task<Illustration>> RecursiveWorks<TEntity, TEntityList>(IRecursiveContentSupplier<TEntity, TEntityList> supplier)
        {
            while (true)
            {
                var contents = await supplier.GetIllusts(null);

                if (contents == null || supplier.GetCondition(supplier.Context))
                {
                    yield break;
                }

                foreach (var content in contents)
                {
                    yield return PixivHelper.IllustrationInfo(supplier.GetIllustId(content));
                }
                supplier.UpdateCondition(supplier.Context);
            }
        }

        private static async IAsyncEnumerable<Task<Illustration>> IterableWorks<TEntity, TEntityList>(IIterableContentSupplier<TEntity, TEntityList> supplier)
        { 
            for (var i = supplier.End == 0 ? 1 : supplier.Start;; i++)
            {
                var contents = (await supplier.GetIllusts(i)).ToImmutableArray();

                if (ContentsInvalid(supplier, contents, i))
                {
                    yield break;
                }

                foreach (var content in contents)
                {
                    yield return PixivHelper.IllustrationInfo(supplier.GetIllustId(content));
                }
            }
        }

        private static bool ContentsInvalid<TEntity, TEntityList>(IIterableContentSupplier<TEntity, TEntityList> supplier, IEnumerable<TEntity> entity, int index)
        {
            return supplier.End != 0 && index > supplier.End || entity == null || supplier.Status == "failure";
        }
    }
}
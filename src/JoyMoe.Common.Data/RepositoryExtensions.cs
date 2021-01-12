namespace JoyMoe.Common.Data
{
    public static class RepositoryExtensions
    {
        public static IRepository<TEntity> IgnoreQueryFilters<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
        {
            repository.IgnoreQueryFilters = true;

            return repository;
        }
    }
}

namespace JoyMoe.Common.Data
{
    public static class RepositoryExtensions
    {
        public static TRepository IgnoreQueryFilters<TRepository, TEntity>(this TRepository repository)
            where TRepository : IRepository<TEntity>
            where TEntity : class
        {
            repository.IgnoreQueryFilters = true;

            return repository;
        }
    }
}

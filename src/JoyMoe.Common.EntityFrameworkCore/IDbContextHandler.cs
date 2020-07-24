using System.Threading.Tasks;

namespace JoyMoe.Common.EntityFrameworkCore
{
    public interface IDbContextHandler
    {
        Task OnCreateEntity(object entity);

        Task OnDeleteEntity(object entity);

        Task OnUpdateEntity(object entity);
    }
}

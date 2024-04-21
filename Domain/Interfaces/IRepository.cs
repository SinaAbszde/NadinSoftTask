using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true);
        Task RemoveAsync(T entity);
        Task SaveAsync();
    }
}

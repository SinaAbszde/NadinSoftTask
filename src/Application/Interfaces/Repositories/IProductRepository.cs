using Domain.Interfaces;
using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task CreateAsync(Product entity);
        Task UpdateAsync(Product entity);
    }
}

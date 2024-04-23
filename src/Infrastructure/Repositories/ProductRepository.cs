using Application.Interfaces;
using Domain.Models;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task CreateAsync(Product entity)
        {
            entity.ProduceDate = DateOnly.FromDateTime(DateTime.Now);
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
            var product = await GetAsync(u => u.ID == entity.ID, tracked: false);
            entity.ProduceDate = product.ProduceDate;
            _db.Products.Update(entity);
            await SaveAsync();
        }
    }
}

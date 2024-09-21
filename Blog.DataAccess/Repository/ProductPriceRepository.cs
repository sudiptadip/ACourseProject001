using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository
{
    public class ProductPriceRepository : Repository<ProductPrice>, IProductPriceRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductPriceRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ProductPrice>> GetProductPricesAsync(int productId)
        {
            return await _db.ProductPrices.Where(pp => pp.ProductId == productId).ToListAsync();
        }
    }
}

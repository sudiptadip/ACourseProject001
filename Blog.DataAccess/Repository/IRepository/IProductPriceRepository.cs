using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository.IRepository
{
    public interface IProductPriceRepository : IRepository<ProductPrice>
    {
        Task<IEnumerable<ProductPrice>> GetProductPricesAsync(int productId);
    }
}

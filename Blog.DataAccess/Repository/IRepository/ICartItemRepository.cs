using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository.IRepository
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<List<CartItem>> GetCartItemsByCartIdAsync(int cartId);

        void Update(CartItem cartItem);
    }
}

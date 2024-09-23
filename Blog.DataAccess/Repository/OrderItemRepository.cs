using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderItem item)
        {
            _db.Update(item);
        }
    }
}

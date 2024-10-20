using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository.IRepository
{
    public interface IUniteOfWork
    {
        IFacultyRepository Faculty { get; }
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ISubjectRepository Subject { get; }
        ICartItemRepository CartItem { get; }
        ICartRepository Cart { get; }
        IBlogRepository Blog { get; }
        IOrderItemRepository OrderItem { get; }
        IOrderRepository Order { get; }
        ISosalMediaRepository SosalMedia { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IContactRepository Contact { get; }
        void Save();
    }
}

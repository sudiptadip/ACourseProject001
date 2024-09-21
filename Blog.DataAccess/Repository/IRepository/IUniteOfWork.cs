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
        IProductPriceRepository ProductPrice { get; }
        ISubjectRepository Subject { get; }
        ICartItemRepository CartItem { get; }
        ICartRepository Cart { get; }
        IBlogRepository Blog { get; }
        void Save();
    }
}

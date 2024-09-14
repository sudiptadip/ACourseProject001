using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            var objFromDb = _db.Categories.FirstOrDefault(c => c.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.CategoryName = obj.CategoryName;
                objFromDb.CategoryName = obj.CategoryName;
                objFromDb.SortedOrder = obj.SortedOrder;
                objFromDb.IsActive = obj.IsActive;
                objFromDb.ModifiedOn = obj.ModifiedOn;
            }
        }
    }
}

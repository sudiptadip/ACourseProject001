using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository
{
    public class UniteOfWork : IUniteOfWork
    {
        public ICategoryRepository Category {  get; private set; }
        private readonly ApplicationDbContext _db;
        public UniteOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}

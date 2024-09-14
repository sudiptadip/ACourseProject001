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
    internal class FacultyRepository : Repository<Faculty>, IFacultyRepository
    {
        private readonly ApplicationDbContext _db;
        public FacultyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Faculty obj)
        {
            _db.Faculties.Update(obj);
        }
    }
}

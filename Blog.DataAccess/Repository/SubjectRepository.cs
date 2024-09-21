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
    public class SubjectRepository : Repository<Subject>, ISubjectRepository
    {
        private readonly ApplicationDbContext _db;
        public SubjectRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async void Update(Subject obj)
        {
            var objFromDb = await _db.Subjects.FirstOrDefaultAsync(c => c.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.SubjectName = obj.SubjectName;
                objFromDb.SortedOrder = obj.SortedOrder;
                objFromDb.ModifiedOn = obj.ModifiedOn;
            }
        }
    }
}

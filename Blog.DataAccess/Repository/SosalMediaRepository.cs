using Blog.DataAccess.Data;
using Blog.DataAccess.Repository.IRepository;
using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository
{
    public class SosalMediaRepository : Repository<SosalMedia>, ISosalMediaRepository
    {
        private readonly ApplicationDbContext _context;
        public SosalMediaRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public void Update(SosalMedia media)
        {
            _context.SosalMedia.Update(media);
        }
    }
}
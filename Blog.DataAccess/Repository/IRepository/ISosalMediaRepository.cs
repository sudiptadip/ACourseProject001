using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.DataAccess.Repository.IRepository
{
    public interface ISosalMediaRepository : IRepository<SosalMedia>
    {
        void Update(SosalMedia media);
    }
}

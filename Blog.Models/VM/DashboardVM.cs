using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class DashboardVM
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public IEnumerable<Order> Order { get; set; }
    }
}

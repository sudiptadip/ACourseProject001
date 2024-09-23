using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class BlogCategoryViewModel
    {
        public string CategoryName { get; set; }
        public List<Blogs> Blogs { get; set; }
    }
}

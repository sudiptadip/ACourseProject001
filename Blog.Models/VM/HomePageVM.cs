using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class HomePageVM
    {
        public SosalMedia SosalMedia { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<BlogCategoryViewModel> BlogCategoryViewModel { get; set; }
    }
}

using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class ProductVM
    {
        public List<Category> CategoryList { get; set; }
        public List<Product> ProductList { get; set; }
        public List<Faculty> FacultyList { get; set; }
        public List<Subject> SubjectList { get; set; }
    }
}

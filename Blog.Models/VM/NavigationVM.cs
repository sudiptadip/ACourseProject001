using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class NavigationVM
    {
        public int TotalCart { get; set; }
        public string UserName { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }
        public SosalMedia SosalMedia { get; set; }
    }
}
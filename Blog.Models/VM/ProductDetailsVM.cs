using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; }
        public List<string> ModeOfLecture { get; set; }
        public List<string> Validity { get; set; }
        public List<string> Views { get; set; }
        public List<string> Attempt { get; set; }
    }
}

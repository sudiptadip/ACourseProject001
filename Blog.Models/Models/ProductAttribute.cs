using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class ProductAttribute
    {
        public int Id { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public string AttributeType { get; set; }


        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

    }
}

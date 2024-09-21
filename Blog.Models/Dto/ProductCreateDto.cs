using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Dto
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public int FacultyId { get; set; }
        public int SubjectId { get; set; }
        public IFormFile ProductImage { get; set; }
        public string ProductImageUrl { get; set; }
        public List<ProductAttribuetDto> ProductAttribuets { get; set; }
    }

    public class ProductAttribuetDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string AttributeType { get; set; }
    }
}

﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string ProductDescription { get; set; }
        public bool IsActive { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultDiscountPrice { get; set; }
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

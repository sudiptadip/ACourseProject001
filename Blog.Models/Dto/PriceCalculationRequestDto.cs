using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Dto
{
    public class PriceCalculationRequestDto
    {
        public int ProductId { get; set; }
        public string ModeOfLecture { get; set; }
        public int ValidityInMonths { get; set; }
        public int Views { get; set; }
    }
}

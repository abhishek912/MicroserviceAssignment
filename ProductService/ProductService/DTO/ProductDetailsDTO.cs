using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.DTO
{
    public class ProductDetailsDTO
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public double ProductSize { get; set; }
        public double ProductPrice { get; set; }
        public string ProductType { get; set; }
    }
}

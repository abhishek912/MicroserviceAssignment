using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDetailsService.Entity
{
    public class ProductDetail
    {
        public int ProductID { get; set; }
        public double ProductSize { get; set; }
        public double ProductPrice { get; set; }
        public string ProductType { get; set; }
    }
}

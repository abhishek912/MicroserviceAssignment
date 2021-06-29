using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Entity
{
    public class Cart
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public double ProductSize { get; set; }
        public double ProductPrice { get; set; }
        public string ProductType { get; set; }
        public DateTime AddToCartDate { get; set; } = DateTime.Now;
    }
}

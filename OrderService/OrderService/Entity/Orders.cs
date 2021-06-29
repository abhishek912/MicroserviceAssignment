using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Entity
{
    public class Orders
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public double ProductSize { get; set; }
        public double ProductPrice { get; set; }
        public string ProductType { get; set; }
        public bool OrderStatus { get; set; } = true;
        public DateTime OrderPlacedDate { get; set; } = DateTime.Now;
    }
}

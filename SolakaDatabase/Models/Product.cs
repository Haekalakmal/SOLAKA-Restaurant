using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Stock { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public int RestoId { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual Restaurant Resto { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

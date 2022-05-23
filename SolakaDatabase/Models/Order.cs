using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Invoice { get; set; } = null!;
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int PaymentId { get; set; }
        public DateTime Created { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual PaymentMethod Payment { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

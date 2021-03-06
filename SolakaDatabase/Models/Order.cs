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
        public string TransactionCode { get; set; } = null!;
        public int CustomerId { get; set; }
        public int RestoId { get; set; }
        public int PaymentId { get; set; }
        public double TotalCost { get; set; }
        public string Status { get; set; } = null!;
        public DateTime Created { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual PaymentMethod Payment { get; set; } = null!;
        public virtual Restaurant Resto { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

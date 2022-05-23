using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class PaymentMethod
    {
        public PaymentMethod()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Type { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public class OrderData
    {
        public int? Id { get; set; }
        public string? Invoice { get; set; }
        public int? CustomerId { get; set; }
        public int ProductId { get; set; }
        public int PaymentId { get; set; }

        public DateTime? Created { get; set; }

        public List<OrderDetailsData> Details { get; set; }

    }
}

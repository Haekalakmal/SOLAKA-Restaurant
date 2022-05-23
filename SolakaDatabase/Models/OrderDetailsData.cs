using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolakaDatabase.Models
{
    public class OrderDetailsData
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public int Quantity { get; set; }
        public double Cost { get; set; }
        public string Status { get; set; }
    }
}

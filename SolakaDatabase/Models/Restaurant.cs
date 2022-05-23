using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class Restaurant
    {
        public Restaurant()
        {
            EmployeeRestos = new HashSet<EmployeeResto>();
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string NameResto { get; set; } = null!;
        public string Location { get; set; } = null!;

        public virtual ICollection<EmployeeResto> EmployeeRestos { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}

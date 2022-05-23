using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class Role
    {
        public Role()
        {
            Customers = new HashSet<Customer>();
            EmployeeApps = new HashSet<EmployeeApp>();
            EmployeeRestos = new HashSet<EmployeeResto>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<EmployeeApp> EmployeeApps { get; set; }
        public virtual ICollection<EmployeeResto> EmployeeRestos { get; set; }
    }
}

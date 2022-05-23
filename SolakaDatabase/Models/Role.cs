using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class Role
    {
        public Role()
        {
            EmployeeApps = new HashSet<EmployeeApp>();
            EmployeeRestos = new HashSet<EmployeeResto>();
        }

        public int Id { get; set; }
        public string RoleName { get; set; } = null!;

        public virtual ICollection<EmployeeApp> EmployeeApps { get; set; }
        public virtual ICollection<EmployeeResto> EmployeeRestos { get; set; }
    }
}

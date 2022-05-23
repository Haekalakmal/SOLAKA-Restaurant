using System;
using System.Collections.Generic;

namespace SolakaDatabase.Models
{
    public partial class EmployeeResto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int RestoId { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;

        public virtual Restaurant Resto { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

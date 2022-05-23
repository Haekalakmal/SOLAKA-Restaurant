namespace SolakaDatabase.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RoleName { get; set; }
    }
}

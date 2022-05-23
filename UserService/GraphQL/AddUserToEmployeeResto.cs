namespace UserService
{
    public record AddUserToEmployeeResto
    (
          int Id,
          int UserId,
          int RoleId,
          int RestoId,
          string Fullname,
          string Email   
     );
}

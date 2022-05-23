namespace UserService.GraphQL
{
    public record AddUserToEmployeeApp
    (
        int Id, 
        int UserId, 
        int RoleId,
        string Fullname, 
        string Email   
    );
}

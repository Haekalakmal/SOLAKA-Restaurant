namespace UserService.GraphQL
{
    public record AddUserToCustomerInput
    (
        int? Id,
        int UserId,
        int RoleId,
        string Name, 
        string Phone
        );
}

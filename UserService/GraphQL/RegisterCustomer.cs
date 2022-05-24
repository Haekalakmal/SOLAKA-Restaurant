namespace UserService.GraphQL
{
    public record RegisterCustomer
    (
        int? Id,
        string UserName,
        string Password,
        string Name, 
        string Phone
     );
}

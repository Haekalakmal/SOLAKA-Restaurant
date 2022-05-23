namespace UserService.GraphQL
{
    public record AddUserToCustomerInput
    (
        int? Id,
        string Name, 
        string Phone
        );
}

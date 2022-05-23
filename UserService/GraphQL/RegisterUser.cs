namespace UserService.GraphQL
{
    public record RegisterUser
    (
        int? Id,
        string UserName,
        string Password
    );
}

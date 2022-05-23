namespace UserService.GraphQL
{
    public record RegisterManagerApp
    (
        int? Id,
        string UserName,
        string Password,
        string Fullname,
        string Email
    );
}

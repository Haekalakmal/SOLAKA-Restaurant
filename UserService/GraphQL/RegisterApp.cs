namespace UserService.GraphQL
{
    public record RegisterApp
    (
        int? Id,
        string UserName,
        string Password,
        string Fullname,
        string Email
    );
}

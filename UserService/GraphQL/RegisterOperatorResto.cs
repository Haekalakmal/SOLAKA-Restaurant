namespace UserService.GraphQL
{
    public record RegisteOperatorResto
    (
        int? Id,
        int RestoId,
        string UserName,
        string Password,
        string Fullname,
        string Email
    );
}

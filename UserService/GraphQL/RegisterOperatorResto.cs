namespace UserService.GraphQL
{
    public record RegisterOperatorResto
    (
        int? Id,
        int RestoId,
        string UserName,
        string Password,
        string Fullname,
        string Email
    );
}

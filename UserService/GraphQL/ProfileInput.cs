namespace UserService.GraphQL
{
    public record ProfilesInput
   (
       int? Id,
       string FullName,
       string Email,
       string RoleName

   );
}

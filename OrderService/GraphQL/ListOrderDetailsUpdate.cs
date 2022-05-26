namespace OrderService.GraphQL
{
    public record ListOrderDetailsUpdate
    (
        int Id,
        int ProductId,
        int Quantity
    );
}

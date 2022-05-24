using SolakaDatabase.Models;

namespace OrderService.GraphQL
{
    public record OrdersUpdate
    (
         int Id,
        int Quantity,
        int ProductId
    );
}

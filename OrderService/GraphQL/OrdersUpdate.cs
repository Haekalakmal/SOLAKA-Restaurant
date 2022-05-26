using SolakaDatabase.Models;

namespace OrderService.GraphQL
{
    public record OrdersUpdate
    (
        int? Id,
        int RestoId,
        int PaymentId,
        int? OrderDetailId,

        List<ListOrderDetailsUpdate> ListOrderDetailsUpdate
    );
}

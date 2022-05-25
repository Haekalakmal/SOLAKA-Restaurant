using SolakaDatabase.Models;

namespace OrderService.GraphQL
{
    public record OrdersInput
    (
      int? Id,
      int RestoId,
      int PaymentId,

      List<ListOrderDetails> ListOrderDetails
    );
}

using SolakaDatabase.Models;

namespace OrderService.GraphQL
{
    public record OrdersInput
    (
      int? Id,
      string Invoice,
      int RestoId,
      int PaymentId,

      List<ListOrderDetails> ListOrderDetails
    );
}

using SolakaDatabase.Models;

namespace OrderService.GraphQL
{
    public record OrdersInput
    (
      int? Id,
       string Invoice,
      int CustomerId,
       int ProductId,
       int PaymentId,
      DateTime? Created,

        List<OrderDetailsData> OrderDetailsDatas
    );
}

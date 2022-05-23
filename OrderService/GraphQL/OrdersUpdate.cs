using SolakaDatabase.Models;

namespace OrderServices.GraphQL
{
    public record OrdersUpdate
    (
         int Id,
         
        int Quantity,
        int Product,

        double Cost

    );
}

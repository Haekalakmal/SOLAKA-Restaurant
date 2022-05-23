namespace FoodService.GraphQL
{
    public record FoodUpdate
        (
            int Id,
            string Name,
            int Stock,
            float Price,
            int CategoryId,
            int RestoId
        );
    
}

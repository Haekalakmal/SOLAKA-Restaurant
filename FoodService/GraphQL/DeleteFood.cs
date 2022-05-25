namespace FoodService.GraphQL
{
    public record DeleteFood
        (
            int? Id,
            string? Name,
            float? Price,
            int? Stock,
            int RestoId,
            int CategoryId
        );

    
}

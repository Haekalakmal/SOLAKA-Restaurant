namespace SolakaDatabase.Models
{
    public class ProductData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public int RestoId { get; set; }
    }
}

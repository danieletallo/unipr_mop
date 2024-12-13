namespace Orders.Shared
{
    public class OrderInsertDto
    {
        public int CustomerId { get; set; }
        public List<OrderDetailInsertDto> OrderDetails { get; set; } = new();
    }
}

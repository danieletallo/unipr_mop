namespace Orders.Shared
{
    public class OrderReadDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderDetailReadDto> OrderDetails { get; set; } = new List<OrderDetailReadDto>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
    }
}

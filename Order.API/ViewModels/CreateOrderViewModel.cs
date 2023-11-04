namespace Order.API.ViewModels
{
    public class CreateOrderVM
    {
        public Guid BuyerId { get; set; }
        public List<CreateOrderItemVM> Items { get; set; }
    }

    public class CreateOrderItemVM
    {
        public decimal Price { get; set; }
        public Guid ProductId { get; set; }
        public int Count { get; set; }
    }
}

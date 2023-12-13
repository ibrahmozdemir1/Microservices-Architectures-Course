namespace Order.API.ViewModels
{
    public class CreateOrderVM
    {
        public int BuyerId { get; set; }
        public List<CreateOrderItemVM> Items { get; set; }
    }

    public class CreateOrderItemVM
    {
        public decimal Price { get; set; }
        public string ProductId { get; set; }
        public int Count { get; set; }
    }
}

namespace HungryHub.Models
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public string RestaurantName { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount => CartItems.Sum(i => i.SubTotal);
        public decimal GrandTotal => TotalAmount + DeliveryFee;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string OrderNote { get; set; } = string.Empty;
    }
}
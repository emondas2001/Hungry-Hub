namespace HungryHub.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Icon { get; set; } = string.Empty;
        public decimal SubTotal => UnitPrice * Quantity;
    }
}

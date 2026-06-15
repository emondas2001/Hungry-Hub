namespace HungryHub.Models
{
    public class MenuViewModel
    {
        public Restaurant Restaurant { get; set; } = new();
        public List<MenuItem> MenuItems { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<CartItem> Cart { get; set; } = new();
    }
}
using HungryHub.Data;
using HungryHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HungryHub.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(
            ApplicationDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session
                .GetString("UserEmail") != null;

        private List<CartItem> GetCart()
        {
            try
            {
                var json = HttpContext.Session
                    .GetString("Cart");

                if (string.IsNullOrEmpty(json))
                    return new List<CartItem>();

                var type = typeof(List<CartItem>);
                var result =
                    System.Text.Json
                        .JsonSerializer
                        .Deserialize(json, type);

                if (result == null)
                    return new List<CartItem>();

                return (List<CartItem>)result;
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        public IActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var cart = GetCart();

            int restaurantId =
                HttpContext.Session
                    .GetInt32("CartRestaurantId")
                ?? 0;

            // Get restaurant info from DB
            var restDb = _db.Restaurants
                .FirstOrDefault(r =>
                    r.RestaurantId == restaurantId);

            string restName =
                restDb?.Name ?? "Unknown";
            decimal deliveryFee =
                restDb?.DeliveryFee ?? 20;

            var vm = new CartViewModel
            {
                CartItems = cart,
                RestaurantId = restaurantId,
                RestaurantName = restName,
                DeliveryFee = deliveryFee,
                UserFullName =
                    HttpContext.Session
                        .GetString("UserFullName")
                    ?? "",
                UserEmail =
                    HttpContext.Session
                        .GetString("UserEmail")
                    ?? ""
            };

            ViewBag.UserFullName =
                vm.UserFullName;

            // Set unread count for layout
            int uid = HttpContext.Session
                .GetInt32("UserId") ?? 0;
            ViewBag.UnreadCount =
                _db.Notifications
                    .Count(n =>
                        n.UserId == uid &&
                        !n.IsRead);

            return View(vm);
        }
    }
}
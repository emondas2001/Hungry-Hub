using HungryHub.Data;
using HungryHub.Models;
using HungryHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HungryHub.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PaymentService _payment;

        public OrderController(
            ApplicationDbContext db,
            PaymentService payment)
        {
            _db = db;
            _payment = payment;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session
                .GetString("UserEmail") != null;
        }

        private List<CartItem> GetCart()
        {
            try
            {
                var json = HttpContext.Session
                    .GetString("Cart");

                if (string.IsNullOrEmpty(json))
                    return new List<CartItem>();

                var type = typeof(List<CartItem>);

                var deserialized =
                    JsonSerializer.Deserialize(
                        json, type);

                if (deserialized == null)
                    return new List<CartItem>();

                return (List<CartItem>)deserialized;
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCart(
            List<CartItem> cart)
        {
            try
            {
                var json = JsonSerializer
                    .Serialize(cart,
                        typeof(List<CartItem>));

                HttpContext.Session.SetString(
                    "Cart", json);
            }
            catch
            {
                // ignore
            }
        }

        // ── GET /Order/Menu/5 ───────────────────
        public IActionResult Menu(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var restaurantDb =
                _db.Restaurants.FirstOrDefault(
                r => r.RestaurantId == id &&
                     r.IsActive);

            if (restaurantDb == null)
                return NotFound();

            var restaurant = new Restaurant
            {
                RestaurantId =
                    restaurantDb.RestaurantId,
                Name = restaurantDb.Name,
                Cuisine = restaurantDb.Cuisine,
                Address = restaurantDb.Address,
                Rating =
                    (double)restaurantDb.Rating,
                DeliveryTime =
                    restaurantDb.DeliveryTime,
                DeliveryFee =
                    restaurantDb.DeliveryFee,
                IsOpen = restaurantDb.IsOpen,
                ImageUrl =
                    string.IsNullOrEmpty(
                        restaurantDb.ImagePath)
                    ? restaurantDb.ImageUrl
                    : restaurantDb.ImagePath,
                Tag = restaurantDb.Tag
            };

            int todayDay =
                (int)DateTime.Now.DayOfWeek;

            var todayHours =
                _db.RestaurantHours.FirstOrDefault(
                h => h.RestaurantId == id &&
                     h.DayOfWeek == todayDay);

            if (todayHours != null)
            {
                if (todayHours.IsClosed)
                {
                    restaurant.IsOpen = false;
                }
                else if (
                    todayHours.OpenTime.HasValue &&
                    todayHours.CloseTime.HasValue)
                {
                    var nowTime =
                        TimeOnly.FromDateTime(
                            DateTime.Now);
                    restaurant.IsOpen =
                        nowTime >=
                            todayHours.OpenTime
                        && nowTime <=
                            todayHours.CloseTime;
                }
            }

            var menuItems = _db.MenuItems
                .Where(m => m.RestaurantId == id)
                .ToList();

            ViewBag.UserFullName =
                HttpContext.Session
                    .GetString("UserFullName");
            ViewBag.UserEmail =
                HttpContext.Session
                    .GetString("UserEmail");

            string hoursDisplay = "Hours not set";
            if (todayHours != null)
            {
                if (todayHours.IsClosed)
                {
                    hoursDisplay = "Closed today";
                }
                else if (
                    todayHours.OpenTime.HasValue &&
                    todayHours.CloseTime.HasValue)
                {
                    hoursDisplay =
                        todayHours.OpenTime.Value
                            .ToString("hh:mm tt") +
                        " - " +
                        todayHours.CloseTime.Value
                            .ToString("hh:mm tt");
                }
            }
            ViewBag.HoursDisplay = hoursDisplay;

            var vm = new MenuViewModel
            {
                Restaurant = restaurant,
                MenuItems = menuItems,
                Categories = menuItems
                    .Select(m => m.Category)
                    .Distinct()
                    .ToList(),
                Cart = GetCart()
            };

            return View(vm);
        }

        // ── POST /Order/AddToCart ───────────────
        [HttpPost]
        public IActionResult AddToCart(
            int menuItemId,
            string itemName,
            decimal unitPrice,
            string icon,
            int restaurantId)
        {
            if (!IsLoggedIn())
                return Json(
                    new { success = false });

            var cart = GetCart();

            var sessionRestId =
                HttpContext.Session
                    .GetInt32(
                        "CartRestaurantId");

            if (sessionRestId.HasValue &&
                sessionRestId.Value !=
                restaurantId)
            {
                cart.Clear();
            }

            HttpContext.Session.SetInt32(
                "CartRestaurantId",
                restaurantId);

            var existing = cart.FirstOrDefault(
                c => c.MenuItemId == menuItemId);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItemId,
                    ItemName = itemName,
                    UnitPrice = unitPrice,
                    Quantity = 1,
                    Icon = icon
                });
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                cartCount =
                    cart.Sum(c => c.Quantity),
                cartTotal =
                    cart.Sum(c => c.SubTotal)
            });
        }

        // ── POST /Order/RemoveFromCart ──────────
        [HttpPost]
        public IActionResult RemoveFromCart(
            int menuItemId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                c => c.MenuItemId == menuItemId);

            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    cart.Remove(item);
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                cartCount =
                    cart.Sum(c => c.Quantity),
                cartTotal =
                    cart.Sum(c => c.SubTotal)
            });
        }

        // ── POST /Order/PlaceOrder ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(
            string deliveryAddress,
            string orderNote)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";
                return RedirectToAction(
                    "Index", "Cart");
            }

            int userId =
                HttpContext.Session
                    .GetInt32("UserId") ?? 0;

            int restaurantId =
                HttpContext.Session
                    .GetInt32("CartRestaurantId")
                ?? 0;

            var restaurantDb =
                _db.Restaurants.FirstOrDefault(
                r => r.RestaurantId ==
                     restaurantId);

            decimal deliveryFee =
                restaurantDb?.DeliveryFee ?? 20;
            decimal totalAmount =
                cart.Sum(c => c.SubTotal);
            decimal grandTotal =
                totalAmount + deliveryFee;

            var orderItems =
                new List<OrderItem>();

            foreach (var c in cart)
            {
                orderItems.Add(new OrderItem
                {
                    MenuItemId = c.MenuItemId,
                    ItemName = c.ItemName,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    SubTotal = c.SubTotal
                });
            }

            // Coupon handling
            string couponCode =
              Request.Form["couponCode"].ToString() ?? "";
            decimal discountAmt = 0;
            int appliedCouponId = 0;

            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = _db.Coupons.FirstOrDefault(
                    c => c.Code == couponCode.ToUpper().Trim() &&
                         c.IsActive);

                if (coupon != null)
                {
                    if (coupon.DiscountType == "Percent")
                    {
                        discountAmt = totalAmount *
                            coupon.DiscountValue / 100;
                        if (coupon.MaxDiscount > 0 &&
                            discountAmt > coupon.MaxDiscount)
                            discountAmt = coupon.MaxDiscount;
                    }
                    else
                    {
                        discountAmt = coupon.DiscountValue;
                    }

                    discountAmt = Math.Min(
                        discountAmt, totalAmount);
                    grandTotal -= discountAmt;
                    if (grandTotal < 0) grandTotal = 0;
                    appliedCouponId = coupon.CouponId;
                }
            }

            var order = new Order
            {
                UserId = userId,
                RestaurantId = restaurantId,
                RestaurantName =
        restaurantDb?.Name ?? "Unknown",
                TotalAmount = totalAmount,
                DeliveryFee = deliveryFee,
                DiscountAmount = discountAmt,
                GrandTotal = grandTotal,
                CouponCode = string.IsNullOrEmpty(
        couponCode) ? null : couponCode.ToUpper(),
                Status = "PendingPayment",
                PaymentStatus = "Pending",
                PaymentMethod = "Pending",
                TransactionId = "",
                DeliveryAddress = deliveryAddress,
                OrderNote = orderNote ?? "",
                OrderDate = DateTime.Now,
                OrderItems = orderItems
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            // Record coupon usage
            if (appliedCouponId > 0)
            {
                var coupon = _db.Coupons.FirstOrDefault(
                    c => c.CouponId == appliedCouponId);
                if (coupon != null)
                {
                    coupon.UsedCount++;
                    _db.CouponUsages.Add(new CouponUsage
                    {
                        CouponId = appliedCouponId,
                        UserId = userId,
                        OrderId = order.OrderId,
                        Discount = discountAmt,
                        UsedAt = DateTime.Now
                    });
                    _db.SaveChanges();
                }
            }

            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove(
                "CartRestaurantId");

            return RedirectToAction(
                "Payment",
                new { orderId = order.OrderId });
        }

        // ── GET /Order/Payment/5 ────────────────
        public IActionResult Payment(
    int orderId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            // Load order items separately first
            var orderItems = _db.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToList();

            // Load order using anonymous type to
            // avoid null mapping exceptions
            var raw = _db.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => new
                {
                    o.OrderId,
                    o.RestaurantName,
                    o.RestaurantId,
                    o.TotalAmount,
                    o.DeliveryFee,
                    o.GrandTotal,
                    o.Status,
                    o.DeliveryAddress,
                    o.OrderNote,
                    o.OrderDate,
                    PaymentMethod =
                        o.PaymentMethod ?? "Cash",
                    PaymentStatus =
                        o.PaymentStatus ?? "Pending",
                    TransactionId =
                        o.TransactionId ?? ""
                })
                .FirstOrDefault();

            if (raw == null)
                return NotFound();

            var order = new Order
            {
                OrderId = raw.OrderId,
                RestaurantName = raw.RestaurantName,
                RestaurantId = raw.RestaurantId,
                TotalAmount = raw.TotalAmount,
                DeliveryFee = raw.DeliveryFee,
                GrandTotal = raw.GrandTotal,
                Status = raw.Status,
                DeliveryAddress = raw.DeliveryAddress,
                OrderNote = raw.OrderNote,
                OrderDate = raw.OrderDate,
                PaymentMethod = raw.PaymentMethod,
                PaymentStatus = raw.PaymentStatus,
                TransactionId = raw.TransactionId,
                OrderItems = orderItems
            };

            ViewBag.UserFullName =
                HttpContext.Session
                    .GetString("UserFullName");
            ViewBag.UserEmail =
                HttpContext.Session
                    .GetString("UserEmail");
            ViewBag.PaymentSuccess =
                TempData["PaymentSuccess"];
            ViewBag.TransactionId =
                TempData["TransactionId"]
                ?? order.TransactionId;
            ViewBag.PaymentMethod =
                TempData["PaymentMethod"]
                ?? order.PaymentMethod;

            return View(order);
        }

        // ── POST /Order/ProcessPayment ──────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ProcessPayment(
            PaymentViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            string fullName =
                HttpContext.Session
                    .GetString("UserFullName")
                ?? "";

            ViewBag.UserFullName = fullName;

            if (string.IsNullOrEmpty(
                    model.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Please select a " +
                    "payment method.");
                model.UserFullName = fullName;
                model.UserEmail =
                    HttpContext.Session
                        .GetString("UserEmail")
                    ?? "";
                return View("Payment", model);
            }

            var result = await _payment
                .ProcessPaymentAsync(model);

            if (result.Success)
            {
                int uid = HttpContext.Session
                    .GetInt32("UserId") ?? 0;

                _db.Notifications.Add(
                    new Notification
                    {
                        UserId = uid,
                        Title =
                            "Payment Successful!",
                        Message =
                            "Payment of " +
                            model.Amount
                                .ToString("N0") +
                            " BDT via " +
                            model.PaymentMethod +
                            " for order #" +
                            model.OrderId +
                            " confirmed. TXN: " +
                            result.TransactionId,
                        Icon = "💳",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                _db.SaveChanges();

                TempData["PaymentSuccess"] =
                    "true";
                TempData["TransactionId"] =
                    result.TransactionId;
                TempData["PaymentMethod"] =
                    model.PaymentMethod;

                return RedirectToAction(
                    "Confirmation",
                    new
                    {
                        orderId = model.OrderId
                    });
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message);
                model.UserFullName = fullName;
                model.UserEmail =
                    HttpContext.Session
                        .GetString("UserEmail")
                    ?? "";
                return View("Payment", model);
            }
        }

        // ── GET /Order/Confirmation/5 ───────────
        public IActionResult Confirmation(
            int orderId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var orderItems = _db.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToList();

            var order = _db.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => new Order
                {
                    OrderId = o.OrderId,
                    RestaurantName =
                       o.RestaurantName,
                    RestaurantId = o.RestaurantId,
                    TotalAmount = o.TotalAmount,
                    DeliveryFee = o.DeliveryFee,
                    GrandTotal = o.GrandTotal,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    TransactionId = o.TransactionId,
                    DeliveryAddress =
                        o.DeliveryAddress,
                    OrderNote = o.OrderNote,
                    OrderDate = o.OrderDate
                })
                .FirstOrDefault();

            if (order == null)
                return NotFound();

            order.OrderItems = orderItems;

            ViewBag.UserFullName =
                HttpContext.Session
                    .GetString("UserFullName");
            ViewBag.UserEmail =
                HttpContext.Session
                    .GetString("UserEmail");
            ViewBag.PaymentSuccess =
                TempData["PaymentSuccess"];
            ViewBag.TransactionId =
                TempData["TransactionId"]
                ?? order.TransactionId;
            ViewBag.PaymentMethod =
                TempData["PaymentMethod"]
                ?? order.PaymentMethod;

            return View(order);
        }

        // ── POST /Order/CancelOrder ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(
            int orderId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int userId = HttpContext.Session
                .GetInt32("UserId") ?? 0;

            var order = _db.Orders.FirstOrDefault(
                o => o.OrderId == orderId &&
                     o.UserId == userId);

            if (order == null)
            {
                TempData["Error"] =
                    "Order not found.";
                return RedirectToAction(
                    "MyOrders", "Dashboard");
            }

            if (order.Status == "Delivered")
            {
                TempData["Error"] =
                    "Delivered orders cannot " +
                    "be cancelled.";
                return RedirectToAction(
                    "MyOrders", "Dashboard");
            }

            var items = _db.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToList();

            _db.OrderItems.RemoveRange(items);
            _db.Orders.Remove(order);

            _db.Notifications.Add(
                new Notification
                {
                    UserId = userId,
                    Title = "Order Cancelled",
                    Message =
                        "Order #" + orderId +
                        " cancelled successfully.",
                    Icon = "❌",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Order #" + orderId +
                " has been cancelled.";

            return RedirectToAction(
                "MyOrders", "Dashboard");
        }
    }
}
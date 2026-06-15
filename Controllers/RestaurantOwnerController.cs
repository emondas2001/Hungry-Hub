using HungryHub.Data;
using HungryHub.Models;
using HungryHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryHub.Controllers
{
    public class RestaurantOwnerController
        : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ImageService _imageService;

        public RestaurantOwnerController(
            ApplicationDbContext db,
            ImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        // ── Session helpers ──────────────────────
        private bool IsOwnerLoggedIn() =>
            HttpContext.Session
                .GetString("OwnerEmail") != null;

        private int OwnerId() =>
            HttpContext.Session
                .GetInt32("OwnerId") ?? 0;

        private int OwnerRestaurantId() =>
            HttpContext.Session
                .GetInt32("OwnerRestaurantId") ?? 0;

        private void SetOwnerViewBag()
        {
            ViewBag.OwnerName =
                HttpContext.Session
                    .GetString("OwnerName");
            ViewBag.OwnerEmail =
                HttpContext.Session
                    .GetString("OwnerEmail");
            ViewBag.OwnerRestaurantName =
                HttpContext.Session
                    .GetString("OwnerRestaurantName");
            ViewBag.OwnerRestaurantId =
                OwnerRestaurantId();

            // Pending orders count
            int rid = OwnerRestaurantId();
            ViewBag.PendingOrdersCount =
                _db.Orders.Count(o =>
                    o.RestaurantId == rid &&
                    o.Status == "Confirmed");
        }

        private void LogActivity(
            string action,
            string? details = null)
        {
            _db.RestaurantActivityLogs.Add(
                new RestaurantActivityLog
                {
                    RestaurantId =
                        OwnerRestaurantId(),
                    OwnerId = OwnerId(),
                    Action = action,
                    Details = details,
                    CreatedAt = DateTime.Now
                });
            _db.SaveChanges();
        }

        // ════════════════════════════════════════
        // LOGIN / LOGOUT
        // ════════════════════════════════════════

        [HttpGet]
        public IActionResult Login()
        {
            if (IsOwnerLoggedIn())
                return RedirectToAction(
                    "Dashboard");
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];
            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            OwnerLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var owner = _db.RestaurantOwners
                .FirstOrDefault(o =>
                    o.Email == model.Email &&
                    o.IsActive);

            if (owner == null)
            {
                ModelState.AddModelError(
                    "Email",
                    "No active account found " +
                    "with this email.");
                return View(model);
            }

            bool match = BCrypt.Net.BCrypt
                .Verify(
                    model.Password,
                    owner.PasswordHash);

            if (!match)
            {
                ModelState.AddModelError(
                    "Password",
                    "Incorrect password.");
                return View(model);
            }

            var restaurant = _db.Restaurants
                .FirstOrDefault(r =>
                    r.RestaurantId ==
                    owner.RestaurantId);

            HttpContext.Session.SetString(
                "OwnerEmail", owner.Email);
            HttpContext.Session.SetString(
                "OwnerName", owner.FullName);
            HttpContext.Session.SetInt32(
                "OwnerId", owner.OwnerId);
            HttpContext.Session.SetInt32(
                "OwnerRestaurantId",
                owner.RestaurantId);
            HttpContext.Session.SetString(
                "OwnerRestaurantName",
                restaurant?.Name ?? "Restaurant");

            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session
                .Remove("OwnerEmail");
            HttpContext.Session
                .Remove("OwnerName");
            HttpContext.Session
                .Remove("OwnerId");
            HttpContext.Session
                .Remove("OwnerRestaurantId");
            HttpContext.Session
                .Remove("OwnerRestaurantName");
            TempData["SuccessMessage"] =
                "Logged out successfully.";
            return RedirectToAction("Login");
        }

        // ════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════

        public IActionResult Dashboard()
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            SetOwnerViewBag();
            int rid = OwnerRestaurantId();

            var restaurant =
                _db.Restaurants.FirstOrDefault(
                r => r.RestaurantId == rid);

            if (restaurant == null)
                return RedirectToAction("Login");

            ViewBag.Restaurant = restaurant;

            // Stats
            ViewBag.TotalMenuItems =
                _db.MenuItems.Count(
                    m => m.RestaurantId == rid);

            ViewBag.TotalOrders =
                _db.Orders.Count(
                    o => o.RestaurantId == rid);

            ViewBag.TodayOrders =
                _db.Orders.Count(o =>
                    o.RestaurantId == rid &&
                    o.OrderDate.Date ==
                    DateTime.Today);

            ViewBag.TotalRevenue =
                _db.Orders
                    .Where(o =>
                        o.RestaurantId == rid)
                    .Sum(o =>
                        (decimal?)o.GrandTotal)
                ?? 0;

            ViewBag.AvgRating =
                _db.Ratings.Any(r =>
                    r.RestaurantId == rid)
                ? Math.Round(
                    _db.Ratings
                        .Where(r =>
                            r.RestaurantId == rid)
                        .Average(r =>
                            (double)r.Stars), 1)
                : 0.0;

            // Recent orders
            ViewBag.RecentOrders =
                _db.Orders
                    .Where(o =>
                        o.RestaurantId == rid)
                    .OrderByDescending(
                        o => o.OrderDate)
                    .Take(10)
                    .Select(o => new
                    {
                        o.OrderId,
                        o.GrandTotal,
                        o.Status,
                        o.OrderDate,
                        o.DeliveryAddress,
                        UserEmail = _db.Users
                            .Where(u =>
                                u.UserId == o.UserId)
                            .Select(u => u.Email)
                            .FirstOrDefault()
                            ?? "Unknown"
                    })
                    .ToList();

            // Recent reviews
            ViewBag.RecentRatings =
                _db.Ratings
                    .Where(r =>
                        r.RestaurantId == rid)
                    .OrderByDescending(
                        r => r.RatedAt)
                    .Take(5)
                    .Select(r => new
                    {
                        r.Stars,
                        r.Comment,
                        r.RatedAt,
                        UserEmail = _db.Users
                            .Where(u =>
                                u.UserId == r.UserId)
                            .Select(u => u.Email)
                            .FirstOrDefault()
                            ?? "Unknown"
                    })
                    .ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        // ════════════════════════════════════════
        // MENU MANAGEMENT
        // ════════════════════════════════════════

        public IActionResult Menu(string? search)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            SetOwnerViewBag();
            int rid = OwnerRestaurantId();

            var items = _db.MenuItems
                .Where(m => m.RestaurantId == rid)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                items = items.Where(m =>
                    m.Name.Contains(search) ||
                    m.Category.Contains(search));

            ViewBag.MenuItems =
                items.OrderBy(m => m.Category)
                     .ThenBy(m => m.Name)
                     .ToList();

            ViewBag.Categories =
                _db.MenuItems
                    .Where(m =>
                        m.RestaurantId == rid)
                    .Select(m => m.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

            ViewBag.Search = search;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMenuItem(
            string name,
            string category,
            decimal price,
            string? description,
            string? icon,
            IFormFile? foodImage)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            string imagePath = string.Empty;
            if (foodImage != null &&
                foodImage.Length > 0)
            {
                imagePath = await _imageService
                    .SaveMenuImageAsync(foodImage);
            }

            _db.MenuItems.Add(new MenuItem
            {
                RestaurantId = rid,
                Name = name,
                Category = category,
                Price = price,
                Description = description ?? "",
                Icon = icon ?? "🍽️",
                ImagePath = imagePath,
                IsAvailable = true
            });
            _db.SaveChanges();

            LogActivity(
                "Added menu item",
                "'" + name + "' at ৳" +
                price.ToString("N0"));

            TempData["SuccessMessage"] =
                "'" + name + "' added!";
            return RedirectToAction("Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UpdateMenuItem(
            int menuItemId,
            string name,
            string category,
            decimal price,
            string? description,
            string? icon,
            IFormFile? foodImage)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            var item = _db.MenuItems.FirstOrDefault(
                m => m.MenuItemId == menuItemId &&
                     m.RestaurantId == rid);

            if (item == null)
            {
                TempData["Error"] =
                    "Item not found.";
                return RedirectToAction("Menu");
            }

            item.Name = name;
            item.Category = category;
            item.Price = price;
            item.Description = description ?? "";
            item.Icon = icon ?? item.Icon;

            if (foodImage != null &&
                foodImage.Length > 0)
            {
                _imageService
                    .DeleteImage(item.ImagePath);
                item.ImagePath = await _imageService
                    .SaveMenuImageAsync(foodImage);
            }

            _db.SaveChanges();

            LogActivity(
                "Updated menu item",
                "'" + name + "'");

            TempData["SuccessMessage"] =
                "'" + name + "' updated!";
            return RedirectToAction("Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleMenuItem(
            int menuItemId)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            var item = _db.MenuItems.FirstOrDefault(
                m => m.MenuItemId == menuItemId &&
                     m.RestaurantId == rid);

            if (item != null)
            {
                item.IsAvailable = !item.IsAvailable;
                _db.SaveChanges();

                LogActivity(
                    item.IsAvailable
                        ? "Enabled menu item"
                        : "Disabled menu item",
                    "'" + item.Name + "'");
            }

            TempData["SuccessMessage"] =
                "Item updated.";
            return RedirectToAction("Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMenuItem(
            int menuItemId)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            var item = _db.MenuItems.FirstOrDefault(
                m => m.MenuItemId == menuItemId &&
                     m.RestaurantId == rid);

            if (item != null)
            {
                string name = item.Name;
                _imageService
                    .DeleteImage(item.ImagePath);
                _db.MenuItems.Remove(item);
                _db.SaveChanges();

                LogActivity(
                    "Deleted menu item",
                    "'" + name + "'");

                TempData["SuccessMessage"] =
                    "'" + name + "' deleted.";
            }

            return RedirectToAction("Menu");
        }

        // ════════════════════════════════════════
        // RESTAURANT PROFILE
        // ════════════════════════════════════════

        public IActionResult Profile()
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            SetOwnerViewBag();
            int rid = OwnerRestaurantId();

            var restaurant =
                _db.Restaurants.FirstOrDefault(
                r => r.RestaurantId == rid);

            var hours = _db.RestaurantHours
                .Where(h => h.RestaurantId == rid)
                .OrderBy(h => h.DayOfWeek)
                .ToList();

            ViewBag.Restaurant = restaurant;
            ViewBag.Hours = hours;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UpdateProfile(
            string description,
            string phone,
            string address,
            IFormFile? restaurantImage)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            var r = _db.Restaurants.FirstOrDefault(
                x => x.RestaurantId == rid);

            if (r == null)
                return RedirectToAction("Profile");

            r.Description = description;
            r.Phone = phone;
            r.Address = address;

            if (restaurantImage != null &&
                restaurantImage.Length > 0)
            {
                _imageService
                    .DeleteImage(r.ImagePath);
                r.ImagePath = await _imageService
                    .SaveRestaurantImageAsync(
                        restaurantImage);
            }

            _db.SaveChanges();

            LogActivity(
                "Updated restaurant profile");

            TempData["SuccessMessage"] =
                "Profile updated!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleOpen()
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            var r = _db.Restaurants.FirstOrDefault(
                x => x.RestaurantId == rid);

            if (r != null)
            {
                r.IsOpen = !r.IsOpen;
                _db.SaveChanges();

                LogActivity(
                    r.IsOpen
                        ? "Opened restaurant"
                        : "Closed restaurant");
            }

            TempData["SuccessMessage"] =
                "Restaurant status updated.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHours(
            List<int> hoursIds,
            List<string> openTimes,
            List<string> closeTimes,
            List<bool> isCloseds)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            for (int i = 0;
                 i < hoursIds.Count; i++)
            {
                var h = _db.RestaurantHours
                    .FirstOrDefault(x =>
                        x.HoursId == hoursIds[i]);
                if (h == null) continue;

                h.IsClosed =
                    i < isCloseds.Count &&
                    isCloseds[i];

                if (!h.IsClosed)
                {
                    if (i < openTimes.Count &&
                        TimeOnly.TryParse(
                            openTimes[i],
                            out var ot))
                        h.OpenTime = ot;

                    if (i < closeTimes.Count &&
                        TimeOnly.TryParse(
                            closeTimes[i],
                            out var ct))
                        h.CloseTime = ct;
                }
            }

            _db.SaveChanges();
            LogActivity("Updated opening hours");

            TempData["SuccessMessage"] =
                "Opening hours saved!";
            return RedirectToAction("Profile");
        }

        // ════════════════════════════════════════
        // ORDERS
        // ════════════════════════════════════════

        public IActionResult Orders(
            string? status)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            SetOwnerViewBag();
            int rid = OwnerRestaurantId();

            var query = _db.Orders
                .Where(o => o.RestaurantId == rid)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(
                    o => o.Status == status);

            var rawOrders = query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.GrandTotal,
                    o.TotalAmount,
                    o.DeliveryFee,
                    Status =
                        o.Status ?? "Pending",
                    DeliveryAddress =
                        o.DeliveryAddress ?? "",
                    OrderNote =
                        o.OrderNote ?? "",
                    PaymentMethod =
                        o.PaymentMethod ?? "Cash",
                    PaymentStatus =
                        o.PaymentStatus ?? "Pending",
                    TransactionId =
                        o.TransactionId ?? "",
                    o.OrderDate,
                    UserEmail = _db.Users
                        .Where(u =>
                            u.UserId == o.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefault()
                        ?? "Unknown",
                    ItemCount = _db.OrderItems
                        .Count(i =>
                            i.OrderId == o.OrderId)
                })
                .ToList();

            ViewBag.OrderList = rawOrders;
            ViewBag.FilterStatus = status ?? "";

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(
            int orderId, string newStatus)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int rid = OwnerRestaurantId();

            var order = _db.Orders.FirstOrDefault(
                o => o.OrderId == orderId &&
                     o.RestaurantId == rid);

            if (order != null)
            {
                order.Status = newStatus;

                _db.Notifications.Add(
                    new Notification
                    {
                        UserId = order.UserId,
                        Title = "Order Updated",
                        Message =
                            "Your order #" +
                            orderId +
                            " from " +
                            (HttpContext.Session
                                .GetString(
                                "OwnerRestaurantName")
                             ?? "Restaurant") +
                            " is now: " +
                            newStatus + ".",
                        Icon =
                            newStatus == "Delivered"
                                ? "✅" : "📦",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });

                _db.SaveChanges();

                LogActivity(
                    "Updated order #" +
                    orderId + " to " + newStatus);

                TempData["SuccessMessage"] =
                    "Order #" + orderId +
                    " updated to " + newStatus + ".";
            }

            return RedirectToAction("Orders");
        }

        // ════════════════════════════════════════
        // REVIEWS
        // ════════════════════════════════════════

        public IActionResult Reviews()
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            SetOwnerViewBag();
            int rid = OwnerRestaurantId();

            var ratings = _db.Ratings
                .Where(r => r.RestaurantId == rid)
                .OrderByDescending(r => r.RatedAt)
                .Select(r => new
                {
                    r.RatingId,
                    r.Stars,
                    r.Comment,
                    r.RatedAt,
                    UserEmail = _db.Users
                        .Where(u =>
                            u.UserId == r.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefault()
                        ?? "Unknown"
                })
                .ToList();

            ViewBag.Ratings = ratings;

            ViewBag.AvgRating = ratings.Any()
                ? Math.Round(
                    ratings.Average(
                        r => (double)r.Stars), 1)
                : 0.0;

            ViewBag.StarCounts = new int[6];
            for (int s = 1; s <= 5; s++)
            {
                ViewBag.StarCounts[s] =
                    ratings.Count(
                        r => r.Stars == s);
            }

            return View();
        }

        // ════════════════════════════════════════
        // CHANGE PASSWORD
        // ════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(
            string currentPassword,
            string newPassword)
        {
            if (!IsOwnerLoggedIn())
                return RedirectToAction("Login");

            int ownerId = OwnerId();

            var owner = _db.RestaurantOwners
                .FirstOrDefault(o =>
                    o.OwnerId == ownerId);

            if (owner == null)
                return RedirectToAction("Login");

            bool correct = BCrypt.Net.BCrypt
                .Verify(
                    currentPassword,
                    owner.PasswordHash);

            if (!correct)
            {
                TempData["Error"] =
                    "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            owner.PasswordHash =
                BCrypt.Net.BCrypt
                    .HashPassword(newPassword);
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Password changed successfully!";
            return RedirectToAction("Profile");
        }
    }
}
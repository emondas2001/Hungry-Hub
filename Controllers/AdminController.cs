using HungryHub.Data;
using HungryHub.Models;
using HungryHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryHub.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly EmailReportService _emailSvc;
        private readonly ImageService _imageService;

        // ── ONE constructor only ─────────────────────
        public AdminController(
            ApplicationDbContext db,
            IConfiguration config,
            EmailReportService emailSvc,
            ImageService imageService)
        {
            _db = db;
            _config = config;
            _emailSvc = emailSvc;
            _imageService = imageService;
        }

        private bool IsAdminLoggedIn() =>
            HttpContext.Session
                .GetString("AdminEmail") != null;

        private void SetAdminViewBag()
        {
            ViewBag.AdminFullName =
                HttpContext.Session
                    .GetString("AdminFullName");
            ViewBag.AdminEmail =
                HttpContext.Session
                    .GetString("AdminEmail");
            ViewBag.TotalUsers = _db.Users.Count();
            ViewBag.TotalOrders = _db.Orders.Count();
            ViewBag.PendingOrders = _db.Orders
                .Count(o => o.Status == "Confirmed");
        }

        // ════════════════════════════════════════════
        // SETUP
        // ════════════════════════════════════════════

        [HttpGet]
        public IActionResult Setup()
        {
            if (_db.Admins.Any())
            {
                TempData["Error"] =
                    "Admin account already exists.";
                return RedirectToAction("Login");
            }

            var vm = new Admin
            {
                FirstName =
                    _config["AdminSetup:FirstName"]!,
                LastName =
                    _config["AdminSetup:LastName"]!,
                Email =
                    _config["AdminSetup:Email"]!,
                PhoneNumber =
                    _config["AdminSetup:PhoneNumber"]!,
                Gender =
                    _config["AdminSetup:Gender"]!,
                DateOfBirth = DateTime.Parse(
                    _config["AdminSetup:DateOfBirth"]!)
            };

            ViewBag.PlainPassword =
                _config["AdminSetup:PlainPassword"];
            return View(vm);
        }

        [HttpPost]
        public IActionResult SetupConfirm()
        {
            if (_db.Admins.Any())
                return RedirectToAction("Login");

            string plain =
                _config["AdminSetup:PlainPassword"]!;
            string hash =
                BCrypt.Net.BCrypt.HashPassword(plain);

            _db.Admins.Add(new Admin
            {
                FirstName =
                    _config["AdminSetup:FirstName"]!,
                LastName =
                    _config["AdminSetup:LastName"]!,
                Email =
                    _config["AdminSetup:Email"]!,
                PhoneNumber =
                    _config["AdminSetup:PhoneNumber"]!,
                Gender =
                    _config["AdminSetup:Gender"]!,
                DateOfBirth = DateTime.Parse(
                    _config["AdminSetup:DateOfBirth"]!),
                PasswordHash = hash,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Admin account created! Please login.";
            return RedirectToAction("Login");
        }

        // ════════════════════════════════════════════
        // LOGIN / LOGOUT
        // ════════════════════════════════════════════

        [HttpGet]
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
                return RedirectToAction("Dashboard");

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(
            AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = _db.Admins.FirstOrDefault(
                a => a.Email == model.Email &&
                     a.IsActive);

            if (admin == null)
            {
                ModelState.AddModelError("Email",
                    "No active admin account found.");
                return View(model);
            }

            bool match = BCrypt.Net.BCrypt.Verify(
                model.Password, admin.PasswordHash);

            if (!match)
            {
                ModelState.AddModelError("Password",
                    "Incorrect password.");
                return View(model);
            }

            HttpContext.Session.SetString(
                "AdminEmail", admin.Email);
            HttpContext.Session.SetString(
                "AdminFullName",
                admin.FirstName + " " +
                admin.LastName);
            HttpContext.Session.SetInt32(
                "AdminId", admin.AdminId);

            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminEmail");
            HttpContext.Session.Remove("AdminFullName");
            HttpContext.Session.Remove("AdminId");
            TempData["SuccessMessage"] =
                "Logged out successfully.";
            return RedirectToAction("Login");
        }

        // ════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════

        public IActionResult Dashboard()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var now = DateTime.Now;
            var weekStart = DateTime.Today.AddDays(-7);
            var monthStart =
                new DateTime(now.Year, now.Month, 1);

            ViewBag.TotalRevenue = _db.Orders
                .Sum(o => (decimal?)o.GrandTotal)
                ?? 0;
            ViewBag.WeekRevenue = _db.Orders
                .Where(o => o.OrderDate >= weekStart)
                .Sum(o => (decimal?)o.GrandTotal)
                ?? 0;
            ViewBag.MonthRevenue = _db.Orders
                .Where(o => o.OrderDate >= monthStart)
                .Sum(o => (decimal?)o.GrandTotal)
                ?? 0;
            ViewBag.TotalRestaurants =
                _db.Restaurants.Count(r => r.IsActive);
            ViewBag.TotalMenuItems =
                _db.MenuItems.Count();
            ViewBag.TotalRatings =
                _db.Ratings.Count();
            ViewBag.CancelledOrders = _db.Orders
                .Count(o => o.Status == "Cancelled");
            ViewBag.DeliveredOrders = _db.Orders
                .Count(o => o.Status == "Delivered");
            ViewBag.WeekOrders = _db.Orders
                .Count(o => o.OrderDate >= weekStart);
            ViewBag.WeekUsers = _db.Users
                .Count(u => u.CreatedAt >= weekStart);

            ViewBag.AvgOrderValue = _db.Orders.Any()
                ? Math.Round((double)(
                    _db.Orders.Average(o =>
                        (decimal?)o.GrandTotal)
                    ?? 0), 0)
                : 0;

            ViewBag.AvgRating = _db.Ratings.Any()
                ? Math.Round(
                    _db.Ratings.Average(
                        r => (double)r.Stars), 1)
                : 0.0;

            ViewBag.RecentOrders = _db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(8)
                .Select(o => new {
                    o.OrderId,
                    o.RestaurantName,
                    o.GrandTotal,
                    o.Status,
                    o.OrderDate,
                    UserEmail = _db.Users
                        .Where(u =>
                            u.UserId == o.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefault()
                        ?? "Unknown"
                })
                .ToList();

            ViewBag.RecentUsers = _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(6).ToList();

            ViewBag.RestaurantRevenue = _db.Orders
                .GroupBy(o => o.RestaurantName)
                .Select(g => new {
                    Name = g.Key,
                    Revenue = g.Sum(o => o.GrandTotal),
                    Orders = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(6).ToList();

            var last7 = Enumerable.Range(0, 7)
                .Select(i =>
                    DateTime.Today.AddDays(-i))
                .Reverse().ToList();

            ViewBag.OrdersPerDay = last7
                .Select(day => new {
                    Date = day.ToString("MMM dd"),
                    Count = _db.Orders.Count(o =>
                        o.OrderDate.Date == day.Date)
                }).ToList();

            ViewBag.RevenuePerDay = last7
                .Select(day => new {
                    Date = day.ToString("MMM dd"),
                    Revenue = (double)(_db.Orders
                        .Where(o =>
                            o.OrderDate.Date ==
                            day.Date)
                        .Sum(o =>
                            (decimal?)o.GrandTotal)
                        ?? 0)
                }).ToList();

            ViewBag.OrdersByStatus = new[] {
                new {
                    Status = "Confirmed",
                    Count  = _db.Orders.Count(
                        o => o.Status == "Confirmed")
                },
                new {
                    Status = "Delivered",
                    Count  = _db.Orders.Count(
                        o => o.Status == "Delivered")
                },
                new {
                    Status = "Cancelled",
                    Count  = _db.Orders.Count(
                        o => o.Status == "Cancelled")
                }
            };

            ViewBag.TopMenuItems = _db.OrderItems
                .GroupBy(i => i.ItemName)
                .Select(g => new {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5).ToList();

            ViewBag.PeakHours =
                Enumerable.Range(0, 24)
                .Select(h => new {
                    Hour = h,
                    Count = _db.Orders.Count(
                        o => o.OrderDate.Hour == h)
                })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .Take(5).ToList();

            ViewBag.UserGrowth =
                Enumerable.Range(0, 4)
                .Select(w => {
                    var ws = DateTime.Today
                        .AddDays(-(w + 1) * 7);
                    var we = DateTime.Today
                        .AddDays(-w * 7);
                    return new
                    {
                        Week = "Week -" + (w + 1),
                        Count = _db.Users.Count(u =>
                            u.CreatedAt >= ws &&
                            u.CreatedAt < we)
                    };
                })
                .Reverse().ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // ── Send weekly report now ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            SendReportNow()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            try
            {
                await _emailSvc
                    .SendWeeklyReportAsync();
                TempData["SuccessMessage"] =
                    "Weekly report sent!";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Failed: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }

        // ════════════════════════════════════════════
        // RESTAURANTS
        // ════════════════════════════════════════════

        public IActionResult Restaurants(
            string? search)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var list = _db.Restaurants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(r =>
                    r.Name.Contains(search) ||
                    r.Cuisine.Contains(search) ||
                    r.Address.Contains(search));

            ViewBag.Search = search;
            ViewBag.RestaurantList =
                list.OrderBy(r => r.Name).ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            CreateRestaurant(
            RestaurantDb model,
            IFormFile? restaurantImage)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            string imagePath = string.Empty;
            if (restaurantImage != null &&
                restaurantImage.Length > 0)
            {
                imagePath = await _imageService
                    .SaveRestaurantImageAsync(
                        restaurantImage);
            }

            _db.Restaurants.Add(new RestaurantDb
            {
                Name = model.Name,
                Cuisine = model.Cuisine,
                Address = model.Address,
                Phone = model.Phone,
                Description = model.Description,
                DeliveryFee = model.DeliveryFee,
                DeliveryTime = model.DeliveryTime,
                MinOrder = model.MinOrder,
                ImageUrl = string.Empty,
                ImagePath = imagePath,
                Tag = model.Tag
                               ?? string.Empty,
                IsOpen = true,
                IsActive = true,
                Rating = 0,
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            var newRest = _db.Restaurants
                .OrderByDescending(r =>
                    r.RestaurantId)
                .First();

            for (int d = 0; d <= 6; d++)
            {
                _db.RestaurantHours.Add(
                    new RestaurantHours
                    {
                        RestaurantId =
                            newRest.RestaurantId,
                        DayOfWeek = d,
                        OpenTime = new TimeOnly(9, 0),
                        CloseTime = new TimeOnly(23, 0),
                        IsClosed = false
                    });
            }
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Restaurant '" +
                model.Name + "' created!";
            return RedirectToAction("Restaurants");
        }

        [HttpGet]
        public IActionResult EditRestaurant(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var r = _db.Restaurants.FirstOrDefault(
                x => x.RestaurantId == id);
            if (r == null)
                return RedirectToAction("Restaurants");

            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            EditRestaurant(
            RestaurantDb model,
            IFormFile? restaurantImage)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var r = _db.Restaurants.FirstOrDefault(
                x => x.RestaurantId ==
                     model.RestaurantId);
            if (r == null)
                return RedirectToAction("Restaurants");

            r.Name = model.Name;
            r.Cuisine = model.Cuisine;
            r.Address = model.Address;
            r.Phone = model.Phone;
            r.Description = model.Description;
            r.DeliveryFee = model.DeliveryFee;
            r.DeliveryTime = model.DeliveryTime;
            r.MinOrder = model.MinOrder;
            r.Tag = model.Tag
                             ?? string.Empty;

            if (restaurantImage != null &&
                restaurantImage.Length > 0)
            {
                _imageService.DeleteImage(r.ImagePath);
                r.ImagePath = await _imageService
                    .SaveRestaurantImageAsync(
                        restaurantImage);
            }

            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "'" + r.Name + "' updated!";
            return RedirectToAction("Restaurants");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleRestaurant(
            int restaurantId, string toggleType)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var r = _db.Restaurants.FirstOrDefault(
                x => x.RestaurantId == restaurantId);
            if (r != null)
            {
                if (toggleType == "active")
                    r.IsActive = !r.IsActive;
                else
                    r.IsOpen = !r.IsOpen;
                _db.SaveChanges();
            }
            return RedirectToAction("Restaurants");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteRestaurant(
            int restaurantId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var r = _db.Restaurants.FirstOrDefault(
                x => x.RestaurantId == restaurantId);
            if (r != null)
            {
                _imageService.DeleteImage(r.ImagePath);
                _db.MenuItems.RemoveRange(
                    _db.MenuItems.Where(m =>
                        m.RestaurantId ==
                        restaurantId));
                _db.RestaurantHours.RemoveRange(
                    _db.RestaurantHours.Where(h =>
                        h.RestaurantId ==
                        restaurantId));
                _db.Restaurants.Remove(r);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Restaurant deleted.";
            }
            return RedirectToAction("Restaurants");
        }

        public IActionResult RestaurantHours(
            int restaurantId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var rest = _db.Restaurants.FirstOrDefault(
                r => r.RestaurantId == restaurantId);
            if (rest == null)
                return RedirectToAction("Restaurants");

            var hours = _db.RestaurantHours
                .Where(h =>
                    h.RestaurantId == restaurantId)
                .OrderBy(h => h.DayOfWeek)
                .ToList();

            ViewBag.Restaurant = rest;
            ViewBag.Hours = hours;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveHours(
            int restaurantId,
            List<int> hoursIds,
            List<string> openTimes,
            List<string> closeTimes,
            List<bool> isCloseds)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            for (int i = 0; i < hoursIds.Count; i++)
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
            TempData["SuccessMessage"] =
                "Opening hours saved!";
            return RedirectToAction(
                "RestaurantHours",
                new { restaurantId });
        }

        public IActionResult RestaurantMenu(
            int restaurantId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var rest = _db.Restaurants.FirstOrDefault(
                r => r.RestaurantId == restaurantId);
            if (rest == null)
                return RedirectToAction("Restaurants");

            var items = _db.MenuItems
                .Where(m =>
                    m.RestaurantId == restaurantId)
                .OrderBy(m => m.Category)
                .ToList();

            ViewBag.Restaurant = rest;
            ViewBag.ItemList = items;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        // ════════════════════════════════════════════
        // USERS
        // ════════════════════════════════════════════

        public IActionResult Users(string? search)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var users = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                users = users.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.PhoneNumber.Contains(search));

            ViewBag.Search = search;
            ViewBag.UserList = users
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
            ViewBag.UserOrderCounts = _db.Orders
                .GroupBy(o => o.UserId)
                .ToDictionary(
                    g => g.Key, g => g.Count());

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int userId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var user = _db.Users.FirstOrDefault(
                u => u.UserId == userId);
            if (user != null)
            {
                var orders = _db.Orders
                    .Where(o => o.UserId == userId)
                    .ToList();
                foreach (var o in orders)
                {
                    _db.OrderItems.RemoveRange(
                        _db.OrderItems.Where(i =>
                            i.OrderId == o.OrderId));
                }
                _db.Orders.RemoveRange(orders);
                _db.Favourites.RemoveRange(
                    _db.Favourites.Where(f =>
                        f.UserId == userId));
                _db.Ratings.RemoveRange(
                    _db.Ratings.Where(r =>
                        r.UserId == userId));
                _db.Notifications.RemoveRange(
                    _db.Notifications.Where(n =>
                        n.UserId == userId));
                _db.Users.Remove(user);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "User deleted.";
            }
            return RedirectToAction("Users");
        }

        // ════════════════════════════════════════════
        // ORDERS
        // ════════════════════════════════════════════

        public IActionResult Orders(
            string? status, string? search)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var orders = _db.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                orders = orders.Where(
                    o => o.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
                orders = orders.Where(o =>
                    o.RestaurantName
                        .Contains(search) ||
                    o.DeliveryAddress
                        .Contains(search));

            ViewBag.FilterStatus = status ?? "";
            ViewBag.Search = search ?? "";
            ViewBag.OrderList = orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new {
                    o.OrderId,
                    o.UserId,
                    o.RestaurantName,
                    o.TotalAmount,
                    o.DeliveryFee,
                    o.GrandTotal,
                    o.Status,
                    o.DeliveryAddress,
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
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var order = _db.Orders.FirstOrDefault(
                o => o.OrderId == orderId);
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
                            " is now: " +
                            newStatus + ".",
                        Icon =
                            newStatus == "Delivered"
                                ? "✅" : "📦",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Order #" + orderId +
                    " updated to " +
                    newStatus + ".";
            }
            return RedirectToAction("Orders");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteOrder(int orderId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var order = _db.Orders.FirstOrDefault(
                o => o.OrderId == orderId);
            if (order != null)
            {
                _db.OrderItems.RemoveRange(
                    _db.OrderItems.Where(i =>
                        i.OrderId == orderId));
                _db.Orders.Remove(order);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Order #" + orderId +
                    " deleted.";
            }
            return RedirectToAction("Orders");
        }

        // ════════════════════════════════════════════
        // MENU ITEMS
        // ════════════════════════════════════════════

        public IActionResult MenuItems(
            string? search)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var items = _db.MenuItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                items = items.Where(m =>
                    m.Name.Contains(search) ||
                    m.Category.Contains(search));

            ViewBag.Search = search;
            ViewBag.ItemList = items
                .OrderBy(m => m.RestaurantId)
                .ThenBy(m => m.Category)
                .ToList();

            ViewBag.RestaurantNames = _db.Restaurants
                .ToDictionary(
                    r => r.RestaurantId, r => r.Name);

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMenuItem(
            int restaurantId,
            string name,
            string description,
            decimal price,
            string category,
            string icon,
            IFormFile? foodImage)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            string imagePath = string.Empty;
            if (foodImage != null &&
                foodImage.Length > 0)
            {
                imagePath = await _imageService
                    .SaveMenuImageAsync(foodImage);
            }

            _db.MenuItems.Add(new MenuItem
            {
                RestaurantId = restaurantId,
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                Icon = icon ?? "🍽️",
                ImagePath = imagePath,
                IsAvailable = true
            });
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "'" + name + "' added.";
            return RedirectToAction(
                "RestaurantMenu",
                new { restaurantId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleMenuItem(
            int menuItemId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var item = _db.MenuItems.FirstOrDefault(
                m => m.MenuItemId == menuItemId);
            if (item != null)
            {
                item.IsAvailable = !item.IsAvailable;
                _db.SaveChanges();
            }
            return RedirectToAction("MenuItems");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMenuItem(
            int menuItemId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var item = _db.MenuItems.FirstOrDefault(
                m => m.MenuItemId == menuItemId);
            if (item != null)
            {
                _imageService.DeleteImage(
                    item.ImagePath);
                _db.MenuItems.Remove(item);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Menu item deleted.";
            }
            return RedirectToAction("MenuItems");
        }

        // ════════════════════════════════════════════
        // RATINGS
        // ════════════════════════════════════════════

        public IActionResult Ratings()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            ViewBag.RatingList = _db.Ratings
                .OrderByDescending(r => r.RatedAt)
                .Select(r => new {
                    r.RatingId,
                    r.OrderId,
                    r.RestaurantId,
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

            ViewBag.RestaurantNames = _db.Restaurants
                .ToDictionary(
                    r => r.RestaurantId, r => r.Name);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteRating(
            int ratingId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var r = _db.Ratings.FirstOrDefault(
                x => x.RatingId == ratingId);
            if (r != null)
            {
                _db.Ratings.Remove(r);
                _db.SaveChanges();
            }
            return RedirectToAction("Ratings");
        }

        // ════════════════════════════════════════════
        // SPLIT ORDERS — Admin view
        // ════════════════════════════════════════════

        public IActionResult SplitOrders()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var splits = _db.SplitOrders
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            var splitIds = splits
                .Select(s => s.SplitOrderId).ToList();

            var participants = _db.SplitParticipants
                .Where(p =>
                    splitIds.Contains(p.SplitOrderId))
                .ToList();

            foreach (var s in splits)
            {
                s.Participants = participants
                    .Where(p =>
                        p.SplitOrderId == s.SplitOrderId)
                    .ToList();
            }

            ViewBag.SplitList = splits;

            ViewBag.CreatorEmails = _db.Users
                .Where(u => splits
                    .Select(s => s.CreatorUserId)
                    .Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u.Email);

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSplitAdmin(
            int splitOrderId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var split = _db.SplitOrders.FirstOrDefault(
                s => s.SplitOrderId == splitOrderId);
            if (split != null)
            {
                _db.SplitParticipants.RemoveRange(
                    _db.SplitParticipants.Where(p =>
                        p.SplitOrderId == splitOrderId));
                _db.SplitOrders.Remove(split);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Split deleted.";
            }
            return RedirectToAction("SplitOrders");
        }

        // ════════════════════════════════════════════
        // PRE-ORDERS — Admin view
        // ════════════════════════════════════════════

        public IActionResult PreOrders(
            string? status)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var query = _db.PreOrders.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(
                    p => p.Status == status);

            var list = query
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var preOrderIds = list
                .Select(p => p.PreOrderId).ToList();

            var items = _db.PreOrderItems
                .Where(i =>
                    preOrderIds.Contains(i.PreOrderId))
                .ToList();

            foreach (var po in list)
            {
                po.Items = items
                    .Where(i =>
                        i.PreOrderId == po.PreOrderId)
                    .ToList();
            }

            ViewBag.PreOrderList = list;
            ViewBag.FilterStatus = status ?? "";

            ViewBag.UserEmails = _db.Users
                .Where(u => list
                    .Select(p => p.UserId)
                    .Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u.Email);

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePreOrderStatus(
            int preOrderId,
            string newStatus,
            string? adminNote)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var po = _db.PreOrders.FirstOrDefault(
                p => p.PreOrderId == preOrderId);
            if (po != null)
            {
                po.Status = newStatus;
                po.AdminNote = adminNote ?? "";

                _db.Notifications.Add(new Notification
                {
                    UserId = po.UserId,
                    Title = "Pre-Order " + newStatus,
                    Message =
                        "Your pre-order for " +
                        po.EventName + " has been " +
                        newStatus.ToLower() + "." +
                        (string.IsNullOrEmpty(adminNote)
                            ? ""
                            : " Note: " + adminNote),
                    Icon = newStatus == "Confirmed"
                        ? "✅" : "❌",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Pre-order #" + preOrderId +
                    " updated to " + newStatus + ".";
            }
            return RedirectToAction("PreOrders");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePreOrder(
            int preOrderId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var po = _db.PreOrders.FirstOrDefault(
                p => p.PreOrderId == preOrderId);
            if (po != null)
            {
                _db.PreOrderItems.RemoveRange(
                    _db.PreOrderItems.Where(i =>
                        i.PreOrderId == preOrderId));
                _db.PreOrders.Remove(po);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Pre-order deleted.";
            }
            return RedirectToAction("PreOrders");
        }

        // ════════════════════════════════════════════
        // SUBSCRIPTIONS — Admin view
        // ════════════════════════════════════════════

        public IActionResult Subscriptions()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var subs = _db.UserSubscriptions
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            var planIds = subs
                .Select(s => s.PlanId)
                .Distinct().ToList();

            var plans = _db.MealPlans
                .Where(p => planIds.Contains(p.PlanId))
                .ToList();

            foreach (var sub in subs)
            {
                sub.Plan = plans.FirstOrDefault(
                    p => p.PlanId == sub.PlanId);
            }

            ViewBag.SubList = subs;

            ViewBag.UserEmails = _db.Users
                .Where(u => subs
                    .Select(s => s.UserId)
                    .Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u.Email);

            ViewBag.AllPlans = _db.MealPlans.ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSubscription(
            int subscriptionId, string newStatus)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var sub = _db.UserSubscriptions
                .FirstOrDefault(s =>
                    s.SubscriptionId == subscriptionId);
            if (sub != null)
            {
                sub.Status = newStatus;

                _db.Notifications.Add(new Notification
                {
                    UserId = sub.UserId,
                    Title = "Subscription " + newStatus,
                    Message =
                        "Your meal plan subscription " +
                        "has been " +
                        newStatus.ToLower() +
                        " by admin.",
                    Icon = "🍱",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Subscription updated.";
            }
            return RedirectToAction("Subscriptions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveMealPlan(
            int planId, string planName,
            string description,
            int mealsPerWeek, decimal pricePerWeek)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (planId == 0)
            {
                // Create new
                _db.MealPlans.Add(new MealPlan
                {
                    PlanName = planName,
                    Description = description,
                    MealsPerWeek = mealsPerWeek,
                    PricePerWeek = pricePerWeek,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                // Update existing
                var plan = _db.MealPlans.FirstOrDefault(
                    p => p.PlanId == planId);
                if (plan != null)
                {
                    plan.PlanName = planName;
                    plan.Description = description;
                    plan.MealsPerWeek = mealsPerWeek;
                    plan.PricePerWeek = pricePerWeek;
                }
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] =
                "Meal plan saved.";
            return RedirectToAction("Subscriptions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleMealPlan(int planId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var plan = _db.MealPlans.FirstOrDefault(
                p => p.PlanId == planId);
            if (plan != null)
            {
                plan.IsActive = !plan.IsActive;
                _db.SaveChanges();
            }
            return RedirectToAction("Subscriptions");
        }

        // ════════════════════════════════════════════
        // COUPONS
        // ════════════════════════════════════════════

        public IActionResult Coupons()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            ViewBag.CouponList = _db.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.UsageStats = _db.CouponUsages
                .GroupBy(u => u.CouponId)
                .ToDictionary(
                    g => g.Key,
                    g => new {
                        Count = g.Count(),
                        TotalSaved = g.Sum(x => x.Discount)
                    });

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCoupon(
            string code, string title,
            string description,
            string discountType,
            decimal discountValue,
            decimal minOrderAmount,
            decimal maxDiscount,
            int usageLimit,
            DateTime startDate,
            DateTime expiryDate)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            bool exists = _db.Coupons.Any(
                c => c.Code ==
                     code.ToUpper().Trim());
            if (exists)
            {
                TempData["Error"] =
                    "Coupon code already exists.";
                return RedirectToAction("Coupons");
            }

            _db.Coupons.Add(new Coupon
            {
                Code =
                    code.ToUpper().Trim(),
                Title = title,
                Description = description,
                DiscountType = discountType,
                DiscountValue = discountValue,
                MinOrderAmount = minOrderAmount,
                MaxDiscount = maxDiscount,
                UsageLimit = usageLimit,
                UsedCount = 0,
                StartDate = startDate,
                ExpiryDate = expiryDate,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Coupon '" + code.ToUpper() +
                "' created!";
            return RedirectToAction("Coupons");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCoupon(
            int couponId, string title,
            string description,
            decimal discountValue,
            decimal minOrderAmount,
            decimal maxDiscount,
            int usageLimit,
            DateTime startDate,
            DateTime expiryDate)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var c = _db.Coupons.FirstOrDefault(
                x => x.CouponId == couponId);
            if (c != null)
            {
                c.Title = title;
                c.Description = description;
                c.DiscountValue = discountValue;
                c.MinOrderAmount = minOrderAmount;
                c.MaxDiscount = maxDiscount;
                c.UsageLimit = usageLimit;
                c.StartDate = startDate;
                c.ExpiryDate = expiryDate;
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Coupon updated!";
            }
            return RedirectToAction("Coupons");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleCoupon(int couponId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var c = _db.Coupons.FirstOrDefault(
                x => x.CouponId == couponId);
            if (c != null)
            {
                c.IsActive = !c.IsActive;
                _db.SaveChanges();
            }
            return RedirectToAction("Coupons");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCoupon(int couponId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var c = _db.Coupons.FirstOrDefault(
                x => x.CouponId == couponId);
            if (c != null)
            {
                _db.CouponUsages.RemoveRange(
                    _db.CouponUsages.Where(u =>
                        u.CouponId == couponId));
                _db.Coupons.Remove(c);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Coupon deleted.";
            }
            return RedirectToAction("Coupons");
        }

        // ════════════════════════════════════════════
        // RESTAURANT OWNERS
        // ════════════════════════════════════════════

        public IActionResult RestaurantOwners()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            SetAdminViewBag();

            var owners = _db.RestaurantOwners
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var restIds = owners
                .Select(o => o.RestaurantId)
                .Distinct().ToList();

            var restaurants = _db.Restaurants
                .Where(r => restIds.Contains(
                    r.RestaurantId))
                .ToDictionary(
                    r => r.RestaurantId, r => r.Name);

            ViewBag.OwnerList = owners;
            ViewBag.RestaurantNames = restaurants;

            ViewBag.AllRestaurants = _db.Restaurants
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToList();

            // Activity logs
            ViewBag.RecentLogs =
                _db.RestaurantActivityLogs
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(20)
                    .Select(l => new {
                        l.LogId,
                        l.RestaurantId,
                        l.Action,
                        l.Details,
                        l.CreatedAt,
                        RestaurantName =
                            _db.Restaurants
                                .Where(r =>
                                    r.RestaurantId ==
                                    l.RestaurantId)
                                .Select(r => r.Name)
                                .FirstOrDefault()
                            ?? "Unknown"
                    })
                    .ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRestaurantOwner(
            int restaurantId,
            string fullName,
            string email,
            string password,
            string? phone)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            bool emailExists = _db.RestaurantOwners
                .Any(o => o.Email == email);
            if (emailExists)
            {
                TempData["Error"] =
                    "Email already in use.";
                return RedirectToAction(
                    "RestaurantOwners");
            }

            _db.RestaurantOwners.Add(
                new RestaurantOwner
                {
                    RestaurantId = restaurantId,
                    FullName = fullName,
                    Email = email,
                    PasswordHash =
                        BCrypt.Net.BCrypt
                            .HashPassword(password),
                    Phone = phone,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Owner account created for " +
                fullName + ".";
            return RedirectToAction("RestaurantOwners");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleOwner(int ownerId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var owner = _db.RestaurantOwners
                .FirstOrDefault(o =>
                    o.OwnerId == ownerId);
            if (owner != null)
            {
                owner.IsActive = !owner.IsActive;
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Owner account " +
                    (owner.IsActive
                        ? "activated."
                        : "deactivated.");
            }
            return RedirectToAction("RestaurantOwners");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteOwner(int ownerId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var owner = _db.RestaurantOwners
                .FirstOrDefault(o =>
                    o.OwnerId == ownerId);
            if (owner != null)
            {
                _db.RestaurantOwners.Remove(owner);
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Owner deleted.";
            }
            return RedirectToAction("RestaurantOwners");
        }
    }
}
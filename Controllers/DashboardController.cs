using HungryHub.Data;
using HungryHub.Models;
using HungryHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryHub.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ExcelService _excel;
        private readonly WeatherService _weather;

        public DashboardController(
            ApplicationDbContext db,
            ExcelService excel,
            WeatherService weather)
        {
            _db = db;
            _excel = excel;
            _weather = weather;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session
                .GetString("UserEmail") != null;

        private int UserId() =>
            HttpContext.Session
                .GetInt32("UserId") ?? 0;

        private void SetViewBag()
        {
            ViewBag.UserFullName =
                HttpContext.Session
                    .GetString("UserFullName");
            ViewBag.UserEmail =
                HttpContext.Session
                    .GetString("UserEmail");
            int uid = UserId();
            ViewBag.UnreadCount =
                _db.Notifications
                    .Count(n =>
                        n.UserId == uid &&
                        !n.IsRead);
        }

        // ── GET /Dashboard ──────────────────────
        public async Task<IActionResult> Index(
            string? search)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();

            // Fetch weather
            var weather =
                await _weather.GetWeatherAsync();
            ViewBag.Weather = weather;

            // Load active coupons
            var now = DateTime.Now;
            ViewBag.ActiveCoupons = _db.Coupons
                .Where(c =>
                    c.IsActive &&
                    c.ExpiryDate > now &&
                    c.StartDate <= now &&
                    c.UsedCount < c.UsageLimit)
                .OrderBy(c => c.ExpiryDate)
                .ToList();

            // Load restaurants from DB
            var allRestaurants = _db.Restaurants
                .Where(r => r.IsActive)
                .Select(r => new Restaurant
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    Cuisine = r.Cuisine,
                    Address = r.Address,
                    Rating = (double)r.Rating,
                    DeliveryTime = r.DeliveryTime,
                    DeliveryFee = r.DeliveryFee,
                    IsOpen = r.IsOpen,
                    ImageUrl =
                        string.IsNullOrEmpty(
                            r.ImagePath)
                        ? r.ImageUrl
                        : r.ImagePath,
                    Tag = r.Tag
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                allRestaurants = allRestaurants
                    .Where(r =>
                        r.Name.Contains(search,
                            StringComparison
                                .OrdinalIgnoreCase)
                     || r.Cuisine.Contains(search,
                            StringComparison
                                .OrdinalIgnoreCase)
                     || r.Address.Contains(search,
                            StringComparison
                                .OrdinalIgnoreCase))
                    .ToList();
            }

            int uid = UserId();

            var favIds = _db.Favourites
                .Where(f => f.UserId == uid)
                .Select(f => f.RestaurantId)
                .ToList();

            ViewBag.FavouriteIds = favIds;

            ViewBag.StatTotalOrders = _db.Orders
                .Count(o => o.UserId == uid);

            ViewBag.StatActiveOrders = _db.Orders
                .Count(o =>
                    o.UserId == uid &&
                    o.Status == "Confirmed");

            ViewBag.StatTotalSpent = _db.Orders
                .Where(o => o.UserId == uid)
                .Sum(o => (decimal?)o.GrandTotal)
                ?? 0;

            var vm = new DashboardViewModel
            {
                UserFullName =
                    HttpContext.Session
                        .GetString("UserFullName")
                    ?? "Guest",
                UserEmail =
                    HttpContext.Session
                        .GetString("UserEmail")
                    ?? "",
                Restaurants = allRestaurants,
                FeaturedRestaurants = allRestaurants
                    .Where(r => r.Tag == "Popular")
                    .ToList(),
                Categories = GetCategories(),
                SearchQuery = search
            };

            return View(vm);
        }

        // ── GET /Dashboard/MyOrders ─────────────
        public IActionResult MyOrders()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();

            var rawOrders = _db.Orders
                .Where(o => o.UserId == uid)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.RestaurantId,
                    RestaurantName =
                        o.RestaurantName ?? "",
                    o.TotalAmount,
                    o.DeliveryFee,
                    o.GrandTotal,
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
                    o.OrderDate
                })
                .ToList();

            var orderIds = rawOrders
                .Select(o => o.OrderId)
                .ToList();

            var allItems = _db.OrderItems
                .Where(i =>
                    orderIds.Contains(i.OrderId))
                .ToList();

            var orders = rawOrders.Select(o =>
                new Order
                {
                    OrderId = o.OrderId,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.RestaurantName,
                    TotalAmount = o.TotalAmount,
                    DeliveryFee = o.DeliveryFee,
                    GrandTotal = o.GrandTotal,
                    Status = o.Status,
                    DeliveryAddress = o.DeliveryAddress,
                    OrderNote = o.OrderNote,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    TransactionId = o.TransactionId,
                    OrderDate = o.OrderDate,
                    OrderItems = allItems
                        .Where(i =>
                            i.OrderId == o.OrderId)
                        .ToList()
                })
                .ToList();

            ViewBag.RatedOrderIds = _db.Ratings
                .Where(r => r.UserId == uid)
                .Select(r => r.OrderId)
                .ToList();

            try { _excel.GenerateExcelReport(); }
            catch { /* non-blocking */ }

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(orders);
        }

        // ── POST /Dashboard/SubmitRating ────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitRating(
            int orderId,
            int restaurantId,
            int stars,
            string comment)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();

            bool alreadyRated = _db.Ratings.Any(
                r => r.UserId == uid &&
                     r.OrderId == orderId);

            if (!alreadyRated &&
                stars >= 1 && stars <= 5)
            {
                _db.Ratings.Add(new Rating
                {
                    UserId = uid,
                    OrderId = orderId,
                    RestaurantId = restaurantId,
                    Stars = stars,
                    Comment = comment ?? "",
                    RatedAt = DateTime.Now
                });

                _db.Notifications.Add(
                    new Notification
                    {
                        UserId = uid,
                        Title = "Rating Submitted!",
                        Message =
                            "Thank you for rating " +
                            "order #" + orderId +
                            ". Your feedback " +
                            "helps us improve.",
                        Icon = "⭐",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });

                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Thank you for your rating!";
            }

            return RedirectToAction("MyOrders");
        }

        // ── GET /Dashboard/Favourites ───────────
        public IActionResult Favourites()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();

            var favs = _db.Favourites
                .Where(f => f.UserId == uid)
                .OrderByDescending(f => f.AddedAt)
                .ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View(favs);
        }

        // ── POST /Dashboard/ToggleFavourite ─────
        [HttpPost]
        public IActionResult ToggleFavourite(
            int restaurantId,
            string restaurantName,
            string restaurantCuisine,
            string restaurantIcon)
        {
            if (!IsLoggedIn())
                return Json(new { success = false });

            int uid = UserId();

            var existing = _db.Favourites
                .FirstOrDefault(f =>
                    f.UserId == uid &&
                    f.RestaurantId == restaurantId);

            bool isFav;
            if (existing != null)
            {
                _db.Favourites.Remove(existing);
                isFav = false;
            }
            else
            {
                _db.Favourites.Add(new Favourite
                {
                    UserId = uid,
                    RestaurantId = restaurantId,
                    RestaurantName = restaurantName,
                    RestaurantCuisine =
                        restaurantCuisine,
                    RestaurantIcon = restaurantIcon,
                    AddedAt = DateTime.Now
                });
                isFav = true;
            }

            _db.SaveChanges();
            return Json(new
            {
                success = true,
                isFavourite = isFav
            });
        }

        // ── GET /Dashboard/Notifications ────────
        public IActionResult Notifications()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();

            var unread = _db.Notifications
                .Where(n =>
                    n.UserId == uid && !n.IsRead)
                .ToList();
            unread.ForEach(n => n.IsRead = true);
            _db.SaveChanges();

            var notifications = _db.Notifications
                .Where(n => n.UserId == uid)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];

            return View(notifications);
        }

        // ── POST /Dashboard/ClearNotifications ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearNotifications()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();
            var all = _db.Notifications
                .Where(n => n.UserId == uid)
                .ToList();
            _db.Notifications.RemoveRange(all);
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "All notifications cleared.";
            return RedirectToAction("Notifications");
        }

        // ── GET /Dashboard/Profile ──────────────
        public IActionResult Profile()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();
            var user = _db.Users.FirstOrDefault(
                u => u.UserId == uid);
            if (user == null)
                return RedirectToAction(
                    "Login", "Account");

            var vm = new ProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                TotalOrders = _db.Orders
                    .Count(o => o.UserId == uid),
                TotalSpent = _db.Orders
                    .Where(o => o.UserId == uid)
                    .Sum(o =>
                        (decimal?)o.GrandTotal)
                    ?? 0
            };

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(vm);
        }

        // ── POST /Dashboard/Profile ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(
            ProfileViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            if (!ModelState.IsValid)
            {
                SetViewBag();
                return View(model);
            }

            int uid = UserId();
            var user = _db.Users.FirstOrDefault(
                u => u.UserId == uid);
            if (user == null)
                return RedirectToAction(
                    "Login", "Account");

            bool emailTaken = _db.Users.Any(u =>
                u.Email == model.Email &&
                u.UserId != uid);
            if (emailTaken)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already used " +
                    "by another account.");
                SetViewBag();
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            _db.SaveChanges();

            HttpContext.Session.SetString(
                "UserFullName",
                user.FirstName + " " +
                user.LastName);
            HttpContext.Session.SetString(
                "UserEmail", user.Email);

            try { _excel.GenerateExcelReport(); }
            catch { }

            TempData["SuccessMessage"] =
                "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ── GET /Dashboard/Settings ─────────────
        public IActionResult Settings()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(new ChangePasswordViewModel());
        }

        // ── POST /Dashboard/Settings ────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(
            ChangePasswordViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();

            if (!ModelState.IsValid)
                return View(model);

            int uid = UserId();
            var user = _db.Users.FirstOrDefault(
                u => u.UserId == uid);
            if (user == null)
                return RedirectToAction(
                    "Login", "Account");

            bool correct = BCrypt.Net.BCrypt
                .Verify(
                    model.CurrentPassword,
                    user.PasswordHash);

            if (!correct)
            {
                ModelState.AddModelError(
                    "CurrentPassword",
                    "Current password is incorrect.");
                return View(model);
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    model.NewPassword);
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Password changed successfully!";
            return RedirectToAction("Settings");
        }

        // ── GET /Dashboard/DownloadExcel ────────
        public IActionResult DownloadExcel()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var path =
                _excel.GenerateExcelReport();
            var bytes =
                System.IO.File.ReadAllBytes(path);
            return File(bytes,
                "application/vnd.openxmlformats-" +
                "officedocument" +
                ".spreadsheetml.sheet",
                "HungryHub_Data_" +
                DateTime.Now
                    .ToString("yyyyMMdd") +
                ".xlsx");
        }

        // ── Categories seed ─────────────────────
        private List<FoodCategory> GetCategories()
            => new()
        {
            new FoodCategory
            {
                CategoryId = 1,
                Name  = "Burgers",
                Icon  = "🍔",
                Color = "#FF6B6B"
            },
            new FoodCategory
            {
                CategoryId = 2,
                Name  = "Pizza",
                Icon  = "🍕",
                Color = "#FF8E53"
            },
            new FoodCategory
            {
                CategoryId = 3,
                Name  = "Biryani",
                Icon  = "🍛",
                Color = "#FFA94D"
            },
            new FoodCategory
            {
                CategoryId = 4,
                Name  = "Sushi",
                Icon  = "🍣",
                Color = "#51CF66"
            },
            new FoodCategory
            {
                CategoryId = 5,
                Name  = "Desserts",
                Icon  = "🍰",
                Color = "#CC5DE8"
            },
            new FoodCategory
            {
                CategoryId = 6,
                Name  = "Healthy",
                Icon  = "🥗",
                Color = "#339AF0"
            },
            new FoodCategory
            {
                CategoryId = 7,
                Name  = "Chinese",
                Icon  = "🥡",
                Color = "#F06595"
            },
            new FoodCategory
            {
                CategoryId = 8,
                Name  = "Shawarma",
                Icon  = "🌯",
                Color = "#20C997"
            }
        };
    }
}
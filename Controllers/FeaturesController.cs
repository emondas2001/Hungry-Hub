using HungryHub.Data;
using HungryHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace HungryHub.Controllers
{
    public class FeaturesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FeaturesController(
            ApplicationDbContext db)
        {
            _db = db;
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

        // ════════════════════════════════════
        // SPLIT ORDER
        // ════════════════════════════════════

        // GET: /Features/SplitOrder
        public IActionResult SplitOrder()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();

            // Get user's completed orders
            var orders = _db.Orders
                .Where(o =>
                    o.UserId == uid &&
                    o.Status == "Confirmed")
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.RestaurantName,
                    o.GrandTotal,
                    o.OrderDate
                })
                .ToList();

            // Get existing splits
            var splits = _db.SplitOrders
                .Where(s =>
                    s.CreatorUserId == uid)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            var splitIds = splits
                .Select(s => s.SplitOrderId)
                .ToList();

            var participants = _db.SplitParticipants
                .Where(p =>
                    splitIds.Contains(
                        p.SplitOrderId))
                .ToList();

            foreach (var split in splits)
            {
                split.Participants = participants
                    .Where(p =>
                        p.SplitOrderId ==
                        split.SplitOrderId)
                    .ToList();
            }

            ViewBag.UserOrders = orders;
            ViewBag.SplitOrders = splits;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // POST: Create split
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSplit(
            int orderId,
            string splitType,
            List<string> names,
            List<string> emails,
            List<decimal> amounts)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();

            var order = _db.Orders
                .FirstOrDefault(o =>
                    o.OrderId == orderId &&
                    o.UserId == uid);

            if (order == null)
            {
                TempData["Error"] =
                    "Order not found.";
                return RedirectToAction(
                    "SplitOrder");
            }

            var split = new SplitOrder
            {
                OrderId = orderId,
                CreatorUserId = uid,
                TotalAmount = order.GrandTotal,
                SplitType = splitType,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _db.SplitOrders.Add(split);
            _db.SaveChanges();

            // Add participants
            decimal equalShare = names.Count > 0
                ? order.GrandTotal / names.Count
                : 0;

            for (int i = 0; i < names.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(
                        names[i])) continue;

                decimal amt = splitType == "Equal"
                    ? equalShare
                    : (i < amounts.Count
                        ? amounts[i] : 0);

                _db.SplitParticipants.Add(
                    new SplitParticipant
                    {
                        SplitOrderId = split.SplitOrderId,
                        Name = names[i],
                        Email = i < emails.Count
                            ? emails[i] : null,
                        AmountOwed = amt,
                        IsPaid = false
                    });
            }

            _db.SaveChanges();

            // Notification
            _db.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = "Split Order Created!",
                Message =
                    "Order #" + orderId +
                    " split between " +
                    names.Count + " people.",
                Icon = "💰",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Split order created successfully!";
            return RedirectToAction("SplitOrder");
        }

        // POST: Mark participant paid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkPaid(
            int participantId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var p = _db.SplitParticipants
                .FirstOrDefault(x =>
                    x.ParticipantId ==
                    participantId);
            if (p != null)
            {
                p.IsPaid = true;
                p.PaidAt = DateTime.Now;
                _db.SaveChanges();
            }
            TempData["SuccessMessage"] =
                "Marked as paid!";
            return RedirectToAction("SplitOrder");
        }

        // POST: Delete split
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSplit(
            int splitOrderId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            var split = _db.SplitOrders
                .FirstOrDefault(s =>
                    s.SplitOrderId == splitOrderId);
            if (split != null)
            {
                _db.SplitParticipants.RemoveRange(
                    _db.SplitParticipants.Where(p =>
                        p.SplitOrderId ==
                        splitOrderId));
                _db.SplitOrders.Remove(split);
                _db.SaveChanges();
            }
            TempData["SuccessMessage"] =
                "Split deleted.";
            return RedirectToAction("SplitOrder");
        }

        // ════════════════════════════════════
        // PRE-ORDER / SPECIAL EVENTS
        // ════════════════════════════════════

        // GET: /Features/PreOrder
        public IActionResult PreOrder()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();

            var restaurants = _db.Restaurants
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new
                {
                    r.RestaurantId,
                    r.Name,
                    r.Cuisine
                })
                .ToList();

            var preOrders = _db.PreOrders
                .Where(p => p.UserId == uid)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var preOrderIds = preOrders
                .Select(p => p.PreOrderId)
                .ToList();

            var items = _db.PreOrderItems
                .Where(i =>
                    preOrderIds.Contains(
                        i.PreOrderId))
                .ToList();

            foreach (var po in preOrders)
            {
                po.Items = items
                    .Where(i =>
                        i.PreOrderId == po.PreOrderId)
                    .ToList();
            }

            ViewBag.Restaurants = restaurants;
            ViewBag.PreOrders = preOrders;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // POST: Create pre-order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePreOrder(
            PreOrder model,
            List<string> itemNames,
            List<int> quantities,
            List<decimal> prices)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();

            if (model.EventDate <= DateTime.Now)
            {
                TempData["Error"] =
                    "Event date must be in the future.";
                return RedirectToAction("PreOrder");
            }

            var restaurant = _db.Restaurants
                .FirstOrDefault(r =>
                    r.RestaurantId ==
                    model.RestaurantId);

            decimal total = 0;
            var itemList = new List<PreOrderItem>();

            for (int i = 0;
                 i < itemNames.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(
                        itemNames[i])) continue;

                int qty = i < quantities.Count
                    ? quantities[i] : 1;
                decimal price = i < prices.Count
                    ? prices[i] : 0;
                decimal sub = qty * price;
                total += sub;

                itemList.Add(new PreOrderItem
                {
                    ItemName = itemNames[i],
                    Quantity = qty,
                    UnitPrice = price,
                    SubTotal = sub
                });
            }

            decimal advance = total * 0.3m;

            var preOrder = new PreOrder
            {
                UserId = uid,
                RestaurantId = model.RestaurantId,
                RestaurantName =
                    restaurant?.Name ?? "Unknown",
                EventName = model.EventName,
                EventDate = model.EventDate,
                EventAddress = model.EventAddress,
                GuestCount = model.GuestCount,
                SpecialRequests =
                    model.SpecialRequests,
                TotalAmount = total,
                AdvanceAmount = advance,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                Items = itemList
            };

            _db.PreOrders.Add(preOrder);
            _db.SaveChanges();

            _db.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = "Pre-Order Submitted!",
                Message =
                    "Your event pre-order for " +
                    model.EventName +
                    " on " +
                    model.EventDate
                        .ToString("dd MMM yyyy") +
                    " has been submitted. " +
                    "Awaiting admin confirmation.",
                Icon = "🎉",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Pre-order submitted! " +
                "Admin will confirm soon.";
            return RedirectToAction("PreOrder");
        }

        // POST: Cancel pre-order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelPreOrder(
            int preOrderId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();
            var po = _db.PreOrders.FirstOrDefault(
                p => p.PreOrderId == preOrderId &&
                     p.UserId == uid);

            if (po != null &&
                po.Status != "Confirmed")
            {
                po.Status = "Cancelled";
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Pre-order cancelled.";
            }
            else
            {
                TempData["Error"] =
                    "Cannot cancel confirmed " +
                    "pre-orders.";
            }
            return RedirectToAction("PreOrder");
        }

        // ════════════════════════════════════
        // SUBSCRIPTION MEAL PLANS
        // ════════════════════════════════════

        // GET: /Features/Subscription
        public IActionResult Subscription()
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            SetViewBag();
            int uid = UserId();

            var plans = _db.MealPlans
                .Where(p => p.IsActive)
                .ToList();

            var restaurants = _db.Restaurants
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new
                {
                    r.RestaurantId,
                    r.Name,
                    r.Cuisine
                })
                .ToList();

            // User's active subscriptions
            var subs = _db.UserSubscriptions
                .Where(s => s.UserId == uid)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            var planIds = subs
                .Select(s => s.PlanId)
                .Distinct().ToList();
            var planData = _db.MealPlans
                .Where(p =>
                    planIds.Contains(p.PlanId))
                .ToList();

            foreach (var sub in subs)
            {
                sub.Plan = planData
                    .FirstOrDefault(p =>
                        p.PlanId == sub.PlanId);
            }

            ViewBag.Plans = plans;
            ViewBag.Restaurants = restaurants;
            ViewBag.Subs = subs;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage =
                    TempData["SuccessMessage"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // POST: Subscribe to meal plan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Subscribe(
            int planId,
            int restaurantId,
            string deliveryTime,
            string deliveryAddr,
            DateTime startDate)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();

            // Check no active subscription
            bool hasActive = _db.UserSubscriptions
                .Any(s =>
                    s.UserId == uid &&
                    s.Status == "Active");

            if (hasActive)
            {
                TempData["Error"] =
                    "You already have an active " +
                    "subscription. Cancel it first.";
                return RedirectToAction(
                    "Subscription");
            }

            var plan = _db.MealPlans
                .FirstOrDefault(p =>
                    p.PlanId == planId);
            var restaurant = _db.Restaurants
                .FirstOrDefault(r =>
                    r.RestaurantId == restaurantId);

            if (plan == null)
            {
                TempData["Error"] =
                    "Invalid plan selected.";
                return RedirectToAction(
                    "Subscription");
            }

            // 4 week subscription
            var endDate = startDate.AddDays(28);

            // Total = 4 weeks
            decimal total =
                plan.PricePerWeek * 4;

            var sub = new UserSubscription
            {
                UserId = uid,
                PlanId = planId,
                RestaurantId = restaurantId,
                RestaurantName =
                    restaurant?.Name ?? "Unknown",
                StartDate = startDate,
                EndDate = endDate,
                DeliveryTime = deliveryTime,
                DeliveryAddr = deliveryAddr,
                Status = "Active",
                TotalPaid = total,
                CreatedAt = DateTime.Now
            };

            _db.UserSubscriptions.Add(sub);

            _db.Notifications.Add(new Notification
            {
                UserId = uid,
                Title = "Subscription Activated!",
                Message =
                    plan.PlanName +
                    " activated from " +
                    startDate.ToString("dd MMM") +
                    " to " +
                    endDate.ToString("dd MMM yyyy") +
                    ". Total: ৳" +
                    total.ToString("N0"),
                Icon = "🍱",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            _db.SaveChanges();

            TempData["SuccessMessage"] =
                "Subscription activated!";
            return RedirectToAction("Subscription");
        }

        // POST: Cancel subscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelSubscription(
            int subscriptionId)
        {
            if (!IsLoggedIn())
                return RedirectToAction(
                    "Login", "Account");

            int uid = UserId();
            var sub = _db.UserSubscriptions
                .FirstOrDefault(s =>
                    s.SubscriptionId ==
                    subscriptionId &&
                    s.UserId == uid);

            if (sub != null)
            {
                sub.Status = "Cancelled";
                _db.SaveChanges();
                TempData["SuccessMessage"] =
                    "Subscription cancelled.";
            }
            return RedirectToAction("Subscription");
        }
    }
}
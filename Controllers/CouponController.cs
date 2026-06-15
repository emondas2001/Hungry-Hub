using HungryHub.Data;
using HungryHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace HungryHub.Controllers
{
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(
            ApplicationDbContext db)
        {
            _db = db;
        }

        private int UserId() =>
            HttpContext.Session
                .GetInt32("UserId") ?? 0;

        private bool IsLoggedIn() =>
            HttpContext.Session
                .GetString("UserEmail") != null;

        // ── AJAX: Validate coupon ────────────────
        [HttpPost]
        public IActionResult ValidateCoupon(
            string code, decimal orderAmount)
        {
            if (!IsLoggedIn())
                return Json(new
                {
                    success = false,
                    message = "Not logged in."
                });

            int uid = UserId();

            var coupon = _db.Coupons
                .FirstOrDefault(c =>
                    c.Code == code.ToUpper().Trim() &&
                    c.IsActive);

            if (coupon == null)
                return Json(new
                {
                    success = false,
                    message = "Invalid coupon code."
                });

            if (DateTime.Now < coupon.StartDate)
                return Json(new
                {
                    success = false,
                    message = "Coupon not yet active."
                });

            if (DateTime.Now > coupon.ExpiryDate)
                return Json(new
                {
                    success = false,
                    message = "Coupon has expired."
                });

            if (coupon.UsedCount >= coupon.UsageLimit)
                return Json(new
                {
                    success = false,
                    message = "Coupon usage limit reached."
                });

            if (orderAmount < coupon.MinOrderAmount)
                return Json(new
                {
                    success = false,
                    message =
                        "Minimum order amount is ৳" +
                        coupon.MinOrderAmount
                            .ToString("N0") + "."
                });

            // Check if user already used it
            bool alreadyUsed = _db.CouponUsages
                .Any(u =>
                    u.CouponId == coupon.CouponId &&
                    u.UserId == uid);

            if (alreadyUsed)
                return Json(new
                {
                    success = false,
                    message =
                        "You have already used " +
                        "this coupon."
                });

            // Calculate discount
            decimal discount = 0;
            if (coupon.DiscountType == "Percent")
            {
                discount = orderAmount *
                    coupon.DiscountValue / 100;
                if (coupon.MaxDiscount > 0 &&
                    discount > coupon.MaxDiscount)
                    discount = coupon.MaxDiscount;
            }
            else
            {
                discount = coupon.DiscountValue;
            }

            discount = Math.Min(discount, orderAmount);

            return Json(new
            {
                success = true,
                message =
                    "Coupon applied! You save ৳" +
                    discount.ToString("N0"),
                discount = discount,
                couponId = coupon.CouponId,
                code = coupon.Code,
                title = coupon.Title,
                discountType = coupon.DiscountType,
                discountValue = coupon.DiscountValue
            });
        }
    }
}
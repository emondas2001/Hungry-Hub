using HungryHub.Models;
using Microsoft.EntityFrameworkCore;

namespace HungryHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext>
            options) : base(options) { }

        public DbSet<User> Users
        { get; set; }
        public DbSet<Admin> Admins
        { get; set; }
        public DbSet<RestaurantDb> Restaurants
        { get; set; }
        public DbSet<RestaurantHours> RestaurantHours
        { get; set; }
        public DbSet<MenuItem> MenuItems
        { get; set; }
        public DbSet<Order> Orders
        { get; set; }
        public DbSet<OrderItem> OrderItems
        { get; set; }
        public DbSet<Payment> Payments
        { get; set; }
        public DbSet<Favourite> Favourites
        { get; set; }
        public DbSet<Rating> Ratings
        { get; set; }
        public DbSet<Notification> Notifications
        { get; set; }
        public DbSet<SplitOrder> SplitOrders
        { get; set; }
        public DbSet<SplitParticipant> SplitParticipants
        { get; set; }
        public DbSet<PreOrder> PreOrders
        { get; set; }
        public DbSet<PreOrderItem> PreOrderItems
        { get; set; }
        public DbSet<MealPlan> MealPlans
        { get; set; }
        public DbSet<UserSubscription> UserSubscriptions
        { get; set; }
        public DbSet<Coupon> Coupons
        { get; set; }
        public DbSet<CouponUsage> CouponUsages
        { get; set; }
        public DbSet<RestaurantOwner> RestaurantOwners
        { get; set; }
        public DbSet<RestaurantActivityLog>
            RestaurantActivityLogs
        { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RestaurantDb>()
                .HasKey(r => r.RestaurantId);
            modelBuilder.Entity<RestaurantHours>()
                .HasKey(h => h.HoursId);
            modelBuilder.Entity<RestaurantHours>()
                .Ignore(h => h.DayName);

            modelBuilder.Entity<RestaurantDb>()
                .HasMany(r => r.Hours)
                .WithOne()
                .HasForeignKey(h => h.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RestaurantDb>()
                .HasMany(r => r.MenuItems)
                .WithOne()
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SplitOrder>()
                .HasKey(s => s.SplitOrderId);
            modelBuilder.Entity<SplitParticipant>()
                .HasKey(p => p.ParticipantId);
            modelBuilder.Entity<PreOrder>()
                .HasKey(p => p.PreOrderId);
            modelBuilder.Entity<PreOrderItem>()
                .HasKey(p => p.PreOrderItemId);
            modelBuilder.Entity<MealPlan>()
                .HasKey(m => m.PlanId);
            modelBuilder.Entity<UserSubscription>()
                .HasKey(u => u.SubscriptionId);
            modelBuilder.Entity<Coupon>()
                .HasKey(c => c.CouponId);
            modelBuilder.Entity<CouponUsage>()
                .HasKey(c => c.UsageId);
            modelBuilder.Entity<RestaurantOwner>()
                .HasKey(o => o.OwnerId);
            modelBuilder.Entity<RestaurantActivityLog>()
                .HasKey(l => l.LogId);
        }
    }
}
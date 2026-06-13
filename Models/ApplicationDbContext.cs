using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Webbanhang.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.Discount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.Total)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Province>()
                .HasIndex(p => p.Code)
                .IsUnique();

            builder.Entity<District>()
                .HasIndex(d => d.Code)
                .IsUnique();

            builder.Entity<Ward>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<District>()
                .HasOne(d => d.Province)
                .WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Ward>()
                .HasOne(w => w.District)
                .WithMany(d => d.Wards)
                .HasForeignKey(w => w.DistrictId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
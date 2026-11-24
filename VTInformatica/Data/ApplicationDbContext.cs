using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VTInformatica.Models.Auth;
using VTInformatica.Models;
using VTInformatica.DTOs.Product;

namespace VTInformatica.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //Identity DbSet
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }

        //All others DbSets

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUserRole>().Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()").IsRequired(true);
            modelBuilder.Entity<ApplicationUserRole>().HasOne(p => p.User).WithMany(p => p.ApplicationUserRoles).HasForeignKey(p => p.UserId).IsRequired(true);
            modelBuilder.Entity<ApplicationUserRole>().HasOne(p => p.Role).WithMany(p => p.ApplicationUserRoles).HasForeignKey(p => p.RoleId).IsRequired(true);

            // GUID fissi, uguali a quelli già nei DB esistenti (migration 20250508102202_Update,
            // DatabaseScript.sql). Con Guid.NewGuid() ogni migration cancellava e reinseriva i ruoli.
            const string adminId = "98b4e551-a53d-40fc-9fa4-29d6dd582481";
            const string sellerId = "9ab947f2-b737-46bf-a388-2f8962573a2b";
            const string customerId = "bd7ef491-0f1b-4b8c-9c80-340e14cfac9b";

            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = adminId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = adminId
                }, 
                new ApplicationRole
                {
                    Id = sellerId,
                    Name = "Seller",
                    NormalizedName = "SELLER",
                    ConcurrencyStamp = sellerId
                },
                new ApplicationRole
                {
                    Id = customerId,
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    ConcurrencyStamp = customerId
                }
            );

            modelBuilder.Entity<Cart>().HasOne(c => c.User).WithOne(u => u.Cart).HasForeignKey<Cart>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>().HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId);
            modelBuilder.Entity<CartItem>().HasOne(ci => ci.Product).WithMany(p => p.CartItems).HasForeignKey(ci => ci.ProductId);

            // Numero ordine unico (M3c1): una collisione diventa un errore invece di un duplicato silenzioso.
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

            // Lo storico ordini non deve mai essere cancellato insieme al prodotto (prima: cascade per convenzione).
            modelBuilder.Entity<OrderItem>().HasOne(oi => oi.Product).WithMany(p => p.Orders).HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);

            // Prodotti eliminati (soft delete) esclusi da tutte le query. Le query sugli ordini
            // usano IgnoreQueryFilters: OrderItem→Product è required e il filtro nasconderebbe le righe.
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>().HasOne(p => p.SubCategory).WithMany(s => s.Products).HasForeignKey(p => p.SubCategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SubCategory>().HasOne(s => s.Category).WithMany(c => c.SubCategories).HasForeignKey(s => s.CategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductImage>().HasOne(pi => pi.Product).WithMany(p => p.ProductImages).HasForeignKey(pi => pi.ProductId);
            modelBuilder.Entity<Manufacturer>().HasMany(m => m.Products).WithOne(p => p.Manufacturer).HasForeignKey(p => p.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>().HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Review>().HasOne(r => r.User).WithMany(p => p.Reviews).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);

            // Categorie
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Componenti PC" },
                new Category { Id = 2, Name = "Periferiche" },
                new Category { Id = 3, Name = "Gaming" },
                new Category { Id = 4, Name = "Accessori e Cavi" },
                new Category { Id = 5, Name = "Networking" },
                new Category { Id = 6, Name = "PC Assemblati" }
            );

            // Sottocategorie
            modelBuilder.Entity<SubCategory>().HasData(
                // Componenti PC
                new SubCategory { Id = 1, Name = "CPU", CategoryId = 1 },
                new SubCategory { Id = 2, Name = "Schede madri", CategoryId = 1 },
                new SubCategory { Id = 3, Name = "RAM", CategoryId = 1 },
                new SubCategory { Id = 4, Name = "Schede video", CategoryId = 1 },
                new SubCategory { Id = 5, Name = "SSD / Hard Disk", CategoryId = 1 },
                new SubCategory { Id = 6, Name = "Alimentatori", CategoryId = 1 },
                new SubCategory { Id = 7, Name = "Case", CategoryId = 1 },
                new SubCategory { Id = 8, Name = "Dissipatori", CategoryId = 1 },

                // Periferiche
                new SubCategory { Id = 9, Name = "Monitor", CategoryId = 2 },
                new SubCategory { Id = 10, Name = "Tastiere", CategoryId = 2 },
                new SubCategory { Id = 11, Name = "Mouse", CategoryId = 2 },
                new SubCategory { Id = 12, Name = "Cuffie", CategoryId = 2 },
                new SubCategory { Id = 13, Name = "Microfoni", CategoryId = 2 },
                new SubCategory { Id = 14, Name = "Webcam", CategoryId = 2 },

                // Gaming
                new SubCategory { Id = 15, Name = "Controller", CategoryId = 3 },
                new SubCategory { Id = 16, Name = "Volanti", CategoryId = 3 },
                 new SubCategory { Id = 17, Name = "Sedie da gaming", CategoryId = 3 },
                new SubCategory { Id = 18, Name = "Accessori RGB", CategoryId = 3 },

                // Accessori e Cavi
                new SubCategory { Id = 19, Name = "Cavi di alimentazione", CategoryId = 4 },
                new SubCategory { Id = 20, Name = "Adattatori e hub", CategoryId = 4 },
                new SubCategory { Id = 21, Name = "Ventole", CategoryId = 4 },
                new SubCategory { Id = 22, Name = "Paste termiche", CategoryId = 4 },

                // Networking
                new SubCategory { Id = 23, Name = "Router", CategoryId = 5 },
                new SubCategory { Id = 24, Name = "Schede di rete", CategoryId = 5 },
                new SubCategory { Id = 25, Name = "Range extender", CategoryId = 5 },

                // PC Assemblati
                new SubCategory { Id = 26, Name = "Gaming entry-level", CategoryId = 6 },
                new SubCategory { Id = 27, Name = "Gaming mid-range", CategoryId = 6 },
                new SubCategory { Id = 28, Name = "Gaming high-end", CategoryId = 6 },
                new SubCategory { Id = 29, Name = "Workstation", CategoryId = 6 },
                new SubCategory { Id = 30, Name = "Mini PC", CategoryId = 6 }
            );

            modelBuilder.Entity<Manufacturer>().HasData(
    new Manufacturer { ManufacturerId = 1, ManufacturerName = "Intel", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/intel.svg" },
    new Manufacturer { ManufacturerId = 2, ManufacturerName = "AMD", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/amd-logo-1.svg" },
    new Manufacturer { ManufacturerId = 3, ManufacturerName = "NVIDIA", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/nvidia.svg" },
    new Manufacturer { ManufacturerId = 4, ManufacturerName = "ASUS", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/asus-logo.svg" },
    new Manufacturer { ManufacturerId = 5, ManufacturerName = "Corsair", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/corsair-2.svg" },
    new Manufacturer { ManufacturerId = 6, ManufacturerName = "MSI", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/micro-star-international-logo.svg" },
    new Manufacturer { ManufacturerId = 7, ManufacturerName = "Gigabyte", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/gigabyte-technology-logo-2008.svg" },
    new Manufacturer { ManufacturerId = 8, ManufacturerName = "ThermalRight", ManufacturerLogo = "https://upload.wikimedia.org/wikipedia/commons/3/34/Thermalright_Logo.svg" },
    new Manufacturer { ManufacturerId = 9, ManufacturerName = "Be quiet!", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/be-quiet-logo.svg" },
    new Manufacturer { ManufacturerId = 10, ManufacturerName = "LG", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/lg-electronics.svg" },
    new Manufacturer { ManufacturerId = 11, ManufacturerName = "Noctua", ManufacturerLogo = "https://noctua.at/pub/static/version1743603791/frontend/SIT/noctua/en_US/images/logo.svg" },
    new Manufacturer { ManufacturerId = 12, ManufacturerName = "Samsung", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/samsung-8.svg" },
    new Manufacturer { ManufacturerId = 13, ManufacturerName = "Seagate", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/seagate-logo-1.svg" },
    new Manufacturer { ManufacturerId = 14, ManufacturerName = "Western Digital", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/western-digital-2.svg" },
    new Manufacturer { ManufacturerId = 15, ManufacturerName = "Kingston", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/kingston-1.svg" },
    new Manufacturer { ManufacturerId = 16, ManufacturerName = "Crucial", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/crucial.svg" },
    new Manufacturer { ManufacturerId = 17, ManufacturerName = "EVGA", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/evga.svg" },
    new Manufacturer { ManufacturerId = 18, ManufacturerName = "Cooler Master", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/cooler-master-black-logo.svg" },
    new Manufacturer { ManufacturerId = 19, ManufacturerName = "Thermaltake", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/thermaltake.svg" },
    new Manufacturer { ManufacturerId = 20, ManufacturerName = "Razer", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/razer-1.svg" },
    new Manufacturer { ManufacturerId = 21, ManufacturerName = "NZXT", ManufacturerLogo = "https://cdn.worldvectorlogo.com/logos/nzxt-1.svg" }
);
        }
    }
}

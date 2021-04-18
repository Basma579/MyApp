
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        

        }



        public DbSet<Items> Items { get; set; }
        public DbSet<Order_Header> order_Header { get; set; }
        public DbSet<Order_Details> order_Details { get; set; }
        public DbSet<UOM> uOM { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UOM>().HasData(new UOM
            {
                UomID = 1,
                Uom = "KG",
                Description = "Killogram"

            });

            modelBuilder.Entity<Items>().HasData(new Items
            {
                ItemD=1,
                Name = "Blouse",
                Price = 200 ,
                Discount =10 ,
                Quantity =2000 ,
                Uom="KG" 
              

            });

            modelBuilder.Entity<Items>().HasData(new Items
            {
                ItemD=2,
                Name = "Jeans",
                Price = 150,
                Discount = 20,
                Quantity = 5000,
                Uom = "KG"
            });


            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() });

            var hasher = new PasswordHasher<IdentityUser>();

            modelBuilder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", // primary key
                UserName = "Admin",
                    NormalizedUserName = "ADMIN",
                    PasswordHash = hasher.HashPassword(null, "Pa$$w0rd")
                }
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
                }
            );



            modelBuilder.Entity<Order_Header>()
            .HasMany(c => c.Order_Details)
            .WithOne(e => e.Order_Header);



        }


    }
}

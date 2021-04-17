
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

            modelBuilder.Entity<Order_Header>()
        .HasMany(c => c.Order_Details)
        .WithOne(e => e.Order_Header);



        }


    }
}

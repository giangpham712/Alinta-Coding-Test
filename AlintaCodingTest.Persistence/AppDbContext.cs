using System;
using AlintaCodingTest.Domain;
using AlintaCodingTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlintaCodingTest.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
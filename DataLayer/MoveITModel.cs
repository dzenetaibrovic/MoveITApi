using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class MoveITModel: IdentityDbContext
    {
        public MoveITModel()
        {

        }
        public MoveITModel(DbContextOptions<MoveITModel> options)
            : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("dbo");
            builder.Entity<Customer>(entity => 
            {
                entity.ToTable("Customer");

                entity.Property(e => e.CustomerId)
                      .ValueGeneratedNever()                      
                      .HasMaxLength(100);

                entity.Property(e => e.FirstName)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.LastName)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Phone)                     
                      .HasMaxLength(40);

            });
            builder.Entity<Order>(entity => 
            {
                entity.ToTable("Order");                

                entity.Property(e => e.From)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.To)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.CustomerId)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasOne(d => d.Customer)
                      .WithMany(p => p.Orders)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

            });
            base.OnModelCreating(builder);
        }
    }
}

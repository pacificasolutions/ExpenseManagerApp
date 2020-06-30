using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ExpenseManager.Areas.Identity.Data;


namespace ExpenseManager.Models
{
    public class ExpenseDBContext : IdentityDbContext<ExpenseManagerUser>
    {
        public ExpenseDBContext(DbContextOptions<ExpenseDBContext> options) 
            : base(options)
        {

        }

        public virtual DbSet<ExpenseReport> ExpenseReport { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<ExpenseReport>().ToTable("ExpenseReport");
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=SEKHON\SQLEXPRESS;Database=ExpenseManagerDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
            }
        }
    }
}
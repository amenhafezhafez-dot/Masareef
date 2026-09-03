using System;
using System.Collections.Generic;
using Masareef.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Masareef.DAL.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Business> Businesses { get; set; }

    public virtual DbSet<BusinessExpense> BusinessExpenses { get; set; }

    public virtual DbSet<BusinessIncome> BusinessIncomes { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Debt> Debts { get; set; }

    public virtual DbSet<DebtPayment> DebtPayments { get; set; }

    public virtual DbSet<HomeExpense> HomeExpenses { get; set; }

    public virtual DbSet<HomeIncome> HomeIncomes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductSale> ProductSales { get; set; }

    public virtual DbSet<VwDebtSummary> VwDebtSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Currency)
                .HasDefaultValue("SYP")
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasKey(entity => entity.BusinessId).HasName("PK_Businesses");

            entity.HasOne(d => d.User).WithMany(p => p.Businesses).HasConstraintName("FK_Businesses_Users");
        });

        modelBuilder.Entity<BusinessExpense>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PaymentMethod).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Business).WithMany(p => p.BusinessExpenses).HasConstraintName("FK_BusinessExpenses_Businesses");

            entity.HasOne(d => d.Category).WithMany(p => p.BusinessExpenses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessExpenses_Categories");
        });

        modelBuilder.Entity<BusinessIncome>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Business).WithMany(p => p.BusinessIncomes).HasConstraintName("FK_BusinessIncomes_Businesses");

            entity.HasOne(d => d.Category).WithMany(p => p.BusinessIncomes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessIncomes_Categories");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.Categories)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Categories_Users");
        });

        modelBuilder.Entity<Debt>(entity =>
        {
            entity.HasIndex(e => e.DueDate, "IX_Debts_DueDate").HasFilter("([Status] IN ((1), (2)))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Business).WithMany(p => p.Debts).HasConstraintName("FK_Debts_Businesses");

            entity.HasOne(d => d.User).WithMany(p => p.Debts).HasConstraintName("FK_Debts_Users");
        });

        modelBuilder.Entity<DebtPayment>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PaymentMethod).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Debt).WithMany(p => p.DebtPayments).HasConstraintName("FK_DebtPayments_Debts");
        });

        modelBuilder.Entity<HomeExpense>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PaymentMethod).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Category).WithMany(p => p.HomeExpenses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HomeExpenses_Categories");

            entity.HasOne(d => d.User).WithMany(p => p.HomeExpenses).HasConstraintName("FK_HomeExpenses_Users");
        });

        modelBuilder.Entity<HomeIncome>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Category).WithMany(p => p.HomeIncomes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HomeIncomes_Categories");

            entity.HasOne(d => d.User).WithMany(p => p.HomeIncomes).HasConstraintName("FK_HomeIncomes_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<VwDebtSummary>(entity =>
        {
            entity.ToView("vw_DebtSummary");
        });


        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StockQuantity).HasDefaultValue(1);

            entity.HasOne(d => d.Business).WithMany(p => p.Products)
                .HasConstraintName("FK_Products_Businesses");
        });

        modelBuilder.Entity<ProductSale>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSales)
                .HasConstraintName("FK_ProductSales_Products");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

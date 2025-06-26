using System.Collections.Generic;
using MSPR_bloc_4_products.Models;
using Microsoft.EntityFrameworkCore;

namespace MSPR_bloc_4_products.Data;

public partial class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduit).HasName("PK__Product__774C53F860EC2AE9");

            entity.ToTable("Product");

            entity.Property(e => e.IdProduit)
                .ValueGeneratedOnAdd()
                .HasColumnName("Id_produit");
            entity.Property(e => e.Couleur)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("couleur");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Nom)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nom");
            entity.Property(e => e.Prix)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("prix");
            entity.Property(e => e.Stock)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("stock");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // Aquí definirías tus DbSets (tus tablas)
    // public DbSet<Producto> Productos { get; set; }
    // se guarda datros sin hashear 
    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Compra> Compra { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompra { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVenta { get; set; }

    public virtual DbSet<Producto> Producto { get; set; }

    public virtual DbSet<Proveedor> Proveedor { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    public virtual DbSet<Venta> Venta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue(true, "DF_Categoria_Estado");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.Property(e => e.FechaCompra).HasDefaultValueSql("(sysdatetime())", "DF_Compra_Fecha");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Proveedor");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Usuario");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.Property(e => e.Subtotal).HasComputedColumnSql("(CONVERT([decimal](10,2),[Cantidad]*[PrecioUnitario]))", true);

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Compra");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Producto");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.Property(e => e.Subtotal).HasComputedColumnSql("(CONVERT([decimal](10,2),[Cantidad]*[PrecioUnitario]))", true);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Producto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue(true, "DF_Producto_Estado");
            entity.Property(e => e.StockMinimo).HasDefaultValue(5, "DF_Producto_StockMinimo");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Producto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Categoria");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue(true, "DF_Proveedor_Estado");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue(true, "DF_Usuario_Estado");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(e => e.Cambio).HasComputedColumnSql("(CONVERT([decimal](10,2),[MontoPagado]-[Total]))", true);
            entity.Property(e => e.FechaVenta).HasDefaultValueSql("(sysdatetime())", "DF_Venta_Fecha");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

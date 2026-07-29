using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaFarmaciaWeb.Models;

[Index("IdCategoria", Name = "IX_Producto_IdCategoria")]
[Index("Codigo", Name = "UQ_Producto_Codigo", IsUnique = true)]
public partial class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [StringLength(50)]
    public string Codigo { get; set; } = null!;

    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal PrecioCompra { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal PrecioVenta { get; set; }

    public int Stock { get; set; }

    public int StockMinimo { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public int IdCategoria { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<DetalleCompra> DetalleCompra { get; set; } = new List<DetalleCompra>();

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    [ForeignKey("IdCategoria")]
    [InverseProperty("Producto")]
    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;
}

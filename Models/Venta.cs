using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaFarmaciaWeb.Models;

[Index("FechaVenta", Name = "IX_Venta_FechaVenta")]
public partial class Venta
{
    [Key]
    public int IdVenta { get; set; }

    [Precision(0)]
    public DateTime FechaVenta { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal MontoPagado { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Cambio { get; set; }

    public int IdUsuario { get; set; }

    [InverseProperty("IdVentaNavigation")]
    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("Venta")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

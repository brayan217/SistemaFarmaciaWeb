using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaFarmaciaWeb.Models;

[Index("IdProveedor", Name = "IX_Compra_IdProveedor")]
public partial class Compra
{
    [Key]
    public int IdCompra { get; set; }

    [Precision(0)]
    public DateTime FechaCompra { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    public int IdProveedor { get; set; }

    public int IdUsuario { get; set; }

    [InverseProperty("IdCompraNavigation")]
    public virtual ICollection<DetalleCompra> DetalleCompra { get; set; } = new List<DetalleCompra>();

    [ForeignKey("IdProveedor")]
    [InverseProperty("Compra")]
    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Compra")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

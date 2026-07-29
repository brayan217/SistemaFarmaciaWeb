using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaFarmaciaWeb.Models;

public partial class Proveedor
{
    [Key]
    public int IdProveedor { get; set; }

    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [StringLength(150)]
    public string? Correo { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdProveedorNavigation")]
    public virtual ICollection<Compra> Compra { get; set; } = new List<Compra>();
}

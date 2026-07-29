using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaFarmaciaWeb.Models;

[Index("Correo", Name = "UQ_Usuario_Correo", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [StringLength(255)]
    public string Contrasena { get; set; } = null!;

    [StringLength(20)]
    public string Rol { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Compra> Compra { get; set; } = new List<Compra>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}

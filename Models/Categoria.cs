using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemaFarmaciaWeb.Models;

[Index("Nombre", Name = "UQ_Categoria_Nombre", IsUnique = true)]
public partial class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdCategoriaNavigation")]
    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}

using System.ComponentModel.DataAnnotations;

namespace SistemaFarmaciaWeb.ViewModels
{
    public class DetalleCompraViewModel
    {
        [Required(ErrorMessage = "Debes seleccionar un producto.")]
        [Display(Name = "Producto")]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(typeof(decimal), "0.01", "99999999.99",
            ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio unitario")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal
        {
            get
            {
                return Cantidad * PrecioUnitario;
            }
        }
    }
}
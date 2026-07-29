using System.ComponentModel.DataAnnotations;

namespace SistemaFarmaciaWeb.ViewModels
{
    public class DetalleVentaViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        [Range(0.01, 99999999, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal
        {
            get
            {
                return Cantidad * PrecioUnitario;
            }
        }

        // Se utilizarán para mostrar información en el formulario.
        public string? NombreProducto { get; set; }

        public int StockDisponible { get; set; }
    }
}
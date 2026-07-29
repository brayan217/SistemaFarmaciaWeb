using System.ComponentModel.DataAnnotations;

namespace SistemaFarmaciaWeb.ViewModels
{
    public class CompraCreateViewModel
    {
        [Required(ErrorMessage = "Debes seleccionar un proveedor.")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Display(Name = "Fecha de compra")]
        public DateTime FechaCompra { get; set; } = DateTime.Now;

        public List<DetalleCompraViewModel> Detalles { get; set; }
            = new List<DetalleCompraViewModel>();

        public decimal Total
        {
            get
            {
                return Detalles.Sum(d => d.Subtotal);
            }
        }
    }
}
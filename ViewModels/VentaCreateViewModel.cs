using System.ComponentModel.DataAnnotations;

namespace SistemaFarmaciaWeb.ViewModels
{
    public class VentaCreateViewModel
    {
        public VentaCreateViewModel()
        {
            Detalles = new List<DetalleVentaViewModel>();
        }

        [Required(ErrorMessage = "El monto pagado es obligatorio.")]
        [Range(0.01, 99999999, ErrorMessage = "El monto pagado debe ser mayor que cero.")]
        [Display(Name = "Monto pagado")]
        public decimal MontoPagado { get; set; }

        public List<DetalleVentaViewModel> Detalles { get; set; }

        public decimal Total
        {
            get
            {
                return Detalles?.Sum(d => d.Subtotal) ?? 0;
            }
        }

        public decimal Cambio
        {
            get
            {
                return MontoPagado >= Total
                    ? MontoPagado - Total
                    : 0;
            }
        }
    }
}
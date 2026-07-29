using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.ViewModels
{
    public class ReporteVentasViewModel
    {
        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public int CantidadVentas { get; set; }

        public decimal TotalVendido { get; set; }

        public List<Venta> Ventas { get; set; } = new();
    }
}
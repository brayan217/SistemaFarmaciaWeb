using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalVentas { get; set; }

        public int CantidadVentas { get; set; }

        public int ProductosActivos { get; set; }

        public int ProductosBajoStock { get; set; }

        public List<Producto> ProductosStockMinimo { get; set; } = new();

        public List<Venta> UltimasVentas { get; set; } = new();
        public int ProductosVencidos { get; set; }

        public int ProductosProximosAVencer { get; set; }

        public List<Producto> ListaProductosVencidos { get; set; } = new();

        public List<Producto> ListaProductosProximosAVencer { get; set; } = new();
    }
}
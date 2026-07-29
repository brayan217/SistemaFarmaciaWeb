using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.ViewModels;

namespace SistemaFarmaciaWeb.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            DateTime inicio =
                fechaInicio?.Date
                ?? DateTime.Today.AddDays(-30);

            DateTime fin =
                fechaFin?.Date
                ?? DateTime.Today;

            DateTime finConsulta =
                fin.AddDays(1);

            var consulta = _context.Venta
                .Include(venta => venta.IdUsuarioNavigation)
                .Where(venta =>
                    venta.FechaVenta >= inicio &&
                    venta.FechaVenta < finConsulta);

            var ventas = await consulta
                .OrderByDescending(venta => venta.FechaVenta)
                .ToListAsync();

            var modelo = new ReporteVentasViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin,
                CantidadVentas = ventas.Count,
                TotalVendido = ventas.Sum(venta => venta.Total),
                Ventas = ventas
            };

            return View(modelo);
        }
    }
}
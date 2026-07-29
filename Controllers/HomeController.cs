using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Models;
using SistemaFarmaciaWeb.ViewModels;

namespace SistemaFarmaciaWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fecha actual sin hora.
            DateOnly fechaActual = DateOnly.FromDateTime(DateTime.Today);

            // Fecha límite para considerar un producto próximo a vencer.
            DateOnly fechaLimiteVencimiento = fechaActual.AddDays(30);

            // Suma del total de todas las ventas registradas.
            decimal totalVentas = await _context.Venta
                .Select(venta => (decimal?)venta.Total)
                .SumAsync() ?? 0;

            // Cantidad total de ventas registradas.
            int cantidadVentas = await _context.Venta.CountAsync();

            // Cantidad de productos que se encuentran activos.
            int productosActivos = await _context.Producto
                .CountAsync(producto => producto.Estado);

            // Productos activos cuyo stock es menor o igual al stock mínimo.
            int productosBajoStock = await _context.Producto
                .CountAsync(producto =>
                    producto.Estado &&
                    producto.Stock <= producto.StockMinimo);

            // Lista de productos con poco stock.
            List<Producto> productosStockMinimo = await _context.Producto
                .Where(producto =>
                    producto.Estado &&
                    producto.Stock <= producto.StockMinimo)
                .OrderBy(producto => producto.Stock)
                .ThenBy(producto => producto.Nombre)
                .Take(5)
                .ToListAsync();

            // Cantidad de productos vencidos.
            int productosVencidos = await _context.Producto
                .CountAsync(producto =>
                    producto.Estado &&
                    producto.FechaVencimiento.HasValue &&
                    producto.FechaVencimiento.Value < fechaActual);

            // Lista de productos vencidos.
            List<Producto> listaProductosVencidos = await _context.Producto
                .Where(producto =>
                    producto.Estado &&
                    producto.FechaVencimiento.HasValue &&
                    producto.FechaVencimiento.Value < fechaActual)
                .OrderBy(producto => producto.FechaVencimiento)
                .ThenBy(producto => producto.Nombre)
                .Take(5)
                .ToListAsync();

            // Cantidad de productos que vencen desde hoy hasta los próximos 30 días.
            int productosProximosAVencer = await _context.Producto
                .CountAsync(producto =>
                    producto.Estado &&
                    producto.FechaVencimiento.HasValue &&
                    producto.FechaVencimiento.Value >= fechaActual &&
                    producto.FechaVencimiento.Value <= fechaLimiteVencimiento);

            // Lista de productos próximos a vencer.
            List<Producto> listaProductosProximosAVencer =
                await _context.Producto
                    .Where(producto =>
                        producto.Estado &&
                        producto.FechaVencimiento.HasValue &&
                        producto.FechaVencimiento.Value >= fechaActual &&
                        producto.FechaVencimiento.Value <= fechaLimiteVencimiento)
                    .OrderBy(producto => producto.FechaVencimiento)
                    .ThenBy(producto => producto.Nombre)
                    .Take(5)
                    .ToListAsync();

            // Últimas cinco ventas, incluyendo el usuario que las registró.
            List<Venta> ultimasVentas = await _context.Venta
                .Include(venta => venta.IdUsuarioNavigation)
                .OrderByDescending(venta => venta.FechaVenta)
                .Take(5)
                .ToListAsync();

            DashboardViewModel modelo = new DashboardViewModel
            {
                TotalVentas = totalVentas,
                CantidadVentas = cantidadVentas,
                ProductosActivos = productosActivos,
                ProductosBajoStock = productosBajoStock,
                ProductosStockMinimo = productosStockMinimo,
                UltimasVentas = ultimasVentas,

                ProductosVencidos = productosVencidos,
                ProductosProximosAVencer = productosProximosAVencer,
                ListaProductosVencidos = listaProductosVencidos,
                ListaProductosProximosAVencer =
                    listaProductosProximosAVencer
            };

            return View(modelo);
        }
    }
}
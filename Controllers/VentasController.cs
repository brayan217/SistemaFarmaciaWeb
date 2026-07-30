using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Documentos;
using SistemaFarmaciaWeb.Models;
using SistemaFarmaciaWeb.ViewModels;

namespace SistemaFarmaciaWeb.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR VENTAS
        // GET: Ventas
        // =====================================================
        public async Task<IActionResult> Index()
        {
            List<Venta> ventas = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .OrderByDescending(v => v.FechaVenta)
                .ThenByDescending(v => v.IdVenta)
                .ToListAsync();

            return View(ventas);
        }

        // =====================================================
        // VER DETALLES DE UNA VENTA
        // GET: Ventas/Details/5
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Venta? venta = await _context.Venta
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }







        // =====================================================
        // DESCARGAR COMPROBANTE DE VENTA EN PDF
        // GET: Ventas/DescargarPdf/5
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            Venta? venta = await _context.Venta
                .AsNoTracking()
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            ComprobanteVentaPdf documento =
                new ComprobanteVentaPdf(venta);

            byte[] archivoPdf = documento.GeneratePdf();

            string nombreArchivo =
                $"Comprobante-Venta-{venta.IdVenta}.pdf";

            return File(
                archivoPdf,
                "application/pdf",
                nombreArchivo
            );
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA REGISTRAR VENTA
        // GET: Ventas/Create
        // =====================================================
        public async Task<IActionResult> Create()
        {
            await CargarProductos();

            VentaCreateViewModel modelo = new VentaCreateViewModel
            {
                Detalles = new List<DetalleVentaViewModel>
                {
                    new DetalleVentaViewModel
                    {
                        Cantidad = 1
                    }
                }
            };

            return View(modelo);
        }

        // =====================================================
        // GUARDAR VENTA
        // POST: Ventas/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            VentaCreateViewModel modelo)
        {
            LimpiarDetallesVacios(modelo);

            await ValidarVenta(modelo);

            if (!ModelState.IsValid)
            {
                await CargarProductos();
                return View(modelo);
            }

            int? idUsuario = ObtenerIdUsuarioActual();

            if (idUsuario == null)
            {
                TempData["MensajeError"] =
                    "No se pudo identificar al usuario que registra la venta.";

                await CargarProductos();
                return View(modelo);
            }

            decimal total = modelo.Detalles.Sum(
                d => d.Cantidad * d.PrecioUnitario
            );

            decimal cambio = modelo.MontoPagado - total;

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                Venta venta = new Venta
                {
                    FechaVenta = DateTime.Now,
                    Total = total,
                    MontoPagado = modelo.MontoPagado,
                    Cambio = cambio,
                    IdUsuario = idUsuario.Value
                };

                _context.Venta.Add(venta);

                // Guarda la venta para obtener su IdVenta.
                await _context.SaveChangesAsync();

                DateOnly fechaActual =
                    DateOnly.FromDateTime(DateTime.Today);

                foreach (DetalleVentaViewModel detalleModelo
                         in modelo.Detalles)
                {
                    Producto? producto = await _context.Producto
                        .FirstOrDefaultAsync(
                            p => p.IdProducto ==
                                 detalleModelo.IdProducto
                        );

                    if (producto == null)
                    {
                        throw new InvalidOperationException(
                            "Uno de los productos seleccionados no existe."
                        );
                    }

                    // Verifica que el producto continúe activo.
                    if (!producto.Estado)
                    {
                        throw new InvalidOperationException(
                            $"El producto \"{producto.Nombre}\" está inactivo."
                        );
                    }

                    // Vuelve a comprobar el vencimiento dentro
                    // de la transacción.
                    if (producto.FechaVencimiento.HasValue &&
                        producto.FechaVencimiento.Value < fechaActual)
                    {
                        throw new InvalidOperationException(
                            $"El producto \"{producto.Nombre}\" está vencido " +
                            $"desde el {producto.FechaVencimiento.Value:dd/MM/yyyy} " +
                            "y no puede venderse."
                        );
                    }

                    // Vuelve a comprobar el stock dentro
                    // de la transacción.
                    if (producto.Stock < detalleModelo.Cantidad)
                    {
                        throw new InvalidOperationException(
                            $"No existe stock suficiente de " +
                            $"\"{producto.Nombre}\"."
                        );
                    }

                    DetalleVenta detalle = new DetalleVenta
                    {
                        IdVenta = venta.IdVenta,
                        IdProducto = detalleModelo.IdProducto,
                        Cantidad = detalleModelo.Cantidad,
                        PrecioUnitario = detalleModelo.PrecioUnitario
                    };

                    _context.DetalleVenta.Add(detalle);

                    // Disminuye el stock.
                    producto.Stock -= detalleModelo.Cantidad;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] =
                    $"La venta #{venta.IdVenta} fue registrada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = venta.IdVenta }
                );
            }
            catch (InvalidOperationException ex)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message
                );

                await CargarProductos();

                return View(modelo);
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar la venta. " +
                    "Verifica el stock y los datos ingresados."
                );

                await CargarProductos();

                return View(modelo);
            }
        }

        // =====================================================
        // CARGAR PRODUCTOS DISPONIBLES PARA VENDER
        // =====================================================
        private async Task CargarProductos()
        {
            DateOnly fechaActual =
                DateOnly.FromDateTime(DateTime.Today);

            List<Producto> productos = await _context.Producto
                .Where(p =>
                    p.Estado &&
                    p.Stock > 0 &&
                    (
                        !p.FechaVencimiento.HasValue ||
                        p.FechaVencimiento.Value >= fechaActual
                    )
                )
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Productos = productos;
        }

        // =====================================================
        // VALIDAR LA VENTA
        // =====================================================
        private async Task ValidarVenta(
            VentaCreateViewModel modelo)
        {
            if (modelo.Detalles == null ||
                modelo.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.Detalles),
                    "Debes agregar al menos un producto."
                );

                return;
            }

            DateOnly fechaActual =
                DateOnly.FromDateTime(DateTime.Today);

            List<int> productosRepetidos = modelo.Detalles
                .GroupBy(d => d.IdProducto)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (productosRepetidos.Count > 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.Detalles),
                    "No puedes agregar el mismo producto más de una vez."
                );
            }

            for (int i = 0; i < modelo.Detalles.Count; i++)
            {
                DetalleVentaViewModel detalle =
                    modelo.Detalles[i];

                Producto? producto = await _context.Producto
                    .FirstOrDefaultAsync(p =>
                        p.IdProducto == detalle.IdProducto &&
                        p.Estado
                    );

                if (producto == null)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].IdProducto",
                        "Selecciona un producto activo."
                    );

                    continue;
                }

                // Impide vender productos vencidos.
                if (producto.FechaVencimiento.HasValue &&
                    producto.FechaVencimiento.Value < fechaActual)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].IdProducto",
                        $"El producto \"{producto.Nombre}\" está vencido " +
                        $"desde el {producto.FechaVencimiento.Value:dd/MM/yyyy} " +
                        "y no puede venderse."
                    );
                }

                if (detalle.Cantidad <= 0)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].Cantidad",
                        "La cantidad debe ser mayor que cero."
                    );
                }
                else if (detalle.Cantidad > producto.Stock)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].Cantidad",
                        $"Stock insuficiente. Solo hay " +
                        $"{producto.Stock} unidades disponibles."
                    );
                }

                if (detalle.PrecioUnitario <= 0)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].PrecioUnitario",
                        "El precio de venta debe ser mayor que cero."
                    );
                }
            }

            decimal total = modelo.Detalles.Sum(
                d => d.Cantidad * d.PrecioUnitario
            );

            if (total <= 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.Detalles),
                    "El total de la venta debe ser mayor que cero."
                );
            }

            if (modelo.MontoPagado <= 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.MontoPagado),
                    "El monto pagado debe ser mayor que cero."
                );
            }
            else if (modelo.MontoPagado < total)
            {
                ModelState.AddModelError(
                    nameof(modelo.MontoPagado),
                    $"El monto pagado es insuficiente. " +
                    $"El total es Bs. {total:N2}."
                );
            }
        }

        // =====================================================
        // ELIMINAR FILAS VACÍAS DEL FORMULARIO
        // =====================================================
        private static void LimpiarDetallesVacios(
            VentaCreateViewModel modelo)
        {
            modelo.Detalles ??=
                new List<DetalleVentaViewModel>();

            modelo.Detalles = modelo.Detalles
                .Where(d =>
                    d.IdProducto > 0 ||
                    d.Cantidad > 0 ||
                    d.PrecioUnitario > 0
                )
                .ToList();
        }

        // =====================================================
        // OBTENER EL ID DEL USUARIO AUTENTICADO
        // =====================================================
        private int? ObtenerIdUsuarioActual()
        {
            string? valorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(valorId, out int idUsuario))
            {
                return idUsuario;
            }

            return null;
        }
    }
}

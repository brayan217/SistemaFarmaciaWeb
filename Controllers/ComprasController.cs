using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Models;
using SistemaFarmaciaWeb.ViewModels;

namespace SistemaFarmaciaWeb.Controllers
{
    // Todo el módulo de compras es exclusivo
    // para usuarios con el rol Administrador.
    [Authorize(Roles = "Administrador")]
    public class ComprasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComprasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR COMPRAS
        // SOLO ADMINISTRADOR
        // GET: Compras
        // =====================================================
        public async Task<IActionResult> Index()
        {
            List<Compra> compras = await _context.Compra
                .Include(c => c.IdProveedorNavigation)
                .Include(c => c.IdUsuarioNavigation)
                .OrderByDescending(c => c.FechaCompra)
                .ThenByDescending(c => c.IdCompra)
                .ToListAsync();

            return View(compras);
        }

        // =====================================================
        // VER DETALLES DE UNA COMPRA
        // SOLO ADMINISTRADOR
        // GET: Compras/Details/5
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Compra? compra = await _context.Compra
                .Include(c => c.IdProveedorNavigation)
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.DetalleCompra)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdCompra == id);

            if (compra == null)
            {
                return NotFound();
            }

            return View(compra);
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA REGISTRAR COMPRA
        // SOLO ADMINISTRADOR
        // GET: Compras/Create
        // =====================================================
        public async Task<IActionResult> Create()
        {
            await CargarListas();

            CompraCreateViewModel modelo = new CompraCreateViewModel
            {
                FechaCompra = DateTime.Now,

                Detalles = new List<DetalleCompraViewModel>
                {
                    new DetalleCompraViewModel
                    {
                        Cantidad = 1
                    }
                }
            };

            return View(modelo);
        }

        // =====================================================
        // GUARDAR COMPRA
        // SOLO ADMINISTRADOR
        // POST: Compras/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CompraCreateViewModel modelo)
        {
            LimpiarDetallesVacios(modelo);

            await ValidarCompra(modelo);

            if (!ModelState.IsValid)
            {
                await CargarListas();

                return View(modelo);
            }

            int? idUsuario = ObtenerIdUsuarioActual();

            if (idUsuario == null)
            {
                TempData["MensajeError"] =
                    "No se pudo identificar al usuario que registra la compra.";

                await CargarListas();

                return View(modelo);
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                Compra compra = new Compra
                {
                    FechaCompra = modelo.FechaCompra,
                    IdProveedor = modelo.IdProveedor,
                    IdUsuario = idUsuario.Value,

                    Total = modelo.Detalles.Sum(
                        d => d.Cantidad * d.PrecioUnitario
                    )
                };

                _context.Compra.Add(compra);

                // Guarda la compra para obtener su IdCompra.
                await _context.SaveChangesAsync();

                foreach (DetalleCompraViewModel detalleModelo
                         in modelo.Detalles)
                {
                    Producto? producto = await _context.Producto
                        .FirstOrDefaultAsync(p =>
                            p.IdProducto ==
                            detalleModelo.IdProducto
                        );

                    if (producto == null)
                    {
                        throw new InvalidOperationException(
                            "Uno de los productos seleccionados no existe."
                        );
                    }

                    DetalleCompra detalle = new DetalleCompra
                    {
                        IdCompra = compra.IdCompra,
                        IdProducto = detalleModelo.IdProducto,
                        Cantidad = detalleModelo.Cantidad,
                        PrecioUnitario = detalleModelo.PrecioUnitario
                    };

                    _context.DetalleCompra.Add(detalle);

                    // Aumenta el stock con la cantidad comprada.
                    producto.Stock += detalleModelo.Cantidad;

                    // Actualiza el último precio de compra.
                    producto.PrecioCompra =
                        detalleModelo.PrecioUnitario;
                }

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["MensajeExito"] =
                    $"La compra #{compra.IdCompra} fue registrada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = compra.IdCompra }
                );
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar la compra. " +
                    "Verifica los datos e intenta nuevamente."
                );

                await CargarListas();

                return View(modelo);
            }
        }

        // =====================================================
        // CARGAR PROVEEDORES Y PRODUCTOS ACTIVOS
        // =====================================================
        private async Task CargarListas()
        {
            List<Proveedor> proveedores =
                await _context.Proveedor
                    .Where(p => p.Estado)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

            List<Producto> productos =
                await _context.Producto
                    .Where(p => p.Estado)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

            ViewBag.Proveedores = new SelectList(
                proveedores,
                nameof(Proveedor.IdProveedor),
                nameof(Proveedor.Nombre)
            );

            ViewBag.Productos = productos;
        }

        // =====================================================
        // VALIDAR LA COMPRA
        // =====================================================
        private async Task ValidarCompra(
            CompraCreateViewModel modelo)
        {
            bool proveedorValido =
                await _context.Proveedor.AnyAsync(p =>
                    p.IdProveedor == modelo.IdProveedor &&
                    p.Estado
                );

            if (!proveedorValido)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdProveedor),
                    "Debes seleccionar un proveedor activo."
                );
            }

            if (modelo.FechaCompra == default)
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaCompra),
                    "La fecha de compra es obligatoria."
                );
            }

            if (modelo.Detalles == null ||
                modelo.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.Detalles),
                    "Debes agregar al menos un producto."
                );

                return;
            }

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
                DetalleCompraViewModel detalle =
                    modelo.Detalles[i];

                bool productoValido =
                    await _context.Producto.AnyAsync(p =>
                        p.IdProducto == detalle.IdProducto &&
                        p.Estado
                    );

                if (!productoValido)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].IdProducto",
                        "Selecciona un producto activo."
                    );
                }

                if (detalle.Cantidad <= 0)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].Cantidad",
                        "La cantidad debe ser mayor que cero."
                    );
                }

                if (detalle.PrecioUnitario <= 0)
                {
                    ModelState.AddModelError(
                        $"Detalles[{i}].PrecioUnitario",
                        "El precio debe ser mayor que cero."
                    );
                }
            }
        }

        // =====================================================
        // ELIMINAR FILAS VACÍAS DEL FORMULARIO
        // =====================================================
        private static void LimpiarDetallesVacios(
            CompraCreateViewModel modelo)
        {
            modelo.Detalles ??=
                new List<DetalleCompraViewModel>();

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
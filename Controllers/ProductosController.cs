using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.Controllers
{
    // Administrador y Vendedor pueden entrar al módulo.
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR PRODUCTOS
        // ADMINISTRADOR Y VENDEDOR
        // GET: Productos
        // =====================================================
        public async Task<IActionResult> Index()
        {
            List<Producto> productos = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }

        // =====================================================
        // VER DETALLES
        // ADMINISTRADOR Y VENDEDOR
        // GET: Productos/Details/5
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producto? producto = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE CREACIÓN
        // SOLO ADMINISTRADOR
        // GET: Productos/Create
        // =====================================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create()
        {
            await CargarCategorias();

            Producto producto = new Producto
            {
                Stock = 0,
                StockMinimo = 5,
                Estado = true
            };

            return View(producto);
        }

        // =====================================================
        // GUARDAR NUEVO PRODUCTO
        // SOLO ADMINISTRADOR
        // POST: Productos/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(
            [Bind(
                "Codigo,Nombre,Descripcion,PrecioCompra,PrecioVenta," +
                "Stock,StockMinimo,FechaVencimiento,IdCategoria,Estado"
            )]
            Producto producto)
        {
            PrepararProducto(producto);

            ModelState.Remove(
                nameof(Producto.IdCategoriaNavigation)
            );

            await ValidarProducto(producto);

            if (!ModelState.IsValid)
            {
                await CargarCategorias(producto.IdCategoria);

                return View(producto);
            }

            try
            {
                _context.Producto.Add(producto);

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El producto fue registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar el producto. " +
                    "Verifica que el código no esté repetido."
                );

                await CargarCategorias(producto.IdCategoria);

                return View(producto);
            }
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN
        // SOLO ADMINISTRADOR
        // GET: Productos/Edit/5
        // =====================================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producto? producto =
                await _context.Producto.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            await CargarCategorias(producto.IdCategoria);

            return View(producto);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // SOLO ADMINISTRADOR
        // POST: Productos/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "IdProducto,Codigo,Nombre,Descripcion,PrecioCompra," +
                "PrecioVenta,Stock,StockMinimo,FechaVencimiento," +
                "IdCategoria,Estado"
            )]
            Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            PrepararProducto(producto);

            ModelState.Remove(
                nameof(Producto.IdCategoriaNavigation)
            );

            await ValidarProducto(
                producto,
                producto.IdProducto
            );

            if (!ModelState.IsValid)
            {
                await CargarCategorias(producto.IdCategoria);

                return View(producto);
            }

            try
            {
                _context.Producto.Update(producto);

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El producto fue actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductoExiste(
                        producto.IdProducto))
                {
                    return NotFound();
                }

                ModelState.AddModelError(
                    string.Empty,
                    "El producto fue modificado por otro proceso. " +
                    "Intenta nuevamente."
                );
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo actualizar el producto. " +
                    "Verifica que el código no esté repetido."
                );
            }

            await CargarCategorias(producto.IdCategoria);

            return View(producto);
        }

        // =====================================================
        // MOSTRAR CONFIRMACIÓN PARA DESACTIVAR
        // SOLO ADMINISTRADOR
        // GET: Productos/Delete/5
        // =====================================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producto? producto = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(
                    p => p.IdProducto == id
                );

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // =====================================================
        // DESACTIVAR PRODUCTO
        // SOLO ADMINISTRADOR
        // POST: Productos/Delete/5
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            Producto? producto =
                await _context.Producto.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            if (!producto.Estado)
            {
                TempData["MensajeError"] =
                    "El producto ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            producto.Estado = false;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "El producto fue desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LIMPIAR DATOS DEL PRODUCTO
        // =====================================================
        private static void PrepararProducto(
            Producto producto)
        {
            producto.Codigo =
                producto.Codigo?.Trim() ?? string.Empty;

            producto.Nombre =
                producto.Nombre?.Trim() ?? string.Empty;

            producto.Descripcion =
                producto.Descripcion?.Trim();
        }

        // =====================================================
        // VALIDACIONES ADICIONALES
        // =====================================================
        private async Task ValidarProducto(
            Producto producto,
            int? idProductoActual = null)
        {
            bool codigoRepetido =
                await _context.Producto.AnyAsync(p =>
                    p.Codigo == producto.Codigo &&
                    (
                        !idProductoActual.HasValue ||
                        p.IdProducto !=
                        idProductoActual.Value
                    )
                );

            if (codigoRepetido)
            {
                ModelState.AddModelError(
                    nameof(producto.Codigo),
                    "Ya existe un producto con ese código."
                );
            }

            bool categoriaValida =
                await _context.Categoria.AnyAsync(c =>
                    c.IdCategoria ==
                    producto.IdCategoria &&
                    c.Estado
                );

            if (!categoriaValida)
            {
                ModelState.AddModelError(
                    nameof(producto.IdCategoria),
                    "Selecciona una categoría activa."
                );
            }

            if (producto.PrecioCompra < 0)
            {
                ModelState.AddModelError(
                    nameof(producto.PrecioCompra),
                    "El precio de compra no puede ser negativo."
                );
            }

            if (producto.PrecioVenta < 0)
            {
                ModelState.AddModelError(
                    nameof(producto.PrecioVenta),
                    "El precio de venta no puede ser negativo."
                );
            }

            if (producto.Stock < 0)
            {
                ModelState.AddModelError(
                    nameof(producto.Stock),
                    "El stock no puede ser negativo."
                );
            }

            if (producto.StockMinimo < 0)
            {
                ModelState.AddModelError(
                    nameof(producto.StockMinimo),
                    "El stock mínimo no puede ser negativo."
                );
            }
        }

        // =====================================================
        // COMPROBAR SI EL PRODUCTO EXISTE
        // =====================================================
        private async Task<bool> ProductoExiste(int id)
        {
            return await _context.Producto
                .AnyAsync(
                    p => p.IdProducto == id
                );
        }

        // =====================================================
        // CARGAR CATEGORÍAS ACTIVAS EN EL SELECT
        // =====================================================
        private async Task CargarCategorias(
            int? categoriaSeleccionada = null)
        {
            List<Categoria> categorias =
                await _context.Categoria
                    .Where(c => c.Estado)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

            ViewData["IdCategoria"] = new SelectList(
                categorias,
                nameof(Categoria.IdCategoria),
                nameof(Categoria.Nombre),
                categoriaSeleccionada
            );
        }
    }
}
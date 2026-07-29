using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.Controllers
{
    // Solo los usuarios con el rol Administrador
    // pueden acceder al módulo de proveedores.
    [Authorize(Roles = "Administrador")]
    public class ProveedoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR PROVEEDORES
        // GET: Proveedores
        // =====================================================
        public async Task<IActionResult> Index()
        {
            List<Proveedor> proveedores = await _context.Proveedor
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(proveedores);
        }

        // =====================================================
        // VER DETALLES
        // GET: Proveedores/Details/5
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Proveedor? proveedor = await _context.Proveedor
                .FirstOrDefaultAsync(p => p.IdProveedor == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA CREAR
        // GET: Proveedores/Create
        // =====================================================
        public IActionResult Create()
        {
            Proveedor proveedor = new Proveedor
            {
                Estado = true
            };

            return View(proveedor);
        }

        // =====================================================
        // GUARDAR NUEVO PROVEEDOR
        // POST: Proveedores/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Telefono,Correo,Direccion,Estado")]
            Proveedor proveedor)
        {
            PrepararProveedor(proveedor);

            await ValidarProveedor(proveedor);

            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            try
            {
                _context.Proveedor.Add(proveedor);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El proveedor fue registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar el proveedor. " +
                    "Verifica que los datos no estén repetidos."
                );

                return View(proveedor);
            }
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA EDITAR
        // GET: Proveedores/Edit/5
        // =====================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Proveedor? proveedor =
                await _context.Proveedor.FindAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // POST: Proveedores/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "IdProveedor,Nombre,Telefono,Correo,Direccion,Estado"
            )]
            Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor)
            {
                return NotFound();
            }

            PrepararProveedor(proveedor);

            await ValidarProveedor(
                proveedor,
                proveedor.IdProveedor
            );

            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            try
            {
                _context.Proveedor.Update(proveedor);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El proveedor fue actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProveedorExiste(
                        proveedor.IdProveedor))
                {
                    return NotFound();
                }

                ModelState.AddModelError(
                    string.Empty,
                    "El proveedor fue modificado por otro proceso. " +
                    "Intenta nuevamente."
                );
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo actualizar el proveedor. " +
                    "Verifica que los datos no estén repetidos."
                );
            }

            return View(proveedor);
        }

        // =====================================================
        // MOSTRAR CONFIRMACIÓN PARA DESACTIVAR
        // GET: Proveedores/Delete/5
        // =====================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Proveedor? proveedor = await _context.Proveedor
                .FirstOrDefaultAsync(
                    p => p.IdProveedor == id
                );

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // =====================================================
        // DESACTIVAR PROVEEDOR
        // POST: Proveedores/Delete/5
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Proveedor? proveedor =
                await _context.Proveedor.FindAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            if (!proveedor.Estado)
            {
                TempData["MensajeError"] =
                    "El proveedor ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            proveedor.Estado = false;

            try
            {
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El proveedor fue desactivado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["MensajeError"] =
                    "No se pudo desactivar el proveedor.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LIMPIAR DATOS DEL PROVEEDOR
        // =====================================================
        private static void PrepararProveedor(
            Proveedor proveedor)
        {
            proveedor.Nombre =
                proveedor.Nombre?.Trim() ?? string.Empty;

            proveedor.Telefono =
                proveedor.Telefono?.Trim();

            proveedor.Correo =
                proveedor.Correo?.Trim().ToLower();

            proveedor.Direccion =
                proveedor.Direccion?.Trim();
        }

        // =====================================================
        // VALIDACIONES ADICIONALES
        // =====================================================
        private async Task ValidarProveedor(
            Proveedor proveedor,
            int? idProveedorActual = null)
        {
            if (string.IsNullOrWhiteSpace(
                    proveedor.Nombre))
            {
                ModelState.AddModelError(
                    nameof(proveedor.Nombre),
                    "El nombre del proveedor es obligatorio."
                );
            }

            bool nombreRepetido =
                await _context.Proveedor.AnyAsync(p =>
                    p.Nombre.ToLower() ==
                    proveedor.Nombre.ToLower() &&
                    (
                        !idProveedorActual.HasValue ||
                        p.IdProveedor !=
                        idProveedorActual.Value
                    )
                );

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(proveedor.Nombre),
                    "Ya existe un proveedor con ese nombre."
                );
            }

            if (!string.IsNullOrWhiteSpace(
                    proveedor.Correo))
            {
                if (!CorreoValido(proveedor.Correo))
                {
                    ModelState.AddModelError(
                        nameof(proveedor.Correo),
                        "Ingresa una dirección de correo válida."
                    );
                }
                else
                {
                    bool correoRepetido =
                        await _context.Proveedor.AnyAsync(p =>
                            p.Correo != null &&
                            p.Correo.ToLower() ==
                            proveedor.Correo.ToLower() &&
                            (
                                !idProveedorActual.HasValue ||
                                p.IdProveedor !=
                                idProveedorActual.Value
                            )
                        );

                    if (correoRepetido)
                    {
                        ModelState.AddModelError(
                            nameof(proveedor.Correo),
                            "Ya existe un proveedor con ese correo."
                        );
                    }
                }
            }
        }

        // =====================================================
        // VALIDAR FORMATO DEL CORREO
        // =====================================================
        private static bool CorreoValido(string correo)
        {
            try
            {
                MailAddress direccion =
                    new MailAddress(correo);

                return direccion.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        // =====================================================
        // COMPROBAR SI EL PROVEEDOR EXISTE
        // =====================================================
        private async Task<bool> ProveedorExiste(int id)
        {
            return await _context.Proveedor
                .AnyAsync(
                    p => p.IdProveedor == id
                );
        }
    }
}
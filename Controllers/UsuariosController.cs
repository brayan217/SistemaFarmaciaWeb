using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.Controllers
{
    // Todo el módulo de usuarios es exclusivo
    // para usuarios con el rol Administrador.
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] RolesPermitidos =
        {
            "Administrador",
            "Vendedor"
        };

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR USUARIOS
        // SOLO ADMINISTRADOR
        // GET: Usuarios
        // =====================================================
        public async Task<IActionResult> Index()
        {
            List<Usuario> usuarios = await _context.Usuario
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        // =====================================================
        // VER DETALLES
        // SOLO ADMINISTRADOR
        // GET: Usuarios/Details/5
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Usuario? usuario = await _context.Usuario
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id
                );

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE REGISTRO
        // SOLO ADMINISTRADOR
        // GET: Usuarios/Create
        // =====================================================
        public IActionResult Create()
        {
            Usuario usuario = new Usuario
            {
                // La base de datos permite Administrador o Vendedor.
                Rol = "Vendedor",
                Estado = true
            };

            return View(usuario);
        }

        // =====================================================
        // GUARDAR NUEVO USUARIO
        // SOLO ADMINISTRADOR
        // POST: Usuarios/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Correo,Contrasena,Rol,Estado")]
            Usuario usuario)
        {
            PrepararUsuario(usuario);

            await ValidarUsuario(usuario);

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            try
            {
                _context.Usuario.Add(usuario);

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El usuario fue registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo registrar el usuario. " +
                    "Verifica el correo y el rol seleccionado."
                );

                return View(usuario);
            }
        }

        // =====================================================
        // MOSTRAR FORMULARIO DE EDICIÓN
        // SOLO ADMINISTRADOR
        // GET: Usuarios/Edit/5
        // =====================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Usuario? usuario =
                await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // =====================================================
        // GUARDAR CAMBIOS DEL USUARIO
        // SOLO ADMINISTRADOR
        // POST: Usuarios/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "IdUsuario,Nombre,Correo,Contrasena,Rol,Estado"
            )]
            Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            PrepararUsuario(usuario);

            await ValidarUsuario(
                usuario,
                usuario.IdUsuario
            );

            int? idUsuarioActual =
                ObtenerIdUsuarioActual();

            // Impide que el administrador desactive
            // su propia cuenta mientras está conectado.
            if (idUsuarioActual == usuario.IdUsuario &&
                !usuario.Estado)
            {
                ModelState.AddModelError(
                    nameof(usuario.Estado),
                    "No puedes desactivar tu propia cuenta."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            try
            {
                _context.Usuario.Update(usuario);

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El usuario fue actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UsuarioExiste(
                        usuario.IdUsuario))
                {
                    return NotFound();
                }

                ModelState.AddModelError(
                    string.Empty,
                    "El usuario fue modificado por otro proceso. " +
                    "Intenta nuevamente."
                );
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo actualizar el usuario. " +
                    "Verifica el correo y el rol seleccionado."
                );
            }

            return View(usuario);
        }

        // =====================================================
        // MOSTRAR CONFIRMACIÓN PARA DESACTIVAR
        // SOLO ADMINISTRADOR
        // GET: Usuarios/Delete/5
        // =====================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Usuario? usuario = await _context.Usuario
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id
                );

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // =====================================================
        // DESACTIVAR USUARIO
        // SOLO ADMINISTRADOR
        // POST: Usuarios/Delete/5
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            Usuario? usuario =
                await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            int? idUsuarioActual =
                ObtenerIdUsuarioActual();

            if (idUsuarioActual == usuario.IdUsuario)
            {
                TempData["MensajeError"] =
                    "No puedes desactivar tu propia cuenta.";

                return RedirectToAction(nameof(Index));
            }

            if (!usuario.Estado)
            {
                TempData["MensajeError"] =
                    "El usuario ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            // No se elimina físicamente de SQL Server.
            usuario.Estado = false;

            try
            {
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "El usuario fue desactivado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["MensajeError"] =
                    "No se pudo desactivar el usuario.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LIMPIAR Y NORMALIZAR DATOS
        // =====================================================
        private static void PrepararUsuario(
            Usuario usuario)
        {
            usuario.Nombre =
                usuario.Nombre?.Trim() ?? string.Empty;

            usuario.Correo =
                usuario.Correo?.Trim().ToLower()
                ?? string.Empty;

            usuario.Contrasena =
                usuario.Contrasena?.Trim()
                ?? string.Empty;

            usuario.Rol =
                NormalizarRol(usuario.Rol);
        }

        // =====================================================
        // VALIDACIONES DEL USUARIO
        // =====================================================
        private async Task ValidarUsuario(
            Usuario usuario,
            int? idUsuarioActual = null)
        {
            if (string.IsNullOrWhiteSpace(
                    usuario.Nombre))
            {
                ModelState.AddModelError(
                    nameof(usuario.Nombre),
                    "El nombre es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(
                    usuario.Correo))
            {
                ModelState.AddModelError(
                    nameof(usuario.Correo),
                    "El correo es obligatorio."
                );
            }
            else
            {
                bool correoExiste =
                    await _context.Usuario.AnyAsync(u =>
                        u.Correo.ToLower() ==
                        usuario.Correo.ToLower() &&
                        (
                            !idUsuarioActual.HasValue ||
                            u.IdUsuario !=
                            idUsuarioActual.Value
                        )
                    );

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        nameof(usuario.Correo),
                        "Este correo ya está registrado."
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(
                    usuario.Contrasena))
            {
                ModelState.AddModelError(
                    nameof(usuario.Contrasena),
                    "La contraseña es obligatoria."
                );
            }

            bool rolValido =
                RolesPermitidos.Contains(usuario.Rol);

            if (!rolValido)
            {
                ModelState.AddModelError(
                    nameof(usuario.Rol),
                    "Selecciona el rol Administrador o Vendedor."
                );
            }
        }

        // =====================================================
        // NORMALIZAR EL NOMBRE DEL ROL
        // =====================================================
        private static string NormalizarRol(
            string? rol)
        {
            string valor = rol?.Trim() ?? string.Empty;

            if (valor.Equals(
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Administrador";
            }

            if (valor.Equals(
                    "Vendedor",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Vendedor";
            }

            return valor;
        }

        // =====================================================
        // OBTENER EL USUARIO CONECTADO
        // =====================================================
        private int? ObtenerIdUsuarioActual()
        {
            string? valorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (int.TryParse(
                    valorId,
                    out int idUsuario))
            {
                return idUsuario;
            }

            return null;
        }

        // =====================================================
        // COMPROBAR SI EXISTE EL USUARIO
        // =====================================================
        private async Task<bool> UsuarioExiste(
            int id)
        {
            return await _context.Usuario
                .AnyAsync(u =>
                    u.IdUsuario == id
                );
        }
    }
}
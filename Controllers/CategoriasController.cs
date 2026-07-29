using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;
using SistemaFarmaciaWeb.Models;

namespace SistemaFarmaciaWeb.Controllers
{
    // Todo el módulo de categorías es exclusivo
    // para usuarios con el rol Administrador.
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTAR CATEGORÍAS
        // SOLO ADMINISTRADOR
        // GET: Categorias
        // =====================================================
        public async Task<IActionResult> Index()
        {
            List<Categoria> categorias =
                await _context.Categoria
                    .OrderBy(categoria => categoria.Nombre)
                    .ToListAsync();

            return View(categorias);
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA CREAR
        // SOLO ADMINISTRADOR
        // GET: Categorias/Create
        // =====================================================
        public IActionResult Create()
        {
            Categoria categoria = new Categoria
            {
                Estado = true
            };

            return View(categoria);
        }

        // =====================================================
        // GUARDAR NUEVA CATEGORÍA
        // SOLO ADMINISTRADOR
        // POST: Categorias/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Categoria categoria)
        {
            categoria.Nombre =
                categoria.Nombre?.Trim() ?? string.Empty;

            categoria.Descripcion =
                categoria.Descripcion?.Trim();

            bool nombreRepetido =
                await _context.Categoria.AnyAsync(c =>
                    c.Nombre == categoria.Nombre
                );

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(categoria.Nombre),
                    "Ya existe una categoría con ese nombre."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            _context.Categoria.Add(categoria);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "La categoría fue registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // MOSTRAR FORMULARIO PARA EDITAR
        // SOLO ADMINISTRADOR
        // GET: Categorias/Edit/5
        // =====================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Categoria? categoria =
                await _context.Categoria.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // =====================================================
        // GUARDAR CAMBIOS
        // SOLO ADMINISTRADOR
        // POST: Categorias/Edit/5
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Categoria categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return NotFound();
            }

            categoria.Nombre =
                categoria.Nombre?.Trim() ?? string.Empty;

            categoria.Descripcion =
                categoria.Descripcion?.Trim();

            bool nombreRepetido =
                await _context.Categoria.AnyAsync(c =>
                    c.Nombre == categoria.Nombre &&
                    c.IdCategoria != categoria.IdCategoria
                );

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(categoria.Nombre),
                    "Ya existe otra categoría con ese nombre."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            try
            {
                _context.Categoria.Update(categoria);

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "La categoría fue actualizada correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                bool categoriaExiste =
                    await _context.Categoria.AnyAsync(c =>
                        c.IdCategoria == categoria.IdCategoria
                    );

                if (!categoriaExiste)
                {
                    return NotFound();
                }

                ModelState.AddModelError(
                    string.Empty,
                    "La categoría fue modificada por otro proceso. " +
                    "Intenta nuevamente."
                );

                return View(categoria);
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // MOSTRAR CONFIRMACIÓN PARA ELIMINAR
        // SOLO ADMINISTRADOR
        // GET: Categorias/Delete/5
        // =====================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Categoria? categoria =
                await _context.Categoria
                    .Include(c => c.Producto)
                    .FirstOrDefaultAsync(c =>
                        c.IdCategoria == id
                    );

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // =====================================================
        // ELIMINAR CATEGORÍA
        // SOLO ADMINISTRADOR
        // POST: Categorias/Delete/5
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            Categoria? categoria =
                await _context.Categoria
                    .Include(c => c.Producto)
                    .FirstOrDefaultAsync(c =>
                        c.IdCategoria == id
                    );

            if (categoria == null)
            {
                return NotFound();
            }

            if (categoria.Producto.Any())
            {
                TempData["MensajeError"] =
                    "No se puede eliminar la categoría porque " +
                    "tiene productos registrados.";

                return RedirectToAction(nameof(Index));
            }

            _context.Categoria.Remove(categoria);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "La categoría fue eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
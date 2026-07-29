using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaWeb.Data;

namespace SistemaFarmaciaWeb.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==========================
    // Muestra la página de Login
    // ==========================
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // ==========================
    // Procesa el Login
    // ==========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            ViewBag.Error = "Debe ingresar el correo y la contraseña.";
            return View();
        }

        var usuario = await _context.Usuario.FirstOrDefaultAsync(u =>
            u.Correo == correo &&
            u.Contrasena == contrasena &&
            u.Estado);

        if (usuario == null)
        {
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return RedirectToAction("Index", "Home");
    }

    // ==========================
    // Cerrar sesión
    // ==========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction(nameof(Login));
    }

    // ==========================
    // Acceso denegado
    // ==========================
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
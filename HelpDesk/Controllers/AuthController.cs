using Microsoft.AspNetCore.Mvc;
using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;



namespace HelpDesk.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            try
            {
                Console.WriteLine($"=== TENTATIVA DE LOGIN ===");
                Console.WriteLine($"Email: {email}");
                Console.WriteLine($"Senha: {senha}");

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);

                if (usuario != null)
                {
                    Console.WriteLine($"✅ USUÁRIO ENCONTRADO: {usuario.Nome}");
                    Console.WriteLine($"✅ IsAdmin: {usuario.IsAdministrador}");

                    // Criar claims
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim("IsAdmin", usuario.IsAdministrador.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    // Sessão
                    HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
                    HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);

                    Console.WriteLine($"✅ REDIRECIONANDO PARA Chamados/Index");
                    return RedirectToAction("Index", "Chamados");
                }
                else
                {
                    Console.WriteLine($"❌ USUÁRIO NÃO ENCONTRADO");
                    ViewBag.MensagemErro = "Email ou senha inválidos"; 
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO: {ex.Message}");
                TempData["MensagemErro"] = "Erro interno no servidor";
                return View();
            }
        }
        // GET: Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["MensagemSucesso"] = "Logout realizado com sucesso!";
            return RedirectToAction("Login", "Auth");
        }

        // GET: Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Método auxiliar para verificar se usuário está logado
        public static bool IsUserLoggedIn(HttpContext httpContext)
        {
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
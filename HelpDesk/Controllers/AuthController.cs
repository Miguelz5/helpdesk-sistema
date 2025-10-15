using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;
using HelpDesk.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            try
            {
                // LIMPAR MENSAGENS ANTIGAS
                TempData.Remove("MensagemSucesso");
                TempData.Remove("MensagemErro");
                ModelState.Clear(); // Limpar erros anteriores

                // Buscar usuário no banco
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);

                if (usuario != null)
                {
                    // Criar claims (dados do usuário)
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim("IsAdmin", usuario.IsAdministrador.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Criar cookie de autenticação
                    await HttpContext.SignInAsync(claimsPrincipal);

                    // Salvar dados na sessão
                    HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
                    HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);

                    TempData["MensagemSucesso"] = "Login realizado com sucesso!";
                    return RedirectToAction("Index", "Chamados");
                }
                else
                {
                    // ADICIONAR ERRO AO MODELSTATE (aparece na view de login)
                    ModelState.AddModelError("", "Email ou senha inválidos");
                    ViewBag.MensagemErro = "Email ou senha inválidos"; // Alternativa
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao conectar com o banco de dados");
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
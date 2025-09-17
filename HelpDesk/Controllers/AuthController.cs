using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace HelpDesk.Controllers
{
    public class AuthController : Controller
    {
        // Credenciais padrão
        private const string DEFAULT_EMAIL = "admin@helpdesk.com";
        private const string DEFAULT_PASSWORD = "admin123";

        // GET: Auth/Login
        public IActionResult Login()
        {
            // Se já estiver logado, redireciona para os chamados
            if (HttpContext.Session.GetString("UsuarioLogado") == "true")
            {
                return RedirectToAction("Index", "Chamados");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                bool loginValido = false;
                string nomeUsuario = "";

                // Verificar credenciais padrão
                if (login.Email == DEFAULT_EMAIL && login.Senha == DEFAULT_PASSWORD)
                {
                    loginValido = true;
                    nomeUsuario = "Administrador Padrão";
                }

                // Verificar usuários cadastrados
                if (!loginValido)
                {
                    var usuario = UsuariosController.GetUsuarios()
                        .FirstOrDefault(u => u.Email == login.Email && u.Senha == login.Senha && u.IsAdministrador);

                    if (usuario != null)
                    {
                        loginValido = true;
                        nomeUsuario = usuario.Nome;
                    }
                }

                if (loginValido)
                {
                    // Configurar sessão
                    HttpContext.Session.SetString("UsuarioLogado", "true");
                    HttpContext.Session.SetString("UsuarioNome", nomeUsuario);
                    HttpContext.Session.SetString("UsuarioEmail", login.Email);

                    return RedirectToAction("Index", "Chamados");
                }

                ModelState.AddModelError(string.Empty, "Email ou senha inválidos");
            }

            return View(login);
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            // Limpar sessão
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Método helper para verificar se usuário está logado
        public static bool IsUserLoggedIn(HttpContext context)
        {
            return context.Session.GetString("UsuarioLogado") == "true";
        }
    }
}
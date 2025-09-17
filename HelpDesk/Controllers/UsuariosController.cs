using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using HelpDesk.Models;

namespace HelpDesk.Controllers
{
    public class UsuariosController : Controller
    {
        private static List<Usuario> usuarios = new List<Usuario>();
        private static int nextId = 1;

        // Helper method para verificar login
        private IActionResult CheckLogin()
        {
            if (!AuthController.IsUserLoggedIn(HttpContext))
            {
                // Só mostra mensagem de erro se não for a página inicial
                if (HttpContext.Request.Path != "/")
                {
                    TempData["MensagemErro"] = "Por favor, faça login para acessar o sistema";
                }
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // Método estático para acesso à lista de usuários
        public static List<Usuario> GetUsuarios()
        {
            return usuarios;
        }

        // GET: Usuarios
        public IActionResult Index()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            return View(usuarios);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (ModelState.IsValid)
            {
                usuario.Id = nextId++;
                usuario.DataCadastro = DateTime.Now;
                usuario.IsAdministrador = true;
                usuarios.Add(usuario);
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public IActionResult Delete(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            Usuario usuario = usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            Usuario usuario = usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                usuarios.Remove(usuario);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
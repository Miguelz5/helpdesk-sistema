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

                if (HttpContext.Request.Path != "/")
                {
                    // Só mostra mensagem de erro se não for a página inicial
                    TempData["MensagemErro"] = "Por favor, faça login para acessar o sistema";
                }
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // GET: Usuarios - MÉTODO QUE ESTAVA FALTANDO!!!
        public IActionResult Index()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            return View(usuarios);
        }

        // Método estático para acesso à lista de usuários
        public static List<Usuario> GetUsuarios()
        {
            return usuarios;
        }

        // GET: Usuarios
        // GET: Usuarios/Edit/5
        public IActionResult Edit(int id)
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

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Usuario usuario)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Remova a validação automática do ModelState para Senha
            ModelState.Remove("Senha");

            if (ModelState.IsValid)
            {
                Usuario existing = usuarios.FirstOrDefault(u => u.Id == usuario.Id);
                if (existing != null)
                {
                    existing.Nome = usuario.Nome;
                    existing.Email = usuario.Email;

                    // Só atualiza a senha se foi informada e não está vazia
                    if (!string.IsNullOrEmpty(usuario.Senha))
                    {
                        existing.Senha = usuario.Senha;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
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

            // Validação manual da senha apenas no Create
            if (string.IsNullOrEmpty(usuario.Senha))
            {
                ModelState.AddModelError("Senha", "A senha é obrigatória");
            }

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
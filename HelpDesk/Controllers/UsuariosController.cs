using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization; // ← ADICIONAR ESTE USING
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDesk.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios/Perfil - Meu perfil
        [Authorize] // ← AGORA VAI FUNCIONAR
        public async Task<IActionResult> Perfil()
        {
            // Pegar ID do usuário logado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Perfil - Editar meu perfil
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario usuario)
        {
            // Pegar ID do usuário logado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (usuarioId != usuario.Id)
            {
                return Forbid(); // Não pode editar outro usuário
            }

            // Remover validação da senha para permitir campo vazio
            ModelState.Remove("Senha");

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioExistente = await _context.Usuarios.FindAsync(usuarioId);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    // Verificar se email já existe (exceto para o próprio usuário)
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email && u.Id != usuarioId))
                    {
                        ModelState.AddModelError("Email", "Este email já está em uso por outro usuário");
                        return View(usuario);
                    }

                    // Atualizar dados
                    usuarioExistente.Nome = usuario.Nome;
                    usuarioExistente.Email = usuario.Email;

                    // Só atualiza a senha se foi informada
                    if (!string.IsNullOrEmpty(usuario.Senha))
                    {
                        usuarioExistente.Senha = usuario.Senha;
                    }

                    _context.Update(usuarioExistente);
                    await _context.SaveChangesAsync();

                    // Atualizar nome no cookie de autenticação
                    await UpdateAuthCookie(usuarioExistente);

                    TempData["MensagemSucesso"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction(nameof(Perfil));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(usuario);
        }

        // Método para atualizar o cookie de autenticação
        private async Task UpdateAuthCookie(Usuario usuario)
        {
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
        }

        // GET: Usuarios
        [Authorize] // ← PROTEGER A LISTA DE USUÁRIOS
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Verificar se email já existe
                if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                {
                    ModelState.AddModelError("Email", "Este email já está cadastrado");
                    return View(usuario);
                }

                usuario.DataCadastro = DateTime.Now;
                usuario.IsAdministrador = true;

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Administrador cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Remover validação da senha para permitir campo vazio
            ModelState.Remove("Senha");

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioExistente = await _context.Usuarios.FindAsync(id);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    // Atualizar apenas os campos permitidos
                    usuarioExistente.Nome = usuario.Nome;
                    usuarioExistente.Email = usuario.Email;

                    // Só atualiza a senha se foi informada
                    if (!string.IsNullOrEmpty(usuario.Senha))
                    {
                        usuarioExistente.Senha = usuario.Senha;
                    }

                    _context.Update(usuarioExistente);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Administrador atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioLogadoId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                // Se o usuário deletou a própria conta, fazer logout
                if (usuarioLogadoId == id)
                {
                    await HttpContext.SignOutAsync();
                    HttpContext.Session.Clear();
                    TempData["MensagemSucesso"] = "Sua conta foi excluída com sucesso!";
                    return RedirectToAction("Login", "Auth");
                }

                TempData["MensagemSucesso"] = "Administrador excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;
using HelpDesk.Data;
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

        // ✅ MÉTODO INDEX ATUALIZADO - MOSTRA APENAS ADMINISTRADORES
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // ✅ FILTRAR APENAS ADMINISTRADORES
            var administradores = await _context.Usuarios
                .Where(u => u.IsAdministrador == true)
                .ToListAsync();

            return View(administradores);
        }

        // ✅ NOVO MÉTODO PARA USUÁRIOS COMUNS
        [Authorize]
        public async Task<IActionResult> UsuariosComuns()
        {
            // ✅ FILTRAR APENAS USUÁRIOS COMUNS
            var usuariosComuns = await _context.Usuarios
                .Where(u => u.IsAdministrador == false)
                .ToListAsync();

            return View(usuariosComuns);
        }

        // GET: Usuarios/Perfil
        [Authorize]
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

        // POST: Usuarios/Perfil
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario usuario)
        {
            // Pegar ID do usuário logado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (usuarioId != usuario.Id)
            {
                return Forbid();
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

        [Authorize]
        public IActionResult Create(string from)
        {
            ViewBag.FromUsuariosComuns = (from == "usuarioscomuns");
            return View();
        }

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

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                if (usuario.IsAdministrador)
                {
                    TempData["MensagemSucesso"] = "Administrador cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["MensagemSucesso"] = "Usuário comum cadastrado com sucesso!";
                    return RedirectToAction(nameof(UsuariosComuns));
                }
            }
            return View(usuario);
        }

        [Authorize]
        public IActionResult CreateUsuarioComum()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUsuarioComum(Usuario usuario)
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
                usuario.IsAdministrador = false;

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Usuário comum cadastrado com sucesso!";
                return RedirectToAction(nameof(UsuariosComuns));
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

                    // Verificar se email já existe (exceto para o próprio usuário)
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email && u.Id != id))
                    {
                        ModelState.AddModelError("Email", "Este email já está em uso por outro usuário");
                        return View(usuario);
                    }

                    // Atualizar apenas os campos permitidos
                    usuarioExistente.Nome = usuario.Nome;
                    usuarioExistente.Email = usuario.Email;
                    usuarioExistente.IsAdministrador = usuario.IsAdministrador; // ✅ PERMITIR ALTERAR TIPO

                    // Só atualiza a senha se foi informada
                    if (!string.IsNullOrEmpty(usuario.Senha))
                    {
                        usuarioExistente.Senha = usuario.Senha;
                    }

                    _context.Update(usuarioExistente);
                    await _context.SaveChangesAsync();

                    // Se o usuário editou o próprio perfil, atualizar o cookie
                    var usuarioLogadoId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    if (usuarioLogadoId == id)
                    {
                        await UpdateAuthCookie(usuarioExistente);
                    }

                    TempData["MensagemSucesso"] = "Usuário atualizado com sucesso!";
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

                // ✅ REDIRECIONAR PARA A LISTA CORRETA
                if (usuario.IsAdministrador)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(UsuariosComuns));
                }
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
                // ✅ SALVAR O TIPO DO USUÁRIO ANTES DE EXCLUIR
                var isAdministrador = usuario.IsAdministrador;

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

                TempData["MensagemSucesso"] = "Usuário excluído com sucesso!";

                // ✅ REDIRECIONAR PARA A LISTA CORRETA
                if (isAdministrador)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(UsuariosComuns));
                }
            }

            return RedirectToAction(nameof(Index));
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

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
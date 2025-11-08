using HelpDesk.Models;
using HelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Se estiver usando Entity Framework

namespace HelpDesk.Controllers
{
    public class FaqController : Controller
    {
        private readonly AppDbContext _context;

        public FaqController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Faq - Página pública
        public async Task<IActionResult> Index()
        {
            var faqs = await _context.Faqs
                .Where(f => f.Ativo)
                .OrderBy(f => f.Ordem)
                .ThenBy(f => f.Categoria)
                .ToListAsync();

            // Agrupar por categoria para a view
            var faqsPorCategoria = faqs.GroupBy(f => f.Categoria);
            return View(faqsPorCategoria);
        }

        // GET: Faq/Admin - Área administrativa
        public async Task<IActionResult> Admin()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            var faqs = await _context.Faqs
                .OrderBy(f => f.Ordem)
                .ThenBy(f => f.Categoria)
                .ToListAsync();

            return View(faqs);
        }

        // GET: Faq/Create
        public IActionResult Create()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            ViewBag.Categorias = new List<string>
            {
                "Geral",
                "Acesso e Login",
                "Problemas Técnicos",
                "Software",
                "Hardware",
                "Rede",
                "Outros"
            };
            return View();
        }

        // POST: Faq/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Faq faq)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(faq);
                    await _context.SaveChangesAsync();
                    TempData["MensagemSucesso"] = "FAQ criada com sucesso!";
                    return RedirectToAction(nameof(Admin));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erro ao salvar FAQ: " + ex.Message);
                }
            }

            ViewBag.Categorias = new List<string>
            {
                "Geral",
                "Acesso e Login",
                "Problemas Técnicos",
                "Software",
                "Hardware",
                "Rede",
                "Outros"
            };
            return View(faq);
        }

        // GET: Faq/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id == null) return NotFound();

            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound();

            ViewBag.Categorias = new List<string>
            {
                "Geral",
                "Acesso e Login",
                "Problemas Técnicos",
                "Software",
                "Hardware",
                "Rede",
                "Outros"
            };
            return View(faq);
        }

        // POST: Faq/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Faq faq)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id != faq.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faq);
                    await _context.SaveChangesAsync();
                    TempData["MensagemSucesso"] = "FAQ atualizada com sucesso!";
                    return RedirectToAction(nameof(Admin));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FaqExists(faq.Id)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erro ao atualizar FAQ: " + ex.Message);
                }
            }

            ViewBag.Categorias = new List<string>
            {
                "Geral",
                "Acesso e Login",
                "Problemas Técnicos",
                "Software",
                "Hardware",
                "Rede",
                "Outros"
            };
            return View(faq);
        }

        // POST: Faq/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            var faq = await _context.Faqs.FindAsync(id);
            if (faq != null)
            {
                _context.Faqs.Remove(faq);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "FAQ excluída com sucesso!";
            }
            return RedirectToAction(nameof(Admin));
        }

        // POST: Faq/ToggleStatus/5 - Ativar/Desativar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            var faq = await _context.Faqs.FindAsync(id);
            if (faq != null)
            {
                faq.Ativo = !faq.Ativo;
                _context.Update(faq);
                await _context.SaveChangesAsync();

                var status = faq.Ativo ? "ativada" : "desativada";
                TempData["MensagemSucesso"] = $"FAQ {status} com sucesso!";
            }
            return RedirectToAction(nameof(Admin));
        }

        private bool FaqExists(int id)
        {
            return _context.Faqs.Any(e => e.Id == id);
        }

        private IActionResult CheckLogin()
        {
            if (!AuthController.IsUserLoggedIn(HttpContext))
            {
                TempData["MensagemErro"] = "Por favor, faça login para acessar esta área";
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }
    }
}
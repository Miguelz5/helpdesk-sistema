using HelpDesk.Models;
using HelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    public class ChamadosController : Controller
    {
        private readonly AppDbContext _context;

        // CONSTRUTOR COM INJEÇÃO DO BANCO
        public ChamadosController(AppDbContext context)
        {
            _context = context;
        }

        // Helper method para verificar login
        private IActionResult CheckLogin()
        {
            if (!AuthController.IsUserLoggedIn(HttpContext))
            {
                if (HttpContext.Request.Path != "/")
                {
                    TempData["MensagemErro"] = "Por favor, faça login para acessar o sistema";
                }
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // GET: Chamados
        public async Task<IActionResult> Index()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            // DATAS PARA OS PERÍODOS
            var hoje = DateTime.Today;
            var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            // BUSCAR DO BANCO - ESTATÍSTICAS GERAIS
            var chamadosUrgentes = await _context.Chamados
                .Where(c => c.Prioridade == "Urgente" && c.Status != "Resolvido")
                .ToListAsync();

            var todosChamados = await _context.Chamados.ToListAsync();

            // BUSCAR DO BANCO - ESTATÍSTICAS POR PERÍODO
            ViewBag.ChamadosHoje = await _context.Chamados
                .Where(c => c.DataAbertura.Date == hoje)
                .CountAsync();

            ViewBag.ResolvidosHoje = await _context.Chamados
                .Where(c => c.DataFechamento.HasValue &&
                           c.DataFechamento.Value.Date == hoje &&
                           c.Status == "Resolvido")
                .CountAsync();

            ViewBag.ChamadosSemana = await _context.Chamados
                .Where(c => c.DataAbertura >= inicioSemana)
                .CountAsync();

            ViewBag.ResolvidosSemana = await _context.Chamados
                .Where(c => c.DataFechamento.HasValue &&
                           c.DataFechamento.Value >= inicioSemana &&
                           c.Status == "Resolvido")
                .CountAsync();

            ViewBag.ChamadosMes = await _context.Chamados
                .Where(c => c.DataAbertura >= inicioMes)
                .CountAsync();

            ViewBag.ResolvidosMes = await _context.Chamados
                .Where(c => c.DataFechamento.HasValue &&
                           c.DataFechamento.Value >= inicioMes &&
                           c.Status == "Resolvido")
                .CountAsync();

            // VIEWBAGS EXISTENTES
            ViewBag.ChamadosUrgentes = chamadosUrgentes;
            ViewBag.TotalChamados = todosChamados.Count;
            ViewBag.ChamadosAbertos = todosChamados.Count(c => c.Status == "Aberto");
            ViewBag.ChamadosEmAndamento = todosChamados.Count(c => c.Status == "Em Andamento");
            ViewBag.ChamadosResolvidos = todosChamados.Count(c => c.Status == "Resolvido");

            return View(todosChamados);
        }

        // ... (restante do seu controller permanece igual)
        // GET: Chamados/Create
        public IActionResult Create()
        {
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Chamado chamado)
        {
            // REMOVER VALIDAÇÃO DO STATUS
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                // DEFINIR STATUS AUTOMATICAMENTE E SALVAR NO BANCO
                chamado.Status = "Aberto";
                chamado.DataAbertura = DateTime.Now;

                _context.Chamados.Add(chamado);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Chamado criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View(chamado);
        }

        // GET: Chamados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // BUSCAR DO BANCO
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null) return NotFound();

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View(chamado);
        }

        // POST: Chamados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Chamado chamado)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id != chamado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // BUSCAR E ATUALIZAR NO BANCO
                    var chamadoExistente = await _context.Chamados.FindAsync(id);
                    if (chamadoExistente == null)
                    {
                        return NotFound();
                    }

                    chamadoExistente.Titulo = chamado.Titulo;
                    chamadoExistente.Descricao = chamado.Descricao;
                    chamadoExistente.Status = chamado.Status;
                    chamadoExistente.Prioridade = chamado.Prioridade;
                    chamadoExistente.Categoria = chamado.Categoria;
                    chamadoExistente.Responsavel = chamado.Responsavel;

                    // DATA DE FECHAMENTO AUTOMÁTICA
                    if (chamado.Status == "Resolvido" && chamadoExistente.DataFechamento == null)
                    {
                        chamadoExistente.DataFechamento = DateTime.Now;
                    }
                    else if (chamado.Status != "Resolvido")
                    {
                        chamadoExistente.DataFechamento = null;
                    }

                    _context.Update(chamadoExistente);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Chamado atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChamadoExists(chamado.Id))
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

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View(chamado);
        }

        // GET: Chamados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // BUSCAR DO BANCO
            var chamado = await _context.Chamados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chamado == null)
            {
                return NotFound();
            }

            return View(chamado);
        }

        // POST: Chamados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // BUSCAR E EXCLUIR DO BANCO
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado != null)
            {
                _context.Chamados.Remove(chamado);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Chamado excluído com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ChamadoExists(int id)
        {
            return _context.Chamados.Any(e => e.Id == id);
        }
    }
}
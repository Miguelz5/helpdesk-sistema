using HelpDesk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Controllers
{
    public class ChamadosController : Controller
    {
        private static List<Chamado> chamados = new List<Chamado>();
        private static int nextId = 1;

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
        public IActionResult Index()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            // CHAMADOS URGENTES: Apenas os NÃO RESOLVIDOS com prioridade Urgente
            var chamadosUrgentes = chamados
                .Where(c => c.Prioridade == "Urgente" && c.Status != "Resolvido")
                .ToList();

            ViewBag.ChamadosUrgentes = chamadosUrgentes;
            ViewBag.TotalChamados = chamados.Count;
            ViewBag.ChamadosAbertos = chamados.Count(c => c.Status == "Aberto");
            ViewBag.ChamadosEmAndamento = chamados.Count(c => c.Status == "Em Andamento");
            ViewBag.ChamadosResolvidos = chamados.Count(c => c.Status == "Resolvido");

            return View(chamados);
        }

        // GET: Chamados/Create
        public IActionResult Create()
        {
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Chamado chamado)
        {
            // REMOVER VALIDAÇÃO DO STATUS - será definido automaticamente
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                // DEFINIR STATUS AUTOMATICAMENTE COMO "Aberto"
                chamado.Id = nextId++;
                chamado.Status = "Aberto";
                chamado.DataAbertura = DateTime.Now;

                chamados.Add(chamado);

                TempData["MensagemSucesso"] = "Chamado criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View(chamado);
        }

        // GET: Chamados/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var chamado = chamados.FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View(chamado);
        }

        // POST: Chamados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Chamado chamado)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id != chamado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Chamado existing = chamados.FirstOrDefault(c => c.Id == chamado.Id);
                if (existing != null)
                {
                    existing.Titulo = chamado.Titulo;
                    existing.Descricao = chamado.Descricao;
                    existing.Status = chamado.Status;
                    existing.Prioridade = chamado.Prioridade;
                    existing.Categoria = chamado.Categoria; // NOVO CAMPO
                    existing.Responsavel = chamado.Responsavel;

                    if (chamado.Status == "Resolvido" && existing.DataFechamento == null)
                    {
                        existing.DataFechamento = DateTime.Now;
                    }
                    else if (chamado.Status != "Resolvido")
                    {
                        existing.DataFechamento = null;
                    }
                }
                TempData["MensagemSucesso"] = "Chamado atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };

            return View(chamado);
        }

        // GET: Chamados/Delete/5
        public IActionResult Delete(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            Chamado chamado = chamados.FirstOrDefault(c => c.Id == id);
            if (chamado == null)
            {
                return NotFound();
            }
            return View(chamado);
        }

        // POST: Chamados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            Chamado chamado = chamados.FirstOrDefault(c => c.Id == id);
            if (chamado != null)
            {
                chamados.Remove(chamado);
                TempData["MensagemSucesso"] = "Chamado excluído com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
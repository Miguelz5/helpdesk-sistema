using HelpDesk.Models;
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

        // GET: Chamados
        public IActionResult Index()
        {
            var chamadosUrgentes = chamados.Where(c => c.Prioridade == "Urgente").ToList();
            ViewBag.ChamadosUrgentes = chamadosUrgentes;
            ViewBag.TotalChamados = chamados.Count;
            ViewBag.ChamadosAbertos = chamados.Count(c => c.Status == "Aberto");
            ViewBag.ChamadosResolvidos = chamados.Count(c => c.Status == "Resolvido");

            return View(chamados);
        }

        // GET: Chamados/Create
        public IActionResult Create()
        {
            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };

            return View();
        }

        // POST: Chamados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Chamado chamado)
        {
            if (ModelState.IsValid)
            {
                chamado.Id = nextId++;
                chamado.DataAbertura = DateTime.Now;
                chamados.Add(chamado);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };

            return View(chamado);
        }

        // GET: Chamados/Edit/5
        public IActionResult Edit(int id)
        {
            Chamado chamado = chamados.FirstOrDefault(c => c.Id == id);
            if (chamado == null)
            {
                return NotFound();
            }

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };

            return View(chamado);
        }

        // POST: Chamados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Chamado chamado)
        {
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
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };

            return View(chamado);
        }

        // GET: Chamados/Delete/5
        public IActionResult Delete(int id)
        {
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
            Chamado chamado = chamados.FirstOrDefault(c => c.Id == id);
            if (chamado != null)
            {
                chamados.Remove(chamado);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
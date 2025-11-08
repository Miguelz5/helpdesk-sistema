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
        private readonly IWebHostEnvironment _environment;

        public ChamadosController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            try
            {
                // DATAS PARA OS PERÍODOS
                var hoje = DateTime.Today;
                var inicioSemana = hoje.AddDays(-6);
                var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

               
                var todosChamados = await _context.Chamados
                    .Where(c => c.NumeroChamado != null) 
                    .ToListAsync();

                var chamadosUrgentes = await _context.Chamados
                    .Where(c => c.Prioridade == "Urgente" && c.Status != "Resolvido" && c.NumeroChamado != null)
                    .ToListAsync();

                // BUSCAR DO BANCO - ESTATÍSTICAS POR PERÍODO
                ViewBag.ChamadosHoje = await _context.Chamados
                    .Where(c => c.DataAbertura.Date == hoje && c.NumeroChamado != null)
                    .CountAsync();

                ViewBag.ResolvidosHoje = await _context.Chamados
                    .Where(c => c.DataFechamento.HasValue &&
                               c.DataFechamento.Value.Date == hoje &&
                               c.Status == "Resolvido" &&
                               c.NumeroChamado != null)
                    .CountAsync();

                ViewBag.ChamadosSemana = await _context.Chamados
                    .Where(c => c.DataAbertura >= inicioSemana && c.NumeroChamado != null)
                    .CountAsync();

                ViewBag.ResolvidosSemana = await _context.Chamados
                    .Where(c => c.DataFechamento.HasValue &&
                               c.DataFechamento.Value >= inicioSemana &&
                               c.Status == "Resolvido" &&
                               c.NumeroChamado != null)
                    .CountAsync();

                ViewBag.ChamadosMes = await _context.Chamados
                    .Where(c => c.DataAbertura >= inicioMes && c.NumeroChamado != null)
                    .CountAsync();

                ViewBag.ResolvidosMes = await _context.Chamados
                    .Where(c => c.DataFechamento.HasValue &&
                               c.DataFechamento.Value >= inicioMes &&
                               c.Status == "Resolvido" &&
                               c.NumeroChamado != null)
                    .CountAsync();

                // VIEWBAGS EXISTENTES
                ViewBag.ChamadosUrgentes = chamadosUrgentes;
                ViewBag.TotalChamados = todosChamados.Count;
                ViewBag.ChamadosAbertos = todosChamados.Count(c => c.Status == "Aberto");
                ViewBag.ChamadosEmAndamento = todosChamados.Count(c => c.Status == "Em Andamento");
                ViewBag.ChamadosResolvidos = todosChamados.Count(c => c.Status == "Resolvido");

                return View(todosChamados);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"ERRO NO INDEX: {ex.Message}");

                // Buscar apenas os chamados que funcionam
                var chamadosSeguros = await _context.Chamados
                    .Where(c => c.NumeroChamado != null && c.Titulo != null && c.Descricao != null)
                    .ToListAsync();

                TempData["MensagemErro"] = "Alguns chamados com dados inválidos foram ignorados.";
                return View(chamadosSeguros);
            }
        }

        // GET: Chamados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id == null)
            {
                TempData["MensagemErro"] = "ID do chamado não informado!";
                return RedirectToAction(nameof(Index));
            }

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                TempData["MensagemErro"] = "Chamado não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            // Preparar as listas para o dropdown
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

           
            ModelState.Remove("NumeroChamado");
            ModelState.Remove("DataAbertura");
            ModelState.Remove("DataFechamento");
            ModelState.Remove("Comentarios");
            ModelState.Remove("Responsavel"); 

            if (!ModelState.IsValid)
            {
                ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
                ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
                ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };
                return View(chamado);
            }

            try
            {
                // BUSCAR O CHAMADO EXISTENTE
                var chamadoExistente = await _context.Chamados.FindAsync(id);
                if (chamadoExistente == null)
                {
                    return NotFound();
                }

                
                var novoResponsavel = !string.IsNullOrWhiteSpace(chamado.Responsavel)
                    ? chamado.Responsavel
                    : chamadoExistente.Responsavel;

               
                chamadoExistente.Titulo = chamado.Titulo;
                chamadoExistente.Descricao = chamado.Descricao;
                chamadoExistente.Status = chamado.Status;
                chamadoExistente.Prioridade = chamado.Prioridade;
                chamadoExistente.Categoria = chamado.Categoria;
                chamadoExistente.Responsavel = novoResponsavel ?? "Não atribuído"; 

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
                return RedirectToAction(nameof(Index));
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
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao salvar: {ex.Message}";
                ViewBag.StatusList = new List<string> { "Aberto", "Em Andamento", "Resolvido" };
                ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
                ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };
                return View(chamado);
            }
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
        public async Task<IActionResult> Create(Chamado chamado, List<IFormFile> anexos)
        {
            ModelState.Remove("Status");
            ModelState.Remove("NumeroChamado");
            ModelState.Remove("DataAbertura");
            ModelState.Remove("DataFechamento");
            ModelState.Remove("Responsavel");
            ModelState.Remove("Comentarios");
            ModelState.Remove("Anexos");

            if (ModelState.IsValid)
            {
                try
                {
                    chamado.Prioridade = ObterPrioridadePorCategoria(chamado.Categoria);
                    chamado.NumeroChamado = GerarNumeroChamado();
                    chamado.Status = "Aberto";
                    chamado.DataAbertura = DateTime.Now;
                    chamado.Responsavel = "";

                    _context.Chamados.Add(chamado);
                    await _context.SaveChangesAsync(); 

                    if (anexos != null && anexos.Count > 0)
                    {
                        await ProcessarAnexos(chamado.Id, anexos);
                    }

                    TempData["MensagemSucesso"] = $"Chamado #{chamado.NumeroChamado} criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = $"Erro ao criar chamado: {ex.Message}";
                }
            }

            ViewBag.PrioridadeList = new List<string> { "Baixa", "Média", "Alta", "Urgente" };
            ViewBag.CategoriaList = new List<string> { "Hardware", "Software", "Rede", "Acesso", "Outros" };
            return View(chamado);
        }

        private async Task ProcessarAnexos(int chamadoId, List<IFormFile> anexos)
        {
            foreach (var anexo in anexos)
            {
                if (anexo.Length > 0 && anexo.Length < 10 * 1024 * 1024) 
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await anexo.CopyToAsync(memoryStream);

                        var fileName = Path.GetFileName(anexo.FileName);
                        var fileExtension = Path.GetExtension(fileName).ToLower();

                        var novoAnexo = new Anexo
                        {
                            ChamadoId = chamadoId,
                            NomeArquivo = fileName,
                            TipoArquivo = anexo.ContentType, 
                            TamanhoArquivo = anexo.Length,
                            DadosArquivo = memoryStream.ToArray(),
                            DataUpload = DateTime.Now,
                            UploadPor = ObterNomeUsuarioLogado()
                        };

                        if (string.IsNullOrEmpty(novoAnexo.TipoArquivo))
                        {
                            novoAnexo.TipoArquivo = fileExtension;
                        }

                        _context.Anexos.Add(novoAnexo);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
        private string ObterPrioridadePorCategoria(string categoria)
        {
            return categoria?.ToLower() switch
            {
                "hardware" => "Alta",
                "software" => "Média",
                "rede" => "Urgente",
                "acesso" => "Urgente",
                "outros" => "Baixa",
                _ => "Média"
            };
        }

        public async Task<IActionResult> DownloadAnexo(int id)
        {
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo == null)
            {
                return NotFound();
            }

            var contentType = ObterContentType(anexo.TipoArquivo);
            return File(anexo.DadosArquivo, contentType, anexo.NomeArquivo);
        }

        public async Task<IActionResult> VisualizarAnexo(int id)
        {
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo == null)
            {
                return NotFound();
            }

            var contentType = ObterContentType(anexo.TipoArquivo);

            if (contentType.StartsWith("image/") || contentType == "application/pdf")
            {
                return File(anexo.DadosArquivo, contentType);
            }
            else
            {
                return File(anexo.DadosArquivo, contentType, anexo.NomeArquivo);
            }
        }

        private string ObterContentType(string tipoArquivo)
        {
            if (string.IsNullOrEmpty(tipoArquivo))
                return "application/octet-stream";

            // Se já é um content type válido, retorna como está
            if (tipoArquivo.Contains("/"))
                return tipoArquivo;

            // Mapear extensões para content types
            return tipoArquivo.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirAnexo(int id)
        {
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo == null)
            {
                TempData["MensagemErro"] = "Anexo não encontrado!";
                return RedirectToAction("Details", new { id = anexo?.ChamadoId });
            }

            var chamadoId = anexo.ChamadoId;

            _context.Anexos.Remove(anexo);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Anexo excluído com sucesso!";
            return RedirectToAction("Details", new { id = chamadoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarAnexos(int id, List<IFormFile> anexos)
        {
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                TempData["MensagemErro"] = "Chamado não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            if (anexos != null && anexos.Any())
            {
                await ProcessarAnexos(id, anexos);
                TempData["MensagemSucesso"] = "Anexos adicionados com sucesso!";
            }
            else
            {
                TempData["MensagemErro"] = "Nenhum arquivo selecionado!";
            }

            return RedirectToAction("Details", new { id });
        }


        private string GerarNumeroChamado()
        {
            var ano = DateTime.Now.Year;

            // Buscar o MAIOR número sequencial do ano atual
            var ultimoNumero = _context.Chamados
                .Where(c => c.DataAbertura.Year == ano && c.NumeroChamado != null)
                .Select(c => c.NumeroChamado)
                .ToList()
                .Select(n =>
                {
                    if (n != null && n.Contains('-'))
                    {
                        var partes = n.Split('-');
                        if (partes.Length == 2 && int.TryParse(partes[1], out int sequencia))
                            return sequencia;
                    }
                    return 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var novaSequencia = ultimoNumero + 1;
            return $"{ano}-{novaSequencia.ToString("D4")}";
        }

        
        public async Task<IActionResult> CorrigirNumerosChamados()
        {
            var chamadosSemNumero = await _context.Chamados
                .Where(c => c.NumeroChamado == null)
                .ToListAsync();

            foreach (var chamado in chamadosSemNumero)
            {
                var ano = chamado.DataAbertura.Year;
                var sequencia = _context.Chamados
                    .Count(c => c.DataAbertura.Year == ano && c.NumeroChamado != null) + 1;
                chamado.NumeroChamado = $"{ano}-{sequencia.ToString("D4")}";
            }

            await _context.SaveChangesAsync();
            TempData["MensagemSucesso"] = $"{chamadosSemNumero.Count} chamados corrigidos!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Chamados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            if (id == null)
            {
                TempData["MensagemErro"] = "ID do chamado não informado!";
                return RedirectToAction(nameof(Index));
            }
            var chamado = await _context.Chamados
    .Include(c => c.Comentarios)
    .Include(c => c.Anexos)
    .FirstOrDefaultAsync(m => m.Id == id);
                
            if (chamado == null)
            {
                TempData["MensagemErro"] = "Chamado não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            // Ordenar comentários
            chamado.Comentarios = chamado.Comentarios?
                .OrderBy(c => c.DataCriacao)
                .ToList();

            ViewBag.NomeUsuarioLogado = ObterNomeUsuarioLogado();
            ViewBag.Admins = await _context.Usuarios.ToListAsync();

            return View(chamado);
        }

        // POST: Chamados/AdicionarComentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarComentario(int chamadoId, string mensagem)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null || string.IsNullOrEmpty(chamado.Responsavel))
            {
                TempData["MensagemErro"] = "Este chamado precisa ser atribuído antes de adicionar comentários!";
                return RedirectToAction("Details", new { id = chamadoId });
            }

            if (chamado.Status == "Resolvido")
            {
                TempData["MensagemErro"] = "Este chamado já foi resolvido e está fechado para novos comentários!";
                return RedirectToAction("Details", new { id = chamadoId });
            }

            if (string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["MensagemErro"] = "A mensagem não pode estar vazia!";
                return RedirectToAction("Details", new { id = chamadoId });
            }

            var comentario = new Comentario
            {
                ChamadoId = chamadoId,
                Mensagem = mensagem.Trim(),
                Autor = ObterNomeUsuarioLogado(),
                EhAdministrador = true,
                DataCriacao = DateTime.Now
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Comentário adicionado com sucesso!";
            return RedirectToAction("Details", new { id = chamadoId });
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

        // POST: Chamados/AtribuirParaMim/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtribuirParaMim(int id)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                TempData["MensagemErro"] = "Chamado não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            var nomeUsuarioLogado = ObterNomeUsuarioLogado();

            chamado.Responsavel = nomeUsuarioLogado;
            chamado.Status = "Em Andamento";

            _context.Update(chamado);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = $"Chamado #{chamado.NumeroChamado} atribuído para você!";
            return RedirectToAction("Details", new { id = chamado.Id });
        }

        private string ObterNomeUsuarioLogado()
        {
            var email = User.Identity.Name; 

            if (!string.IsNullOrEmpty(email))
            {
                var nome = email.Split('@')[0];
                return char.ToUpper(nome[0]) + nome.Substring(1);
            }

            return "Administrador";
        }

        // POST: Chamados/AtribuirParaAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtribuirParaAdmin(int id, string adminId)
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null) return loginCheck;

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                TempData["MensagemErro"] = "Chamado não encontrado!";
                return RedirectToAction(nameof(Index));
            }

            // Buscar o admin selecionado
            var admin = await _context.Usuarios.FindAsync(int.Parse(adminId));
            if (admin == null)
            {
                TempData["MensagemErro"] = "Administrador não encontrado!";
                return RedirectToAction("Details", new { id = chamado.Id });
            }

            chamado.Responsavel = admin.Nome; 
            chamado.Status = "Em Andamento";

            _context.Update(chamado);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = $"Chamado #{chamado.NumeroChamado} atribuído para {admin.Nome}!";
            return RedirectToAction("Details", new { id = chamado.Id });
        }

        // POST: Chamados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var chamado = await _context.Chamados
                    .Include(c => c.Comentarios)
                    .Include(c => c.Anexos)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (chamado != null)
                {

                    if (chamado.Comentarios != null && chamado.Comentarios.Any())
                    {
                        _context.Comentarios.RemoveRange(chamado.Comentarios);
                    }

                    if (chamado.Anexos != null && chamado.Anexos.Any())
                    {
                        _context.Anexos.RemoveRange(chamado.Anexos);
                    }

                    _context.Chamados.Remove(chamado);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Chamado excluído com sucesso!";
                }
                else
                {
                    TempData["MensagemErro"] = "Chamado não encontrado!";
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao excluir chamado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ChamadoExists(int id)
        {
            return _context.Chamados.Any(e => e.Id == id);
        }
    }
}
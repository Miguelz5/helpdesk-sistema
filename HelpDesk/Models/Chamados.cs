using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Chamado
    {
        public int Id { get; set; }

        public string NumeroChamado { get; set; } = string.Empty;

        [Required(ErrorMessage = "O título é obrigatório")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; } = string.Empty;

        public DateTime DataAbertura { get; set; } 
        public DateTime? DataFechamento { get; set; } 

        public string Status { get; set; } 

        [Required(ErrorMessage = "A prioridade é obrigatória")]
        public string Prioridade { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; }

        public string Responsavel { get; set; }

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Chamado
    {
        public int Id { get; set; }

        public string NumeroChamado { get; set; } // SEM [Required]

        [Required(ErrorMessage = "O título é obrigatório")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; }

        public DateTime DataAbertura { get; set; } // SEM [Required]
        public DateTime? DataFechamento { get; set; }

        public string Status { get; set; } // SEM [Required]

        [Required(ErrorMessage = "A prioridade é obrigatória")]
        public string Prioridade { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; }

        public string Responsavel { get; set; } // SEM [Required]

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
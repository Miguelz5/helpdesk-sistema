using System;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Chamado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; }

        public DateTime DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }

        // REMOVA O [Required] - status será definido automaticamente
        public string Status { get; set; }

        [Required(ErrorMessage = "A prioridade é obrigatória")]
        public string Prioridade { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; }

        public string Responsavel { get; set; }
    }
}
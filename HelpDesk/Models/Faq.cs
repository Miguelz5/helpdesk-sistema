using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Faq
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A pergunta é obrigatória")]
        public string Pergunta { get; set; }

        [Required(ErrorMessage = "A resposta é obrigatória")]
        public string Resposta { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; }

        public int Ordem { get; set; } = 0;
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}